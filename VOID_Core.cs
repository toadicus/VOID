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
using System.Reflection;
using KSP;
using UnityEngine;

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
		protected string VoidVersion = "0.9.9";

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

		// Celestial Body Housekeeping
		protected List<CelestialBody> _allBodies = new List<CelestialBody>();
		protected bool bodiesLoaded = false;

		// Vessel Type Housekeeping
		protected List<VesselType> _allVesselTypes = new List<VesselType>();
		protected bool vesselTypesLoaded = false;

		public float saveTimer = 0;

		protected string defaultSkin = "KSP window 2";
		protected VOID_SaveValue<int> _skinIdx = int.MinValue;
		protected List<GUISkin> skin_list;
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
				if (this.skin_list == null || this._skinIdx < 0 || this._skinIdx > this.skin_list.Count)
				{
					return AssetBase.GetGUISkin(this.defaultSkin);
				}
				return this.skin_list[this._skinIdx];
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

		/*
		 * Methods
		 * */
		protected VOID_Core()
		{
			this._Name = "VOID Core";

			this._Active = true;

			this.VOIDIconOn = GameDatabase.Instance.GetTexture (this.VOIDIconOnPath, false);
			this.VOIDIconOff = GameDatabase.Instance.GetTexture (this.VOIDIconOffPath, false);

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

		public void Update()
		{
			this.saveTimer += Time.deltaTime;

			if (!this.bodiesLoaded)
			{
				this.LoadAllBodies();
			}

			if (!this.vesselTypesLoaded)
			{
				this.LoadVesselTypes();
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
				    !HighLogic.LoadedSceneIsFlight
				    || this.factoryReset
				    )
				{
					module.StopGUI();
				}

				if (module is IVOID_BehaviorModule)
				{
					((IVOID_BehaviorModule)module).Update();
				}
			}

			if (this.saveTimer > 2f)
			{
				this.SaveConfig ();
				this.saveTimer = 0;
			}
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

		protected void LoadSkins()
		{
			this.skin_list = AssetBase.FindObjectsOfTypeIncludingAssets(typeof(GUISkin))
				.Where(s => !this.forbiddenSkins.Contains(s.name))
					.Select(s => s as GUISkin)
					.ToList();

			Tools.PostDebugMessage(string.Format(
				"{0}: loaded {1} GUISkins.",
				this.GetType().Name,
				this.skin_list.Count
			));

			if (this._skinIdx == int.MinValue)
			{
				this._skinIdx = this.skin_list.IndexOf(this.Skin);
				Tools.PostDebugMessage(string.Format(
					"{0}: resetting _skinIdx to default.",
					this.GetType().Name
					));
			}

			Tools.PostDebugMessage(string.Format(
				"{0}: _skinIdx = {1}.",
				this.GetType().Name,
				this._skinIdx.ToString()
				));

			this.skinsLoaded = true;
		}

		protected void LoadGUIStyles()
		{
			this.LabelStyles["center"] = new GUIStyle(GUI.skin.label);
			this.LabelStyles["center"].normal.textColor = Color.white;
			this.LabelStyles["center"].alignment = TextAnchor.UpperCenter;

			this.LabelStyles["center_bold"] = new GUIStyle(GUI.skin.label);
			this.LabelStyles["center_bold"].normal.textColor = Color.white;
			this.LabelStyles["center_bold"].alignment = TextAnchor.UpperCenter;
			this.LabelStyles["center_bold"].fontStyle = FontStyle.Bold;

			this.LabelStyles["txt_right"] = new GUIStyle(GUI.skin.label);
			this.LabelStyles["txt_right"].normal.textColor = Color.white;
			this.LabelStyles["txt_right"].alignment = TextAnchor.UpperRight;

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

		public void VOIDMainWindow(int _)
		{
			GUILayout.BeginVertical();
			
			if (this.powerAvailable)
			{
				string str = "ON";
				if (togglePower) str = "OFF";
				if (GUILayout.Button("Power " + str)) togglePower = !togglePower;
			    if (togglePower)
			    {
					foreach (IVOID_Module module in this.Modules)
					{
						module.toggleActive = GUILayout.Toggle (module.toggleActive, module.Name);
					}
			    }
			}
			else
			{
			    GUIStyle label_txt_red = new GUIStyle(GUI.skin.label);
			    label_txt_red.normal.textColor = Color.red;
			    label_txt_red.alignment = TextAnchor.MiddleCenter;
			    GUILayout.Label("-- POWER LOST --", label_txt_red);
			}

			this.configWindowMinimized = !GUILayout.Toggle (!this.configWindowMinimized, "Configuration");

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
			this.consumeResource = GUILayout.Toggle (this.consumeResource, "Consume Resources");

			this.VOIDIconLocked = GUILayout.Toggle (this.VOIDIconLocked, "Lock Icon Position");

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

			GUILayout.Label("Skin:", GUILayout.ExpandWidth(false));

			GUIContent _content = new GUIContent();

			_content.text = "◄";
			_content.tooltip = "Select previous skin";
			if (GUILayout.Button(_content, GUILayout.ExpandWidth(true)))
			{
				this._skinIdx--;
				if (this._skinIdx < 0) this._skinIdx = skin_list.Count - 1;
				Tools.PostDebugMessage("[VOID] new this._skin = " + this._skinIdx + " :: skin_list.Count = " + skin_list.Count);
			}

			string skin_name = skin_list[this._skinIdx].name;
			_content.text = skin_name;
			_content.tooltip = "Current skin";
			GUILayout.Label(_content, this.LabelStyles["center"], GUILayout.ExpandWidth(true));

			_content.text = "►";
			_content.tooltip = "Select next skin";
			if (GUILayout.Button(_content, GUILayout.ExpandWidth(true)))
			{
				this._skinIdx++;
				if (this._skinIdx >= skin_list.Count) this._skinIdx = 0;
				Tools.PostDebugMessage("[VOID] new this._skin = " + this._skinIdx + " :: skin_list.Count = " + skin_list.Count);
			}

			GUILayout.EndHorizontal();

			foreach (IVOID_Module mod in this.Modules)
			{
				mod.DrawConfigurables ();
			}

			this._factoryReset = GUILayout.Toggle (this._factoryReset, "Factory Reset");
		}

		public void OnGUI()
		{
			if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint)
			{
				return;
			}

			Tools.PostDebugMessage(string.Format(
				"Event.current.type: {0}" +
				"this.VOIDIconLocked: {1}" +
				"Event.current.mousePosition: {2}",
				Event.current.type,
				this.VOIDIconLocked,
				Event.current.mousePosition
				));

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

		public override void DrawGUI()
		{
			if (!this._modulesLoaded)
			{
				this.LoadModulesOfType<IVOID_Module> ();
			}

			this._windowID = this.windowBaseID;

			if (!this.skinsLoaded)
			{
				this.LoadSkins();
			}

			GUI.skin = this.Skin;

			if (!this.GUIStylesLoaded)
			{
				this.LoadGUIStyles ();
			}

			this.VOIDIconTexture = this.VOIDIconOff;  //icon off default
			if (this.togglePower) this.VOIDIconTexture = this.VOIDIconOn;     //or on if power_toggle==true
			if (GUI.Button(VOIDIconPos, VOIDIconTexture, new GUIStyle()) && this.VOIDIconLocked)
			{
				this.mainGuiMinimized = !this.mainGuiMinimized;
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

				if (_mainWindowPos != this.mainWindowPos)
				{
					this.mainWindowPos = _mainWindowPos;
				}
			}

			if (!this.configWindowMinimized)
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

				if (_configWindowPos != this.configWindowPos)
				{
					this.configWindowPos = _configWindowPos;
				}
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
			if (!this.configDirty)
			{
				return;
			}

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

