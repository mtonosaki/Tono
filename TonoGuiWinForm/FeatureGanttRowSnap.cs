// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeatureGanttRowSnap �̊T�v�̐����ł��B
    /// </summary>
    public class FeatureGanttRowSnap : FeatureBase, IMouseListener
    {
        #region �����i�V���A���C�Y���Ȃ��j

        /// <summary>�p�[�c�ʒu�Ǘ��I�u�W�F�N�g</summary>
        protected PartsPositionManager _pos;

        #endregion

        /// <summary>
        /// �������i���L�ϐ��̊����Ȃǁj
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();

            // �X�e�[�^�X����
            _pos = (PartsPositionManager)Share.Get("MovingParts", typeof(PartsPositionManager));    // �ړ����̃p�[�c�ꗗ
        }

        /// <summary>
        /// �}�E�X�ړ����h���b�O���Ƃ��Ď�������
        /// </summary>
        public void OnMouseMove(MouseState e)
        {
            if (_pos.Count > 0)
            {
                foreach (DictionaryEntry de in _pos)            // �I�𒆑S�p�[�c�ɑ΂��čs��
                {
                    var p3 = (PartsPositionManager.Pos3)de.Value;
                    // �c�ʒu��16�̔{���ɃX�i�b�v����T���v��
                    //p3.Now.LT.Y = (p3.Now.LT.Y) / 16 * 16;
                    //p3.Now.RB.Y = (p3.Now.RB.Y) / 16 * 16;

                    // �Ƃ肠�����c�����̈ړ��͂��Ȃ��B
                    //p3.Now.LT.Y = p3.Pre.LT.Y;
                    //p3.Now.RB.Y = p3.Pre.LT.Y;
                }
            }
        }
        #region IMouseListener �����o

        public void OnMouseDown(MouseState e)
        {
        }

        public void OnMouseUp(MouseState e)
        {
        }

        public void OnMouseWheel(MouseState e)
        {
        }


        #endregion
    }
}
