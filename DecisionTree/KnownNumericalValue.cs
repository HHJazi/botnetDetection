using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// This class implements the value of a known numerical attribute.
    /// </summary>
    public class KnownNumericalValue : NumericalValue
    {
        /// <summary>
        /// This attribute value represented as an Double number.
        /// </summary>
        public double Value { get; set; }

        public KnownNumericalValue()
        { }

        public KnownNumericalValue(double val) :
            base()
        {
            Value = val;
        }

        public override bool IsUnknown()
        {
            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            
            if (GetType() != obj.GetType())
                return false;
            
            return ((KnownNumericalValue)obj).Value == this.Value;
        }

        public override int GetHashCode()
        {
            return (int)Value;
        }

        public override string ToString()
        {
            return "" + Value;
        }
    }
}
