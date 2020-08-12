// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{

    /// <summary>
    /// daPartsBase の概要の説明です。
    /// データ管理の基本クラス
    /// </summary>
    public abstract class PartsCollectionBase : ICloneable
    {
        #region 列挙用クラス
        /// <summary>
        /// パーツを列挙する際のCurrentデータになる型
        /// </summary>
        public struct PartsEntry
        {
            /// <summary>パーツ</summary>
            public PartsBase Parts;

            /// <summary>そのパーツが登録されているペーン</summary>
            public IRichPane Pane;

            /// <summary>そのパーツが属するレイヤーレベル</summary>
            public int LayerLevel;

            /// <summary>
            /// 初期化コンストラクタ
            /// </summary>
            /// <param name="parts">パーツ</param>
            /// <param name="pane">ペーン</param>
            /// <param name="layerLevel">レベル値</param>
            public PartsEntry(PartsBase parts, IRichPane pane, int layerLevel)
            {
                Parts = parts;
                Pane = pane;
                LayerLevel = layerLevel;
            }

            public override bool Equals(object obj)
            {
                if (obj is PartsEntry pe)
                {
                    return Parts.Equals(pe.Parts) && Pane == pe.Pane && LayerLevel == pe.LayerLevel;
                }
                else
                {
                    return false;
                }
            }
            public static bool operator ==(PartsEntry a, PartsEntry b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(PartsEntry a, PartsEntry b)
            {
                return !a.Equals(b);
            }

            public override int GetHashCode()
            {
                return Parts.GetHashCode();
            }

            #region Data tips for debugging
#if DEBUG
            /// <summary>
            /// 
            /// </summary>
            public string _
            {
                get
                {
                    var s = "";
                    if (Parts != null)
                    {
                        s = "Parts=" + s.ToString() + "  ";
                    }
                    if (Pane != null)
                    {
                        s += "Pane = " + Pane.IdText;
                    }
                    return s;
                }
            }
#endif
            #endregion
        }

        /// <summary>
        /// パーツ列挙用IEnumerator
        /// </summary>
        public interface IPartsEnumerator : IEnumerator
        {
            /// <summary>パーツ</summary>
            PartsBase Parts { get; }

            /// <summary>そのパーツが登録されているペーン</summary>
            IRichPane Pane { get; }
        }
        #endregion

        #region	属性(シリアライズする)
        /// <summary>
        /// クリック不感地帯
        /// </summary>
        private List<ScreenRect> _skipzones = new List<ScreenRect>();
        #endregion

        #region 属性（シリアライズしない）
        /// <summary>フィーチャーサイクル内で削除されたパーツの一覧</summary>
        [NonSerialized]
        private IList _removedParts;
        /// <summary>フィーチャーサイクル内で追加されたパーツの一覧</summary>
        [NonSerialized]
        private IList _addedParts;
        #endregion

        /// <summary>
        /// Cloneなどで用いる
        /// </summary>
        /// <param name="dst"></param>
        protected void copyBasePropertyTo(PartsCollectionBase dst)
        {
            if (_removedParts is ListDummy)
            {
                dst._removedParts = new ListDummy();
                dst._addedParts = new ListDummy();
            }
            else
            {
                dst._removedParts = new ArrayList(_removedParts);
                dst._addedParts = new ArrayList(_addedParts);
            }
            dst._skipzones = new List<ScreenRect>(_skipzones);
        }

        protected PartsCollectionBase()
        {
            _removedParts = new ArrayList();
            _addedParts = new ArrayList();
        }

        /// <summary>
        /// 登録・削除のイベント処理用の操作をしない軽量設定
        /// </summary>
        public void SetTemporaryMode()
        {
            _removedParts = new ListDummy();
            _addedParts = new ListDummy();
        }

        /// <summary>
        /// 描画処理を行わせる（Paintイベントからコールされるので、ユーザーは実行禁止
        /// </summary>
        public virtual void ProvideDrawFunction()
        {
        }

        /// <summary>
        /// 不感地帯
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public void AddSkipZone(ScreenRect zone)
        {
            _skipzones.Add(zone);
        }

        /// <summary>
        /// 指定位置がスキップゾーンかどうかを調べる
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        protected bool isInSkipzone(ScreenPos pos)
        {
            foreach (var r in _skipzones)
            {
                if (r.IsIn(pos))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 指定した二つのパーツの重なり判定をする
        /// </summary>
        /// <param name="pane1">パーツ1位置を計算するペーン</param>
        /// <param name="parts1">パーツ1</param>
        /// <param name="pane2">パーツ2位置を計算するペーン</param>
        /// <param name="parts2">パーツ2</param>
        /// <param name="isIllusionCheck">true = イリュージョンを考慮する</param>
        /// <returns>true = 重なっている / false = 重なっていない</returns>
        public virtual bool IsOverlapped(IRichPane pane1, PartsBase parts1, IRichPane pane2, PartsBase parts2, bool isIllusionCheck)
        {
            return false;
        }

        private static Mes _prevMessage = null;

        /// <summary>
        /// uMesの状態の変化を検出したら、全パーツのTextFormatによるTextを更新する
        /// </summary>
        public virtual void CheckAndResetLocalized()
        {
            if (object.ReferenceEquals(_prevMessage, Mes.Current) == false)
            {
                _prevMessage = Mes.Current;
                foreach (PartsEntry pe in this)
                {
                    pe.Parts.ResetTextByFormat();
                }
            }
        }

        /// <summary>
        /// 指定ID（通常はuRowKey）を指定して、dpBase.LT.Y == pos.ID の行をすべてを戻す
        /// </summary>
        /// <param name="pos">検索キー</param>
        /// <returns>指定キーに合致するパーツ群</returns>
        public virtual IList<PartsBase> GetPartsByLocationID(Id pos)
        {
            return null;
        }

        /// <summary>
        /// ペーンを指定して、指定ID（通常はuRowKey）を指定して、
        /// dpBase.LT.Y == pos.ID の行の指定ペーンの描画領域内にあるパーツを戻す
        /// </summary>
        /// <param name="rp">描画するペーン</param>
        /// <param name="pos">検索キー</param>
        /// <returns>指定キーに合致するパーツ群</returns>
        public virtual IList<PartsBase> GetPartsByLocationID(IRichPane rp, Id pos)
        {
            return null;
        }


        /// <summary>
        /// 指定ペーンにある指定型のパーツをひとつ取得する
        /// </summary>
        /// <param name="rp">ペーン</param>
        /// <param name="dpType">型</param>
        /// <returns>パーツのインスタンスの参照 / null = 見つからなかった</returns>
        public abstract PartsBase GetSample(IRichPane rp, Type dpType);

        /// <summary>
        /// 指定ペーンにある指定型のパーツをひとつ取得する
        /// </summary>
        /// <returns>パーツのインスタンスの参照 / null = 見つからなかった</returns>
        public abstract PartsBase GetSample();

        /// <summary>
        /// パーツを追加する（最下レイヤーに追加される）
        /// </summary>
        /// <param name="target">主ペーン</param>
        /// <param name="value">追加するパーツ</param>
        public void Add(IRichPane target, PartsBase value)
        {
            Add(target, value, 0);
        }
        /// <summary>
        /// パーツを追加する
        /// </summary>
        /// <param name="target">主ペーン</param>
        /// <param name="value">追加するパーツ</param>
        /// <param name="layerLevel">パーツのレイヤー（０が最下）</param>
        public virtual void Add(IRichPane target, PartsBase value, int layerLevel)
        {
            _addedParts.Add(new PartsCollectionBase.PartsEntry(value, target, layerLevel));
        }

        /// <summary>
        /// パーツを追加する
        /// </summary>
        /// <param name="value">追加するパーツ</param>
        public void Add(PartsEntry value)
        {
            Add(value.Pane, value.Parts, value.LayerLevel);
        }

        /// <summary>
        /// 指定IDのパーツを取得する
        /// </summary>
        /// <param name="partsID"></param>
        /// <returns>null = パーツが見つからなかった</returns>
        public virtual PartsBase GetParts(Id partsID)
        {
            foreach (PartsEntry pe in this)
            {
                if (pe.Parts.ID == partsID)
                {
                    return pe.Parts;
                }
            }
            return null;
        }

        /// <summary>
        /// 指定IDのパーツが存在するか調べる
        /// （全パーツスキャン）
        /// </summary>
        /// <param name="partsID"></param>
        /// <returns></returns>
        public bool Contains(Id partsID)
        {
            return GetParts(partsID) == null ? false : true;
        }

        /// <summary>
        /// 指定パーツを再描画要求する
        /// </summary>
        /// <param name="parts">再描画するパーツの領域</param>
        /// <param name="rp">指定パーツに属すリッチペーン</param>
        public virtual void Invalidate(PartsBase parts, IRichPane rp)
        {
            var r = ((ScreenRect)parts.GetScRect(rp, parts.Rect).GetPpSize()) & rp.GetPaneRect();
            rp.Invalidate(r);
        }

        /// <summary>
        /// すべての登録を削除する
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// すべての登録を削除する
        /// </summary>
        public abstract void Clear(IRichPane targetPane);

        /// <summary>
        /// すべての登録を削除する
        /// </summary>
        public abstract void Clear(IRichPane targetPane, int layerLevel);

        /// <summary>
        /// 指定した型のパーツをすべて削除する（実装しないとNotSupportException）
        /// </summary>
        /// <param name="type"></param>
        /// <returns>消した数</returns>
        public virtual int Clear(Type type)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 指定マウス座標のパーツをひとつ取得する
        /// </summary>
        /// <param name="pos">座標</param>
        /// <param name="isSelectableOnly"></param>
        /// <returns>取得できたパーツ / null=なし</returns>
        public PartsBase GetPartsAt(ScreenPos pos, bool isSelectableOnly)
        {
            return GetPartsAt(pos, isSelectableOnly, out var rp);
        }

        /// <summary>
        /// 指定マウス座標のパーツをひとつ取得する
        /// </summary>
        /// <param name="pos">座標</param>
        /// <param name="rp">パーツが属するペーン</param>
        /// <param name="isSelectableOnly"></param>
        /// <returns>取得できたパーツ / null=なし</returns>
        public virtual PartsBase GetPartsAt(ScreenPos pos, bool isSelectableOnly, out IRichPane rp)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 指定領域内のパーツを検索する
        /// </summary>
        /// <param name="pos">座標</param>
        /// <param name="rp">検索ペーン</param>
        /// <param name="layer">検索レイヤー</param>
        /// <param name="isSelectableOnly">選択可能なパーツのみ検索</param>
        /// <returns>取得できたパーツ / null=なし</returns>
        public virtual PartsBase GetPartsAt(ScreenPos pos, IRichPane rp, int layer, bool isSelectableOnly)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 指定パーツと重なっているパーツをすべて取得する
        /// </summary>
        /// <param name="partsClass">パーツのクラスタイプ typeof(object)で全て</param>
        /// <param name="tar">取得対象</param>
        /// <param name="rp">ペーン</param>
        /// <param name="checkIllustion"></param>
        /// <returns>パーツのコレクション</returns>
        public virtual ICollection<PartsBase> GetOverlappedParts(Type partsClass, PartsBase tar, IRichPane rp, bool checkIllustion)
        {
            return new List<PartsBase>();
        }

        /// <summary>
        /// 指定パーツを削除する
        /// </summary>
        /// <param name="value"></param>
        public virtual void Remove(PartsBase value)
        {
            _removedParts.Add(value);
        }

        #region IEnumerable メンバ

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual IPartsEnumerator GetEnumerator()
        {
            return null;
        }

        #endregion

        /// <summary>
        /// 登録されているパーツ件数を返す
        /// </summary>
        public abstract int Count
        {
            get;
        }
        #region ICloneable メンバ

        /// <summary>
        /// パーツセットのクローン（各要素の中身はクローンされない）
        /// </summary>
        /// <returns></returns>
        public abstract object Clone();

        #endregion
    }
}
