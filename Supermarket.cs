using System;
using System.Diagnostics;
using System.Text;

namespace SupermarketSim {

	public class Supermarket {

		public TimeOnly OpeningTime { get; set; }
		public TimeOnly ClosingTime { get; set; }
		public TimeSpan ExpectedCheckoutTime { get; set; }

		public int ExpectedNumCustomers { get; set; }
		public int NumCheckoutLanes { get; set; }

		public int RefreshDelay { get; set; }

		private PriorityQueue<Event, TimeOnly> eventQueue;
		private List<Queue<Customer>> checkoutLanes;
		
		private int numCustomers;
		private int numArrivalEventsProcessed;
		private int totalNumEvents;
		private int numExitEventsProcessed;
		private int numEventsProcessed;
		private int longestLineLength;

		/// <summary>
		/// member-wise constructor
		/// </summary>
		/// <param name="openingTime"></param>
		/// <param name="closingTime"></param>
		/// <param name="expectedCheckoutTime"></param>
		/// <param name="expectedNumCustomers"></param>
		/// <param name="numCheckoutLanes"></param>
		public Supermarket (TimeOnly openingTime, TimeOnly closingTime, TimeSpan expectedCheckoutTime,
			int expectedNumCustomers, int numCheckoutLanes, int refreshDelay) {

			OpeningTime = openingTime;
			ClosingTime = closingTime;
			ExpectedCheckoutTime = expectedCheckoutTime;
			ExpectedNumCustomers = expectedNumCustomers;
			NumCheckoutLanes = numCheckoutLanes;
			RefreshDelay = refreshDelay;

			eventQueue = new PriorityQueue<Event, TimeOnly>(numCustomers * 2);
			checkoutLanes = new List<Queue<Customer>>(numCheckoutLanes);
			for (int i = 0; i < numCheckoutLanes; i++)
				checkoutLanes.Add(new Queue<Customer>());

			numCustomers = Util.GetRandomPoisson(ExpectedNumCustomers);
			totalNumEvents = numCustomers * 2;
			longestLineLength = Int32.MinValue;
		}

		/// <summary>
		/// predetermines all arrival events
		/// </summary>
		public void GenerateArrivals() {

			var totalTimeOpen = ClosingTime - OpeningTime;

			//generate an arrival time and checkout time for each customer
			for (int i = 0; i < numCustomers; i++) {

				//random uniformly-distributed between opening and closing time
				var arrivalTime = new TimeOnly(OpeningTime.Ticks + Util.Rng.NextInt64(0, totalTimeOpen.Ticks));

				Debug.Assert(arrivalTime.IsBetween(OpeningTime, ClosingTime));
				
				//negative-exponentially distributed with lambda = ExpectedClosingTime
				var timeTakenToCheckout = new TimeSpan((long) Util.GetRandomNegExp(ExpectedCheckoutTime.Ticks));

				//NOTE: this line is where each customer is instantiated
				var customer = new Customer {
					TimeEnteredLine = arrivalTime,
					TimeTakenToCheckout = timeTakenToCheckout
				};

				var evt = new ArrivalEvent(arrivalTime, customer);
				eventQueue.Enqueue(evt, arrivalTime);
			}
		}

		/// <summary>
		/// encapsulates all the logic needed to process an arrival
		/// </summary>
		/// <param name="evt"></param>
		public void ProcessArrival(Event evt) {

			var arrival = (evt as ArrivalEvent)!;
			numArrivalEventsProcessed++;

			int index = GetShortestLine();
			checkoutLanes[index].Enqueue(arrival.Customer);

			//so now that the newly arrived customer is in line
			//we must use their TimeTakenToCheckout to queue an exit event

			var exitTime = evt.TimeStamp.Add(arrival.Customer.TimeTakenToCheckout);

			var newExitEvent = new ExitEvent(exitTime, index);
			eventQueue.Enqueue(newExitEvent, exitTime);
		}

