using System;
using System.Collections.Generic;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// 様々なイベントを捕獲（フィルター）する属性（IF文を少なくする施策）
    /// </summary>
    public class EventCatchAttribute : Attribute
    {
        /// <summary>
        /// 指定TokenIDの場合かつ、メソッドのパラメータにTokenIDがある場合にHIT
        /// </summary>
        public string TokenID { get; set; }

        /// <summary>
        /// 指定Nameの場合かつ、メソッドのパラメータにNameが有る場合にHIT
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 指定すれば、このステータスを評価する
        /// </summary>
        public string Status { get; set; }

        #region COMMAND CLASSES
        private interface ICheck
        {
            bool Check(IStatusManager stsmgr);
        }

        private abstract class NameValue
        {
            public string StatusName { get; set; }
            public string StatusValue { get; set; }
        }

        private class Equal : NameValue, ICheck
        {
            public bool Check(IStatusManager stsmgr)
            {
                if (stsmgr.TryGetStatus(StatusName, out var sts))
                {
                    return sts.IsOn(StatusValue);
                }
                else
                {
                    return false;
                }
            }
        }

        private class NotEqual : NameValue, ICheck
        {
            public bool Check(IStatusManager stsmgr)
            {
                if (stsmgr.TryGetStatus(StatusName, out var sts))
                {
                    return sts.IsNotOn(StatusValue);
                }
                else
                {
                    return false;
                }
            }
        }

        private class And : ICheck
        {
            public List<ICheck> Commands { get; set; } = new List<ICheck>();

            public bool Check(IStatusManager stsmgr)
            {
                return Commands.TrueForAll(a => a.Check(stsmgr));
            }
        }
        #endregion

        private And _op = null;

        /// <summary>
        /// ステータスをチェック：ステータスが存在しない場合は falseを返す
        /// </summary>
        /// <param name="stsmgr"></param>
        /// <returns></returns>
        public bool CheckStatus(IStatusManager stsmgr)
        {
            if (_op == null && Status != null)
            {
                _op = new And();

                var coa = Status.Split('&', StringSplitOptions.RemoveEmptyEntries);
                foreach (var a in coa)
                {
                    var idx = a.IndexOf("!=");
                    if (idx >= 0)
                    {
                        var eq = new NotEqual
                        {
                            StatusName = StrUtil.Left(a, idx).Trim(),
                            StatusValue = StrUtil.Mid(a, idx + 2),
                        };
                        _op.Commands.Add(eq);
                        continue;
                    }
                    idx = a.IndexOf("=");
                    if (idx >= 0)
                    {
                        var eq = new Equal
                        {
                            StatusName = StrUtil.Left(a, idx).Trim(),
                            StatusValue = StrUtil.Mid(a, idx + 1).Trim(),
                        };
                        _op.Commands.Add(eq);
                        continue;
                    }
                    else
                    {
                        if (a.Trim().StartsWith('!'))
                        {
                            _op.Commands.Add(new Equal
                            {
                                StatusName = a.Trim().Substring(1),
                                StatusValue = "False",
                            });
                        }
                        else
                        {
                            _op.Commands.Add(new NotEqual
                            {
                                StatusName = a.Trim(),
                                StatusValue = "False",
                            });
                        }
                    }
                }
            }
            return _op?.Check(stsmgr) ?? false;
        }

        /// <summary>
        /// ステータスフィルターが存在するかどうか
        /// </summary>
        /// <returns></returns>
        public bool IsStatusFilter()
        {
            return Status != null;
        }

        public override bool Equals(object obj)
        {
            if (obj is EventCatchAttribute ea)
            {
                return TokenID == ea.TokenID && Name == ea.Name;
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return $"{(TokenID ?? "(null)")}&{(Name ?? "(n/a)")}";
        }
    }
}
