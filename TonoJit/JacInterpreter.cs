// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using static Tono.Jit.Utils;

/// <summary>
/// Just-in-time model as a Code interpreter
/// </summary>
/// <see cref="UnitTestProject1.TonoJit_JaC.cs"/>
/// <remarks>
/// [1] to create a new instance.
/// new [Type]
/// 
/// [1-1] get return
/// a = new [Type]
/// 
/// ** example **
/// a = new Process
/// 
/// [2] to create a new initialized instance
/// new [Type1] 
/// ____[Property1-1] = [Object1-1]
/// ____[Property1-2] = [Object1-2]
/// 
/// ** example **
/// new Process
///     Name = "TestProcess"
///     
/// [3] nest property
/// new [Type1]
/// ____[Property1-1]
/// ____ ___[Property1-1-1] = [Object1-1-1]
/// ____ ___[Property1-1-2] = new [Type1-1-2]
/// ____ ___ ___[Propery1-1-2-1] = [Object1-1-2-1]
/// ____ ___[Property1-1-3] = [Variable1-1-3] new [Type1-1-3]
/// ____ ___ ___[Property1-1-3-1] = [Object1-1-3-1]
/// 
/// 
/// [4] Collection
/// new [Type1]
/// ____[Property1-1]  // collection property that set [JacListAdd] or [JacListRemove]
/// ____ ___add [Object1-1-1]
/// ____ ___add [Variable1-1-2] = [Object1-1-2]
/// ____ ___remove [Object1-1-3]
/// ____ ___add new [Type1-1-4]
/// ____ ___ ___[Property1-1-4-1] = [Object1-1-4-1]
/// </remarks>
/// <example>
/// </example>
namespace Tono.Jit
{
    /// <summary>
    /// Jac : Jit model as a Code
    /// Interpreter object
    /// </summary>
    public class JacInterpreter
    {
        private static readonly Dictionary<string/*JacTarget.Name*/, Type> JacTargets = new Dictionary<string, Type>();
        private readonly Stack<(int Level, string Com)> rpnStack = new Stack<(int Level, string Com)>();
        private readonly Stack<(int Level, string Com)> opeStack = new Stack<(int Level, string Com)>();
        private readonly Dictionary<string/*instance Key*/, object> instanceBuf = new Dictionary<string, object>();
        private readonly Dictionary<string/*variable name*/, object> varBuf = new Dictionary<string, object>();
        private readonly List<List<string>> Lines = new List<List<string>>();
        private readonly List<List<int>> Levels = new List<List<int>>();
        private readonly List<string> removeRequestInstance = new List<string>();    // instanceBuf.Remove(...);
        private readonly List<string> removeRequestVarBuf = new List<string>();      // varBuf.Remove(...);
        private int TargetLevel;
        private int instanceIdCounter = 0;
        private const int tabspace = 4;

        /// <summary>
        /// Reset Jac interpreter instance
        /// </summary>
        public void Reset()
        {
            rpnStack.Clear();
            opeStack.Clear();
            instanceBuf.Clear();
            varBuf.Clear();
            removeRequestInstance.Clear();
            removeRequestVarBuf.Clear();
            Lines.Clear();
            Levels.Clear();
            TargetLevel = 0;
            instanceIdCounter = 0;
        }

        /// <summary>
        /// Static constructor
        /// </summary>
        static JacInterpreter()
        {
            RegisterJacTarget(typeof(JacInterpreter).Assembly);
        }

