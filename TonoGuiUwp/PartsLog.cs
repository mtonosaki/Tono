// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Text;
using static Tono.Gui.Uwp.CastUtil;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// general log panel design parts
    /// </summary>
    public class PartsLog : PartsBase<float, float>
    {
        private readonly Dictionary<LLV, bool> visibles = new Dictionary<LLV, bool>
        {
            [LLV.ERR] = true,
            [LLV.WAR] = true,
            [LLV.INF] = true,
            [LLV.DEV] = false,
        };
        private readonly Dictionary<LLV, ScreenRect> btnAreas = new Dictionary<LLV, ScreenRect>();

        /// <summary>
        /// query button drawing rectangles
        /// </summary>
        /// <returns></returns>
        public IEnumerable<(LLV, ScreenRect)> GetButtonAreas()
        {
            return from ba in btnAreas select (ba.Key, ba.Value);
        }

        /// <summary>
        /// set visible flag of log level
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="sw"></param>
        public void SetVisible(LLV lv, bool sw)
        {
            visibles[lv] = sw;
        }

        /// <summary>
        /// get visible flag of log level
        /// </summary>
        /// <param name="lv"></param>
        /// <returns></returns>
        public bool GetVisible(LLV lv)
        {
            return visibles[lv];
        }

        public PartsLog()
        {
        }

        private FeatureLogPanel _parent;

        /// <summary>
        /// save owner feature
        /// </summary>
        /// <param name="parent"></param>
        public void SetParent(FeatureLogPanel parent)
        {
            _parent = parent;
        }

        protected FeatureLogPanel Parent => _parent;

        /// <summary>
        /// drawing
        /// </summary>
        /// <param name="dp"></param>
        public override void Draw(DrawProperty dp)
        {
            if (Parent.IsVisible == false)
            {
                return;  // draw when feature is active only.
            }

            var sr = dp.PaneRect;

            drawBackground(dp, sr);

            var theight = drawTitleBar(dp, sr);

            drawMessage(dp, sr, theight);
        }

        /// <summary>
        /// background design
        /// </summary>
        /// <param name="dp"></param>
        /// <param name="sr"></param>
        protected virtual void drawBackground(DrawProperty dp, ScreenRect sr)
        {
            var bg = new CanvasLinearGradientBrush(dp.Canvas, Parent.BackgroundColor1, Parent.BackgroundColor2)
            {
                StartPoint = _(ScreenPos.From(sr.LT.X, sr.LT.Y)),
                EndPoint = _(ScreenPos.From(sr.RB.X, sr.RB.Y)),
            };
            dp.Graphics.FillRectangle(_(dp.PaneRect), bg);
        }

        /// <summary>
        /// title bar design
        /// </summary>
        /// <param name="dp"></param>
        /// <param name="sr">log panel drawing area</param>
        /// <returns></returns>
        protected virtual ScreenY drawTitleBar(DrawProperty dp, ScreenRect sr)
        {
            var _bd = new CanvasSolidColorBrush(dp.Canvas, Color.FromArgb(64, 0, 0, 0));
            var titleHeight = 8;
            var w3 = sr.Width / 4;
            var pst = new ScreenPos[] { ScreenPos.From(sr.LT.X, sr.LT.Y + titleHeight + 16), ScreenPos.From(sr.LT.X, sr.LT.Y), ScreenPos.From(sr.RB.X, sr.LT.Y), };
            var psb = new ScreenPos[] { ScreenPos.From(sr.RB.X, sr.LT.Y + titleHeight), ScreenPos.From(sr.RB.X - w3, sr.LT.Y + titleHeight + 2), ScreenPos.From(sr.RB.X - w3 * 2, sr.LT.Y + titleHeight + 8), ScreenPos.From(sr.RB.X - w3 * 3, sr.LT.Y + titleHeight + 16), ScreenPos.From(sr.LT.X, sr.LT.Y + titleHeight + 16), };
            var ps = pst.Union(psb).ToList();
            var path = new CanvasPathBuilder(dp.Canvas);
            path.BeginFigure(ps[0]);
            for (var i = 1; i < ps.Count; i++)
            {
                path.AddLine(ps[i]);
            }
            path.EndFigure(CanvasFigureLoop.Closed);
            var geo = CanvasGeometry.CreatePath(path);
            dp.Graphics.FillGeometry(geo, _bd);

            // edge highlight
            for (var i = 1; i < pst.Length; i++)
            {
                dp.Graphics.DrawLine(pst[i - 1], pst[i], Color.FromArgb(96, 255, 255, 255));
            }
            // edge shadow
            for (var i = 1; i < psb.Length; i++)
            {
                dp.Graphics.DrawLine(psb[i - 1], psb[i], Color.FromArgb(96, 0, 0, 0));
            }

            // title bar design
            var btr = sr.Clone();
            btr.RB = ScreenPos.From(btr.LT.X + ScreenX.From(24), btr.LT.Y + ScreenY.From(12));
            btr = btr + ScreenPos.From(4, 4);
            var imgttl = Assets.Image("LogPanelTitileDesign");
            if (imgttl != null)
            {
                dp.Graphics.DrawImage(imgttl, btr.LT + ScreenSize.From(-3, -14));
            }
            btr = btr + ScreenX.From(50);

            // title filter buttons
            var btn1 = Assets.Image("btnLogPanel");
            var btn0 = Assets.Image("btnLogPanelOff");
            if (btn1 != null && btn0 != null)
            {
                var ctfb = new CanvasTextFormat
                {
                    FontFamily = "Arial",
                    FontSize = 10.0f,
                    FontWeight = FontWeights.Normal,
                    FontStyle = FontStyle.Italic,
                };
                foreach ((var lv, var caption) in new (LLV, string)[] { (LLV.ERR, "e"), (LLV.WAR, "w"), (LLV.INF, "i"), (LLV.DEV, "d") })
                {
                    var bpos = btr.LT + ScreenSize.From(-3, 1);
                    btnAreas[lv] = ScreenRect.From(bpos, 16, 16);
                    dp.Graphics.DrawImage(visibles[lv] ? btn1 : btn0, bpos);
                    dp.Graphics.DrawText(caption, btr.LT + ScreenSize.From(1, 1), ColorUtil.ChangeAlpha(GetLevelColor(lv), 0.5f), ctfb);
                    btr = btr + ScreenX.From(18);
                }
            }

            return ScreenY.From(titleHeight + 16);
        }

        /// <summary>
        /// flag to fit align bottom
        /// </summary>
        protected virtual bool isAlignBottom => false;

        /// <summary>
        /// draw log message
        /// </summary>
        /// <param name="dp"></param>
        /// <param name="sr"></param>
        /// <param name="theight">タイトルバーの高さ</param>
        protected virtual void drawMessage(DrawProperty dp, ScreenRect sr, ScreenY theight)
        {
            var dupcheck = new Dictionary<string, LogAccessor.RecordStr>();
            sr.Deflate(ScreenSize.From(2, 2));  // sr = text drawing area
            sr.LT = ScreenPos.From(sr.LT.X + ScreenX.From(20), sr.LT.Y + theight);
            var ctfm = new CanvasTextFormat
            {
                FontFamily = "Meiryo UI",
                FontSize = 11,
                FontWeight = FontWeights.Normal,
            };
            float lm = 2;   // space between lines
            var y1 = 16 + lm;
            var dispLineN = (float)Math.Floor(sr.Height / y1);
            float btY;

            float curN = 0; // number of lines
            for (var lu = LOG.Queue.Last; lu != null; lu = lu.Previous)
            {
                if (visibles[lu.Value.Level] == false)
                {
                    continue;
                }
                var s = lu.Value.Message;
                if (lu.Value.IsSolo)
                {
                    if (dupcheck.ContainsKey(s))
                    {
                        continue;
                    }
                    else
                    {
                        dupcheck[s] = lu.Value;
                    }
                }
                curN++;
                if (curN > dispLineN)
                {
                    break;
                }
            }
            dupcheck.Clear();

            if (curN < dispLineN && isAlignBottom == false)
            {
                btY = sr.RB.Y - (dispLineN - curN + 1) * y1;
            }
            else
            {
                btY = sr.RB.Y - y1;
            }

            var lastIndex = 0;
            for (var lu = LOG.Queue.Last; lu != null; lu = lu.Previous)
            {
                if (btY < sr.LT.Y - 2 || lu == null)
                {
                    break;
                }
                if (visibles[lu.Value.Level] == false)
                {
                    continue;
                }
                var s = lu.Value.Message;
                if (lu.Value.IsSolo)
                {
                    if (dupcheck.ContainsKey(s))
                    {
                        continue;
                    }
                    else
                    {
                        dupcheck[s] = lu.Value;
                    }
                }
                if (lu.Value.ExtendDesign is LogAccessor.Image icon)
                {
                    var img0 = Assets.Image(icon.Key);
                    if (img0 != null)
                    {
                        var srcrect = ScreenRect.FromLTWH(0, 0, (float)img0.Size.Width, (float)img0.Size.Height);
                        dp.Graphics.DrawImage(img0, sr.LT.X - ScreenX.From(18), btY, _(srcrect), getIconOpacity(lu.Value.Level, lastIndex, (int)curN));
                    }
                }
                dp.Graphics.DrawText(s, sr.LT.X, btY, getMessageColor(lu.Value.Level, lastIndex, (int)curN), ctfm);
                dp.Graphics.DrawLine(sr.LT.X, btY + y1 - 2, sr.RB.X.Sx, btY + y1 - 2, Color.FromArgb(32, 0, 0, 0));
                btY -= y1;
                lastIndex++;
            }
        }

        /// <summary>
        /// get message color
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="lastIndex">0=last line</param>
        /// <param name="maxIndex">top line</param>
        /// <returns></returns>
        protected virtual Color getMessageColor(LLV lv, int lastIndex, int maxIndex)
        {
            return GetLevelColor(lv);
        }

        /// <summary>
        /// get icon opacity
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="lastIndex"></param>
        /// <param name="maxIndex"></param>
        /// <returns></returns>
        protected virtual float getIconOpacity(LLV lv, int lastIndex, int maxIndex)
        {
            return 1.0f;
        }

        /// <summary>
        /// make log level color
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public Color GetLevelColor(LLV level)
        {
            switch (level)
            {
                case LLV.WAR:
                    return _parent.MessageColorWar;
                case LLV.ERR:
                    return _parent.MessageColorErr;
                case LLV.DEV:
                    return _parent.MessageColorDev;
                default:
                    return _parent.MessageColorInf;
            }
        }
    }
}
