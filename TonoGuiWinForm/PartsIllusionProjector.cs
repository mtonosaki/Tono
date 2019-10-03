using System;
using System.Collections;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// dfPartsIllusionBase の概要の説明です。
    /// このクラスは、特定のパーツが複数箇所にあるように見せかける幻影を作り出すクラス
    /// 原理は、特定のIRichPaneの内容をそのまま別のIRichPaneに描画することで実現
    /// このクラスの使い方の例は、FeatureDposeRichPaneのOnInitialInstanceを参照
    /// </summary>
    /// <remarks>
    /// 特許公開番号：2008-171229
    /// </remarks>
    public class PartsIllusionProjector : IDisposable
    {
        #region デリゲート型
        public delegate void SetIllusionStateMethod(PartsIllusionProjector projector, PartsBase target);
        public delegate bool IsIllusionProjectMethod(IRichPane originalPane, PartsBase targetParts);
        #endregion
        #region ペーンEnumerator

        /// <summary>
        /// イリュージョンと元ペーンを含めて列挙を制御する
        /// </summary>
        public class PaneEnumerator : IEnumerable, IEnumerator
        {
            private readonly IList/*<dfPartsIllusionProjector>*/ _dat;
            private readonly IRichPane _parent;
            private object _current = null;
            private int _pointer;
            private readonly PartsBase _parts;
#if DEBUG
            public string _
            {
                get
                {
                    if (_current == null)
                    {
                        return "(null)";
                    }
                    return "Current = " + ((IRichPane)_current).IdText;
                }
            }
#endif
            /// <summary>
            /// Enumeratorを構築する
            /// </summary>
            /// <param name="parent">targetが0件の時に適用されるペーン</param>
            /// <param name="target">dfPartsIllusionProjector型のリスト</param>
            /// <param name="parts">参照中のパーツ</param>
            internal PaneEnumerator(IRichPane parent, IList target, PartsBase parts)
            {
                _parent = parent;
                _dat = target;
                if (_dat != null)
                {
                    if (_dat.Count > 0)
                    {
                        _parent = ((PartsIllusionProjector)_dat[0]).OriginalPane;
                    }
                }
                _parts = parts;
                Reset();
            }
            #region IEnumerable メンバ

            public IEnumerator GetEnumerator()
            {
                return this;
            }

            #endregion

            #region IEnumerator メンバ

            public void Reset()
            {
                _pointer = -1;
            }

            /// <summary>
            /// IRichPane型
            /// </summary>
            public object Current => _current;

            public bool MoveNext()
            {
                if (_pointer == -1)
                {
                    _current = _parent;
                    _pointer++;
                    return true;
                }
                if (_dat == null)
                {
                    return false;
                }
                if (_dat.Count > _pointer)
                {
                    var proj = (PartsIllusionProjector)_dat[_pointer];
                    _pointer++;
                    if (proj.isNeedProject(_parts))
                    {
                        proj.ChangeState(_parts);
                        _current = proj.ScreenPane;
                    }
                    else
                    {
                        return MoveNext();
                    }
                    return true;
                }
                return false;
            }

            #endregion
        }

        #endregion

        #region 属性（シリアライズする）

        private IRichPane _screen;
        private IRichPane _original;
        private bool _isEnable = true;

        #endregion
        #region 属性（シリアライズしない）

        /// <summary>イリュージョン中の状態変更関数を指定する</summary>
        private SetIllusionStateMethod _stateFunction = null;

        /// <summary>イリュージョンするかどうかの判定を処理する関数を指定する</summary>
        private IsIllusionProjectMethod _isIllutionFunction = null;

        #endregion

        /// <summary>
        /// 有効／無効 getProjectorの時に取捨される条件になる
        /// </summary>
        public bool Enabled
        {
            get => _isEnable;
            set => _isEnable = value;
        }

        /// <summary>
        /// ペーンを列挙するEnumeratorを取得する
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="target"></param>
        /// <param name="parts"></param>
        /// <returns></returns>
        public static PaneEnumerator GetEnumerator(IRichPane parent, IList target, PartsBase parts)
        {
            return new PaneEnumerator(parent, target, parts);
        }

        /// <summary>
        /// イリュージョンするかどうかの判定を処理する関数を指定する
        /// </summary>
        public IsIllusionProjectMethod IsIllusionProject
        {
            set => _isIllutionFunction = value;
        }

        /// <summary>
        /// イリュージョンすべきかどうかを判断する
        /// </summary>
        /// <param name="parts">調査パーツ</param>
        /// <returns>true = イリュージョンするべき / false = 不要</returns>
        internal bool isNeedProject(PartsBase parts)
        {
            if (_isIllutionFunction != null)
            {
                return _isIllutionFunction(OriginalPane, parts);
            }
            return true;
        }

        /// <summary>
        /// イリュージョン中の状態変更関数を指定する
        /// </summary>
        public SetIllusionStateMethod SetIllusionState
        {
            set => _stateFunction = value;
        }

        /// <summary>
        /// 最新のイリュージョン状態に更新する
        /// </summary>
        public void ChangeState(PartsBase target)
        {
            if (_stateFunction != null)
            {
                _stateFunction(this, target);
            }
        }

        /// <summary>
        /// 唯一のコンストラクタ
        /// </summary>
        /// <param name="target">基データが収まるペーン</param>
        /// <param name="idText">イリュージョン管理用のペーンのIdText</param>
        public PartsIllusionProjector(IRichPane target, string idtext)
        {
            _original = target;
            _screen = new RichPaneBinder(_original)
            {

                // プロパティ適用
                IdText = idtext,
                // 以下のプロパティは、デリゲートの_stateFunction関数で事あるごとに更新される
                Zoom = _original.Zoom,
                Scroll = _original.Scroll
            };
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~PartsIllusionProjector()
        {
            Dispose();
        }

        #region IDisposable メンバ

        public void Dispose()
        {
            if (_screen == null)
            {
                return;
            }
            _original.Control.Controls.Remove(_screen.Control);
            _screen = null;
            _original = null;
        }

        #endregion

        /// <summary>
        /// イリュージョン側のペーン（オリジナル側ではない）
        /// </summary>
        public IRichPane ScreenPane => _screen;

        /// <summary>
        /// パーツが所属する側のペーン（イリュージョン側ではない）
        /// </summary>
        public IRichPane OriginalPane => _original;
    }
}
