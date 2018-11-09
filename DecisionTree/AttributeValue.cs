using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class AttributeValue
    {

        /// <summary>
        /// Check if the attribute value is unknown.
        /// </summary>
        /// <returns></returns>
        public abstract bool IsUnknown();
    }
}
