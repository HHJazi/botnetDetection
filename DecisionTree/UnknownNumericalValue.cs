using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Signature.DT
{
    public class UnknownNumericalValue : NumericalValue
    {
        public override bool IsUnknown()
        {
            return true;
        }

        public override string ToString()
        {
            return "?";
        }
    }
}
