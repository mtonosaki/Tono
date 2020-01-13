// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.Serialization;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �`��ɕK�v�Ȋ�{�I�ȋ@�\��񋟂��钊�ۃN���X
    /// dp = Draw Parts
    /// </summary>
    [Serializable]
    public abstract class PartsBase : ICloneable, IDisposable
    {
        #region �R�[�_�E�|�W�V���i�֘A

        private static readonly Hashtable _positionerBufK2I = new Hashtable();
        private static readonly Hashtable _positionerBufI2K = new Hashtable();
        private static readonly Hashtable _coderBufK2I = new Hashtable();
        private static readonly Hashtable _coderBufI2K = new Hashtable();

        /// <summary>�ʒu��ϊ����邽�߂̏����������ł���</summary>
        public delegate LayoutRect PositionerMethod(CodeRect code, PartsBase target);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public delegate CodeRect PosCoderMethod(LayoutRect rect, PartsBase target);

        private string _positionerKeyForSave = null;
        private string _coderKeyForSave = null;

        #endregion
        #region �����p
        [NonSerialized]
        private static int _idCounter = 0;
        #endregion
        #region		����(�V���A���C�Y����)

        /// <summary>�p�[�c�ɕt�^���ꂽ���j�[�N��ID</summary>
        private Id _partsID = new Id { Value = _idCounter++ };

        /** <summary>��`�̈�̏��</summary> */
        private CodeRect _Pos = CodeRect.FromLTRB(0, 0, 0, 0);

        /** <summary>�e�L�X�g</summary> */
        private string _Text = "";
        private Mes.Format _textFormat = null;

        #endregion
        #region		����(�V���A���C�Y���Ȃ�)

        /// <summary>���Ƀ}�X�N���w�肷��ꍇ�̃y�[��</summary>
        [NonSerialized]
        private ArrayList _specifiedMaskPane = null;

        /// <summary>�ʒu��ϊ����邽�߂̏����������ł���</summary>
        [NonSerialized]
        private PositionerMethod _partsPositioner = null;

        /// <summary>�p�[�c���W�𕄍������邽�߂̏����������ł���</summary>
        [NonSerialized]
        private PosCoderMethod _partsCoder = null;

        #endregion
        #region IDisposable �����o

        public virtual void Dispose()
        {
        }

        #endregion

        /// <summary>
        /// ������
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Text.Length > 0)
            {
                return GetType().Name + " \"" + Text + "\" " + _partsID.ToString();
            }
            else
            {
                return GetType().Name + " " + _partsID.ToString();
            }
        }


        /// <summary>
        /// �t�H�[�}�b�g�̓��e�ɏ]���āAText�v���p�e�B�����Z�b�g����
        /// </summary>
        public void ResetTextByFormat()
        {
            if (_textFormat != null)
            {
                Text = _textFormat.ToString();
            }
        }

        /// <summary>
        /// ����؂�ւ��Ɏ����Ή�����e�L�X�g�t�H�[�}�b�g���w�肷��
        /// </summary>
        public Mes.Format TextFormat
        {
            set
            {
                _textFormat = value;
                ResetTextByFormat();
            }
        }

        /// <summary>
        /// �p�[�c��{�N���X�̃f�t�H���g�R���X�g���N�^
        /// </summary>
        protected PartsBase()
        {
        }

        /// <summary>
        /// �p�[�c��ID
        /// </summary>
        public Id ID => _partsID;

        #region ICloneable �����o

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            var ret = (PartsBase)Activator.CreateInstance(GetType());
            ret._Pos = _Pos;
            ret._Text = _Text;
            ret._specifiedMaskPane = _specifiedMaskPane;
            ret._partsPositioner = _partsPositioner;
            ret._partsCoder = _partsCoder;
            return ret;
        }

        #endregion

        #region ISerializable �����o

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            SerializerEx.GetObjectData(typeof(PartsBase), this, info, context, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected PartsBase(SerializationInfo info, StreamingContext context)
        {
            SerializerEx.Instanciate(typeof(PartsBase), this, info, context, true);
        }

        #endregion

        /// <summary>
        /// ID���n�b�V���R�[�h�Ƃ���
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ID.Value;
        }

        /// <summary>
        /// ID�Ŕ�r����
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is PartsBase tar)
            {
                return ID == tar.ID;
            }
            return false;
        }

        /// <summary>
        /// �w����W���`��N���b�v�����ǂ������ʂ���
        /// </summary>
        /// <param name="rp">�y�[��</param>
        /// <param name="rect">�����͈�</param>
        /// <returns>true = �����͈͂��`��N���b�v�� / false = �����łȂ�</returns>
        //[DebuggerStepThrough]
        protected bool isInClip(IRichPane rp, ScreenRect rect0)
        {
            var rect = (ScreenRect)rect0.Clone();
            if (rect.Width < 1)
            {
                rect.RB.X = rect0.LT.X + 1;
            }
            var r = rect & rp.GetPaintClipRect();
            if (r == null)
            {
                return false;
            }

            r = rect & rp.GetPaneRect();
            if (r == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// �w����W���`��N���b�v�����ǂ������ʂ���
        /// </summary>
        /// <param name="rp">�y�[��</param>
        /// <returns>true = �����͈͂��`��N���b�v�� / false = �����łȂ�</returns>
        public bool isInClip(IRichPane rp)
        {
            var rect = GetScRect(rp, Rect);
            var r = rect & rp.GetPaneRect();
            return r != null;
        }

        /// <summary>
        /// PartsPositioner���R�s�[����
        /// �i����PartsPositioner�̃C���X�^���X��Dispose����Ă��邩������Ȃ��̂Œ��ӂ���j
        /// </summary>
        /// <param name="org"></param>
        public void SetPositionerFrom(PartsBase org)
        {
            PartsPositioner = org.PartsPositioner;
        }

        /// <summary>
        /// PartsPositionCorder���R�s�[����
        /// �i����PartsPositionCorder�̃C���X�^���X��Dispose����Ă��邩������Ȃ��̂Œ��ӂ���j
        /// </summary>
        /// <param name="org"></param>
        public void SetPositionCorderFrom(PartsBase org)
        {
            PartsPositionCorder = org.PartsPositionCorder;
        }


        /// <summary>
        /// �ʒu��ϊ����鏈�����o�^�ł���
        /// </summary>
        public PositionerMethod PartsPositioner
        {
            [DebuggerStepThrough]
            set
            {
                _partsPositioner = value;
                string key;
                if (value.Target == null)
                {
                    key = "(static)." + value.Method.Name;
                }
                else
                {
                    key = value.Target.GetType().Name + "." + value.Method.Name;
                }
                _positionerBufK2I[key] = value;
                _positionerBufI2K[value] = key;
            }
            protected get => _partsPositioner;
        }

        /// <summary>
        /// �|�W�V���i�[�̖��O�i������j���擾����
        /// </summary>
        public void SetPartsPositionerName(bool sw)
        {
            if (sw && _partsPositioner != null)
            {
                _positionerKeyForSave = (string)_positionerBufI2K[_partsPositioner];
            }
            else
            {
                _positionerKeyForSave = string.Empty;
            }
        }

        /// <summary>
        /// �|�W�V���i�[�̖��O�i������j���C���X�^���X�ɓK�p����
        /// </summary>
        public void InstanciatePartsPositioner()
        {
            _partsPositioner = (PartsBase.PositionerMethod)_positionerBufK2I[_positionerKeyForSave];
            _positionerKeyForSave = string.Empty;
        }

        /// <summary>
        /// �ʒu�𕄍������鏈�����o�^�ł���
        /// </summary>
        public PosCoderMethod PartsPositionCorder
        {
            set
            {
                _partsCoder = value;
                string key;
                if (value.Target == null)   // static ���\�b�h
                {
                    key = "(static)." + value.Method.Name;
                }
                else
                {
                    key = value.Target.GetType().Name + "." + value.Method.Name;
                }
                _coderBufK2I[key] = value;
                _coderBufI2K[value] = key;
            }
            protected get => _partsCoder;
        }

        /// <summary>
        /// �R�[�_�[�̖��O�i������j���擾����
        /// </summary>
        public void SetPartsPositionCorderName(bool sw)
        {
            if (sw && _partsCoder != null)
            {
                _coderKeyForSave = (string)_coderBufI2K[_partsCoder];
            }
            else
            {
                _coderKeyForSave = string.Empty;
            }
        }
        /// <summary>
        /// �|�W�V���i�[�̖��O�i������j���C���X�^���X�ɓK�p����
        /// </summary>
        public void InstanciatePartsPositionCorderName()
        {
            _partsCoder = (PartsBase.PosCoderMethod)_coderBufK2I[_coderKeyForSave];
            _coderKeyForSave = string.Empty;
        }

        /// <summary>
        /// �}�X�N�̈����肷��
        /// </summary>
        /// <param name="pane"></param>
        [DebuggerStepThrough]
        public void AddSpecifiedMask(IRichPane pane)
        {
            if (_specifiedMaskPane == null)
            {
                _specifiedMaskPane = new ArrayList();
            }
            _specifiedMaskPane.Add(pane);
        }

        /// <summary>
        /// �\���p�̃X�N���[�����W���擾����
        /// </summary>
        /// <param name="rp">�v�Z�ɗp���郊�b�`�y�[��</param>
        /// <returns>�X�N���[�����W</returns>
        public virtual ScreenRect GetScRect(IRichPane rp)
        {
            return GetScRect(rp, Rect);
        }

        /// <summary>
        /// �p�[�c���W���擾����
        /// </summary>
        /// <returns></returns>
        public virtual LayoutRect GetPtRect()
        {
            return GetPtRect(Rect);
        }

        /// <summary>
        /// �p�[�c���W���擾����
        /// </summary>
        /// <returns></returns>
        public virtual LayoutRect GetPtRect(CodeRect rect)
        {
            if (_partsPositioner == null)
            {
                return LayoutRect.FromLTRB(rect.LT.X, rect.LT.Y, rect.RB.X, rect.RB.Y);
            }
            return _partsPositioner(rect, this);
        }

        /// <summary>
        /// �\���p�̃X�N���[�����W���擾����
        /// </summary>
        /// <param name="rp">�v�Z�ɗp���郊�b�`�y�[��</param>
        /// <param name="rect"></param>
        /// <returns>�X�N���[�����W</returns>
        public virtual ScreenRect GetScRect(IRichPane rp, CodeRect rect)
        {
            if (_partsPositioner != null)
            {
                return rp.Convert(_partsPositioner(rect, this));
            }
            return rp.Convert(LayoutRect.FromLTRB(rect.LT.X, rect.LT.Y, rect.RB.X, rect.RB.Y));
        }

        /// <summary>
        /// �\���p�̃X�N���[�����W���擾����
        /// </summary>
        /// <param name="rp">�v�Z�ɗp���郊�b�`�y�[��</param>
        /// <param name="code"></param>
        /// <returns>�X�N���[�����W</returns>
        [DebuggerStepThrough]
        public ScreenPos GetZoomed(IRichPane rp, CodePos code)
        {
            if (_partsPositioner != null)
            {
                var rect = CodeRect.FromLTWH(code.X, code.Y, 1, 1);
                return rp.GetZoomed(_partsPositioner(rect, this).LT);
            }
            return rp.GetZoomed(LayoutPos.FromInt(code.X, code.Y));
        }

        /// <summary>
        /// �w��X�N���[�����W���p�[�c���W�i�������j�ɕϊ�����
        /// </summary>
        /// <param name="rp">�y�[��</param>
        /// <param name="rect">�X�N���[�����W</param>
        /// <returns>�p�[�c���W</returns>
        [DebuggerStepThrough]
        public CodeRect GetCdRect(IRichPane rp, ScreenRect rect)
        {
            var ret = rp.Convert(rect);
            if (_partsCoder != null)
            {
                return _partsCoder(ret, this);
            }
            return CodeRect.FromLTRB(ret.LT.X, ret.LT.Y, ret.RB.X, ret.RB.Y);
        }

        /// <summary>
        /// �w��X�N���[�����W���p�[�c���W�i�������j�ɕϊ�����
        /// </summary>
        /// <param name="rp">�y�[��</param>
        /// <param name="pos">�X�N���[�����W</param>
        /// <returns>�p�[�c���W</returns>
        [DebuggerStepThrough]
        public CodePos GetCdPos(IRichPane rp, ScreenPos pos)
        {
            var ret = rp.Convert(pos);
            if (_partsCoder != null)
            {
                var pt = new LayoutRect
                {
                    LT = ret,
                    RB = ret
                };
                return _partsCoder(pt, this).LT;
            }
            return CodePos.FromInt(ret.X, ret.Y);
        }

        /// <summary>
        /// �}�X�N�̎��
        /// </summary>
        protected enum MaskType
        {
            /// <summary>
            /// 
            /// </summary>
            None,
            /// <summary>
            /// 
            /// </summary>
            Pane,
            /// <summary>
            /// 
            /// </summary>
            Parts,
            /// <summary>
            /// 
            /// </summary>
            Specification
        }

        /// <summary>
        /// �y�[���Ń}�X�N����
        /// </summary>
        /// <param name="rp">�v�Z�ɗp���郊�b�`�y�[��</param>
        /// <returns>���O�̃}�X�N�̈�</returns>
        public static Region Mask(IRichPane rp)
        {
            var ret = rp.Graphics.Clip;
            rp.Graphics.Clip = new Region(rp.GetPaneRect().GetPpSize() & rp.GetPaintClipRect());
            return ret;
        }

        /// <summary>
        /// �`��̈���}�X�N����
        /// </summary>
        /// <param name="rp">�`����s�����n�y�[��</param>
        /// <param name="type">�}�X�N�̃^�C�v</param>
        /// <returns>���O�̃}�X�N�̈�</returns>
        [DebuggerStepThrough]
        protected Region Mask(IRichPane rp, MaskType type)
        {
            Region ret;

            switch (type)
            {
                case MaskType.Pane:
                    ret = Mask(rp);
                    break;
                case MaskType.Parts:
                    ret = rp.Graphics.Clip;
                    rp.Graphics.Clip = new Region(GetScRect(rp).GetPpSize());
                    break;
                case MaskType.None:
                    ret = rp.Graphics.Clip.Clone();
                    rp.Graphics.Clip.MakeInfinite();
                    break;
                case MaskType.Specification:
                    ret = rp.Graphics.Clip;
                    if (_specifiedMaskPane != null)
                    {
                        var reg = new Region();
                        reg.MakeEmpty();
                        foreach (IRichPane mrp in _specifiedMaskPane)
                        {
                            reg.Union(mrp.GetPaneRect().GetPpSize());
                        }
                        rp.Graphics.Clip = reg;
                    }
                    break;
                default:
                    ret = rp.Graphics.Clip;
                    break;
            }
            return ret;
        }

        /// <summary>
        /// �`��
        /// </summary>
        /// <param name="rp">���b�`�y�[��</param>
        /// <returns>false = �`��ł��Ȃ�����</returns>
        public abstract bool Draw(IRichPane rp);

        /// <summary>
        /// �I����Ԃ̕W�������i�eDraw�ŃR�[�����邩�A�Ǝ��ɑI����Ԃ��������邱�Ɓj
        /// </summary>
        /// <param name="rp">���b�`�y�[��</param>
        protected virtual void drawSelected(IRichPane rp)
        {
            if (this is IPartsSelectable)
            {
                if (((IPartsSelectable)this).IsSelected)
                {
                    rp.Graphics.DrawRectangle(Pens.Red, GetScRect(rp));
                }
            }
        }

        /// <summary>
        /// IsOn�̖߂�l
        /// </summary>
        [Flags]
        public enum PointType
        {
            /// �̈�O
            Outside = 0,
            /// �̈��
            Inside = 0x10,
            /// ���E��i���[�j
            OnLeft = 0x01,
            /// ���E��i��[�j
            OnTop = 0x04,
            /// ���E��i�E�[�j
            OnRight = 0x02,
            /// ���E��i���[�j
            OnBottom = 0x08
        }

        /// <summary>
        /// �w��X�N���[�����W����`�̈������������
        /// </summary>
        /// <param name="sp">�X�N���[�����W�ł̈ʒu</param>
        /// <param name="rp">���b�`�y�[��</param>
        /// <returns>����</returns>
        public virtual PointType IsOn(ScreenPos sp, IRichPane rp)
        {
            return IsOn(sp, rp, PointType.OnBottom | PointType.OnLeft | PointType.OnRight | PointType.OnTop | PointType.Outside | PointType.Inside);
        }

        /// <summary>
        /// �w��X�N���[�����W����`�̈������������
        /// </summary>
        /// <param name="sp">�X�N���[�����W�i�}�E�X�̍��W�j</param>
        /// <param name="rp">���b�`�y�[��</param>
        /// <param name="check">�`�F�b�N�Ώۂ̗񋓁i�t���O�j</param>
        public virtual PointType IsOn(ScreenPos sp, IRichPane rp, PointType check)
        {
            var r = GetScRect(rp);
            if (r.Width < 1 || r.Height < 1)
            {
                return PointType.Outside;
            }
            var w = 3;  // ���E���𔻒f���邽�߂̕�

            if (r.IsIn(sp))
            {
                if (r.Width > r.Height)
                {
                    if ((check & PointType.OnRight) != 0 && Math.Abs(r.RB.X - sp.X) <= w)
                    {
                        return PointType.OnRight;
                    }

                    if ((check & PointType.OnLeft) != 0 && Math.Abs(r.LT.X - sp.X) <= w)
                    {
                        return PointType.OnLeft;
                    }

                    if ((check & PointType.OnBottom) != 0 && Math.Abs(r.RB.Y - sp.Y) <= w)
                    {
                        return PointType.OnBottom;
                    }

                    if ((check & PointType.OnTop) != 0 && Math.Abs(r.LT.Y - sp.Y) <= w)
                    {
                        return PointType.OnTop;
                    }
                }
                else
                {
                    if ((check & PointType.OnBottom) != 0 && Math.Abs(r.RB.Y - sp.Y) <= w)
                    {
                        return PointType.OnBottom;
                    }

                    if ((check & PointType.OnTop) != 0 && Math.Abs(r.LT.Y - sp.Y) <= w)
                    {
                        return PointType.OnTop;
                    }

                    if ((check & PointType.OnRight) != 0 && Math.Abs(r.RB.X - sp.X) <= w)
                    {
                        return PointType.OnRight;
                    }

                    if ((check & PointType.OnLeft) != 0 && Math.Abs(r.LT.X - sp.X) <= w)
                    {
                        return PointType.OnLeft;
                    }
                }
                return PointType.Inside;
            }
            return PointType.Outside;
        }

        /// <summary>
        /// �������W��^���āA�p�[�c�������ǂ����𔻒肷��
        /// �iIsOn(uScPos�E�E�E�ƈႢ�A�����ɏ������ł���j
        /// </summary>
        /// <param name="cp">�������W</param>
        /// <returns>Inside = ���� / Outside = �O��</returns>
        public virtual PointType IsOn(CodePos cp)
        {
            return Rect.IsIn(cp) ? PointType.Inside : PointType.Outside;
        }

        /// <summary>
        /// ��`�̈�̎擾/�ݒ�
        /// </summary>
        public virtual CodeRect Rect
        {
            [DebuggerStepThrough]
            get => _Pos;
            //[DebuggerStepThrough]
            set => _Pos = value;
        }

        /// <summary>
        /// �e�L�X�g�̎擾/�ݒ�
        /// </summary>
        public virtual string Text
        {
            [DebuggerStepThrough]
            get => _Text;
            [DebuggerStepThrough]
            set => _Text = value;
        }
    }
}
