#AT Utils ChangeLog

* **v1.4.1**
    * Sorted GUI classes into subfolders. Added SimpleDialog framework.
    * Included call to GUIWindowBase.can_draw() into doShow property.
    * Removed Closed fields/property from transfer windows; using WindowEnabled instead.
    * Fixed AddonWindowBase.ShowWithButton.
    * Added Utils.MouseInLastElement() method.
    * Changed default control lock type and renamed LockEditor to LockControls.
    * Added Part.HighlightAlways extension.
    * Vessel.Radius extension uses GetTotalMass call instead of totalMass field for unloaded vessels.
    * Changed scroll-bar styles and tooltip color. Merged InitSkin and InitGUI methods of Styles class.
    * Fixed tooltips for scroll-views; limited them in width, enabled word-wrap.
    * Added State.Empty property.
    * Utils.ParseLine now returns empty array instead of null if argument is empty.
    * Added SubmoduleResizable module in a separate dll; for @Enceos.
    * Added ToString overrides to all ResourceProxy classes; using ListDict instead of plane Dictionary in VesselResources.
    * Removed transferNow flag from ResourceTransferWindow; added TransferAction delegate instead.

* **v1.4.0**
    * Added no_window style for borderless, titleless transparent window.
    * Fixed PartIsPurchased for ScienceSandbox mode.
    * **TooltipManager** is now a KSPAddon and is drawing tooltips above everything by itself.
    * Reimplemented **GUIWindowBase** framework:
        * To save some field in the GUI_CFG, this field has to be tagged with
        * ConfigOption attribute. The Load/SaveConfig handle such fields using Reflection.
        * Added subwindows framework: any field that is a GUIWindowBase itself is automaticaly initialized in Awake and its Save/LoadConfig methods are called when needed.
        * ALL GUIWindowBase, being MonoBehaviour as they are, have to be instantiated using GameObject.AddComponent, not the new keyword. For subwindows this is done automatically. For any GUIWindowBase fields in other classes this should be done by hand.
        * GUIWindowBase tracks scene changes to hide itself when no UI should be shown.
        * Added Move method.
        * window_enabled is true by default.
        * UnlockControls unlocks controls for all subwindows.
        * Moved Show and Toggle methods from AddonWindowBase here.
        * Moved can_draw() from AddonWindowBase here.
        * Renamed AddonWindowBase.Show/Toggle static methods to ShowInstance/ToggleInstance. The instance itself is made public and is now called Instance.
        * Added AddonWindowBase.Show/ToggleWithButton static methods to handle AppLauncher buttons.
    * SimpleDialog uses rich_label style.
    * Added **CompoundWindow** framework.
        * Added AnchoredWindow, SubWindow and CompoundWindow base classes.
        * Added SubwindowSpec attribute to control SubWindow rendering.
        * Added FloatingWindow as a CompoundWindow subclass.
        * GUIWindowBase:
    * Fixed NRE in Coordinates.GetAtPointer/Search methods.
    * Added InOrbit/OnPlanet Vessel extension methods.

* v1.3.1.1
    * Numerous small bugfixes.

* v1.3.1
	* Moved CrewTransferBatch from Hangar here.
    * Fixed resource transfer UI.
    * Removed IsPhysicallySignificant as it was obsolete.
    * Improved GLDrawBounds/Point.
    * Fixed Metric calculation: disabled objects are not taken into account + all parts have mass. Fixed #176 issue in Hangar's bugtracker.
    * Moved debug routines into Debugging. Added ResourceHack module to replenish resources in flight.
* v1.3.0
    * Compiled against **KSP-1.2.2**
    * Added **SerializableFieldsPartModule** -- a base PartModule that uses reflection to serialize any field with [SerializeField] attribute that is of either [Serializable] type, or an IConfigNode, or the ConfigNode itself.
    	* So **ConfigNodeWrapper is now obsolete.**
    * Added **Resource Transfer framework** that facilitates transfer of resources between ships represented by either a Vessel object, a ProtoVessel object or a ConfigNode produced by ProtoVessel.Save.
    * Factored most of the **SimpleTextureSwitcher** into the new **TextureSwitcherServer**.
        * The STS now only provides the UI, while all the switching is done by TSS.
    * Moved from TCA: Coordinates; Draw*Marker methods Markers static class.
    * Added support for persistent ConfigNode fields. Added PersistentList/Queue. Added CNO.LoadFrom method.
    * Added calculation of ConvexHull Volume and Area.
    * Added Queue extensions: FillFrom, Remove, MoveUp.
    * Added tooltip to LeftRightChooser; Added generic variants of the chooser.
    * Added Coordinates.OnWater flag and SurfaceAlt(under_water) option.
    * Fixes and improvements:
        * Fixed VectorCurve.Load, improved VectorCurve.Save performance using string.Join.
        * Optimized CNO Load/Save performance.
        * Fixed ConvexHull calculation in Metric.init_with_mesh.
	    * Made LockName of GUIWindowBase unique.
	    * Fixed Utils.formatSmallValue. Improved performance of convert_args.
    	* Declared Metric as IConfigNode for easy persistence. Added Metric(IShipConstruct).
    	* Made tooltips light-green with black text for better visibility.
        * Fixed Part.TotalCost.
        * Added ShipConstruct.Unload extension.
* v1.2.2
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