// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// IRichPaneを実装する仮想リッチペーン
    /// </summary>
    public class RichPaneBinder : IRichPane
    {
        protected IRichPane _parent;
        protected string _idtext;
        protected XyBase _zoom = new XyBase();
        protected ScreenPos _scroll = new ScreenPos();
#if DEBUG
        public string _ => "IdText = " + _idtext;
#endif

        /// <summary>
        /// 内部処理用
        /// </summary>
        protected RichPaneBinder()
        {
        }

        /// <summary>
        /// 参照用にインスタンスを生成する（イリュージョン用）
        /// </summary>
        /// <param name="parent"></param>
        public RichPaneBinder(IRichPane parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// シリアライズ用に実体としてインスタンスを生成する(_parentは使用しないモード）
        /// </summary>
        /// <param name="value">元になるIRichPane</param>
        /// <returns>新しいインスタンス</returns>
        public static RichPaneBinder CreateCopy(IRichPane value)
        {
            var ret = new RichPaneBinder
            {
                _parent = null,
                _zoom = value.Zoom,
                _scroll = value.Scroll,
                _idtext = value.IdText
            };
            return ret;
        }

        /// <summary>
        /// シリアライズ用に実体としてインスタンスを生成する(_parentの参照もコピーする）
        /// </summary>
        /// <param name="value">元になるIRichPane</param>
        /// <returns>新しいインスタンス</returns>
        public static RichPaneBinder CreateCopyComplete(IRichPane value)
        {
            var ret = new RichPaneBinder
            {
                _parent = value,
                _zoom = value.Zoom,
                _scroll = value.Scroll,
                _idtext = value.IdText
            };
            return ret;
        }

        #region IRichPane メンバ

        public string IdText
        {
            get => _idtext;
            set => _idtext = value;
        }

        public Control Control => _parent.Control;

        public IRichPane GetParent()
        {
            return _parent.GetParent();
        }

        public IRichPane GetPane(string name)
        {
            return _parent.GetPane(name);
        }

        public ScreenRect GetPaneRect()
        {
            return _parent.GetPaneRect();
        }

        public ScreenRect GetPaintClipRect()
        {
            return _parent.GetPaintClipRect();
        }

        public void Invalidate(ScreenRect rect)
        {
            _parent.Invalidate(rect);
        }

        public XyBase Zoom
        {
            get => _zoom;
            set => _zoom = value;
        }

        public ScreenPos Scroll
        {
            get => _scroll;
            set => _scroll = value;
        }

        public virtual System.Drawing.Graphics Graphics => _parent.Graphics;

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
            XyBase ret = (value * _zoom / 1000) + _scroll + GetPaneRect().LT;
            return ScreenPos.FromInt(ret.X, ret.Y);
        }
        public ScreenRect Convert(LayoutRect value)
        {
            var ret = (value * _zoom / 1000) + _scroll + GetPaneRect().LT;
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

        #endregion
    }
}
