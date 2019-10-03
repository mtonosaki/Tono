using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uLog �̊T�v�̐����ł��B
    /// </summary>
    public class LogUtil : IDisposable
    {
        private enum logmode
        {
            Unknown, File, EventLogInfo, EventLogError, DebugConsole,
        }

        /// <summary>����ۑ��ꎞ�t�@�C��</summary>
        protected StreamWriter _stream = null;
        /// <summary>�t�@�C����</summary>
        private readonly string _filename = string.Empty;
        private logmode _mode = logmode.Unknown;

        /// <summary>�d��������ۑ��h�~</summary>
        private readonly IDictionary _noDupCheck = new Hashtable();

        /// <summary>
        /// �V���O���g��
        /// </summary>
        private static LogUtil _std = null;
        private static LogUtil _elInfo = null;
        private static LogUtil _elError = null;
        private static LogUtil _debugOut = null;

        #region �C�x���g���O

        /// <summary>
        /// �C�x���g���O�ɏ��i����j���o�͂���
        /// </summary>
        /// <param name="s"></param>
        private static void elInfo(string s)
        {
            try
            {
                System.Diagnostics.EventLog.WriteEntry(System.Windows.Forms.Application.ProductName + System.Windows.Forms.Application.ProductVersion, s, System.Diagnostics.EventLogEntryType.Information);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// �C�x���g���O�ɏ��i�G���[�j���o�͂���
        /// </summary>
        /// <param name="s"></param>
        private static void elErr(string s)
        {
            try
            {
                System.Diagnostics.EventLog.WriteEntry(System.Windows.Forms.Application.ProductName + System.Windows.Forms.Application.ProductVersion, s, System.Diagnostics.EventLogEntryType.Error);
            }
            catch (Exception)
            {
            }
        }
        #endregion

        /// <summary>
        /// Visual�X�^�W�I�̏o��
        /// </summary>
        public static LogUtil VSOut
        {
            get
            {
                if (_debugOut == null)
                {
                    _debugOut = new LogUtil
                    {
                        _mode = logmode.DebugConsole
                    };
                }
                return _debugOut;
            }
        }

        /// <summary>
        /// �C�x���g���O�i���j
        /// </summary>
        public static LogUtil EventLogInfo
        {
            get
            {
                if (_elInfo == null)
                {
                    _elInfo = new LogUtil
                    {
                        _mode = logmode.EventLogInfo
                    };
                }
                return _elInfo;
            }
        }

        /// <summary>
        /// �C�x���g���O�i�G���[�j
        /// </summary>
        public static LogUtil EventLogErr
        {
            get
            {
                if (_elError == null)
                {
                    _elError = new LogUtil
                    {
                        _mode = logmode.EventLogError
                    };
                }
                return _elError;
            }
        }

        /// <summary>
        /// �W�����O
        /// </summary>
        public static LogUtil Std
        {
            get
            {
                if (_std == null)
                {
                    _std = new LogUtil("std");
                }
                return _std;
            }
        }

        /// <summary>
        /// ���O�t�@�C�������쐬����
        /// </summary>
        /// <returns></returns>
        private string getFileName(string name)
        {
            var t = Environment.GetEnvironmentVariable("Temp");
            var proc = System.Diagnostics.Process.GetCurrentProcess();

            if (t == "")
            {
                var ass = System.Reflection.Assembly.GetEntryAssembly();
                t = ass.CodeBase;
                var id = t.LastIndexOf('/');
                t = t.Substring(0, id);
            }
            var fn = new Uri(t + "/uLog." + name + "." + proc.Id.ToString() + ".log");
            return fn.LocalPath;
        }

        /// <summary>
        /// �X�g���[�����J��
        /// </summary>
        private void _open()
        {
            if (_stream == null)
            {
                _stream = new StreamWriter(_filename, true, System.Text.Encoding.Unicode);
            }
        }

        /// <summary>
        /// �������Ȃ��R���X�g���N�^
        /// </summary>
        private LogUtil()
        {
        }

        /// <summary>
        /// �B��̃R���X�g���N�^
        /// </summary>
        /// <param name="name">���ʎq</param>
        public LogUtil(string fileid)
        {
            if (fileid == null || fileid.Length < 1)
            {
                fileid = "default";
            }
            _filename = getFileName(fileid);
            _mode = logmode.File;
        }

        /// <summary>
        /// ���b�Z�[�W��ǉ�����i�������b�Z�[�W���d�Ȃ鎞�͂P�����L�^����j
        /// </summary>
        /// <param name="str"></param>
        public void AddOne(string str)
        {
            lock (_noDupCheck.SyncRoot)
            {
                if (_noDupCheck.Contains(str))
                {
                    return;
                }
                _noDupCheck[str] = true;
            }
            Add(str);
        }

        /// <summary>
        /// �t�H�[�}�b�g�Ń��O���L�^
        /// </summary>
        /// <param name="format">�t�H�[�}�b�g������</param>
        /// <param name="args">�l�p�����[�^</param>
        public void Add(string format, params object[] args)
        {
            Add(string.Format(format, args));
        }

        /// <summary>
        /// ���b�Z�[�W��ǉ�����
        /// </summary>
        /// <param name="str"></param>
        public void Add(string str)
        {
            switch (_mode)
            {
                case logmode.EventLogInfo:
                    elInfo(str);
                    break;
                case logmode.EventLogError:
                    elErr(str);
                    break;
                case logmode.File:
                    _open();
                    var st = new System.Diagnostics.StackTrace(true);
                    var now = DateTime.Now;
                    _stream.Write(now.ToString("G"));
                    var t = (now.Ticks / 10000) % 1000;
                    _stream.Write(" (" + t.ToString() + ")\t: ");
                    _stream.WriteLine(str);
                    _stream.Flush();
                    break;
                case logmode.DebugConsole:
                    Debug.WriteLine(str);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// ��O�����O�ɋL�^����
        /// </summary>
        /// <param name="title"></param>
        /// <param name="ex"></param>
        public void Add(string title, Exception ex)
        {
            Add(title + "\r\n\r\n" + ex.Message + "\r\n\r\n" + ex.StackTrace);
        }

        #region IDisposable �����o

        public void Dispose()
        {
            if (_stream != null)
            {
                _stream.Close();
                _stream = null;
            }
        }

        #endregion
    }
}
