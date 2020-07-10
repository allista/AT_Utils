using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace AT_Utils.UI
{
    public class UI_Utils
    {
        public static List<Dropdown.OptionData>
            namesToOptions(IEnumerable<string> names, bool convertCamelCase = true) =>
            names?.Select(name =>
                    new Dropdown.OptionData(convertCamelCase ? FormatUtils.ParseCamelCase(name) : name))
                .ToList()
            ?? new List<Dropdown.OptionData>();
    }
}
