// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

#pragma warning disable 1591, 1572, 1573

using System;

namespace Tono.GuiWinForm
{
    public static class Validator
    {
        /// <summary>
        /// 例外クラス
        /// </summary>
        public class TestFailureException : System.Exception
        {
            /// <summary>
            /// デフォルトコンストラクタ
            /// </summary>
            public TestFailureException()
            {
            }

            /// <summary>
            /// メッセージ指定コンストラクタ
            /// </summary>
            /// <param name="mes"></param>
            public TestFailureException(string mes) : base(mes)
            {
            }
        }

        /// <summary>
        /// 一致判定
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="meskey"></param>
        /// <param name="mesver"></param>
        /// <param name="param"></param>
        public static void AreEqual<T>(T a, T b, string meskey, string mesver, params object[] param)
        {
            if (a.Equals(b) == false)
            {
                LOG.WriteMesLine(meskey, mesver, param);
                throw new TestFailureException(string.Format("Not Equals {0} to {1}", a.ToString(), b.ToString()));
            }
        }

        public static void AreLessOrEqual<T>(T a, T b, string meskey, string mesver, params object[] param) where T : IComparable<T>
        {
            if (a.CompareTo(b) > 0)
            {
                LOG.WriteMesLine(meskey, mesver, param);
                throw new TestFailureException(string.Format("Not Less or Equals {0} to {1}", a.ToString(), b.ToString()));
            }
        }

        /// <summary>
        /// Trueかどうか判断する
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="meskey"></param>
        /// <param name="mesver"></param>
        /// <param name="param"></param>
        public static void IsTrue(bool condition, string meskey, string mesver, params object[] param)
        {
            if (condition == false)
            {
                LOG.WriteMesLine(meskey, mesver, param);
                throw new TestFailureException("Boolean check error");
            }
        }

        /// <summary>
        /// Falseかどうか判断する
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="meskey"></param>
        /// <param name="mesver"></param>
        /// <param name="param"></param>
        public static void IsFalse(bool condition, string meskey, string mesver, params object[] param)
        {
            IsTrue(!condition, meskey, mesver, param);
        }
    }
}
