// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// Valudate utility
    /// </summary>
    public static class Validator
    {
        /// <summary>
        /// validate error exception
        /// </summary>
        public class TestFailureException : System.Exception
        {
            /// <summary>
            /// default constructor
            /// </summary>
            public TestFailureException()
            {
            }

            /// <summary>
            /// initialize constructor
            /// </summary>
            /// <param name="mes"></param>
            public TestFailureException(string mes) : base(mes)
            {
            }
        }

        /// <summary>
        /// check same a with b
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="meskey"></param>
        /// <param name="param"></param>
        public static void AreEqual<T>(T a, T b, string meskey, params object[] param)
        {
            if (a.Equals(b) == false)
            {
                LOG.AddMes(LLV.Auto, meskey, param);
                throw new TestFailureException(string.Format("Not Equals {0} to {1}", a.ToString(), b.ToString()));
            }
        }

        /// <summary>
        /// check true
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="meskey"></param>
        /// <param name="mesver"></param>
        /// <param name="param"></param>
        public static void IsTrue(bool condition, string meskey, params object[] param)
        {
            if (condition == false)
            {
                LOG.AddMes(LLV.Auto, meskey, param);
                throw new TestFailureException("Boolean check error");
            }
        }

        /// <summary>
        /// check true
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="extendDesign">NG時のログに表示するアイコンなど</param>
        /// <param name="meskey"></param>
        /// <param name="param"></param>
        public static void IsTrue(bool condition, LogAccessor.ExtendDesign extendDesign, string meskey, params object[] param)
        {
            if (condition == false)
            {
                LOG.AddMes(LLV.Auto, extendDesign, meskey, param);
                throw new TestFailureException("Boolean check error");
            }
        }

        /// <summary>
        /// check false
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="meskey"></param>
        /// <param name="mesver"></param>
        /// <param name="param"></param>
        public static void IsFalse(bool condition, string meskey, params object[] param)
        {
            LOG.AddMes(LLV.Auto, meskey, param);
            IsTrue(!condition, meskey, param);
        }
    }
}
