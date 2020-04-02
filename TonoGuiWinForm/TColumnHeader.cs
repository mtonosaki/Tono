// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Windows.Forms;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// カラムヘッダーの拡張クラス
    /// </summary>
    public class TColumnHeader : System.Windows.Forms.ColumnHeader
    {
        #region	属性(シリアライズする)
        /// <summary>自カラムに格納可能なデータの型</summary>
        private Type _datType = typeof(object);
        #endregion
        #region	属性(シリアライズしない)
        /// <summary>並び替えの形式フラグ</summary>
        public int SortType = 0;
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TColumnHeader() : base()
        {
        }

        /// <summary>
        /// 初期化コンストラクタ
        /// </summary>
        /// <param name="str">ヘッダーテキスト</param>
        /// <param name="width">カラムの幅</param>
        /// <param name="textAlign">テキストの表示位置</param>
        /// <param name="type">格納可能データの型</param>
        public TColumnHeader(string str, int width, HorizontalAlignment textAlign, Type type)
        {
            base.Text = str;
            base.Width = width;
            base.TextAlign = textAlign;
            _datType = type;
        }

        /// <summary>
        /// 格納データのタイプの取得/設定
        /// </summary>
        public Type DataType
        {
            get => _datType;
            set => _datType = value;
        }
    }
}
