namespace Tono
{
    /// <summary>
    /// ID type for safe caluclation 
    /// </summary>
    public struct Id
    {
        /// <summary>
        /// no value ID
        /// </summary>
        public static readonly Id Nothing = new Id { Value = -1, };

        /// <summary>
        /// ID integer value
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// test if nothing value
        /// </summary>
        /// <returns></returns>
        public bool IsNothing()
        {
            return Equals(Nothing);
        }

        public static bool operator ==(Id left, Id right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Id left, Id right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return Value ^ 0x55555555;
            //	return uBinary.BitRotateRight(0x78965489, Value & 31) ^ uBinary.BitRotateRight(0x513e7951, Value & 15); // if hash code distribution to expect tree balance
        }

        public override bool Equals(object obj)
        {
            if (obj is Id tar)
            {
                return Value == tar.Value;
            }
            else
            {
                return false;
            }
        }
        public override string ToString()
        {
            return $"ID={Value}";
        }
    }
}
