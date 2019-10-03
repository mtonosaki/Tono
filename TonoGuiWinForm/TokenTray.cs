using System.Collections;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �g�[�N�����󂯂�ꏊ�i�g�[�N���Ƃ́A���̃t�B�[�`���[���N�����邽�߂̎d�g�݁j
    /// </summary>
    public class TokenTray
    {
        //		private IDictionary _dat = new HybridDictionary();	// TONO
        private readonly IDictionary _dat = new Hashtable();

        /// <summary>InvokeStart�̋N���ς݃t���O�p�ϐ�</summary>
        //		private /*friane fgBase*/IDictionary TokenInvokedChecker = new HybridDictionary();	// TONO
        private /*friane fgBase*/readonly IDictionary TokenInvokedChecker = new Hashtable();

        /// <summary>
        /// �w��t�B�[�`���[�̃g�[�N�����X�i�[�Ɏw��ID���܂܂�Ă��邩�ǂ������ׂ�
        /// </summary>
        /// <param name="fo"></param>
        /// <param name="tokenid"></param>
        /// <returns></returns>
        public static bool ContainsTokenID(FeatureBase fo, NamedId tokenid)
        {
            if (fo is ITokenListener)
            {
                if (((ITokenListener)fo).TokenTriggerID == tokenid)
                {
                    return true;
                }
            }
            if (fo is IMultiTokenListener)
            {
                foreach (var fid in ((IMultiTokenListener)fo).MultiTokenTriggerID)
                {
                    if (fid == tokenid)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// �g�[�N����ǉ��i�\��j����B�����Flush�����s�����Ƃ��ɁA�{���ɒǉ������
        /// </summary>
        /// <param name="id">�g�[�N���ɕt�������O</param>
        /// <param name="from">�g�[�N����ǉ������N���X</param>
        public void Add(NamedId id, FeatureBase from)
        {
            System.Diagnostics.Debug.Assert(id != null, "ffTokenTray.Add��null id ���w�肵�Ȃ��ł�������");
            lock (_dat.SyncRoot)
            {
                _dat[id] = from;
            }

            // �g�[�N���������Ƀg�[�N���ǉ����ꂽ�ꍇ�A�N���ς݃t�B�[�`���[���������āA���̃t�B�[�`���[��������x�N���ł���悤�ɂ���
            IList dels = new ArrayList();
            lock (dels.SyncRoot)
            {
                foreach (FeatureBase fo in TokenInvokedChecker.Keys) // fo = �N���ς݃t�B�[�`���[
                {
                    if (ContainsTokenID(fo, id))
                    {
                        dels.Add(fo);
                    }
                }
                foreach (FeatureBase fo in dels)
                {
                    TokenInvokedChecker.Remove(fo);
                }
            }
        }

        /// <summary>
        /// �g�[�N���ɓo�^���ꂽ�SID��
        /// </summary>
        public int Count => _dat.Count;

        /// <summary>
        /// �w��ID���܂܂�Ă��邩�ǂ����𒲂ׂ�
        /// </summary>
        /// <param name="value">ID</param>
        /// <returns>true = �܂܂�Ă��� / false = �܂܂�Ȃ�</returns>
        public bool Contains(NamedId value)
        {
            if (value == null)
            {
                return false;
            }
            return _dat.Contains(value);
        }

        /// <summary>
        /// �w��t�B�[�`���[���v�����郊�X�i�[��TokenTray�ɂ��邩�ǂ����𒲍�����
        /// </summary>
        /// <param name="value">�����ΏۂƂ���t�B�[�`���[</param>
        /// <returns>true = �܂܂�Ă��� / false = �܂܂�Ȃ�</returns>
        public bool Contains(FeatureBase value)
        {
            var ret = false;
            if (value is ITokenListener)
            {
                ret = _dat.Contains(((ITokenListener)value).TokenTriggerID);
            }
            if (!ret && value is IMultiTokenListener)
            {
                foreach (var id in ((IMultiTokenListener)value).MultiTokenTriggerID)
                {
                    ret = _dat.Contains(id);
                    if (ret)
                    {
                        break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// ���ׂẴg�[�N�����폜����
        /// </summary>
        internal void _clear()
        {
            lock (_dat.SyncRoot)
            {
                _dat.Clear();
            }
        }
    }
}
