//Nick Sells, 2022

using System;

namespace SupermarketSim {
	public class Util {

		/// <summary>
		/// The random number generator used across the entire application
		/// </summary>
		public static Random Rng { get; }

		/// <summary>
		/// static constructor to initialize the Rng
		/// </summary>
		static Util() {
			Rng = new Random();
		}

		public static void DrawSpanningBar(char ch) {

			Console.WriteLine(new String(ch, Console.BufferWidth));
		}

		/// <summary>
		/// gets a uniformly-distrubuted value in the range 0 (inclusive) to 1
		/// (exclusive)
		/// </summary>
		/// <returns></returns>
		public static double GetRandomUniform() {
			return Rng.NextDouble();
		}

		/// <summary>
		/// generates a random, negative-exponentially distributed value in the
		/// range 0 (inclusive) to 1 (exclusive)
		/// </summary>
		/// <param name="lambda">the rate of the distribution, or rather the value
		/// towards which all results are biased</param>
		/// <returns></returns>
		public static double GetRandomNegExp(double lambda) {

			//NOTE: bad code? generates incorrect values
			double uniform = Rng.NextDouble();
			//double negexp = -Math.Log(1 - uniform * (1 - Math.Exp(-lambda))) / lambda;
			//return negexp;

			//\lambda\cdot\left(1 - x\cdot\left(1 - e ^{ -\lambda}\right)\right)

			double negexp = lambda * (1 - uniform * (1 - Math.Exp(-lambda)));
			return negexp;
		}

		/// <summary>
		/// gets a random poisson-distributed number
		/// https://www.johndcook.com/blog/csharp_poisson/
		/// </summary>
		/// <param name="lambda"></param>
		/// <returns></returns>
		public static int GetRandomPoisson(double lambda) {
			return (lambda < 30.0) ? PoissonSmall(lambda) : PoissonLarge(lambda);
		}

		#region PoissonHelperMethods

		/// <summary>
		/// helper method for GetRandomPoisson(double). this is used for values of
		/// lambda less or equal to 30
		/// </summary>
		/// <param name="lambda"></param>
		/// <returns></returns>
		private static int PoissonSmall(double lambda) {
			// Algorithm due to Donald Knuth, 1969.
			double p = 1.0, L = Math.Exp(-lambda);
			int k = 0;
			do {
				k++;
				p *= Rng.NextDouble();
			} while (p > L);
			return k - 1;
		}

		/// <summary>
		/// helper method for GetRandomPoisson(double). this is used for values of lambda greater than 30
		/// https://www.johndcook.com/blog/csharp_poisson/
		/// </summary>
		/// <param name="lambda"></param>
		/// <returns></returns>
		private static int PoissonLarge(double lambda) {

			double c = 0.767 - 3.36 / lambda;
			double beta = Math.PI / Math.Sqrt(3.0 * lambda);
			double alpha = beta * lambda;
			double k = Math.Log(c) - lambda - Math.Log(beta);
			while (true) {
				double u = Rng.NextDouble();
				double x = (alpha - Math.Log((1.0 - u) / u)) / beta;
				int n = (int)Math.Floor(x + 0.5);
				if (n < 0)
					continue;
				double v = Rng.NextDouble();
				double y = alpha - beta * x;
				double temp = 1.0 + Math.Exp(y);
				double lhs = y + Math.Log(v / (temp * temp));
				//ln(x!) = ln(Γ(x+1)), so by adding one to the parameter, we achieve the log
				//of the factorial
				double rhs = k + n * Math.Log(lambda) - LogGamma(n + 1);
				if (lhs <= rhs)
					return n;
			}
		}

		/// <summary>
		/// approximation of the Euler-Mascheroni constant
		/// https://wikipedia.org/wiki/Euler%E2%80%93Mascheroni_constant
		/// </summary>
		private const double EULER_MASCHERONI = 0.5772156649;

		/// <summary>
		/// the number to actually stop at when performing unbounded summation
		/// </summary>
		private const int EFFECTIVE_INFINITY = 100000;

		/// <summary>
		/// Approximates the result of the natural logarithm of the gamma function,
		/// using Boros and Moll's method
		/// https://mathworld.wolfram.com/LogGammaFunction.html
		/// </summary>
		/// <param name="z">argument to the log gamma function</param>
		/// <returns></returns>
		private static double LogGamma(double z) {

			double sum = -EULER_MASCHERONI * z - Math.Log(z);
			for (int k = 1; k < EFFECTIVE_INFINITY; k++) {

				double zDivK = z / k;
				sum += (zDivK - Math.Log(1 + zDivK));
			}
			return sum;
		}

		#endregion
	}
}
