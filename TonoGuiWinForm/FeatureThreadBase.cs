using System;
using System.Diagnostics;
using System.Threading;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �t�B�[�`���[���X���b�h�������{�N���X
    /// </summary>
    public abstract class FeatureThreadBase : FeatureControlBridgeBase, System.IDisposable
    {
        #region	����(�V���A���C�Y����)
        #endregion
        #region	����(�V���A���C�Y���Ȃ�)
        /// <summary>�X���b�h�̃n���h��</summary>
        private Thread _trd = null;
        /// <summary>�X���b�h�̗D��x(�����l�͕W��)</summary>
        protected System.Threading.ThreadPriority _priority = System.Threading.ThreadPriority.Normal;
        #endregion

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public FeatureThreadBase()
        {
        }

        /// <summary>
        /// �g�[�N���ɂ��N���C�x���g
        /// </summary>
        /// <param name="who"></param>
        public sealed override void Start(NamedId who)
        {
            if (_trd != null && _trd.IsAlive == false)
            {
                _trd = null;
            }
            System.Diagnostics.Debug.Assert(_trd == null, "�X���b�h�N�����ɕʂ�Token����������܂����B��d�N����L���ɂ���ɂ́A_trd��z��ɂ��ĊǗ����Ă�������");

            //base.Start (who);
            _trd = new Thread(new ThreadStart(Run))
            {
                IsBackground = true,       // �����TRUE����Ȃ��ƃA�v�����I�����Ȃ��Ȃ�̂Œ���
                Name = GetType().Name,
                Priority = _priority
            };
            _trd.Start();
            if (Pane.Control != null)
            {
                Pane.Control.Disposed += new EventHandler(parentControl_Disposed);
            }
        }

        /// <summary>
        /// �X���b�h�����c��Ȃ��悤�ȍH�v
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void parentControl_Disposed(object sender, EventArgs e)
        {
            if (_trd != null)
            {
                if (_trd.Join(10000) == false)
                {
                    Debug.WriteLine("Waiting thread [" + _trd.Name + "] end normally for 60sec.");
                    if (_trd.Join(60000) == false)
                    {
                        var name = _trd.Name;
                        _trd.Abort();
                        _trd = null;
                        Debug.WriteLine("Thread [" + name + "] is aborted forcely.");
                    }
                }
            }
        }

        /// <summary>
        /// �X���b�h�N�����Ɏ��s����鏈��
        /// (�X���b�h�ōs�������͂�����I�[�o�[���C�h���ċL�q���Ă�������)
        /// </summary>
        protected abstract void Run();

        /// <summary>
        /// �N�����̃X���b�h�������I������
        /// </summary>
        protected void Stop()
        {
            if (_trd != null)
            {
                _trd.Abort();       // ���s���̃X���b�h�������I��
            }
        }

        /// <summary>
        /// �X���b�h���w�莞��(�_�b)�����X���[�v������
        /// </summary>
        /// <param name="ms">�X���[�v���鎞��(�_�b)</param>
        protected void Sleep(int ms)
        {
            Thread.Sleep(ms);
        }

        /// <summary>
        /// �X���b�h�̗D��x�̎擾/�ݒ�
        /// </summary>
        /// ThreadPriority.Highest		�ō�
        /// ThreadPriority.AboveNormal	��
        /// ThreadPriority.Normal		��
        /// ThreadPriority.BelowNormal	��
        /// ThreadPriority.Lowest		�Œ�
        protected ThreadPriority Priority
        {
            get => _priority;
            set => _priority = value;
        }

        #region IDisposable �����o
        public new void Dispose()
        {
            base.Dispose(); // �I������
            Stop();
            if (_trd != null)
            {
                _trd.Join();    // �X���b�h���I������܂őҋ@���܂�
                _trd = null;
            }
        }
        #endregion
    }
}
