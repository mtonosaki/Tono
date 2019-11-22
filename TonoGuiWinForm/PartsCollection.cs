// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

using Illustios = System.Collections.Generic.List<Tono.GuiWinForm.PartsIllusionProjector>;
using Layers = System.Collections.Generic.Dictionary<int/*レイヤID*/, System.Collections.Generic.List<Tono.GuiWinForm.PartsBase>>;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// daPartsset の概要の説明です。
    /// 複数のパーツを管理するクラス
    /// </summary>
    public class PartsCollection : PartsCollectionBase
    {
        #region 全パーツ列挙用クラス
        /// <summary>
        /// パーツ列挙制御
        /// </summary>
        private class PartsEnumerator : IPartsEnumerator
        {
            private readonly IDictionary _orgData;
            private IDictionaryEnumerator _rpe;
            private IDictionaryEnumerator _lve;
            private IEnumerator _pte;

            public PartsEnumerator(IDictionary value)
            {
                _orgData = value;
                Reset();
            }

            #region IPartsEnumerator メンバ

            public PartsBase Parts => null;

            public IRichPane Pane => null;

            #endregion

            #region IEnumerator メンバ

            public void Reset()
            {
                _rpe = _orgData.GetEnumerator();
                _lve = null;
                _pte = null;
            }

            public object Current
            {
                get
                {
                    if (_pte == null)
                    {
                        return null;
                    }
                    var pe = new PartsEntry
                    {
                        Parts = (PartsBase)_pte.Current,
                        Pane = (IRichPane)_rpe.Key,
                        LayerLevel = (int)_lve.Key
                    };
                    return pe;
                }
            }

            public bool MoveNext()
            {
                if (_pte == null)
                {
                    if (_lve == null)
                    {
                        if (_rpe.MoveNext() == false)
                        {
                            return false;
                        }
                        _lve = ((IDictionary)_rpe.Value).GetEnumerator();
                        return MoveNext();
                    }
                    if (_lve.MoveNext() == false)
                    {
                        _lve = null;
                        return MoveNext();
                    }
                    _pte = ((ICollection)_lve.Value).GetEnumerator();
                    return MoveNext();
                }
                if (_pte.MoveNext())
                {
                    return true;
                }
                _pte = null;
                return MoveNext();
            }

            #endregion
        }

        #endregion
        #region 属性（シリアライズする）

        /// <summary>
        /// データを管理する配列
        /// </summary>
        protected Dictionary<IRichPane, Layers> _data = new Dictionary<IRichPane, Layers>();  /*<IRichPane, ArrayList<dpBase>>*/

        /// <summary>
        /// レイヤー番号ソート用
        /// </summary>
        protected Dictionary<IRichPane, List<int>> _layerNos = new Dictionary<IRichPane, List<int>>();

        /// <summary>
        /// レイヤー表示スイッチ true=表示
        /// </summary>
        protected Dictionary<int, bool> _layerVisibleSwitch = new Dictionary<int, bool>();

        /// <summary>
        /// 使用するイリュージョン
        /// </summary>
        protected Dictionary<IRichPane, Illustios> _projectors = new Dictionary<IRichPane, Illustios>();    /*<IRichPane, ArrayIllustios>*/

        /// <summary>
        /// イリュージョンスクリーンからオリジナルを検索するキー
        /// </summary>
        protected Dictionary<IRichPane, IRichPane> _projectorsRevKey = new Dictionary<IRichPane, IRichPane>();

        #endregion

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public PartsCollection() : base()
        {
        }

        /// <summary>
        /// 初期化コンストラクタ
        /// </summary>
        public PartsCollection(IRichPane pane, ICollection tars) : base()
        {
            foreach (PartsBase pt in tars)
            {
                Add(pane, pt);
            }
        }

        /// <summary>
        /// 初期化コンストラクタ
        /// </summary>
        public PartsCollection(IRichPane pane, ICollection<PartsBase> tars) : base()
        {
            foreach (var pt in tars)
            {
                Add(pane, pt);
            }
        }

        /// <summary>
        /// 表示スイッチ
        /// </summary>
        /// <param name="layerlevel"></param>
        /// <param name="sw"></param>
        public void SetLayerVisible(int layerlevel, bool sw)
        {
            _layerVisibleSwitch[layerlevel] = sw;
        }

        /// <summary>
        /// レイヤーを指定してパーツを取得する
        /// （最初に見つかったペーンのみ）
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public virtual IEnumerable<PartsBase> GetLayerParts(int layer)
        {
            foreach (var layers in _data.Values)
            {
                if (layers.TryGetValue(layer, out var ret))
                {
                    return ret;
                }
            }
            return new List<PartsBase>();
        }

        /// <summary>
        /// レイヤーを変更する
        /// </summary>
        /// <param name="pane"></param>
        /// <param name="layer_from"></param>
        /// <param name="parts"></param>
        /// <param name="layer_to"></param>
        public void MovePartsLayer(IRichPane pane, int layer_from, PartsBase parts, int layer_to)
        {
            var ls = _data[pane];
            if (ls.TryGetValue(layer_from, out var listf) == false)
            {
                listf = new List<PartsBase>();
            }
            if (ls.TryGetValue(layer_to, out var listt) == false)
            {
                listt = makeNewLayer(pane, layer_to, ls);
            }
            listf.Remove(parts);
            listt.Remove(parts);
            listt.Add(parts);
        }

        /// <summary>
        /// レイヤーを指定してパーツを取得する
        /// （最初にみつかったペーンのみ）
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="retPane">指定レイヤーが属するペーンが返る</param>
        /// <returns>パーツコレクション</returns>
        public virtual IList<PartsBase> GetLayerParts(int layer, out IRichPane retPane)
        {

            foreach (var de in _data)
            {
                var layers = de.Value;
                if (layers.TryGetValue(layer, out var ret))
                {
                    retPane = de.Key;
                    return ret;
                }
            }
            retPane = null;
            return new List<PartsBase>();
        }

        /// <summary>
        /// レイヤーを指定してパーツを取得する
        /// </summary>
        /// <param name="layer">指定レイヤー</param>
        /// <param name="pane">指定ペーン</param>
        /// <returns></returns>
        public virtual IList<PartsBase> GetLayerParts(int layer, IRichPane pane)
        {
            var layers = _data[pane];
            if (layers.TryGetValue(layer, out var ret))
            {
                return ret;
            }
            return new List<PartsBase>();
        }

        /// <summary>
        /// 指定ID（通常はuRowKey）を指定して、dpBase.LT.Y == pos.ID の行をすべてを戻す
        /// </summary>
        /// <param name="pos">検索キー</param>
        /// <returns>指定キーに合致するパーツ群</returns>
        public override IList<PartsBase> GetPartsByLocationID(Id pos)
        {
            var ret = new List<PartsBase>();

            foreach (var layers in _data.Values)
            {
                foreach (var list in layers.Values)
                {
                    foreach (var dp in list)
                    {
                        if (dp.Rect.LT.Y == pos.Value)
                        {
                            ret.Add(dp);
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// ペーンを指定して、指定ID（通常はuRowKey）を指定して、
        /// dpBase.LT.Y == pos.ID の行の指定ペーンの描画領域内にあるパーツを戻す
        /// </summary>
        /// <param name="rp">描画対象ペーン</param>
        /// <param name="pos">検索キー</param>
        /// <returns>指定キーに合致するパーツ群</returns>
        public override IList<PartsBase> GetPartsByLocationID(IRichPane rp, Id pos)
        {
            // HACK: Slow GetPartsByLocationID method
            var ret = new List<PartsBase>();

            foreach (var layers in _data.Values)
            {
                foreach (var list in layers.Values)
                {
                    foreach (var dp in list)
                    {
                        if (dp.Rect.LT.Y == pos.Value)
                        {
                            ret.Add(dp);
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// 指定パーツと重なっているパーツをすべて取得する
        /// </summary>
        /// <param name="partsClass">パーツのクラスタイプ typeof(object)で全て</param>
        /// <param name="tar">取得対象</param>
        /// <param name="rp">ペーン</param>
        /// <param name="checkIllustion"></param>
        /// <returns>パーツのコレクション</returns>
        public override ICollection<PartsBase> GetOverlappedParts(Type partsClass, PartsBase tar, IRichPane rp, bool checkIllustion)
        {
            //HACK: Slow GetOverlappedParts method
            var ret = new List<PartsBase>();

            if (_data.TryGetValue(rp, out var layers))
            {
                var lnos = _layerNos[rp];
                for (var lnosid = 0; lnosid < lnos.Count; lnosid++)
                {
                    if (_layerVisibleSwitch[lnos[lnosid]])
                    {
                        var list = layers[lnos[lnosid]];
                        foreach (var dp in list)
                        {
                            var t = dp.GetType();
                            if (dp != tar && (t.Equals(partsClass) || t.IsSubclassOf(partsClass)))
                            {
                                if (IsOverlapped(rp, tar, rp, dp, checkIllustion))
                                {
                                    ret.Add(dp);
                                }
                            }
                        }
                    }
                }
            }
            return ret;
        }


        /// <summary>
        /// イリュージョンプロジェクタを登録する
        /// </summary>
        /// <param name="original">投影元</param>
        /// <param name="idtext"></param>
        /// <returns></returns>
        public PartsIllusionProjector AddIllusionProjector(IRichPane original, string idtext)
        {
            var ret = new PartsIllusionProjector(original, idtext);
            if (_projectors.TryGetValue(original, out var prs) == false)
            {
                _projectors.Add(original, prs = new Illustios());
            }
            prs.Add(ret);
            _projectorsRevKey.Add(ret.ScreenPane, original);
            return ret;
        }

        /// <summary>
        /// 指定したパーツをペーン内、レイヤーないで最上に移動する
        /// </summary>
        /// <param name="tar">移動させるパーツ</param>
        public virtual void MovePartsZOrderToTop(PartsBase tar)
        {
            foreach (var layers in _data.Values)
            {
                foreach (var list in layers.Values)
                {
                    if (list.Contains(tar))
                    {
                        list.Remove(tar);
                        list.Insert(list.Count, tar);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 追加処理
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        /// <param name="layerLevel"></param>
        private void _addProc(IRichPane target, PartsBase value, int layerLevel)
        {
            try
            {

                if (!_data.TryGetValue(target, out var layers))
                {
                    // ペーンを登録
                    _data.Add(target, new Layers());    // TONO
                    _addProc(target, value, layerLevel);
                    return;
                }
                if (!layers.TryGetValue(layerLevel, out var ps))
                {
                    // レイヤーを登録
                    makeNewLayer(target, layerLevel, layers);
                    _addProc(target, value, layerLevel);
                    return;
                }
                ps.Add(value);
            }
            catch (System.NullReferenceException)
            {
                _data[target] = new Layers();
                _addProc(target, value, layerLevel);
            }
        }

        /// <summary>
        /// 新しいレイヤーを作る
        /// </summary>
        /// <param name="target"></param>
        /// <param name="layerLevel"></param>
        /// <param name="layers"></param>
        /// <returns></returns>
        private List<PartsBase> makeNewLayer(IRichPane target, int layerLevel, Layers layers)
        {
            List<PartsBase> ret;
            layers[layerLevel] = ret = new List<PartsBase>();
            _layerVisibleSwitch[layerLevel] = true;

            if (_layerNos.TryGetValue(target, out var lnos) == false)
            {
                lnos = _layerNos[target] = new List<int>();

            }
            if (lnos.Contains(layerLevel) == false)
            {
                lnos.Add(layerLevel);
                lnos.Sort();
            }
            return ret;
        }

        /// <summary>
        /// パーツを追加する
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value">追加するパーツ</param>
        /// <param name="layerLevel"></param>
        public override void Add(IRichPane target, PartsBase value, int layerLevel)
        {
            lock (_data)
            {
                base.Add(target, value, layerLevel);    // 必須

                _addProc(target, value, layerLevel);
            }
        }

        /// <summary>
        /// 指定ペーンのEnableなプロジェクタリストを取得する
        /// この関数は、RichPaneBinderを指定した際に、オリジナルを返す工夫を行う
        /// これにより、_projectors[ペーン]が可能となる
        /// </summary>
        /// <param name="tar">リストを取得するためのキーとなるペーン</param>
        /// <param name="isAll">true=Enabled=falseも対象に含める2011.3.8</param>
        /// <returns>プロジェクタリスト</returns>
        protected Illustios getProjectors(IRichPane tar, bool isAll)
        {
            if (tar is RichPaneBinder)
            {
                // 値となるペーンからリストを取得
                if (_projectorsRevKey.TryGetValue(tar, out var key))
                {
                    if (_projectors.TryGetValue(key, out var ret))
                    {
                        var cpy = new Illustios();
                        foreach (var ip in ret)
                        {
                            if (ip.Enabled || isAll)
                            {
                                cpy.Add(ip);
                            }
                        }
                        return cpy;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            // キーとなるペーンからリストを取得
            if (_projectors.TryGetValue(tar, out var ret2))
            {
                var cpy = new Illustios();
                foreach (var ip in ret2)
                {
                    if (ip.Enabled || isAll)
                    {
                        cpy.Add(ip);
                    }
                }
                return cpy;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 指定した二つのパーツの重なり判定をする
        /// </summary>
        /// <param name="p1">パーツ１</param>
        /// <param name="p2">パーツ２</param>
        /// <param name="isIllusionCheck">true = イリュージョンを考慮する</param>
        /// <returns>true = 重なっている / false = 重なっていない</returns>
        public override bool IsOverlapped(IRichPane pane1, PartsBase parts1, IRichPane pane2, PartsBase parts2, bool isIllusionCheck)
        {
            try
            {
                if (isIllusionCheck)
                {
                    foreach (IRichPane pp1 in PartsIllusionProjector.GetEnumerator(pane1, getProjectors(pane1, false), parts1))
                    {
                        var sr1 = parts1.GetScRect(pp1, parts1.Rect);
                        foreach (IRichPane pp2 in PartsIllusionProjector.GetEnumerator(pane2, getProjectors(pane2, false), parts2))
                        {
                            var sr2 = parts2.GetScRect(pp2, parts2.Rect);
                            var union = sr1 & sr2;
                            if (union != null)
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    if (pane1 == pane2)
                    {
                        var pt1 = parts1.GetPtRect(parts1.Rect);
                        var pt2 = parts2.GetPtRect(parts2.Rect);
                        return pt1.IsIn(pt2);
                    }
                    else
                    {
                        var sr1 = parts1.GetScRect(pane1, parts1.Rect);
                        var sr2 = parts2.GetScRect(pane2, parts2.Rect);
                        return sr1.IsIn(sr2);
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("IsOverlappedは次の例外でキャンセル; " + e.Message);
            }
            return false;
        }

        /// <summary>
        /// 描画させる（Paintイベントから実行されるため、ユーザーは実行禁止
        /// </summary>
        public override void ProvideDrawFunction()
        {
            try
            {
                for (IDictionaryEnumerator de = _data.GetEnumerator(); de.MoveNext();)
                {
                    var pane = (IRichPane)de.Key;
                    pane.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    // 全体に影響する描画処理
                    var clipRect = pane.GetPaintClipRect() & pane.GetPaneRect();
                    if (clipRect != null)   // クリップとペイントの領域内のみ描画する
                    {
                        PartsBase.Mask(pane); // ペーン領域をマスクする
                                              //System.Diagnostics.Debug.WriteLine(clipRect.ToString());
                        using (Brush brush = new SolidBrush(pane.Control.BackColor))
                        {
                            pane.Graphics.FillRectangle(brush, clipRect); // 背景を描画
                        }

                        // レイヤーでループする
                        var layers = (Layers)de.Value;
                        var lnos = _layerNos[pane];
                        for (var layerid = 0; layerid < lnos.Count; layerid++)
                        {
                            if (_layerVisibleSwitch[lnos[layerid]])
                            {
                                IEnumerable<PartsBase> pts = layers[lnos[layerid]];
                                // 描画する
                                drawLayer(pane, lnos[layerid], pts);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("描画中に例外；" + e.Message);
            }
        }

        /// <summary>
        /// レイヤーを描画
        /// </summary>
        /// <param name="pane">描画するペーン</param>
        /// <param name="pts">描画するパーツ</param>
        protected virtual void drawLayer(IRichPane pane, int layerid, IEnumerable<PartsBase> pts)
        {
            foreach (var dp in pts)
            {
                foreach (IRichPane pp in PartsIllusionProjector.GetEnumerator(pane, getProjectors(pane, false), dp))
                {
#if DEBUG
                    try
                    {
                        dp.Draw(pp);
                    }
                    catch (Exception ex)
                    {
                        LOG.WriteLineException(ex);
                        //throw exinner;	// ここにブレークポイントを設けると、どのパーツで例外発生したか特定できる
                    }
#else
                    dp.Draw(pp);
#endif
                }
            }
        }

        /// <summary>
        /// 指定したパーツコレクションから、該当するパーツを検索する
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="pane"></param>
        /// <param name="isSelectableOnly"></param>
        /// <param name="pcol"></param>
        /// <param name="rp">ペーンを返す（イリュージョンプロジェクタのペーンが返るので、paneと異なる場合がある）</param>
        /// <returns></returns>
        private PartsBase getparts(ScreenPos pos, IRichPane pane, bool isSelectableOnly, IList pcol, out IRichPane rp)
        {
            for (var pidx = pcol.Count - 1; pidx >= 0; pidx--)
            {
                var dp = (PartsBase)pcol[pidx];

                if (dp is IPartsVisible)
                {
                    if (((IPartsVisible)dp).Visible == false)
                    {
                        continue;
                    }
                }
                if (isSelectableOnly)
                {
                    if (dp is IPartsSelectable == false)
                    {
                        continue;
                    }
                }
                // プロジェクタを通して、パーツ座標を調査する
                foreach (IRichPane pp in PartsIllusionProjector.GetEnumerator(pane, getProjectors(pane, false), dp))
                {
                    if (dp.IsOn(pos, pp) != PartsBase.PointType.Outside)
                    {
                        rp = pp;
                        return dp;
                    }
                }
            }
            rp = null;
            return null;
        }

        /// <summary>
        /// 指定領域から特定位置のパーツを探す
        /// </summary>
        /// <param name="pos">位置</param>
        /// <param name="pane">検索ペーン</param>
        /// <param name="layer">検索レイヤー</param>
        /// <param name="isSelectableOnly">選択可能のみ</param>
        /// <returns>取得できたパーツ / null=なし</returns>
        public override PartsBase GetPartsAt(ScreenPos pos, IRichPane pane, int layer, bool isSelectableOnly)
        {
            PartsBase ret = null;
            if (_data.TryGetValue(pane, out var layers))
            {
                if (layers.TryGetValue(layer, out var pcol))
                {
                    ret = getparts(pos, pane, isSelectableOnly, pcol, out var rp);
                }
            }
            return ret;
        }

        /// <summary>
        /// 指定マウス座標のパーツをひとつ取得する
        /// </summary>
        /// <param name="pos">パーツ座標</param>
        /// <returns>取得できたパーツ / null=なし</returns>
        public override PartsBase GetPartsAt(ScreenPos pos, bool isSelectableOnly, out IRichPane rp)
        {
            rp = null;
            if (isInSkipzone(pos))
            {
                return null;
            }

            // リッチペーンによるループ
            for (IDictionaryEnumerator de = _data.GetEnumerator(); de.MoveNext();)
            {
                var pane = (IRichPane)de.Key;

                if (pane.GetPaneRect().IsIn(pos) == false)
                {
                    continue;
                }

                // レイヤーによるループ
                for (var layerde = ((IDictionary)de.Value).GetEnumerator(); layerde.MoveNext();)
                {
                    if (_layerVisibleSwitch[(int)layerde.Key])
                    {
                        // すべてのパーツを順に調査する、低速の処理。気に入らなければ機能をオーバーライドしてください
                        // ただし、以下のプロジェクタを考慮した作りにする必要があります。
                        var pcol = (IList)layerde.Value;
                        var ret = getparts(pos, pane, isSelectableOnly, pcol, out rp);
                        if (ret != null)
                        {
                            return ret;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// パーツを列挙するためのIEnumeratorを取得する
        /// </summary>
        /// <returns>IEnumerator</returns>
        public override IPartsEnumerator GetEnumerator()
        {
            return new PartsEnumerator(_data);
        }

        /// <summary>
        /// 全ペーンに登録されているパーツの数を合計する
        /// </summary>
        public override int Count
        {
            get
            {
                var n = 0;
                foreach (var layers in _data.Values)
                {
                    foreach (IList<PartsBase> lde in layers.Values)
                    {
                        n += lde.Count;
                    }
                }
                return n;
            }
        }

        /// <summary>
        /// 指定パーツを削除する
        /// </summary>
        /// <param name="value"></param>
        public override void Remove(PartsBase value)
        {
            base.Remove(value); // 必須

            foreach (var layers in _data.Values)
            {
                foreach (var co in layers.Values)
                {
                    if (co.Contains(value))
                    {
                        co.Remove(value);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// すべてを削除する
        /// </summary>
        public override void Clear()
        {
            lock (_data)
            {
                foreach (var de in _data.Keys)
                {
                    de.Invalidate(de.GetPaneRect());    // パーツの領域を再描画する
                }
                _data.Clear();
                _layerVisibleSwitch.Clear();
                _layerNos.Clear();
            }
        }

        /// <summary>
        /// 指定した型を削除する
        /// </summary>
        /// <param name="type"></param>
        public override int Clear(Type type)
        {
            lock (_data)
            {
                var dels = new List<PartsBase>();
                foreach (var kv in _data)
                {
                    var rp = kv.Key;
                    var layers = kv.Value;
                    foreach (var pts in layers.Values)
                    {
                        foreach (var p in pts)
                        {
                            if (p.GetType().IsSubclassOf(type) || p.GetType() == type)
                            {
                                dels.Add(p);
                            }
                        }
                    }
                }
                foreach (var p in dels)
                {
                    Remove(p);
                }
                return dels.Count;
            }
        }

        /// <summary>
        /// 指定ペーンのパーツを削除する
        /// </summary>
        public override void Clear(IRichPane targetPane)
        {
            lock (_data)
            {
                if (_data.TryGetValue(targetPane, out var ls))
                {
                    ls.Clear();
                    targetPane.Invalidate(targetPane.GetPaneRect());    // 削除した様子を再描画
                }
            }
        }

        /// <summary>
        /// 指定ペーンで指定レイヤーのパーツを削除する
        /// </summary>
        public override void Clear(IRichPane targetPane, int layerLevel)
        {
            lock (_data)
            {
                if (_data.TryGetValue(targetPane, out var ls))
                {
                    if (ls.TryGetValue(layerLevel, out var ps))
                    {
                        ps.Clear();
                        targetPane.Invalidate(targetPane.GetPaneRect());    // 削除した様子を再描画
                    }
                }
            }
        }

        /// <summary>
        /// 全要素をコピーする（コピー先はコピー元と全く同じになる）
        /// 各要素（パーツ）はCloneされないで参照となる
        /// </summary>
        public override object Clone()
        {
            lock (_data)
            {
                var ret = new PartsCollection();
                copyBasePropertyTo(ret);
                foreach (PartsEntry pe in this)
                {
                    ret.Add(pe);
                }
                return ret;
            }
        }

        /// <summary>
        /// 領域更新を予約する（プロジェクタをサポートしているので、これを使用してください）
        /// </summary>
        /// <param name="parts">更新するパーツ</param>
        /// <param name="rp">使用するペーン</param>
        public override void Invalidate(PartsBase parts, IRichPane rp)
        {
            try
            {
                foreach (IRichPane pp in PartsIllusionProjector.GetEnumerator(rp, getProjectors(rp, false), parts))
                {
                    base.Invalidate(parts, pp);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 指定ペーンにある指定型のパーツをひとつ取得する
        /// </summary>
        /// <param name="rp">ペーン</param>
        /// <param name="dpType">型</param>
        /// <returns>パーツのインスタンスの参照 / null = 見つからなかった</returns>
        public override PartsBase GetSample(IRichPane rp, Type dpType)
        {
            lock (_data)
            {
                if (_data.TryGetValue(rp, out var layers))
                {
                    foreach (var pts in layers.Values)
                    {
                        foreach (var p in pts)
                        {
                            if (p.GetType().IsSubclassOf(dpType) || p.GetType() == dpType)
                            {
                                return p;
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 指定ペーンにある指定型のパーツをひとつ取得する
        /// </summary>
        /// <param name="rp">ペーン</param>
        /// <param name="dpType">型</param>
        /// <returns>パーツのインスタンスの参照 / null = 見つからなかった</returns>
        public override PartsBase GetSample()
        {
            lock (_data)
            {
                foreach (var layers in _data.Values)
                {
                    foreach (var pts in layers.Values)
                    {
                        foreach (var p in pts)
                        {
                            return p;
                        }
                    }
                }
                return null;
            }
        }
    }
}
