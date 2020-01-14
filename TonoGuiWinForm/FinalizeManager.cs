// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Specialized;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// Finalizer Manager �̊T�v�̐����ł��B
    /// </summary>
    public class FinalizeManager
    {
        /// <summary>
        /// �t�@�C�i���C�Y�̂��߂Ƀf���Q�[�g�^
        /// </summary>
        public delegate void Finalize();

        #region �����i�V���A���C�Y���Ȃ��j
        private readonly Finalize _func = null;
        #endregion

        /// <summary>
        /// �B��̃R���X�g���N�^
        /// </summary>
        /// <param name="function">�t�@�C�i���C�Y���鎞�Ɏ��s����f���Q�[�g���\�b�h</param>
        public FinalizeManager(Finalize function)
        {
            _func = function;
        }

        /// <summary>
        /// �f���Q�[�g�������\�b�h�����s����
        /// </summary>
        public bool Invoke()
        {
            if (_func != null)
            {
                _func();
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// �t�@�C�i���C�Y��ۑ�����
    /// </summary>
    public class FinalizeManageBuffer : IEnumerable
    {
        private class Enumerator : IEnumerator
        {
            private readonly IDictionary _dat1;
            private readonly IList _dat2;
            private int _curData = 0;
            private IEnumerator _cur;

            public Enumerator(IDictionary value1, IList value2)
            {
                _dat1 = value1;
                _dat2 = value2;
                Reset();
            }

            #region IEnumerator �����o

            public void Reset()
            {
                _cur = _dat1.Values.GetEnumerator();
                _curData = 0;
            }

            public object Current => _cur.Current;

            public bool MoveNext()
            {
                var ret = _cur.MoveNext();
                if (ret == false)
                {
                    if (_curData == 0)
                    {
                        _curData++;
                        _cur = _dat2.GetEnumerator();
                        return MoveNext();
                    }
                }
                return ret;
            }

            #endregion

        }
        #region �����i�V���A���C�Y���Ȃ��j
        /// <summary>�O���[�v�Ǘ�����t�@�C�i���C�U</summary>
        private readonly IDictionary _dat1 = new HybridDictionary();

        /// <summary>�O���[�v�Ǘ����Ȃ��t�@�C�i���C�U</summary>
        private readonly IList _dat2 = new ArrayList();
        #endregion

        /// <summary>
        /// �t�@�C�i���C�U��o�^����
        /// </summary>
        /// <param name="key">�t�@�C�i���C�Y�O���[�vID�iId����擾����ƕ֗��j</param>
        /// <param name="value">�t�@�C�i���C�U</param>
        public void Add(NamedId key, FinalizeManager.Finalize value)
        {
            _dat1[key] = new FinalizeManager(value);
        }
        /// <summary>
        /// �t�@�C�i���C�U��o�^����iID�Ȃ��̃t�@�C�i���C�U�͐�Ɏ��s�����j
        /// </summary>
        /// <param name="value">�t�@�C�i���C�U</param>
        public void Add(FinalizeManager.Finalize value)
        {
            _dat2.Add(new FinalizeManager(value));
        }

        /// <summary>
        /// �w��O���[�v�̃t�@�C�i���C�U���o�^����Ă��邩��������
        /// </summary>
        /// <param name="key">�O���[�v�ԍ�</param>
        /// <returns>true = �o�^����Ă��� / false = �o�^����Ă��Ȃ�</returns>
        public bool Contains(NamedId key)
        {
            return _dat1.Contains(key);
        }

        /// <summary>
        /// �o�^�ς݂̃t�@�C�i���C�U�����s���Ȃ��ō폜����
        /// </summary>
        public void Clear()
        {
            _dat1.Clear();
            _dat2.Clear();
        }

        /// <summary>
        /// �o�^���Ă���t�@�C�i���C�U�����ׂĎ��s����
        /// </summary>
        public bool Flush()
        {
            var cnt = 0;
            try
            {
                for (var en = GetEnumerator(); en.MoveNext();)
                {
                    var res = ((FinalizeManager)en.Current).Invoke();
                    if (res)
                    {
                        cnt++;
                    }
                }
                Clear();
            }
            catch (Exception ex)
            {
                LOG.WriteLineException(ex);
            }
            return cnt > 0;
        }

        #region IEnumerable �����o

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(_dat1, _dat2);
        }

        #endregion
    }
}
