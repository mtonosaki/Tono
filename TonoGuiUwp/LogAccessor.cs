// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Tono.Gui.Uwp.LogAccessor;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// Log Level
    /// </summary>
    public enum LLV
    {
        Auto,
        ERR, WAR, INF, DEV,
    }

    /// <summary>
    /// global log accessor
    /// </summary>
    /// <example>
    /// LOG.WriteLine(LLV.DEV, "Tono\r\nMana").Select(a => a.Solo()).Count();
    /// LOG.AddMes(LLV.INF, "Start-Quickhelp").Solo();
    /// .Solo() means write log only onetime even if write twice (same log no twice display)
    /// </example>
    public static class LOG
    {
        public static LogAccessor Queue = LogAccessor.Current;

        public static Id NextID => RecordStr.NextID;

        /// <summary>
        /// max log count for memory saving
        /// </summary>
        public static int MaxLogCount { get => Queue.MaxLogCount; set => Queue.MaxLogCount = value; }

        /// <summary>
        /// add log from Exception
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static IEnumerable<RecordStr> AddException(Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex.StackTrace);
            return Queue.WriteLine(LLV.ERR, null, ex.Message);
        }

        /// <summary>
        /// add log from exception (and set additional message)
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="additionalMessage"></param>
        /// <returns></returns>
        public static IEnumerable<RecordStr> AddException(Exception ex, string additionalMessage)
        {
            Debug.Write(additionalMessage);
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex.StackTrace);
            return Queue.WriteLine(LLV.ERR, null, $"{additionalMessage} : {ex.Message}");
        }

        /// <summary>
        /// write log message
        /// </summary>
        /// <param name="level"></param>
        /// <param name="extendDesign">such as Icon</param>
        /// <param name="message"></param>
        public static IEnumerable<RecordStr> WriteLine(LLV level, ExtendDesign extendDesign, string message)
        {
            return Queue.WriteLine(level, extendDesign, message);
        }

        /// <summary>
        /// write log message (text only)
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        public static IEnumerable<RecordStr> WriteLine(LLV level, string message)
        {
            return Queue.WriteLine(level, null, message);
        }

        /// <summary>
        /// write log formatted message (support language runtime change)
        /// </summary>
        /// <param name="level"></param>
        /// <param name="messageFormatKey"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static RecordStr AddMes(LLV level, string messageFormatKey, params object[] args)
        {
            return Queue.AddMes(level, null, messageFormatKey, args);
        }

        /// <summary>
        /// write log formatted message (support language runtime change)
        /// </summary>
        /// <param name="level"></param>
        /// <param name="extendDesign">like icon object</param>
        /// <param name="messageFormatKey"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static RecordStr AddMes(LLV level, ExtendDesign extendDesign, string messageFormatKey, params object[] args)
        {
            return Queue.AddMes(level, extendDesign, messageFormatKey, args);
        }

        /// <summary>
        /// query all log message
        /// </summary>
        /// <returns></returns>
        public static string GetAllLogText()
        {
            return Queue.GetAllLogText();
        }
    }

    /// <summary>
    /// log writer
    /// </summary>
    public class LogAccessor
    {
        /// <summary>
        /// Default log accessor
        /// </summary>
        public static readonly LogAccessor Current = new LogAccessor();

        /// <summary>
        /// log data in memory
        /// </summary>
        private readonly LinkedList<RecordStr> _dat = new LinkedList<RecordStr>();

        /// <summary>
        /// log add event
        /// </summary>
        public event EventHandler<LogEventArgs> LogAdded;

        /// <summary>
        /// log record of language runtime change support
        /// </summary>
        public class RecordMes : RecordStr
        {
            /// <summary>
            /// message format key
            /// </summary>
            public string Key { get; set; }

            /// <summary>
            /// message optional parameter
            /// </summary>
            public string[] ArgStrings { get; set; }

            /// <summary>
            /// log level
            /// </summary>
            public override LLV Level
            {
                get
                {
                    if (base.Level == LLV.Auto)
                    {
                        var s = Mes.Get(Key, ArgStrings);
                        if (s.StartsWith("@E ")) { return LLV.ERR; }
                        if (s.StartsWith("@W ")) { return LLV.WAR; }
                        if (s.StartsWith("@I ")) { return LLV.INF; }
                        if (s.StartsWith("@D ")) { return LLV.DEV; }
                        return LLV.INF;
                    }
                    else
                    {
                        return base.Level;
                    }
                }
            }

            /// <summary>
            /// log message (get only)
            /// </summary>
            public override string Message
            {
                get
                {
                    var s = Mes.Get(Key, ArgStrings);
                    foreach (var com in new[] { "@E ", "@W ", "@I ", "@D ", "@SOLO ", "@ONES " })
                    {
                        s = s.Replace(com, "");
                    }
                    return s;
                }
                set => throw new NotSupportedException();
            }
        }

        /// <summary>
        /// log extend design base class
        /// </summary>
        public abstract class ExtendDesign
        {
        }

        /// <summary>
        /// Auto get image icon to show with GuiAssets.Image(...) function
        /// </summary>
        public class Image : ExtendDesign
        {
            /// <summary>
            /// image key of GuiAssets.Image(key)
            /// </summary>
            public string Key { get; set; }
        }

        /// <summary>
        /// Log record model
        /// </summary>
        public class RecordStr
        {
            public static Id NextID => new Id { Value = _idcounter };

            /// <summary>
            /// log record id
            /// </summary>
            public Id ID { get; private set; }

            /// <summary>
            /// log record UTC time
            /// </summary>
            public DateTime Utc { get; set; }

            /// <summary>
            /// log level
            /// </summary>
            public virtual LLV Level { get; set; }

            /// <summary>
            /// extend design command
            /// </summary>
            public ExtendDesign ExtendDesign { get; set; }

            /// <summary>
            /// log message
            /// </summary>
            public virtual string Message { get; set; }

            private static int _idcounter = 0;

            /// <summary>
            /// true=solo mode (same log no twice)
            /// </summary>
            private bool _isSolo = false;

            /// <summary>
            /// true=solo mode (same log no twice)
            /// </summary>
            public bool IsSolo => _isSolo;

            /// <summary>
            /// the constructor of this class
            /// </summary>
            public RecordStr()
            {
                ID = new Id { Value = ++_idcounter };
            }

            public override string ToString()
            {
                return $"{Level.ToString().Substring(0, 1)} {Message}";
            }

            /// <summary>
            /// set log record as Solo mode (same log not show twice)
            /// </summary>
            /// <returns></returns>
            public RecordStr Solo()
            {
                _isSolo = true;
                return this;
            }
        }

        /// <summary>
        /// log event parameter
        /// </summary>
        public class LogEventArgs : EventArgs
        {
            public RecordStr Record { get; internal set; }
        }

        /// <summary>
        /// log data count (including IsOnes record)
        /// </summary>
        public int Count => _dat.Count;

        /// <summary>
        /// get all log text (but MaxLogRecord limit)
        /// </summary>
        /// <returns></returns>
        public string GetAllLogText()
        {
            var logs = "";

            foreach (var rq in _dat)
            {
                logs += $"{rq.ID.Value}\t{rq.Level}\t{rq.Message}\n";
            }

            return logs;
        }

        /// <summary>
        /// Get last log record object
        /// </summary>
        public LinkedListNode<RecordStr> Last => _dat.Last;

        private int maxLogCount = 0;

        /// <summary>
        /// maximum log count setting for memory saving
        /// </summary>
        public int MaxLogCount
        {
            get => maxLogCount;
            set
            {
                maxLogCount = value;
                LOG.WriteLine(LLV.INF, $"Max Log Count = {maxLogCount}");
            }
        }

        /// <summary>
        /// <summary>
        /// </summary>
        /// <param name="level"></param>
        /// <param name="extendDesign"></param>
        /// <param name="meskey">message format key</param>
        /// <param name="args">message data</param>
        /// <returns></returns>
        public RecordStr AddMes(LLV level, ExtendDesign extendDesign, string meskey, params object[] args)
        {
            var rec = new RecordMes
            {
                Utc = DateTime.UtcNow,
                Level = level,
                ExtendDesign = extendDesign,
                Key = meskey,
                ArgStrings = (from obj in args select obj?.ToString()).ToArray(),
            };
            if (MaxLogCount < 1)
            {
                MaxLogCount = 4000;
            }
            lock (_dat)
            {
                _dat.AddLast(rec);
                while (_dat.Count > MaxLogCount)
                {
                    _dat.RemoveFirst(); // ぽかよけ
                }
            }
            LogAdded?.Invoke(this, new LogEventArgs
            {
                Record = rec,
            });
            return rec;
        }

        /// <summary>
        /// add log message
        /// </summary>
        /// <param name="level"></param>
        /// <param name="extendDesign"></param>
        /// <param name="mes"></param>
        /// <returns></returns>
        public IEnumerable<RecordStr> WriteLine(LLV level, ExtendDesign extendDesign, string message)
        {
            var ret = new List<RecordStr>();
            foreach (var mes0 in message.Split('\r'))
            {
                var mes1 = mes0.Replace("\n", "");
                var rec = new RecordStr
                {
                    Utc = DateTime.UtcNow,
                    Level = level,
                    ExtendDesign = extendDesign,
                    Message = mes1,
                };
                ret.Add(rec);
                if (MaxLogCount < 1)
                {
                    MaxLogCount = 4000;
                }
                lock (_dat)
                {
                    _dat.AddLast(rec);
                    while (_dat.Count > MaxLogCount)
                    {
                        _dat.RemoveFirst(); // Poka yoke
                    }
                }
                LogAdded?.Invoke(this, new LogEventArgs
                {
                    Record = rec,
                });
            }
            return ret;
        }
    }
}
