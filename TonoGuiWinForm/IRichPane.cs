// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ペーンへのアクセスを提供するインターフェース
    /// </summary>
    public interface IRichPane
    {
        /// <summary>
        /// IDテキスト
        /// </summary>
        string IdText { get; set; }

        /// <summary>
        /// IRichPaneの実体のコントロール型
        /// </summary>
        System.Windows.Forms.Control Control
        {
            get;
        }

        /// <summary>
        /// 親ペーンを返す
        /// </summary>
        /// <returns>null = 親はいない</returns>
        IRichPane GetParent();

        /// <summary>
        /// 名前でペーンを検索する（遅いので注意）
        /// </summary>
        /// <param name="name">検索するペーンのNameプロパティ</param>
        /// <returns>見つかったペーン / null = 見つからなかった</returns>
        IRichPane GetPane(string name);

        /// <summary>
        /// ペーンの領域を返すインターフェース
        /// </summary>
        /// <returns>領域</returns>
        ScreenRect GetPaneRect();

        /// <summary>
        /// 描画が必要な領域を返すインターフェース
        /// </summary>
        /// <returns>領域</returns>
        ScreenRect GetPaintClipRect();

        /// <summary>
        /// 画面を再描画する
        /// </summary>
        /// <param name="rect">再描画スクリーン絶対位置（ペーン相対座標でない） / null=全領域</param>
        void Invalidate(ScreenRect rect);

        /// <summary>
        /// ズーム倍率を返すインターフェース
        ///　×10[%]の値が格納されている
        /// </summary>
        XyBase Zoom
        {
            get;
            set;
        }

        /// <summary>
        /// スクロール量を返すインターフェース
        /// プラス方向は、画面の右下
        /// </summary>
        ScreenPos Scroll
        {
            get;
            set;
        }

        /// <summary>
        /// グラフィックオブジェクト
        /// </summary>
        System.Drawing.Graphics Graphics
        {
            get;
        }

        /// <summary>
        /// パーツ座標からスクリーン（マウス）座標に変換する
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        ScreenPos Convert(LayoutPos value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        ScreenRect Convert(LayoutRect value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        ScreenPos GetZoomed(LayoutPos value);

        /// <summary>
        /// スクリーン（マウス）座標からパーツ座標に変換する
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        LayoutPos Convert(ScreenPos value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        LayoutRect Convert(ScreenRect value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        LayoutPos GetZoomed(ScreenPos value);
    }
}
