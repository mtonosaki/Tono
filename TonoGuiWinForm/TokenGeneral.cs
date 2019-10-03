using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// Tono.GuiWinForm内で使うTokenID
    /// </summary>
    public class TokenGeneral
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
