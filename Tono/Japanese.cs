using System.Collections.Generic;
using System.Text;

namespace Tono
{
    /// <summary>
    /// Japanese language utility
    /// </summary>
    public static class Japanese
    {
        private static readonly Dictionary<char, char> c2c1 = new Dictionary<char, char>();             // for Passs1
        private static readonly Dictionary<string, char> hkana2c1 = new Dictionary<string, char>();     // for Pass1(byte kana)

        static Japanese()
        {
            var SF = "あいうえおぁぃぅぇぉヴかきくけこがぎぐげごさしすせそざじずぜぞたちつてとだぢヂづヅでどっなにぬねのはひふへほばびぶべぼぱぴぷぺぽまみむめもやゆよゃゅょらりるれろわをヲんａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９！”＃＄％＆’（）＝－＾～￥｜／｛｝［］＜＞，．＿";
            var ST = "アイウエオァィゥェォブカキクケコガギグゲゴサシスセソザジズゼゾタチツテトダジジズズデドッナニヌネノハヒフヘホバビブベボパピプペポマミムメモヤユヨャュョラリルレロワオオンabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz0123456789!\"#$%&'()=-^~\\|/{}[]<>,._";

            // Pass1
            for (var i = 0; i < SF.Length; i++)
            {
                var cs = SF[i];
                var cr = ST[i];
                if (cs == cr)
                {
                    cr = '_';
                }
                c2c1[cs] = cr;
            }
            c2c1['ー'] = '-';
            c2c1['－'] = '-';
            c2c1['～'] = '-';
            c2c1['→'] = '-';

            // Hankaku kana
            SF = "ｱｲｳｴｵｧｨｩｪｫｶｷｸｹｺｻｼｽｾｿﾀﾁﾂｯﾃﾄﾅﾆﾇﾈﾉﾊﾋﾌﾍﾎﾏﾐﾑﾒﾓﾔﾕﾖｬｭｮﾗﾘﾙﾚﾛﾜｦﾝ";
            ST = "アイウエオァィゥェォカキクケコサシスセソタチツッテトナニヌネノハヒフヘホマミムメモヤユヨャュョラリルレロワヲン";
            for (var i = 0; i < SF.Length; i++)
            {
                var cs = SF.Substring(i, 1);
                var cr = ST[i];
                hkana2c1[cs] = cr;
            }
            SF = "ｶﾞｷﾞｸﾞｹﾞｺﾞｻﾞｼﾞｽﾞｾﾞｿﾞﾀﾞﾁﾞﾂﾞﾃﾞﾄﾞﾊﾞﾋﾞﾌﾞﾍﾞﾎﾞﾊﾟﾋﾟﾌﾟﾍﾟﾎﾟ";
            ST = "ガギグゲゴザジズゼゾダジズデドバビブベボパピプペポ";
            for (var i = 0; i < SF.Length; i += 2)
            {
                var cs = SF.Substring(i, 2);
                var cr = ST[i / 2];
                hkana2c1[cs] = cr;
            }
        }

