// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// feature interface to support urgent token listening
    /// </summary>
    public interface IUrgentTokenListener
    {
        string UrgentToken { get; }
        void UrgentStart(EventToken token);
    }
}
