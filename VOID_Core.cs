//
//  VOID_Core.cs
//
//  Author:
//       toadicus <>
//
//  Copyright (c) 2013 toadicus
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using KSP;
using UnityEngine;
using Engineer.VesselSimulator;

namespace VOID
{
	public class VOID_Core : VOID_Module, IVOID_Module
	{
		/*
		 * Static Members
		 * */
		protected static bool _initialized = false;
		public static bool Initialized
		{
			get 
			{
			return _initialized;
			}
		}

		protected static VOID_Core _instance;
		public static VOID_Core Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new VOID_Core();
					_initialized = true;
				}
				return _instance;
			}
		}

		public static void Reset()
		{
			_instance.StopGUI();
			_instance = null;
			_initialized = false;
		}

		public static double Constant_G = 6.674e-11;

		/*
		 * Fields
		 * */
		protected string VoidName = "VOID";
		protected string VoidVersion = "0.9.15";

		protected bool _factoryReset = false;

		[AVOID_SaveValue("configValue")]
		protected VOID_SaveValue<int> configVersion = 1;

		protected List<IVOID_Module> _modules = new List<IVOID_Module>();
		protected bool _modulesLoaded = false;

		[AVOID_SaveValue("mainWindowPos")]
		protected VOID_SaveValue<Rect> mainWindowPos = new Rect(475, 575, 10f, 10f);

		[AVOID_SaveValue("mainGuiMinimized")]
		protected VOID_SaveValue<bool> mainGuiMinimized = false;

		[AVOID_SaveValue("configWindowPos")]
		protected VOID_SaveValue<Rect> configWindowPos = new Rect(825, 625, 10f, 10f);

		[AVOID_SaveValue("configWindowMinimized")]
		protected VOID_SaveValue<bool> configWindowMinimized = true;

		[AVOID_SaveValue("VOIDIconPos")]
		protected VOID_SaveValue<Rect> VOIDIconPos = new Rect(Screen.width / 2 - 200, Screen.height - 30, 30f, 30f);
		protected Texture2D VOIDIconOff = new Texture2D(30, 30, TextureFormat.ARGB32, false);
		protected Texture2D VOIDIconOn = new Texture2D(30, 30, TextureFormat.ARGB32, false);
		protected Texture2D VOIDIconTexture;
		protected string VOIDIconOnPath = "VOID/Textures/void_icon_on";
		protected string VOIDIconOffPath = "VOID/Textures/void_icon_off";
		protected bool VOIDIconLocked = true;

		protected int windowBaseID = -96518722;
		protected int _windowID = 0;

		protected bool GUIStylesLoaded = false;

		protected Dictionary<string, GUIStyle> _LabelStyles = new Dictionary<string, GUIStyle>();

		[AVOID_SaveValue("togglePower")]
		public VOID_SaveValue<bool> togglePower = true;

		public bool powerAvailable = true;

		[AVOID_SaveValue("consumeResource")]
		protected VOID_SaveValue<bool> consumeResource = false;

		[AVOID_SaveValue("resourceName")]
		protected VOID_SaveValue<string> resourceName = "ElectricCharge";

		[AVOID_SaveValue("resourceRate")]
		protected VOID_SaveValue<float> resourceRate = 0.2f;

		[AVOID_SaveValue("updatePeriod")]
		protected VOID_SaveValue<double> _updatePeriod = 1001f/15000f;
		protected float _updateTimer = 0f;
		protected string stringFrequency;

		// Celestial Body Housekeeping
		protected List<CelestialBody> _allBodies = new List<CelestialBody>();
		protected bool bodiesLoaded = false;

		// Vessel Type Housekeeping
		protected List<VesselType> _allVesselTypes = new List<VesselType>();
		protected bool vesselTypesLoaded = false;

		public float saveTimer = 0;

		protected string defaultSkin = "KSP window 2";

		[AVOID_SaveValue("defaultSkin")]
		protected VOID_SaveValue<string> _skinName;
		protected Dictionary<string, GUISkin> skin_list;
		protected List<string> skinNames;
		protected string[] forbiddenSkins =
		{
			"PlaqueDialogSkin",
			"FlagBrowserSkin",
			"SSUITextAreaDefault",
			"ExperimentsDialogSkin",
			"ExpRecoveryDialogSkin",
			"KSP window 5",
			"KSP window 6"
		};
		protected bool skinsLoaded = false;

		public bool configDirty;

		[AVOID_SaveValue("UseBlizzyToolbar")]
		protected VOID_SaveValue<bool> _UseToolbarManager;
		protected bool ToolbarManagerLoaded = false;
		internal ToolbarButtonWrapper ToolbarButton;

		/*
		 * Properties
		 * */
		public bool factoryReset
		{
			get
			{
				return this._factoryReset;
			}
		}

		public List<IVOID_Module> Modules
		{
			get
			{
				return this._modules;
			}
		}

		public GUISkin Skin
		{
			get
			{
				if (!this.skinsLoaded || this._skinName == null)
				{
					return AssetBase.GetGUISkin(this.defaultSkin);
				}
				return this.skin_list[this._skinName];
			}
		}

		public int windowID
		{
			get
			{
				if (this._windowID == 0)
				{
				this._windowID = this.windowBaseID;
				}
				return this._windowID++;
			}
		}

		public Dictionary<string, GUIStyle> LabelStyles
		{
			get
			{
				return this._LabelStyles;
			}
		}

		public List<CelestialBody> allBodies
		{
			get
			{
				return this._allBodies;
			}
		}

		public List<VesselType> allVesselTypes
		{
			get
			{
				return this._allVesselTypes;
			}
		}

		public float updateTimer
		{
			get
			{
				return this._updateTimer;
			}
		}

		public double updatePeriod
		{
			get
			{
				return this._updatePeriod;
			}
		}

		protected bool UseToolbarManager
		{
			get
			{
				return _UseToolbarManager;
			}
			set
			{
				if (value == false && this.ToolbarManagerLoaded && this.ToolbarButton != null)
				{
					this.ToolbarButton.Destroy();
					this.ToolbarButton = null;
				}
				if (value == true && this.ToolbarManagerLoaded && this.ToolbarButton == null)
				{
					this.InitializeToolbarButton();
				}

				_UseToolbarManager.value = value;
			}
		}

		/*
		 * Methods
		 * */
		protected VOID_Core()
		{
			this._Name = "VOID Core";

			this._Active.value = true;

			this.VOIDIconOn = GameDatabase.Instance.GetTexture (this.VOIDIconOnPath, false);
			this.VOIDIconOff = GameDatabase.Instance.GetTexture (this.VOIDIconOffPath, false);

			this._skinName = this.defaultSkin;

			this.UseToolbarManager = false;

			this.LoadConfig ();
		}
		
		protected void LoadModulesOfType<T>()
		{
			var types = AssemblyLoader.loadedAssemblies
				.Select (a => a.assembly.GetExportedTypes ())
					.SelectMany (t => t)
					.Where (v => typeof(T).IsAssignableFrom (v)
					        && !(v.IsInterface || v.IsAbstract) &&
					        !typeof(VOID_Core).IsAssignableFrom (v)
					        );

			Tools.PostDebugMessage (string.Format (
				"{0}: Found {1} modules to check.",
				this.GetType ().Name,
				types.Count ()
				));
			foreach (var voidType in types)
			{
				if (!HighLogic.LoadedSceneIsEditor &&
				    typeof(IVOID_EditorModule).IsAssignableFrom(voidType)
				    )
				{
					continue;
				}

				Tools.PostDebugMessage (string.Format (
					"{0}: found Type {1}",
					this.GetType ().Name,
					voidType.Name
					));

				this.LoadModule(voidType);
			}

			this._modulesLoaded = true;

			Tools.PostDebugMessage(string.Format(
				"{0}: Loaded {1} modules.",
				this.GetType().Name,
				this.Modules.Count
				));
		}

		protected void LoadModule(Type T)
		{
			var existingModules = this._modules.Where (mod => mod.GetType ().Name == T.Name);
			if (existingModules.Any())
			{
				Tools.PostDebugMessage(string.Format(
					"{0}: refusing to load {1}: already loaded",
					this.GetType().Name,
					T.Name
					));
				return;
			}
			IVOID_Module module = Activator.CreateInstance (T) as IVOID_Module;
			module.LoadConfig();
			this._modules.Add (module);

			Tools.PostDebugMessage(string.Format(
				"{0}: loaded module {1}.",
				this.GetType().Name,
				T.Name
				));
		}

		protected void LoadSkins()
		{
			Tools.PostDebugMessage ("AssetBase has skins: \n" +
			                        string.Join("\n\t", AssetBase.FindObjectsOfTypeIncludingAssets (
				typeof(GUISkin))
			            .Select(s => s.ToString())
			            .ToArray()
			            )
			                        );

			this.skin_list = AssetBase.FindObjectsOfTypeIncludingAssets(typeof(GUISkin))
				.Where(s => !this.forbiddenSkins.Contains(s.name))
					.Select(s => s as GUISkin)
					.GroupBy(s => s.name)
					.Select(g => g.First())
					.ToDictionary(s => s.name);

			Tools.PostDebugMessage(string.Format(
				"{0}: loaded {1} GUISkins.",
				this.GetType().Name,
				this.skin_list.Count
				));

			this.skinNames = this.skin_list.Keys.ToList();
			this.skinNames.Sort();

			if (this._skinName == null || !this.skinNames.Contains(this._skinName))
			{
				this._skinName = this.defaultSkin;
				Tools.PostDebugMessage(string.Format(
					"{0}: resetting _skinIdx to default.",
					this.GetType().Name
					));
			}

			Tools.PostDebugMessage(string.Format(
				"{0}: _skinIdx = {1}.",
				this.GetType().Name,
				this._skinName.ToString()
				));

			this.skinsLoaded = true;
		}

		protected void LoadGUIStyles()
		{
			this.LabelStyles["link"] = new GUIStyle(GUI.skin.label);
			this.LabelStyles["link"].fontStyle = FontStyle.Bold;

			this.LabelStyles["center"] = new GUIStyle(GUI.skin.label);
			this.LabelStyles["center"].normal.textColor = Color.white;
			this.LabelStyles["center"].alignment = TextAnchor.UpperCenter;

			this.LabelStyles["center_bold"] = new GUIStyle(GUI.skin.label);
			this.LabelStyles["center_bold"].normal.textColor = Color.white;
			this.LabelStyles["center_bold"].alignment = TextAnchor.UpperCenter;
			this.LabelStyles["center_bold"].fontStyle = FontStyle.Bold;

			this.LabelStyles["right"] = new GUIStyle(GUI.skin.label);
			this.LabelStyles["right"].normal.textColor = Color.white;
			this.LabelStyles["right"].alignment = TextAnchor.UpperRight;

			this.LabelStyles ["red"] = new GUIStyle(GUI.skin.label);
			this.LabelStyles ["red"].normal.textColor = Color.red;
			this.LabelStyles ["red"].alignment = TextAnchor.MiddleCenter;

			this.GUIStylesLoaded = true;
		}

		protected void LoadAllBodies()
		{
			this._allBodies = FlightGlobals.Bodies;
			this.bodiesLoaded = true;
		}

		protected void LoadVesselTypes()
		{
			this._allVesselTypes = Enum.GetValues(typeof(VesselType)).OfType<VesselType>().ToList();
			this.vesselTypesLoaded = true;
		}

		protected void LoadBeforeUpdate()
		{
			if (!this.bodiesLoaded)
			{
				this.LoadAllBodies();
			}

			if (!this.vesselTypesLoaded)
			{
				this.LoadVesselTypes();
			}
		}

		protected void LoadToolbarManager()
		{
			Type ToolbarManager = AssemblyLoader.loadedAssemblies
				.Select(a => a.assembly.GetExportedTypes())
					.SelectMany(t => t)
					.FirstOrDefault(t => t.FullName == "Toolbar.ToolbarManager");

			if (ToolbarManager == null)
			{
				Tools.PostDebugMessage(string.Format(
					"{0}: Could not load ToolbarManager.",
					this.GetType().Name
				));

				return;
			}

			this.InitializeToolbarButton();

			this.ToolbarManagerLoaded = true;
		}

		protected void InitializeToolbarButton()
		{
			this.ToolbarButton = new ToolbarButtonWrapper(this.GetType().Name, "coreToggle");
			this.ToolbarButton.Text = this.VoidName;
			this.ToolbarButton.TexturePath = this.VOIDIconOffPath + "_24x24";
			this.ToolbarButton.AddButtonClickHandler(
				(e) => this.mainGuiMinimized = !this.mainGuiMinimized
			);
		}

		public void VOIDMainWindow(int _)
		{
			GUILayout.BeginVertical();
			
			if (this.powerAvailable || HighLogic.LoadedSceneIsEditor)
			{
				if (!HighLogic.LoadedSceneIsEditor)
				{
					string str = "ON";
					if (togglePower) str = "OFF";
					if (GUILayout.Button("Power " + str)) togglePower.value = !togglePower;
				}

				if (togglePower || HighLogic.LoadedSceneIsEditor)
			    {
					foreach (IVOID_Module module in this.Modules)
					{
						module.toggleActive = GUILayout.Toggle (module.toggleActive, module.Name);
					}
			    }
			}
			else
			{
			    GUILayout.Label("-- POWER LOST --", this.LabelStyles["red"]);
			}

			this.configWindowMinimized.value = !GUILayout.Toggle (!this.configWindowMinimized, "Configuration");

			GUILayout.EndVertical();
			GUI.DragWindow();
		}

		public void VOIDConfigWindow(int _)
		{
			GUILayout.BeginVertical ();

			this.DrawConfigurables ();

			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}

		public override void DrawConfigurables()
		{
			int skinIdx;

			GUIContent _content;

			if (HighLogic.LoadedSceneIsFlight)
			{
				this.consumeResource.value = GUILayout.Toggle (this.consumeResource, "Consume Resources");

				this.VOIDIconLocked = GUILayout.Toggle (this.VOIDIconLocked, "Lock Icon Position");
			}

			this.UseToolbarManager = GUILayout.Toggle(this.UseToolbarManager, "Use Blizzy's Toolbar If Available");

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

			GUILayout.Label("Skin:", GUILayout.ExpandWidth(false));

			_content = new GUIContent();

			if (skinNames.Contains(this._skinName))
			{
				skinIdx = skinNames.IndexOf(this._skinName);
			}
			else if (skinNames.Contains(this.defaultSkin))
			{
				skinIdx = skinNames.IndexOf(this.defaultSkin);
			}
			else
			{
				skinIdx = 0;
			}

			_content.text = "◄";
			_content.tooltip = "Select previous skin";
			if (GUILayout.Button(_content, GUILayout.ExpandWidth(true)))
			{
				skinIdx--;
				if (skinIdx < 0) skinIdx = skinNames.Count - 1;
				Tools.PostDebugMessage (string.Format (
					"{0}: new this._skinIdx = {1} :: skin_list.Count = {2}",
					this.GetType().Name,
					this._skinName,
					this.skin_list.Count
				));
			}

			_content.text = this.Skin.name;
			_content.tooltip = "Current skin";
			GUILayout.Label(_content, this.LabelStyles["center"], GUILayout.ExpandWidth(true));

			_content.text = "►";
			_content.tooltip = "Select next skin";
			if (GUILayout.Button(_content, GUILayout.ExpandWidth(true)))
			{
				skinIdx++;
				if (skinIdx >= skinNames.Count) skinIdx = 0;
				Tools.PostDebugMessage (string.Format (
					"{0}: new this._skinIdx = {1} :: skin_list.Count = {2}",
					this.GetType().Name,
					this._skinName,
					this.skin_list.Count
					));
			}

			if (this._skinName != skinNames[skinIdx])
			{
				this._skinName = skinNames[skinIdx];
			}

			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Update Rate (Hz):");
			if (this.stringFrequency == null)
			{
				this.stringFrequency = (1f / this.updatePeriod).ToString();
			}
			this.stringFrequency = GUILayout.TextField(this.stringFrequency.ToString(), 5, GUILayout.ExpandWidth(true));
			// GUILayout.FlexibleSpace();
			if (GUILayout.Button("Apply"))
			{
				double updateFreq = 1f / this.updatePeriod;
				double.TryParse(stringFrequency, out updateFreq);
				this._updatePeriod = 1 / updateFreq;
			}
			GUILayout.EndHorizontal();

			foreach (IVOID_Module mod in this.Modules)
			{
				mod.DrawConfigurables ();
			}

			this._factoryReset = GUILayout.Toggle (this._factoryReset, "Factory Reset");
		}

		public override void DrawGUI()
		{
			this._windowID = this.windowBaseID;

			if (!this._modulesLoaded)
			{
				this.LoadModulesOfType<IVOID_Module> ();
			}

			if (this.UseToolbarManager && !this.ToolbarManagerLoaded)
			{
				this.LoadToolbarManager();
			}

			if (!this.skinsLoaded)
			{
				this.LoadSkins();
			}

			GUI.skin = this.Skin;

			if (!this.GUIStylesLoaded)
			{
				this.LoadGUIStyles ();
			}

			if (this.UseToolbarManager && this.ToolbarManagerLoaded)
			{
				this.ToolbarButton.TexturePath = VOIDIconOffPath + "_24x24";
				if (this.togglePower)
				{
					this.ToolbarButton.TexturePath = VOIDIconOnPath + "_24x24";
				}
			}
			else
			{
				this.VOIDIconTexture = this.VOIDIconOff;  //icon off default
				if (this.togglePower)
					this.VOIDIconTexture = this.VOIDIconOn;     //or on if power_toggle==true
				if (GUI.Button(VOIDIconPos, VOIDIconTexture, new GUIStyle()) && this.VOIDIconLocked)
				{
					this.mainGuiMinimized.value = !this.mainGuiMinimized;
				}
			}

			if (!this.mainGuiMinimized)
			{
				Rect _mainWindowPos = this.mainWindowPos;

				_mainWindowPos = GUILayout.Window (
					this.windowID,
					_mainWindowPos,
					this.VOIDMainWindow,
					string.Join (" ", new string[] {this.VoidName, this.VoidVersion}),
					GUILayout.Width (250),
					GUILayout.Height (50)
					);

				_mainWindowPos = Tools.ClampRectToScreen (_mainWindowPos);

				if (_mainWindowPos != this.mainWindowPos)
				{
					this.mainWindowPos = _mainWindowPos;
				}
			}

			if (!this.configWindowMinimized && !this.mainGuiMinimized)
			{
				Rect _configWindowPos = this.configWindowPos;

				_configWindowPos = GUILayout.Window (
					this.windowID,
					_configWindowPos,
					this.VOIDConfigWindow,
					string.Join (" ", new string[] {this.VoidName, "Configuration"}),
					GUILayout.Width (250),
					GUILayout.Height (50)
					);

				_configWindowPos = Tools.ClampRectToScreen (_configWindowPos);

				if (_configWindowPos != this.configWindowPos)
				{
					this.configWindowPos = _configWindowPos;
				}
			}
		}

		public void OnGUI()
		{
			if (Event.current.type == EventType.Repaint)
			{
				return;
			}

			/*
			Tools.PostDebugMessage(string.Format(
				"Event.current.type: {0}" +
				"\nthis.VOIDIconLocked: {1}" +
				"\nEvent.current.mousePosition: {2}" +
				"\nVOIDIconPos: ({3}, {4}),({5}, {6})",
				Event.current.type,
				this.VOIDIconLocked,
				Event.current.mousePosition,
				this.VOIDIconPos.value.xMin,
				this.VOIDIconPos.value.yMin,
				this.VOIDIconPos.value.xMax,
				this.VOIDIconPos.value.yMax
				));
				*/

			if (!this.VOIDIconLocked &&
			    VOIDIconPos.value.Contains(Event.current.mousePosition)
			    && Event.current.type == EventType.mouseDrag
			    )
			{
				Tools.PostDebugMessage(string.Format(
					"Event.current.type: {0}" +
					"\ndelta.x: {1}; delta.y: {2}",
					Event.current.type,
					Event.current.delta.x,
					Event.current.delta.y
				));

				Rect tmp = new Rect(VOIDIconPos);

				tmp.x = Event.current.mousePosition.x - tmp.width / 2;
				tmp.y = Event.current.mousePosition.y - tmp.height / 2;

				if (tmp.x > Screen.width - tmp.width)
				{
					tmp.x = Screen.width - tmp.width;
				}

				if (tmp.y > Screen.height - tmp.height)
				{
					tmp.y = Screen.height - tmp.height;
				}

				VOIDIconPos = tmp;
			}
		}

		public void Update()
		{
			this.LoadBeforeUpdate ();

			if (this.vessel != null)
			{
				SimManager.Instance.Gravity = VOID_Core.Instance.vessel.mainBody.gravParameter /
					Math.Pow(VOID_Core.Instance.vessel.mainBody.Radius, 2);
				SimManager.Instance.TryStartSimulation();
			}

			if (!this.guiRunning)
			{
				this.StartGUI ();
			}

			if (!HighLogic.LoadedSceneIsFlight && this.guiRunning)
			{
				this.StopGUI ();
			}

			foreach (IVOID_Module module in this.Modules)
			{
				if (!module.guiRunning && module.toggleActive)
				{
					module.StartGUI ();
				}
				if (module.guiRunning && !module.toggleActive ||
				    !this.togglePower ||
				    !HighLogic.LoadedSceneIsFlight ||
				    this.factoryReset
				    )
				{
					module.StopGUI();
				}

				if (module is IVOID_BehaviorModule)
				{
					((IVOID_BehaviorModule)module).Update();
				}
			}

			this.CheckAndSave ();
			this._updateTimer += Time.deltaTime;
		}

		public void FixedUpdate()
		{
			if (this.consumeResource &&
			    this.vessel.vesselType != VesselType.EVA &&
			    TimeWarp.deltaTime != 0
			    )
			{
				float powerReceived = this.vessel.rootPart.RequestResource(this.resourceName,
				                                                           this.resourceRate * TimeWarp.fixedDeltaTime);
				if (powerReceived > 0)
				{
					this.powerAvailable = true;
				}
				else
				{
					this.powerAvailable = false;
				}
			}

			foreach (IVOID_BehaviorModule module in
			         this._modules.OfType<IVOID_BehaviorModule>().Where(m => !m.GetType().IsAbstract))
			{
				module.FixedUpdate();
			}
		}

		public void ResetGUI()
		{
			this.StopGUI ();

			foreach (IVOID_Module module in this.Modules)
			{
				module.StopGUI ();
				module.StartGUI ();
			}

			this.StartGUI ();
		}

		protected void CheckAndSave()
		{
			this.saveTimer += Time.deltaTime;

			if (this.saveTimer > 2f)
			{
				if (!this.configDirty)
				{
					return;
				}

				Tools.PostDebugMessage (string.Format (
					"{0}: Time to save, checking if configDirty: {1}",
					this.GetType ().Name,
					this.configDirty
					));

				this.SaveConfig ();
				this.saveTimer = 0;
			}
		}

		public override void LoadConfig()
		{
			base.LoadConfig ();

			foreach (IVOID_Module module in this.Modules)
			{
				module.LoadConfig ();
			}
		}

		public void SaveConfig()
		{
			var config = KSP.IO.PluginConfiguration.CreateForType<VOID_Core> ();
			config.load ();

			this._SaveToConfig(config);

			foreach (IVOID_Module module in this.Modules)
			{
				module._SaveToConfig (config);
			}

			config.save();

			this.configDirty = false;
		}
	}
}

