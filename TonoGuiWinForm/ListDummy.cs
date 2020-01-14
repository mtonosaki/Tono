// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{

    /// <summary>
    /// Addしてもデータが格納できない偽リストオブジェクト
    /// </summary>
    public class ListDummy : IList
    {
        #region IList メンバ

        public int Add(object value)
        {
            return 0;
        }

        public void Clear()
        {
        }

        public bool Contains(object value)
        {
            return false;
        }

        public int IndexOf(object value)
        {
            return -1;
        }

        public void Insert(int index, object value)
        {
        }

        public bool IsFixedSize => true;

        public bool IsReadOnly => false;

        public void Remove(object value)
        {
        }

        public void RemoveAt(int index)
        {
        }

        private static readonly IList _zero = new ArrayList();

        public object this[int index]
        {
            get => _zero[index];
            set
            {
            }
        }

        #endregion

        #region ICollection メンバ

        public void CopyTo(Array array, int index)
        {
        }

        public int Count => 0;

        public bool IsSynchronized => true;

        public object SyncRoot => new object();

        #endregion

        #region IEnumerable メンバ

        public IEnumerator GetEnumerator()
        {
            return _zero.GetEnumerator();
        }

        #endregion
    }

    /// <summary>
    /// Addしてもデータが格納できない偽リストオブジェクト
    /// </summary>
    public class ListDummy<T> : IList<T>
    {
        private static readonly List<T> _zero = new List<T>();

        #region IList<T> メンバ

        public int IndexOf(T item)
        {
            return -1;
        }

        public void Insert(int index, T item)
        {
        }

        public void RemoveAt(int index)
        {
        }

        public T this[int index]
        {
            get => _zero[index];
            set
            {
            }
        }

        #endregion

        #region ICollection<T> メンバ

        public void Add(T item)
        {
        }

        public void Clear()
        {
        }

        public bool Contains(T item)
        {
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
        }

        public int Count => 0;

        public bool IsReadOnly => true;

        public bool Remove(T item)
        {
            return false;
        }

        #endregion

        #region IEnumerable<T> メンバ

        public IEnumerator<T> GetEnumerator()
        {
            return _zero.GetEnumerator();
        }

        #endregion

        #region IEnumerable メンバ

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _zero.GetEnumerator();
        }

        #endregion
    }
}
