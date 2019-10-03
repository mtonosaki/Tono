using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// レベル分けしたログを表示するインターフェース
    /// 指定したペーンの上に丁度乗っかる
    /// </summary>
    public class FeatureLogGroupPanel : FeatureBase, IMultiTokenListener, IMouseListener
    {
        #region Parts class
        /// <summary>
        /// パネル描画
        /// </summary>
        protected internal class dpLogPanel : PartsBase, IPartsVisible
        {
            internal class IconJumpState
            {
                private static readonly int _steps = 30;
                private static readonly Angle _angleStart = Angle.FromDeg(240);
                private Angle _angle = _angleStart;
                private int _n = -1;
                private readonly double _r = 60;
                private Thread _th = null;
                public event EventHandler Jumping = null;

                private void _timerProc()
                {
                    for (; ; )
                    {
                        Thread.Sleep(600 / _steps);
                        if (Next())
                        {
                            if (Jumping != null)
                            {
                                Jumping(this, EventArgs.Empty);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                public void Start()
                {
                    lock (this)
                    {
                        _angle = _angleStart;
                        _n = _steps;
                        if (_th != null)
                        {
                            _th.Abort();
                        }
                        _th = new Thread(_timerProc)
                        {
                            Name = "dpLogPanel_timerProc"
                        };
                        _th.Start();
                    }
                }
                public double R => -Y / 3;
                public int Y
                {
                    get
                    {
                        lock (this)
                        {
                            var st = -Math.Sin(_angleStart.Rad) * _r;
                            var now = -Math.Sin(_angle.Rad) * _r;
                            return (int)(now - st);
                        }
                    }
                }
                public bool Next()
                {
                    lock (this)
                    {
                        _n--;
                        if (_n < 0)
                        {
                            return false;
                        }
                        _angle += Angle.FromDeg(360 / _steps);
                        return true;
                    }
                }
            }

            protected static Font _fontTitle = new Font("Meiryo UI", 9f, FontStyle.Bold);
            protected static Font _font = new Font("Meiryo UI", 7.5f, FontStyle.Regular);
            protected static Brush _bDev = new SolidBrush(Color.FromArgb(255, 0, 0, 0));
            protected static Brush _bInf = new SolidBrush(Color.FromArgb(255, 126, 255, 58));
            protected static Brush _bTodo = new SolidBrush(Color.FromArgb(255, 126, 255, 58));
            protected static Brush _bWar = new SolidBrush(Color.FromArgb(255, 227, 227, 227));
            protected static Brush _bErr = new SolidBrush(Color.FromArgb(255, 255, 255, 0));
            protected static Pen _pLine = new Pen(Color.FromArgb(16, 0, 0, 0), 1);
            protected static Pen _pRegionShadow = new Pen(Color.FromArgb(64, 64, 0, 0), 1);
            private readonly Dictionary<LLV, ScreenPos> _clickArea;
            private ScreenRect _margin = ScreenRect.FromLTRB(0, 0, 0, 0);

            /// <summary>
            /// 唯一のコンストラクタ
            /// </summary>
            /// <param name="clickArea"></param>
            public dpLogPanel(Dictionary<LLV, ScreenPos> clickArea)
            {
                _clickArea = clickArea;
                Visible = true;
            }

            /// <summary>
            /// マージンをセットする
            /// </summary>
            /// <param name="margin"></param>
            public void SetMargin(ScreenRect margin)
            {
                _margin = margin;
            }

            private ScreenPos savePos(LLV lv, ScreenPos pos)
            {
                _clickArea[lv] = pos;
                return pos;
            }

            protected virtual Brush createLogPanelBG(ScreenRect sr)
            {
                return new System.Drawing.Drawing2D.LinearGradientBrush(sr.LT, sr.RT,
                    Color.FromArgb(224, 96, 64, 48), Color.FromArgb(128, 48, 48, 0));
            }

            /// <summary>
            /// 描画
            /// </summary>
            /// <param name="rp"></param>
            /// <returns></returns>
            public override bool Draw(IRichPane rp)
            {
                if (Visible)
                {
                    var sr = rp.GetPaneRect();
                    sr.LT.X += _margin.LT.X;
                    sr.RB.X -= _margin.RB.X;
                    sr.RB.Y -= _margin.RB.Y;

                    if (Rect.Height < 1)
                    {
                        return true;
                    }

                    #region 領域の矩形
                    var lgb = createLogPanelBG(sr);

                    sr.LT.Y = sr.RB.Y - Rect.Height;

                    rp.Graphics.DrawLine(_pRegionShadow, sr.RB.X, sr.LT.Y, sr.RB.X, sr.RB.Y);
                    rp.Graphics.DrawLine(_pRegionShadow, sr.LT.X, sr.RB.Y, sr.RB.X, sr.RB.Y);

                    rp.Graphics.FillRectangle(lgb, sr);
                    lgb.Dispose();
                    #endregion
                    #region タイトルボーダー
                    Brush _bd = new SolidBrush(Color.FromArgb(64, 0, 0, 0));
                    var b = 8;
                    var w3 = sr.Width / 4;
                    var pst = new Point[] {
                    new Point(sr.LT.X, sr.LT.Y + b + 16), new Point(sr.LT.X, sr.LT.Y), new Point(sr.RB.X, sr.LT.Y),
                };
                    var psb = new Point[] {
                    new Point(sr.RB.X, sr.LT.Y + b), new Point(sr.RB.X-w3, sr.LT.Y + b + 2),    new Point(sr.RB.X - w3*2, sr.LT.Y + b + 8), new Point(sr.RB.X - w3*3, sr.LT.Y + b + 16), new Point(sr.LT.X, sr.LT.Y + b + 16),
                };
                    var ps = new Point[pst.Length + psb.Length];
                    int i;
                    for (i = 0; i < pst.Length; i++)
                    {
                        ps[i] = pst[i];
                    }

                    for (var j = 0; j < psb.Length; j++)
                    {
                        ps[i++] = psb[j];
                    }

                    rp.Graphics.FillPolygon(_bd, ps);
                    _bd.Dispose();
                    // ハイライト
                    using (var p = new Pen(Color.FromArgb(96, 255, 255, 255)))
                    {
                        rp.Graphics.DrawLines(p, pst);
                    }
                    // シャドウ
                    using (var p = new Pen(Color.FromArgb(96, 0, 0, 0)))
                    {
                        rp.Graphics.DrawLines(p, psb);
                    }
                    #endregion

                    // タイトルメッセージ
                    var btr = sr.Clone() as ScreenRect;
                    btr.RB.X = btr.LT.X + 24;
                    btr.RB.Y = btr.LT.Y + 12;
                    btr += XyBase.FromInt(4, 4);
                    var titlestr = Mes.Current["LogGroupPanel", "Title"];
                    rp.Graphics.DrawString(titlestr, _fontTitle, new SolidBrush(Color.FromArgb(192, 192, 255)), btr.LT.X, btr.LT.Y);
                    btr += XyBase.FromInt((int)rp.Graphics.MeasureString(titlestr, _fontTitle).Width + 8, 0);

                    // 表示レベルボタン
                    _clickArea.Clear();
                    ScreenPos pos;
                    pos = savePos(LLV.ERR, btr.LT + XyBase.FromInt(0 - (int)(LOG.JumpErr.R / 2), LOG.JumpErr.Y));
                    rp.Graphics.DrawImage(LOG.ErrSw ? Properties.Resources.lp_Err_on : Properties.Resources.lp_Err_off, pos.X, pos.Y, (float)(LOG.JumpErr.R + Properties.Resources.lp_Err_on.Width), (float)(LOG.JumpErr.R + Properties.Resources.lp_Err_on.Height));
                    pos = savePos(LLV.WAR, btr.LT + XyBase.FromInt(22 - (int)(LOG.JumpWar.R / 2), LOG.JumpWar.Y));
                    rp.Graphics.DrawImage(LOG.WarSw ? Properties.Resources.lp_War_on : Properties.Resources.lp_War_off, pos.X, pos.Y, (float)(LOG.JumpWar.R + Properties.Resources.lp_War_on.Width), (float)(LOG.JumpWar.R + Properties.Resources.lp_War_on.Height));
                    pos = savePos(LLV.INF, btr.LT + XyBase.FromInt(44 - (int)(LOG.JumpInf.R / 2), LOG.JumpInf.Y));
                    rp.Graphics.DrawImage(LOG.InfSw ? Properties.Resources.lp_Inf_on : Properties.Resources.lp_Inf_off, pos.X, pos.Y, (float)(LOG.JumpInf.R + Properties.Resources.lp_Inf_on.Width), (float)(LOG.JumpInf.R + Properties.Resources.lp_Inf_on.Height));
                    pos = savePos(LLV.DEV, btr.LT + XyBase.FromInt(66 - (int)(LOG.JumpDev.R / 2), LOG.JumpDev.Y));
                    rp.Graphics.DrawImage(LOG.DevSw ? Properties.Resources.lp_Dev_on : Properties.Resources.lp_Dev_off, pos.X, pos.Y, (float)(LOG.JumpDev.R + Properties.Resources.lp_Dev_on.Width), (float)(LOG.JumpDev.R + Properties.Resources.lp_Dev_on.Height));

                    // クローズボタン
                    pos = savePos(0, btr.LT + XyBase.FromInt(100 - (int)(LOG.JumpDev.R / 2), LOG.JumpDev.Y));
                    rp.Graphics.DrawImage(LOG.DevSw ? Properties.Resources.Cancel : Properties.Resources.Cancel, (float)pos.X, pos.Y, 16, 16);

                    // テキスト表示領域のみ
                    sr.Deflate(2);
                    sr.LT.X += 16;
                    sr.LT.Y += b + 16;
                    // for test		rp.Graphics.DrawLine(Pens.White, sr.LT, sr.RB);

                    // メッセージ表示				
                    var ms = rp.Graphics.MeasureString("AX08iIay", _font);
                    float lm = 2;   // 行間
                    var y1 = ms.Height + lm;
                    var dispLineN = (int)(sr.Height / y1);
                    float curN = LOG.GetCurrentCount();
                    float btY;
                    if (curN < dispLineN)
                    {
                        btY = sr.RB.Y - (int)(dispLineN - curN + 1) * y1;
                    }
                    else
                    {
                        btY = sr.RB.Y - y1;
                    }
                    var lu = LOG.GetCurrentLast();

                    for (; ; )
                    {
                        if (btY < sr.LT.Y - 2 || lu == null)
                        {
                            break;
                        }

                        var br = _bInf;
                        switch (lu.Value.Level)
                        {
                            case LLV.WAR:
                                br = _bWar;
                                break;
                            case LLV.ERR:
                                br = _bErr;
                                break;
                            case LLV.DEV:
                                br = _bDev;
                                break;
                            case LLV.INF:
                                br = _bInf;
                                break;
                            case LLV.TODO:
                                br = _bTodo;
                                break;
                        }
                        rp.Graphics.DrawString(lu.Value.Mes, _font, br, new PointF(sr.LT.X, btY));
                        rp.Graphics.DrawLine(_pLine, sr.LT.X, btY + y1 - 3, sr.RB.X, btY + y1 - 3);
                        if (lu.Value.Icon != null)
                        {
                            rp.Graphics.DrawImageUnscaled(lu.Value.Icon, sr.LT.X - 17, (int)(btY - 1));
                        }
                        btY -= y1;
                        lu = lu.Previous;
                    }
                }
                return true;
            }

            public bool Visible
            {
                get;
                set;
            }
        }
        #endregion

        /// <summary>
        /// ログ表示ペーン
        /// </summary>
        private IRichPane _tarPane = null;
        /// <summary>
        /// パーツインスタンス
        /// </summary>
        private dpLogPanel _lp = null;
        /// <summary>
        /// レベル毎の表示ボタンクリック左上座標
        /// </summary>
        private readonly Dictionary<LLV, ScreenPos> _clickArea = new Dictionary<LLV, ScreenPos>();
        /// <summary>
        /// ログパネル表示位置マージン（各LTRBをマージン幅として用いる）
        /// </summary>
        private readonly ScreenRect _margin = ScreenRect.FromLTRB(0, 0, 0, 0);

        /// <summary>
        /// パラメータを取得する
        /// </summary>
        /// <param name="param"></param>
        public override void ParseParameter(string param)
        {
            base.ParseParameter(param);
            foreach (var c1 in StrUtil.SplitTrim(param, ";"))
            {
                var com = StrUtil.SplitTrim(c1, "=");
                if (com.Length != 2)
                {
                    continue;
                }

                if (com[0].Equals("Pane", StringComparison.CurrentCultureIgnoreCase))
                {
                    _tarPane = Pane.GetPane(com[1]);
                }
                if (com[0].Equals("MarginL", StringComparison.CurrentCultureIgnoreCase))
                {
                    _margin.LT.X = int.Parse(com[1]);
                }
                if (com[0].Equals("MarginT", StringComparison.CurrentCultureIgnoreCase))
                {
                    _margin.LT.Y = int.Parse(com[1]);
                }
                if (com[0].Equals("MarginR", StringComparison.CurrentCultureIgnoreCase))
                {
                    _margin.RB.X = int.Parse(com[1]);
                }
                if (com[0].Equals("MarginB", StringComparison.CurrentCultureIgnoreCase))
                {
                    _margin.RB.Y = int.Parse(com[1]);
                }
            }
        }
        /// <summary>
        /// フィーチャー初期化
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();
            Token.Add(MultiTokenTriggerID[0], null);
            LOG.LogClearRequested += new EventHandler<EventArgs>(LOG_LogClearRequested);
        }

        /// <summary>
        /// ログクリア要求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LOG_LogClearRequested(object sender, EventArgs e)
        {
            _tarPane.Invalidate(null);
        }

        /// <summary>
        /// dpLogPanelのインスタンスを生成する
        /// </summary>
        /// <param name="clickArea"></param>
        /// <returns></returns>
        protected virtual dpLogPanel createLogPartInstance(Dictionary<LLV, ScreenPos> clickArea)
        {
            return new dpLogPanel(clickArea);
        }

        /// <summary>
        /// パーツ作成開始
        /// </summary>
        /// <param name="who"></param>
        public override void Start(NamedId who)
        {
            base.Start(who);
            if (MultiTokenTriggerID[0].Equals(who))
            {
                var ph = (int)ConfigRegister.Current["LogPanelGroupHeight", 128];
                _lp = createLogPartInstance(_clickArea);
                LOG.JumpInf.Jumping += new EventHandler(onJumping);
                LOG.JumpErr.Jumping += new EventHandler(onJumping);
                LOG.JumpWar.Jumping += new EventHandler(onJumping);
                LOG.JumpDev.Jumping += new EventHandler(onJumping);
                _lp.SetMargin(_margin);
                _lp.Rect = CodeRect.FromLTWH(0, 0, 0, ph);   // 高さの初期値（単位；ドット）
                Parts.Add(_tarPane, _lp, Const.Layer.StaticLayers.LogPanel);
                _tarPane.Invalidate(null);

                Timer.AddTrigger(320, new GuiTimer.Proc0(refresh0));
            }
            if (MultiTokenTriggerID[1].Equals(who))
            {
                triggerCloseButtonProc();
            }
        }

        /// <summary>
        /// 折りたたんだ時のサイズ
        /// </summary>
        protected virtual int foldingHeight => 24;

        /// <summary>
        /// クローズボタンを押したときの処理
        /// </summary>
        private void triggerCloseButtonProc()
        {
            if (_lp.Rect.RB.Y > 32)
            {
                ConfigRegister.Current["LogPanelHeight"] = _lp.Rect.RB.Y;
                _lp.Rect.RB.Y = foldingHeight;
            }
            else
            {
                try
                {
                    var h = (int)ConfigRegister.Current["LogPanelHeight", _tarPane.Control.Height / 5];
                    if (h > _tarPane.Control.Height * 8 / 10)
                    {
                        h = _tarPane.Control.Height * 8 / 10;
                    }

                    if (h < 64)
                    {
                        h = 64;
                    }

                    _lp.Rect.RB.Y = h;
                }
                catch (Exception)
                {
                    _lp.Rect.RB.Y = 160;
                    ConfigRegister.Current["LogPanelHeight"] = 0;
                }
            }
            _tarPane.Invalidate(null);
        }

        /// <summary>
        /// ジャンプしているときの継続イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onJumping(object sender, EventArgs e)
        {
            Pane.Invalidate(null);
        }

        /// <summary>
        /// リフレッシュ
        /// </summary>
        private void refresh0()
        {
            refresh();
            LOG.NoJumpNext();
            LOG.WriteMesFormatLine(LLV.INF, Mes.Format.FromString("@LogGroupPanel.Welcome@"));
            LOG.NoJumpNext();
            LOG.WriteLine(LLV.INF, "          Version {0}", null, Application.ProductVersion);
            LOG.NoJumpNext();
            LOG.WriteMesFormatLine(LLV.INF, Mes.Format.FromString("@LogGroupPanel.LangInfo@"), Properties.Resources.li_hint);
            _tarPane.Invalidate(null);
        }

        /// <summary>
        /// ログの表示更新を試みる
        /// </summary>
        private void refresh()
        {
            if (LOG.CheckAndClearRequestFlag())
            {
                var sr = _tarPane.GetPaneRect();
                sr.LT.Y = sr.RB.Y - _lp.Rect.Height;
                Pane.Invalidate(sr);
            }
            Timer.AddTrigger(500, new GuiTimer.Proc0(refresh));
        }

        /// <summary>
        /// ログパネルの高さを保存して、次回起動時に反映できるようにする
        /// </summary>
        private void savePanelHeight()
        {
            ConfigRegister.Current["LogPanelGroupHeight"] = _lp.Rect.Height;
        }

        #region IMouseListener メンバ

        private ScreenPos _sizingHeightOrg = null;	// ログパネルの高さ変更中

        private ScreenRect getPaneRect()
        {

            var pr = _tarPane.GetPaneRect();
            pr.LT.X += _margin.LT.X;
            // pr.LT.Y += _margin.LT.Y;
            pr.RB.X -= _margin.RB.X;
            pr.RB.Y -= _margin.RB.Y;

            return pr;
        }

        public void OnMouseMove(MouseState e)
        {
            var pr = getPaneRect();

            if (_sizingHeightOrg != null)
            {
                if (e.Attr.IsButton)
                {
                    var adp = e.Pos.Clone() as ScreenPos;
                    if (adp.Y < pr.LT.Y)
                    {
                        adp.Y = pr.LT.Y;
                    }

                    if (pr.RB.Y - adp.Y < foldingHeight)
                    {
                        adp.Y = pr.RB.Y - foldingHeight;
                    }

                    Pane.Control.Cursor = Cursors.SizeNS;
                    _lp.Rect.RB.Y = pr.RB.Y - adp.Y;
                    _tarPane.Invalidate(null);
                }
                else
                {
                    Pane.Control.Cursor = Cursors.Arrow;
                    _sizingHeightOrg = null;
                    savePanelHeight();
                }
            }
            else
            {
                if (_lp != null && Math.Abs(e.Pos.Y - (pr.RB.Y - _lp.Rect.Height)) < 4 && pr.LR.IsIn(e.Pos))    // ログパネルサイズ変更
                {
                    Pane.Control.Cursor = Cursors.SizeNS;
                }
                else
                {
                    var isCur = false;
                    foreach (var kv in _clickArea)
                    {
                        var re = ScreenRect.FromLTWH(kv.Value.X, kv.Value.Y, 20, 20);
                        if (re.IsIn(e.Pos))
                        {
                            isCur = true;
                        }
                    }
                    if (isCur)
                    {
                        Pane.Control.Cursor = Cursors.Hand;
                    }
                    else
                    {
                        Pane.Control.Cursor = Cursors.Arrow;
                    }
                }
            }
        }

        public void OnMouseDown(MouseState e)
        {
            var pr = getPaneRect();
            if (_lp != null && Math.Abs(e.Pos.Y - (pr.RB.Y - _lp.Rect.Height)) < 4 && pr.LR.IsIn(e.Pos))    // ログパネルサイズ変更
            {
                _sizingHeightOrg = e.Pos.Clone() as ScreenPos;
                Pane.Control.Cursor = Cursors.SizeNS;
            }
            else
            {
                foreach (var kv in _clickArea)
                {
                    var re = ScreenRect.FromLTWH(kv.Value.X, kv.Value.Y, 20, 20);
                    if (re.IsIn(e.Pos))
                    {
                        LOG.ChangeDispLevel(kv.Key);
                        if (kv.Key == 0)    // クローズボタン
                        {
                            triggerCloseButtonProc();
                        }
                    }
                }
            }
        }

        public void OnMouseUp(MouseState e)
        {
            if (_sizingHeightOrg != null)
            {
                _sizingHeightOrg = null;
                savePanelHeight();
            }
        }

        public void OnMouseWheel(MouseState e)
        {
        }

        #endregion
        #region IMultiTokenListener メンバ
        private static readonly NamedId[] _tokens = new NamedId[] { NamedId.FromName("FeatureLogGroupPanel_Init"), NamedId.FromName("LogPanelCloseButton") };

        public NamedId[] MultiTokenTriggerID => _tokens;

        #endregion
    }
}
