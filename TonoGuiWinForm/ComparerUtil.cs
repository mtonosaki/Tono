using System;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �\�[�g�@�\���T�|�[�g���܂�
    /// </summary>
    public class ComparerUtil
    {
        /// <summary>
        /// �啶���������������Ĕ�r����N���X
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class IgnoreCaseEquality<T> : IEqualityComparer<T>
        {
            #region IEqualityComparer<T> �����o

            public bool Equals(T x, T y)
            {
                return x.ToString().Equals(y.ToString(), StringComparison.CurrentCultureIgnoreCase);
            }

            public int GetHashCode(T obj)
            {
                return obj.ToString().ToUpper().GetHashCode();
            }

            #endregion
        }

        public static IComparer ListViewString(int columnNo)
        {
            return new listViewString(columnNo);
        }
        #region	�e�^�ɉ�����Comparer
        private class listViewString : IComparer
        {
            private readonly int _colNo;

            public listViewString(int columnNo)
            {
                _colNo = columnNo;
            }

            #region IComparer �����o

            public int Compare(object x, object y)
            {
                return ((System.Windows.Forms.ListViewItem)x).Text.CompareTo(((System.Windows.Forms.ListViewItem)y).Text);
            }

            #endregion
        }

        /// <summary>
        /// long�^�̔�r
        /// </summary>
        public static readonly IComparer LongComparer = new longComparer();
        private class longComparer : IComparer
        {
            #region IComparer �����o

            public int Compare(object x, object y)
            {
                return (long)x == (long)y ? 0 : ((long)x < (long)y ? -1 : 1);
            }

            #endregion
        }

        /// <summary>
        /// string�^�̔�r
        /// </summary>
        public static readonly IComparer StringComparer = new stringComparer();
        /// <summary>
        /// string�^�̔�r
        /// </summary>
        private class stringComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                var d1 = (string)x;
                var d2 = (string)y;
                return d1.CompareTo(d2);
            }
        }

        /// <summary>
        /// int�^�̔�r
        /// </summary>
        public static readonly IComparer IntComparer = new intComparer();
        /// <summary>
        /// int�^�̔�r
        /// </summary>
        private class intComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                var d1 = (int)x;
                var d2 = (int)y;
                return d1.CompareTo(d2);
            }
        }

        /// <summary>
        /// double�^�̔�r
        /// </summary>
        public static readonly IComparer DoubleComparer = new doubleComparer();
        /// <summary>
        /// double�^�̔�r
        /// </summary>
        private class doubleComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                var d1 = (double)x;
                var d2 = (double)y;
                return d1.CompareTo(d2);
            }
        }

        /// <summary>
        /// ITimeSpan�^��Start��r
        /// </summary>
        public static readonly IComparer TimeSpanStartComparer = new timeSpanStartComparer();
        /// <summary>
        /// ITimeSpan�^��Start��r
        /// </summary>
        private class timeSpanStartComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                var d1 = (ITimeSpan)x;
                var d2 = (ITimeSpan)y;
                return d1.Start.CompareTo(d2.Start);
            }
        }

        /// <summary>
        /// ITimeSpan�^��End��r
        /// </summary>
        public static readonly IComparer TimeSpanEndComparer = new timeSpanEndComparer();
        /// <summary>
        /// ITimeSpan�^��End��r
        /// </summary>
        private class timeSpanEndComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                var d1 = (ITimeSpan)x;
                var d2 = (ITimeSpan)y;
                return d1.End.CompareTo(d2.End);
            }
        }

        /// <summary>
        /// uCdRect�^�̍��[��X���W�̔�r
        /// </summary>
        public static readonly IComparer CdRectLComparer = new cdRectLComparer();
        /// <summary>
        /// uCdRect�^�̍��[��X���W�̔�r
        /// </summary>
        private class cdRectLComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                var cdr1 = (CodeRect)x;
                var cdr2 = (CodeRect)y;
                return cdr1.LT.X.CompareTo(cdr2.LT.X);
            }
        }

        /// <summary>
        /// �T�̕��� M, T, W, R, F, A, S���l��������r���Z
        /// </summary>
        public static readonly IComparer WeekStrComparer = new weekStrComparer();
        /// <summary>
        /// �T�̕��� M, T, W, R, F, A, S���l��������r���Z
        /// </summary>
        public static readonly IComparer<string> WeekStrComparerT = new weekStrComparer();
        /// <summary>
        /// �T�̕��� M, T, W, R, F, A, S���l��������r���Z
        /// </summary>
        private class weekStrComparer : IComparer, IComparer<string>
        {
            #region IComparer �����o

            public int Compare(object x, object y)
            {
                var s0 = x == null ? "" : x.ToString();
                var s1 = y == null ? "" : y.ToString();
                if (s0.Length < 1)
                {
                    s0 = ("?" + s0);
                }

                if (s1.Length < 1)
                {
                    s1 = ("?" + s1);
                }

                var p0 = dayNo(s0[0]);
                var p1 = dayNo(s1[0]);
                var pc = p0 - p1;
                if (pc == 0)
                {
                    return StrUtil.ComparerThinkNumber(s0, s1);
                }
                else
                {
                    return pc;
                }
            }

            #endregion

            private int dayNo(char day)
            {
                switch (day)
                {
                    case 'M':
                    case 'm': return 1;
                    case 'T':
                    case 't': return 2;
                    case 'W':
                    case 'w': return 3;
                    case 'R':
                    case 'r': return 4;
                    case 'F':
                    case 'f': return 5;
                    case 'A':
                    case 'a': return 6;
                    case 'S':
                    case 's': return 7;
                    default: return 0;
                }
            }

            #region IComparer<string> �����o

            public int Compare(string x, string y)
            {
                return Compare(x, (object)y);
            }

            #endregion
        }

        #endregion

        #region	�e�^�ɉ�����Comparer
        public class CompareListViewString : IComparer
        {
            private readonly int _colNo;

            public CompareListViewString(int columnNo)
            {
                _colNo = columnNo;
            }

            #region IComparer �����o

            public int Compare(object x, object y)
            {
                return ((System.Windows.Forms.ListViewItem)x).Text.CompareTo(((System.Windows.Forms.ListViewItem)y).Text);
            }

            #endregion
        }

        public class ComparerLong : IComparer
        {
            #region IComparer �����o

            public int Compare(object x, object y)
            {
                return (long)x == (long)y ? 0 : ((long)x < (long)y ? -1 : 1);
            }

            #endregion
        }

        /// <summary>
        /// string�^�̔�r
        /// </summary>
        public class ComparerString : IComparer
        {
            public int Compare(object x, object y)
            {
                var d1 = (string)x;
                var d2 = (string)y;
                return d1.CompareTo(d2);
            }
        }

        /// <summary>
        /// int�^�̔�r
        /// </summary>
        public class ComparerInt : IComparer
        {
            public int Compare(object x, object y)
            {
                var d1 = (int)x;
                var d2 = (int)y;
                return d1.CompareTo(d2);
            }
        }

        /// <summary>
        /// double�^�̔�r
        /// </summary>
        public class ComparerDouble : IComparer
        {
            public int Compare(object x, object y)
            {
                var d1 = (double)x;
                var d2 = (double)y;
                return d1.CompareTo(d2);
            }
        }

        /// <summary>
        /// ITimeSpan�^��Start��r
        /// </summary>
        public class ComparerTimeSpanStart : IComparer
        {
            public int Compare(object x, object y)
            {
                var d1 = (ITimeSpan)x;
                var d2 = (ITimeSpan)y;
                return d1.Start.CompareTo(d2.Start);
            }
        }

        /// <summary>
        /// ITimeSpan�^��End��r
        /// </summary>
        public class ComparerTimeSpanEnd : IComparer
        {
            public int Compare(object x, object y)
            {
                var d1 = (ITimeSpan)x;
                var d2 = (ITimeSpan)y;
                return d1.End.CompareTo(d2.End);
            }
        }

        /// <summary>
        /// uCdRect�^�̍��[��X���W�̔�r
        /// </summary>
        public class ComparerCdRectL : IComparer
        {
            public int Compare(object x, object y)
            {
                var cdr1 = (CodeRect)x;
                var cdr2 = (CodeRect)y;
                return cdr1.LT.X.CompareTo(cdr2.LT.X);
            }
        }

        /// <summary>
        /// LinkedList�ɁA�}���ӏ��𒲂ׂȂ���}������
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <param name="comparer"></param>
        /// <param name="isFrontput">true=���l�̓L���[�̑O�� / false=���</param>
        public static void AddAfterComparedItem<T>(LinkedList<T> list, T item, Func<T, T, int> comparer)
        {
            var node = list.Last;
            while (node != null)
            {
                if (comparer(node.Value, item) <= 0)
                {
                    list.AddAfter(node, item);
                    return;
                }
                node = node.Previous;
            }
            list.AddFirst(item);
        }

        /// <summary>
        /// LinkedList�ɁA�}���ӏ��𒲂ׂȂ���}������
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <param name="comparer"></param>
        /// <param name="isFrontput">true=���l�̓L���[�̑O�� / false=���</param>
        public static void AddBeforeComparedItem<T>(LinkedList<T> list, T item, Func<T, T, int> comparer)
        {
            var node = list.First;
            while (node != null)
            {
                if (comparer(node.Value, item) >= 0)
                {
                    list.AddBefore(node, item);
                    return;
                }
                node = node.Next;
            }
            list.AddLast(item);
        }

        #endregion
        #region	����(�V���A���C�Y����)
        #endregion
        #region	����(�V���A���C�Y���Ȃ�)
        #endregion
    }
}
