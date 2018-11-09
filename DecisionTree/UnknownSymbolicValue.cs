using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// This class implements the value of a symbolic attribute.
    /// </summary>
    public sealed class UnknownSymbolicValue : SymbolicValue
    {
        public override bool IsUnknown() { return true;}

        public override string ToString()
        {
            return "?";
        }
    }
}
