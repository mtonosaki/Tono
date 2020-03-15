// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// Hot data base class
    /// </summary>
    /// <remarks>
    /// sharing data between features
    /// Design policy: need to lock when accessing fields/properties
    /// </remarks>
    public class DataHotBase
    {
        /// <summary>
        /// redraw requesting flag
        /// </summary>
        public bool IsRedrawRequested { get; set; }

        /// <summary>
        /// parts moving flag（fcMoveSelectedParts）
        /// </summary>
        public Dictionary<FeatureBase, bool> IsPartsMoving { get; } = new Dictionary<FeatureBase, bool>();
    }
}
