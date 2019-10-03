using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uLog の概要の説明です。
    /// </summary>
    public class LogUtil : IDisposable
    {
        private enum logmode
        {
            Unknown, File, EventLogInfo, EventLogError, DebugConsole,
        }

        /// <summary>履歴保存一時ファイル</summary>
        protected StreamWriter _stream = null;
        /// <summary>ファイル名</summary>
        private readonly string _filename = string.Empty;
        private logmode _mode = logmode.Unknown;

        /// <summary>重複文字列保存防止</summary>
        private readonly IDictionary _noDupCheck = new Hashtable();

        /// <summary>
        /// シングルトン
        /// </summary>
        private static LogUtil _std = null;
        private static LogUtil _elInfo = null;
        private static LogUtil _elError = null;
        private static LogUtil _debugOut = null;

        #region イベントログ

        /// <summary>
        /// イベントログに情報（正常）を出力する
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
        /// イベントログに情報（エラー）を出力する
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
        /// Visualスタジオの出力
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
        /// イベントログ（情報）
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
        /// イベントログ（エラー）
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
        /// 標準ログ
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
        /// ログファイル名を作成する
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
        /// ストリームを開く
        /// </summary>
        private void _open()
        {
            if (_stream == null)
            {
                _stream = new StreamWriter(_filename, true, System.Text.Encoding.Unicode);
            }
        }

        /// <summary>
        /// 何もしないコンストラクタ
        /// </summary>
        private LogUtil()
        {
        }

        /// <summary>
        /// 唯一のコンストラクタ
        /// </summary>
        /// <param name="name">識別子</param>
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
        /// メッセージを追加する（同じメッセージが重なる時は１つだけ記録する）
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
        /// フォーマットでログを記録
        /// </summary>
        /// <param name="format">フォーマット文字列</param>
        /// <param name="args">値パラメータ</param>
        public void Add(string format, params object[] args)
        {
            Add(string.Format(format, args));
        }

        /// <summary>
        /// メッセージを追加する
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
        /// 例外をログに記録する
        /// </summary>
        /// <param name="title"></param>
        /// <param name="ex"></param>
        public void Add(string title, Exception ex)
        {
            Add(title + "\r\n\r\n" + ex.Message + "\r\n\r\n" + ex.StackTrace);
        }

        #region IDisposable メンバ

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
