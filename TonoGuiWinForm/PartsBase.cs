// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.Serialization;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 描画に必要な基本的な機能を提供する抽象クラス
    /// dp = Draw Parts
    /// </summary>
    [Serializable]
    public abstract class PartsBase : ICloneable, IDisposable
    {
        #region コーダ・ポジショナ関連

        private static readonly Hashtable _positionerBufK2I = new Hashtable();
        private static readonly Hashtable _positionerBufI2K = new Hashtable();
        private static readonly Hashtable _coderBufK2I = new Hashtable();
        private static readonly Hashtable _coderBufI2K = new Hashtable();

        /// <summary>位置を変換するための処理が明示できる</summary>
        public delegate LayoutRect PositionerMethod(CodeRect code, PartsBase target);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public delegate CodeRect PosCoderMethod(LayoutRect rect, PartsBase target);

        private string _positionerKeyForSave = null;
        private string _coderKeyForSave = null;

        #endregion
        #region 生成用
        [NonSerialized]
        private static int _idCounter = 0;
        #endregion
        #region		属性(シリアライズする)

        /// <summary>パーツに付与されたユニークなID</summary>
        private Id _partsID = new Id { Value = _idCounter++ };

        /** <summary>矩形領域の情報</summary> */
        private CodeRect _Pos = CodeRect.FromLTRB(0, 0, 0, 0);

        /** <summary>テキスト</summary> */
        private string _Text = "";
        private Mes.Format _textFormat = null;

        #endregion
        #region		属性(シリアライズしない)

        /// <summary>特にマスクを指定する場合のペーン</summary>
        [NonSerialized]
        private ArrayList _specifiedMaskPane = null;

        /// <summary>位置を変換するための処理が明示できる</summary>
        [NonSerialized]
        private PositionerMethod _partsPositioner = null;

        /// <summary>パーツ座標を符号化するための処理が明示できる</summary>
        [NonSerialized]
        private PosCoderMethod _partsCoder = null;

        #endregion
        #region IDisposable メンバ

        public virtual void Dispose()
        {
        }

        #endregion

        /// <summary>
        /// 文字列化
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Text.Length > 0)
            {
                return GetType().Name + " \"" + Text + "\" " + _partsID.ToString();
            }
            else
            {
                return GetType().Name + " " + _partsID.ToString();
            }
        }


        /// <summary>
        /// フォーマットの内容に従って、Textプロパティをリセットする
        /// </summary>
        public void ResetTextByFormat()
        {
            if (_textFormat != null)
            {
                Text = _textFormat.ToString();
            }
        }

        /// <summary>
        /// 言語切り替えに自動対応するテキストフォーマットを指定する
        /// </summary>
        public Mes.Format TextFormat
        {
            set
            {
                _textFormat = value;
                ResetTextByFormat();
            }
        }

        /// <summary>
        /// パーツ基本クラスのデフォルトコンストラクタ
        /// </summary>
        protected PartsBase()
        {
        }

        /// <summary>
        /// パーツのID
        /// </summary>
        public Id ID => _partsID;

        #region ICloneable メンバ

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            var ret = (PartsBase)Activator.CreateInstance(GetType());
            ret._Pos = _Pos;
            ret._Text = _Text;
            ret._specifiedMaskPane = _specifiedMaskPane;
            ret._partsPositioner = _partsPositioner;
            ret._partsCoder = _partsCoder;
            return ret;
        }

        #endregion

        #region ISerializable メンバ

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            SerializerEx.GetObjectData(typeof(PartsBase), this, info, context, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected PartsBase(SerializationInfo info, StreamingContext context)
        {
            SerializerEx.Instanciate(typeof(PartsBase), this, info, context, true);
        }

        #endregion

        /// <summary>
        /// IDをハッシュコードとする
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ID.Value;
        }

        /// <summary>
        /// IDで比較する
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is PartsBase tar)
            {
                return ID == tar.ID;
            }
            return false;
        }

        /// <summary>
        /// 指定座標が描画クリップ内かどうか識別する
        /// </summary>
        /// <param name="rp">ペーン</param>
        /// <param name="rect">調査範囲</param>
        /// <returns>true = 調査範囲が描画クリップ内 / false = そうでない</returns>
        //[DebuggerStepThrough]
        protected bool isInClip(IRichPane rp, ScreenRect rect0)
        {
            var rect = (ScreenRect)rect0.Clone();
            if (rect.Width < 1)
            {
                rect.RB.X = rect0.LT.X + 1;
            }
            var r = rect & rp.GetPaintClipRect();
            if (r == null)
            {
                return false;
            }

            r = rect & rp.GetPaneRect();
            if (r == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 指定座標が描画クリップ内かどうか識別する
        /// </summary>
        /// <param name="rp">ペーン</param>
        /// <returns>true = 調査範囲が描画クリップ内 / false = そうでない</returns>
        public bool isInClip(IRichPane rp)
        {
            var rect = GetScRect(rp, Rect);
            var r = rect & rp.GetPaneRect();
            return r != null;
        }

        /// <summary>
        /// PartsPositionerをコピーする
        /// （元のPartsPositionerのインスタンスがDisposeされているかもしれないので注意する）
        /// </summary>
        /// <param name="org"></param>
        public void SetPositionerFrom(PartsBase org)
        {
            PartsPositioner = org.PartsPositioner;
        }

        /// <summary>
        /// PartsPositionCorderをコピーする
        /// （元のPartsPositionCorderのインスタンスがDisposeされているかもしれないので注意する）
        /// </summary>
        /// <param name="org"></param>
        public void SetPositionCorderFrom(PartsBase org)
        {
            PartsPositionCorder = org.PartsPositionCorder;
        }


        /// <summary>
        /// 位置を変換する処理が登録できる
        /// </summary>
        public PositionerMethod PartsPositioner
        {
            [DebuggerStepThrough]
            set
            {
                _partsPositioner = value;
                string key;
                if (value.Target == null)
                {
                    key = "(static)." + value.Method.Name;
                }
                else
                {
                    key = value.Target.GetType().Name + "." + value.Method.Name;
                }
                _positionerBufK2I[key] = value;
                _positionerBufI2K[value] = key;
            }
            protected get => _partsPositioner;
        }

        /// <summary>
        /// ポジショナーの名前（文字列）を取得する
        /// </summary>
        public void SetPartsPositionerName(bool sw)
        {
            if (sw && _partsPositioner != null)
            {
                _positionerKeyForSave = (string)_positionerBufI2K[_partsPositioner];
            }
            else
            {
                _positionerKeyForSave = string.Empty;
            }
        }

        /// <summary>
        /// ポジショナーの名前（文字列）をインスタンスに適用する
        /// </summary>
        public void InstanciatePartsPositioner()
        {
            _partsPositioner = (PartsBase.PositionerMethod)_positionerBufK2I[_positionerKeyForSave];
            _positionerKeyForSave = string.Empty;
        }

        /// <summary>
        /// 位置を符号化する処理が登録できる
        /// </summary>
        public PosCoderMethod PartsPositionCorder
        {
            set
            {
                _partsCoder = value;
                string key;
                if (value.Target == null)   // static メソッド
                {
                    key = "(static)." + value.Method.Name;
                }
                else
                {
                    key = value.Target.GetType().Name + "." + value.Method.Name;
                }
                _coderBufK2I[key] = value;
                _coderBufI2K[value] = key;
            }
            protected get => _partsCoder;
        }

        /// <summary>
        /// コーダーの名前（文字列）を取得する
        /// </summary>
        public void SetPartsPositionCorderName(bool sw)
        {
            if (sw && _partsCoder != null)
            {
                _coderKeyForSave = (string)_coderBufI2K[_partsCoder];
            }
            else
            {
                _coderKeyForSave = string.Empty;
            }
        }
        /// <summary>
        /// ポジショナーの名前（文字列）をインスタンスに適用する
        /// </summary>
        public void InstanciatePartsPositionCorderName()
        {
            _partsCoder = (PartsBase.PosCoderMethod)_coderBufK2I[_coderKeyForSave];
            _coderKeyForSave = string.Empty;
        }

        /// <summary>
        /// マスク領域を特定する
        /// </summary>
        /// <param name="pane"></param>
        [DebuggerStepThrough]
        public void AddSpecifiedMask(IRichPane pane)
        {
            if (_specifiedMaskPane == null)
            {
                _specifiedMaskPane = new ArrayList();
            }
            _specifiedMaskPane.Add(pane);
        }

        /// <summary>
        /// 表示用のスクリーン座標を取得する
        /// </summary>
        /// <param name="rp">計算に用いるリッチペーン</param>
        /// <returns>スクリーン座標</returns>
        public virtual ScreenRect GetScRect(IRichPane rp)
        {
            return GetScRect(rp, Rect);
        }

        /// <summary>
        /// パーツ座標を取得する
        /// </summary>
        /// <returns></returns>
        public virtual LayoutRect GetPtRect()
        {
            return GetPtRect(Rect);
        }

        /// <summary>
        /// パーツ座標を取得する
        /// </summary>
        /// <returns></returns>
        public virtual LayoutRect GetPtRect(CodeRect rect)
        {
            if (_partsPositioner == null)
            {
                return LayoutRect.FromLTRB(rect.LT.X, rect.LT.Y, rect.RB.X, rect.RB.Y);
            }
            return _partsPositioner(rect, this);
        }

        /// <summary>
        /// 表示用のスクリーン座標を取得する
        /// </summary>
        /// <param name="rp">計算に用いるリッチペーン</param>
        /// <param name="rect"></param>
        /// <returns>スクリーン座標</returns>
        public virtual ScreenRect GetScRect(IRichPane rp, CodeRect rect)
        {
            if (_partsPositioner != null)
            {
                return rp.Convert(_partsPositioner(rect, this));
            }
            return rp.Convert(LayoutRect.FromLTRB(rect.LT.X, rect.LT.Y, rect.RB.X, rect.RB.Y));
        }

        /// <summary>
        /// 表示用のスクリーン座標を取得する
        /// </summary>
        /// <param name="rp">計算に用いるリッチペーン</param>
        /// <param name="code"></param>
        /// <returns>スクリーン座標</returns>
        [DebuggerStepThrough]
        public ScreenPos GetZoomed(IRichPane rp, CodePos code)
        {
            if (_partsPositioner != null)
            {
                var rect = CodeRect.FromLTWH(code.X, code.Y, 1, 1);
                return rp.GetZoomed(_partsPositioner(rect, this).LT);
            }
            return rp.GetZoomed(LayoutPos.FromInt(code.X, code.Y));
        }

        /// <summary>
        /// 指定スクリーン座標をパーツ座標（符号化）に変換する
        /// </summary>
        /// <param name="rp">ペーン</param>
        /// <param name="rect">スクリーン座標</param>
        /// <returns>パーツ座標</returns>
        [DebuggerStepThrough]
        public CodeRect GetCdRect(IRichPane rp, ScreenRect rect)
        {
            var ret = rp.Convert(rect);
            if (_partsCoder != null)
            {
                return _partsCoder(ret, this);
            }
            return CodeRect.FromLTRB(ret.LT.X, ret.LT.Y, ret.RB.X, ret.RB.Y);
        }

        /// <summary>
        /// 指定スクリーン座標をパーツ座標（符号化）に変換する
        /// </summary>
        /// <param name="rp">ペーン</param>
        /// <param name="pos">スクリーン座標</param>
        /// <returns>パーツ座標</returns>
        [DebuggerStepThrough]
        public CodePos GetCdPos(IRichPane rp, ScreenPos pos)
        {
            var ret = rp.Convert(pos);
            if (_partsCoder != null)
            {
                var pt = new LayoutRect
                {
                    LT = ret,
                    RB = ret
                };
                return _partsCoder(pt, this).LT;
            }
            return CodePos.FromInt(ret.X, ret.Y);
        }

        /// <summary>
        /// マスクの種類
        /// </summary>
        protected enum MaskType
        {
            /// <summary>
            /// 
            /// </summary>
            None,
            /// <summary>
            /// 
            /// </summary>
            Pane,
            /// <summary>
            /// 
            /// </summary>
            Parts,
            /// <summary>
            /// 
            /// </summary>
            Specification
        }

        /// <summary>
        /// ペーンでマスクする
        /// </summary>
        /// <param name="rp">計算に用いるリッチペーン</param>
        /// <returns>直前のマスク領域</returns>
        public static Region Mask(IRichPane rp)
        {
            var ret = rp.Graphics.Clip;
            rp.Graphics.Clip = new Region(rp.GetPaneRect().GetPpSize() & rp.GetPaintClipRect());
            return ret;
        }

        /// <summary>
        /// 描画領域をマスクする
        /// </summary>
        /// <param name="rp">描画を行う立地ペーン</param>
        /// <param name="type">マスクのタイプ</param>
        /// <returns>直前のマスク領域</returns>
        [DebuggerStepThrough]
        protected Region Mask(IRichPane rp, MaskType type)
        {
            Region ret;

            switch (type)
            {
                case MaskType.Pane:
                    ret = Mask(rp);
                    break;
                case MaskType.Parts:
                    ret = rp.Graphics.Clip;
                    rp.Graphics.Clip = new Region(GetScRect(rp).GetPpSize());
                    break;
                case MaskType.None:
                    ret = rp.Graphics.Clip.Clone();
                    rp.Graphics.Clip.MakeInfinite();
                    break;
                case MaskType.Specification:
                    ret = rp.Graphics.Clip;
                    if (_specifiedMaskPane != null)
                    {
                        var reg = new Region();
                        reg.MakeEmpty();
                        foreach (IRichPane mrp in _specifiedMaskPane)
                        {
                            reg.Union(mrp.GetPaneRect().GetPpSize());
                        }
                        rp.Graphics.Clip = reg;
                    }
                    break;
                default:
                    ret = rp.Graphics.Clip;
                    break;
            }
            return ret;
        }

        /// <summary>
        /// 描画
        /// </summary>
        /// <param name="rp">リッチペーン</param>
        /// <returns>false = 描画できなかった</returns>
        public abstract bool Draw(IRichPane rp);

        /// <summary>
        /// 選択状態の標準実装（各Drawでコールするか、独自に選択状態を実装すること）
        /// </summary>
        /// <param name="rp">リッチペーン</param>
        protected virtual void drawSelected(IRichPane rp)
        {
            if (this is IPartsSelectable)
            {
                if (((IPartsSelectable)this).IsSelected)
                {
                    rp.Graphics.DrawRectangle(Pens.Red, GetScRect(rp));
                }
            }
        }

        /// <summary>
        /// IsOnの戻り値
        /// </summary>
        [Flags]
        public enum PointType
        {
            /// 領域外
            Outside = 0,
            /// 領域内
            Inside = 0x10,
            /// 境界上（左端）
            OnLeft = 0x01,
            /// 境界上（上端）
            OnTop = 0x04,
            /// 境界上（右端）
            OnRight = 0x02,
            /// 境界上（下端）
            OnBottom = 0x08
        }

        /// <summary>
        /// 指定スクリーン座標が矩形領域内か調査する
        /// </summary>
        /// <param name="sp">スクリーン座標での位置</param>
        /// <param name="rp">リッチペーン</param>
        /// <returns>結果</returns>
        public virtual PointType IsOn(ScreenPos sp, IRichPane rp)
        {
            return IsOn(sp, rp, PointType.OnBottom | PointType.OnLeft | PointType.OnRight | PointType.OnTop | PointType.Outside | PointType.Inside);
        }

        /// <summary>
        /// 指定スクリーン座標が矩形領域内か調査する
        /// </summary>
        /// <param name="sp">スクリーン座標（マウスの座標）</param>
        /// <param name="rp">リッチペーン</param>
        /// <param name="check">チェック対象の列挙（フラグ）</param>
        public virtual PointType IsOn(ScreenPos sp, IRichPane rp, PointType check)
        {
            var r = GetScRect(rp);
            if (r.Width < 1 || r.Height < 1)
            {
                return PointType.Outside;
            }
            var w = 3;  // 境界線を判断するための幅

            if (r.IsIn(sp))
            {
                if (r.Width > r.Height)
                {
                    if ((check & PointType.OnRight) != 0 && Math.Abs(r.RB.X - sp.X) <= w)
                    {
                        return PointType.OnRight;
                    }

                    if ((check & PointType.OnLeft) != 0 && Math.Abs(r.LT.X - sp.X) <= w)
                    {
                        return PointType.OnLeft;
                    }

                    if ((check & PointType.OnBottom) != 0 && Math.Abs(r.RB.Y - sp.Y) <= w)
                    {
                        return PointType.OnBottom;
                    }

                    if ((check & PointType.OnTop) != 0 && Math.Abs(r.LT.Y - sp.Y) <= w)
                    {
                        return PointType.OnTop;
                    }
                }
                else
                {
                    if ((check & PointType.OnBottom) != 0 && Math.Abs(r.RB.Y - sp.Y) <= w)
                    {
                        return PointType.OnBottom;
                    }

                    if ((check & PointType.OnTop) != 0 && Math.Abs(r.LT.Y - sp.Y) <= w)
                    {
                        return PointType.OnTop;
                    }

                    if ((check & PointType.OnRight) != 0 && Math.Abs(r.RB.X - sp.X) <= w)
                    {
                        return PointType.OnRight;
                    }

                    if ((check & PointType.OnLeft) != 0 && Math.Abs(r.LT.X - sp.X) <= w)
                    {
                        return PointType.OnLeft;
                    }
                }
                return PointType.Inside;
            }
            return PointType.Outside;
        }

        /// <summary>
        /// 符号座標を与えて、パーツ内側かどうかを判定する
        /// （IsOn(uScPos・・・と違い、高速に処理ができる）
        /// </summary>
        /// <param name="cp">符号座標</param>
        /// <returns>Inside = 内側 / Outside = 外側</returns>
        public virtual PointType IsOn(CodePos cp)
        {
            return Rect.IsIn(cp) ? PointType.Inside : PointType.Outside;
        }

        /// <summary>
        /// 矩形領域の取得/設定
        /// </summary>
        public virtual CodeRect Rect
        {
            [DebuggerStepThrough]
            get => _Pos;
            //[DebuggerStepThrough]
            set => _Pos = value;
        }

        /// <summary>
        /// テキストの取得/設定
        /// </summary>
        public virtual string Text
        {
            [DebuggerStepThrough]
            get => _Text;
            [DebuggerStepThrough]
            set => _Text = value;
        }
    }
}
