// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Drawing;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FreeDrawLayer の概要の説明です。
    /// </summary>
    public class FreeDrawLayer
    {
        private readonly int _layerLevel;
        private readonly IRichPane _parent;
        private Bitmap _bmp = null;
        private Graphics _gr = null;

        /// <summary>
        /// 初期化コンストラクタ
        /// </summary>
        /// <param name="layerLevel">レイヤーレベル</param>
        public FreeDrawLayer(IRichPane parent, int layerLevel)
        {
            _layerLevel = layerLevel;
            _parent = parent;
        }

        /// <summary>
        /// 画面準備ができたかどうかを識別する
        /// </summary>
        public bool IsReady => _parent.Graphics != null;

        /// <summary>
        /// 描画用のハンドル
        /// </summary>
        public Graphics Graphics
        {
            get
            {
                if (_gr == null)
                {
                    System.Diagnostics.Debug.Assert(_parent.Graphics != null, "Graphicsオブジェクトを参照するタイミングは、cFeatureRichが描画された後です");
                    var ts = new ThreadUtil();
                    var pH = ts.GetHandleControl(_parent.Control);
                    var g = Graphics.FromHwnd(pH);

                    _bmp = new Bitmap(_parent.GetPaneRect().Width, _parent.GetPaneRect().Height, g);
                    _gr = Graphics.FromImage(_bmp);
                }
                return _gr;
            }
        }

        /// <summary>
        /// このフリーレイヤーに一度でも描画したかどうか調べる
        /// </summary>
        public bool IsUsing => _bmp != null;

        /// <summary>
        /// （内部処理用実行しないでください）描画イメージインスタンス
        /// </summary>
        public Image Image => _bmp;

        /// <summary>
        /// レベル
        /// </summary>
        public int Level => _layerLevel;

        /// <summary>
        /// ハッシュコード＝レイヤーレベル
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _layerLevel;
        }

    }
}
