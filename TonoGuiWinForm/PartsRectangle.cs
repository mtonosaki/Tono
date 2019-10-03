using System;
using System.Drawing;
using System.Runtime.Serialization;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �l�p�`�̕`��I�u�W�F�N�g
    /// </summary>
    [Serializable]
    public class PartsRectangle : PartsBase, ICloneable, ISerializable
    {
        #region		����(�V���A���C�Y����)
        /** <summary>���̐F</summary> */
        protected Color _penColor = Color.Black;
        protected float _lineWidth = 1;
        protected int _minFontVisibleZoom = 250;

        /** <summary>�e�L�X�g�̐F</summary> */
        protected Color _textColor = Color.Black;

        /** <summary>�c����</summary> */
        private bool _isVertText;
        /** <summary>�t�H���g</summary> */
        protected string _fontFace = "Arial";
        /** <summary>�t�H���g�T�C�Y</summary> */
        protected float _fontSize = 9.0f;
        /// <summary>�����c���ݒ�@�\</summary>
        private bool _isAutoVertical = false;
        /// <summary>
        /// �}�[�W���i�S�����j
        /// </summary>
        public int _margin = 0;

        /// <summary>
        /// �N���b�v�����𖳎�����
        /// </summary>
        public bool IsClipDraw { get; set; }

        #endregion

        #region ISerializable �����o
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            SerializerEx.GetObjectData(typeof(PartsRectangle), this, info, context, false);
        }
        protected PartsRectangle(SerializationInfo info, StreamingContext context) : base(info, context)

        {
            SerializerEx.Instanciate(typeof(PartsRectangle), this, info, context, false);
        }

        #endregion
        #region		����(�V���A���C�Y���Ȃ�)

        [NonSerialized] protected ScreenRect spos = null;
        [NonSerialized] protected StringFormat _sf = new StringFormat(); // �e�L�X�g�̃t�H�[�}�b�g���쐬

        #endregion

        #region ICloneable �����o

        public override object Clone()
        {
            var ret = (PartsRectangle)base.Clone();
            ret._textColor = _textColor;
            ret._penColor = _penColor;
            ret._lineWidth = _lineWidth;
            ret._isVertText = _isVertText;
            ret._fontFace = _fontFace;
            ret._fontSize = _fontSize;
            ret._isAutoVertical = _isAutoVertical;
            ret.IsClipDraw = IsClipDraw;
            return ret;
        }

        #endregion

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public PartsRectangle()
        {
            FontFace = "MS UI Gothic";
            StringAlignment = StringAlignment.Center;
            LineAlignment = StringAlignment.Center;
            StringTrimming = StringTrimming.EllipsisPath;
            IsClipDraw = true;
        }

        /// <summary>
        /// �ŏ��t�H���g�T�C�Y
        /// </summary>
        public float MinimunFontSize { get; set; }

        /// <summary>
        /// �ő�t�H���g�T�C�Y
        /// </summary>
        public float MaximumFontSize { get; set; }

        /// <summary>
        /// �����c�����ݒ�
        /// </summary>
        public bool IsAutoVerticalText
        {
            get => _isAutoVertical;
            set => _isAutoVertical = value;
        }

        /// <summary>
        /// �e�L�X�g�}�[�W���i�S�����j
        /// </summary>
        public int TextMargin
        {
            get => _margin;
            set => _margin = value;
        }

        /// <summary>
        /// �����c�����v���p�e�B�ύX
        /// </summary>
        protected void checkAutoVert(IRichPane rp)
        {
            if (IsAutoVerticalText)
            {
                if (spos == null)
                {
                    spos = GetScRect(rp);
                }

                if (spos.Height > spos.Width)
                {
                    IsVerticalText = true;
                }
                else
                {
                    IsVerticalText = false;
                }
            }
        }

        /// <summary>
        /// ������̉��z�u
        /// </summary>
        protected StringAlignment StringAlignment { get; set; }

        /// <summary>
        /// ������̏c�z�u
        /// </summary>
        protected StringAlignment LineAlignment { get; set; }

        /// <summary>
        /// ������g���~���O�ݒ�
        /// </summary>
        protected StringTrimming StringTrimming { get; set; }

        /// <summary>
        /// �`��
        /// </summary>
        /// <param name="rp">�`�搧��n���h��</param>
        public override bool Draw(IRichPane rp)
        {
            spos = GetScRect(rp);

            if (isInClip(rp, spos) == false && IsClipDraw)  // �`��s�v�ł���΁A�Ȃɂ����Ȃ�
            {
                return false;
            }

            using (var pen = new Pen(_penColor, _lineWidth))
            {
                rp.Graphics.DrawRectangle(pen, spos.LT.X, spos.LT.Y, spos.Width, spos.Height); // ��`��`��
            }
            if (rp.Zoom > XyBase.FromInt(_minFontVisibleZoom, _minFontVisibleZoom))    // �k�������Q�T������������e�L�X�g�̕\���͂��Ȃ�
            {
                _sf.Alignment = StringAlignment;
                _sf.LineAlignment = LineAlignment;
                _sf.Trimming = StringTrimming;
                if (IsVerticalText)
                {
                    _sf.FormatFlags = StringFormatFlags.DirectionVertical;
                }
                using (var font = new Font(_fontFace, Math.Min(MaximumFontSize, Math.Max(MinimunFontSize, _fontSize * rp.Zoom.Y / 1000))))
                { // �t�H���g���쐬
                    var sposm = (ScreenRect)spos.Clone();
                    sposm.Deflate(_margin);
                    rp.Graphics.DrawString(Text, font, new SolidBrush(_textColor), sposm, _sf); // �e�L�X�g��`��
                }
            }

            drawSelected(rp);   // �I����Ԃ�`��
            return true;
        }


        private static readonly Pen _highlightpen2 = new Pen(Color.FromArgb(128, 255, 255, 224));
        private static readonly Pen _highlightpen3 = new Pen(Color.FromArgb(64, 255, 255, 192));

        /// <summary>
        /// �I����Ԃ̕W�������i�eDraw�ŃR�[�����邩�A�Ǝ��ɑI����Ԃ��������邱�Ɓj
        /// </summary>
        /// <param name="rp">���b�`�y�[��</param>
        protected override void drawSelected(IRichPane rp)
        {
            if (this is IPartsSelectable)
            {
                if (((IPartsSelectable)this).IsSelected)
                {
                    rp.Graphics.DrawRectangle(Pens.White, spos);
                    var rr = (ScreenRect)spos.Clone();
                    rr.Deflate(XyBase.FromInt(1, 1));
                    rp.Graphics.DrawRectangle(_highlightpen2, rr);
                    rr.Deflate(XyBase.FromInt(1, 1));
                    rp.Graphics.DrawRectangle(_highlightpen3, rr);
                }
            }
        }

        /// <summary>
        /// �c�����̎擾/�ݒ�
        /// </summary>
        public bool IsVerticalText
        {
            get => _isVertText;
            set => _isVertText = value;
        }

        /// <summary>
        /// ���̐F�̎擾/�ݒ�
        /// </summary>
        public Color LineColor
        {
            get => _penColor;
            set => _penColor = value;
        }

        /// <summary>
        /// ���̑����̎擾/�ݒ�
        /// </summary>
        public float LineWidth
        {
            get => _lineWidth;
            set => _lineWidth = value;
        }

        /// <summary>
        /// �e�L�X�g�̐F�̎擾/�ݒ�
        /// </summary>
        public Color TextColor
        {
            get => _textColor;
            set => _textColor = value;
        }

        /// <summary>
        /// �t�H���g�̎擾/�ݒ�
        /// </summary>
        public string FontFace
        {
            get => _fontFace;
            set => _fontFace = value;
        }

        /// <summary>
        /// �t�H���g�T�C�Y�̎擾/�ݒ�
        /// </summary>
        public float FontSize
        {
            get => _fontSize;
            set => _fontSize = value;
        }
    }
}
