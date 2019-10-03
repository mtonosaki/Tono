using System;
using System.Collections;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �X�g�b�v�E�H�b�`�N���X
    /// </summary>
    [NoTestClass]
    public class StopWatch
    {
        private readonly IList _buf = new ArrayList();
        private readonly string _name;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public StopWatch()
        {
            _name = "Stop";
            Start();
        }

        /// <summary>
        /// �������R���X�g���N�^
        /// </summary>
        /// <param name="name">�X�g�b�v�E�H�b�`�̖��O</param>
        public StopWatch(string name)
        {
            _name = name;
            Start();
        }

        /// <summary>
        /// �������R���X�g���N�^
        /// </summary>
        /// <param name="sw">TRUE:�v�����J�n����/FALSE:�v�����J�n���Ȃ�</param>
        public StopWatch(bool sw)
        {
            _name = "Stop";
            if (sw == true)
            {
                Start();
            }
        }

        /// <summary>
        /// �f�X�g���N�^�͂��Ă΂�邩�킩��Ȃ�
        /// </summary>
        ~StopWatch()
        {
            _buf.Clear();
        }

        public void Start()
        {
            if ((_buf.Count % 2) == 1)  // �v����
            {
                return;
            }
            _buf.Add(DateTime.Now.Ticks);
        }

        /// <summary>
        /// �v���J�n����̑��b����Ԃ�
        /// </summary>
        /// <returns>�ݐϕb</returns>
        public double span()
        {
            if (_buf.Count < 2)
            {
                return 0.0;
            }
            if ((_buf.Count % 2) == 1)  // �v����
            {
                return 0.0;
            }
            double dif = (long)_buf[_buf.Count - 1] - (long)_buf[0];
            return dif * 100 / 1000000000;
        }

        /// <summary>
        /// �v���J�n���猻�݂܂ł̌o�ߎ��ԁm�b�n��Ԃ�
        /// </summary>
        /// <returns>�ݐϕb��</returns>
        public double NowSpan()
        {
            if (_buf.Count == 0)
            {
                return 0.0;        // ���v��
            }

            if ((_buf.Count % 2) == 0)
            {
                return 0.0;  // �v���I����
            }

            double ret = DateTime.Now.Ticks - (long)_buf[0];
            return ret * 100 / 1000000000;
        }

        /// <summary>
        /// �X�g�b�v�E�H�b�`���~���ėݐϕb��Ԃ�
        /// </summary>
        /// <returns>�ݐϕb</returns>
        public double Stop()
        {
            _buf.Add(DateTime.Now.Ticks);
            var ret = span();
            return ret;
        }

        /// <summary>
        /// �X�g�b�v�E�H�b�`���~���ėݐϕb���f�o�b�O�o�͂ɕ\������
        /// </summary>
        public void StopAndDiag()
        {
            var r = Stop();
            System.Diagnostics.Debug.WriteLine("> " + _name + " at " + DateTime.Now.ToString() + " Result = " + (r * 1000).ToString("0.000") + " ms");
        }
    }
}