        /// <summary>
        /// Parse command
        /// </summary>
        /// <param name="code"></param>
        public void Exec(string code)
        {
            // PREPARING
            NormalizeCode(code);

            // PARSING
            for (var iChunk = 0; iChunk < Levels.Count; iChunk++)
            {
                for (var iLine = 0; iLine < Lines[iChunk].Count; iLine++)
                {
                    var line = Lines[iChunk][iLine];
                    var level = Levels[iChunk][iLine];
                    var isNextLine = iLine < Lines[iChunk].Count - 1;
                    var blocks = StrUtil.SplitConsideringQuatationContainsSeparator(line, new[] { '=', '-', '>', ':', ' ' }, true, true, false).ToList();

                    ExecPass1(blocks);              // PASS-1
                    ExecPass2(blocks);              // PASS-2
                    ExecPushPhase(level, blocks);   // PUSH Phase

                    // POP PHASE =========================================
                    var isNextIndent = false;
                    var isSameIndent = false;
                    var isBackIndent = false;
                    var isNextAddRemove = false;
                    var nextLineLevel = int.MaxValue;
                    if (isNextLine)
                    {
                        isNextIndent = Levels[iChunk][iLine + 1] > Levels[iChunk][iLine];
                        isBackIndent = Levels[iChunk][iLine + 1] < Levels[iChunk][iLine];
                        isSameIndent = Levels[iChunk][iLine + 1] == Levels[iChunk][iLine];
                        nextLineLevel = Levels[iChunk][iLine + 1];
                        var nextTrimmed = Lines[iChunk][iLine + 1].Trim();
                        isNextAddRemove = nextTrimmed.StartsWith("add") | nextTrimmed.StartsWith("remove");
                    }
                    ExecPopPhase(isNextLine, isNextIndent, isSameIndent, isBackIndent, isNextAddRemove, nextLineLevel);
                }
            }

            foreach (var key in removeRequestInstance)
            {
                instanceBuf.Remove(key);
            }
            foreach (var key in removeRequestVarBuf)
            {
                varBuf.Remove(key);
            }
        }

        private void ExecPopPhase(bool isNextLine, bool isNextIndent, bool isSameIndent, bool isBackIndent, bool isNextAddRemove, int nextLineLevel)
        {
            if (opeStack.Count > 0 && isNextIndent)
            {
                var ope = opeStack.Peek();
                switch (ope.Com)
                {
                    case "new":
                        opeStack.Pop();
                        ProcNew(rpnStack);
                        break;
                }
            }
            if (opeStack.Count > 0 && (isBackIndent || isSameIndent || isNextLine == false))
            {
                TargetLevel = (nextLineLevel - 1) / tabspace * tabspace;
                bool isStackComsuming;
                do
                {
                    isStackComsuming = !isNextLine;
                    var ope = opeStack.Peek();
                    switch (ope.Com)
                    {
                        case "=":
                            opeStack.Pop();
                            ProcSet(rpnStack);
                            break;
                        case "add":
                            opeStack.Pop();
                            ProcList(rpnStack, isNextAddRemove, true);
                            break;
                        case "remove":
                            opeStack.Pop();
                            ProcList(rpnStack, isNextAddRemove, false);
                            break;
                        case "new":
                            opeStack.Pop();
                            ProcNew(rpnStack);
                            break;
                        case "->":
                        case "=>":
                        case ":":
                            opeStack.Pop();
                            ProcPair(rpnStack, ope.Com);
                            break;

                    }
                    if (isStackComsuming == false && opeStack.Count > 0)
                    {
                        var nextope = opeStack.Peek();
                        if (nextope.Level > TargetLevel) // || nextope.Com == "=" && nextope.Level >= targetLevel)
                        {
                            isStackComsuming = true;
                        }
                    }
                } while (isStackComsuming && opeStack.Count > 0);
            }
        }

        private static void ExecPass1(List<string> blocks)
        {
            for (var i = 0; i < blocks.Count - 1; i++)
            {
                if (blocks[i] == "=" && blocks[i + 1] == ">")
                {
                    blocks[i] = "=>";
                    blocks.RemoveAt(i + 1);
                }
                if (blocks[i] == "-")
                {
                    blocks[i] = $"-{blocks[i + 1]}";
                    blocks.RemoveAt(i + 1);
                }
            }
        }

        private static void ExecPass2(List<string> blocks)
        {
            var reg = new Regex(@"^[-+]?[0-9]+(\.[0-9]+)?[eE]$");
            for (var i = 0; i < blocks.Count - 1; i++)
            {
                if (reg.IsMatch(blocks[i]) && blocks[i + 1].StartsWith("-"))
                {
                    blocks[i] = blocks[i] + blocks[i + 1];
                    blocks.RemoveAt(i + 1);
                }
            }
        }

        private void ExecPushPhase(int level, List<string> blocks)
        {
            for (var i = 0; i < blocks.Count; i++)
            {
                var com = blocks[i];
                switch (com)
                {
                    case "=":
                    case "->":
                    case ":":
                    case "new":
                    case "add":
                    case "remove":
                        opeStack.Push((level, com));
                        break;
                    default:
                        rpnStack.Push((level, com));
                        break;
                }
            }
        }


        /// <summary>
        /// Parse command from JitTemplate
        /// </summary>
        /// <param name="template"></param>
        public void Exec(JitTemplate template)
        {
            foreach (var block in template.GetBlocks())
            {
                Exec(block);
            }
        }

