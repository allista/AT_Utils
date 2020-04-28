#AT Utils ChangeLog

* **v1.9.2**
    * Fixed UI bundle loading
    * Changed Min/Max KSP versions to 1.9.0 - 1.9.1
    * Changed references to KSP-1.9.1

* v1.9.1
    * MagneticDamper
        * dampPackedVessels works one last time for just-upacked vessels
        * PAW group starts collapsed if controls are disabled in MODULE
        * added OnDamperAutoDisabled callback
        * do not execute late update when not in flight or in pause
        * added GetforceForAcceleration public method
        * limiting the total force acted upon ALL rigid bodies in the field
        * added public attractorAxisW property
        * made attractor transform and attractorAxis public
        * added auto-deactivation by timer if damper is empty
        * renamed AutoEnable gui text to "Field Activation: Auto/Manual"
        * added AutoEnable PAW toggle and OndamperAutoEnabled callback
        * added dynamic PAW grouping
        * disable the module if there's no sensor
        * thermal losses are included in energy spent by the damper
        * added Thermal Losses to part tooltip in Editor
        * renamed EnergyToThermalK to ThermalLossesRatio for clarity
        * rename Damper to Sensor and dampers to sensors
        * moved all dampening logic from Damper to the main module
        * added Damper.VesselsInside set of persistentIds
        * extracted addDamper protected method from OnStart
        * AddThermalFlux is in kW, not kJ
        * generate heat flux while working
        * MaxField should be inactive when there's no damper
        * corrected MaxField guiName to Damper Max. Force
        * using multiple Sensors and thus multiple Dampers
    * Added CargoAccelerators assets as a symbolic link; added CA bundle definition
    * Added LabelFigure prefab to AT_Utils.UI
    * Added FormatUtils.G0 constant
    * Added ColorizerBase.UpdateColor and Selectable.SetInteractable extension
    * Added Orbital/OrbitalUtils that extends Utils class
    * Added Math/PhysicsUtils that extends Utils class; added AngularAcceleration
    * Added Utils.FromToRottation - QuaternionD version, since native is broken
    * Moved SetUnpackDistance from TCA to VesselExtensions
    * Fixed NRE in DEBUG label in TooltipManager
    * Fixed ATGroundAnchor that was not working after vessel switch

* v1.9.0
    * Core:
        * Added UniversalDrill to BadParts as per @pmborg finding
        * Fixed singleton implementation of BackupLogger
        * Fixed singleton implementation of AddonWindowBase
        * Fixed singleton implementation of ScenarioTester
        * Fixed singleton implementation of ScenarioTester
        * Fixed singleton implementation of ComputationBalancer
        * Added static bool property ComputationBalancer.Running
        * Moved ComputationBalancer to Addons
        * Instantiating ToolbarWindow in Awake of the ToolbarManager. Fixed #18
        * Added IAnimator interface that describes minimal MultiAnimator protocol
        * ATGroundAnchor uses onPartPack/Unpack GameEvents instead of obsolete methods
        * Added ability to animate ATGroundAnchor via IAnimator module
        * Moved ResourceInfo to Resources
        * ResourcePump.Request is now a public ro property
        * Part.Title extension method works with part == null
        * ATMagneticDamper:
            * Damper holds damped vessels still in TimeWarp
            * Damper consumes EC when damping something: EnergyConsumptionK
            * Damper consumes EC when enabled and idle: IdleEnergyConsumption
            * Renamed magnet to attractor
            * Attractor power is configurable in flight: AttractorPower
            * Attractor may be inverted to become repulsive: InvertAttractor
            * Total force that is applied to a rigid body is capped: MaxForce + MaxEnergyConsumption   
            * Added module info to part tooltip
            * The layer of the sensor object is set OnStart: 21 in editor, 2 in flight
            * Added ATMagneticDamperUpdater to APR
            * When computing relative RB velocity, take angular velocity of the host into account
            * Added ability to animate working MagneticDamper with an IAnimator module
            * Does not affect Rigidbodies of the same vessel
        * VesselSpawner + SpawnSpaceManager:
            * When setting external SpaceMetric need to update spawn_space_sorted_size
            * Fixed inability of the VesselSpawner to disable launched vessel colliders on time
            * Fixed vessel stabilization when spawning to ground
        * Added frameCount to log message header
        * DebugUtils.formatTransformTree also prints activeSelf and activeInHierarchy
        * Moved DEBUG info panel (clock,frameCount,situation) from TCA to TooltipManager
    * Anisotropic Part Resizer:
        * Fixed PAW freezing when resized part was in symmetry group
        * **BREAKING**: using BaseField.OnValueModified instead 
          of UI_Control.onFiledChanged to change size/aspect
        * Added ATMagneticDamperUpdater
        * Removed static AnisotropicResizable.unequal
        * Set caps for breakingForce/Torque to 50k to be closer 
          to current stock values
        * Fixed AnizotropicResizable.GetModuleMass/Cost calculations
        * **BREAKING**: ResourcesUpdater is IPartCostModifier and only 
          handles resources present in prefab
        * **BREAKING**: In Scale treating aspect like size -- with respect 
          to original aspect
        * Fixed NRE in JettisonUpdater
        * Various small fixes and refactoring
    * Multi Animators:
        * Forward/ReverseSpeed is changed on rescale
        * In editor slow animations are played in 3s despite their duration
        * Calculating hasSound and hasEmitter in OnStart once to avoid expencive null check
        * Renamed ntime to n_time for clarity
        * Removed redundant override of FixedUpdate in MultiLights
        * Changed enable/disable light labels in PAW
        * Corrected typos
        * Deprecated AnimatedGroundAnchor
        * Using AT_Utils.IAnimator interface