        /// <summary>
        /// Get あかさたな character for index.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Getあかさたな(string s)
        {
            var org = "アイウエオァィゥェォヴカキクケコガギグゲゴサシスセソザジズゼゾタチツテトダヂヅデドッナニヌネノハヒフヘホバビブベボパピプペポマミムメモヤユヨャュョラリルレロワヲン0123456789０１２３４５６７８９";
            var ret = "あああああああああああかかかかかかかかかかささささささささささたたたたたたたたたたたなななななはははははははははははははははまままままややややややらららららわわわ11111111111111111111";
            var sb = new StringBuilder();
            foreach (var c0 in s)
            {
                var c = c0.ToString();
                var s1 = GetKeyOne(c, true);
                s1 = s1.ToUpper();
                var i = org.IndexOf(s1);
                if (i >= 0)
                {
                    sb.Append(ret.Substring(i, 1));
                }
                else
                {
                    sb.Append(s1);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Make fuzzy search string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <remarks>
        /// c/d → cd で検索できるような / を削除する処理は入れない
        /// </remarks>
        public static string GetKeyJpKeepver(string s)
        {
            s = GetKeyOne(s, true);
            s = wordSpecialSynonim(s);
            return s;
        }

        /// <summary>
        /// make string for fazzy search
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string GetKeyJp(string s)
        {
            s = GetKeyOne(s, false);
            s = wordSpecialSynonim(s);
            return s;
        }

        /// <summary>
        /// normalize some synonim character
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string wordSpecialSynonim(string s)
        {
            s = s.Replace("稼", "可");
            s = s.Replace("働", "動");
            s = s.Replace("標", "表");
            s = s.Replace("可視", "みえる");
            s = s.Replace("見える", "みえる");
            s = s.Replace("視界", "みえる");
            s = s.Replace("インスタンス", "実体");
            s = s.Replace("オブジェクト", "実体");
            s = s.Replace("クラス", "型");
            s = s.Replace("改善", "カイゼン");
            s = s.Replace("平準化", "ヘイジュンカ");
            s = s.Replace("無駄", "ムダ");
            s = s.Replace("表準", "標準");
            s = s.Replace("表順", "標準");
            return s;
        }

        private static char GetChar(string s, ref int i)
        {
            char cr;
            // 半角（濁点）
            if (i < s.Length - 1)
            {
                var ss = s.Substring(i, 2);
                if (hkana2c1.TryGetValue(ss, out cr))
                {
                    i++;
                    return cr;
                }
            }
            // 半角
            var sss = s.Substring(i, 1);
            if (hkana2c1.TryGetValue(sss, out cr))
            {
                return cr;
            }

            // その他
            var c = s[i];
            if (c2c1.TryGetValue(c, out cr))
            {
                return cr;
            }
            else
            {
                return c;
            }
        }

        /// <summary>
        /// Fuzzy single character
        /// </summary>
        /// <param name="s"></param>
        /// <param name="isKeepChar">true=Not delete character</param>
        /// <returns></returns>
        private static string GetKeyOne(string s, bool isKeepChar)
        {
            s = s.Trim();
            s = s.ToLower();

            var ss = "";
            for (int i = 0; i < s.Length; i++)
            {
                ss += GetChar(s, ref i);
            }
            ss = ss.Replace("クァ", "カ");
            ss = ss.Replace("クィ", "キ");
            ss = ss.Replace("クゥ", "ク");
            ss = ss.Replace("クェ", "ケ");
            ss = ss.Replace("クォ", "コ");
            ss = ss.Replace("クワ", "クア");
            ss = ss.Replace("ディ", "デ");
            ss = ss.Replace("デュ", "ド");
            ss = ss.Replace("ヴァ", "バ");
            ss = ss.Replace("ファ", "ハ");
            ss = ss.Replace("フィ", "ヒ");
            ss = ss.Replace("フェ", "ヘ");
            ss = ss.Replace("フォ", "ホ");
            ss = ss.Replace("ヒュ", "フ");
            ss = ss.Replace("ファ", "ハ");
            ss = ss.Replace("フィ", "ヒ");
            ss = ss.Replace("フゥ", "フ");
            ss = ss.Replace("フュ", "フ");
            ss = ss.Replace("フェ", "ヘ");
            ss = ss.Replace("フォ", "ホ");
            ss = ss.Replace("ニュ", "ヌ");
            ss = ss.Replace("㎥", "m3");
            ss = ss.Replace("㎡", "m2");
            ss = ss.Replace("&", "and");
            ss = ss.Replace("v.s.", "vs");
            ss = ss.Replace("#", "no");
            ss = ss.Replace("no.", "no");
            ss = ss.Replace("　", " ");
            if (isKeepChar == false)
            {
                ss = ss.Replace(" ", "");
                ss = ss.Replace("_", "");
                ss = ss.Replace("-", "");
                ss = ss.Replace(".", "");   // A.K.A→AKAで検索
                ss = ss.Replace("/", "");   // c/d→cdで検索
                ss = ss.Replace("・", "");
                ss = ss.Replace("ッ", "");
            }

            // a contracted sound.（拗音）
            var os = "ァィゥェォッャュョ";
            var ns = "アイウエオッヤユヨ";
            for (var i = 0; i < os.Length; i++)
            {
                ss = ss.Replace(os[i], ns[i]);
            }

            // a long sound.（長音）
            var r = "アアカアガアサアザアタアダアナアハアバアパアマアヤアラアワアイイキイギイシイジイチイニイヒイビイピイミイリイウウクウグウスウズウツウヌウフウブウプウムウユウルウエエケエゲエセエゼエテエデエネエヘエベエペエメエレエオオコオゴオソオゾオトオドオノオホオボオポオモオヨオロオ";
            for (var i = 0; i < r.Length; i += 2)
            {
                var w2 = r.Substring(i, 2);
                var s1 = w2.Substring(0, 1);
                int l;
                do
                {
                    l = ss.Length;
                    ss = ss.Replace(w2, s1);
                } while (ss.Length != l);
            }
            return ss;
        }
    }
}
