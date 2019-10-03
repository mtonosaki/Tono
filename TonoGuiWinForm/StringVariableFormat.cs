#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 文字列で、変数と定数の両方が表現できるクラス
    /// 変数は、ToString時に評価される
    /// </summary>
    public class StringVariableFormat
    {
        /// <summary>
        /// 変数の場合の文字列生成
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public delegate string VariableText(string key);

        private readonly string _key;
        private readonly VariableText _val;

        /// <summary>
        /// 定数で初期化
        /// </summary>
        /// <param name="fix"></param>
        public StringVariableFormat(string fix)
        {
            _key = fix;
            _val = null;
        }

        /// <summary>
        /// 変数で初期化
        /// </summary>
        /// <param name="key">変数を生成するためのキー</param>
        /// <param name="val">変数文字列生成用の関数</param>
        public StringVariableFormat(string key, VariableText val)
        {
            _key = key;
            _val = val;
        }

        /// <summary>
        /// 定数か変数で文字列を返す
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_val == null)
            {
                return _key;
            }
            else
            {
                return _val(_key);
            }
        }
    }
}
