//   FormatUtils.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2019 Allis Tauri

using System.Text.RegularExpressions;
using UnityEngine;

namespace AT_Utils.UI
{
    public static class FormatUtils
    {
        public const float G0 = 9.80665f; //m/s2

        /// <summary>
        /// The camel case components matching regexp.
        /// From: http://stackoverflow.com/questions/155303/net-how-can-you-split-a-caps-delimited-string-into-an-array
        /// </summary>
        private const string CamelCaseRegexp = "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))";

        private static readonly Regex CCR = new Regex(CamelCaseRegexp);

        public static string ParseCamelCase(string s) => CCR.Replace(s, "$1 ");

        public static string formatVeryBigValue(float value, string unit, string format = "F1")
        {
            var mod = "";
            if(value > 1e24)
            {
                value /= 1e24f;
                mod = "Y";
            }
            else if(value > 1e21)
            {
                value /= 1e21f;
                mod = "Z";
            }
            else if(value > 1e18)
            {
                value /= 1e18f;
                mod = "E";
            }
            else if(value > 1e15)
            {
                value /= 1e15f;
                mod = "P";
            }
            else if(value > 1e12)
            {
                value /= 1e12f;
                mod = "T";
            }
            else
                return formatBigValue(value, unit, format);
            return $"{value.ToString(format)}{mod}{unit}";
        }

        public static string formatBigValue(float value, string unit, string format = "F1")
        {
            var mod = "";
            if(value > 1e9)
            {
                value /= 1e9f;
                mod = "G";
            }
            else if(value > 1e6)
            {
                value /= 1e6f;
                mod = "M";
            }
            else if(value > 1e3)
            {
                value /= 1e3f;
                mod = "k";
            }
            else
                return formatSmallValue(value, unit, format);
            return $"{value.ToString(format)}{mod}{unit}";
        }

        public static string formatSmallValue(float value, string unit, string format = "F1")
        {
            var mod = "";
            if(value > 1)
                mod = "";
            else if(value > 1e-3)
            {
                value *= 1e3f;
                mod = "m";
            }
            else if(value > 1e-6)
            {
                value *= 1e6f;
                mod = "μ";
            }
            else if(value > 1e-9)
            {
                value *= 1e9f;
                mod = "n";
            }
            return $"{value.ToString(format)}{mod}{unit}";
        }

        public static string formatMass(float mass)
        {
            if(mass > 1)
                return formatBigValue(mass, "t", "n1");
            if(mass >= 0.1f)
                return $"{mass:n2}t";
            if(mass >= 0.001f)
                return $"{(mass * 1e3f):n1}kg";
            return $"{(mass * 1e6f):n0}g";
        }

        public static string formatVolume(double volume) =>
            volume < 1f
                ? $"{(volume * 1e3f):n0}L"
                : $"{volume:n1}m3";

        public static string formatDimensions(Vector3 size)
        {
            return $"{size.x:F2}m x {size.y:F2}m x {size.z:F2}m";
        }
    }
}
