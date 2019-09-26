using System;

namespace Tono.Excel
{
    /// <summary>
    /// エクセル操作ユーティリティ
    /// </summary>
    public class ExcelUtil
    {
        private static readonly DateTime OrgDT = new DateTime(1900, 1, 1);


        /// <summary>
        /// エクセルの日付を値からDateTimeに変換する
        /// </summary>
        /// <param name="excelDateTimeValue"></param>
        /// <returns></returns>
        public static DateTime MakeDateTime(double excelDateTimeValue)
        {
            var ret = OrgDT + TimeSpan.FromDays(excelDateTimeValue);
            return ret - TimeSpan.FromDays(2);
        }

        /// <summary>
        /// 文字列が空かどうかを評価する
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsStrEmpty(object s)
        {
            if (s == null)
            {
                return true;
            }
            if (s.ToString().Trim() == "")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// AAC245 → AAC　列だけ切り出す
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string GetColString(string s)
        {
            var ret = "";
            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (char.IsNumber(c))
                {
                    break;
                }
                ret += c;
            }
            return ret;
        }

        /// <summary>
        /// 行番号を切り出す   AA123 → 123
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int GetRowNo(string s)
        {
            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (char.IsNumber(c))
                {
                    return int.Parse(s.Substring(i));
                }
            }
            return 0;
        }

        /// <summary>
        /// A → 1,  AA → 27 列番号を取得する
        /// </summary>
        /// <param name="s"></param>
        /// <returns>列番号</returns>
        /// <remarks>列文字列は3桁まで</remarks>
        public static int GetColNo(string s)
        {
            double val = 0;
            s = s.ToUpper();
            double keta = 0;
            for (var i = s.Length - 1; i >= 0; i--)
            {
                char c = s[i];
                double d = c - 'A' + 1;
                val += Math.Pow(26, keta) * d;
                keta++;
            }
            return (int)val;
        }

        /// <summary>
        /// 列番号から列文字列を作成する  1 → A,  27 → AA
        /// </summary>
        /// <param name="colno"></param>
        /// <returns></returns>
        /// <remarks>制限：3桁まで</remarks>
        public static string MakeColStr(int colno)
        {
            colno--;    // 1始まりを0始まりにして計算しやすくする
            var c = colno / 26;
            var a1 = (colno - 26) / 676;
            var a2 = (c - 1) % 26 + 1;
            var a3 = colno - c * 26 + 1;

            var ret = "";
            if (a1 > 0)
            {
                ret += (char)(a1 + 64);
            }
            if (colno > 25)
            {
                ret += (char)(a2 + 64);
            }
            ret += (char)(a3 + 64);

            return ret;
        }
    }
}
