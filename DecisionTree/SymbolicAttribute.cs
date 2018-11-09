using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// Symbolic attributes have a finite set of possible values represented by a positive integer.
    /// </summary>
    public class SymbolicAttribute : Attribute
    {
        /// <summary>
        /// The number of different values allowd for this attribute.
        /// </summary>
        public int NumOfValues { get; set; }

        public SymbolicAttribute()
            : base()
        {
            
        }

        public SymbolicAttribute(int nbValues)
            : base(string.Empty)
        {
            if (nbValues < 0)
                throw new ArgumentException("The number of allowed attribute values must be positive.");

            NumOfValues = nbValues;
        }

        public SymbolicAttribute(string name, int nbValues)
            : base(name)
        {
            if (nbValues < 0)
                throw new ArgumentException("The number of allowed attribute values must be positive.");

            NumOfValues = nbValues;
        }

        public string ValueToString(SymbolicValue value)
        {
            return value.ToString();
        }
    }
}
