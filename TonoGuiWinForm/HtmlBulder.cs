// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// HTML�̃��|�[�g�𐶐�����
    /// </summary>
    public class HtmlBulder
    {
        /// <summary>
        /// �e�[�u���r���_
        /// </summary>
        public class Table
        {
            private readonly Dictionary<int/*col*/, Dictionary<int/*row*/, string>> _dat = new Dictionary<int, Dictionary<int, string>>();
            private readonly Dictionary2d<int, int, HorizontalAlignment> _halign = new Dictionary2d<int, int, HorizontalAlignment>();

            /// <summary>
            /// �e�L�X�g�̉������̈ʒu���Z�b�g
            /// </summary>
            /// <param name="col"></param>
            /// <param name="row"></param>
            /// <param name="hal"></param>
            public void SetHolizontalAlignment(int col, int row, HorizontalAlignment hal)
            {
                _halign[col, row] = hal;
            }

            /// <summary>
            /// �Z���Ƀf�[�^����������
            /// </summary>
            /// <param name="col"></param>
            /// <param name="row"></param>
            /// <param name="content"></param>
            public void Add(int col, int row, string content)
            {
                if (_dat.TryGetValue(col, out var rows) == false)
                {
                    _dat[col] = rows = new Dictionary<int, string>();
                }
                rows[row] = content;
            }

            /// <summary>
            /// �Z���f�[�^
            /// </summary>
            /// <param name="col"></param>
            /// <param name="row"></param>
            /// <returns></returns>
            public string this[int col, int row]
            {
                get
                {
                    if (_dat.TryGetValue(col, out var rows))
                    {
                        if (rows.TryGetValue(row, out var cell))
                        {
                            return cell;
                        }
                    }
                    return "";
                }
                set => Add(col, row, value);
            }

            private string _width = "";
            /// <summary>
            /// �� 20px / 100% �Ȃ�
            /// </summary>
            public string Width
            {
                set => _width = value;
            }

            /// <summary>
            /// �������HTML�R�[�h��\��
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                var colmax = -1;
                var rowmax = -1;
                foreach (var kv in _dat)
                {
                    if (kv.Key > colmax)
                    {
                        colmax = kv.Key;
                    }
                    foreach (var row in kv.Value.Keys)
                    {
                        if (row > rowmax)
                        {
                            rowmax = row;
                        }
                    }
                }
                var sb = new StringBuilder();
                sb.Append("\r\n<table");
                if (string.IsNullOrEmpty(_width) == false)
                {
                    sb.AppendFormat(" width=\"{0}\"", _width);
                }
                sb.Append(">");
                for (var row = 0; row <= rowmax; row++)
                {
                    sb.Append("\r\n<tr>");
                    for (var col = 0; col <= colmax; col++)
                    {
                        if (_halign.ContainKeys(col, row))
                        {
                            sb.AppendFormat("\r\n<td align=\"{0}\">", _halign[col, row].ToString());
                        }
                        else
                        {
                            sb.Append("\r\n<td>");
                        }
                        sb.Append(this[col, row]);
                        sb.Append("</td>");
                    }
                    sb.Append("\r\n</tr>");
                }
                sb.Append("\r\n</table>");
                return sb.ToString();
            }
        }

        public abstract class TemplateBase
        {
            /// <summary>
            /// ���ɂȂ�R���e���c
            /// </summary>
            protected string _contents = null;

            public string Contents
            {
                set => _contents = value;
            }
        }

        /// <summary>
        /// �e���v���[�g
        /// </summary>
        public class Template : TemplateBase
        {
            /// <summary>
            /// �x�[�X�ɂȂ�e���v���[�g
            /// </summary>
            private readonly string _basePath = "HtmlBulder.base.htm";
            /// <summary>
            /// �^�C�g��
            /// </summary>
            private string _title = "A HTML Report";

            /// <summary>
            /// �^�C�g��
            /// </summary>
            public string Title
            {
                get => _title;
                set => _title = value;
            }

            /// <summary>
            /// ������
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                var path = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), _basePath);
                var sb = new StringBuilder(File.ReadAllText(path, Encoding.ASCII));
                sb.Replace("@TITLE@", _title);
                sb.Replace("@CONTENTS", _contents.ToString());
                return sb.ToString();
            }
        }

        /// <summary>
        /// �R���e���c
        /// </summary>
        private readonly StringBuilder _contents = new StringBuilder();

        /// <summary>
        /// �g�p����e���v���[�g
        /// </summary>
        private readonly TemplateBase _template = null;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public HtmlBulder()
        {
        }

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public HtmlBulder(TemplateBase temp)
        {
            _template = temp;
        }

        /// <summary>
        /// �R���e���c��ǉ�����
        /// </summary>
        /// <param name="content"></param>
        public void Add(string content)
        {
            _contents.Append(StrUtil.S(content));
        }

        /// <summary>
        /// �i���Œǉ�
        /// </summary>
        /// <param name="content"></param>
        public void AddParagraph(string content)
        {
            _contents.Append("<p>");
            _contents.Append(StrUtil.S(content));
            _contents.Append("</p>");
        }

        /// <summary>
        /// �ۑ�����
        /// </summary>
        /// <param name="sw"></param>
        public void Save(StreamWriter sw)
        {
            if (_template != null)
            {
                _template.Contents = _contents.ToString();
                sw.Write(_template.ToString());
            }
            else
            {
                sw.Write(_contents.ToString());
            }
        }

        /// <summary>
        /// �e�L�X�g��Ԃ�
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _contents.ToString();
        }

        /// <summary>
        /// ������
        /// </summary>
        public void Show()
        {
            var fname = Path.GetTempFileName() + ".htm";
            using (var sw = new StreamWriter(fname))
            {
                Save(sw);
            }
            Process.Start(fname);
        }
    }
}
