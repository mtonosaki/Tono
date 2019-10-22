// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// フィーチャー駆動の基本コントロール
    /// 各種イベントをフィーチャークラスに転送できるHMI
    /// </summary>
    public partial class TGuiView : ContainerControl, IDisposable, IRichPane, IRichPaneSync, IKeyListener, IControlUI
    {
        #region 属性（シリアライズする）

        /// <summary>ルートフィーチャーグループ</summary>
        private FeatureGroupRoot _rootGroup = null;

        /// <summary>ズーム倍率 * 1000[%]</summary>
        private XyBase _zoom = XyBase.FromInt(1000, 1000);

        /// <summary>スクロール量</summary>
        private ScreenPos _scroll = ScreenPos.FromInt(0, 0);

        /// <summary>パーツが登録されていないリッチペーンの背景を塗りつぶすかどうか。trueにするとチラツキの原因になるかも</summary>
        private bool _isDrawEmptyBackground = true;


        #endregion
        #region 属性（シリアライズしない）

        /// <summary>Paintイベント最新のGraphicオブジェクト</summary>
        private Graphics _currentGraphics;
        /// <summary>Paintイベント最新のペイント領域</summary>
        private ScreenRect _currentPaintClip;
        /// <summary>直前のuMouseState.Buttonsを記憶する</summary>
        private readonly MouseState.Buttons _mouseStateButtons = new MouseState.Buttons(false, false, false, false, false);
        /// <summary>ドラッグ＆ドロップの情報</summary>
        private DragState _DragState = null;

        /// <summary>マウスの軌跡等を描画するためのビットマップレイヤー</summary>
        private readonly ArrayList _freeLayers = new ArrayList();

        /// <summary>キーEnablerを覚えておく</summary>
        private Tono.GuiWinForm.TKeyEnabler _KeyEnabler = null;

        /// <summary>描画中フラグ</summary>
        private bool _isDrawing = false;
        #endregion

        /// <summary>
        /// 構築用コード
        /// </summary>
        private void _constract()
        {
            // フィーチャーアーキテクチャ
            _rootGroup = new FeatureGroupRoot(this);

            // この呼び出しは、Windows.Forms フォーム デザイナで必要です。
            InitializeComponent();
            var doubleBuffer = ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer;
            var hmi = ControlStyles.UserMouse;
            var etc = ControlStyles.Selectable | ControlStyles.Opaque | ControlStyles.ResizeRedraw;
            SetStyle(doubleBuffer | hmi | etc, true);

            AllowDrop = true;  // コントロールがドラッグ＆ドロップを受け付けるようにする
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public TGuiView()
        {
            _constract();
        }

        /// <summary>
        /// コンテナを返す
        /// </summary>
        /// <returns></returns>
        public IContainer GetContainer()
        {
            return components;
        }

        /// <summary>
        /// コンテナ指定のコンストラクタ
        /// </summary>
        /// <param name="container"></param>
        public TGuiView(IContainer container)
        {
            container.Add(this);
            _constract();
        }

        #region IDisposable メンバ

        void IDisposable.Dispose()
        {
            if (_rootGroup != null)
            {
                _rootGroup.Dispose();
                _rootGroup = null;
            }
        }

        #endregion

        /// <summary>
        /// 処理中のGraphicsインスタンスを取得する
        /// </summary>
        /// <returns></returns>

        public Graphics GetCurrentGraphics()
        {
            return _currentGraphics;
        }

        /// <summary>
        /// フリーレイヤーを並び替える比較クラス
        /// </summary>
        private class FreeLayerComparer : IComparer
        {
            #region IComparer メンバ

            public int Compare(object x, object y)
            {
                return ((FreeDrawLayer)x).Level - ((FreeDrawLayer)y).Level;
            }

            #endregion
        }

        /// <summary>
        /// 自分が使いたいフリーレイヤーを登録する
        /// </summary>
        /// <param name="layerLevel">レイヤー番号。小さいほど下側</param>
        /// <returns>Graphicsオブジェクト</returns>
        public FreeDrawLayer AddFreeLayer(int layerLevel)
        {
            var fl = new FreeDrawLayer(this, layerLevel);
            _freeLayers.Add(fl);
            _freeLayers.Sort(new FreeLayerComparer());
            return fl;
        }

        /// <summary>
        /// 文字列生成
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{{FeatureRich={0}}}", IdText);
        }

        /// <summary>
        /// フォームのOnLoadでまず最初にこのメソッドをコールしなければならない
        /// 言語設定より前にコールすること
        /// </summary>
        /// <returns>ルートフィーチャーグループの参照</returns>
        // 
        public void Initialize(Type featureLoader)
        {
            if (DesignMode == false)
            {
                if (featureLoader != null)
                {
                    _rootGroup.Initialize(featureLoader);
                }
                foreach (var o in Control.Controls)
                {
                    if (o is TKeyEnabler == false)
                    {
                        continue;
                    }

                    _KeyEnabler = (TKeyEnabler)o;
                    break;
                }
            }
            _rootGroup.SetUrgentToken(TokenGeneral.TokenAllFeatureLoaded, TokenGeneral.TokenAllFeatureLoaded, null);
        }

        /// <summary>
        /// ファイル名を指定してフォームのOnLoadで
        /// まず最初にこのメソッドをコールしなければならない
        /// 言語設定より前にコールすること
        /// </summary>
        // 
        public void Initialize(Type featureLoader, string file)
        {
            if (DesignMode == false)
            {
                if (featureLoader != null)
                {
                    _rootGroup.Initialize(featureLoader, file);
                }
                Initialize(null);
            }
        }

        /// <summary>
        /// ルートグループを取得する
        /// </summary>
        /// <returns>関連づけられているルートグループ</returns>
        public FeatureGroupRoot GetFeatureRoot()
        {
            return _rootGroup;
        }

        /// <summary>
        /// パーツが無くても背景を描画するかどうかのフラグ
        /// これは、falseにしておくとパフォーマンスが向上するが、パーツが配置されないペーンは真っ黒になる
        /// trueにしておくと、背景色でペイントされる処理が追加される
        /// </summary>
        [Category("Tono.GuiWinForm")]
        [Description("パーツが無くても背景を描画するかどうかのフラグ")]
        public bool IsDrawEmptyBackground
        {

            get => _isDrawEmptyBackground;

            set => _isDrawEmptyBackground = value;
        }

        /// <summary>
        /// マウス移動イベントの処理
        /// </summary>
        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            if (_cannotEventProc())
            {
                return;
            }

            // クリックしたペーンを捜す
            IRichPane pane = this;
            foreach (Control c in Controls)
            {
                if (c is IRichPane)
                {
                    if (ScreenRect.FromControl(c).IsIn(ScreenPos.FromInt(e.X, e.Y)))
                    {
                        pane = (IRichPane)c;
                        break;
                    }
                }
            }
            if (_KeyEnabler != null)
            {
                #region ダイアログ表示によっておかしくなったキーステータスを元に戻す処理(マウスムーブのフィーリングが多少悪くなるかも･･･)
                var ke = new KeyEventArgs(Keys.None);
                // 記憶してあるキー情報を元にイベントArgsを生成
                if (_mouseStateButtons.IsShift)
                {
                    ke = new KeyEventArgs(ke.KeyData | Keys.Shift);
                }

                if (_mouseStateButtons.IsCtrl)
                {
                    ke = new KeyEventArgs(ke.KeyData | Keys.Control);
                }
                // keをデフォルトとして記憶
                var defE = new KeyEventArgs(ke.KeyData);
                // 現在のキーの状態を調査
                if (((System.Windows.Forms.Form.ModifierKeys & Keys.Shift) == Keys.Shift) != _mouseStateButtons.IsShift)
                {   // Shiftが離されてしまっていた場合
                    ke = new KeyEventArgs(ke.KeyData ^ Keys.Shift);
                }
                if ((System.Windows.Forms.Form.ModifierKeys & Keys.Control) == Keys.Control != _mouseStateButtons.IsCtrl)
                {   // Ctrlが話されてしまっていた場合
                    ke = new KeyEventArgs(ke.KeyData ^ Keys.Control);
                }
                // 記憶しているキー情報と現状のキー情報が違っていた場合
                if (ke.KeyData != defE.KeyData)
                {
                    _KeyEnabler.KickKeyUp(ke);
                }
                #endregion
            }

            // イベントを飛ばす
            var ma = MouseState.FromMouseEventArgs(e, pane);
            ma.Attr.SetKeyFrags(_mouseStateButtons);

            _rootGroup.OnMouseMove(ma);
        }

        /// <summary>
        /// Play
        /// </summary>
        /// <param name="t"></param>
		public void Play(DeviceRecord.TagMouseMove t)
        {
            _rootGroup.OnMouseMove(t.MouseState);
        }

        /// <summary>
        /// Play
        /// </summary>
        /// <param name="t"></param>
		public void Play(DeviceRecord.TagMouseDown t)
        {
            _rootGroup.OnMouseDown(t.MouseState);
        }

        /// <summary>
        /// Play
        /// </summary>
        /// <param name="t"></param>
		public void Play(DeviceRecord.TagMouseUp t)
        {
            _rootGroup.OnMouseUp(t.MouseState);
        }

        /// <summary>
        /// Play
        /// </summary>
        /// <param name="t"></param>
		public void Play(DeviceRecord.TagMouseWheel t)
        {
            _rootGroup.OnMouseWheel(t.MouseState);
        }

        /// <summary>
        /// Play
        /// </summary>
        /// <param name="t"></param>
		public void Play(DeviceRecord.TagKeyDown t)
        {
            _rootGroup.OnKeyDown(t.KeyState);
        }

        public void Play(DeviceRecord.TagKeyUp t)
        {
            _rootGroup.OnKeyUp(t.KeyState);
        }

        /// <summary>
        /// Play
        /// </summary>
        /// <param name="t"></param>
		public void Play(DeviceRecord.TagToken t)
        {
            if (t.State is NamedId lid)
            {
                var name = lid.Name;
                name = name.Replace("@@", "\a");
                var nms = name.Split(new char[] { '\a' });
                if (nms.Length == 3)
                {
                    var fct = Type.GetType(nms[1]);
                    Id i = new Id { Value = int.Parse(nms[2]) };
                    var tokenid = NamedId.FromIDNoName(i);
                    switch (nms[0])
                    {
                        case "request":
                            _rootGroup.RequestStartup(fct, tokenid);
                            break;
                            //						case "event":
                            //							_rootGroup.FireEvent(fct, tokenid);
                            //							break;
                    }
                }
            }
        }

        /// <summary>
        /// マウスボタンダウンイベントの処理
        /// </summary>
        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            if (_cannotEventProc())
            {
                return;
            }

            // クリックしたペーンを捜す
            IRichPane pane = this;
            foreach (Control c in Controls)
            {
                if (c is IRichPane)
                {
                    if (ScreenRect.FromControl(c).IsIn(ScreenPos.FromInt(e.X, e.Y)))
                    {
                        pane = (IRichPane)c;
                        break;
                    }
                }
            }
            // イベントを飛ばす
            var ma = MouseState.FromMouseEventArgs(e, pane);

            //マウスボタンの状態を記憶する
            _mouseStateButtons.IsButton = ma.Attr.IsButton;
            _mouseStateButtons.IsButtonMiddle = ma.Attr.IsButtonMiddle;

            // uMouseState.FromMouseEventArgsで対応できなかった他の属性を反映する
            ma.Attr.SetKeyFrags(_mouseStateButtons);
            _rootGroup.OnMouseDown(ma);
        }

        /// <summary>
        /// マウスボタンアップイベントの処理
        /// </summary>
        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            if (_cannotEventProc())
            {
                return;
            }

            // クリックしたペーンを捜す
            IRichPane pane = this;
            foreach (Control c in Controls)
            {
                if (c is IRichPane)
                {
                    if (ScreenRect.FromControl(c).IsIn(ScreenPos.FromInt(e.X, e.Y)))
                    {
                        pane = (IRichPane)c;
                        break;
                    }
                }
            }
            // イベントを飛ばす
            var ma = MouseState.FromMouseEventArgs(e, pane);
            ma.Attr.SetKeyFrags(_mouseStateButtons);
            _rootGroup.OnMouseUp(ma);

            //マウスボタンの状態を記憶する
            if (ma.Attr.IsButton)
            {
                _mouseStateButtons.IsButton = false;
            }
            if (ma.Attr.IsButtonMiddle)
            {
                _mouseStateButtons.IsButtonMiddle = false;
            }

            // コンテキストメニューが登録されていたら表示させる
            if (e.Button == MouseButtons.Right)
            {
                if (pane.Control.ContextMenu != null)
                {
                    MouseState.saveContext(e, pane);
                    pane.Control.ContextMenu.Show(pane.Control, ma.Pos - pane.GetPaneRect().LT);
                }
                if (pane.Control.ContextMenuStrip != null)
                {
                    MouseState.saveContext(e, pane);
                    pane.Control.ContextMenuStrip.Show(pane.Control, ma.Pos - pane.GetPaneRect().LT);
                }
            }
        }

        /// <summary>
        /// マウスホイールのイベント
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            if (_cannotEventProc())
            {
                return;
            }

            // クリックしたペーンを捜す
            IRichPane pane = this;
            foreach (Control c in Controls)
            {
                if (c is IRichPane)
                {
                    if (ScreenRect.FromControl(c).IsIn(ScreenPos.FromInt(e.X, e.Y)))
                    {
                        pane = (IRichPane)c;
                        break;
                    }
                }
            }
            // イベントを飛ばす
            var ma = MouseState.FromMouseEventArgs(e, pane);
            ma.Attr.SetKeyFrags(_mouseStateButtons);

            _rootGroup.OnMouseWheel(ma);
            //base.OnMouseWheel (e);
        }

        /// <summary>
        /// アイテムがドラッグされてきた時のイベント(転送はしない)
        /// </summary>
        /// <param name="drgevent"></param>
        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            if (DesignMode)
            {
                return;
            }

            if (_cannotEventProc())
            {
                return;
            }

            //base.OnDragEnter (drgevent);
            _DragState = DragState.FromDragEventArgs(drgevent, null);
            drgevent.Effect = DragDropEffects.Copy;
            //_DragKeyState = drgevent.KeyState;
            if ((drgevent.KeyState & (8 + 4)) == (8 + 4))
            {   // Ctrl + Shift
                drgevent.Effect = DragDropEffects.Link;
            }
            else if ((drgevent.KeyState & 8) == 8)
            {   // Ctrl
                drgevent.Effect = DragDropEffects.Copy;
            }
            else if ((drgevent.KeyState & 4) == 4)
            {   // Shift
                drgevent.Effect = DragDropEffects.Move;
            }
        }

        /// <summary>
        /// 処理不可なら true
        /// </summary>
        /// <returns></returns>
        private bool _cannotEventProc()
        {
            if (_isDrawing)
            {
                System.Diagnostics.Debug.WriteLine("！？　描画中に描画イベント on " + GetType().Name);
                return true;
            }
            return false;
        }

        /// <summary>
        /// アイテムがドロップされた時のイベント
        /// </summary>
        /// <param name="drgevent"></param>
        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            if (DesignMode)
            {
                return;
            }

            if (_cannotEventProc())
            {
                return;
            }

            // マウスカーソルのスクリーン座標⇒フォームのクライアント座標を算出
            var p = PointToClient(MouseState.NowPosition);
            // ドロップされたペーンを捜す
            IRichPane pane = this;
            foreach (Control c in Controls)
            {
                if (c is IRichPane)
                {
                    if (ScreenRect.FromControl(c).IsIn(ScreenPos.FromInt(p.X, p.Y)))
                    {
                        pane = (IRichPane)c;
                        break;
                    }
                }
            }

            _DragState.Pos = ScreenPos.FromInt(p.X, p.Y);
            _DragState.Pane = pane;
            _DragState.Attr.SetKeyFrags(_mouseStateButtons);

            _rootGroup.OnDragDrop(_DragState);
            _DragState = null;
        }

        /// <summary>
        /// 描画イベント処理
        /// </summary>
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            // for Design mode
            if (_rootGroup == null)
            {
                using (Brush brush = new SolidBrush(BackColor))
                {
                    e.Graphics.FillRectangle(brush, e.ClipRectangle);
                }
                using (var fo = new Font("Arial", 9.0f))
                {
                    e.Graphics.DrawString("Tono.GuiWinForm Architecture", fo, Brushes.Blue, 20, 20);
                }
                base.OnPaint(e);
                return;
            }

            // Thread safe check
            if (_isDrawing)
            {
                System.Diagnostics.Debug.WriteLine("！？　描画中に描画イベント on " + GetType().Name);
                return;
            }
            _isDrawing = true;
            _currentGraphics = e.Graphics;
            _currentPaintClip = ScreenRect.FromRectangle(e.ClipRectangle);

            if (_rootGroup != null)
            {
                // 必要であれば背景を描画
                if (IsDrawEmptyBackground)
                {
                    using Brush brush = new SolidBrush(BackColor);
                    e.Graphics.FillRectangle(brush, e.ClipRectangle);
                }
                // パーツ等の描画
                var pb = _rootGroup.GetPartsSet();
                if (pb != null)
                {
                    pb.CheckAndResetLocalized();
                    pb.ProvideDrawFunction();

                    // フリーレイヤーを描画
                    for (var i = 0; i < _freeLayers.Count; i++)
                    {
                        var fl = (FreeDrawLayer)_freeLayers[i];
                        if (fl.IsUsing)
                        {
                            e.Graphics.Clip = new Region(new Rectangle(0, 0, fl.Image.Width, fl.Image.Height));
                            e.Graphics.DrawImage(fl.Image, 0, 0);
                        }
                    }
                }
                base.OnPaint(e);
            }
            _isDrawing = false;
        }

        /// <summary>
        /// サイズ変更時にレイアウトを調整する
        /// </summary>
        protected override void OnSizeChanged(System.EventArgs e)
        {
            if (DesignMode == false)
            {
                PerformLayout();
                SendScrollChangedEvent(null);
                foreach (Control c in Controls)
                {
                    IRichPane rp = c as TPane;
                    if (rp != null)
                    {
                        SendScrollChangedEvent(rp);
                    }
                }
                Invalidate();
            }
        }

        #region IRichPane メンバ


        public IRichPane GetParent()
        {
            if (Parent is IRichPane)
            {
                return (IRichPane)Parent;
            }
            return null;
        }

        public string IdText
        {

            get => Name;
            set
            {
            }
        }

        public Control Control => this;

        /// <summary>
        /// 名前でペーンを検索する
        /// </summary>
        /// <param name="tar">検索ペーンの階層現在位置</param>
        /// <param name="name">検索Name</param>
        /// <returns>見つかったペーン / null = その階層には見つからなかった</returns>
        private IRichPane _findPaneByIdText(Control tar, string IdText)
        {
            foreach (Control c in tar.Controls)
            {
                if (c is IRichPane)
                {
                    if (((IRichPane)c).IdText == IdText)
                    {
                        return (IRichPane)c;
                    }
                }
                var ret = _findPaneByIdText(c, IdText);
                if (ret != null)
                {
                    return ret;
                }
            }
            return null;
        }
        public IRichPane GetPane(string IdText)
        {
            if (this.IdText == IdText)
            {
                return this;
            }
            IRichPane root;
            for (root = this; root.GetParent() != null; root = root.GetParent())
            {
                ;
            }

            return _findPaneByIdText((Control)root, IdText);
        }

        public ScreenPos GetZoomed(LayoutPos value)
        {
            XyBase ret = value * _zoom / 1000;
            return ScreenPos.FromInt(ret.X, ret.Y);
        }
        public LayoutPos GetZoomed(ScreenPos value)
        {
            XyBase ret = value * 1000 / _zoom;
            return LayoutPos.FromInt(ret.X, ret.Y);
        }
        public ScreenPos Convert(LayoutPos value)
        {
            XyBase ret = value * _zoom / 1000 + _scroll + GetPaneRect().LT;
            return ScreenPos.FromInt(ret.X, ret.Y);
        }
        public ScreenRect Convert(LayoutRect value)
        {
            var ret = value * _zoom / 1000 + _scroll + GetPaneRect().LT;
            return ScreenRect.FromLTWH(ret.LT.X, ret.LT.Y, ret.Width, ret.Height);
        }

        public LayoutPos Convert(ScreenPos value)
        {
            XyBase ret = (value - GetPaneRect().LT - _scroll) * 1000 / _zoom;
            return LayoutPos.FromInt(ret.X, ret.Y);
        }
        public LayoutRect Convert(ScreenRect value)
        {
            var ret = (value - GetPaneRect().LT - _scroll) * 1000 / _zoom;
            return LayoutRect.FromLTRB(ret.LT.X, ret.LT.Y, ret.RB.X, ret.RB.Y);
        }

        /// <summary>
        /// マスターペーンの領域を返す
        /// </summary>
        /// <returns></returns>

        public ScreenRect GetPaneRect()
        {
            return ScreenRect.FromLTWH(0, 0, Width, Height);
        }

        /// <summary>
        /// 描画が必要な領域を返すインターフェース
        /// </summary>
        /// <returns>領域</returns>

        public ScreenRect GetPaintClipRect()
        {
            return _currentPaintClip;
        }

        /// <summary>
        /// スクロール量
        /// </summary>
        [Category("Tono.GuiWinForm")]
        [Description("スクロール量")]
        public new ScreenPos Scroll    // new 識別子は、.NET2.0で必要
        {

            get => _scroll;
            set
            {
                if (value != null)
                {
                    _scroll = value;
                    if (_scroll != null)
                    {
                        foreach (Control c in Controls)
                        {
                            if (c is IRichPane)
                            {
                                ((IRichPane)c).Scroll = (ScreenPos)_scroll.Clone();
                            }
                        }
                        SendScrollChangedEvent(this);
                    }
                }
                else
                {
                    _scroll = ScreenPos.FromInt(0, 0);
                }
            }
        }

        /// <summary>
        /// ズーム倍率 * 10[%]
        /// </summary>
        [Category("Tono.GuiWinForm")]
        [Description("現在のズーム値 * 10[%]")]
        public XyBase Zoom
        {

            get => _zoom;
            set
            {
                if (value != null)
                {
                    _zoom = ZoomCheck(value);
                    foreach (Control c in Controls)
                    {
                        if (c is IRichPane)
                        {
                            ((IRichPane)c).Zoom = (XyBase)_zoom.Clone();
                        }
                    }
                    SendZoomChangedEvent(this);
                }
                else
                {
                    _zoom = XyBase.FromInt(1000, 1000);
                }
            }
        }

        /// <summary>
        /// ズーム変更イベントを送信する
        /// </summary>
        /// <param name="rp">ズームしたペーン（イベント送り元）</param>
        public void SendZoomChangedEvent(IRichPane rp)
        {
            _rootGroup.ZoomChanged(rp);
        }

        /// <summary>
        /// スクロール変更イベントを送信する
        /// </summary>
        /// <param name="rp">スクロールしたペーン（イベント送り元）</param>
        public void SendScrollChangedEvent(IRichPane rp)
        {
            _rootGroup.ScrollChanged(rp);
        }

        /// <summary>
        /// ズームの値をチェックして不正値なら調整する
        /// </summary>
        /// <param name="value">希望するズーム値</param>
        /// <returns>ふさわしく調整されたズーム値</returns>
        public XyBase ZoomCheck(XyBase value)
        {
            if (value.X < 8)
            {
                value.X = 8;
            }

            if (value.Y < 10)
            {
                value.Y = 10;
            }

            if (value.X > 32000)
            {
                value.X = 32000;
            }

            if (value.Y > 32000)
            {
                value.Y = 32000;
            }

            return value;
        }

        /// <summary>
        /// グラフィックオブジェクト
        /// </summary>
        [Category("Tono.GuiWinForm")]
        [Description("描画用のGraphicオブジェクト")]
        public System.Drawing.Graphics Graphics => _currentGraphics;

        /// <summary>
        /// 画面を再描画する
        /// </summary>
        /// <param name="rect">再描画する範囲</param>
        public void Invalidate(ScreenRect rect)
        {
            if (rect == null)
            {
                base.Invalidate();
            }
            else
            {
                base.Invalidate(rect);
            }
        }

        #endregion

        #region IRichPaneSync メンバ

        /// <summary>
        /// ズーム倍率 * 10[%]
        /// </summary>
        [Category("Tono.GuiWinForm")]
        [Description("現在のズーム値 * 10[%]")]
        public XyBase ZoomMute
        {
            get => _zoom;
            set
            {
                if (value != null)
                {
                    _zoom = ZoomCheck(value);
                    SendZoomChangedEvent(this);
                }
                else
                {
                    _zoom = XyBase.FromInt(1000, 1000);
                }
            }
        }

        /// <summary>
        /// スクロール量
        /// </summary>
        [Category("Tono.GuiWinForm")]
        [Description("現在のスクロール量")]
        public ScreenPos ScrollMute
        {
            get => _scroll;
            set
            {
                if (value != null)
                {
                    _scroll = value;
                    SendScrollChangedEvent(this);
                }
                else
                {
                    _scroll = ScreenPos.FromInt(0, 0);
                }
            }
        }

        #endregion

        #region IKeyListener メンバ（fiKeyEnablerコンポーネントからメッセージを受ける）

        public void OnKeyDown(KeyState e)
        {
            if (DesignMode)
            {
                return;
            }

            _mouseStateButtons.SetKeyFrags(e);
            _rootGroup.OnKeyDown(e);
        }

        public void OnKeyUp(KeyState e)
        {
            if (DesignMode)
            {
                return;
            }

            _mouseStateButtons.SetKeyFrags(e);
            _rootGroup.OnKeyUp(e);
        }

        #endregion

        /// <summary>
        /// キーイベントを再構築して、必要であればイベントを出力する
        /// </summary>
        public void ResetKeyEvents()
        {
            var k = new KeyState(Form.ModifierKeys);
            var m = KeyState.FromMouseStateButtons(_mouseStateButtons);
            var down = false;
            var up = false;
            if (k.IsControl != m.IsControl)
            {
                if (k.IsControl)
                {
                    down = true;
                }
                else
                {
                    up = true;
                }
            }
            if (k.IsShift != m.IsShift)
            {
                if (k.IsShift)
                {
                    down = true;
                }
                else
                {
                    up = true;
                }
            }
            if (down)
            {
                OnKeyDown(k);
            }
            if (up)
            {
                OnKeyUp(k);
            }
            _mouseStateButtons.SetKeyFrags(k);
        }
    }
}
