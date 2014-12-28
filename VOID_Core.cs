// VOID
//
// VOID_Core.cs
//
// Copyright © 2014, toadicus
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice,
//    this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation and/or other
//    materials provided with the distribution.
//
// 3. Neither the name of the copyright holder nor the names of its contributors may be used
//    to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using KerbalEngineer.VesselSimulator;
using KSP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	public class VOID_Core : VOID_Module, IVOID_Module
	{
		#region Singleton Members
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
			_instance.Dispose();
			_instance = null;
			_initialized = false;
		}
		#endregion

		public static readonly double Constant_G = 6.674e-11;

		/*
		 * Fields
		 * */
		protected string VoidName = "VOID";
		protected string VoidVersion;

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
		protected VOID_SaveValue<Rect> VOIDIconPos = new Rect(Screen.width / 2 - 200, Screen.height - 32, 32f, 32f);

		protected Texture2D VOIDIconTexture;
		protected string VOIDIconOnActivePath;
		protected string VOIDIconOnInactivePath;
		protected string VOIDIconOffActivePath;
		protected string VOIDIconOffInactivePath;

		protected bool VOIDIconLocked = true;

		protected GUIStyle iconStyle;

		protected int windowBaseID = -96518722;
		protected int _windowID = 0;

		protected bool GUIStylesLoaded = false;

		protected CelestialBody _homeBody;

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
		protected VOID_SaveValue<double> _updatePeriod = 1001f / 15000f;
		protected float _updateTimer = 0f;
		protected string stringFrequency;

		[AVOID_SaveValue("vesselSimActive")]
		protected VOID_SaveValue<bool> vesselSimActive;

		// Vessel Type Housekeeping
		protected List<VesselType> _allVesselTypes = new List<VesselType>();
		protected bool vesselTypesLoaded = false;
		public float saveTimer = 0;

		protected string defaultSkin = "KSP window 2";

		[AVOID_SaveValue("defaultSkin")]
		protected VOID_SaveValue<string> _skinName;
		protected int _skinIdx;

		protected Dictionary<string, GUISkin> validSkins;
		protected string[] skinNames;
		protected string[] forbiddenSkins =
			{
				"PlaqueDialogSkin",
				"FlagBrowserSkin",
				"SSUITextAreaDefault",
				"ExperimentsDialogSkin",
				"ExpRecoveryDialogSkin",
				"KSP window 5",
				"KSP window 6",
				"PartTooltipSkin",
				"KSCContextMenuSkin"
			};
		protected bool skinsLoaded = false;

		public bool configDirty;

		[AVOID_SaveValue("UseBlizzyToolbar")]
		protected VOID_SaveValue<bool> _UseToolbarManager;
		internal IButton ToolbarButton;

		internal ApplicationLauncherButton AppLauncherButton;

		/*
		 * Events
		 * */
		public delegate void VOIDEventHandler(object sender);
		public event VOIDEventHandler onApplicationQuit;

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
				if (this.skinsLoaded)
				{
					try
					{
						return this.validSkins[this._skinName];
					}
					catch
					{
					}
				}

				return AssetBase.GetGUISkin(this.defaultSkin);
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

		public List<CelestialBody> allBodies
		{
			get
			{
				return FlightGlobals.Bodies;
			}
		}

		public List<CelestialBody> sortedBodyList
		{
			get;
			private set;
		}

		public CelestialBody HomeBody
		{
			get
			{
				if (this._homeBody == null)
				{
					if (Planetarium.fetch != null)
					{
						this._homeBody = Planetarium.fetch.Home;
					}
				}

				return this._homeBody;
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

		public Stage[] Stages
		{
			get;
			protected set;
		}

		public Stage LastStage
		{
			get;
			protected set;
		}

		protected IconState powerState
		{
			get
			{
				if (this.togglePower && this.powerAvailable)
				{
					return IconState.PowerOn;
				}
				else
				{
					return IconState.PowerOff;
				}

			}
		}

		protected IconState activeState
		{
			get
			{
				if (this.mainGuiMinimized)
				{
					return IconState.Inactive;
				}
				else
				{
					return IconState.Active;
				}

			}
		}

		protected bool UseToolbarManager
		{
			get
			{
				return _UseToolbarManager & ToolbarManager.ToolbarAvailable;
			}
			set
			{
				if (this._UseToolbarManager == value)
				{
					return;
				}

				if (value == false && this.ToolbarButton != null)
				{
					this.ToolbarButton.Destroy();
					this.ToolbarButton = null;
				}
				if (value == true)
				{
					if (this.AppLauncherButton != null)
					{
						ApplicationLauncher.Instance.RemoveModApplication(this.AppLauncherButton);
						this.AppLauncherButton = null;
					}

					this.InitializeToolbarButton();
				}

				_UseToolbarManager.value = value;
			}
		}

		protected virtual ApplicationLauncher.AppScenes appIconVisibleScenes
		{
			get
			{
				return ApplicationLauncher.AppScenes.FLIGHT;
			}
		}

		/*
		 * Methods
		 * */
		public override void DrawGUI()
		{
			this._windowID = this.windowBaseID;

			if (!this._modulesLoaded)
			{
				this.LoadModulesOfType<IVOID_Module>();
			}

			if (!this.skinsLoaded)
			{
				this.LoadSkins();
			}

			GUI.skin = this.Skin;

			if (!this.GUIStylesLoaded)
			{
				this.LoadGUIStyles();

				Tools.PostDebugMessage(
					this,
					"ToolbarAvailable: {0}, UseToobarManager: {1}",
					ToolbarManager.ToolbarAvailable,
					this.UseToolbarManager);
			}

			if (!this.UseToolbarManager)
			{
				if (this.AppLauncherButton == null)
				{
					Tools.PostDebugMessage(this,
						"UseToolbarManager = false (ToolbarAvailable = {0}) and " +
						"AppLauncherButton is null, making AppLauncher button.",
						ToolbarManager.ToolbarAvailable
					);
					this.InitializeAppLauncherButton();
				}
			}
			else if (this.ToolbarButton == null)
			{
				Tools.PostDebugMessage(this,
					"UseToolbarManager = true (ToolbarAvailable = {0}) and " +
					"ToolbarButton is null, making Toolbar button.",
					ToolbarManager.ToolbarAvailable
				);
				this.InitializeToolbarButton();
			}

			if (!this.mainGuiMinimized)
			{

				Rect _mainWindowPos = this.mainWindowPos;

				_mainWindowPos = GUILayout.Window(
					this.windowID,
					_mainWindowPos,
					VOID_Tools.GetWindowHandler(this.VOIDMainWindow),
					string.Join(" ", new string[] { this.VoidName, this.VoidVersion }),
					GUILayout.Width(250),
					GUILayout.Height(50)
				);

				if (HighLogic.LoadedSceneIsEditor)
				{
					_mainWindowPos = Tools.ClampRectToEditorPad(_mainWindowPos);
				}
				else
				{
					_mainWindowPos = Tools.ClampRectToScreen(_mainWindowPos);
				}

				if (_mainWindowPos != this.mainWindowPos)
				{
					this.mainWindowPos = _mainWindowPos;
				}
			}

			if (!this.configWindowMinimized && !this.mainGuiMinimized)
			{
				Rect _configWindowPos = this.configWindowPos;

				_configWindowPos = GUILayout.Window(
					this.windowID,
					_configWindowPos,
					VOID_Tools.GetWindowHandler(this.VOIDConfigWindow),
					string.Join(" ", new string[] { this.VoidName, "Configuration" }),
					GUILayout.Width(250),
					GUILayout.Height(50)
				);

				if (HighLogic.LoadedSceneIsEditor)
				{
					_configWindowPos = Tools.ClampRectToEditorPad(_configWindowPos);
				}
				else
				{
					_configWindowPos = Tools.ClampRectToScreen(_configWindowPos);
				}

				if (_configWindowPos != this.configWindowPos)
				{
					this.configWindowPos = _configWindowPos;
				}
			}
		}

		public virtual void OnGUI()
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
			    && Event.current.type == EventType.mouseDrag)
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

		public virtual void Update()
		{
			this.LoadBeforeUpdate();

			if (this.vessel != null && this.vesselSimActive)
			{
				Tools.PostDebugMessage(this, "Updating SimManager.");
				this.UpdateSimManager();
			}

			if (!this.guiRunning)
			{
				this.StartGUI();
			}

			if (!HighLogic.LoadedSceneIsFlight && this.guiRunning)
			{
				this.StopGUI();
			}

			foreach (IVOID_Module module in this.Modules)
			{
				if (!module.guiRunning && module.toggleActive)
				{
					module.StartGUI();
				}
				if (module.guiRunning && !module.toggleActive ||
				    !this.togglePower ||
				    !HighLogic.LoadedSceneIsFlight ||
				    this.factoryReset)
				{
					module.StopGUI();
				}

				if (module is IVOID_BehaviorModule)
				{
					((IVOID_BehaviorModule)module).Update();
				}
			}

			this.CheckAndSave();
			this._updateTimer += Time.deltaTime;
		}

		public virtual void FixedUpdate()
		{
			bool newPowerState = this.powerAvailable;

			if (this.togglePower && this.consumeResource &&
			    this.vessel.vesselType != VesselType.EVA &&
			    TimeWarp.deltaTime != 0)
			{
				float powerReceived = this.vessel.rootPart.RequestResource(
					this.resourceName,
					this.resourceRate * TimeWarp.fixedDeltaTime
				);

				if (powerReceived > 0)
				{
					newPowerState = true;
				}
				else
				{
					newPowerState = false;
				}

				if (this.powerAvailable != newPowerState)
				{
					this.powerAvailable = newPowerState;
					this.SetIconTexture(this.powerState | this.activeState);
				}
			}

			foreach (IVOID_Module module in this.Modules)
			{
				if (module is IVOID_BehaviorModule)
				{
					((IVOID_BehaviorModule)module).FixedUpdate();
				}
			}
		}

		public void OnDestroy()
		{
			foreach (IVOID_Module module in this.Modules)
			{
				if (module is IVOID_BehaviorModule)
				{
					((IVOID_BehaviorModule)module).OnDestroy();
				}
			}
		}

		public void OnApplicationQuit()
		{
			this.onApplicationQuit(this);
		}

		public void ResetGUI()
		{
			this.StopGUI();

			foreach (IVOID_Module module in this.Modules)
			{
				module.StopGUI();
				module.StartGUI();
			}

			this.StartGUI();
		}

		public void VOIDMainWindow(int _)
		{
			GUILayout.BeginVertical();

			if (this.powerAvailable || HighLogic.LoadedSceneIsEditor)
			{
				if (!HighLogic.LoadedSceneIsEditor)
				{
					string str = string.Intern("ON");
					if (togglePower)
						str = string.Intern("OFF");
					if (GUILayout.Button("Power " + str))
					{
						togglePower.value = !togglePower;
						this.SetIconTexture(this.powerState | this.activeState);
					}
				}

				if (togglePower || HighLogic.LoadedSceneIsEditor)
				{
					foreach (IVOID_Module module in this.Modules)
					{
						module.toggleActive = GUILayout.Toggle(module.toggleActive, module.Name);
					}
				}
			}
			else
			{
				GUILayout.Label("-- POWER LOST --", VOID_Styles.labelRed);
			}

			this.configWindowMinimized.value = !GUILayout.Toggle(!this.configWindowMinimized, "Configuration");

			GUILayout.EndVertical();
			GUI.DragWindow();
		}

		public void VOIDConfigWindow(int _)
		{
			GUILayout.BeginVertical();

			this.DrawConfigurables();

			GUILayout.EndVertical();
			GUI.DragWindow();
		}

		public override void DrawConfigurables()
		{
			GUIContent _content;

			if (HighLogic.LoadedSceneIsFlight)
			{
				this.consumeResource.value = GUILayout.Toggle(this.consumeResource, "Consume Resources");

				this.VOIDIconLocked = GUILayout.Toggle(this.VOIDIconLocked, "Lock Icon Position");
			}

			this.UseToolbarManager = GUILayout.Toggle(this.UseToolbarManager, "Use Blizzy's Toolbar If Available");

			this.vesselSimActive.value = GUILayout.Toggle(this.vesselSimActive.value,
				"Enable Engineering Calculations");

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

			GUILayout.Label("Skin:", GUILayout.ExpandWidth(false));

			_content = new GUIContent();

			_content.text = "◄";
			_content.tooltip = "Select previous skin";
			if (GUILayout.Button(_content, GUILayout.ExpandWidth(true)))
			{
				this._skinIdx--;
				Tools.PostDebugMessage(string.Format(
					"{0}: new this._skinIdx = {1} :: skin_list.Count = {2}",
					this.GetType().Name,
					this._skinName,
					this.validSkins.Count
				));
			}

			_content.text = this.Skin.name;
			_content.tooltip = "Current skin";
			GUILayout.Label(_content, VOID_Styles.labelCenter, GUILayout.ExpandWidth(true));

			_content.text = "►";
			_content.tooltip = "Select next skin";
			if (GUILayout.Button(_content, GUILayout.ExpandWidth(true)))
			{
				this._skinIdx++;
				Tools.PostDebugMessage(string.Format(
					"{0}: new this._skinIdx = {1} :: skin_list.Count = {2}",
					this.GetType().Name,
					this._skinName,
					this.validSkins.Count
				));
			}

			this._skinIdx %= this.skinNames.Length;
			if (this._skinIdx < 0)
			{
				this._skinIdx += this.skinNames.Length;
			}

			if (this._skinName != skinNames[this._skinIdx])
			{
				this._skinName.value = skinNames[this._skinIdx];
				this.GUIStylesLoaded = false;
			}

			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Update Rate (Hz):");
			if (this.stringFrequency == null)
			{
				this.stringFrequency = (1f / this.updatePeriod).ToString();
			}
			this.stringFrequency = GUILayout.TextField(this.stringFrequency.ToString(), 5, GUILayout.ExpandWidth(true));

			if (GUILayout.Button("Apply"))
			{
				double updateFreq = 1f / this.updatePeriod;
				double.TryParse(stringFrequency, out updateFreq);
				this._updatePeriod = 1 / updateFreq;
			}
			GUILayout.EndHorizontal();

			foreach (IVOID_Module mod in this.Modules)
			{
				mod.DrawConfigurables();
			}

			this._factoryReset = GUILayout.Toggle(this._factoryReset, "Factory Reset");
		}

		protected void UpdateSimManager()
		{
			if (SimManager.ResultsReady())
			{
				Tools.PostDebugMessage(this, "VesselSimulator results ready, setting Stages.");

				this.Stages = SimManager.Stages;

				if (this.Stages != null)
				{
					this.LastStage = this.Stages.Last();
				}

				if (HighLogic.LoadedSceneIsEditor)
				{
					SimManager.Gravity = VOID_Data.KerbinGee;
				}
				else
				{
					double radius = this.vessel.Radius();
					SimManager.Gravity = this.vessel.mainBody.gravParameter / (radius * radius);
				}

				SimManager.minSimTime = new TimeSpan(0, 0, 0, 0, (int)(this.updatePeriod * 1000d));

				SimManager.TryStartSimulation();
			}
			#if DEBUG
			else
			{
				Tools.PostDebugMessage(this, "VesselSimulator results not ready.");
			}
			#endif
		}

		protected void LoadModulesOfType<T>()
		{
			StringBuilder sb = new StringBuilder("Loading modules...");
			sb.AppendLine();

			foreach (AssemblyLoader.LoadedAssembly assy in AssemblyLoader.loadedAssemblies)
			{
				foreach (Type loadedType in assy.assembly.GetExportedTypes())
				{
					if (
						loadedType.IsInterface ||
						loadedType.IsAbstract ||
						!typeof(T).IsAssignableFrom(loadedType) ||
						this.GetType().IsAssignableFrom(loadedType)
					)
					{
						continue;
					}

					// HACK: This stops editor modules from loading in flight.  It is a dirty hack and should be fixed.
					if (!HighLogic.LoadedSceneIsEditor && typeof(IVOID_EditorModule).IsAssignableFrom(loadedType))
					{
						continue;
					}

					sb.AppendFormat("Loading IVOID_Module type {0}...", loadedType.Name);

					try
					{
						this.LoadModule(loadedType);
						sb.AppendLine("Success.");
					}
					catch (Exception ex)
					{
						sb.AppendFormat("Failed, caught {0}", ex.GetType().Name);
						sb.AppendLine();

						#if DEBUG
						Debug.LogException(ex);
						#endif
					}
				}
			}

			this._modulesLoaded = true;

			sb.AppendFormat("Loaded {0} modules.", this.Modules.Count);
			sb.AppendLine();
		}

		protected void LoadModule(Type T)
		{
			var existingModules = this._modules.Where(mod => mod.GetType().Name == T.Name);
			if (existingModules.Any())
			{
				Tools.PostDebugMessage(string.Format(
					"{0}: refusing to load {1}: already loaded",
					this.GetType().Name,
					T.Name
				));
				return;
			}
			IVOID_Module module = Activator.CreateInstance(T) as IVOID_Module;
			module.LoadConfig();
			this._modules.Add(module);

			Tools.PostDebugMessage(string.Format(
				"{0}: loaded module {1}.",
				this.GetType().Name,
				T.Name
			));
		}

		protected void LoadSkins()
		{
			Tools.PostDebugMessage("AssetBase has skins: \n" +
				string.Join("\n\t",
					Resources.FindObjectsOfTypeAll(typeof(GUISkin))
					.Select(s => s.ToString())
					.ToArray()
				)
			);

			this.validSkins = Resources.FindObjectsOfTypeAll(typeof(GUISkin))
				.Where(s => !this.forbiddenSkins.Contains(s.name))
				.Select(s => s as GUISkin)
				.GroupBy(s => s.name)
				.Select(g => g.First())
				.ToDictionary(s => s.name);

			Tools.PostDebugMessage(string.Format(
				"{0}: loaded {1} GUISkins.",
				this.GetType().Name,
				this.validSkins.Count
			));

			this.skinNames = this.validSkins.Keys.ToArray();
			Array.Sort(this.skinNames);

			int defaultIdx = int.MinValue;

			for (int i = 0; i < this.skinNames.Length; i++)
			{
				if (this.skinNames[i] == this._skinName)
				{
					this._skinIdx = i;
				}
				if (this.skinNames[i] == this.defaultSkin)
				{
					defaultIdx = i;
				}
				if (this._skinIdx != int.MinValue && defaultIdx != int.MinValue)
				{
					break;
				}
			}

			if (this._skinIdx == int.MinValue)
			{
				this._skinIdx = defaultIdx;
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
			VOID_Styles.OnSkinChanged();

			this.GUIStylesLoaded = true;
		}

		protected void LoadVesselTypes()
		{
			this._allVesselTypes = Enum.GetValues(typeof(VesselType)).OfType<VesselType>().ToList();
			this.vesselTypesLoaded = true;
		}

		protected void LoadBeforeUpdate()
		{
			if (!this.vesselTypesLoaded)
			{
				this.LoadVesselTypes();
			}

			if (this.sortedBodyList == null && FlightGlobals.Bodies != null && FlightGlobals.Bodies.Count > 0)
			{
				this.sortedBodyList = new List<CelestialBody>(FlightGlobals.Bodies);
				this.sortedBodyList.Sort(new CBListComparer());
				this.sortedBodyList.Reverse();

				Debug.Log(string.Format("sortedBodyList: {0}", string.Join("\n\t", this.sortedBodyList.Select(b => b.bodyName).ToArray())));
			}

		}

		protected void InitializeToolbarButton()
		{
			// Do nothing if (the Toolbar is not available.
			if (!ToolbarManager.ToolbarAvailable)
			{
				Tools.PostDebugMessage(this, "Refusing to make a ToolbarButton: ToolbarAvailable = false");
				return;
			}

			this.ToolbarButton = ToolbarManager.Instance.add(this.VoidName, "coreToggle");
			this.ToolbarButton.Text = this.VoidName;
			this.SetIconTexture(this.powerState | this.activeState);

			this.ToolbarButton.Visibility = new GameScenesVisibility(GameScenes.EDITOR, GameScenes.FLIGHT);

			this.ToolbarButton.OnClick += 
				(e) =>
			{
				this.ToggleMainWindow();
			};

			Tools.PostDebugMessage(string.Format("{0}: Toolbar Button initialized.", this.GetType().Name));
		}

		protected void InitializeAppLauncherButton()
		{
			if (ApplicationLauncher.Ready)
			{
				this.AppLauncherButton = ApplicationLauncher.Instance.AddModApplication(
					this.ToggleMainWindow, this.ToggleMainWindow,
					this.appIconVisibleScenes,
					this.VOIDIconTexture
				);

				Tools.PostDebugMessage(
					this,
					"AppLauncherButton initialized in {0}",
					Enum.GetName(
						typeof(GameScenes),
						HighLogic.LoadedScene
					)
				);
			}
		}

		protected void ToggleMainWindow()
		{
			this.mainGuiMinimized = !this.mainGuiMinimized;
			this.SetIconTexture(this.powerState | this.activeState);
		}

		protected void SetIconTexture(IconState state)
		{
			switch (state)
			{
				case (IconState.PowerOff | IconState.Inactive):
					this.SetIconTexture(this.VOIDIconOffInactivePath);
					break;
				case (IconState.PowerOff | IconState.Active):
					this.SetIconTexture(this.VOIDIconOffActivePath);
					break;
				case (IconState.PowerOn | IconState.Inactive):
					this.SetIconTexture(this.VOIDIconOnInactivePath);
					break;
				case (IconState.PowerOn | IconState.Active):
					this.SetIconTexture(this.VOIDIconOnActivePath);
					break;
				default:
					throw new NotImplementedException();
			}
		}

		protected void SetIconTexture(string texturePath)
		{
			if (this.ToolbarButton != null)
			{
				this.ToolbarButton.TexturePath = texturePath;
			}

			this.VOIDIconTexture = GameDatabase.Instance.GetTexture(texturePath.Replace("icon", "appIcon"), false);

			if (this.AppLauncherButton != null)
			{
				this.AppLauncherButton.SetTexture(VOIDIconTexture);
			}
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

				Tools.PostDebugMessage(string.Format(
					"{0}: Time to save, checking if configDirty: {1}",
					this.GetType().Name,
					this.configDirty
				));

				this.SaveConfig();
				this.saveTimer = 0;
			}
		}

		public override void LoadConfig()
		{
			base.LoadConfig();

			foreach (IVOID_Module module in this.Modules)
			{
				module.LoadConfig();
			}
		}

		public void SaveConfig()
		{
			var config = KSP.IO.PluginConfiguration.CreateForType<VOID_Core>();
			config.load();

			this._SaveToConfig(config);

			foreach (IVOID_Module module in this.Modules)
			{
				module._SaveToConfig(config);
			}

			config.save();

			this.configDirty = false;
		}

		protected VOID_Core()
		{
			this._Name = "VOID Core";

			System.Version version = this.GetType().Assembly.GetName().Version;

			this.VoidVersion = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.MajorRevision);

			this.toggleActive = true;

			this._skinName = this.defaultSkin;
			this._skinIdx = int.MinValue;

			this.VOIDIconOnInactivePath = "VOID/Textures/void_icon_light_glow";
			this.VOIDIconOnActivePath = "VOID/Textures/void_icon_dark_glow";
			this.VOIDIconOffInactivePath = "VOID/Textures/void_icon_light";
			this.VOIDIconOffActivePath = "VOID/Textures/void_icon_dark";

			this.vesselSimActive = true;

			this.UseToolbarManager = ToolbarManager.ToolbarAvailable;

			this.LoadConfig();

			this.SetIconTexture(this.powerState | this.activeState);
		}

		public virtual void Dispose()
		{
			if (this.AppLauncherButton != null)
			{
				ApplicationLauncher.Instance.RemoveModApplication(this.AppLauncherButton);
				this.AppLauncherButton = null;
			}
			if (this.ToolbarButton != null)
			{
				this.ToolbarButton.Destroy();
				this.ToolbarButton = null;
			}
		}

		protected enum IconState
		{
			PowerOff = 1,
			PowerOn = 2,
			Inactive = 4,
			Active = 8
		}
	}
}

