#AT Utils ChangeLog

* **v1.2.2**
    * Added ClampedAssymetricFilter3D.
        * Renamed AsymmetricFilter.alpha/Tau[R|F] to [Up|Down].
    * Added Min property to Vector6.
    * Fixed PIDv_2/3 omega handling in Update methods.
    * No more default(Color) in GraphicsUtils.*Draw* methods.
    * Documented Timer class. Added Restart, and StartIf methods. Start method works only when the timer was not already started.
    * Added Vector3Field GUI for editing Vector3 components.
    * Added State<T>.ToString method.
    * Reworked OscillationDetector<T> framework.
        * Instead of a filtered boolean flag OD now provides filtered max.peak value.
        * Added OscillationDetectorF.
    * DebugUtils.CSV writes directly to a file, not to a common log.
    * DebugUtils.CSV now recognizes Vector3/4(d)
    * Reworked PID_Controller class family. Added PIDv_Controller3 with Vector3 PID parameters.
    * Added OscillationDetector3D that encapsulates 3 OscillationDetectorD for vector components.
    * Added many Vector3(d) extension methods:
        * ScaleChain(Vector3[]) that sequentially sacales given vector with each of the given list. Returns the result.
        * ClampMagnitude(Vector3,Vector3,Vector3) to clamp magnitude component-wise.
        * ClampComponentsH/L, Squared/Sqrt/PowComponents
        * Max/MinI, Component, Exclude, Max/MinComponentV, Max/MinComponentD(Vector3d).
        * Added ForEach(IList).
    * Fixed CNO.NodeName property. Added SaveInto method.
    * Updated AT-Utils.netkan file; updated to CC with .netkans.

* **v1.2.1**
    * Added alternative GLDrawPoint.
    * Fixed Utils.SaveGame. Added optional with_message argument.
    * Fixed AddonWindowBase configuration saving.
    * Fixed NRE in PartIsPurchased.
    * Fixed TypedConfigNodeObject.
    * Made AddonWindow a true singleton.
    * Added Part.UpdatePartMenu() extension method.
    * LeftRightChooser's width parameter now defines the total width of the controls, not only the field itself.
    * Added FloatField.IsSet property. Default formatting is changed to roundtrip. Added separate increment formatting.
    * Moved ToolbarWrapper to AT_Utils. Updated it from upstream.