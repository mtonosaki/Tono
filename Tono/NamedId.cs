namespace Tono
{
    /// <summary>
    /// ID type that have name
    /// </summary>
    public class NamedId
    {
        /// <summary>
        /// nothing id
        /// </summary>
        public static readonly NamedId Nothing = From("Nothing", int.MinValue);

        /// <summary>
        /// ID integer value
        /// </summary>
        public int Value { get; private set; }

        /// <summary>
        /// the ID's name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Make instance
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static NamedId From(string name, int value)
        {
            return new NamedId
            {
                Value = value,
                Name = name,
            };
        }

        public override string ToString()
        {
            return $"{Name}(id={Value})";
        }
        public override bool Equals(object obj)
        {
            if (obj is NamedId ni)
            {
                return ni.Value == Value;
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return Value;
        }
    }
}
