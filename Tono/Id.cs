// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

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
        /// make instance from int32
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Id From(int value)
        {
            return new Id { Value = value, };
        }

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
            return Value;                           // TonoId.Test_TreeSpeed :   212ms for 0...2M sequence Id Dictionary write-read
            //return Binary.ByteReverse(Value);     // TonoId.Test_TreeSpeed : 1,007ms for 0...2M sequence Id Dictionary write-read
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
