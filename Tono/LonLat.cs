using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Tono
{
    /// <summary>
    /// Datum type
    /// </summary>
    public enum Datum
    {
        WGS84,      // World Geodetic System 1984. 世界測地系。米国が構築・維持し、GPSによるナビゲ－ションの位置表示の基準。Google Map、Mapion
        JGD2000,    // Japan Geodetic Datum 2000. 日本が採用する世界測地系。GRS80楕円体、ITRF94座標系。電子国土背景地図、DocomoのGPSサービス
        JGD2011,    // 東日本大震災による地殻変動に伴う変更。東北地方・関東地方・新潟県・富山県・石川県・福井県・山梨県・長野県・岐阜県はITRF2008座標系
        TKY,        // Tokyo Datum. 日本測地系. 東京都港区麻布台の旧国立天文台跡地. ベッセル楕円体。Yahoo!地図、マップファン、goo地図、livedoor地図、ゼンリン
        SK42,       // Пулково-1942. ロシア測地系。クラソフスキー楕円体
        PZ90,       // Параметры Земли 1990. ロシア。GLONASSで用いられている全地球的座標系. PZ-90楕円体
        GSK2011,    // еодезическая система координат ГСК-2011. ロシア。ITRF系に準拠。GSK-2011楕円体
    }

    /// <summary>
    /// Longitude(WGS84)
    /// </summary>
    public struct Longitude
    {
        public static readonly Datum ClassDatum = Datum.WGS84;

        public Angle Lon { get; set; }

        [IgnoreDataMember]
        public Datum Datum => Tono.Longitude.ClassDatum;

        /// <summary>
        /// Make WGS84 longitude from tokyo datum
        /// </summary>
        /// <param name="tkylon"></param>
        /// <param name="tkylat"></param>
        /// <returns></returns>
        /// <seealso cref="http://tancro.e-central.tv/grandmaster/excel/tky2wgs.html#section"/>
        /// <remarks>
        /// 再配布可能情報として：師匠が作成したPerlやJavaSciptには著作権を主張するほどの内容はありませんので、訪問者の方が気に入ったものであれば自由にお持ち帰りいただいても結構です。
        /// 再配布可能情報：/excel フォルダに掲載されている情報
        /// </remarks>
        public static Longitude From(LongitudeTky tkylon, LatitudeTky tkylat)
        {
            return new Longitude
            {
                Lon = Angle.FromDeg(tkylon.Lon.Deg - tkylat.Lat.Deg * 0.000046038f - tkylon.Lon.Deg * 0.000083043f + 0.01004f)
            };
        }

        /// <summary>
        /// make WGS84 instance
        /// </summary>
        /// <param name="deg"></param>
        /// <returns></returns>
        public static Longitude FromDeg(double deg)
        {
            return new Longitude
            {
                Lon = Angle.FromDeg(deg)
            };
        }

        /// <summary>
        /// Make instance from angle string
        /// </summary>
        /// <param name="deg"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static Longitude FromDeg(string deg, double def = 0.0)
        {
            return FromDeg(double.TryParse(deg, out double val) ? val : def);
        }

        /// <summary>
        /// make instance from second value
        /// </summary>
        /// <param name="sec"></param>
        /// <returns></returns>
        public static Longitude FromSecond(double sec)
        {
            return FromDeg(sec / 3600);
        }

        /// <summary>
        /// make instance from radian
        /// </summary>
        /// <param name="rad"></param>
        /// <returns></returns>
        public static Longitude FromRad(double rad)
        {
            return new Longitude
            {
                Lon = Angle.FromRad(rad)
            };
        }

        /// <summary>
        /// make instance string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{ClassDatum} Lon={Lon.Deg}";
        }

        public override bool Equals(object obj)
        {
            if (obj is Longitude tar)
            {
                return Math.Abs(tar.Lon.Deg - Lon.Deg) < 1.0 / 3600 / 40 / 10;  // 10cm以内は同じ位置とする
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return Lon.GetHashCode();
        }


        
        public static Longitude operator +(Longitude v1, Angle v2)
        {
            return new Longitude { Lon = v1.Lon + v2 };
        }

        
        public static Longitude operator -(Longitude v1, Angle v2)
        {
            return new Longitude { Lon = v1.Lon - v2 };
        }

        
        public static Longitude operator +(Longitude v1, Longitude v2)
        {
            return new Longitude { Lon = v1.Lon + v2.Lon };
        }

        
        public static Longitude operator -(Longitude v1, Longitude v2)
        {
            return new Longitude { Lon = v1.Lon - v2.Lon };
        }

        
        public static Longitude operator *(Longitude v1, double v2)
        {
            return new Longitude { Lon = v1.Lon * v2 };
        }

        
        public static Longitude operator /(Longitude v1, double v2)
        {
            return new Longitude { Lon = v1.Lon / v2 };
        }

        
        public static bool operator <(Longitude v1, Longitude v2)
        {
            return v1.Lon.Rad < v2.Lon.Rad;
        }

        
        public static bool operator <=(Longitude v1, Longitude v2)
        {
            return v1.Lon.Rad <= v2.Lon.Rad;
        }

        
        public static bool operator >(Longitude v1, Longitude v2)
        {
            return v1.Lon.Rad > v2.Lon.Rad;
        }

        
        public static bool operator >=(Longitude v1, Longitude v2)
        {
            return v1.Lon.Rad >= v2.Lon.Rad;
        }
    }

    /// <summary>
    /// Latitude(WGS84)
    /// </summary>
    public struct Latitude
    {
        public static readonly Datum ClassDatum = Datum.WGS84;

        public Angle Lat { get; set; }

        [IgnoreDataMember]
        public Datum Datum => Longitude.ClassDatum;

        /// <summary>
        /// Make WGS84 latitude from tokyo datum
        /// </summary>
        /// <param name="tkylon"></param>
        /// <param name="tkylat"></param>
        /// <returns></returns>
        /// <seealso cref="http://tancro.e-central.tv/grandmaster/excel/tky2wgs.html#section"/>
        /// <remarks>
        /// 再配布可能情報として：師匠が作成したPerlやJavaSciptには著作権を主張するほどの内容はありませんので、訪問者の方が気に入ったものであれば自由にお持ち帰りいただいても結構です。
        /// 再配布可能情報：/excel フォルダに掲載されている情報
        /// </remarks>
        /// <seealso cref="http://tancro.e-central.tv/grandmaster/style/notes.html"/>
        public static Latitude From(LongitudeTky tkylon, LatitudeTky tkylat)
        {
            return new Latitude
            {
                Lat = Angle.FromDeg(tkylat.Lat.Deg - tkylat.Lat.Deg * 0.00010695f + tkylon.Lon.Deg * 0.000017464f + 0.0046017f)
            };
        }
        public static Latitude FromDeg(double deg)
        {
            return new Latitude
            {
                Lat = Angle.FromDeg(deg)
            };
        }

        /// <summary>
        /// make instance from string
        /// </summary>
        /// <param name="deg"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static Latitude FromDeg(string deg, double def = 0.0)
        {
            return FromDeg(double.TryParse(deg, out double val) ? val : def);
        }

        /// <summary>
        /// make instance from second value
        /// </summary>
        /// <param name="sec"></param>
        /// <returns></returns>
        public static Latitude FromSecond(double sec)
        {
            return FromDeg(sec / 3600);
        }

        /// <summary>
        /// make instance from radian
        /// </summary>
        /// <param name="rad"></param>
        /// <returns></returns>
        public static Latitude FromRad(double rad)
        {
            return new Latitude
            {
                Lat = Angle.FromRad(rad)
            };
        }


        /// <summary>
        /// make string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Datum} Lat={Lat.Deg}";
        }

        public override bool Equals(object obj)
        {
            if (obj is Latitude tar)
            {
                return Math.Abs(tar.Lat.Deg - Lat.Deg) < 1.0 / 3600 / 40 / 10;  // 10cm以内は同じ位置とする
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return Lat.GetHashCode();
        }


        
        public static Latitude operator +(Latitude v1, Angle v2)
        {
            return new Latitude { Lat = v1.Lat + v2 };
        }

        
        public static Latitude operator -(Latitude v1, Angle v2)
        {
            return new Latitude { Lat = v1.Lat - v2 };
        }

        
        public static Latitude operator +(Latitude v1, Latitude v2)
        {
            return new Latitude { Lat = v1.Lat + v2.Lat };
        }

        
        public static Latitude operator -(Latitude v1, Latitude v2)
        {
            return new Latitude { Lat = v1.Lat - v2.Lat };
        }

        
        public static Latitude operator *(Latitude v1, double v2)
        {
            return new Latitude { Lat = v1.Lat * v2 };
        }

        
        public static Latitude operator /(Latitude v1, double v2)
        {
            return new Latitude { Lat = v1.Lat / v2 };
        }

        
        public static bool operator <(Latitude v1, Latitude v2)
        {
            return v1.Lat.Rad < v2.Lat.Rad;
        }

        
        public static bool operator <=(Latitude v1, Latitude v2)
        {
            return v1.Lat.Rad <= v2.Lat.Rad;
        }

        
        public static bool operator >(Latitude v1, Latitude v2)
        {
            return v1.Lat.Rad > v2.Lat.Rad;
        }

        
        public static bool operator >=(Latitude v1, Latitude v2)
        {
            return v1.Lat.Rad >= v2.Lat.Rad;
        }
    }

    /// <summary>
    /// Longitude of Tokyo datum
    /// </summary>
    public struct LongitudeTky
    {
        public static readonly Datum Datum = Datum.TKY;

        public Angle Lon { get; set; }
        public override string ToString()
        {
            return $"{Datum} Lon={Lon.Deg}";
        }
        
        public static LongitudeTky operator +(LongitudeTky v1, Angle v2)
        {
            return new LongitudeTky { Lon = v1.Lon + v2 };
        }

        /// <summary>
        /// Make tokyo dadum from WGS84
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        /// <returns></returns>
        public static LongitudeTky From(Longitude lon, Latitude lat)
        {
            return new LongitudeTky
            {
                Lon = Angle.FromDeg(lon.Lon.Deg + 0.000046047 * lat.Lat.Deg + 0.000083049 * lon.Lon.Deg - 0.010041046),
            };
        }
    }

    /// <summary>
    /// Latitude of Tokyo datum
    /// </summary>
    public struct LatitudeTky
    {
        public static readonly Datum Datum = Datum.TKY;

        public Angle Lat { get; set; }

        /// <summary>
        /// make latitude of tokyo datum from WGS84
        /// </summary>
        /// <param name="lon"></param>
        /// <param name="lat"></param>
        /// <returns></returns>
        public static LatitudeTky From(Longitude lon, Latitude lat)
        {
            return new LatitudeTky
            {
                Lat = Angle.FromDeg(lat.Lat.Deg + 0.000106961 * lat.Lat.Deg - 0.000017467 * lon.Lon.Deg - 0.004602017),
            };
        }


        public override string ToString()
        {
            return $"{Datum} Lat={Lat.Deg}";
        }

        
        public static LatitudeTky operator +(LatitudeTky v1, Angle v2)
        {
            return new LatitudeTky { Lat = v1.Lat + v2 };
        }
    }

}
