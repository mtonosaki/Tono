using System;
using System.Collections;
using System.Collections.Specialized;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �p�[�c�̈ʒu�̈ړ����Ǘ�����
    /// Key = dpBase
    /// Value = Pos3
    /// </summary>
    public class PartsPositionManager : HybridDictionary
    {
        /// <summary>
        /// �p�[�c���W�ύX�^�C�v
        /// </summary>
        [Flags]
        public enum DevelopType
        {
            Unknown = 0,                // ���ݒ�
            Move = 1,                   // �ړ�
            SizeFlag = 0x80,            // �T�C�Y�ύX�t���O
            SizeLeft = SizeFlag | 1,        // ���[�̃T�C�Y�ύX
            SizeRight = SizeFlag | 2,       // �E�[�̃T�C�Y�ύX
            SizeTop = SizeFlag | 4,         // ��[�̃T�C�Y�ύX
            SizeBottom = SizeFlag | 8,      // ���[�̃T�C�Y�ύX
        }

        #region	����(�V���A���C�Y����)
        #endregion
        #region	����(�V���A���C�Y���Ȃ�)
        /// <summary>�I�����</summary>
        private DevelopType dev = DevelopType.Move;

        /// <summary>
        /// ����^�C�v���w�肷��
        /// </summary>
        /// <param name="value"></param>
        public void SetDevelop(DevelopType value)
        {
            dev = value;
            _lastDevelpType = dev;
        }
        #endregion

        /// <summary>
        /// �p�[�c���W�̊Ǘ�
        /// </summary>
        public class Pos3
        {
            /// <summary>�Y���y�[��</summary>
            public IRichPane OrgPane;
            /// <summary>�Y���y�[��</summary>
            public IRichPane NowPane;
            /// <summary>�ړ��h���b�O�O�̈ʒu�i���������W�̏ꍇ�A�����̒l�j</summary>
            public CodeRect Org;
            /// <summary>���O�̈ʒu�i���������W�̏ꍇ�A�����̒l�j</summary>
            public CodeRect Pre;
            /// <summary>���݂̈ʒu�i���������W�̏ꍇ�A�����̒l�j</summary>
            public CodeRect Now;
            /// <summary>�r������p�[�c���ǉ����ꂽ�ꍇ�ɒǉ����ꂽ���̈ړ���</summary>
            public CodePos Offset = new CodePos();

            public override string ToString()
            {
                var s = "";
                if (Org != null)
                {
                    s += "O{" + Org.ToString() + "} ";
                }

                if (Pre != null)
                {
                    s += "P{" + Pre.ToString() + "} ";
                }

                if (Now != null)
                {
                    s += "N{" + Now.ToString() + "} ";
                }

                if (NowPane != null)
                {
                    s += "Pane = " + NowPane.IdText;
                }
                return s;
            }

            /// <summary>
            /// ���̈ʒu���ǂ������ׂ�
            /// </summary>
            public bool IsStanding
            {
                get
                {
                    if (Org.Equals(Now))
                    {
                        return true;
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// �C���X�^���X������������
        /// </summary>
        /// <param name="with">��ʒu�ƃp�[�c�̎�ނ̌����</param>
        /// <param name="pane">��ʒu�̃X�N���[���ƃY�[���l���L�����邽�߂̃C���v�b�g</param>
        public void Initialize(PartsCollectionBase with)
        {
            Clear();

            _lastDevelpType = DevelopType.Unknown;

            foreach (PartsCollectionBase.PartsEntry pe in with)
            {
                var p = new Pos3
                {
                    OrgPane = RichPaneBinder.CreateCopyComplete(pe.Pane),
                    NowPane = pe.Pane,
                    Org = (CodeRect)pe.Parts.Rect.Clone(), // �������̍��W�i�������Ȃ��ꍇ�A�P�Ȃ�p�[�c���W�j
                    Pre = (CodeRect)pe.Parts.Rect.Clone(), // �������̍��W�i�������Ȃ��ꍇ�A�P�Ȃ�p�[�c���W�j
                    Now = (CodeRect)pe.Parts.Rect.Clone(), // �������̍��W�i�������Ȃ��ꍇ�A�P�Ȃ�p�[�c���W�j
                    Offset = CodePos.FromInt(0, 0)
                };
                base[pe.Parts] = p;
            }
        }

        /// <summary>
        /// �p�[�c���w�肵�ăC���X�^���X������������
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="pane"></param>
        public void AddParts(PartsBase parts, IRichPane pane, CodePos offset)
        {
            _lastDevelpType = DevelopType.Unknown;

            var p = new Pos3
            {
                OrgPane = RichPaneBinder.CreateCopyComplete(pane),
                NowPane = pane,
                Org = (CodeRect)parts.Rect.Clone(), // �������̍��W�i�������Ȃ��ꍇ�A�P�Ȃ�p�[�c���W�j
                                                   //p.Org = (uCdRect)parts.Rect.Clone() - parts.GetCdPos(pane, _prevShift); // �������̍��W�i�������Ȃ��ꍇ�A�P�Ȃ�p�[�c���W�j
                Pre = (CodeRect)parts.Rect.Clone(), // �������̍��W�i�������Ȃ��ꍇ�A�P�Ȃ�p�[�c���W�j
                Now = (CodeRect)parts.Rect.Clone(), // �������̍��W�i�������Ȃ��ꍇ�A�P�Ȃ�p�[�c���W�j
                Offset = (CodePos)offset.Clone()
            };
            base[parts] = p;
        }

        /// <summary>
        /// �p�[�c���w�肵�� POS�𓾂�
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Pos3 this[PartsBase key] => (Pos3)base[key];

        /// <summary>
        /// �p�[�c�̍��W�����ۂɏ���������
        /// </summary>
        /// <param name="partsCollection">�p�[�c�̈�̕`��X�V������ꍇ�AParts�C���X�^���X���w�肷��</param>
        public void SetNowPositionsToParts(PartsCollectionBase partsCollection)
        {
            foreach (DictionaryEntry de in this)
            {
                var parts = (PartsBase)de.Key;
                var pos = (PartsPositionManager.Pos3)de.Value;

                if (partsCollection != null)
                {
                    partsCollection.Invalidate(parts, pos.NowPane); // �ړ��O��Invalidate
                }
                parts.Rect = pos.Now;                           // �ʒu�𒲐�����
                if (partsCollection != null)
                {
                    partsCollection.Invalidate(parts, pos.NowPane); // �ړ����Invalidate
                }
            }
        }


        /// <summary>
        /// �Ō��Develop���s�������̈ʒu�ύX���@
        /// </summary>
        public DevelopType LastDevelop => _lastDevelpType;
        private DevelopType _lastDevelpType = DevelopType.Unknown;
        private ScreenPos _prevShift = ScreenPos.FromInt(0, 0);

        /// <summary>
        /// �ړ������X�V����
        /// ���ӁF��������X�N���[�����W�ɕϊ����Ĉړ�����̂ŁA�R�[�h���W�ɕϊ��덷��������\��������B
        /// </summary>
        /// <param name="dragStartPos">�ړ��J�n���̃}�E�X���W�i�h���b�O�J�n�_�j</param>
        /// <param name="currentPos">���݂̃}�E�X���W</param>
        /// <param name="type">���W�ύX�^�C�v</param>
        public void Develop(ScreenPos dragStartPos, ScreenPos currentPos, DevelopType type)
        {
            _lastDevelpType = type;
            var sdelta = currentPos - dragStartPos;     // �}�E�X�̈ړ���
            _prevShift = sdelta;

            foreach (DictionaryEntry de in this)            // �I�𒆑S�p�[�c�ɑ΂��čs��
            {
                var p3 = (Pos3)de.Value;
                var target = (PartsBase)de.Key;
                p3.Pre = p3.Now;                            // �ЂƂO�̍��W�ɋL��
                p3.Now = (CodeRect)p3.Now.Clone();           // �T�C�Y�n�̏��������������삷�邽�߂ɕK�v
                var virtualPos = target.GetCdRect(p3.NowPane, target.GetScRect(p3.OrgPane, p3.Org) + sdelta); // �ړ���̈ʒu���v�Z
                var d = target.GetCdPos(p3.NowPane, sdelta);
                //Debug.WriteLine( string.Format( "[{0}] Delta[{1}]:Offset[{2}]", i, d, p3.Offset) );

                virtualPos = CodeRect.FromLTWH(virtualPos.LT.X + p3.Offset.X, virtualPos.LT.Y, virtualPos.Width, virtualPos.Height);

                switch (type)
                {
                    case DevelopType.Move:
                        p3.Now = virtualPos;
                        if (p3.Now.Width != p3.Org.Width)
                        {
                            p3.Now.RB.X = p3.Now.LT.X + p3.Org.Width - 1;   // �����ς��Ȃ��悤�ɂ���
                        }
                        break;
                    case DevelopType.SizeRight:
                        p3.Now.RB.X = virtualPos.RB.X;
                        break;
                    case DevelopType.SizeLeft:
                        p3.Now.LT.X = virtualPos.LT.X;
                        break;
                    case DevelopType.SizeTop:
                        p3.Now.LT.Y = virtualPos.LT.Y;
                        break;
                    case DevelopType.SizeBottom:
                        p3.Now.RB.Y = virtualPos.RB.Y;
                        break;
                }
            }
        }

        /// <summary>
        /// �w�肵���L�[�̈ړ������폜����
        /// </summary>
        /// <param name="key">�폜����ʒu���̃p�[�c</param>
        public void Remove(PartsBase key)
        {
            base.Remove(key);
        }

        /// <summary>
        /// �S�ҏW�p�[�c�������Ă��Ȃ����Ƃ��m�F
        /// </summary>
        public bool IsStanding
        {
            get
            {
                foreach (Pos3 pos in Values)
                {
                    if (pos.IsStanding == false)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
