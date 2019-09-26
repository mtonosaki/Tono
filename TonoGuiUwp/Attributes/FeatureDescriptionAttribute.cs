using System;

namespace Tono.Gui.Uwp
{
    public class FeatureDescriptionAttribute : Attribute
    {
        public string Jp { get; set; }
        public string En { get; set; }

        public FeatureDescriptionAttribute()
        {
        }

        public FeatureDescriptionAttribute(string desc)
        {
            Jp = desc;
        }
    }
}
