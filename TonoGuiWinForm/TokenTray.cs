// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// トークンを受ける場所（トークンとは、他のフィーチャーを起動するための仕組み）
    /// </summary>
    public class TokenTray
    {
        //		private IDictionary _dat = new HybridDictionary();	// TONO
        private readonly IDictionary _dat = new Hashtable();

        /// <summary>InvokeStartの起動済みフラグ用変数</summary>
        //		private /*friane fgBase*/IDictionary TokenInvokedChecker = new HybridDictionary();	// TONO
        private /*friane fgBase*/readonly IDictionary TokenInvokedChecker = new Hashtable();

        /// <summary>
        /// 指定フィーチャーのトークンリスナーに指定IDが含まれているかどうか調べる
        /// </summary>
        /// <param name="fo"></param>
        /// <param name="tokenid"></param>
        /// <returns></returns>
        public static bool ContainsTokenID(FeatureBase fo, NamedId tokenid)
        {
            if (fo is ITokenListener)
            {
                if (((ITokenListener)fo).TokenTriggerID == tokenid)
                {
                    return true;
                }
            }
            if (fo is IMultiTokenListener)
            {
                foreach (var fid in ((IMultiTokenListener)fo).MultiTokenTriggerID)
                {
                    if (fid == tokenid)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// トークンを追加（予約）する。これはFlushを実行したときに、本当に追加される
        /// </summary>
        /// <param name="id">トークンに付けた名前</param>
        /// <param name="from">トークンを追加したクラス</param>
        public void Add(NamedId id, FeatureBase from)
        {
            System.Diagnostics.Debug.Assert(id != null, "ffTokenTray.Addにnull id を指定しないでください");
            lock (_dat.SyncRoot)
            {
                _dat[id] = from;
            }

            // トークン処理中にトークン追加された場合、起動済みフィーチャーを解除して、そのフィーチャーをもう一度起動できるようにする
            IList dels = new ArrayList();
            lock (dels.SyncRoot)
            {
                foreach (FeatureBase fo in TokenInvokedChecker.Keys) // fo = 起動済みフィーチャー
                {
                    if (ContainsTokenID(fo, id))
                    {
                        dels.Add(fo);
                    }
                }
                foreach (FeatureBase fo in dels)
                {
                    TokenInvokedChecker.Remove(fo);
                }
            }
        }

        /// <summary>
        /// トークンに登録された全ID数
        /// </summary>
        public int Count => _dat.Count;

        /// <summary>
        /// 指定IDが含まれているかどうかを調べる
        /// </summary>
        /// <param name="value">ID</param>
        /// <returns>true = 含まれている / false = 含まれない</returns>
        public bool Contains(NamedId value)
        {
            if (value == null)
            {
                return false;
            }
            return _dat.Contains(value);
        }

        /// <summary>
        /// 指定フィーチャーが要求するリスナーがTokenTrayにあるかどうかを調査する
        /// </summary>
        /// <param name="value">調査対象とするフィーチャー</param>
        /// <returns>true = 含まれている / false = 含まれない</returns>
        public bool Contains(FeatureBase value)
        {
            var ret = false;
            if (value is ITokenListener)
            {
                ret = _dat.Contains(((ITokenListener)value).TokenTriggerID);
            }
            if (!ret && value is IMultiTokenListener)
            {
                foreach (var id in ((IMultiTokenListener)value).MultiTokenTriggerID)
                {
                    ret = _dat.Contains(id);
                    if (ret)
                    {
                        break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// すべてのトークンを削除する
        /// </summary>
        internal void _clear()
        {
            lock (_dat.SyncRoot)
            {
                _dat.Clear();
            }
        }
    }
}
