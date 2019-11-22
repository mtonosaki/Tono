using System;
using System.Collections.Generic;
using System.Text;

namespace Tono.Jit
{
    public class Dummy
    {
        public static string[] SplitConsideringQuatationContainsSeparator(string str, char[] separations, bool isTrim, bool isIgnoreEmpty, bool isRemoveQuotation = true)
        {
            var ret = new List<string>();
            var sb = new StringBuilder();
            var isDQ = false;
            char preQ = char.MinValue;
            var sepstr = string.Join("", separations);
            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (isDQ == false && c == '\"' || c == '\'')
                {
                    preQ = c;
                    isDQ = true;
                    if (isRemoveQuotation) continue;
                }
                else
                if (isDQ && c == preQ)
                {
                    isDQ = false;
                    if (isRemoveQuotation) continue;
                }
                else
                if (isDQ == false)
                {
                    var sepid = sepstr.IndexOf(c);
                    if (sepid >= 0)
                    {
                        var ss = sb.ToString();
                        if (isTrim)
                        {
                            ss = ss.Trim();
                        }
                        if (sb.Length > 0 && (isIgnoreEmpty == false || ss.Length > 0))
                        {
                            ret.Add(ss);
                            sb.Clear();
                        }
                        var sep = sepstr[sepid].ToString();
                        if (isTrim)
                        {
                            sep = sep.Trim();
                        }
                        if (isIgnoreEmpty == false || sep.Length > 0)
                        {
                            ret.Add(sep);
                        }
                        continue;
                    }
                }
                sb.Append(c);
            }
            var sss = sb.ToString();
            if (isTrim)
            {
                sss = sss.Trim();
            }
            if (sb.Length > 0 && (isIgnoreEmpty == false || sss.Length > 0))
            {
                ret.Add(sss);
            }
            return ret.ToArray();
        }
    }
}
