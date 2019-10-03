using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// IRichPane���������鉼�z���b�`�y�[��
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
        /// ���������p
        /// </summary>
        protected RichPaneBinder()
        {
        }

        /// <summary>
        /// �Q�Ɨp�ɃC���X�^���X�𐶐�����i�C�����[�W�����p�j
        /// </summary>
        /// <param name="parent"></param>
        public RichPaneBinder(IRichPane parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// �V���A���C�Y�p�Ɏ��̂Ƃ��ăC���X�^���X�𐶐�����(_parent�͎g�p���Ȃ����[�h�j
        /// </summary>
        /// <param name="value">���ɂȂ�IRichPane</param>
        /// <returns>�V�����C���X�^���X</returns>
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
        /// �V���A���C�Y�p�Ɏ��̂Ƃ��ăC���X�^���X�𐶐�����(_parent�̎Q�Ƃ��R�s�[����j
        /// </summary>
        /// <param name="value">���ɂȂ�IRichPane</param>
        /// <returns>�V�����C���X�^���X</returns>
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

        #region IRichPane �����o

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
