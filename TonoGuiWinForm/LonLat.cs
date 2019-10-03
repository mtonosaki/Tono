namespace Tono.GuiWinForm
{
    /// <summary>
    /// 東経北緯を表すシンプルなクラス１
    /// </summary>
    public class LonLat : ILonLat
    {
        /// <summary>
        /// 東経：単位＝度
        /// </summary>
        public double Lon { get; set; }

        /// <summary>
        /// 北緯：単位＝度
        /// </summary>
        public double Lat { get; set; }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public LonLat()
        {
        }

        /// <summary>
        /// 初期化コンストラクタ
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        public LonLat(double lon, double lat)
        {
            Lon = lon;
            Lat = lat;
        }

        /// <summary>
        /// 初期化コンストラクタ
        /// </summary>
        /// <param name="ll"></param>
        public LonLat(ILonLat ll)
        {
            Lon = ll.Lon;
            Lat = ll.Lat;
        }
    }
}
