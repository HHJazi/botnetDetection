
namespace Biotracker.Signature.DT
{
    /// <summary>
    /// An abstract representation of an attribute.
    /// </summary>
    public class Attribute
    {
        /// <summary>
        /// Attribute name.
        /// </summary>
        public string Name { get; set; }

        public Attribute()
        {
            Name = default(string);
        }

        public Attribute(string name)
        {
            this.Name = name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            if (string.IsNullOrEmpty((obj as Attribute).Name))
            {
                if (string.IsNullOrEmpty(Name))
                    return true;
                else
                    return false;
            }
            else 
                return string.Compare(Name, (obj as Attribute).Name, true) == 0;
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? "No name defined" : Name;
        }
    }
}
