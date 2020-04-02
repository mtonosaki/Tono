// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Drawing;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 直線の描画オブジェクト
    /// </summary>
    public class PartsLine : PartsBase
    {
        #region		属性(シリアライズする)
        // ペンオブジェクト
        private Pen _pen = new Pen(Color.FromArgb(255, 96, 128));
        #endregion
        #region		属性(シリアライズしない)
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PartsLine()
        {
        }

        /// <summary>
        /// 描画
        /// </summary>
        /// <param name="rp"></param>
        public override bool Draw(IRichPane rp)
        {
            var pos = GetScRect(rp);        // 位置を取得する
                                            // InClipをすると右下がりの線しか描画できない為、InClipはやらない
                                            //if( isInClip(rp, pos) == false )	// 描画不要であれば、なにもしない
                                            //{
                                            //	return false;
                                            //}
                                            //Mask(rp, eMask.Specification);	// 特定マスクのみで表示
            rp.Graphics.DrawLine(_pen, pos.LT, pos.RB);
            return true;
        }

        /// <summary>
        /// ペンオブジェクト（SETする場合、元のPenをDisposeすること）
        /// </summary>
        public Pen Style
        {
            get => _pen;
            set => _pen = value;
        }

        /// <summary>
        /// 線の太さの取得/設定
        /// </summary>
        public float LineWidth
        {
            get => _pen.Width;
            set => _pen.Width = value;
        }

        /// <summary>
        /// 線の色の取得/設定
        /// </summary>
        public virtual Color LineColor
        {
            get => _pen.Color;
            set => _pen.Color = value;
        }

        /// <summary>
        /// ペンの線のスタイルを取得/設定
        /// </summary>
        public System.Drawing.Drawing2D.DashStyle DashStyle
        {
            get => _pen.DashStyle;
            set => _pen.DashStyle = value;
        }

        /// <summary>
        /// ペンに終端のスタイルの取得/設定
        /// </summary>
        public System.Drawing.Drawing2D.LineCap EndCap
        {
            get => _pen.EndCap;
            set => _pen.EndCap = value;
        }

        /// <summary>
        /// ペンの始端のスタイルの取得/設定
        /// </summary>
        public System.Drawing.Drawing2D.LineCap StartCap
        {
            get => _pen.StartCap;
            set => _pen.StartCap = value;
        }
    }
}
