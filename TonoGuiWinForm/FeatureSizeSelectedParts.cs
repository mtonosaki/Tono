#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �I������Ă���p�[�c���ړ�������
    /// </summary>
    public class FeatureSizeSelectedParts : FeatureMoveSelectedParts
    {
        #region	����(�V���A���C�Y����)
        #endregion
        #region	����(�V���A���C�Y���Ȃ�)
        #endregion

        /// <summary>
        /// �f�t�H���g�R���X�g���N�^
        /// </summary>
        public FeatureSizeSelectedParts() : base()
        {
            _tokenMouseUpJob = NamedId.FromName("SizeMouseUpJob");
        }

        /// <summary>
        /// �}�E�X�_�E�������̍ŏI����
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
            // MouseDown�g�[�N��
            if (who.Equals(_tokens[0]))
            {
                Finalizers.Add(new FinalizeManager.Finalize(finalMouseDownPart4Size));  // SezeSelected�E�E�E���p�����Ă���̂ŁAID�s�w��
                                                                                    //			Finalizers.Add(_tokenListenID, new ffFinalizer.Finalize(_finalMouseDownPart)); by Tono ������ 2006.2.2
            }
            // MouseMove�g�[�N��
            if (who.Equals(_tokens[1]))
            {
                OnMouseMove(null);
            }
        }
    }
}