* v1.8.3
    * Changed default Min/Max Size/Aspect of APR to [0.1, 1000]
    * ToggleAction of MultiAnimators syncs with its action group if it's set
    * In MultiGeometryAnimator made drag cube names configurable
      to be able to use predefined cubes.
    * Added OnHoverTrigger UI component
    * Add the trash icon to at_utils_ui asset bundle
    * Added Indicator UI component
    * Added ClickableLabel UI component
    * Made UIWindowBase.SyncState virtual
    * Added UIWindowBase.onGamePause/Unpause virtual methods that are called
      when corresponding GameEvents are fired
    * Considering a vessel to be OnPlanet only when its orbit radius is less
      than MinPeR
    * Allow for cancellation of a ComputationBalancer.Task

* v1.8.2
    * Extracted SpatialSensor from SpawnSpaceManager.SpawnSpaceSensor.
        Added several factories to add the sensor to GO starting from 
        different components like MeshFilter or Collider.
    * Added on_vessel_positioned callback to VesselSpawner.SpawnProtoVessel
    * Ignoring GForces while vessel goes off rails in VesselSpawner
    * VesselSpawner calls on_vessel_off_rails before switching to it
    * SpawnSpaceManager supports SpawnOffset AND AutoPositionVessel at the same 
        time.
    * Braking change: removed GetSpawnOffset methods. Instead, GetSpawnTransform
        returns out spawn_offset parameter.
    * Added additional_rotation argument to GetSpawnTransform methods
    * Removed AutoPositionVessel option from SpawnSpaceManager
        * Instead, added GetSpawnRotation and GetOptimalRotation methods that 
            allow calculation of the rotation that was used when AutoPositionVessel was used.
        * Renamed additional_rotation argument of GetSpawnTransform to just 
            rotation.
    * Fixed NRE in ToggleColorizer
    * Optimized initial positioning of the panel; still doesn't work though)
    * Fixed NRE during spawning of a vessel that is quickly destroyed
    * Corrected some typos
    * Fixed the crash on right click on toolbar buttons
    * Fixed NRE when calling SaveState on an Instance that's no longer there
    * Hiding subwindows if a parent window is not shown
    * Checking for null return value when adding component to a GO
    * Better logging from AppToolbar
    * Checking for null value when running Show coro of UIWindowBase on a MB
    * Initialize UIWindow Controller position without waiting for it to resize 
        itself, because sometimes it doesn't do this and the waiting is infinite.
    * Saving state of a UIWindow when it is closed.
    * Added NIGHTBUILD build configuration
    * Separated extension classes to their own files under Extensions directory
    * Added Part.TryUseResource extension method
    * Added Utils.GetLayer/GetLayers method to resolve layer masks from layer 
        names and cache the results.
    * Changed target framework to .NET-4.5
    * Added required Unity-2019 Module dlls
    * Using new EventType constants
    * Using MonoUtilities.RefreshPartContextWindow per SQUAD request
    * Fixed ArgumentException and NREs caused by not-found material
    * Fixed Additive material path
    * Storing background textures for styles in Styles class.
        * Background textures in Unity 2019 should be persisted outside of the 
            GUISyleState.background
    * Moving GUIContent instances to static members for performance.
    * Added Toggle.SetIsOnAndColorWithoutNotify extension.

