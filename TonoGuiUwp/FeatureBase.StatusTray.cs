// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

namespace Tono.Gui.Uwp
{
    public abstract partial class FeatureBase
    {
        /// <summary>
        /// feature shared status manager object
        /// </summary>
        public class StatusTray
        {
            private readonly FeatureBase _parent;
            internal StatusTray(FeatureBase parent)
            {
                _parent = parent;
            }

            /// <summary>
            /// get shared status object by name
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public EventStatus this[string name] => _parent.View.GetStatus(name);
        }
    }
}
