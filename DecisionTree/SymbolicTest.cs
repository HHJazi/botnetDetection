using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// A test on a single symbolic attribute. Teh test checks if an attribute's value belongs to a 
    /// given set or not.
    /// </summary>
    public class SymbolicTest : Test
    {
        private KnownSymbolicValue[] _values;
        public KnownSymbolicValue[] Values
        {
            get { return _values; }
            set { _values = value; }
        }

        /// <summary>
        /// The number of Issues. For Symbolic Test, it is 2.
        /// </summary>
        [XmlIgnore]
        public override int NumOfIssues
        {
            get { return 2; }
        }

        public SymbolicTest()
            : base()
        { 
        
        }

        public SymbolicTest(SymbolicAttribute attr, IEnumerable<KnownSymbolicValue> values)
            :base(attr)
        {
            if (values == null)
                throw new ArgumentNullException();

            _values = values.ToArray();
        }
        
        #region Public Methods

        /// <summary>
        /// Applies the test. The test checks if an attribute value belongs to a given set of values.
        /// </summary>
        /// <param name="val">The value to test. </param>
        /// <returns>1 - the value belongs to the set of admitted values, 0 - otherwise.</returns>
        public override int Perform(AttributeValue val)
        {
            if (val.IsUnknown())
                throw new ArgumentException("Cannot perform test on an unknow value.s");

            if (!(val is KnownSymbolicValue))
                throw new InvalidOperationException("Value is not KnowSymbolicValue");

            return Perform((KnownSymbolicValue)val);
        }

        /// <summary>
        /// Applies the test. The test checks if an attribute value belongs to a given set of values.
        /// </summary>
        /// <param name="val">The value to test. </param>
        /// <returns>1 - the value belongs to the set of admitted values, 0 - otherwise.</returns>
        public int Perform(KnownSymbolicValue val)
        {
            foreach (KnownSymbolicValue kv in _values)
            {
                if (kv.Equals(val))
                    return 1;
            }

            return 0;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(this.Attribute.ToString());
            sb.Append(" in [");

            foreach (KnownSymbolicValue ksv in _values)
            {
                sb.Append(ksv);
            }
            sb.Append("]");

            return sb.ToString();
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
                    throw new ArgumentException("Invalid issue number");
            }
        }

        #endregion
    }
}