* v1.8.1
    * Made UIBundle reusable for different bundles. Extracted UIWindowBase code from Styles
        * Derived ColorListWindow from UIWindowBase, both being extracted from Styles; UIWindowBase will be the base for the new UI framework on uGUI.
        * Added UI folder for the new framework
    * Fixed ScreenBoundRect, extracted static ClampToScreen method from OnDrag
    * Changed mku to μu in formatUnits
    * Fixed references to UnityEngine+.UI; should not make lockal copies of them
    * Added PanelledUI and FloatValueUI base classes and FloatController UI prefab
    * Added TooltipView+TooltipTrigger framework
    * Added AT_Utils.UI.FormatUtils.cs with the code from MiscUtils for formatting
    * Added CommonEvents.cs with UnityEvents subclasses for common use
    * Added Extensions class and backported Toggle.SetIsOnWithoutNotify
    * Added PluginState static class that handles persistent configs
        * The code was extracted from GUIWindowBase. Now everything that needs to persist its configs have to use PluginState.
        * For that there are two attributes: ConfigOption for fields and PersistState for classes. The former marks fields that should be saved and restored, the later is used to recursively save/load state of a tree of objects.
        * Added ICachedState interface to PluginState framework to allow classes to sync their runtime state before saving it to config.xml
    * Made ToggleColorizer.UpdateColor public
        * To change toggle color when isOn is set without calling the callback
    * Added TooltipView via TooltipWindow to TooltipManager addon
    * Added Label prefab that is a Panel->Text composite
    * Fixed DragableRect in case of parents changing midflight
    * Fixed DragableRect position calculation; using IBeginDragHandler to catch the start
    * Added PanelledUI.IsActive property
    * Added EmptyPanel prefab
    * Added Panel prefab
    * Added ColorSetting.Alpha method to return the Color with changed alpha value
    * SpawnSpaceManager adds separate GO for AutoPosition transform

* v1.8.0
    * **APR: Updating AttachNodes with the model OnLoad**
    * Added DebugUtils.SetupHullMeshes Vessel extension method.
    * **Fixed ConvexHull3D calculation** that added zero vector to the hull in some cases
    * SimpleDialog and subclasses accept one-time callbacks
    * Added WorldSpaceTrace script to add temporary markers to WorldSpace
    * Added BoundTriangles to easily create meshes from BoundCorners output.
    * Added Utils.standard_material and comments on how to set up the Standard Unity shader
    * **Reimplemented SimpleDialog and its subclasses to use callbacks and OnGUI**
    * SimpleScrollView is hidden by default.
    * **SimpleScrollView is drawn in its own OnGUI method.**
    * Added ButtonSwitch(GUIContent) methods

* v1.7.0
    * Added Styles.onSkinInit callback
    * Added Styles.ToggleStylesUI MonoBehaviour extension method
    * Added Styles.MakeButton(Color) method to unify button creation.
    * Added AppToolbar addon to generalize handling of AppLaucher/Toolbar buttons
    * Moved ToolbarWrapper to GUI
    * Added ConfigNodeObject.SavePartial to programmatically manage .user override files
    * GC49 added dmFlexoTube parts to BadParts list
    * Removed ResourceHack config
    * Added AT_Utils.UI - new UI framework based on uGUI
    * Using AT_Utils.UI.Colors framework to change UI colors at runtime
    * Moved static CurrentCamera prop from Markers to GraphicsUtils
    * Added VectorsityLineRenderer and UnityLineRenderer to simplify 3D-spline usage
    * Removed Draw methods using dynamic Meshes

* v1.6.3
    * Added ShipConstruct.SelectPart(flagURL)
    * Added PartMaker to make a part from the name and a ShipConstruct from the part
        * Added PartSelector: like SubassemblySelector, but selects AvailableParts
    * Added Utils.PartIsPurchased(AvailablePart)
    * Try and catch everything while creating a part in PartMaker.
        * Also, consider the possibility of unequal module nodes and modules counts.
        * And of modules that add other modules (like CC does).
    * Safer ForEach extension; fixed NRE in StartState.
    * PartSelector uses pagination, async loading and title filter.
    * Fixed Out of memory exception in ComputationBalancer on scene change
    * ResourcePump.request is double, which fixes some issues with KSP-1.6

* v1.6.2
    * Added StringID extensions for classes in ksp object's hierarchy.
        * To better track what happens where during debugging.
    * Fixed SelectMax; .Log extensions use GetID() for prefixes.
    * Added NamedDockingPort module
    * Vector3d and Orbit are serialized in CNO. Added bin->base64 de/serialization.
    * Added SimpleScrollView GUI component
    * Added default constructor to ResourceInfo
    * Fixed NRE in UnlockControls and OnDestroy.
        * Apparently, reference type fields should be initialized in Awake, not in class definition.
    * Fixed check for bad parts in Metric; added MKS drills' names to the list.
    * Fixed UpdateAttachedPartPos extensions.
        * Now they distinguish between flight and editor and do not mess with the orbits.
    * Fixed staging of spawned ship constructs.

