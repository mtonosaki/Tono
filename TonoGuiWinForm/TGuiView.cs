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
    /// �t�B�[�`���[�쓮�̊�{�R���g���[��
    /// �e��C�x���g���t�B�[�`���[�N���X�ɓ]���ł���HMI
    /// </summary>
    public partial class TGuiView : ContainerControl, IDisposable, IRichPane, IRichPaneSync, IKeyListener, IControlUI
    {
        #region �����i�V���A���C�Y����j

        /// <summary>���[�g�t�B�[�`���[�O���[�v</summary>
        private FeatureGroupRoot _rootGroup = null;

        /// <summary>�Y�[���{�� * 1000[%]</summary>
        private XyBase _zoom = XyBase.FromInt(1000, 1000);

        /// <summary>�X�N���[����</summary>
        private ScreenPos _scroll = ScreenPos.FromInt(0, 0);

        /// <summary>�p�[�c���o�^����Ă��Ȃ����b�`�y�[���̔w�i��h��Ԃ����ǂ����Btrue�ɂ���ƃ`���c�L�̌����ɂȂ邩��</summary>
        private bool _isDrawEmptyBackground = true;


        #endregion
        #region �����i�V���A���C�Y���Ȃ��j

        /// <summary>Paint�C�x���g�ŐV��Graphic�I�u�W�F�N�g</summary>
        private Graphics _currentGraphics;
        /// <summary>Paint�C�x���g�ŐV�̃y�C���g�̈�</summary>
        private ScreenRect _currentPaintClip;
        /// <summary>���O��uMouseState.Buttons���L������</summary>
        private readonly MouseState.Buttons _mouseStateButtons = new MouseState.Buttons(false, false, false, false, false);
        /// <summary>�h���b�O���h���b�v�̏��</summary>
        private DragState _DragState = null;

        /// <summary>�}�E�X�̋O�Փ���`�悷�邽�߂̃r�b�g�}�b�v���C���[</summary>
        private readonly ArrayList _freeLayers = new ArrayList();

        /// <summary>�L�[Enabler���o���Ă���</summary>
        private Tono.GuiWinForm.TKeyEnabler _KeyEnabler = null;

        /// <summary>�`�撆�t���O</summary>
        private bool _isDrawing = false;
        #endregion

        /// <summary>
        /// �\�z�p�R�[�h
        /// </summary>
        private void _constract()
        {
            // �t�B�[�`���[�A�[�L�e�N�`��
            _rootGroup = new FeatureGroupRoot(this);

            // ���̌Ăяo���́AWindows.Forms �t�H�[�� �f�U�C�i�ŕK�v�ł��B
            InitializeComponent();
            var doubleBuffer = ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer;
            var hmi = ControlStyles.UserMouse;
            var etc = ControlStyles.Selectable | ControlStyles.Opaque | ControlStyles.ResizeRedraw;
            SetStyle(doubleBuffer | hmi | etc, true);

            AllowDrop = true;  // �R���g���[�����h���b�O���h���b�v���󂯕t����悤�ɂ���
        }

        /// <summary>
        /// �f�t�H���g�R���X�g���N�^
        /// </summary>
        public TGuiView()
        {
            _constract();
        }

        /// <summary>
        /// �R���e�i��Ԃ�
        /// </summary>
        /// <returns></returns>
        public IContainer GetContainer()
        {
            return components;
        }

        /// <summary>
        /// �R���e�i�w��̃R���X�g���N�^
        /// </summary>
        /// <param name="container"></param>
        public TGuiView(IContainer container)
        {
            container.Add(this);
            _constract();
        }

        #region IDisposable �����o

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
        /// ��������Graphics�C���X�^���X���擾����
        /// </summary>
        /// <returns></returns>

        public Graphics GetCurrentGraphics()
        {
            return _currentGraphics;
        }

        /// <summary>
        /// �t���[���C���[����ёւ����r�N���X
        /// </summary>
        private class FreeLayerComparer : IComparer
        {
            #region IComparer �����o

            public int Compare(object x, object y)
            {
                return ((FreeDrawLayer)x).Level - ((FreeDrawLayer)y).Level;
            }

            #endregion
        }

        /// <summary>
        /// �������g�������t���[���C���[��o�^����
        /// </summary>
        /// <param name="layerLevel">���C���[�ԍ��B�������قǉ���</param>
        /// <returns>Graphics�I�u�W�F�N�g</returns>
        public FreeDrawLayer AddFreeLayer(int layerLevel)
        {
            var fl = new FreeDrawLayer(this, layerLevel);
            _freeLayers.Add(fl);
            _freeLayers.Sort(new FreeLayerComparer());
            return fl;
        }

        /// <summary>
        /// �����񐶐�
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{{FeatureRich={0}}}", IdText);
        }

        /// <summary>
        /// �t�H�[����OnLoad�ł܂��ŏ��ɂ��̃��\�b�h���R�[�����Ȃ���΂Ȃ�Ȃ�
        /// ����ݒ���O�ɃR�[�����邱��
        /// </summary>
        /// <returns>���[�g�t�B�[�`���[�O���[�v�̎Q��</returns>
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
        /// �t�@�C�������w�肵�ăt�H�[����OnLoad��
        /// �܂��ŏ��ɂ��̃��\�b�h���R�[�����Ȃ���΂Ȃ�Ȃ�
        /// ����ݒ���O�ɃR�[�����邱��
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
        /// ���[�g�O���[�v���擾����
        /// </summary>
        /// <returns>�֘A�Â����Ă��郋�[�g�O���[�v</returns>
        public FeatureGroupRoot GetFeatureRoot()
        {
            return _rootGroup;
        }

        /// <summary>
        /// �p�[�c�������Ă��w�i��`�悷�邩�ǂ����̃t���O
        /// ����́Afalse�ɂ��Ă����ƃp�t�H�[�}���X�����シ�邪�A�p�[�c���z�u����Ȃ��y�[���͐^�����ɂȂ�
        /// true�ɂ��Ă����ƁA�w�i�F�Ńy�C���g����鏈�����ǉ������
        /// </summary>
        [Category("Tono.GuiWinForm")]
        [Description("�p�[�c�������Ă��w�i��`�悷�邩�ǂ����̃t���O")]
        public bool IsDrawEmptyBackground
        {

            get => _isDrawEmptyBackground;

            set => _isDrawEmptyBackground = value;
        }

        /// <summary>
        /// �}�E�X�ړ��C�x���g�̏���
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

            // �N���b�N�����y�[����{��
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
                #region �_�C�A���O�\���ɂ���Ă��������Ȃ����L�[�X�e�[�^�X�����ɖ߂�����(�}�E�X���[�u�̃t�B�[�����O�����������Ȃ邩�॥�)
                var ke = new KeyEventArgs(Keys.None);
                // �L�����Ă���L�[�������ɃC�x���gArgs�𐶐�
                if (_mouseStateButtons.IsShift)
                {
                    ke = new KeyEventArgs(ke.KeyData | Keys.Shift);
                }

                if (_mouseStateButtons.IsCtrl)
                {
                    ke = new KeyEventArgs(ke.KeyData | Keys.Control);
                }
                // ke���f�t�H���g�Ƃ��ċL��
                var defE = new KeyEventArgs(ke.KeyData);
                // ���݂̃L�[�̏�Ԃ𒲍�
                if (((System.Windows.Forms.Form.ModifierKeys & Keys.Shift) == Keys.Shift) != _mouseStateButtons.IsShift)
                {   // Shift��������Ă��܂��Ă����ꍇ
                    ke = new KeyEventArgs(ke.KeyData ^ Keys.Shift);
                }
                if ((System.Windows.Forms.Form.ModifierKeys & Keys.Control) == Keys.Control != _mouseStateButtons.IsCtrl)
                {   // Ctrl���b����Ă��܂��Ă����ꍇ
                    ke = new KeyEventArgs(ke.KeyData ^ Keys.Control);
                }
                // �L�����Ă���L�[���ƌ���̃L�[��񂪈���Ă����ꍇ
                if (ke.KeyData != defE.KeyData)
                {
                    _KeyEnabler.KickKeyUp(ke);
                }
                #endregion
            }

            // �C�x���g���΂�
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
        /// �}�E�X�{�^���_�E���C�x���g�̏���
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

            // �N���b�N�����y�[����{��
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
            // �C�x���g���΂�
            var ma = MouseState.FromMouseEventArgs(e, pane);

            //�}�E�X�{�^���̏�Ԃ��L������
            _mouseStateButtons.IsButton = ma.Attr.IsButton;
            _mouseStateButtons.IsButtonMiddle = ma.Attr.IsButtonMiddle;

            // uMouseState.FromMouseEventArgs�őΉ��ł��Ȃ��������̑����𔽉f����
            ma.Attr.SetKeyFrags(_mouseStateButtons);
            _rootGroup.OnMouseDown(ma);
        }

        /// <summary>
        /// �}�E�X�{�^���A�b�v�C�x���g�̏���
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

            // �N���b�N�����y�[����{��
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
            // �C�x���g���΂�
            var ma = MouseState.FromMouseEventArgs(e, pane);
            ma.Attr.SetKeyFrags(_mouseStateButtons);
            _rootGroup.OnMouseUp(ma);

            //�}�E�X�{�^���̏�Ԃ��L������
            if (ma.Attr.IsButton)
            {
                _mouseStateButtons.IsButton = false;
            }
            if (ma.Attr.IsButtonMiddle)
            {
                _mouseStateButtons.IsButtonMiddle = false;
            }

            // �R���e�L�X�g���j���[���o�^����Ă�����\��������
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
        /// �}�E�X�z�C�[���̃C�x���g
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

            // �N���b�N�����y�[����{��
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
            // �C�x���g���΂�
            var ma = MouseState.FromMouseEventArgs(e, pane);
            ma.Attr.SetKeyFrags(_mouseStateButtons);

            _rootGroup.OnMouseWheel(ma);
            //base.OnMouseWheel (e);
        }

        /// <summary>
        /// �A�C�e�����h���b�O����Ă������̃C�x���g(�]���͂��Ȃ�)
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
        /// �����s�Ȃ� true
        /// </summary>
        /// <returns></returns>
        private bool _cannotEventProc()
        {
            if (_isDrawing)
            {
                System.Diagnostics.Debug.WriteLine("�I�H�@�`�撆�ɕ`��C�x���g on " + GetType().Name);
                return true;
            }
            return false;
        }

        /// <summary>
        /// �A�C�e�����h���b�v���ꂽ���̃C�x���g
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

            // �}�E�X�J�[�\���̃X�N���[�����W�˃t�H�[���̃N���C�A���g���W���Z�o
            var p = PointToClient(MouseState.NowPosition);
            // �h���b�v���ꂽ�y�[����{��
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
        /// �`��C�x���g����
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
                System.Diagnostics.Debug.WriteLine("�I�H�@�`�撆�ɕ`��C�x���g on " + GetType().Name);
                return;
            }
            _isDrawing = true;
            _currentGraphics = e.Graphics;
            _currentPaintClip = ScreenRect.FromRectangle(e.ClipRectangle);

            if (_rootGroup != null)
            {
                // �K�v�ł���Δw�i��`��
                if (IsDrawEmptyBackground)
                {
                    using Brush brush = new SolidBrush(BackColor);
                    e.Graphics.FillRectangle(brush, e.ClipRectangle);
                }
                // �p�[�c���̕`��
                var pb = _rootGroup.GetPartsSet();
                if (pb != null)
                {
                    pb.CheckAndResetLocalized();
                    pb.ProvideDrawFunction();

                    // �t���[���C���[��`��
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
        /// �T�C�Y�ύX���Ƀ��C�A�E�g�𒲐�����
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

        #region IRichPane �����o


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
        /// ���O�Ńy�[������������
        /// </summary>
        /// <param name="tar">�����y�[���̊K�w���݈ʒu</param>
        /// <param name="name">����Name</param>
        /// <returns>���������y�[�� / null = ���̊K�w�ɂ͌�����Ȃ�����</returns>
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
        /// �}�X�^�[�y�[���̗̈��Ԃ�
        /// </summary>
        /// <returns></returns>

        public ScreenRect GetPaneRect()
        {
            return ScreenRect.FromLTWH(0, 0, Width, Height);
        }

        /// <summary>
        /// �`�悪�K�v�ȗ̈��Ԃ��C���^�[�t�F�[�X
        /// </summary>
        /// <returns>�̈�</returns>

        public ScreenRect GetPaintClipRect()
        {
            return _currentPaintClip;
        }

        /// <summary>
        /// �X�N���[����
        /// </summary>
        [Category("Tono.GuiWinForm")]
        [Description("�X�N���[����")]
        public new ScreenPos Scroll    // new ���ʎq�́A.NET2.0�ŕK�v
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
        /// �Y�[���{�� * 10[%]
        /// </summary>
        [Category("Tono.GuiWinForm")]
        [Description("���݂̃Y�[���l * 10[%]")]
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
        /// �Y�[���ύX�C�x���g�𑗐M����
        /// </summary>
        /// <param name="rp">�Y�[�������y�[���i�C�x���g���茳�j</param>
        public void SendZoomChangedEvent(IRichPane rp)
        {
            _rootGroup.ZoomChanged(rp);
        }

        /// <summary>
        /// �X�N���[���ύX�C�x���g�𑗐M����
        /// </summary>
        /// <param name="rp">�X�N���[�������y�[���i�C�x���g���茳�j</param>
        public void SendScrollChangedEvent(IRichPane rp)
        {
            _rootGroup.ScrollChanged(rp);
        }

        /// <summary>
        /// �Y�[���̒l���`�F�b�N���ĕs���l�Ȃ璲������
        /// </summary>
        /// <param name="value">��]����Y�[���l</param>
        /// <returns>�ӂ��킵���������ꂽ�Y�[���l</returns>
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
        /// �O���t�B�b�N�I�u�W�F�N�g
        /// </summary>
        [Category("Tono.GuiWinForm")]
        [Description("�`��p��Graphic�I�u�W�F�N�g")]
        public System.Drawing.Graphics Graphics => _currentGraphics;

        /// <summary>
        /// ��ʂ��ĕ`�悷��
        /// </summary>
        /// <param name="rect">�ĕ`�悷��͈�</param>
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

        #region IRichPaneSync �����o

        /// <summary>
        /// �Y�[���{�� * 10[%]
        /// </summary>
        [Category("Tono.GuiWinForm")]
        [Description("���݂̃Y�[���l * 10[%]")]
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
        /// �X�N���[����
        /// </summary>
        [Category("Tono.GuiWinForm")]
        [Description("���݂̃X�N���[����")]
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

        #region IKeyListener �����o�ifiKeyEnabler�R���|�[�l���g���烁�b�Z�[�W���󂯂�j

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
        /// �L�[�C�x���g���č\�z���āA�K�v�ł���΃C�x���g���o�͂���
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
