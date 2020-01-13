// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ƒIƒyƒ‰ƒ“ƒh‚P‚Â
    /// </summary>
    public class CommandOperand1
    {
        private readonly string _key;
        private readonly object _val;
        public CommandOperand1(string key, string val)
        {
            _key = key;
            _val = val;
        }
        public CommandOperand1(string key, int val)
        {
            _key = key;
            _val = val;
        }
        public CommandOperand1(string key, double val)
        {
            _key = key;
            _val = val;
        }
        public CommandOperand1(string key, bool val)
        {
            _key = key;
            _val = val;
        }
        public string Key => _key;
        public object Value => _val;
        public override int GetHashCode()
        {
            return _key.GetHashCode();
        }
    }
}
