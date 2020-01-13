// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Specialized;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// Finalizer Manager の概要の説明です。
    /// </summary>
    public class FinalizeManager
    {
        /// <summary>
        /// ファイナライズのためにデリゲート型
        /// </summary>
        public delegate void Finalize();

        #region 属性（シリアライズしない）
        private readonly Finalize _func = null;
        #endregion

        /// <summary>
        /// 唯一のコンストラクタ
        /// </summary>
        /// <param name="function">ファイナライズする時に実行するデリゲートメソッド</param>
        public FinalizeManager(Finalize function)
        {
            _func = function;
        }

        /// <summary>
        /// デリゲートしたメソッドを実行する
        /// </summary>
        public bool Invoke()
        {
            if (_func != null)
            {
                _func();
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// ファイナライズを保存する
    /// </summary>
    public class FinalizeManageBuffer : IEnumerable
    {
        private class Enumerator : IEnumerator
        {
            private readonly IDictionary _dat1;
            private readonly IList _dat2;
            private int _curData = 0;
            private IEnumerator _cur;

            public Enumerator(IDictionary value1, IList value2)
            {
                _dat1 = value1;
                _dat2 = value2;
                Reset();
            }

            #region IEnumerator メンバ

            public void Reset()
            {
                _cur = _dat1.Values.GetEnumerator();
                _curData = 0;
            }

            public object Current => _cur.Current;

            public bool MoveNext()
            {
                var ret = _cur.MoveNext();
                if (ret == false)
                {
                    if (_curData == 0)
                    {
                        _curData++;
                        _cur = _dat2.GetEnumerator();
                        return MoveNext();
                    }
                }
                return ret;
            }

            #endregion

        }
        #region 属性（シリアライズしない）
        /// <summary>グループ管理するファイナライザ</summary>
        private readonly IDictionary _dat1 = new HybridDictionary();

        /// <summary>グループ管理しないファイナライザ</summary>
        private readonly IList _dat2 = new ArrayList();
        #endregion

        /// <summary>
        /// ファイナライザを登録する
        /// </summary>
        /// <param name="key">ファイナライズグループID（Idから取得すると便利）</param>
        /// <param name="value">ファイナライザ</param>
        public void Add(NamedId key, FinalizeManager.Finalize value)
        {
            _dat1[key] = new FinalizeManager(value);
        }
        /// <summary>
        /// ファイナライザを登録する（IDなしのファイナライザは先に実行される）
        /// </summary>
        /// <param name="value">ファイナライザ</param>
        public void Add(FinalizeManager.Finalize value)
        {
            _dat2.Add(new FinalizeManager(value));
        }

        /// <summary>
        /// 指定グループのファイナライザが登録されているか調査する
        /// </summary>
        /// <param name="key">グループ番号</param>
        /// <returns>true = 登録されている / false = 登録されていない</returns>
        public bool Contains(NamedId key)
        {
            return _dat1.Contains(key);
        }

        /// <summary>
        /// 登録済みのファイナライザを実行しないで削除する
        /// </summary>
        public void Clear()
        {
            _dat1.Clear();
            _dat2.Clear();
        }

        /// <summary>
        /// 登録しているファイナライザをすべて実行する
        /// </summary>
        public bool Flush()
        {
            var cnt = 0;
            try
            {
                for (var en = GetEnumerator(); en.MoveNext();)
                {
                    var res = ((FinalizeManager)en.Current).Invoke();
                    if (res)
                    {
                        cnt++;
                    }
                }
                Clear();
            }
            catch (Exception ex)
            {
                LOG.WriteLineException(ex);
            }
            return cnt > 0;
        }

        #region IEnumerable メンバ

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(_dat1, _dat2);
        }

        #endregion
    }
}
