// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections;
using System.Collections.Specialized;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ffPersister の概要の説明です。
    /// オブジェクト永続化処理を担当
    /// </summary>
    public class PersistManager
    {
        public interface IGroupID
        {
            Id PersisterGroupID
            {
                get;
            }
        }
        #region 属性（シリアライズしない）

        /// <summary>レコーダ管理</summary>
        private readonly IDictionary/*<string name,IRecorder>*/ _recorders = new HybridDictionary();

        #endregion

        /// <summary>
        /// レコーダを指定する
        /// </summary>
        /// <param name="value">レコーダ</param>
        public void AddRecorder(IRecorder value, Id id)
        {
            _recorders.Add(id.Value, value);
        }

        /// <summary>
        /// レコーダーを解除する
        /// </summary>
        /// <param name="value">レコーダー</param>
        public void RemoveRecorder(Id value)
        {
            var rec = (IRecorder)_recorders[value.Value];
            _recorders.Remove(value.Value);
        }

        /// <summary>
        /// レコーダーを解除する
        /// </summary>
        /// <param name="value">レコーダー</param>
        public void RemoveRecorder(IRecorder value)
        {
            foreach (DictionaryEntry de in _recorders)
            {
                if (de.Value == value)
                {
                    _recorders.Remove(de.Key);
                    return;
                }
            }
        }

        /// <summary>
        /// レコーダーへのブリッジ
        /// </summary>
        public class RecorderBridge
        {
            /// <summary>対象とするレコーダーオブジェクト</summary>
            private readonly IRecorder _recorder;

            /// <summary>
            /// 唯一のコンストラクタ
            /// </summary>
            /// <param name="rec"></param>
            internal RecorderBridge(IRecorder rec)
            {
                _recorder = rec;
            }

            /// <summary>
            /// 保存
            /// </summary>
            /// <param name="value"></param>
            public void Save(object value, Id savingObjectID)
            {
                if (_recorder != null)
                {
                    _recorder.RecorderSave(value, savingObjectID);
                }
            }

            /// <summary>
            /// すべて永続化の情報を消す
            /// </summary>
            public void Reset()
            {
                if (_recorder != null)
                {
                    _recorder.RecorderReset();
                }
            }

            /// <summary>
            /// チャンク開始
            /// </summary>
            public void StartChunk(string debugName)
            {
                if (_recorder != null)
                {
                    _recorder.RecorderStartChunk(debugName);
                }
            }

            /// <summary>
            /// チャンクk中かどうか調査する
            /// </summary>
            /// <returns>true = チャンク中</returns>
            public bool IsStartedChunk
            {
                get
                {
                    if (_recorder == null)
                    {
                        return false;
                    }
                    else
                    {
                        return _recorder.RecorderIsChunkStarted;
                    }
                }
            }

            /// <summary>
            /// チャンク終了
            /// </summary>
            public void EndChunk()
            {
                if (_recorder != null)
                {
                    _recorder.RecorderEndChunk();
                }
            }

            /// <summary>
            /// チャンクキャンセル
            /// </summary>
            public void CancelChunk()
            {
                if (_recorder != null)
                {
                    _recorder.RecorderCancelChunk();
                }
            }

        }

        /// <summary>
        /// 指定グループに属するPersisterのChunk階層を指定する（指定階層以降を切り捨てる）
        /// </summary>
        /// <param name="groupID">グループのID</param>
        /// <param name="length">階層</param>
        public void SetChunkLength(NamedId groupID, int length)
        {
            foreach (IRecorder rec in _recorders.Values)
            {
                if (rec is PersistManager.IGroupID)
                {
                    if (((PersistManager.IGroupID)rec).PersisterGroupID == groupID)
                    {
                        rec.RecorderSetChunkLength(length);
                    }
                }
            }
        }

        /// <summary>
        /// レコーダーを取得する
        /// </summary>
        public RecorderBridge this[Id id] => new RecorderBridge((IRecorder)_recorders[id.Value]);
    }
}
