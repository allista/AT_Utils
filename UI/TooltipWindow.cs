//   TooltipWindow.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2019 Allis Tauri
using AT_Utils.UI;
namespace AT_Utils
{
    public class TooltipWindow : UIWindowBase<TooltipView>
    {
        public TooltipWindow() : base(AT_UtilsGlobals.Instance.AssetBundle) { }
    }
}
