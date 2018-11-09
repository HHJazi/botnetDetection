using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// This class implements a numerical attribute. The values of such an attribute represented by a 
    /// double number.
    /// </summary>
    public class NumericalAttribute :Attribute
    {

        public NumericalAttribute()
            : base()
        { 
        
        }

        public NumericalAttribute(string name)
            : base(name)
        { 
        
        }

        public Attribute Copy(string name)
        {
            return new NumericalAttribute(name);
        }

        public override string ToString()
        {
            return base.Name + " : [numerical]";
        }
    }
}
