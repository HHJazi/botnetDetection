using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biotracker.Signature.DT
{
    /// <summary>
    /// Holds the values of the attributes of an element of the learning/testing set.
    /// </summary>
    public class Item
    {
        private AttributeValue[] _values;


        public Item(AttributeValue[] values)
        {
            if (values == null)
                throw new ArgumentNullException();

            this._values = values;
        }


        #region Public Methods

        /// <summary>
        /// Return the value of an attribute
        /// </summary>
        /// <param name="attrs"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public AttributeValue ValueOf(AttributeSet attrs, Attribute attribute)
        {
            return this.ValueOf(attrs.IndexOf(attribute));
        }

        /// <summary>
        /// Return the value of an attribute.
        /// </summary>
        /// <param name="index">Index of the attribute</param>
        /// <returns>the attribute value</returns>
        public AttributeValue ValueOf(int index)
        {
            return _values[index];
        }

        public AttributeValue[] ToArray()
        {
            return _values;
        }

        /// <summary>
        /// Return the item's number of attributes.
        /// </summary>
        /// <returns></returns>
        public int NumberOfAttributes()
        {
            return _values.Length;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (AttributeValue av in _values)
            {
                sb.AppendFormat("[{0}]", av.ToString());
            }

            return sb.ToString();
        }
        #endregion
    }
}
