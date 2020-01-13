// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// TC（テキストコマンド）の１コマンドを表す基本クラス
    /// </summary>
    public class CommandBase
    {
        private readonly string _com;
        private readonly Dictionary<string, object> _operands = new Dictionary<string, object>();

        /// <summary>
        /// インスタンス複製
        /// </summary>
        /// <returns></returns>
        public CommandBase Clone()
        {
            var c = new CommandBase(_com);
            foreach (var kv in _operands)
            {
                c._operands[kv.Key] = kv.Value;
            }
            return c;
        }

        /// <summary>
        /// インスタンスの文字列を作成する（ファイルに記述されるtc部分（時刻などを除く）と同じ）
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(_com);
            sb.Append(" ");
            var isFirst = true;
            foreach (var kv in _operands)
            {
                if (isFirst == false)
                {
                    sb.Append(",");
                }
                else
                {
                    isFirst = false;
                }
                sb.Append(kv.Key);
                sb.Append("=");
                if (kv.Value is string)
                {
                    sb.Append("\"");
                }
                sb.Append(kv.Value);
                if (kv.Value is string)
                {
                    sb.Append("\"");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// コマンド（大文字・小文字の区別有り）
        /// </summary>
        public string Command => _com;

        /// <summary>
        /// オペランドの一覧を返す
        /// </summary>
        /// <returns></returns>
        public ICollection<string> GetOperandKeys()
        {
            return _operands.Keys;
        }

        /// <summary>
        /// オペランドなしでインスタンス構築
        /// </summary>
        /// <param name="com"></param>
        public CommandBase(string com)
        {
            _com = com;
        }

        /// <summary>
        /// オペランドの値を取得する
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetOperandString(string key)
        {
            if (_operands.TryGetValue(key, out var val))
            {
                if (val == null)
                {
                    return null;
                }

                return val.ToString();
            }
            else
            {
                throw new CommandException(string.Format("Syntax error:Operand '{0}' has not set yet.", key));
            }
        }

        /// <summary>
        /// 整数を取得する
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetOperandInt(string key)
        {
            if (_operands.TryGetValue(key, out var val))
            {
                if (val is int)
                {
                    return (int)val;
                }
            }
            var s = GetOperandString(key);
            try
            {
                return int.Parse(s);
            }
            catch (Exception)
            {
                throw new CommandException(string.Format("Illigal number error:'{0}' is not a number", s));
            }
        }

        /// <summary>
        /// 実数取得する
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public double GetOperandDouble(string key)
        {
            if (_operands.TryGetValue(key, out var val))
            {
                if (val is double)
                {
                    return (double)val;
                }
            }
            var s = GetOperandString(key);
            try
            {
                return double.Parse(s);
            }
            catch (Exception)
            {
                throw new CommandException(string.Format("Illigal number error:'{0}' is not a number", s));
            }
        }

        /// <summary>
        /// Booleanを取得する
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool GetOperandBool(string key)
        {
            if (_operands.TryGetValue(key, out var val))
            {
                if (val is bool)
                {
                    return (bool)val;
                }
            }
            var s = GetOperandString(key);
            try
            {
                return bool.Parse(s);
            }
            catch (Exception)
            {
                throw new CommandException(string.Format("Illigal number error:'{0}' is not a boolean type", s));
            }
        }
        /// <summary>
        /// オペランドを設定する
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public string SetOperand(string key, string val)
        {
            _operands[key] = val;
            return val;
        }

        /// <summary>
        /// オペランドを設定する
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public int SetOperand(string key, int val)
        {
            _operands[key] = val;
            return val;
        }

        /// <summary>
        /// オペランドを設定する
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public double SetOperand(string key, double val)
        {
            _operands[key] = val;
            return val;
        }


        /// <summary>
        /// オペランドを一つ指定してインスタンス構築
        /// </summary>
        /// <param name="com"></param>
        /// <param name="opkey1"></param>
        /// <param name="opval1"></param>
        public CommandBase(string com, params CommandOperand1[] operands)
        {
            _com = com;
            foreach (var op1 in operands)
            {
                _operands[op1.Key] = op1.Value;
            }
        }

        /// <summary>
        /// 文字列から新しいインスタンスを作成する
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static CommandBase Parse(string str)
        {
            var spid = str.IndexOf(' ');
            if (spid >= 0)
            {
                var com = str.Substring(0, spid).Trim();
                var op = str.Substring(spid + 1).Trim();
                var isDQ = false;
                var ret = new CommandBase(com);
                var sb = new StringBuilder();
                for (var i = 0; i < op.Length + 1; i++)
                {
                    char c;
                    if (i >= op.Length)
                    {
                        c = ',';
                        isDQ = false;
                    }
                    else
                    {
                        c = op[i];
                    }
                    if (c == '\"')
                    {
                        isDQ = !isDQ;
                    }
                    if (c == ',' && isDQ == false)
                    {
                        var op1 = sb.ToString().Trim();
                        var ideq = op1.IndexOf('=');
                        if (ideq >= 0)
                        {
                            var key = op1.Substring(0, ideq).Trim();
                            var val = op1.Substring(ideq + 1).Trim();
                            if (val.StartsWith("\"") && val.EndsWith("\""))
                            {
                                ret._operands[key] = val.Substring(1, val.Length - 2);
                            }
                            else
                            {
                                try
                                {
                                    if (val.Equals("false", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        ret._operands[key] = false;
                                    }
                                    else if (val.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        ret._operands[key] = true;
                                    }
                                    else if (val.IndexOf('.') >= 0 || val.IndexOf("e", StringComparison.CurrentCultureIgnoreCase) >= 0)
                                    {
                                        ret._operands[key] = double.Parse(val);
                                    }
                                    else
                                    {
                                        ret._operands[key] = int.Parse(val);
                                    }
                                }
                                catch (Exception)
                                {
                                    throw new CommandPlayException("Illegal value error : '{0}'", val);
                                }
                            }
                        }
                        sb.Remove(0, sb.Length);
                    }
                    sb.Append(c);
                }
                return ret;
            }
            throw new CommandPlayException("Syntax error : the text '{0}' is not for a tcBase.", str);
        }
    }
}
