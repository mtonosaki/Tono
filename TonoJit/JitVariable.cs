// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Tono.Jit
{
    /// <summary>
    /// simulation variable object
    /// </summary>
    /// <remarks>
    /// OBJ.XXX --> ChildValues
    /// OBJ[XXX] --> KeyValuers
    /// </remarks>
    [JacTarget(Name = "Variable")]
    public partial class JitVariable
    {
        /// <summary>
        /// instance name
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// parent object
        /// </summary>
        public virtual JitVariable Parent { get; set; }

        private readonly ClassList classes = new ClassList();

        /// <summary>
        /// class collection (NOTE: set means "ADD")
        /// クラス名のコレクション（すべてのクラス名は、: から始まる文字列）
        /// 注意：Setプロパティは「追加」
        /// </summary>
        public ClassList Classes
        {
            get => classes;
            set => classes.Add(value);
        }

        /// <summary>
        /// value of this instance
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// reference to the Global variable
        /// </summary>
        public static JitVariable Global { get; set; }

        /// <summary>
        /// Having variables   PARENT.name = ***
        /// </summary>
        private Dictionary<JitVariable, JitVariable> KeyValues { get; set; }

        /// <summary>
        /// Child variables    PARENT[name] = ***
        /// </summary>
        public ChildValueDic ChildVriables { get; private set; }

        /// <summary>
        /// Child value accessor for Jac
        /// </summary>
        /// <param name="varname"></param>
        /// <returns></returns>
        [JacGetDotValue]
        public object GetChildValue(string varname)
        {
            return ChildVriables[varname]?.Value;
        }

        /// <summary>
        /// Child value accessor for Jac
        /// </summary>
        /// <param name="varname"></param>
        /// <param name="val"></param>
        [JacSetDotValue]
        public void SetChildValue(string varname, object val)
        {
            ChildVriables[varname] = JitVariable.FromObject(val);
        }

        /// <summary>
        /// the constructor of this class
        /// </summary>
        public JitVariable()
        {
            Classes = new ClassList
            {
                Class.Object,
            };
            KeyValues = new Dictionary<JitVariable, JitVariable>();
            ChildVriables = new ChildValueDic
            {
                Parent = this,
            };
        }

        public JitVariable Clone()
        {
            var ret = new JitVariable
            {
                Name = Name,
                Value = Value,
                Parent = Parent,
                KeyValues = new Dictionary<JitVariable, JitVariable>(KeyValues),
                ChildVriables = ChildVriables.Clone(),
                Classes = new ClassList(Classes),
            };
            return ret;
        }

        /// <summary>
        /// merge variable from other instance
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public JitVariable Merge(JitVariable from)
        {
            Name = from.Name;   // Name will be changed to "from" one.
            Value = from.Value; // Value will be changed to "from" one.
                                // Parent will NOT be changed
            foreach (var kv in KeyValues)   // upsert KeyValue objects
            {
                KeyValues[kv.Key] = kv.Value;
            }
            foreach (var name in from.ChildVriables.Names)
            {
                ChildVriables[name] = from.ChildVriables[name];
            }
            Classes.Add(from.Classes);

            return this;
        }
        public override string ToString()
        {
            if (Value is string)
            {
                return $"Name={Name} / \"{Value}\" / Classes={Classes.ToString()}";
            }
            else
            {
                return $"Name={Name} / {(Value ?? "null")} / Classes={Classes.ToString()}";
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is JitVariable va)
            {
                if (va.Value == null)
                {
                    return Value == null;
                }
                else
                {
                    return va.Value.Equals(Value);
                }
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }

        public static bool operator ==(JitVariable a, JitVariable b) { return a?.Equals(b) ?? b?.Equals(a) ?? true; }
        public static bool operator !=(JitVariable a, JitVariable b) { return !a?.Equals(b) ?? !b?.Equals(a) ?? false; }

        /// <summary>
        /// query class names
        /// </summary>
        /// <param name="instanceAndClassNames"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetClassNames(string instanceAndClassNames)
        {
            var cn = instanceAndClassNames.Trim();
            if (cn.StartsWith(":") == false)    // not start with ":" means "instance" (not "class")
            {
                cn = StrUtil.MidFind(cn, ":");
            }
            var col = instanceAndClassNames.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries).Select(a => ":" + a.Trim()).Where(a => a != ":");
            return col;
        }

        /// <summary>
        /// check there are the all specified classes in this instance
        /// </summary>
        /// <param name="instanceAndClassNames">:Int:Object format classes name(not consider sequece)</param>
        /// <returns>true = Have, false = not enough or not have</returns>
        /// <example>
        /// a:B:C
        /// a is :B == true
        /// a is :C == true
        /// a is :B:C == true
        /// a is :C:B == true
        /// a is :Hoge == false
        /// </example>
        public bool Is(string classNames)
        {
            var cns = classNames.Trim();
            if (cns.StartsWith(":") == false)
            {
                throw new SyntaxErrorException("string of class should start with : character");
            }
            return GetClassNames(cns).All(cn => Classes.Contains(cn));
        }

        /// <summary>
        /// check upper compatible class
        /// </summary>
        /// <param name="tar"></param>
        /// <returns></returns>
        /// <example>
        /// :C:D:E as :C:D == true
        /// :E:D:C as :C:D == true
        /// :C:D as :C:D:E == false
        /// </example>
        public bool As(JitVariable tar)
        {
            foreach (var c in tar.Classes)
            {
                if (Classes.Contains(c) == false)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// access to having value  THIS.key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public JitVariable this[JitVariable key]
        {
            get
            {
                if (KeyValues.TryGetValue(key, out JitVariable ret))
                {
                    return ret;
                }
                else
                {
                    var nv = Null();
                    KeyValues[key] = nv;
                    return nv;
                }
            }
            set => KeyValues[key] = value;
        }

        /// <summary>
        /// key collection of having variables  キーバリューマップのキー一覧（注意：チャイルドバリューマップのキーではない）
        /// </summary>
        public IEnumerable<JitVariable> Keys => KeyValues.Keys;

        public static long _dummyCount = 0;

        /// <summary>
        /// Null instance
        /// </summary>
        public static JitVariable Null(string classNames = null)
        {
            var ret = new JitVariable
            {
                Name = $"(noname_{++_dummyCount})",
                Value = null,
            };
            if (classNames != null)
            {
                ret.Classes.Set(classNames);
            }
            return ret;
        }

        /// <summary>
        /// make a new instance from double value (class = :Double:Object)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static JitVariable From(double value, string classNames = null)
        {
            var ret = new JitVariable
            {
                Name = $"(noname_{++_dummyCount})",
                Value = value,
            };
            if (classNames != null)
            {
                ret.Classes.Set(classNames);
            }
            else
            {
                ret.Classes.Add(Class.Double);
            }
            return ret;
        }

        /// <summary>
        /// make a new instance from int value (class = :Int:Object)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static JitVariable From(int value, string classNames = null)
        {
            var ret = new JitVariable
            {
                Name = $"(noname_{++_dummyCount})",
                Value = value,
            };
            if (classNames != null)
            {
                ret.Classes.Set(classNames);
            }
            else
            {
                ret.Classes.Add(Class.Int);
            }
            return ret;
        }

        /// <summary>
        /// make a new instance from string (class = :String:Object)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static JitVariable From(string value, string classNames = null)
        {
            var ret = new JitVariable
            {
                Name = $"(noname_{++_dummyCount})",
                Value = value,
            };
            if (classNames != null)
            {
                ret.Classes.Set(classNames);
            }
            else
            {
                ret.Classes.Add(Class.String);
            }
            return ret;
        }

        /// <summary>
        /// make a new instance copy from sorce variable (merge classes from source)
        /// </summary>
        /// <param name="src"></param>
        /// <param name="classNames"></param>
        /// <returns></returns>
        public static JitVariable From(JitVariable src, string classNames = null)
        {
            var ret = src.Clone();
            if (classNames != null)
            {
                ret.Classes.Add(classNames);    // コピー元のクラス情報を転送する
            }
            return ret;
        }

        /// <summary>
        /// make a new instance copy from obj (try to set class name from object type or set :Object)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="classNames"></param>
        /// <returns></returns>
        public static JitVariable FromObject(object obj, string classNames = null)
        {
            var ret = new JitVariable
            {
                Name = $"(noname_{++_dummyCount})",
                Value = obj,
            };
            if (classNames != null)
            {
                ret.Classes.Set(classNames);
            }
            else
            {
                if (obj is int)
                {
                    ret.Classes.Add(Class.Int);
                }
                else if (obj is double)
                {
                    ret.Classes.Add(Class.Double);
                }
                else if (obj is string)
                {
                    ret.Classes.Add(Class.String);
                }
                else if (obj is JitVariable jv)
                {
                    ret.Classes.Set(jv.Classes.ToArray());
                }
            }
            return ret;

        }

        /// <summary>
        /// parce JitVariable from string formatted aaa:bb:cc style
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static JitVariable Parse(string str)
        {
            Parse(str, out string val, out string classes);
            return From(val, classes);
        }

        /// <summary>
        /// get value and classes part from string
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <param name="classes"></param>
        public static void Parse(string str, out string value, out string classes)
        {
            str = str.Trim();
            if (str.Contains(':') == false) // no class
            {
                value = str;
                classes = "";
                return;
            }
            if (str.StartsWith(":"))    // no value
            {
                value = "";
                classes = str;
            }
            else
            {
                value = StrUtil.LeftBefore(str, ":");
                classes = StrUtil.MidFind(str, ":");
            }
        }
    }
}
