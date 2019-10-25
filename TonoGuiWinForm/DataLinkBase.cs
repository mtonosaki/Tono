// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Collections;
using System.Collections.Generic;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// デフォルトリンク
    /// リンクが不要な場合に使用してください
    /// </summary>
    public sealed class DataLinkDefault : DataLinkBase
    {
        public override void Clear()
        {
        }

        public override void SetEquivalent(RecordBase record, PartsBase parts)
        {
        }

        public override void RemoveEquivalent(PartsBase parts)
        {
        }

        public override ICollection GetRecordset(PartsBase key)
        {
            return Const.ZeroCollection;
        }

        public override ICollection GetPartsset(RecordBase key)
        {
            return Const.ZeroCollection;
        }
    }

    /// <summary>
    /// daLinkBase の概要の説明です。
    /// PartsとAppDataを結びつける情報を管理する基本クラス
    /// </summary>
    public abstract class DataLinkBase
    {
        /// <summary>レコードからパーツに値変換するデリゲート</summary>
        public delegate void RPAdapter(RecordBase fromValue, PartsBase toValue);

        /// <summary>パーツからレコードに値変換するデリゲート</summary>
        public delegate void PRAdapter(PartsBase fromValue, RecordBase toValue);

        #region 属性（シリアライズする）

        private readonly IList<RPAdapter> _adapterRtoPs = new List<RPAdapter>();
        private readonly IList<PRAdapter> _adapterPtoRs = new List<PRAdapter>();

        #endregion


        /// <summary>
        /// レコード→パーツ 値転送デリゲートを指定する
        /// </summary>
        /// <param name="value">デリゲート</param>
        public void SetRPAdapter(RPAdapter value)
        {
            _adapterRtoPs.Add(value);
        }

        /// <summary>
        /// パーツ→レコード　値転送デリゲートを指定する
        /// </summary>
        /// <param name="value"></param>
        public void SetPRAdapter(PRAdapter value)
        {
            _adapterPtoRs.Add(value);
        }

        /// <summary>
        /// 変更されたレコードの値をパーツに反映させる
        /// （このメソッドを行う度にUNDO対象とするかどうかは、IsAutoChunkStateで指定する））
        /// </summary>
        /// <param name="record">新しくなったレコード</param>
        /// <returns>反映後パーツ参照（参考）</returns>
        public ICollection Equalization(RecordBase record)
        {
            System.Diagnostics.Debug.Assert(_adapterRtoPs.Count > 0, "Equalizationするには、まずSetRPAdapterでアダプターを指定する必要があります");
            var partsset = GetPartsset(record);
            if (partsset.Count > 0)
            {
                foreach (PartsBase parts in partsset)
                {
                    foreach (var rpa in _adapterRtoPs)
                    {
                        // レコードの値をパーツに反映
                        rpa(record, parts);
                    }
                }
            }
            return partsset;
        }

        /// <summary>
        /// 変更されたパーツの値をレコードに反映させる
        /// （このメソッドを行う度にUNDO対象とするかどうかは、IsAutoChunkStateで指定する））
        /// </summary>
        /// <param name="parts">新しくなったパーツ</param>
        /// <returns>反映後レコード参照（参考）</returns>
        public ICollection Equalization(PartsBase parts)
        {
            System.Diagnostics.Debug.Assert(_adapterPtoRs.Count >= 0, "Equalizationするには、まずSetPRAdapterでアダプターを指定する必要があります");
            var records = GetRecordset(parts);
            if (records.Count > 0)
            {
                foreach (RecordBase record in records)
                {
                    foreach (var pra in _adapterPtoRs)
                    {
                        // パーツの値をレコードに反映
                        pra(parts, record);
                    }
                }
            }
            return records;
        }

        /// このメソッドは消してください by Tono チェックアウトできなかったので、暫定
        public ICollection Equalization(PartsBase parts, bool dummy)
        {
            return Equalization(parts);
        }

        /// <summary>
        /// リンクをすべてクリアする
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// データのリンク情報を保存する
        /// </summary>
        /// <param name="record">テーブルのレコード</param>
        /// <param name="parts">パーツ</param>
        public abstract void SetEquivalent(RecordBase record, PartsBase parts);

        /// <summary>
        /// パーツを指定してリンク情報を削除する
        /// </summary>
        /// <param name="parts">削除するパーツ</param>
        public abstract void RemoveEquivalent(PartsBase parts);

        /// <summary>
        /// パーツ指定してレコードを取得する（NULLを許可する）
        /// </summary>
        /// <param name="key">パーツ</param>
        /// <returns>レコード</returns>
        public RecordBase GetRecordOrNull(PartsBase key)
        {
            var rs = GetRecordset(key);
            if (rs.Count == 0)
            {
                return null;
            }
            var e = rs.GetEnumerator();
            var ret = e.MoveNext();
            return (RecordBase)e.Current;
        }

        /// <summary>
        /// パーツ指定してレコードを取得する
        /// </summary>
        /// <param name="key">パーツ</param>
        /// <returns>レコード</returns>
        public RecordBase GetRecord(PartsBase key)
        {
            var ret = GetRecordOrNull(key);
            System.Diagnostics.Debug.Assert(ret != null, "リンク情報がありません");
            return ret;
        }

        /// <summary>
        /// パーツ指定してレコードを取得する
        /// </summary>
        /// <param name="key">パーツ</param>
        /// <returns>レコードのコレクション</returns>
        public abstract ICollection GetRecordset(PartsBase key);

        /// <summary>
        /// レコードを指定してパーツを取得する
        /// </summary>
        /// <param name="key">レコード</param>
        /// <returns>パーツ</returns>
        public PartsBase GetParts(RecordBase key)
        {
            var rs = GetPartsset(key);
            if (rs.Count > 0)
            {
                var e = rs.GetEnumerator();
                e.MoveNext();
                return (PartsBase)e.Current;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// レコードを指定してパーツを取得する
        /// </summary>
        /// <param name="key">レコード</param>
        /// <returns>パーツ</returns>
        public abstract ICollection GetPartsset(RecordBase key);
    }
}
