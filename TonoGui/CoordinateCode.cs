namespace Tono.Gui
{
    /// <summary>
    /// Code coodinate type
    /// </summary>
    /// <typeparam name="TCX">type of code value</typeparam>
    /// <remarks>
    /// This GUI framework separates coodinates to Code, Layout and Screen.
    /// Code is useful for debug, design, log recording...
    /// </remarks>
    public class CodeX<TCX>
    {
        /// <summary>
        /// Code X
        /// </summary>
        public TCX Cx { get; set; }

        public override int GetHashCode()
        {
            return Cx.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is CodeX<TCX> tar)
            {
                return Cx.Equals(tar.Cx);
            }
            else
            {
                return Cx.Equals(obj);
            }
        }
        public override string ToString()
        {
            return $"Cx:{Cx}";
        }

        public static CodeX<TCX> From(TCX val)
        {
            return new CodeX<TCX>
            {
                Cx = val,
            };
        }
    }

    /// <summary>
    /// Code coodinate type
    /// </summary>
    /// <typeparam name="TCY">type of code value</typeparam>
    /// <remarks>
    /// This GUI framework separates coodinates to Code, Layout and Screen.
    /// Code is useful for debug, design, log recording...
    /// </remarks
    public class CodeY<TCY>
    {
        /// <summary>
        /// Code Y
        /// </summary>
        public TCY Cy { get; set; }

        public override int GetHashCode()
        {
            return Cy.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is CodeY<TCY> tar)
            {
                return Cy.Equals(tar.Cy);
            }
            else
            {
                return Cy.Equals(obj);
            }
        }

        public override string ToString()
        {
            return $"Cy:{Cy}";
        }
        public static CodeY<TCY> From(TCY val)
        {
            return new CodeY<TCY>
            {
                Cy = val,
            };
        }
    }

    /// <summary>
    /// X,Y code values
    /// </summary>
    /// <typeparam name="TCX"></typeparam>
    /// <typeparam name="TCY"></typeparam>
    public class CodePos<TCX, TCY>
    {
        public CodeX<TCX> X { get; set; }
        public CodeY<TCY> Y { get; set; }

        public static CodePos<TCX, TCY> From(CodeX<TCX> cx, CodeY<TCY> cy)
        {
            return new CodePos<TCX, TCY>
            {
                X = cx,
                Y = cy,
            };
        }

        public static CodePos<TCX, TCY> From(TCX cx, TCY cy)
        {
            return new CodePos<TCX, TCY>
            {
                X = CodeX<TCX>.From(cx),
                Y = CodeY<TCY>.From(cy),
            };
        }

        public override bool Equals(object obj)
        {
            if (obj is CodePos<TCX, TCY> tar)
            {
                return tar.X.Equals(X) && tar.Y.Equals(Y);
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            int h1 = X.GetHashCode();
            int h2 = Y.GetHashCode();
            return h1 ^ h2;
        }

        public override string ToString()
        {
            return $"C({X},{Y})";
        }
    }
}
