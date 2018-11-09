using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// This class implements the value of a symbolic attribute.
    /// </summary>
    public abstract class SymbolicValue : AttributeValue
    {
        public abstract override bool IsUnknown();
    }
}
