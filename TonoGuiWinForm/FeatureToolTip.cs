// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Drawing;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �c�[���`�b�v�̊Ǘ�
    /// </summary>
    public class FeatureToolTip : FeatureBase
    {
        #region	����(�V���A���C�Y����)
        /// <summary>�c�[���`�b�v��\������g���K</summary>
        protected MouseState.Buttons _trigger;

        #endregion

        #region	����(�V���A���C�Y���Ȃ�)

        /// <summary>�c�[���`�b�v�ɕ\�����镶����</summary>
        private readonly PartsTooltip _tt = new PartsTooltip();

        private IRichPane _rp;

        #endregion

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public FeatureToolTip()
        {
            // �f�t�H���g�Ńh���b�O�X�N���[�����邽�߂̃L�[��ݒ肷��
            _trigger = new MouseState.Buttons
            {
                IsButton = false,
                IsButtonMiddle = false,
                IsCtrl = false,
                IsShift = false
            };
        }

        /// <summary>
        /// �Ō�̈ʒu
        /// </summary>
        protected CodeRect LastRect
        {
            get
            {
                if (_tt == null)
                {
                    return null;
                }
                return _tt.Rect;
            }
        }

        /// <summary>
        /// �w�i�F���w��ł���
        /// </summary>
        protected Color BackColor
        {
            get => _tt.BackColor;
            set => _tt.BackColor = value;
        }

        /// <summary>
        /// �e�L�X�g�F���w��ł���
        /// </summary>
        protected Color TextColor
        {
            get => _tt.TextColor;
            set => _tt.TextColor = value;
        }

        /// <summary>
        /// ���A�C�R���̐F���w�肷��
        /// </summary>
        public Color SquareColor
        {
            set => _tt.SquareColor = value;
        }

        /// <summary>
        /// ����̃A�C�R��
        /// </summary>
        public Image LtImage
        {
            set => _tt.LtImage = value;
        }

        public override void OnInitInstance()
        {
            base.OnInitInstance();

            _rp = Pane.GetPane("Resource");
        }

        /// <summary>
        /// �\���ʒu�������ł��� / null = �}�E�X�̃J�[�\���t��
        /// </summary>
        public ScreenPos Position
        {
            set => _tt.Position = value;
        }

        /// <summary>
        /// ���C���[ID�̃I�[�o�[���[�h
        /// </summary>
        protected virtual int LayerID => Const.Layer.Tooltip;

        /// <summary>
        /// �\��������̎擾/�ݒ�
        /// </summary>
        public string Text
        {
            get => _tt.Text;
            set
            {
                if (_tt.Text != value)
                {
                    _tt.Text = value;
                    Parts.Clear(_rp, LayerID);
                    if (string.IsNullOrEmpty(_tt.Text) == false)
                    {
                        _tt.RequestPosition();
                        Parts.Add(_rp, _tt, LayerID);
                        //Parts.Invalidate(_tt, _rp);
                        Pane.Invalidate(null);
                    }
                    else
                    {
                        Pane.Invalidate(null);
                    }
                }
                else
                {
                    Pane.Invalidate(null);
                }
            }
        }
    }
}
