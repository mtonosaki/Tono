// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// Tono.GuiWinForm内で使うTokenID
    /// </summary>
    public static class TokenGeneral
    {
        /// <summary>マウスボタンを押した際に必要な処理を開始するトークン</summary>
        public static readonly NamedId TokenMouseDownNormalize = NamedId.FromName("MouseDownNormalizeJob");
        /// <summary>マウスムーブが発生した際に必要な処理を開始するトークン</summary>
        public static readonly NamedId TokenMouseMove = NamedId.FromName("MouseMoveJob");
        /// <summary>キーダウンイベントが発生した際に必要な処理を開始するトークン</summary>
        public static readonly NamedId TokenKeyDown = NamedId.FromName("KeyDownJob");
        /// <summary>
        /// 全フィーチャーロード完了時にコールされる緊急トークン
        /// </summary>
        public static readonly NamedId TokenAllFeatureLoaded = NamedId.FromName("Token.TokenAllFeatureLoaded");
    }
}
