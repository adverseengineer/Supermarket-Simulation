//Nick Sells, 2022

using System;

namespace SupermarketSim {

	public abstract class Event {

		public TimeOnly TimeStamp { get; set; }
	}

	public sealed class ArrivalEvent : Event {

		public Customer Customer { get; set; }

		public ArrivalEvent(TimeOnly timeStamp, Customer customer) {

			TimeStamp = timeStamp;
			Customer = customer;			
		}
	}

	public sealed class ExitEvent : Event {

		public int CheckoutLaneIndex { get; set; }

		public ExitEvent(TimeOnly timeStamp, int checkoutLaneIndex) {

			TimeStamp = timeStamp;
			CheckoutLaneIndex = checkoutLaneIndex;
		}
	}
}
