#AT Utils ChangeLog

* **v1.5.0**
    * Using rel pivot distance instead of constant MAX_DIST for LookAt modes.
    * OnPlanet uses additional PeR check using Orbit.MinPeR extension.
    * Moved AngleTo/DistanceTo/*Pos methods to Coordinates.
    * Added CDOS_Optimizer2D generic 2d-function optimizer. Added BackupLogger addon.
        * CDOS_Optimizer2d_Generic is an implementation of the CDOS algorithm for 2d case.
        * BackupLogger does two things: 1. it copies the full Unity player log each second to a time-tagged file (one per game start) to retain its content in case of accidental game restart. 2. it can log anything to a separate time-tagged (per game start) file.
    * Fixed argument types for ClampMagnitude(Vector3d, double).
    * Using BackupLogger to backup Utils.Log messages in DEBUG mode.
    * Added HierarchicalComparer and ValueFieldBase classes.
        * HC is a container for optimization constrains of different importance. VFB is the base of all field widgets; it adds an important ability to set value with Return key.
    * Removed redundant using-s; removed commented out class embryo.
    * Reimplemented Fileds using ValueFieldBase class.
    * Added Utils.GLLines method to draw polyline more efficiently.
    * Fixed orto_shift; added some more debug logging. WIP still.
    * Commented out logging of stack trace, since KSP Development Build does it anyway.
    * 1.3.1 compatibility of SimplePartFilter (NRE fix)
        * Squad changed capitalization of "Filter by Function" and "Filter by Module". Also can be used #autoLOC_453547 for "Filter by Function" and #autoLOC_453705 for "Filter by Module".
    * Merge pull request #5 from jarosm/patch-1
        * 1.3.1 compatibility of SimplePartFilter (NRE fix)
    * Added optional node_name argument to CNO LoadFrom and SaveInto methods.
    * Added ModuleAsteroidFix to preven asteroids from changing form on load.
    * Using Localizer as suggested by maja.
    * Fixed CrewTransferBatch.
    * Using Angle2 in Markers.
    * Small refactoring of Filters.
    * Added docstrings; added Angle2* methods to Utils.
    * Added Vector6.Inverse and .Slice methods.
    * Added formatComponents(Vector3) method to Utils.
    * SimplePartFilter excludes parts in "none" category.
    * Reimplemented and renamed RelSurf/OrbPos methods of Coordinates.
        * After the implementatino of the corresponding CB methods was changed.
    * Made PID.Action and IntegralAction public. Added setClamp method.
        * Overloaded Update method with (error,speed) signature.
        * Added PIDf_Controller3.
    * Added Reset to OD. Fixed normalization.
    * Fixed CDOS optimizer that stuck at feasible borders.
    * GL methods accept material as mat argument.
    * ValueFields accept value both on Enter and KeypadEnter.
    * Added style argument to *Field.Draw methods.
    * Fixed metric and bounds calculation.
    * Renamed Profiler to AT_Profiler to avoid name clash with Unity.
    * Performance optimizations.

* v1.4.4
    * Added kerbal traits and level to CrewTransferWindow; Closed #4
    * Added a slghtly optimized version of GLDrawHull.
    * Converted DebugInfo props of filters into ToString overrides.
    * Added two Utils.LerpTime methods.
    * In SimplePartFilter:
    	* Fixed module matching with moduleInfo.moduleName: Converted MODULES to List of strings that is filled with SetMODULES methods that accepts IEnumerable of Types and converts Type.Name to moduleName/categoryName format.
    	* Added default implementation of the filter method.

* v1.4.3
    * Compatible with KSP-1.3
    * Added simple emailer class (works only with local spooler). Added ScenarioTester framework for automated continious testing.
    * Changed log message upon scenario registration.
    * Added formatTimeDelta, formatCB, formatPatches methods to Utils.
    * Added Extremum.ToString method.
    * Added Vessel.BoundsWithExhaust, Orbit.MinPeR; improved Vessel.Radius calculation.
    * Added ConfigNodeObjectGUI framework. Mainly for debugging.
    * Made Float/Vector3Field implement ITypeUI.
    * Added CreateDefaultOverride method to PluginConfig.
    * Added ClampSignedH/L family of methods, added Circle method to Utils.
    * Made PID.IntegralError public; changed notation of PID class names; added PIDvf and PIDv controllers.
    * Added LookBetween and LookFromTo modes for FCO.
    * Added OrbitAround mode for FCO; changed naming scheme for methods its.
    * Made Bounds an extension of IShipconstruct.

* v1.4.2
    * Added patch for FilterExtensions to properly classify APR parts as "proc" bulkheadProfiles.
    * Added SimplePartFilter class to add custom part subcategories.
    * Fixed calculations of subwindow placement in compound windows.
    * Corrected formatOrbit method.
    * Added double version of EWA.
    * Added Multiplexer.ToString
    * Added Coordinates.normailize_coordinates method to deal with any lat/lon.
        * Coordinates are automatically normalized on creation and Load.
        * Added NormalizedLatitude static method.
    * Prev/Next extensions of SortedList return Key instead of Value.
    * Corrected namespace for Markers class.

* v1.4.1
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

* v1.4.0
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