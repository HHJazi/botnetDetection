using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// This class holds an odered set of attributes.
    /// This object is immutable: attributes cannot be added or removed. this ensure that 
    /// an attribute indes will not change over time.
    /// </summary>
    public class AttributeSet
    {
        private List<Attribute> _attributes;
        public List<Attribute> Attributes
        {
            get { return _attributes; }
            set { _attributes = value; }
        }

        public AttributeSet()
        {
            _attributes = null;
        }

        public AttributeSet(IEnumerable<Attribute> attributes)
        {
            _attributes = new List<Attribute>();

            if (attributes != null)
            {
                _attributes.AddRange(attributes);
            }
        }

        #region Public Methods

        /// <summary>
        /// Search the index of a given attribute.
        /// </summary>
        /// <param name="attr"></param>
        /// <returns>zero-based index - found; -1 - not found.</returns>
        public int IndexOf(Attribute attr)
        {
            return _attributes.IndexOf(attr);
        }

        public int Size()
        {
            return _attributes.Count;
        }

        public Attribute GetAttribute(int index)
        {
            return _attributes[index];
        }

        public bool Contains(Attribute attr)
        {
            return _attributes.Contains(attr);
        }

        /// <summary>
        /// Find an attribute by its name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Attribute FindByName(string name)
        {
            foreach (Attribute attr in _attributes)
            {
                if (string.Compare(attr.Name, name, true) == 0)
                    return attr;
            }

            return null;
        }


        public IEnumerable<Attribute> GetAttributes()
        {
            return _attributes;
        }

        public override int GetHashCode()
        {
            return Size();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            return false;

            if ( obj.GetType() != GetType())
                return false;

            return this._attributes.Equals(((AttributeSet)obj)._attributes);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Attribute attr in _attributes)
            {
                sb.AppendLine(attr.ToString());
            }

            return sb.ToString();
        }

        #endregion

        #region Private Methods

        private void Add(Attribute attr)
        {
            if (!_attributes.Contains(attr))
            {
                _attributes.Add(attr);
            }
        }

        #endregion


    }
}
