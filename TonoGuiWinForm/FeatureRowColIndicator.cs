using System.Drawing;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeatureRowColIndicator の概要の説明です。
    /// </summary>
    public class FeatureRowColIndicator : FeatureBase, IMouseListener
    {
        #region 属性（シリアライズする）
        private string _tarPaneIdText;
        private Control _vl;
        private Control _hl;
        #endregion

        #region 属性（シリアライズしない）
        private IRichPane _tarPane = null;
        #endregion

        /// <summary>
        /// 対象となるペーンの名称をセットする
        /// </summary>
        public string TargetPaneIdText
        {
            get => _tarPaneIdText;
            set => _tarPaneIdText = value;
        }

        /// <summary>
        /// マウス移動時のイベント
        /// </summary>
        public void OnMouseMove(MouseState e)
        {
            // 対象となりペーンを取得する
            if (_tarPane == null)
            {
                _tarPane = Pane.GetPane(_tarPaneIdText);

                Control c;
                if (_tarPane is TPane)
                {
                    c = ((TPane)_tarPane).Parent;
                }
                else
                {
                    c = (Control)_tarPane;
                }
                var pr = _tarPane.GetPaneRect();
                c.Controls.Add(_vl = new Control
                {
                    ForeColor = Color.Green,
                    BackColor = System.Drawing.Color.Black,
                    Name = "GIdeVertLabel",
                    Size = new Size(8, 1),
                    //初期の座標設定
                    Location = new Point(pr.LT.X, pr.LT.Y)
                });
                c.Controls.Add(_hl = new Control
                {
                    ForeColor = Color.Green,
                    BackColor = System.Drawing.Color.Black,
                    Name = "GIdeHorzLabel",
                    //初期の座標設定
                    Location = new Point(pr.LT.X, pr.LT.Y),
                    Size = new Size(1, 8)
                });

            }
            // マウス座標に応じて、場所を示すアイテムを移動する
            //e.Pos：マウス座標
            //_vl.Top：Y座標(縦の動き)　_hl：X座標(横の動き)
            _vl.Top = e.Pos.Y;
            _hl.Left = e.Pos.X;
        }

        #region IMouseListener メンバ
        public void OnMouseDown(MouseState e)
        {
            // 未使用
        }

        public void OnMouseUp(MouseState e)
        {
            // 未使用
        }

        public void OnMouseWheel(MouseState e)
        {
            // 未使用
        }


        #endregion
    }
}
