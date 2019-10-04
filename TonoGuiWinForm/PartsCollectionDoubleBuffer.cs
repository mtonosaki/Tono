using System.Collections.Generic;
using System.Drawing;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// レイヤー毎に画像をBMP化して高速化を図るパーツセット
    /// </summary>
    /// <remarks>
    /// foMain.Load内の処理
    /// partsset.MotherPane = cFeatureRichMain;
    /// ps.SetTargetLayer(uLayer.Bar);
    /// 
    /// 高速化を要する処理
    /// ps.EnableShot = true;
    /// Pane.Invalidate(null);
    /// 
    /// データの最新化
    /// ps.EnableShot = false;
    /// Pane.Invalidate(null);
    /// </remarks>
    public class PartsCollectionDoubleBuffer : PartsCollection
    {
        private readonly Dictionary2d<IRichPane, int, Bitmap> bmps = new Dictionary2d<IRichPane, int, Bitmap>();
        private readonly Dictionary2d<IRichPane, int, IRichPane> panebak = new Dictionary2d<IRichPane, int, IRichPane>();
        private readonly Dictionary<int/*layerno*/, bool> _enabledLayers = new Dictionary<int, bool>();

        /// <summary>
        /// BMPにショットした内容だけで描画する
        /// </summary>
        public bool EnableShot { get; set; }

        public void SetTargetLayer(int layerno)
        {
            _enabledLayers[layerno] = true;
        }

        /// <summary>
        /// 親のcFeatureRichを指定するためのプロパティ
        /// </summary>
        public IRichPane MotherPane { get; set; }

        /// <summary>
        /// BMPキャッシュ機能を追加したレイヤー描画
        /// </summary>
        /// <param name="pane"></param>
        /// <param name="layerid"></param>
        /// <param name="pts"></param>
        protected override void drawLayer(IRichPane pane, int layerid, IEnumerable<PartsBase> pts)
        {
            if (_enabledLayers.ContainsKey(layerid))
            {
                if (bmps.ContainKeys(pane, layerid) == false)
                {
                    var pr = MotherPane.GetPaneRect();
                    bmps[pane, layerid] = new Bitmap(pr.Width, pr.Height);
                }
                var bmp = bmps[pane, layerid];
                if (EnableShot == false)
                {
                    var rp = RichPaneCustomGraphicsBinder.CreateCopy(MotherPane, Graphics.FromImage(bmp));
                    rp.SetParent(MotherPane);
                    rp.Graphics.Clear(Color.Transparent);
                    base.drawLayer(rp, layerid, pts);
                    var sr = pane.GetPaneRect();
                    MotherPane.Graphics.DrawImageUnscaledAndClipped(bmp, sr);
                    panebak[pane, layerid] = rp;
                }
                else
                {
                    var sr = pane.GetPaneRect();
                    var rpbak = panebak[pane, layerid];
                    //					pane.Graphics.SetClip(sr);
                    sr += ScreenPos.FromInt(pane.Scroll.X - rpbak.Scroll.X, pane.Scroll.Y - rpbak.Scroll.Y);
                    MotherPane.Graphics.DrawImageUnscaledAndClipped(bmp, sr);
                }
            }
            else
            {
                base.drawLayer(pane, layerid, pts);
            }
        }
    }
}
