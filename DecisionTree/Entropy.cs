using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// A class holding entropy-related functions
    /// </summary>
    public sealed class Entropy
    {

        /// <summary>
        /// Computes the entropy of a random variable.
        /// </summary>
        /// <param name="probabilities">The distribution of a random variable,
        /// given as a set of probabilities. This set do not need to be normalized:
        /// every element is divided by the sum of all the elements of the array. 
        /// All the elements must be positive.</param>
        /// <returns>the entropy of the variable. </returns>
        public static double CalEntropy(IEnumerable<double> probabilities)
        {
            if (probabilities == null)
                throw new ArgumentNullException();

            double sum = 0.0d;
            double result = 0.0d;

            foreach (double p in probabilities)
            {
                if (p < 0.0d)
                    throw new ArgumentException("Negative probability!");
                
                if (p > 0.0d)
                {
                    result -= p * Math.Log(p);
                    sum += p;
                }
            }

            if (sum == 0.0)
                return 0.0;

            result += sum * Math.Log(sum);

            return result / (Math.Log(2.0) * sum);
        }

    }
}
