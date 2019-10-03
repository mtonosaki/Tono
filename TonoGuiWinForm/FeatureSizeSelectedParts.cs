#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 選択されているパーツを移動させる
    /// </summary>
    public class FeatureSizeSelectedParts : FeatureMoveSelectedParts
    {
        #region	属性(シリアライズする)
        #endregion
        #region	属性(シリアライズしない)
        #endregion

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public FeatureSizeSelectedParts() : base()
        {
            _tokenMouseUpJob = NamedId.FromName("SizeMouseUpJob");
        }

        /// <summary>
        /// マウスダウン処理の最終決定
        /// </summary>
        protected virtual void finalMouseDownPart4Size()
        {
            var e = _clickPos;

            if (_selectedParts.Count > 0)
            {
                var tarPane = ClickPane;
                var parts = ClickParts; // Parts.GetPartsAt(e.Pos, true, out tarPane);
                if (parts != null)
                {
                    var isP = false;
                    switch (parts.IsOn(e.Pos, tarPane))
                    {
                        case PartsBase.PointType.OnLeft: _pos.SetDevelop(_developmentMode = PartsPositionManager.DevelopType.SizeLeft); isP = true; break;
                        case PartsBase.PointType.OnRight: _pos.SetDevelop(_developmentMode = PartsPositionManager.DevelopType.SizeRight); isP = true; break;
                        case PartsBase.PointType.OnTop: _pos.SetDevelop(_developmentMode = PartsPositionManager.DevelopType.SizeTop); isP = true; break;
                        case PartsBase.PointType.OnBottom: _pos.SetDevelop(_developmentMode = PartsPositionManager.DevelopType.SizeBottom); isP = true; break;
                    }
                    if (isP)
                    {
                        _mouseDownOriginal = e.Pos;
                        _pos.Initialize(_selectedParts);
                        Token.Add(TokenGeneral.TokenMouseDownNormalize, this);
                    }
                }
            }
        }

        public override void Start(NamedId who)
        {
            // MouseDownトークン
            if (who.Equals(_tokens[0]))
            {
                Finalizers.Add(new FinalizeManager.Finalize(finalMouseDownPart4Size));  // SezeSelected・・・も継承しているので、ID不指定
                                                                                    //			Finalizers.Add(_tokenListenID, new ffFinalizer.Finalize(_finalMouseDownPart)); by Tono 調査中 2006.2.2
            }
            // MouseMoveトークン
            if (who.Equals(_tokens[1]))
            {
                OnMouseMove(null);
            }
        }
    }
}
