using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// A test on a single numerical attribute.
    /// The test checks if an attribute's value is smaller than a fixed threshold.
    /// </summary>
    public class NumericalTest : Test
    {
        private double _threshold;
        public double Threshold
        {
            get { return _threshold; }
            set { _threshold = value; }
        }

        /// <summary>
        /// The number of issues for Numberical Test. This value is always 2.
        /// </summary>
        /// <returns></returns>
        public override int NumOfIssues
        {
            get { return 2; }
        }

        public NumericalTest()
            : base()
        { 
        
        }

        public NumericalTest(NumericalAttribute attr, double thresh)
            : base(attr)
        {
            if (attr == null)
                throw new ArgumentNullException();

            this._threshold = thresh;
        }

        /// <summary>
        /// Applies the test. The test checks if the tested value is smaller than the threshold.
        /// </summary>
        /// <param name="val"></param>
        /// <returns>1 - tested value is smaller than the threshold; 0 - otherwise.</returns>
        public override int Perform(AttributeValue val)
        {
            if (!(val is KnownNumericalValue))
                throw new ArgumentException("Wrong value type");

            return Perform((KnownNumericalValue)val);
        }

        /// <summary>
        /// Applies the test. The test checks if the tested value is smaller than the threshold.
        /// </summary>
        /// <param name="val"></param>
        /// <returns>1 - tested value is smaller than the threshold; 0 - otherwise.</returns>
        public int Perform(KnownNumericalValue val)
        {
            return (val.Value < _threshold) ? 1 : 0;
        }

        public override string ToString()
        {
            return Attribute.ToString() + "<" + _threshold;
        }

        public override string IssueToString(int num)
        {
            switch (num) 
            {
                case 0:
                    return "No";
                case 1:
                    return "Yes";
                default:
                    throw new ArgumentException("Invalid issue number.");
            }
        }

    }
}
