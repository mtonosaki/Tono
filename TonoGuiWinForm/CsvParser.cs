// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 文字列からCSVフォーマットでDataSetのTableを作成する
    /// </summary>
    public static class CsvParser
    {
        private static int nullColNameNo = 1;

        /// <summary>
        /// ファイル名を指定してCSVをロードする
        /// </summary>
        /// <param name="csvPath"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static DataTable Load(string csvPath, Encoding encoding)
        {
            var reader = new StreamReader(csvPath, encoding);
            return Load(reader);
        }

        /// <summary>
        /// CSVデータをロードする
        /// </summary>
        /// <param name="reader">ロードするストリーム</param>
        /// <returns></returns>
        public static DataTable Load(StreamReader reader)
        {
            var tab = new DataTable();

            var columnNames = new List<string>();
            var dat = reader.ReadToEnd();
            var isHeader = true;
            foreach (var line in dat.Split('\n'))
            {
                var cline = line.Replace("\r", "");
                var cells = cline.Split(',');
                if (isHeader)
                {
                    for (var i = 0; i < cells.Length; i++)
                    {
                        var col = cells[i];
                        col = col.Trim();
                        if (string.IsNullOrEmpty(col))
                        {
                            col = string.Format("[dummy{0}]", nullColNameNo++);
                        }
                        columnNames.Add(col);
                        tab.Columns.Add(new DataColumn(col));
                    }
                }
                else
                {
                    var cellN = 0;
                    var row = tab.NewRow();
                    for (var i = 0; i < columnNames.Count; i++)
                    {
                        if (i >= cells.Length)
                        {
                            row[columnNames[i]] = "";
                        }
                        else
                        {
                            var col = cells[i];
                            col = col.Trim();
                            row[columnNames[i]] = col;
                            if (string.IsNullOrEmpty(col))
                            {
                                cellN++;
                            }
                        }
                    }
                    if (cellN > 0)
                    {
                        tab.Rows.Add(row);
                    }
                }
                isHeader = false;
            }
            return tab;
        }

    }
}
