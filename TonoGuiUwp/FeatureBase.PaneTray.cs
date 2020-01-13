// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.
namespace Tono.Gui.Uwp
{
    public abstract partial class FeatureBase
    {
        public class PaneTray
        {
            private readonly FeatureBase _parent = null;

            /// <summary>
            /// the constructor of this class
            /// </summary>
            /// <param name="parent"></param>
            public PaneTray(FeatureBase parent)
            {
                _parent = parent;
            }

            /// <summary>
            /// Find a pane by name
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public IDrawArea this[string name] => _parent.View.GetPane(name);

            /// <summary>
            /// Get the owner TGuiView
            /// </summary>
            public IDrawArea Main => _parent.View;

            /// <summary>
            /// Set(Get) DrawTarget pane of this feature
            /// </summary>
            public IDrawArea Target { get; set; }
        }
    }
}