        /// <summary>
        /// Instanciate from code
        /// </summary>
        /// <param name="jac"></param>
        /// <returns></returns>
        public static JacInterpreter From(string jac)
        {
            var ret = new JacInterpreter();
            ret.Exec(jac);
            return ret;
        }

        /// <summary>
        /// Instanciate from code
        /// </summary>
        /// <param name="jac"></param>
        /// <returns></returns>
        public static JacInterpreter From(JitTemplate jac)
        {
            var ret = new JacInterpreter();
            ret.Exec(jac);
            return ret;
        }

        /// <summary>
        /// Normalize code string
        /// </summary>
        /// <param name="code"></param>
        private void NormalizeCode(string code)
        {
            Lines.Clear();
            Levels.Clear();
            rpnStack.Clear();
            opeStack.Clear();
            code = code.Replace('\r', '\n');
            code = code.Replace("\n\n", "\n");
            var lines = code.Split('\n').Select(a => CleanLine(a)).Where(a => a.Trim().Length > 0).ToArray();
            var levels = Collection<int>.Rep(0, lines.Length).ToArray();
            TargetLevel = int.MaxValue;

            for (var iLine = 0; iLine < lines.Length; iLine++)
            {
                var line = lines[iLine];
                int nSp = 0;
                for (int i = 0; i < line.Length; i++)
                {
                    var c = line[i];
                    if (c == ' ')
                    {
                        nSp++;
                    }
                    else
                    if (c == '\t')
                    {
                        nSp = nSp / tabspace + tabspace;
                    }
                    else
                    {
                        levels[iLine] = nSp / tabspace * tabspace;
                        lines[iLine] = line.Trim();
                        break;
                    }
                }
            }
            var min = levels.Min();
            levels = levels.Select(a => a - min).ToArray();

            if (levels.Length < 0)
            {
                throw new JacException(JacException.Codes.SyntaxError, $"No data");
            }

            if (levels[0] != 0)
            {
                throw new JacException(JacException.Codes.SyntaxError, $"Indent error. It should be no indent for first line.");
            }

            List<int> lvs = null;
            List<string> lis = null;
            for (var i = 0; i < levels.Length; i++)
            {
                if (levels[i] == 0)
                {
                    Levels.Add(lvs = new List<int>());
                    Lines.Add(lis = new List<string>());
                }
                lvs.Add(levels[i]);
                lis.Add(lines[i]);
            }
        }

        private void ProcPair(Stack<(int Level, string Com)> rpnStack, string ope)
        {
            if (rpnStack.Count < 1)
            {
                throw new JacException(JacException.Codes.SyntaxError, $"Cannot set '=' when no value");
            }

            if (rpnStack.Count < 2)
            {
                throw new JacException(JacException.Codes.SyntaxError, $"Cannot set '=' when no left value");
            }

            var right = rpnStack.Pop();     // left value
            var left = rpnStack.Pop();      // right value
            if (ope == "->")
            {
                var obj = new JacPushLinkDescription
                {
                    Name = MakeID("PushFromTo_"),
                    From = ParseValue(left.Com),
                    To = ParseValue(right.Com),
                };
                SetVarBuf(obj.Name, obj);
                rpnStack.Push((left.Level, obj.Name));
            }
            if (ope == ":")
            {
                var name = MakeID("Tuple_");
                var v1 = ParseValue(left.Com);
                var v2 = ParseValue(right.Com);
                object obj = null;
                if (v1 is JitSubset && (v2 == null || v2 is JitProcess))
                {
                    obj = new ValueTuple<JitSubset, JitProcess>((JitSubset)v1, (JitProcess)v2);
                }
                else
                {
                    obj = new ValueTuple<object, object>(v1, v2);
                }
                SetVarBuf(name, obj);
                rpnStack.Push((left.Level, name));
            }
        }