* **v1.6.1**
    * Added DebugUtils.formatPartJointsTree
    * Fixed Local2Local; fixed UpdateAttachedPartPos*
    * Fixed messed up part rotation when animating part resizing
    * Added Transform.(Inverse)TransformPointUnscaled extensions
    * Fixed VesselSpawner.SpawnShipConstruct positioning.
    * Consider disabled renderers when searching for AffectedObjects in STS

* v1.6.0.1 -- compatible with KSP-1.4.5

* v1.6.0
    * Added ShipConstructLoader component that loads both complete .crafts and subsassemblies.
    * GUIWindowBase.HUD_enabled takes into account level_loaded flag.
    * Added SlowUpdate coroutine helper.
    * Both IShipConstruction.Bounds and Metric support world-space calculation.
    * Added variants of ButtonSwitch that change the label on switch as well as style.
    * **Merged HangarSpaceManager into SpawnSpaceManager**, which now uses .Init instead of .Load to get spawn transform/mesh.
        * Added more variants of GetSpawnTransform/Offset.
        * Added an option to setup a sensor collider to SpawnSpace.
    * Added VesselSpawner class that generalizes Hangar's algorithm to launch vessels in flight from both ProtoVessels and ShipConstructs, and also to launch ShipConstructs to ground, as GC does.
    * Added ATGroundAnchor module that combines GC's and Hangar's anchors.
    * Added ATMagneticDamper module that slows down parts inside a trigger collider defined on the specified mesh.
    * **Removed ModuleAsteroidFix**
    * Added check_module static method to SimplePartFilter.
    * Fixed Metric and Bounds for RadialDrill via explicit workaround.
    * Adde MeshesToSkip and BadParts lists to AT_UtilsGlobals.
    * Optimized IShipConstruct.Bounds.
    * Added Part.AllModelMeshes extension; used in Metric.
    * Added MeshFilter.AddCollider extension; used in SpawnSpaceManager and ATMagneticDamper.
    * Added MetricDebug module to visualize both Metric and .Bounds
    
* v1.5.2
    * Added FeedForward input to PID controllers.
    * Fixed FCO mode switching without changing the anchor.
        * Plus, OrbitAround anchors at CoMs of vessels.
    * CNO saves non-public fileds.
    * Added Ratched trigger; probably not a good name, but still.
        * Anyway, works like this: input is a boolean condition that normally should be True; when it first becomes False, the Ratchet becomes "armed"; then if it becomes True again, the Ratched becomes True itself and stays this way.
    * Added formatting of Vector2(d) to convert_args
    * Added .Draw() method to ITestScenario.
        * Scenarios are created at game start and then only initialized, updated and cleaned.
    * Added Vector2d.Rotate extensions; AtmosphereParams are a struct now.
        * Added Orbit.Contains(UT), ApA/PeAUT, Ap/PeV extensions.
    * Added Vector2d.Angle2 and Vector3(d).ClampDirection extensions.
    * In ValueFileds iformat defaults to format instead of "F1"
        * Made runtime fields non-persistent.
    * Added FuzzyThreshold.Reset() method
    * OscillationDetector.max_filter is assymmetric.
    * Added StallDetector class hierarchy; added ComputationBalancer KSPAddon.
        * ComputationBalancer computes tasks made as IEnumerables (like coroutines) inside its Update event, trying to keep FPS from dropping too low.
        * It works in Flight, Editor and KSC, and also when the game is paused.
        * Also moved ProgressIndicator here from TCA.

* v1.5.1
    * Moved HangarSpaceManagers and SubassemblySelector from Hangar here.
        * Moved all addons to Addons subfolder.
        * Moved all modules to Modules subfoldre.
        * Added ResourceInfo CNO to proxy calls to PartResourceDefinition.
    * Added TextureCache static class to load and store fullres icons.
    * SimplePartFilter uses TextureCache.
    * Added direct support for GUID fields to CNO. Added PersistentDictS<T> class.
    * Added ConvexHull3D.MakeMesh method.
    * Fixed SkinnedMeshRenderer handling by the Metric.
        * Using ResourceInfo("ElectricCharge") for static EC information.
        * Added Metric.hull_mesh lazy property.
    
* v1.5.0
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
