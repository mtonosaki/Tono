using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace Tono.Excel
{
    /// <summary>
    /// Load excel and make DataSet object
    /// </summary>
    public class ExcelParser
    {
        /// <summary>
        /// DateTime default value of Excel
        /// </summary>
        public static readonly DateTime DateDefault = new DateTime(1899, 12, 30);

        /// <summary>
        /// check the all horizontal cells with chkstrs
        /// </summary>
        /// <param name="cells"></param>
        /// <returns></returns>
        public static bool CheckCellHorzStrings(IXLCell cell, StringComparison comparison, params string[] chkstrs)
        {
            var row = cell.WorksheetRow();
            var colno = cell.WorksheetColumn().ColumnNumber();
            for (int i = 0; i < chkstrs.Length; i++)
            {
                var celstr = DbUtil.ToString(Cell(row, colno + i), "");
                if (chkstrs[i].Equals(celstr, comparison) == false)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// get cell string
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public static string ToString(IXLCell cell)
        {
            return Cell(cell)?.ToString();
        }

        /// <summary>
        /// get cell object
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public static object Cell(IXLCell cell)
        {
            if (cell.CachedValue is string cs && string.IsNullOrWhiteSpace(cs) == false && double.TryParse(cs, out double val))
            {
                return val;
            }
            else
            {
                return cell.CachedValue;
            }
        }

        /// <summary>
        /// get cell object
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="rowno"></param>
        /// <param name="colno"></param>
        /// <returns></returns>
        public static object Cell(IXLWorksheet sheet, int rowno, int colno)
        {
            var cell = sheet.Cell(rowno, colno);
            return Cell(cell);
        }


        /// <summary>
        /// get cell object
        /// </summary>
        /// <param name="row"></param>
        /// <param name="colno"></param>
        /// <returns></returns>
        public static object Cell(IXLRow row, int colno)
        {
            var cell = row.Cell(colno);
            return Cell(cell);
        }

        /// <summary>
        /// load excel book file to make DataSet object
        /// </summary>
        /// <param name="excelName">Excel file name</param>
        /// <param name="stream">data stream</param>
        /// <returns></returns>
        public static DataSet Load(string excelName, Stream stream)
        {
            var ds = new DataSet(excelName);
            using (var wbook = new XLWorkbook(stream))
            {
                foreach (var sheet in wbook.Worksheets)
                {
                    if (sheet.Name.StartsWith("@"))  // Ignore sheets named start with "@"
                    {
                        continue;
                    }
                    var dt = new DataTable(sheet.Name);
                    var isHeader = true;
                    var colnos = new List<(int ColNo, string ColName)>();
                    int nNulLRow = 0;
                    foreach (var row in sheet.RowsUsed())
                    {
                        try
                        {
                            // collect column name for column keys
                            if (isHeader)
                            {
                                var isFirstCell = true;
                                foreach (var col in sheet.ColumnsUsed())
                                {
                                    var val = Cell(row, col.ColumnNumber());
                                    var columnName = val?.ToString();
                                    if (string.IsNullOrEmpty(columnName) == false && string.IsNullOrWhiteSpace(columnName) == false)
                                    {
                                        object val2 = Cell(sheet, row.RowNumber() + 1, col.ColumnNumber());
                                        dt.Columns.Add(columnName, "".Equals(val2) || val2 == null ? typeof(string) : val2.GetType());
                                        colnos.Add((col.ColumnNumber(), columnName));
                                    }
                                    else
                                    {
                                        if (isFirstCell)
                                        {
                                            break;
                                        }
                                    }
                                    isFirstCell = false;
                                    isHeader = false;
                                }
                                continue;
                            }

                            // collect row data
                            var dr = dt.NewRow();
                            var nCellInRow = 0;
                            for (var j = 0; j < colnos.Count; j++)
                            {
                                var cnn = colnos[j];
                                object val = Cell(row, cnn.ColNo);
                                if (val != null)
                                {
                                    try
                                    {
                                        if (val is string s && string.IsNullOrWhiteSpace(s))
                                        {
                                            dr[j] = DBNull.Value;
                                        }
                                        else
                                        {
                                            if (val is double v && dr.Table.Columns[j].DataType == typeof(DateTime))
                                            {
                                                dr[j] = ExcelUtil.MakeDateTime(v);
                                            }
                                            else
                                            {
                                                dr[j] = val;
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        Debug.WriteLine($"Cell value '{val}' may not be expected type {dt.Columns[j].DataType.Name} in Row:{row.RowNumber()}, Col:{dt.Columns[j].ColumnName}, {excelName}!{sheet.Name} (this error is a reason of slow task)");
                                        if (dt.Columns[j].DataType.IsValueType)
                                        {
                                            if (string.IsNullOrWhiteSpace(val.ToString()))
                                            {
                                                dr[j] = DBNull.Value;
                                            }
                                            else
                                            {
                                                dr[j] = 0;
                                            }
                                        }
                                        else
                                        {
                                            dr[j] = DBNull.Value;
                                        }
                                    }
                                    nCellInRow++;
                                }
                                else
                                {
                                    dr[j] = DBNull.Value;
                                }
                            }
                            if (nCellInRow > 0)
                            {
                                dt.Rows.Add(dr);
                                nNulLRow = 0;
                            }
                            else
                            {
                                nNulLRow++;
                                if (nNulLRow > 20)  // end if found a lot of empty row
                                {
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                    ds.Tables.Add(dt);
                }
            }
            return ds;
        }
    }
}