        private void ProcSet(Stack<(int Level, string Com)> rpnStack)
        {
            if (rpnStack.Count < 1)
            {
                throw new JacException(JacException.Codes.SyntaxError, $"Cannot set '=' when no value");
            }

            if (rpnStack.Count < 2)
            {
                throw new JacException(JacException.Codes.SyntaxError, $"Cannot set '=' when no left value");
            }

            var val = rpnStack.Pop();           // value name
            var item = ParseValue(val.Com);     // value object
            var variable = rpnStack.Pop();      // variable name

            int dotid;
            if ((dotid = variable.Com.IndexOf('.')) >= 0)
            {
                // varname.varname の場合、ChildValuesに保存する
                var parentVar = StrUtil.Left(variable.Com, dotid);
                var childVar = StrUtil.Mid(variable.Com, dotid + 1);
                var parentVarObj = ParseValue(parentVar);

                // to try to set into property like [obj.Name]
                var pp = parentVarObj.GetType().GetProperty(childVar);
                if (pp != null && pp.PropertyType.Equals(item?.GetType()))
                {
                    pp?.SetValue(parentVarObj, item);
                }

                // To set to JacSetDotValue
                var tarmethods =
                    from me in parentVarObj.GetType().GetMethods()
                    from at in me.GetCustomAttributes<JacSetDotValueAttribute>(true)
                    select me;
                tarmethods.FirstOrDefault()?.Invoke(parentVarObj, new object[] { childVar, item });
            }
            if (rpnStack.Count > 0)
            {
                var objName = rpnStack.Peek();  // parent object name
                if ((IsInstance(objName.Com) || varBuf.ContainsKey(objName.Com)) && objName.Level < variable.Level)
                {
                    var obj = ParseValue(objName.Com);
                    var pi = obj?.GetType().GetProperty(variable.Com);
                    if (pi != null)
                    {
                        // Support lazy Set
                        if (pi.GetMethod.ReturnParameter.ParameterType.Name == "Func`1")
                        {
                            switch (item?.GetType().Name)
                            {
                                case "JitStage": pi.SetValue(obj, (Func<JitStage>)(() => (JitStage)item)); break;
                                case "JitProcess": pi.SetValue(obj, (Func<JitProcess>)(() => (JitProcess)item)); break;
                                case "JitKanban": pi.SetValue(obj, (Func<JitKanban>)(() => (JitKanban)item)); break;
                                case "JitVariable": pi.SetValue(obj, (Func<JitVariable>)(() => (JitVariable)item)); break;
                                case "JitWork": pi.SetValue(obj, (Func<JitWork>)(() => (JitWork)item)); break;
                                case "String":
                                    // Super Lazy Set (Support name only no instance condition)
                                    var expectedType = StrUtil.MidSkip(pi.GetMethod.ReturnParameter.ParameterType.FullName, @"System.Func`1\[\[Tono\.Jit\.");
                                    expectedType = StrUtil.LeftBefore(expectedType, ",");
                                    switch (expectedType)
                                    {
                                        case "JitProcess": SetSuperLazyIfPossible<JitProcess>(item, obj, pi); break;
                                        case "JitStage": SetSuperLazyIfPossible<JitStage>(item, obj, pi); break;
                                        case "JitKanban": SetSuperLazyIfPossible<JitKanban>(item, obj, pi); break;
                                        case "JitVariable": SetSuperLazyIfPossible<JitVariable>(item, obj, pi); break;
                                        case "JitWork": SetSuperLazyIfPossible<JitWork>(item, obj, pi); break;
                                        default: throw new JacException(JacException.Codes.NotSupportLazyMethodType, $"Not supported Func<{item?.GetType().Name}> in Jit class.");
                                    }
                                    break;
                                default: throw new JacException(JacException.Codes.NotSupportLazyMethodType, $"Not supported Func<{item?.GetType().Name}> property.");
                            }
                        }
                        // Normal Set
                        else
                        {
                            var isSet = false;
                            var requestedTypeName = pi.GetMethod.ReturnParameter.ParameterType.Name;
                            if (requestedTypeName == "JitVariable" && item != null && item is JitVariable == false)
                            {
                                pi.SetValue(obj, JitVariable.FromObject(item));
                                isSet = true;
                                // TODO: Save ID Instance to varBuf here.
                            }
                            if (requestedTypeName == "String")
                            {
                            }
                            if (isSet == false)
                            {
                                pi.SetValue(obj, item);
                                if (variable.Com.Equals("ID", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    SetVarBuf(item?.ToString() ?? "null", obj);
                                }
                                if (variable.Com.Equals("Name", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    SetVarBuf(item?.ToString() ?? "null", obj);
                                }
                            }
                        }
                        return;
                    }
                    else
                    {
                        var tarmethods =
                            from me in obj.GetType().GetMethods()
                            from at in me.GetCustomAttributes<JacSetDotValueAttribute>(true)
                            select me;
                        var pp = tarmethods.FirstOrDefault();
                        if (pp != null)
                        {
                            pp.Invoke(obj, new object[] { variable.Com, item });
                        }
                        else
                        {
                            throw new JacException(JacException.Codes.NotImplementedProperty, $"Property name '{variable.Com}' is not implemented yet in {(obj?.GetType().Name ?? "null")}.");
                        }
                        return;
                    }
                }
            }
            SetVarBuf(variable.Com, item);
            rpnStack.Push(variable);
        }

        private void SetSuperLazyIfPossible<LAZYT>(object item, object obj, PropertyInfo pi)
        {
            var pv = ParseValue((string)item, false);
            if (pv != null)
            {
                pi.SetValue(obj, (Func<LAZYT>)(() => (LAZYT)pv));
            }
            else
            {
                pi.SetValue(obj, (Func<LAZYT>)(() => (LAZYT)ParseValue((string)item, false)));
            }
        }

        private void ProcList(Stack<(int Level, string Com)> rpnStack, bool isNextAddRemove, bool isAdd)
        {
            if (rpnStack.Count < 3)
            {
                throw new JacException(JacException.Codes.SyntaxError, $"should chain next command : obj collection {(isAdd ? "add" : "remove")} value");
            }

            //=== EXSAMPLE ===
            //st = new Stage
            //    Procs
            //        add p1 = new Process
            //=== EXSAMPLE ===
            //st
            //    ProcLinks
            //      add p1 -> p2

            var isRemove = !isAdd;
            var attrType = isAdd ? typeof(JacListAddAttribute) : typeof(JacListRemoveAttribute);
            var itemName = rpnStack.Pop();                          // (8,"p1")                     | (8,"PushFromTo_1170D29C45C4AD4F9BCB6700EB1D4EFD") 
            var itemValue = ParseValue(itemName.Com);               // the process instance of p1   | the PushFromTo instance
            var collectionName = rpnStack.Pop();                    // (4,"Procs")                  | (4,"ProcLinks")
            var parentObjectName = rpnStack.Pop();                  // (0, "Stage ID of st")        | (0, "Stage ID of st") 
            var parentObject = ParseValue(parentObjectName.Com);    // the stage instance of st     | the stage instance of st

            // call method that have JacList???Attribute
            var methods =                                           // to get MethodInfo named "Procs" in Stage Class
                from method in parentObject.GetType().GetMethods()
                from atobj in method.GetCustomAttributes(attrType, true)
                let at = atobj as JacListAccessAttrubute
                where at.PropertyName == collectionName.Com
                select method;
            var tarMethod = methods.FirstOrDefault();
            if (tarMethod == null)
            {
                throw new JacException(JacException.Codes.NotImplementedMethod, $"There is no method implemented that have [{attrType.Name} PropertyName=\"{collectionName.Com}\"] in {parentObjectName.Com}");
            }
            tarMethod?.Invoke(parentObject, new[] { itemValue });

            // Remove cached object
            if (isRemove)
            {
                // from instanceBuf
                var dels = instanceBuf.Where(a => ReferenceEquals(a.Value, itemValue)).Select(a => a.Key).ToArray();
                foreach (var delkey in dels)
                {
                    removeRequestInstance.Add(delkey);
                }

                // from varBuf
                if (itemName.Com.StartsWith("'") && itemName.Com.EndsWith("'") || itemName.Com.StartsWith("\"") && itemName.Com.EndsWith("\""))
                {
                    itemName.Com = itemName.Com.Substring(1, itemName.Com.Length - 2);
                }
                removeRequestVarBuf.Add(itemName.Com);
            }
            if (isAdd)
            {
                if (itemValue is ValueTuple<object, object> tpl)
                {
                    foreach (var key in removeRequestVarBuf)
                    {
                        if (tpl.Item1 is IJitObjectID jid1)
                        {
                            if (key.Equals(jid1.ID))
                            {
                                removeRequestVarBuf.Remove(key);
                                break;
                            }
                        }
                        if (tpl.Item2 is IJitObjectID jid2)
                        {
                            if (key.Equals(jid2.ID))
                            {
                                removeRequestVarBuf.Remove(key);
                                break;
                            }
                        }
                    }
                }
                removeRequestInstance.Remove(itemName.Com);
                removeRequestVarBuf.Remove(itemName.Com);
            }

            rpnStack.Push(parentObjectName);
            if (isNextAddRemove)
            {
                rpnStack.Push(collectionName);
            }
        }


        private void ProcNew(Stack<(int Level, string Com)> rpnStack)
        {
            if (rpnStack.Count < 1)
            {
                throw new JacException(JacException.Codes.SyntaxError, $"Cannot new empty instance");
            }

            var typeName = rpnStack.Pop();
            var instanceKey = $"::Jac:Instance:{typeName.Com}:{++instanceIdCounter}::";
            if (JacTargets.TryGetValue(typeName.Com, out var type))
            {
                var instance = type.Assembly.CreateInstance(type.FullName);
                var pi = type.GetProperty("Name");
                if (pi != null)
                {
                    pi.SetValue(instance, instanceKey); // Set Name Property
                }
                SetInstanceBuf(instanceKey, instance);
                rpnStack.Push((typeName.Level, instanceKey));
            }
            else
            {
                throw new JacException(JacException.Codes.TypeNotFound, $"Type {typeName.Com} not found.");
            }
        }
        private void SetInstanceBuf(string instanceKey, object instance)
        {
            instanceBuf[instanceKey] = instance;
            removeRequestInstance.Remove(instanceKey);
        }

        private void SetVarBuf(string name, object instance)
        {
            varBuf[name] = instance;
            removeRequestVarBuf.Remove(name);
        }

        private static readonly Regex chkTimeSpan = new Regex(@"^-?[0-9]+(\.[0-9]*)?([eE][-+]?[0-9]+)?(MS|S|M|H|D|W)$");
        private static readonly Regex chkDistance = new Regex(@"^-?[0-9]+(\.[0-9]*)?(mm|cm|m|km)$");
        private static readonly Regex chkInteger = new Regex(@"^-?[0-9]+$");
        private static readonly Regex chkDouble = new Regex(@"^-?[0-9]+(\.[0-9]*)?([eE][-+]?[0-9]+)?%?$");
        private static readonly Regex chkDotValue = new Regex(@"^[a-z,A-Z]+[a-z,A-Z,0-9]*\.[a-z,A-Z]+[a-z,A-Z,0-9]*$");
        private static readonly Regex chkBoolean = new Regex(@"^([Tt][Rr][Uu][Ee]|[Ff][Aa][Ll][Ss][Ee])$");
        private static readonly Regex chkDateTimeFunction = new Regex(@"^datetime\s*\(\s*'[12][0-9]{3}/[0-9]{1,2}/[0-9]{1,2}\s+[0-9]{1,2}:[0-9]{1,2}:[0-9]{1,2}(\.[0-9]{1,9})?'\s*\)$");

        /// <summary>
        /// Get value managed instance name, variable, string and some object parsing.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private object ParseValue(object value, bool returnInputWhenNotExists = true)
        {
            if (value is string valuestr)
            {
                if (valuestr == "null")
                {
                    return null;
                }
                if (chkDotValue.IsMatch(valuestr))
                {
                    var dotid = valuestr.IndexOf('.');
                    var parentName = StrUtil.Left(valuestr, dotid);
                    var childName = StrUtil.Mid(valuestr, dotid + 1);
                    var parentObj = ParseValue(parentName);

                    // Check [JacGetDotValue] method first
                    var mechk =
                        from me in parentObj?.GetType().GetMethods()
                        from at in me.GetCustomAttributes<JacGetDotValueAttribute>(true)
                        select me;
                    var method = mechk.FirstOrDefault();
                    object ret = null;
                    if (method != null)
                    {
                        ret = method.Invoke(parentObj, new object[] { childName });
                        if (ret != null)
                        {
                            return ret;
                        }
                    }
                    // No [JacGetDotValue] so try to find public property of the object
                    var ppchk =
                        from me in parentObj?.GetType().GetProperties()
                        where me.Name == childName
                        select me;
                    return ppchk.FirstOrDefault()?.GetValue(parentObj);
                }
                if (chkTimeSpan.IsMatch(valuestr))
                {
                    return ParseTimeSpan(valuestr);
                }
                if (chkDistance.IsMatch(valuestr))
                {
                    return ParseDistance(valuestr);
                }
                if (chkInteger.IsMatch(valuestr))
                {
                    return int.Parse(valuestr);
                }
                if (chkDouble.IsMatch(valuestr))
                {
                    return StrUtil.ParseDouble(valuestr);
                }
                if (chkBoolean.IsMatch(valuestr))
                {
                    return DbUtil.ToBoolean(valuestr);
                }
                if (chkDateTimeFunction.IsMatch(valuestr))
                {
                    return ParseDateTime(valuestr);
                }
                if (valuestr.StartsWith("'") && valuestr.EndsWith("'") || valuestr.StartsWith("\"") && valuestr.EndsWith("\""))
                {
                    return valuestr.Substring(1, valuestr.Length - 2);
                }
                if (instanceBuf.TryGetValue(valuestr, out var ret1))
                {
                    return ret1;
                }
                if (varBuf.TryGetValue(valuestr, out var ret2))
                {
                    return ret2;
                }
            }
            if (returnInputWhenNotExists)
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        public static DateTime ParseDateTime(string valuestr)
        {
            if (valuestr.StartsWith("datetime"))
            {
                var sl = StrUtil.MidSkip(valuestr, @"^datetime\s*\(\s*'");
                valuestr = StrUtil.LeftBefore(sl, @"'\s*\)$");
            }
            else if (valuestr.StartsWith("'") && valuestr.EndsWith("'"))
            {
                valuestr = valuestr.Substring(1, valuestr.Length - 2).Trim();
            }
            try
            {
                return DateTime.Parse(valuestr);
            }
            catch
            {
                throw new JitException(JitException.IllegalFormat, $"ParseDateTime");
            }
        }

        /// <summary>
        /// Parse DateTime
        /// </summary>
        /// <param name="valuestr"></param>
        /// <param name="def">default value for parsing error</param>
        /// <returns></returns>
        public static DateTime ParseDateTime(string valuestr, DateTime def)
        {
            if (valuestr.StartsWith("datetime"))
            {
                var sl = StrUtil.MidSkip(valuestr, @"^datetime\s*\(\s*'");
                valuestr = StrUtil.LeftBefore(sl, @"'\s*\)$");
            }
            else if (valuestr.StartsWith("'") && valuestr.EndsWith("'"))
            {
                valuestr = valuestr.Substring(1, valuestr.Length - 2).Trim();
            }
            try
            {
                return DateTime.Parse(valuestr);
            }
            catch
            {
                return def;
            }
        }

        /// <summary>
        /// Parse to Distance instance
        /// </summary>
        /// <param name="valuestr"></param>
        /// <returns></returns>
        public static Distance ParseDistance(string valuestr)
        {
            if (valuestr.EndsWith("mm"))
            {
                return Distance.FromMeter(double.Parse(StrUtil.Left(valuestr, valuestr.Length - 2)) / 1000.0);
            }

            if (valuestr.EndsWith("cm"))
            {
                return Distance.FromMeter(double.Parse(StrUtil.Left(valuestr, valuestr.Length - 2)) / 100.0);
            }

            if (valuestr.EndsWith("km"))
            {
                return Distance.FromMeter(double.Parse(StrUtil.Left(valuestr, valuestr.Length - 2)) * 1000.0);
            }

            if (valuestr.EndsWith("m"))
            {
                return Distance.FromMeter(double.Parse(StrUtil.Left(valuestr, valuestr.Length - 1)));
            }

            throw new JacException(JacException.Codes.NotSupportedUnit, $"Cannot specify distance unit from {valuestr}");
        }

        /// <summary>
        /// Check Variable name rule
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool CheckVariableName(string name)
        {
            if (name == null)
            {
                return false;
            }

            if (name.Length < 1 || name.Length > 16)
            {
                return false;
            }

            var reg = new Regex("^[A-Za-z][A-Za-z0-9]*$");
            return reg.IsMatch(name);
        }

        /// <summary>
        /// Register your assembly that contains Jac Targets
        /// </summary>
        /// <param name="assempbly"></param>
        public static void RegisterJacTarget(Assembly assempbly)
        {
            var tars =
                from type in assempbly.GetTypes()
                from attr0 in type.GetCustomAttributes(typeof(JacTargetAttribute), false)
                let attr = attr0 as JacTargetAttribute
                select (attr.Name, type);
            foreach (var tar in tars)
            {
                JacTargets[tar.Name] = tar.type;
            }
        }

        /// <summary>
        /// Access to Variable object
        /// </summary>
        /// <param name="varname"></param>
        /// <returns></returns>
        public object this[string varname]
        {
            get => ParseValue(varname);
            set => SetVarBuf(varname, value);
        }

        /// <summary>
        /// Get variable as a Template
        /// </summary>
        /// <param name="varname"></param>
        /// <returns></returns>
        public JitTemplate GetTemplate(string varname)
        {
            return ParseValue(varname) as JitTemplate;
        }

        /// <summary>
        /// Get variable as a Stage
        /// </summary>
        /// <param name="varname"></param>
        /// <returns></returns>
        public JitStage GetStage(string varname)
        {
            return ParseValue(varname) as JitStage;
        }

        /// <summary>
        /// Get variable as a Process
        /// </summary>
        /// <param name="varname"></param>
        /// <returns></returns>
        public JitProcess GetProcess(string varname)
        {
            if (ParseValue(varname) is JitProcess ret)
            {
                return ret;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get variable as a Location
        /// </summary>
        /// <param name="varname"></param>
        /// <returns></returns>
        public JitLocation GetLocation(string varname)
        {
            if (ParseValue(varname) is JitLocation ret)
            {
                return ret;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get variable as a Work object
        /// </summary>
        /// <param name="varname"></param>
        /// <returns></returns>
        public JitWork GetWork(string varname)
        {
            return ParseValue(varname) as JitWork;
        }

        /// <summary>
        /// Get variable as a Variable object
        /// </summary>
        /// <param name="varname"></param>
        /// <returns></returns>
        public JitVariable GetVariable(string varname)
        {
            return ParseValue(varname) as JitVariable;
        }

        /// <summary>
        /// Get variable as a Kanban object
        /// </summary>
        /// <param name="varname"></param>
        /// <returns></returns>
        public JitKanban GetKanban(string varname)
        {
            return ParseValue(varname) as JitKanban;
        }

        /// <summary>
        /// line string cleaner like comment remove
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private string CleanLine(string s)
        {
            int i = s.IndexOf("//");
            if (i >= 0)
            {
                return s.Substring(0, i).TrimEnd(); // remove line comment
            }
            else
            {
                return s;
            }
        }

        /// <summary>
        /// Check instance name or not
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private bool IsInstance(string str)
        {
            return str.StartsWith("::Jac:Instance:");
        }

        /// <summary>
        /// Make ID such as GUID
        /// </summary>
        /// <param name="preName"></param>
        /// <returns></returns>
        public static string MakeID(string preName)
        {
            return $"{preName}{string.Join("", Guid.NewGuid().ToByteArray().Select(a => $"{a:X2}"))}";
        }
    }

    /// <summary>
    /// Jit model as a Code common exception object
    /// </summary>
    public class JacException : JitException
    {
        public enum Codes
        {
            TypeMismatch,
            DateTimeFormatError,
            SyntaxError,
            TypeNotFound,
            NotImplementedProperty,
            NotImplementedMethod,
            NotSupportedUnit,
            NotSupportLazyMethodType,
            NotSupportedType,
            ArgumentError,
        }
        public Codes Code { get; set; }

        public JacException()
        {
        }
        public JacException(Codes code)
        {
            Code = code;
        }
        public JacException(Codes code, string message) : base(message)
        {
            Code = code;
        }
    }

    /// <summary>
    /// left -> right object
    /// </summary>
    public class JacPushLinkDescription
    {
        public string Name { get; set; }
        public object From { get; set; }
        public object To { get; set; }
    }

    /// <summary>
    /// Class attribute : Mark to Jit model as a Code
    /// </summary>
    public class JacTargetAttribute : Attribute
    {
        public string Name { get; set; }
    }

    /// <summary>
    /// Method attribute : Mark to ChildValues Accessor   [JacGetDotValue] object GetChildValue(string varname)
    /// </summary>
    public class JacGetDotValueAttribute : Attribute
    {
    }

    /// <summary>
    /// Method attribute : Mark to ChildValues Accessor   [JacSetDotValue] void SetChildValue(string varname, object value)
    /// </summary>
    public class JacSetDotValueAttribute : Attribute
    {
    }

    /// <summary>
    /// Method attribute base class of list accessor
    /// </summary>
    public abstract class JacListAccessAttrubute : Attribute
    {
        public string PropertyName { get; set; }
    }

    /// <summary>
    /// Method attribute : Mark to Jit model as a Code, support list item Add 
    /// </summary>
    public class JacListAddAttribute : JacListAccessAttrubute
    {
    }

    /// <summary>
    /// Method attribute : Mark to Jit model as a Code, support list item Remove 
    /// </summary>
    public class JacListRemoveAttribute : JacListAccessAttrubute
    {
    }
}
