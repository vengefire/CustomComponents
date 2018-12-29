﻿using System.Collections.Generic;
using BattleTech.UI;
using fastJSON;
using UnityEngine;

namespace CustomComponents
{
    [CustomComponent("RGBColor")]
    public class RGBColorComponent : SimpleCustomComponent, IColorComponent, IAfterLoad
    {
        [JsonIgnore]
        public UIColor UIColor => UIColor.Custom;
        [JsonIgnore]
        public Color RGBColor { get; set; }

        public string Color { get; set; }


        public void OnLoaded(Dictionary<string, object> values)
        {
            if (ColorUtility.TryParseHtmlString(Color, out var color))
            {
#if CCDEBUG
                Control.Logger.LogDebug($"RGBCOLOR: {Color} -> {color}");
#endif
                RGBColor = color;
            }
            else
                RGBColor = UnityEngine.Color.magenta;
        }
    }
}