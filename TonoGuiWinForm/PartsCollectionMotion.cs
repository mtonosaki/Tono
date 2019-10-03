using System;
using System.Collections.Generic;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �p�[�c���[�V�����A�X�N���[�����[�V�����̃T�|�[�g�ł���p�[�c�Z�b�g
    /// </summary>
    public class PartsCollectionMotion : PartsCollection
    {
        /// <summary>
        /// �ړ���Ǝc�胂�[�V�����X�e�b�v��
        /// </summary>
        private class TargetPosition
        {
            public CodePos Pos = null;
            public int StepN = 20;

            /// <summary>
            /// �������R���X�g���N�^
            /// </summary>
            /// <param name="pos"></param>
            /// <param name="stepN"></param>
            public TargetPosition(CodePos pos, int stepN)
            {
                Pos = pos;
                StepN = stepN;
            }
        }

        /// <summary>
        /// �X�N���[���w��
        /// </summary>
        private class ScrollCommand : uCouple<ScreenPos, int/*curStepN*/>
        {
            public ScrollCommand(ScreenPos pos, int step)
                : base(pos, step)
            {
            }
        }

        /// <summary>
        /// �Y�[���w��
        /// </summary>
        private class ZoomCommand : uCouple<XyBase, int/*curStepN*/>
        {
            public ZoomCommand(XyBase pos, int step)
                : base(pos, step)
            {
            }
        }

        /// <summary>
        /// �ړ�����
        /// </summary>
        private readonly Dictionary<PartsBase, TargetPosition> _tarPos = new Dictionary<PartsBase, TargetPosition>();

        /// <summary>
        /// ���[�V�����X�N���[���̈ʒu
        /// </summary>
        private readonly Dictionary<IRichPane, ScrollCommand> _motionScroll = new Dictionary<IRichPane, ScrollCommand>();

        /// <summary>
        /// ���[�V�����X�N���[���̈ʒu
        /// </summary>
        private readonly Dictionary<IRichPane, ZoomCommand> _motionZoom = new Dictionary<IRichPane, ZoomCommand>();

        /// <summary>
        /// ���[�V�����Ώۂ̃y�[��
        /// </summary>
        private IRichPane _motionPane = null;

        /// <summary>
        /// ���[�V�����Ώۂ̃y�[�����w�肷��
        /// </summary>
        /// <param name="tar"></param>
        public void SetMotionPane(IRichPane tar)
        {
            _motionPane = tar;
        }

        /// <summary>
        /// ���[�V�������Ĉړ��������w�肷��
        /// </summary>
        /// <param name="nStep"></param>
        /// <param name="pos"></param>
        public void SetMotionPosition(PartsBase part, CodePos pos, int nStep)
        {
            lock (_tarPos)
            {
                _tarPos[part] = new TargetPosition(pos, nStep);
            }
        }

        /// <summary>
        /// �\������
        /// </summary>
        public override void ProvideDrawFunction()
        {
            var isMotionNeed = false;

            isMotionNeed = scroll1step();
            isMotionNeed |= zoom1step();

            base.ProvideDrawFunction();

            isMotionNeed |= move1step();

            if (isMotionNeed)
            {
                _motionPane.Invalidate(null);
            }
        }

        /// <summary>
        /// �Y�[���P�X�e�b�v���s
        /// </summary>
        /// <returns></returns>
        private bool zoom1step()
        {
            var dels = new List<IRichPane>();
            var isNeedRetry = false;
            foreach (var kv in _motionZoom)
            {
                isNeedRetry = true;

                var pane = kv.Key;
                var tarZoom = kv.Value.V1;

                var nowPos = pane.Zoom;

                if (--kv.Value.V2/*curStepN*/ <= 0)
                {
                    pane.Zoom = tarZoom;
                    dels.Add(pane);
                }
                else
                {
                    var diff = tarZoom - nowPos;
                    diff = diff / 4;    // �C�[�Y�A�E�g���[�V�����i�I��肪�����������j
                    pane.Zoom = nowPos + diff;
                }
            }
            foreach (var pane in dels)
            {
                _motionZoom.Remove(pane);
            }
            if (_motionZoom.Count > 0)
            {
                _isMoving |= Moving.Scroll;
            }
            return isNeedRetry;
        }

        /// <summary>
        /// �X�N���[�����P�X�e�b�v���s
        /// </summary>
        /// <returns></returns>
        private bool scroll1step()
        {
            var dels = new List<IRichPane>();
            var isNeedRetry = false;
            lock (_motionScroll)
            {
                foreach (var kv in _motionScroll)
                {
                    isNeedRetry = true;

                    var pane = kv.Key;
                    var tarPos = kv.Value.V1;

                    var nowPos = pane.Scroll;

                    if (--kv.Value.V2/*curStepN*/ <= 0)
                    {
                        pane.Scroll = tarPos;
                        dels.Add(pane);
                    }
                    else
                    {
                        var diff = tarPos - nowPos;
                        diff = diff / 4;    // �C�[�Y�A�E�g���[�V�����i�I��肪�����������j
                        pane.Scroll = nowPos + diff;
                    }
                }
                foreach (var pane in dels)
                {
                    _motionScroll.Remove(pane);
                }
                if (_motionScroll.Count > 0)
                {
                    _isMoving |= Moving.Scroll;
                }
                return isNeedRetry;
            }
        }

        /// <summary>
        /// ���[�V�����ړ��t���O
        /// </summary>
        [Flags]
        public enum Moving
        {
            /// <summary>
            /// �ړ����Ă��Ȃ�
            /// </summary>
            None = 0,
            /// <summary>
            /// �p�[�c�ړ���
            /// </summary>
            Parts = 0x01,
            /// <summary>
            /// ���[�V�����X�N���[����
            /// </summary>
            Scroll = 0x02,
        }

        /// <summary>
        /// ���[�V�����ړ��t���O
        /// </summary>
        private static Moving _isMoving = Moving.None;

        /// <summary>
        /// �ړ������ǂ����������t���O
        /// </summary>
        public static Moving IsMoving => _isMoving;

        /// <summary>
        /// ���[�V�����ړ��Ώۃp�[�c���P���[�V�����X�e�b�v�� �i�߂�
        /// </summary>
        /// <returns>true=������x����Invalidate�Ń��[�V�����p�����K�v</returns>
        private bool move1step()
        {
            var dels = new List<PartsBase>();
            var nDone = 0;
            var ret = false;
            lock (_tarPos)
            {
                foreach (var kv in _tarPos)
                {
                    var part = kv.Key;
                    var tar = kv.Value;

                    if (part.Rect.LT.Equals(tar.Pos))
                    {
                        dels.Add(part);
                        tar.StepN = 0;
                    }
                    else
                    {
                        tar.StepN--;
                        if (tar.StepN == 0)
                        {
                            part.Rect = CodeRect.FromLTWH(tar.Pos.X, tar.Pos.Y, part.Rect.Width, part.Rect.Height);
                            dels.Add(part);

                        }
                        else
                        {
                            float dx = tar.Pos.X - part.Rect.LT.X;
                            float dy = tar.Pos.Y - part.Rect.LT.Y;
                            var div = (float)tar.StepN * 2;
                            dx = dx / div;
                            dy = dy / div;
                            part.Rect += CodePos.FromInt((int)dx, (int)dy);
                        }
                        nDone++;
                    }
                }
            }
            if (nDone > 0)
            {
                ret = true;
            }

            foreach (var del in dels)
            {
                _tarPos.Remove(del);
            }
            _isMoving |= _tarPos.Count > 0 ? Moving.Parts : Moving.None;
            return ret;
        }

        /// <summary>
        /// ���[�V�����X�N���[������ʒu��ݒ肷��
        /// </summary>
        /// <param name="pane">�X�N���[���������y�[��</param>
        /// <param name="pos">�X�N���[���ړI�n</param>
        /// <param name="step">���[�V�����X�e�b�v�� 20�ʂ��Z�b�g����Ƃ��傤�Ǘǂ�</param>
        public void SetMotionScrollPosition(IRichPane pane, ScreenPos pos, int step)
        {
            lock (_motionScroll)
            {
                _motionScroll[pane] = new ScrollCommand(pos, step);
            }
        }

        /// <summary>
        /// ���[�V�����Y�[������l��ݒ肷��
        /// </summary>
        /// <param name="pane"></param>
        /// <param name="zoom"></param>
        /// <param name="step"></param>
        public void SetMotionZoomValue(IRichPane pane, XyBase zoom, int step)
        {
            _motionZoom[pane] = new ZoomCommand(zoom, step);
        }
    }
}
