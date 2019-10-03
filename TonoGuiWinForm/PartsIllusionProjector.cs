using System;
using System.Collections;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// dfPartsIllusionBase �̊T�v�̐����ł��B
    /// ���̃N���X�́A����̃p�[�c�������ӏ��ɂ���悤�Ɍ��������錶�e�����o���N���X
    /// �����́A�����IRichPane�̓��e�����̂܂ܕʂ�IRichPane�ɕ`�悷�邱�ƂŎ���
    /// ���̃N���X�̎g�����̗�́AFeatureDposeRichPane��OnInitialInstance���Q��
    /// </summary>
    /// <remarks>
    /// �������J�ԍ��F2008-171229
    /// </remarks>
    public class PartsIllusionProjector : IDisposable
    {
        #region �f���Q�[�g�^
        public delegate void SetIllusionStateMethod(PartsIllusionProjector projector, PartsBase target);
        public delegate bool IsIllusionProjectMethod(IRichPane originalPane, PartsBase targetParts);
        #endregion
        #region �y�[��Enumerator

        /// <summary>
        /// �C�����[�W�����ƌ��y�[�����܂߂ė񋓂𐧌䂷��
        /// </summary>
        public class PaneEnumerator : IEnumerable, IEnumerator
        {
            private readonly IList/*<dfPartsIllusionProjector>*/ _dat;
            private readonly IRichPane _parent;
            private object _current = null;
            private int _pointer;
            private readonly PartsBase _parts;
#if DEBUG
            public string _
            {
                get
                {
                    if (_current == null)
                    {
                        return "(null)";
                    }
                    return "Current = " + ((IRichPane)_current).IdText;
                }
            }
#endif
            /// <summary>
            /// Enumerator���\�z����
            /// </summary>
            /// <param name="parent">target��0���̎��ɓK�p�����y�[��</param>
            /// <param name="target">dfPartsIllusionProjector�^�̃��X�g</param>
            /// <param name="parts">�Q�ƒ��̃p�[�c</param>
            internal PaneEnumerator(IRichPane parent, IList target, PartsBase parts)
            {
                _parent = parent;
                _dat = target;
                if (_dat != null)
                {
                    if (_dat.Count > 0)
                    {
                        _parent = ((PartsIllusionProjector)_dat[0]).OriginalPane;
                    }
                }
                _parts = parts;
                Reset();
            }
            #region IEnumerable �����o

            public IEnumerator GetEnumerator()
            {
                return this;
            }

            #endregion

            #region IEnumerator �����o

            public void Reset()
            {
                _pointer = -1;
            }

            /// <summary>
            /// IRichPane�^
            /// </summary>
            public object Current => _current;

            public bool MoveNext()
            {
                if (_pointer == -1)
                {
                    _current = _parent;
                    _pointer++;
                    return true;
                }
                if (_dat == null)
                {
                    return false;
                }
                if (_dat.Count > _pointer)
                {
                    var proj = (PartsIllusionProjector)_dat[_pointer];
                    _pointer++;
                    if (proj.isNeedProject(_parts))
                    {
                        proj.ChangeState(_parts);
                        _current = proj.ScreenPane;
                    }
                    else
                    {
                        return MoveNext();
                    }
                    return true;
                }
                return false;
            }

            #endregion
        }

        #endregion

        #region �����i�V���A���C�Y����j

        private IRichPane _screen;
        private IRichPane _original;
        private bool _isEnable = true;

        #endregion
        #region �����i�V���A���C�Y���Ȃ��j

        /// <summary>�C�����[�W�������̏�ԕύX�֐����w�肷��</summary>
        private SetIllusionStateMethod _stateFunction = null;

        /// <summary>�C�����[�W�������邩�ǂ����̔������������֐����w�肷��</summary>
        private IsIllusionProjectMethod _isIllutionFunction = null;

        #endregion

        /// <summary>
        /// �L���^���� getProjector�̎��Ɏ�̂��������ɂȂ�
        /// </summary>
        public bool Enabled
        {
            get => _isEnable;
            set => _isEnable = value;
        }

        /// <summary>
        /// �y�[����񋓂���Enumerator���擾����
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="target"></param>
        /// <param name="parts"></param>
        /// <returns></returns>
        public static PaneEnumerator GetEnumerator(IRichPane parent, IList target, PartsBase parts)
        {
            return new PaneEnumerator(parent, target, parts);
        }

        /// <summary>
        /// �C�����[�W�������邩�ǂ����̔������������֐����w�肷��
        /// </summary>
        public IsIllusionProjectMethod IsIllusionProject
        {
            set => _isIllutionFunction = value;
        }

        /// <summary>
        /// �C�����[�W�������ׂ����ǂ����𔻒f����
        /// </summary>
        /// <param name="parts">�����p�[�c</param>
        /// <returns>true = �C�����[�W��������ׂ� / false = �s�v</returns>
        internal bool isNeedProject(PartsBase parts)
        {
            if (_isIllutionFunction != null)
            {
                return _isIllutionFunction(OriginalPane, parts);
            }
            return true;
        }

        /// <summary>
        /// �C�����[�W�������̏�ԕύX�֐����w�肷��
        /// </summary>
        public SetIllusionStateMethod SetIllusionState
        {
            set => _stateFunction = value;
        }

        /// <summary>
        /// �ŐV�̃C�����[�W������ԂɍX�V����
        /// </summary>
        public void ChangeState(PartsBase target)
        {
            if (_stateFunction != null)
            {
                _stateFunction(this, target);
            }
        }

        /// <summary>
        /// �B��̃R���X�g���N�^
        /// </summary>
        /// <param name="target">��f�[�^�����܂�y�[��</param>
        /// <param name="idText">�C�����[�W�����Ǘ��p�̃y�[����IdText</param>
        public PartsIllusionProjector(IRichPane target, string idtext)
        {
            _original = target;
            _screen = new RichPaneBinder(_original)
            {

                // �v���p�e�B�K�p
                IdText = idtext,
                // �ȉ��̃v���p�e�B�́A�f���Q�[�g��_stateFunction�֐��Ŏ����邲�ƂɍX�V�����
                Zoom = _original.Zoom,
                Scroll = _original.Scroll
            };
        }

        /// <summary>
        /// �f�X�g���N�^
        /// </summary>
        ~PartsIllusionProjector()
        {
            Dispose();
        }

        #region IDisposable �����o

        public void Dispose()
        {
            if (_screen == null)
            {
                return;
            }
            _original.Control.Controls.Remove(_screen.Control);
            _screen = null;
            _original = null;
        }

        #endregion

        /// <summary>
        /// �C�����[�W�������̃y�[���i�I���W�i�����ł͂Ȃ��j
        /// </summary>
        public IRichPane ScreenPane => _screen;

        /// <summary>
        /// �p�[�c���������鑤�̃y�[���i�C�����[�W�������ł͂Ȃ��j
        /// </summary>
        public IRichPane OriginalPane => _original;
    }
}
