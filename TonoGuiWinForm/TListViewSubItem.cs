#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// リストビューのサブアイテムの拡張クラス
    /// </summary>
    public class TListViewSubItem : System.Windows.Forms.ListViewItem.ListViewSubItem
    {
        #region	属性（シリアライズする）
        /// <summary>テキストのデータ</summary>
        private object _obj = null;
        /// <summary>数値型のデータを所持しているか</summary>
        private bool _isInt = false;
        /// <summary>リンク先のテーブル</summary>
        private string _tableName = "";
        /// <summary>リンク先のカラム</summary>
        private string _columnName = "";
        #endregion
        #region	属性（シリアライズしない）
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TListViewSubItem()
        {
            var s = "";
            Text = s.Clone();
        }

        /// <summary>
        /// 初期化コンストラクタ
        /// </summary>
        /// <param name="text">表示するテキスト</param>
        public TListViewSubItem(object value)
        {
            Text = value;
        }

        /// <summary>
        /// int型の値を取得
        /// </summary>
        public int intText
        {
            get
            {
                if (_obj is Id)
                {
                    return ((Id)_obj).Value;
                }

                if (_obj is int)
                {
                    return (int)_obj;
                }

                return 0;   // Id型でもint型でも無かった場合
            }
        }

        /// <summary>
        /// 数値型をデータを所持しているか調査する
        /// </summary>
        public bool IsInt
        {
            get => _isInt;
            set => _isInt = value;
        }

        /// <summary>
        /// リンク先のテーブル名を取得/設定
        /// </summary>
        public string TableName
        {
            get => _tableName;
            set => _tableName = value;
        }

        /// <summary>
        /// リンク先の列名を取得/設定
        /// </summary>
        public string ColumnName
        {
            get => _columnName;
            set => _columnName = value;
        }

        /// <summary>
        /// 表示テキストの取得/設定
        /// </summary>
        public new object Text
        {
            get => _obj;
            set
            {
                if (value is Id)
                {
                    base.Text = ((Id)value).Value.ToString();
                    IsInt = true;
                }
                else if (value is DateTimeEx)
                {
                    base.Text = ((DateTimeEx)value).TotalSeconds.ToString();
                }
                else
                {
                    if (value is int)
                    {
                        IsInt = true;
                    }

                    base.Text = value.ToString();
                }
                _obj = value;
            }
        }
    }
}
