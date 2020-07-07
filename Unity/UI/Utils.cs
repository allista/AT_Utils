using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace AT_Utils.UI
{
    public class UI_Utils
    {
        public static List<Dropdown.OptionData> namesToOptions(IEnumerable<string> names) =>
            names.Select(name =>
                    new Dropdown.OptionData(FormatUtils.ParseCamelCase(name)))
                .ToList();
    }
}
