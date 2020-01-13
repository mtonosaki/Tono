// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// tcó·äOëSî 
    /// </summary>
    public class CommandException : Exception
    {
        public CommandException(string mes)
            : base(mes)
        {
        }
        public CommandException(string mes, params object[] prms)
            : base(string.Format(mes, prms))
        {
        }
    }

    /// <summary>
    /// çƒê∂éûÇÃó·äO
    /// </summary>
    public class CommandPlayException : CommandException
    {
        public CommandPlayException(string mes)
            : base(mes)
        {
        }
        public CommandPlayException(string mes, params object[] prms)
            : base(string.Format(mes, prms))
        {
        }
    }
}
