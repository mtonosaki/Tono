using System.Windows.Forms;

namespace Tono.GuiWinForm
{
    internal class FeatureFontChange : FeatureBase
    {
        #region �����i�V���A���C�Y���Ȃ��j
        /// <summary>�I�𒆂̃p�[�c�i���L�ϐ��j</summary>
        protected PartsCollectionBase _selectedParts;
        #endregion

        public override void OnInitInstance()
        {
            base.OnInitInstance();
            // �X�e�[�^�X����
            _selectedParts = (PartsCollectionBase)Share.Get("SelectedParts", typeof(PartsCollection));   // �I���ς݂̃p�[�c�ꗗ
        }

        /// <summary>
        /// �t�H���g�ύX�ł��邩�ǂ����H
        /// </summary>
        public override bool CanStart => _selectedParts.Count > 0;

        public override void Start(NamedId who)
        {
            base.Start(who);
            var fd = new FontDialog();


            // ������
            foreach (PartsCollectionBase.PartsEntry pe in _selectedParts)
            {
                if (pe.Parts is PartsTextBase)
                {
                    var fp = (PartsTextBase)pe.Parts;
                    fd.Font = fp.LastFont;
                    fd.Color = fp.FontColor;
                    break;
                }
            }
            fd.MinSize = 4;
            fd.MaxSize = 300;
            fd.ShowEffects = false;
            fd.ShowHelp = false;
            fd.ShowColor = true;
            fd.FontMustExist = true;
            fd.AllowVerticalFonts = false;

            // �I��
            if (fd.ShowDialog() == DialogResult.OK)
            {
                foreach (PartsCollectionBase.PartsEntry pe in _selectedParts)
                {
                    if (pe.Parts is PartsTextBase)
                    {
                        var fp = (PartsTextBase)pe.Parts;
                        fp.FontName = fd.Font.Name;
                        fp.FontSize = fd.Font.SizeInPoints;
                        fp.FontColor = fd.Color;
                        fp.IsItalic = fd.Font.Italic;
                        fp.IsBold = fd.Font.Bold;
                        fp.LastFont = fd.Font;
                        Parts.Invalidate(fp, pe.Pane);
                        Link.Equalization(fp);
                        Data.SetModified();
                        Token.Add(NamedId.FromName("TokenFontChanged"), this);
                    }
                }
            }
        }
    }
}
