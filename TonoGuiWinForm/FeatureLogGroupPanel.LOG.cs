using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ログ グローバルクラス
    /// </summary>
    /// <remarks>
    /// ■表示メッセージのプレフィックス
    /// @ONECE メッセージ・・・   → １度だけ表示する
    /// 
    /// ・WriteMesLine でのプレフィックス
    /// @E エラーレベルに設定
    /// @W 警告レベルに設定
    /// @I 情報レベルに設定
    /// @T TODO（アイコン付き）TODOレベル
    /// @H ヒント（アイコン付き）情報レベル
    /// @N 注意（アイコン付き）警告レベル
    /// </remarks>
    public static class LOG
    {
        internal class UnitF : Unit
        {
            private readonly Mes.Format _mesformat;
            public UnitF(LLV lv, Mes.Format mes, Image icon)
                : base(lv, "", icon)
            {
                _mesformat = mes;
            }

            /// <summary>
            /// 言語キー
            /// </summary>
            public override string Mes
            {
                get => _mesformat == null ? "" : _mesformat.ToString();
                set
                {
                }
            }
        }

        internal class Unit
        {
            private static int _no = 1;
            public int Seq;
            private string _mes;
            public LLV Level;
            private Image _icon = null;
            public Unit(LLV lv, string mes, Image icon)
            {
                Seq = _no++;
                Mes = mes;
                Level = lv;
                _icon = icon;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Unit tar))
                {
                    return false;
                }
                if (tar._mes == _mes)
                {
                    if (tar.Level == Level)
                    {
                        return true;
                    }
                }
                return false;
            }
            public override int GetHashCode()
            {
                return (_mes + Level.ToString()).GetHashCode();
            }

            public virtual string Mes
            {
                get => _mes;
                set => _mes = value;
            }
            public virtual Image Icon
            {
                get => _icon;
                set => _icon = value;
            }
        }

        private static bool _swInf = true;
        private static bool _swWar = true;
        private static bool _swErr = true;
        private static bool _swDev = true;
        private static bool _swTodo = true;
        private static readonly LinkedList<Unit> _todo = new LinkedList<Unit>();
        private static readonly LinkedList<Unit> _inf = new LinkedList<Unit>();
        private static readonly LinkedList<Unit> _err = new LinkedList<Unit>();
        private static readonly LinkedList<Unit> _war = new LinkedList<Unit>();
        private static readonly LinkedList<Unit> _dev = new LinkedList<Unit>();
        private static LinkedList<Unit> _cur = new LinkedList<Unit>();
        private static bool _isRequestedDraw = false;
        private static bool _isNoJump = false;  // 一度だけジャンプしない

        #region Stream系インターフェース
        /// <summary>
        /// Stream系インターフェース
        /// </summary>
        public class Writer : TextWriter
        {
            public LLV LogLevel = LLV.INF;
            public override Encoding Encoding => Encoding.Default;
            public override void Write(string value)
            {
                LOG.WriteLine(LogLevel, value);
            }
            public override void WriteLine()
            {
                LOG.WriteLine(LogLevel, "");
            }
            public override void WriteLine(string value)
            {
                LOG.WriteLine(LogLevel, value);
            }
            public override void Write(bool value) { Write(value.ToString()); }
            public override void Write(char value) { Write(value.ToString()); }
            public override void Write(char[] buffer) { throw new NotSupportedException(); }
            public override void Write(char[] buffer, int index, int count) { throw new NotSupportedException(); }
            public override void Write(decimal value) { Write(value.ToString()); }
            public override void Write(double value) { Write(value.ToString()); }
            public override void Write(float value) { Write(value.ToString()); }
            public override void Write(int value) { Write(value.ToString()); }
            public override void Write(long value) { Write(value.ToString()); }
            public override void Write(object value) { Write(value.ToString()); }
            public override void Write(string format, object arg0) { Write(string.Format(format, arg0)); }
            public override void Write(string format, object arg0, object arg1) { Write(string.Format(format, arg0, arg1)); }
            public override void Write(string format, object arg0, object arg1, object arg2) { Write(string.Format(format, arg0, arg1, arg2)); }
            public override void Write(string format, params object[] arg) { Write(string.Format(format, arg)); }
            public override void Write(uint value) { Write(value.ToString()); }
            public override void Write(ulong value) { Write(value.ToString()); }
            public override void WriteLine(bool value) { WriteLine(value.ToString()); }
            public override void WriteLine(char value) { WriteLine(value.ToString()); }
            public override void WriteLine(char[] buffer) { throw new NotSupportedException(); }
            public override void WriteLine(char[] buffer, int index, int count) { throw new NotSupportedException(); }
            public override void WriteLine(decimal value) { WriteLine(value.ToString()); }
            public override void WriteLine(double value) { WriteLine(value.ToString()); }
            public override void WriteLine(float value) { WriteLine(value.ToString()); }
            public override void WriteLine(int value) { WriteLine(value.ToString()); }
            public override void WriteLine(long value) { WriteLine(value.ToString()); }
            public override void WriteLine(object value) { WriteLine(value.ToString()); }
            public override void WriteLine(string format, object arg0) { Write(string.Format(format, arg0)); }
            public override void WriteLine(string format, object arg0, object arg1) { Write(string.Format(format, arg0, arg1)); }
            public override void WriteLine(string format, object arg0, object arg1, object arg2) { Write(string.Format(format, arg0, arg1, arg2)); }
            public override void WriteLine(string format, params object[] arg) { Write(string.Format(format, arg)); }
            public override void WriteLine(uint value) { WriteLine(value.ToString()); }
            public override void WriteLine(ulong value) { WriteLine(value.ToString()); }
        }
        private static Writer _writer = null;


        /// <summary>
        /// InfoレベルのStram
        /// </summary>
        public static Writer Stream
        {
            get
            {
                if (_writer == null)
                {
                    _writer = new Writer
                    {
                        LogLevel = LLV.INF
                    };
                }
                return _writer;
            }
        }

        private static Writer _writerDev = null;

        /// <summary>
        /// DevレベルのStream
        /// </summary>
        public static Writer StreamDev
        {
            get
            {
                if (_writerDev == null)
                {
                    _writerDev = new Writer
                    {
                        LogLevel = LLV.DEV
                    };
                }
                return _writerDev;
            }
        }

        private static Writer _writerErr = null;

        /// <summary>
        /// ErrレベルのStream
        /// </summary>
        public static Writer StreamErr
        {
            get
            {
                if (_writerErr == null)
                {
                    _writerErr = new Writer
                    {
                        LogLevel = LLV.ERR
                    };
                }
                return _writerErr;
            }
        }

        private static Writer _writerWar = null;

        /// <summary>
        /// WarレベルのStream
        /// </summary>
        public static Writer StreamWar
        {
            get
            {
                if (_writerWar == null)
                {
                    _writerWar = new Writer
                    {
                        LogLevel = LLV.WAR
                    };
                }
                return _writerWar;
            }
        }


        #endregion
        #region ログ追加イベント
        /// <summary>
        /// ログ追加イベントハンドラの引数
        /// </summary>
        public class LogAddedEventArgs : EventArgs
        {
            private readonly string _lastLine;
            private readonly LLV _lv;

            public LogAddedEventArgs(LLV lv, string lastLine)
            {
                _lv = lv;
                _lastLine = lastLine;
            }
            public LLV Level => _lv;
            public string LastLine => _lastLine;
        }

        /// <summary>
        /// ログが追加されたときのイベント
        /// </summary>
        public static event EventHandler<LogAddedEventArgs> LogAdded;

        /// <summary>
        /// ログがクリアされた時のイベント
        /// </summary>
        public static event EventHandler<EventArgs> LogClearRequested;
        #endregion

        private static readonly Dictionary<Unit, Unit> _dupcheck = new Dictionary<Unit, Unit>(); // 二重表示チェック用

        /// <summary>
        /// ログにメッセージを追加（アイコンつき）
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="mes"></param>
        /// <param name="img">アイコン。null＝非表示</param>
        /// <remarks>※ 基準になるメソッド</remarks>
        public static void WriteLine(LLV lv, string mes, Image img)
        {
            var isOnece = false;
            if (mes.StartsWith("@ONECE "))
            {
                isOnece = true;
                mes = mes.Substring(7);
            }
            var uf = new Unit(lv, mes, img);
            if (_dupcheck.ContainsKey(uf))
            {
                return;
            }
            if (isOnece)
            {
                _dupcheck[uf] = uf;
            }
            switch (lv)
            {
                case LLV.TODO:
                    _todo.AddLast(new LinkedListNode<Unit>(uf));
                    if (!_isNoJump)
                    {
                        JumpInf.Start();
                    }

                    _isNoJump = false;
                    if (_swTodo)
                    {
                        _cur.AddLast(new LinkedListNode<Unit>(uf));
                        _isRequestedDraw = true;
                    }
                    break;
                case LLV.INF:
                    _inf.AddLast(new LinkedListNode<Unit>(uf));
                    if (!_isNoJump)
                    {
                        JumpInf.Start();
                    }

                    _isNoJump = false;
                    if (_swInf)
                    {
                        _cur.AddLast(new LinkedListNode<Unit>(uf));
                        _isRequestedDraw = true;
                    }
                    break;
                case LLV.WAR:
                    _war.AddLast(new LinkedListNode<Unit>(uf));
                    if (!_isNoJump)
                    {
                        JumpWar.Start();
                    }

                    _isNoJump = false;
                    if (_swWar)
                    {
                        _cur.AddLast(new LinkedListNode<Unit>(uf));
                        _isRequestedDraw = true;
                    }
                    break;
                case LLV.ERR:
                    _err.AddLast(new LinkedListNode<Unit>(uf));
                    if (!_isNoJump)
                    {
                        JumpErr.Start();
                    }

                    _isNoJump = false;
                    if (_swErr)
                    {
                        _cur.AddLast(new LinkedListNode<Unit>(uf));
                        _isRequestedDraw = true;
                    }
                    break;
                case LLV.DEV:
                    _dev.AddLast(new LinkedListNode<Unit>(uf));
                    if (!_isNoJump)
                    {
                        JumpDev.Start();
                    }

                    _isNoJump = false;
                    if (_swDev)
                    {
                        _cur.AddLast(new LinkedListNode<Unit>(uf));
                        _isRequestedDraw = true;
                    }
                    break;
            }
            LogAdded?.Invoke(null, new LogAddedEventArgs(lv, uf.Mes));
        }

        /// <summary>
        /// ログをクリアする
        /// </summary>
        public static void Clear()
        {
            _todo.Clear();
            _inf.Clear();
            _err.Clear();
            _war.Clear();
            _dev.Clear();
            _cur.Clear();
            _dupcheck.Clear();
            _isRequestedDraw = true;

            if (LogClearRequested != null)
            {
                LogClearRequested(null, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 次のWriteでジャンプしない
        /// </summary>
        public static void NoJumpNext()
        {
            _isNoJump = true;
        }

        public static bool InfSw => _swInf;
        public static bool WarSw => _swWar;
        public static bool ErrSw => _swErr;
        public static bool DevSw => _swDev;

        /// <summary>
        /// 描画リクエストがあったか調べ、そのリクエストフラグをクリア（false）にする。
        /// →二回連続してこのメソッドをコールすると、値がfalseになる
        /// </summary>
        /// <returns></returns>
        public static bool CheckAndClearRequestFlag()
        {
            var ret = _isRequestedDraw;
            _isRequestedDraw = false;
            return ret;
        }

        /// <summary>
        /// 表示レベルを更新する（表示用データを更新するので時間がかかる）
        /// スイッチはトグル
        /// </summary>
        /// <param name="lv"></param>
        public static void ChangeDispLevel(LLV lv)
        {
            switch (lv)
            {
                case LLV.ERR: ChangeDispLevel(lv, !_swErr); break;
                case LLV.WAR: ChangeDispLevel(lv, !_swWar); break;
                case LLV.INF: ChangeDispLevel(lv, !_swInf); break;
                case LLV.DEV: ChangeDispLevel(lv, !_swDev); break;
                case LLV.TODO: ChangeDispLevel(lv, !_swTodo); break;
            }
        }

        /// <summary>
        /// 表示レベルを更新する（表示用データを更新するので時間がかかる）
        /// </summary>
        /// <param name="lv">変更したいレベル</param>
        /// <param name="sw">変更後の値</param>
        public static void ChangeDispLevel(LLV lv, bool sw)
        {
            switch (lv)
            {
                case LLV.TODO:
                    if (_swTodo != sw)
                    {
                        _swTodo = sw;
                        resetCurrentList();
                    }
                    break;
                case LLV.INF:
                    if (_swInf != sw)
                    {
                        _swInf = sw;
                        resetCurrentList();
                    }
                    break;
                case LLV.WAR:
                    if (_swWar != sw)
                    {
                        _swWar = sw;
                        resetCurrentList();
                    }
                    break;
                case LLV.ERR:
                    if (_swErr != sw)
                    {
                        _swErr = sw;
                        resetCurrentList();
                    }
                    break;
                case LLV.DEV:
                    if (_swDev != sw)
                    {
                        _swDev = sw;
                        resetCurrentList();
                    }
                    break;
            }
        }

        /// <summary>
        /// カレント表示用のリストを更新する
        /// </summary>
        private static void resetCurrentList()
        {
            _isRequestedDraw = true;    // アイコンジャンプは指定しない
            var dat = new List<Unit>();
            if (_swInf)
            {
                dat.AddRange(_inf);
            }

            if (_swWar)
            {
                dat.AddRange(_war);
            }

            if (_swErr)
            {
                dat.AddRange(_err);
            }

            if (_swDev)
            {
                dat.AddRange(_dev);
            }

            dat.Sort(delegate (Unit x, Unit y)
            {
                return x.Seq - y.Seq;
            });
            _cur = new LinkedList<Unit>();
            for (var i = 0; i < dat.Count; i++)
            {
                _cur.AddLast(dat[i]);
            }
        }

        /// <summary>
        /// 表示中のログを返す
        /// </summary>
        /// <returns></returns>
        public static string GetCurrent()
        {
            if (_cur.First == null)
            {
                return "";
            }
            else
            {
                var sb = new StringBuilder();
                for (var now = _cur.First; ; now = now.Next)
                {
                    sb.AppendLine(now.Value.Mes);
                    if (now.Next == null)
                    {
                        break;
                    }
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 現段階のログテキストをすべて取得する
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetAllLogText()
        {
            return
                from unit in _cur
                select unit.Mes;
        }

        /// <summary>
        /// 表示したいログの行数
        /// </summary>
        /// <returns></returns>
        internal static int GetCurrentCount()
        {
            return _cur.Count;
        }

        /// <summary>
        /// 最後のノードを返す
        /// </summary>
        /// <returns></returns>
        internal static LinkedListNode<Unit> GetCurrentLast()
        {
            return _cur.Last;
        }

        /// <summary>
        /// 最初のノードを返す
        /// </summary>
        /// <returns></returns>
        internal static LinkedListNode<Unit> GetCurrentFirst()
        {
            return _cur.First;
        }

        /// <summary>
        /// レベルを取得する
        /// </summary>
        /// <param name="s"></param>
        /// <param name="sout"></param>
        /// <returns></returns>
        private static LLV getLv(string s, out string sout, out Image img)
        {
            var lv = LLV.INF;
            img = null;
            if (s.StartsWith("@"))
            {
                var ls = s.Substring(1, 1);
                if (ls.Equals("T", StringComparison.CurrentCultureIgnoreCase))      // ＴＯＤＯ
                {
                    lv = LLV.TODO;
                    img = Properties.Resources.li_todo;
                }
                if (ls.Equals("E", StringComparison.CurrentCultureIgnoreCase))      // ERR
                {
                    lv = LLV.ERR;
                }
                if (ls.Equals("W", StringComparison.CurrentCultureIgnoreCase))      // WAR
                {
                    lv = LLV.WAR;
                }
                if (ls.Equals("H", StringComparison.CurrentCultureIgnoreCase))      // HINT
                {
                    lv = LLV.INF;
                    img = Properties.Resources.li_hint;
                }
                if (ls.Equals("N", StringComparison.CurrentCultureIgnoreCase))      // NOTE
                {
                    lv = LLV.WAR;
                    img = Properties.Resources.li_Note;
                }
                var id = s.IndexOf(' ');
                if (id >= 0)
                {
                    s = s.Substring(id + 1).Trim();
                }
            }
            sout = s;
            return lv;
        }

        /// <summary>
        /// 言語切り替え時に自動更新されるログ（パラメータ指定はできない）
        /// 言語別テキスト内の@Eなどの構文が使えない
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="form"></param>
        public static void WriteMesFormatLine(LLV lv, Mes.Format form)
        {
            WriteMesFormatLine(lv, form, null);
        }

        /// <summary>
        /// 言語切り替え時に自動更新されるログ（パラメータ指定はできない）
        /// </summary>
        /// <param name="form"></param>
        public static void WriteMesFormatLine(LLV lv, Mes.Format form, Image icon)
        {
            var uf = new UnitF(lv, form, icon);
            switch (lv)
            {
                case LLV.TODO:
                    _todo.AddLast(new LinkedListNode<Unit>(uf));
                    if (!_isNoJump)
                    {
                        JumpTodo.Start();
                    }

                    _isNoJump = false;
                    if (_swTodo)
                    {
                        _cur.AddLast(new LinkedListNode<Unit>(uf));
                        _isRequestedDraw = true;
                    }
                    break;
                case LLV.INF:
                    _inf.AddLast(new LinkedListNode<Unit>(uf));
                    if (!_isNoJump)
                    {
                        JumpInf.Start();
                    }

                    _isNoJump = false;
                    if (_swInf)
                    {
                        _cur.AddLast(new LinkedListNode<Unit>(uf));
                        _isRequestedDraw = true;
                    }
                    break;
                case LLV.WAR:
                    _war.AddLast(new LinkedListNode<Unit>(uf));
                    if (!_isNoJump)
                    {
                        JumpWar.Start();
                    }

                    _isNoJump = false;
                    if (_swWar)
                    {
                        _cur.AddLast(new LinkedListNode<Unit>(uf));
                        _isRequestedDraw = true;
                    }
                    break;
                case LLV.ERR:
                    _err.AddLast(new LinkedListNode<Unit>(uf));
                    if (!_isNoJump)
                    {
                        JumpErr.Start();
                    }

                    _isNoJump = false;
                    if (_swErr)
                    {
                        _cur.AddLast(new LinkedListNode<Unit>(uf));
                        _isRequestedDraw = true;
                    }
                    break;
                case LLV.DEV:
                    _dev.AddLast(new LinkedListNode<Unit>(uf));
                    if (!_isNoJump)
                    {
                        JumpDev.Start();
                    }

                    _isNoJump = false;
                    if (_swDev)
                    {
                        _cur.AddLast(new LinkedListNode<Unit>(uf));
                        _isRequestedDraw = true;
                    }
                    break;
            }
            LogAdded?.Invoke(null, new LogAddedEventArgs(lv, uf.Mes));
        }

        /// <summary>
        /// uMesのフォーマット文字列にパラメータを適用してログに表示する
        /// 表示レベルはINF。しかしメッセージ中 次の記号で、表示レベルが変更できる。
        /// @E TEST  → TESTというメッセージをエラーレベルで表示
        /// @W NOTE! → NOTE!というメッセージをワーニングレベルで表示
        /// </summary>
        /// <param name="key">uMesのキー</param>
        /// <param name="ver">uMesのバージョン</param>
        /// <param name="param"></param>
        public static void WriteMesLine(string key, string ver, params object[] param)
        {
            var s = Mes.Current[key, ver];
            var lv = getLv(s, out s, out var img);
            if (s.Length > 0)
            {
                WriteLine(lv, s, img, param);
            }
        }

        /// <summary>
        /// uMesのメッセージをログに表示する
        /// 表示レベルはINF。しかしメッセージ中 次の記号で、表示レベルが変更できる。
        /// @E TEST  → TESTというメッセージをエラーレベルで表示
        /// @W NOTE! → NOTE!というメッセージをワーニングレベルで表示
        /// </summary>
        /// <param name="key">uMesのキー</param>
        /// <param name="ver">uMesのバージョン</param>
        public static void WriteMesLine(string key, string ver)
        {
            var s = Mes.Current[key, ver];
            var lv = getLv(s, out s, out var img);
            if (s.Length > 0)
            {
                WriteLine(lv, s, img);
                //LogAdded?.Invoke(null, new LogAddedEventArgs(lv, s));
            }
        }

        /// <summary>
        /// ログにメッセージを追加（フォーマット）
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="mes"></param>
        public static void WriteLine(LLV lv, string format, Image image, params object[] prm)
        {
            WriteLine(lv, string.Format(format, prm), image);
        }

        /// <summary>
        /// ログにメッセージを追加
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="mes"></param>
        public static void WriteLine(LLV lv, string mes)
        {
            WriteLine(lv, mes, null);
        }

        /// <summary>
        /// 例外をログする
        /// </summary>
        /// <param name="ex"></param>
        public static void WriteLineException(Exception ex)
        {
            WriteLineException(LLV.DEV, ex);
        }

        /// <summary>
        /// 例外をログする
        /// </summary>
        /// <param name="ex"></param>
        public static void WriteLineException(LLV lv, Exception ex)
        {
            WriteLine(lv, ex.Message);
            Debug.Fail(ex.Message + "\r\n\r\n" + ex.StackTrace);
        }

        internal static Tono.GuiWinForm.FeatureLogGroupPanel.dpLogPanel.IconJumpState JumpTodo = new FeatureLogGroupPanel.dpLogPanel.IconJumpState();
        internal static Tono.GuiWinForm.FeatureLogGroupPanel.dpLogPanel.IconJumpState JumpInf = new FeatureLogGroupPanel.dpLogPanel.IconJumpState();
        internal static Tono.GuiWinForm.FeatureLogGroupPanel.dpLogPanel.IconJumpState JumpWar = new FeatureLogGroupPanel.dpLogPanel.IconJumpState();
        internal static Tono.GuiWinForm.FeatureLogGroupPanel.dpLogPanel.IconJumpState JumpErr = new FeatureLogGroupPanel.dpLogPanel.IconJumpState();
        internal static Tono.GuiWinForm.FeatureLogGroupPanel.dpLogPanel.IconJumpState JumpDev = new FeatureLogGroupPanel.dpLogPanel.IconJumpState();
    }

    /// <summary>
    /// ログレベル
    /// </summary>
    public enum LLV
    {
        /// <summary>
        /// 知っておくと便利な機能。知らなくても作業ができる
        /// </summary>
        INF = 4,

        /// <summary>
        /// 注意が必要。知らないで作業を続けると、不具合が生じる
        /// 自動的に代替案がセットされた場合など。
        /// </summary>
        WAR = 8,

        /// <summary>
        /// ユーザーが意図している作業が何らかの理由で中断されたこと。
        /// 代替案はユーザーが選ばなければならない場合に、ERRメッセージが表示される。
        /// </summary>
        ERR = 12,

        /// <summary>
        /// ユーザーの操作を促すメッセージ。
        /// </summary>
        TODO = 2,

        /// <summary>
        /// 開発用の情報。Release版では表示されなくて良いもの。
        /// 座標や処理の順番、オブジェクト詳細など。
        /// </summary>
        DEV = 1,
    }

}
