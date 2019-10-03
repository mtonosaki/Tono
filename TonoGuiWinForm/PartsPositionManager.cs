using System;
using System.Collections;
using System.Collections.Specialized;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// パーツの位置の移動を管理する
    /// Key = dpBase
    /// Value = Pos3
    /// </summary>
    public class PartsPositionManager : HybridDictionary
    {
        /// <summary>
        /// パーツ座標変更タイプ
        /// </summary>
        [Flags]
        public enum DevelopType
        {
            Unknown = 0,                // 未設定
            Move = 1,                   // 移動
            SizeFlag = 0x80,            // サイズ変更フラグ
            SizeLeft = SizeFlag | 1,        // 左端のサイズ変更
            SizeRight = SizeFlag | 2,       // 右端のサイズ変更
            SizeTop = SizeFlag | 4,         // 上端のサイズ変更
            SizeBottom = SizeFlag | 8,      // 下端のサイズ変更
        }

        #region	属性(シリアライズする)
        #endregion
        #region	属性(シリアライズしない)
        /// <summary>選択状態</summary>
        private DevelopType dev = DevelopType.Move;

        /// <summary>
        /// 動作タイプを指定する
        /// </summary>
        /// <param name="value"></param>
        public void SetDevelop(DevelopType value)
        {
            dev = value;
            _lastDevelpType = dev;
        }
        #endregion

        /// <summary>
        /// パーツ座標の管理
        /// </summary>
        public class Pos3
        {
            /// <summary>該当ペーン</summary>
            public IRichPane OrgPane;
            /// <summary>該当ペーン</summary>
            public IRichPane NowPane;
            /// <summary>移動ドラッグ前の位置（符号化座標の場合、符号の値）</summary>
            public CodeRect Org;
            /// <summary>直前の位置（符号化座標の場合、符号の値）</summary>
            public CodeRect Pre;
            /// <summary>現在の位置（符号化座標の場合、符号の値）</summary>
            public CodeRect Now;
            /// <summary>途中からパーツが追加された場合に追加された時の移動分</summary>
            public CodePos Offset = new CodePos();

            public override string ToString()
            {
                var s = "";
                if (Org != null)
                {
                    s += "O{" + Org.ToString() + "} ";
                }

                if (Pre != null)
                {
                    s += "P{" + Pre.ToString() + "} ";
                }

                if (Now != null)
                {
                    s += "N{" + Now.ToString() + "} ";
                }

                if (NowPane != null)
                {
                    s += "Pane = " + NowPane.IdText;
                }
                return s;
            }

            /// <summary>
            /// 元の位置かどうか調べる
            /// </summary>
            public bool IsStanding
            {
                get
                {
                    if (Org.Equals(Now))
                    {
                        return true;
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// インスタンスを初期化する
        /// </summary>
        /// <param name="with">基準位置とパーツの種類の元情報</param>
        /// <param name="pane">基準位置のスクロールとズーム値を記憶するためのインプット</param>
        public void Initialize(PartsCollectionBase with)
        {
            Clear();

            _lastDevelpType = DevelopType.Unknown;

            foreach (PartsCollectionBase.PartsEntry pe in with)
            {
                var p = new Pos3
                {
                    OrgPane = RichPaneBinder.CreateCopyComplete(pe.Pane),
                    NowPane = pe.Pane,
                    Org = (CodeRect)pe.Parts.Rect.Clone(), // 符号化の座標（符号しない場合、単なるパーツ座標）
                    Pre = (CodeRect)pe.Parts.Rect.Clone(), // 符号化の座標（符号しない場合、単なるパーツ座標）
                    Now = (CodeRect)pe.Parts.Rect.Clone(), // 符号化の座標（符号しない場合、単なるパーツ座標）
                    Offset = CodePos.FromInt(0, 0)
                };
                base[pe.Parts] = p;
            }
        }

        /// <summary>
        /// パーツを指定してインスタンスを初期化する
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="pane"></param>
        public void AddParts(PartsBase parts, IRichPane pane, CodePos offset)
        {
            _lastDevelpType = DevelopType.Unknown;

            var p = new Pos3
            {
                OrgPane = RichPaneBinder.CreateCopyComplete(pane),
                NowPane = pane,
                Org = (CodeRect)parts.Rect.Clone(), // 符号化の座標（符号しない場合、単なるパーツ座標）
                                                   //p.Org = (uCdRect)parts.Rect.Clone() - parts.GetCdPos(pane, _prevShift); // 符号化の座標（符号しない場合、単なるパーツ座標）
                Pre = (CodeRect)parts.Rect.Clone(), // 符号化の座標（符号しない場合、単なるパーツ座標）
                Now = (CodeRect)parts.Rect.Clone(), // 符号化の座標（符号しない場合、単なるパーツ座標）
                Offset = (CodePos)offset.Clone()
            };
            base[parts] = p;
        }

        /// <summary>
        /// パーツを指定して POSを得る
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Pos3 this[PartsBase key] => (Pos3)base[key];

        /// <summary>
        /// パーツの座標を実際に書き換える
        /// </summary>
        /// <param name="partsCollection">パーツ領域の描画更新をする場合、Partsインスタンスを指定する</param>
        public void SetNowPositionsToParts(PartsCollectionBase partsCollection)
        {
            foreach (DictionaryEntry de in this)
            {
                var parts = (PartsBase)de.Key;
                var pos = (PartsPositionManager.Pos3)de.Value;

                if (partsCollection != null)
                {
                    partsCollection.Invalidate(parts, pos.NowPane); // 移動前のInvalidate
                }
                parts.Rect = pos.Now;                           // 位置を調整する
                if (partsCollection != null)
                {
                    partsCollection.Invalidate(parts, pos.NowPane); // 移動後のInvalidate
                }
            }
        }


        /// <summary>
        /// 最後にDevelop実行した時の位置変更方法
        /// </summary>
        public DevelopType LastDevelop => _lastDevelpType;
        private DevelopType _lastDevelpType = DevelopType.Unknown;
        private ScreenPos _prevShift = ScreenPos.FromInt(0, 0);

        /// <summary>
        /// 移動情報を更新する
        /// 注意：いったんスクリーン座標に変換して移動するので、コード座標に変換誤差が生じる可能性がある。
        /// </summary>
        /// <param name="dragStartPos">移動開始時のマウス座標（ドラッグ開始点）</param>
        /// <param name="currentPos">現在のマウス座標</param>
        /// <param name="type">座標変更タイプ</param>
        public void Develop(ScreenPos dragStartPos, ScreenPos currentPos, DevelopType type)
        {
            _lastDevelpType = type;
            var sdelta = currentPos - dragStartPos;     // マウスの移動量
            _prevShift = sdelta;

            foreach (DictionaryEntry de in this)            // 選択中全パーツに対して行う
            {
                var p3 = (Pos3)de.Value;
                var target = (PartsBase)de.Key;
                p3.Pre = p3.Now;                            // ひとつ前の座標に記憶
                p3.Now = (CodeRect)p3.Now.Clone();           // サイズ系の処理が正しく動作するために必要
                var virtualPos = target.GetCdRect(p3.NowPane, target.GetScRect(p3.OrgPane, p3.Org) + sdelta); // 移動後の位置を計算
                var d = target.GetCdPos(p3.NowPane, sdelta);
                //Debug.WriteLine( string.Format( "[{0}] Delta[{1}]:Offset[{2}]", i, d, p3.Offset) );

                virtualPos = CodeRect.FromLTWH(virtualPos.LT.X + p3.Offset.X, virtualPos.LT.Y, virtualPos.Width, virtualPos.Height);

                switch (type)
                {
                    case DevelopType.Move:
                        p3.Now = virtualPos;
                        if (p3.Now.Width != p3.Org.Width)
                        {
                            p3.Now.RB.X = p3.Now.LT.X + p3.Org.Width - 1;   // 幅が変わらないようにする
                        }
                        break;
                    case DevelopType.SizeRight:
                        p3.Now.RB.X = virtualPos.RB.X;
                        break;
                    case DevelopType.SizeLeft:
                        p3.Now.LT.X = virtualPos.LT.X;
                        break;
                    case DevelopType.SizeTop:
                        p3.Now.LT.Y = virtualPos.LT.Y;
                        break;
                    case DevelopType.SizeBottom:
                        p3.Now.RB.Y = virtualPos.RB.Y;
                        break;
                }
            }
        }

        /// <summary>
        /// 指定したキーの移動情報を削除する
        /// </summary>
        /// <param name="key">削除する位置情報のパーツ</param>
        public void Remove(PartsBase key)
        {
            base.Remove(key);
        }

        /// <summary>
        /// 全編集パーツが動いていないことを確認
        /// </summary>
        public bool IsStanding
        {
            get
            {
                foreach (Pos3 pos in Values)
                {
                    if (pos.IsStanding == false)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
