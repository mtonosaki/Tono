#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// メニュー起動の言語切り替え（トークンで起動する場合、"TokenChangeLanguageTo_??"を投げる??の部分は、言語コード）
    /// </summary>
    public class FeatureChangeLanguage : FeatureBase, ITokenListener
    {
        protected string _code = "en";
        private NamedId _tokenTrigger = null;

        /// <summary>
        /// トークン起動のID
        /// </summary>
        protected NamedId tokenTrigger => _tokenTrigger;

        private void resetTokenTrigger()
        {
            _tokenTrigger = NamedId.FromName("TokenChangeLanguageTo_" + _code);
        }

        public override void ParseParameter(string param)
        {
            _code = param.Trim();
            resetTokenTrigger();
        }

        public override void OnInitInstance()
        {
            base.OnInitInstance();
            resetTokenTrigger();
        }

        public override void Start(NamedId who)
        {
            System.Diagnostics.Debug.WriteLine("Change Language to " + _code);
            Mes.SetCurrentLanguage(_code);
            Mes.Current.ResetText(Pane.Control.FindForm());
            DateTimeEx.SetDayStrings(Mes.Current);
            Pane.Invalidate(null);
        }

        #region ITokenListener メンバ

        public NamedId TokenTriggerID => _tokenTrigger;

        #endregion
    }
}
