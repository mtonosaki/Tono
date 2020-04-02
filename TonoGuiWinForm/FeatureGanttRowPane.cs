// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections;
using System.Collections.Specialized;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeatureGanttRowPane の概要の説明です。
    /// ガントチャートの行ヘッダの表示・管理
    /// </summary>
    public class FeatureGanttRowPane : FeatureBase
    {
        #region 場所を検索するためのクラス

        /// <summary>
        /// 行を特定するための検索装置
        /// </summary>
        public class RowManager
        {
            private readonly IDictionary _idpos = new HybridDictionary();
            private readonly IDictionary _posid = new HybridDictionary();

            /// <summary>
            /// ライン数
            /// </summary>
            /// <returns></returns>
            public int GetLineN()
            {
                return _posid.Count + 1;
            }

            /// <summary>
            /// データが見当たらない場合の値
            /// NA
            /// </summary>
            public static int NA => int.MinValue + 100;

            /// <summary>
            /// データを登録する
            /// </summary>
            /// <param name="id">検索キー</param>
            /// <param name="ptpos">パーツ座標</param>
            public void Add(Id rowid, int ptpos)
            {
                _idpos[rowid.Value] = ptpos;
                _posid[ptpos] = rowid.Value;
            }

            /// <summary>
            /// 全て登録済みのIDPOS関連を削除する
            /// </summary>
            public void Clear()
            {
                _idpos.Clear();
                _posid.Clear();
            }

            /// <summary>
            /// 行IDを指定してパーツ座標を返す
            /// 登録されていない場合はNAを返す
            /// </summary>
            public int this[Id rowid]
            {
                get
                {
                    var ret = _idpos[rowid.Value];
                    if (ret == null)
                    {
                        return NA;
                    }
                    return (int)ret;
                }
            }

            /// <summary>
            /// パーツ座標を指定して行IDを返す
            /// 登録されていない場合はNAを返す
            /// </summary>
            public Id this[int ptpos]
            {
                get
                {
                    var ret = _posid[ptpos];
                    if (ret == null)
                    {
                        return new Id { Value = NA };
                    }
                    return new Id { Value = (int)ret };
                }
            }

            /// <summary>
            /// IDのコレクションを返す
            /// </summary>
            /// <returns>ID一覧</returns>
            public ICollection GetIDs()
            {
                return _idpos.Keys;
            }

            /// <summary>
            /// パーツ座標のコレクションを返す
            /// </summary>
            /// <returns>パーツ座標一覧</returns>
            public ICollection GetPartsPositions()
            {
                return _posid.Keys;
            }

            /// <summary>
            /// Key=ID / Value=PosのDictionaryEnumeratorを取得する
            /// </summary>
            /// <returns>IDictionaryEnumerator</returns>
            public IDictionaryEnumerator GetIDPosEnumerator()
            {
                return _idpos.GetEnumerator();
            }
        }
        #endregion
    }
}
