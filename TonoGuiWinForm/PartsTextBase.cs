using System;
using System.Drawing;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �\���E�ҏW�̂��߂́A�e�L�X�g��{�N���X
    /// �w�i�h��Ԃ��́A�h���N���X�ōs�����Ɓi�f�U�C���_��̂��߁j
    /// ���̃N���X�́A�e�L�X�g�\���������Ȃ��B
    /// </summary>
    public abstract class PartsTextBase : PartsBase, IPartsSelectable
    {
        #region �����i�V���A���C�Y����j
        private string _fontName;
        private float _fontSize;
        private Color _fontColor;
        private bool _isVertText;
        protected Color _bgColor;
        protected Color _lineColor;
        private bool _isSelected;
        private bool _isItalic;
        private bool _isBold;
        private ImeMode _imeMode;
        private bool _isMultiLine;
        #endregion
        #region �����i�V���A���C�Y���Ȃ��j
        [NonSerialized]
        protected StringFormat _sf = new StringFormat(); // �e�L�X�g�̃t�H�[�}�b�g���쐬
        [NonSerialized]
        private Font _lastFont = null;

        #endregion

        #region �v���p�e�B

        /// <summary>
        /// �e�L�X�g�}�[�W���iTop�j
        /// </summary>
        public int MarginTop { get; set; }

        /// <summary>
        /// �e�L�X�g�}�[�W���iLeft�j
        /// </summary>
        public int MarginLeft { get; set; }

        /// <summary>
        /// �t�H���g�T�C�Y
        /// </summary>
        public float FontSize
        {
            get => _fontSize;
            set => _fontSize = value;
        }

        public bool IsItalic
        {
            get => _isItalic;
            set => _isItalic = value;
        }

        public bool IsBold
        {
            get => _isBold;
            set => _isBold = value;
        }

        public ImeMode ImeMode
        {
            get => _imeMode;
            set => _imeMode = value;
        }

        /// <summary>
        /// �t�H���g��
        /// </summary>
        public string FontName
        {
            get => _fontName;
            set => _fontName = value;
        }

        /// <summary>
        /// �Ō�ɕ\�������t�H���g��Ԃ�
        /// </summary>
        public Font LastFont
        {
            get => _lastFont;
            set => _lastFont = value;
        }

        /// <summary>
        /// �w�i�F
        /// </summary>
        public Color BackColor
        {
            get => _bgColor;
            set => _bgColor = value;
        }

        /// <summary>
        /// ���̐F
        /// </summary>
        public Color LineColor
        {
            get => _lineColor;
            set => _lineColor = value;
        }

        /// <summary>
        /// �t�H���g�̐F
        /// </summary>
        public Color FontColor
        {
            get => _fontColor;
            set => _fontColor = value;
        }

        /// <summary>
        /// ���s�������邩�H
        /// </summary>
        public bool IsMultiLine
        {
            get => _isMultiLine;
            set => _isMultiLine = value;
        }
        #endregion

        /// <summary>
        /// ��{�N���X�̃R���X�g���N�^
        /// </summary>
        protected PartsTextBase()
        {
            _fontName = "MS UI Gothic";
            _fontSize = 9.0f;
            _fontColor = Color.Black;
            _bgColor = Color.FromArgb(192, 224, 192);
            _lineColor = Color.FromArgb(96, 192, 96);
            _isVertText = false;
            _isSelected = false;
            _imeMode = ImeMode.NoControl;
            _isMultiLine = false;
            TextAlignHorz = StringAlignment.Near;
            TextAlignVert = StringAlignment.Near;
        }

        /// <summary>
        /// GDI�J��
        /// </summary>
        public override void Dispose()
        {
            if (_lastFont != null)
            {
                _lastFont.Dispose(); _lastFont = null;
            }
            base.Dispose();
        }

        /// <summary>
        /// �e�L�X�g�̕\���ʒu�i�����j
        /// </summary>
        public StringAlignment TextAlignHorz { get; set; }

        /// <summary>
        /// �e�L�X�g�̕\���ʒu�i�����j
        /// </summary>
        public StringAlignment TextAlignVert { get; set; }

        /// <summary>
        /// �c�����ɉ�]�H
        /// </summary>
        public bool IsDirectionVertical { get; set; }

        /// <summary>
        /// �`��I�[�o�[���C�h
        /// </summary>
        /// <param name="rp"></param>
        /// <returns></returns>
        public override bool Draw(IRichPane rp)
        {
            var spos = GetScRect(rp);
            if (isInClip(rp, spos) == false)    // �`��s�v�ł���΁A�Ȃɂ����Ȃ�
            {
                return false;
            }


            // �e�L�X�g�`��
            var fsize = GetPointFromPoint(_fontSize, rp);
            if (fsize > 3)
            {
                _sf.Alignment = TextAlignHorz;
                _sf.LineAlignment = TextAlignVert;
                _sf.Trimming = StringTrimming.EllipsisPath;
                if (IsDirectionVertical)
                {
                    _sf.FormatFlags |= StringFormatFlags.DirectionVertical;
                }
                if (IsVerticalText)
                {
                    _sf.FormatFlags = StringFormatFlags.DirectionVertical;
                }
                rp.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                var style = (_isItalic ? FontStyle.Italic : 0) | (_isBold ? FontStyle.Bold : 0);
                if (_lastFont != null)
                {
                    _lastFont.Dispose();
                }
                if (_lastFont != null)
                {
                    _lastFont.Dispose();
                }
                _lastFont = new Font(_fontName, fsize, style);  // �t�H���g���쐬
                var sr = (ScreenRect)spos.Clone();
                sr.LT.X += MarginLeft;
                sr.LT.Y += MarginTop;
                using (Brush brush = new SolidBrush(_fontColor))
                {
                    rp.Graphics.DrawString(Text, _lastFont, brush, sr, _sf);   // �e�L�X�g��`��
                }
            }
            return true;
        }

        /// <summary>
        /// �Y�[�����l�������|�C���g�����v�Z
        /// </summary>
        /// <param name="point">���Ғl</param>
        /// <param name="rp">���b�`�y�[��</param>
        /// <returns>�Y�[�����l�������|�C���g��</returns>
        public float GetPointFromPoint(float point, IRichPane rp)
        {
            return (float)(point * GeoEu.Length(rp.Zoom.X, rp.Zoom.Y) / 1000 / Math.Sqrt(2));
        }

        /// <summary>
        /// �P�ʃ|�C���g����A�X�N���[�����W�n�̒l���擾����
        /// </summary>
        /// <param name="point">�|�C���g��</param>
        /// <returns>�X�N���[�����W�n�Ɋ��Z�����l</returns>
        public float GetScFromPoint(float point, IRichPane rp)
        {
            var p = CodeRect.FromLTRB((int)(GetMillimeterFromPoint(point) * 1000), 0, 0, 0);
            var lr = GetPtRect(p);
            return (float)(lr.LT.X * GeoEu.Length(rp.Zoom.X, rp.Zoom.Y) / 1000);
        }

        /// <summary>
        /// �|�C���g���~�����[�g���ɕϊ�����
        /// </summary>
        /// <param name="point">�|�C���g</param>
        /// <returns>�~�����[�g��</returns>
        public float GetMillimeterFromPoint(float point)
        {
            return 100f / 284.53f * point;
        }

        /// <summary>
        /// �c�����e�L�X�g���ǂ����̃t���O
        /// </summary>
        public bool IsVerticalText
        {
            get => _isVertText;
            set => _isVertText = value;
        }

        #region IPartsSelectable �����o

        public bool IsSelected
        {
            get => _isSelected;
            set => _isSelected = value;
        }

        #endregion

    }
}
