// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// データデフォルト
    /// データオブジェクトが不要な場合に使用してください
    /// </summary>
    public sealed class DataHotDefault : DataHotBase
    {
    }


    /// <summary>
    /// アプリケーション固有のデータを管理する基本クラス
    /// </summary>
    [Serializable]
    public abstract class DataHotBase
    {
        #region シリアライズしない状態変数
        [NonSerialized]
        private bool _isModified = false;
        /// <summary>
        /// ダーティーフラグを入れる
        /// </summary>
        public virtual void SetModified()
        {
            _isModified = true;
        }
        /// <summary>
        /// ダーティーフラグを変更する
        /// </summary>
        /// <param name="sw"></param>
        public virtual void SetModified(bool sw)
        {
            _isModified = sw;
        }
        /// <summary>
        /// ダーティーフラグ
        /// </summary>
        public virtual bool IsModified => _isModified;
        #endregion
        #region 属性（シリアライズしない）
        [NonSerialized]
        private readonly object _mySingleton = new object();
        #endregion

        /// <summary>
        /// 同期ルート（データ全体）
        /// </summary>
        public virtual object SyncRoot => _mySingleton;

        /// <summary>
        /// 全アプリケーションデータを消去する
        /// </summary>
        public virtual void Clear()
        {
        }

        /// <summary>
        /// レコードを削除する
        /// </summary>
        /// <param name="table">テーブル</param>
        /// <param name="rec">レコード</param>
        public virtual void Remove(TableCollection table, object rec)
        {
            table.DirectRemove(rec);
        }

        /// <summary>
        /// レコードを追加する
        /// </summary>
        /// <param name="table">テーブル</param>
        /// <param name="rec">レコード</param>
        public virtual void Add(TableCollection table, object rec)
        {
            table.DirectAdd(rec);
        }
    }
}
