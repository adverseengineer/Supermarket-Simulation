//Nick Sells, 2022

using System;

namespace SupermarketSim {
	public class Program {

		/// <summary>
		/// used to indicate the outcome of a user command
		/// </summary>
		public enum MenuCommandResult { SUCCESS, FAILURE }

		/// <summary>
		/// a delegate type we use to implement a function lookup table
		/// </summary>
		/// <returns></returns>
		private delegate MenuCommandResult MenuAction();

		/// <summary>
		/// a function lookup table we use to greatly simplify the logic to implement a CLI interface
		/// </summary>
		private static List<(string choice, MenuAction action)> menuChoices = new List<(string choice, MenuAction action)> {
			("Run the simulation", RunSimulation),
			("Set the store opening time", SetOpeningTime),
			("Set the store closing time", SetClosingTime),
			("Set the expected checkout length", SetExpectedCheckoutTime),
			("Set the expected number of customers", SetExpectedNumCustomers),
			("Set the number of checkout lanes", SetNumCheckoutLanes),
			("Set the number of milliseconds per simulation step", SetRefreshDelay),
			("Exit", Exit)
		};

		/// <summary>
		/// the supermarket instance we are simulating
		/// </summary>
		private static Supermarket supermarket = new Supermarket(
			new TimeOnly(8, 00), //8:00 AM
			new TimeOnly(0, 00), //12:00 AM
			new TimeSpan(0, 0, 6, 15), //6 minutes, 15 seconds
			600,
			4,
			30
		);

		private static MenuCommandResult? lastCommandResult = null;
		private static bool closeRequested = false;

		public static void Main() {

			int choice = -1;

			while (!closeRequested) {

				DrawMenu();
				string input = Console.ReadLine()!.Trim();

				if (Int32.TryParse(input, out choice))
					lastCommandResult = menuChoices[choice - 1].action();
				else
					Console.WriteLine("Invalid choice entered, please try again");
			}
		}

		/// <summary>
		/// renders the main menu of the application
		/// </summary>
		private static void DrawMenu() {

			Console.Clear();
			Console.WriteLine("Nicholas Sells");
			Console.WriteLine("CSCI 2210");
			Console.WriteLine("April 14th, 2022");
			Console.WriteLine("Supermarket Simulation");
			Util.DrawSpanningBar('-');
			Console.WriteLine();

			Console.WriteLine(supermarket.ToString());
			Util.DrawSpanningBar('-');
			Console.WriteLine();

			//don't print this line unless there is a value for the last command
			if (lastCommandResult != null) {
				Console.WriteLine($"result of last command: {lastCommandResult}");
				Util.DrawSpanningBar('-');
				Console.WriteLine();
			}

			for (int i = 0; i < menuChoices.Count; i++)
				Console.WriteLine($"{i + 1}. {menuChoices[i].choice}");

			Console.WriteLine();
			Console.Write("Type the number of your choice: ");
		}

		/// <summary>
		/// runs the simulation
		/// </summary>
		/// <returns></returns>
		private static MenuCommandResult RunSimulation() {

			Console.Clear();
			supermarket.DoSimulation();

			Console.WriteLine("Simulation Complete! Press enter to return to main menu");
			Console.ReadLine();
			return MenuCommandResult.SUCCESS;
		}

		/// <summary>
		/// runs a dialogue with the user and sets the opening time
		/// </summary>
		/// <returns></returns>
		private static MenuCommandResult SetOpeningTime() {

			Console.Write("Type the new opening time (HH:MM:SS AM/PM): ");
			string input = Console.ReadLine()!.Trim();
			TimeOnly time;
			if (TimeOnly.TryParse(input, out time)) {
				supermarket.OpeningTime = time;
				return MenuCommandResult.SUCCESS;
			}
			else {
				Console.WriteLine("Failed to parse a time from the input. Please try again");
				return MenuCommandResult.FAILURE;
			}
		}

		/// <summary>
		/// runs a dialogue with the user and sets the closing time
		/// </summary>
		/// <returns></returns>
		private static MenuCommandResult SetClosingTime() {
			
			Console.Write("Type the new closing time (HH:MM:SS AM/PM): ");
			string input = Console.ReadLine()!.Trim();
			TimeOnly time;
			if (TimeOnly.TryParse(input, out time)) {
				supermarket.ClosingTime = time;
				return MenuCommandResult.SUCCESS;
			}
			else {
				Console.WriteLine("Failed to parse a time from the input. Please try again");
				return MenuCommandResult.FAILURE;
			}
		}

		/// <summary>
		/// runs a dialogue with the user and sets the expected time to checkout
		/// </summary>
		/// <returns></returns>
		private static MenuCommandResult SetExpectedCheckoutTime() {

			Console.Write("Type the new expected checkout length (HH:MM:SS): ");
			string input = Console.ReadLine()!.Trim();
			TimeSpan time;
			if (TimeSpan.TryParse(input, out time)) {
				supermarket.ExpectedCheckoutTime = time;
				return MenuCommandResult.SUCCESS;
			}
			else {
				Console.WriteLine("Failed to parse a time from the input. Please try again");
				return MenuCommandResult.FAILURE;
			}
		}

		/// <summary>
		/// runs a dialogue with the user and sets the expected number of customers
		/// </summary>
		/// <returns></returns>
		private static MenuCommandResult SetExpectedNumCustomers() {

			Console.Write("Type the expected number of customers: ");
			string input = Console.ReadLine()!.Trim();
			int num;
			if (Int32.TryParse(input, out num)) {
				supermarket.ExpectedNumCustomers = num;
				return MenuCommandResult.SUCCESS;
			}
			else {
				Console.WriteLine("Failed to parse an integer from input. Please try again");
				return MenuCommandResult.FAILURE;
			}
		}

		/// <summary>
		/// runs a dialogue with the user and sets the number of checkout lanes
		/// </summary>
		/// <returns></returns>
		private static MenuCommandResult SetNumCheckoutLanes() {

			Console.Write("Type the number of checkout lanes: ");
			string input = Console.ReadLine()!.Trim();
			int num;
			if (Int32.TryParse(input, out num)) {
				supermarket.NumCheckoutLanes = num;
				return MenuCommandResult.SUCCESS;
			}
			else {
				Console.WriteLine("Failed to parse an integer from the input. Please try again");
				return MenuCommandResult.FAILURE;
			}
		}

		/// <summary>
		/// runs a dialogue with the user and sets the refresh delay of the simulation
		/// </summary>
		/// <returns></returns>
		private static MenuCommandResult SetRefreshDelay() {

			Console.Write("Type the number milliseconds per simulation step: ");
			string input = Console.ReadLine()!.Trim();
			int num;
			if (Int32.TryParse(input, out num)) {
				supermarket.RefreshDelay = num;
				return MenuCommandResult.SUCCESS;
			}
			else {
				Console.WriteLine("Failed to parse an integer from the input. Please try again");
				return MenuCommandResult.FAILURE;
			}
		}

		/// <summary>
		/// exits the program
		/// </summary>
		/// <returns></returns>
		private static MenuCommandResult Exit() {

			closeRequested = true;
			return MenuCommandResult.SUCCESS;
		}
	}
}
