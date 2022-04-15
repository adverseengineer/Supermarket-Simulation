//Nick Sells, 2022

using System;

namespace SupermarketSim {

	public class Customer {

		private static uint _customerCounter = 0;
		public uint CustomerID { get; }
		public TimeOnly TimeEnteredLine { get; set; }
		public TimeSpan TimeTakenToCheckout { get; set; }


		/// <summary>
		/// simple constructor, also does simple instance numbering
		/// </summary>
		public Customer() {
			CustomerID = ++_customerCounter;
		}
	}
}
