// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;

namespace Tono.Gui.Uwp
{
    public abstract partial class FeatureBase
    {
        /// <summary>
        /// feature event trigger management sharing tray
        /// </summary>
        public class TokenTray
        {
            private readonly FeatureBase _parent;
            internal TokenTray(FeatureBase parent)
            {
                _parent = parent;
            }

            /// <summary>
            /// add a linked event token to throw to other features.
            /// </summary>
            /// <param name="previousToken">previous token</param>
            /// <param name="token">throw new token</param>
            public void Link(EventToken previousToken, EventToken token)
            {
                if (previousToken == null)
                {
                    AddNew(token);
                }
                else
                {
                    token.Previous = previousToken;
                    _parent.View.AddToken(token);
                }
            }

            /// <summary>
            /// add a new event token to throw other features.(NOTE: Use Link method if you have received token)
            /// </summary>
            /// <param name="newtoken">new token</param>
            public void AddNew(EventToken newtoken)
            {
                newtoken.Previous = null;
                _parent.View.AddToken(newtoken);
            }

            /// <summary>
            /// add a linked token in urgent mode.
            /// </summary>
            /// <param name="previousToken"></param>
            /// <param name="token"></param>
            /// <remarks>Use Link method instead of this one. This is for system purpose</remarks>
            public void LinkUrgent(EventToken previousToken, EventTokenTrigger token)
            {
                token.Previous = previousToken;
                _parent.View.AddUrgentToken(token);
            }

            /// <summary>
            /// set a new one time action that will be call automatically when token tray is empty
            /// </summary>
            /// <param name="act"></param>
            public void Finalize(Action act)
            {
                _parent.View.AddFinalizeAction(act);
            }
        }
    }
}
