// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// キーイベントの属性を表現するクラス
    /// </summary>
    public class KeyState
    {
        #region 属性（シリアライズが必要）
        private Keys _key;
        private bool _isControl;
        private bool _isShift;
        #endregion

        #region Data tips for debugging
#if DEBUG
        public string _ => ToString();
#endif
        #endregion

        public override string ToString()
        {
            return (_isControl ? "[CTRL]+" : "") + (_isShift ? "[SHIFT]+" : "") + _key.ToString();
        }


        /// <summary>
        /// キーの状態
        /// </summary>
        public Keys Key => _key;

        /// <summary>
        /// シフトキーの状態
        /// </summary>
        public bool IsShift => _isShift;

        /// <summary>
        /// コントロールキーの状態
        /// </summary>
        public bool IsControl => _isControl;

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>

        public KeyState()
        {
            _key = Keys.None;
        }

        /// <summary>
        /// 初期化コンストラクタ
        /// </summary>

        public KeyState(Keys value)
        {
            _key = value;
            _isControl = false;
            _isShift = false;
        }

        /// <summary>
        /// キーイベント属性からインスタンスを生成する
        /// </summary>
        /// <returns>新しいインスタンス</returns>
        // 
        public static KeyState FromKeyEventArgs(KeyEventArgs e)
        {
            var ret = new KeyState
            {
                _key = e.KeyCode,
                _isControl = e.Control,
                _isShift = e.Shift
            };
            return ret;
        }

        /// <summary>
        /// uMouseStateの一部を切り取りインスタンスを生成する
        /// </summary>
        /// <param name="value">マウス状態</param>
        /// <returns>新しいインスタンス</returns>

        public static KeyState FromMouseStateButtons(MouseState.Buttons value)
        {
            var ret = new KeyState
            {
                _isControl = value.IsCtrl,
                _isShift = value.IsShift
            };
            ret._key = (ret._isControl ? Keys.Control : 0) | (ret._isShift ? Keys.Shift : 0);
            return ret;
        }

        /// <summary>
        /// インスタンスの内容が等しいかどうか調べる
        /// </summary>
        /// <param name="obj">比較対照</param>
        /// <returns>true = インスタンスの内容が等しい</returns>

        public override bool Equals(object obj)
        {
            if (obj is KeyState)
            {
                return GetHashCode() == obj.GetHashCode();
            }
            return false;
        }

        /// <summary>
        /// インスタンスの内容を数値化する
        /// </summary>
        /// <returns>ハッシュコード</returns>

        public override int GetHashCode()
        {
            var ret = (int)Key;
            ret |= IsControl ? (int)Keys.Control : 0;
            ret |= IsShift ? (int)Keys.Shift : 0;
            return ret;
        }
    }
}
