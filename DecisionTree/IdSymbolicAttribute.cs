using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// A symbolic attribute where each attribute value is associated with an object, usually its name
    /// coded as a string
    /// </summary>
    public class IdSymbolicAttribute : SymbolicAttribute
    {
        private List<string> _ids;
        public List<string> IDS
        {
            get { return _ids; }
            set { _ids = value; }
        }

        public IdSymbolicAttribute() : base()
        { 
        
        }

        public IdSymbolicAttribute(IEnumerable<string> ids)
            : base(ids.Count<string>())
        {
            this._ids = ids.ToList<string>();
        }

        public IdSymbolicAttribute(string name, IEnumerable<string> ids)
            : base(name, ids.Count<string>())
        {
            this._ids = ids.ToList();
        }


        public int AddValue(string val)
        {
            if (!_ids.Contains(val))
            {
                _ids.Add(val);
                base.NumOfValues = _ids.Count;
            }

            return _ids.IndexOf(val);
        }

        /// <summary>
        /// Convert a symbolic value to string.  
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public string ValueToString(SymbolicValue val)
        {
            if (val is UnknownSymbolicValue)
                return val.ToString();

            int index = ((KnownSymbolicValue)val).IntValue;

            return this._ids[index].ToString();
        }

        public override string ToString()
        { 
            StringBuilder sb= new StringBuilder();

            sb.Append(Name);
            sb.Append(": ");
 
            int i = 0;
            foreach(object o in _ids)
            {
                sb.AppendFormat("{0}-{1};", i, o.ToString());
                i++;
            }

            return sb.ToString();
        }
    }
}
