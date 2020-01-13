// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeaturePartsSelect �̊T�v�̐����ł��B
    /// </summary>
    public class FeaturePartsCrossMove : FeatureBase, IMouseListener
    {
        #region �����i�V���A���C�Y���Ȃ��j

        /// <summary>�p�[�c�ʒu�Ǘ��I�u�W�F�N�g</summary>
        protected PartsPositionManager _pos;
        private ScreenPos _clickPos = ScreenPos.FromInt(0, 0);
        private bool _isNoKey = false;  // Shift�L�[�������Ȃ��Ă��@�\���������ꍇ��True

        #endregion

        public FeaturePartsCrossMove()
        {
        }

        /// <summary>
        /// �p�����[�^�ǂݍ���
        /// </summary>
        /// <param name="param"></param>
        public override void ParseParameter(string param)
        {
            base.ParseParameter(param);
            if (param.Equals("KEYREVERSE", StringComparison.CurrentCultureIgnoreCase))
            {
                _isNoKey = true;
            }
        }

        /// <summary>
        /// ����������
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();

            // �X�e�[�^�X����
            _pos = (PartsPositionManager)Share.Get("MovingParts", typeof(PartsPositionManager));    // �ړ����̃p�[�c�ꗗ
        }

        /// <summary>
        /// �p�[�c�I���������}�E�X�_�E���C�x���g�ōs��
        /// </summary>
        public void OnMouseDown(MouseState e)
        {
            _clickPos = (ScreenPos)e.Pos.Clone();
        }

        /// <summary>
        /// �}�E�X�ړ��C�x���g�͕s�v
        /// </summary>
        public void OnMouseMove(MouseState e)
        {
            if (e.Attr.IsShift && _isNoKey == false || e.Attr.IsShift == false && _isNoKey)
            {
                int dx = 0, dy = 0;
                if (e != null)
                {
                    dx = Math.Abs(e.Pos.X - _clickPos.X);
                    dy = Math.Abs(e.Pos.Y - _clickPos.Y);
                }

                foreach (DictionaryEntry de in _pos)
                {
                    var parts = (PartsBase)de.Key;
                    var p3 = (PartsPositionManager.Pos3)de.Value;
                    if (dx > dy)
                    {
                        p3.Now.LT.Y = p3.Org.LT.Y;
                        p3.Now.RB.Y = p3.Org.RB.Y;
                    }
                    else
                    {
                        p3.Now.LT.X = p3.Org.LT.X;
                        p3.Now.RB.X = p3.Org.RB.X;
                    }
                }
            }
        }

        /// <summary>
        /// �}�E�X�A�b�v�C�x���g�͕s�v
        /// </summary>
        public void OnMouseUp(MouseState e)
        {
        }

        public void OnMouseWheel(MouseState e)
        {
        }
    }
}