		/// <summary>
		/// encapsualtes all the logic needed to process an exit
		/// </summary>
		/// <param name="evt"></param>
		public void ProcessExit(Event evt) {

			var exit = (evt as ExitEvent)!;

			var checkoutLane = checkoutLanes[exit.CheckoutLaneIndex];

			Debug.Assert(checkoutLane.Count > 0);
			checkoutLane.Dequeue();

			numExitEventsProcessed++;
		}

		public void DoSimulation() {

			GenerateArrivals();

			while (eventQueue.Count > 0) {

				//every time we dequeue from the eventqueue, it represents a customer arriving or leaving
				//these do happen in chronological order, but with a wildly varying timestep

				//because arrival events are our only measurement of time, we must queue exit events while
				//handling arrival events. 

				var evt = eventQueue.Dequeue();
				numEventsProcessed++;

				//handle an arrival event
				if (evt is ArrivalEvent) {

					ProcessArrival(evt);

					//keep longestLineLength up to date
					foreach (var lane in checkoutLanes)
						longestLineLength = Math.Max(longestLineLength, lane.Count);
				}

				//handle an exit event
				else if (evt is ExitEvent)
					ProcessExit(evt);

				DrawAndWait();
			}

			//sanity checks
			Debug.Assert(numEventsProcessed == numCustomers * 2);
			Debug.Assert(numArrivalEventsProcessed == numExitEventsProcessed);
			Debug.Assert(numEventsProcessed == numArrivalEventsProcessed + numExitEventsProcessed);

			int totalCustomersInLine = 0;
			foreach (var lane in checkoutLanes)
				totalCustomersInLine += lane.Count;

			Debug.Assert(numArrivalEventsProcessed - numExitEventsProcessed == totalCustomersInLine);
		}

		/// <summary>
		/// return the index of the shortest line
		/// </summary>
		public int GetShortestLine() {

			//choose the shortest line and get in it
			int leastCustomers = Int32.MaxValue;
			int shortestLine = 0;
			for (int i = 0; i < checkoutLanes.Count; i++) {
				if (checkoutLanes[i].Count < leastCustomers) {
					leastCustomers = checkoutLanes[i].Count;
					shortestLine = i;
				}
			}

			return shortestLine;
		}

		/// <summary>
		/// writes the current supermarket state to the console and waits
		/// </summary>
		/// <param name="milliseconds"></param>
		public void DrawAndWait() {

			Console.Clear();
			Console.WriteLine("Running Simulation...");
			Util.DrawSpanningBar('-');
			Console.WriteLine();

			for (int i = 0; i < NumCheckoutLanes; i++) {

				Console.Write($"{i + 1}:".PadLeft(3));

				foreach (var customer in checkoutLanes[i])
					Console.Write(customer.CustomerID.ToString().PadLeft(5));

				Console.WriteLine();
			}

			Console.WriteLine($"Number of events processed: {numEventsProcessed}/{totalNumEvents}");
			Console.WriteLine($"Number of arrivals processed: {numArrivalEventsProcessed}");
			Console.WriteLine($"Number of exits processed: {numExitEventsProcessed}");
			Console.WriteLine($"Longest line encountered so far: {longestLineLength}");
			Console.WriteLine();

			Thread.Sleep(RefreshDelay);
		}

		public override string ToString() {

			StringBuilder result = new StringBuilder();

			result.Append($"Opening Time: {OpeningTime}\n");
			result.Append($"Closing Time: {ClosingTime}\n");
			result.Append($"Expected Checkout Time: {ExpectedCheckoutTime}\n");
			result.Append($"Expected Number of Customers: {ExpectedNumCustomers}\n");
			result.Append($"Number of Checkout Lanes: {NumCheckoutLanes}\n");
			result.Append($"Number of Milliseconds per Simulation Step: {RefreshDelay}");

			return result.ToString();
		}
	}
}
