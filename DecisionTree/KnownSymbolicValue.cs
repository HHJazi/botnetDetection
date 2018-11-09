using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// This class implements the value of a symbolic attribute.
    /// </summary>
    public sealed class KnownSymbolicValue : SymbolicValue
    {
        /// <summary>
        /// Represents as an integer.
        /// </summary>
        public int IntValue { get; set; }

        public KnownSymbolicValue()
        { 
        }

        public KnownSymbolicValue(int val)
            : base()
        {
            if (val < 0)
                throw new ArgumentException("Value must be positive number");

            this.IntValue = val;
        }

        public override bool IsUnknown()
        {
            return false;
        }

        public override int GetHashCode()
        {
            return IntValue;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            
            if(GetType() != obj.GetType())
                return false;

            return this.IntValue == ((KnownSymbolicValue)obj).IntValue;
        }

        public override string ToString()
        {
            return ""  + IntValue;
        }
    }
}
