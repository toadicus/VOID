// VOID
//
// VOIDCore_Generic.cs
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
	public abstract class VOIDCore_Generic<T> : VOID_SingletonModule<T>, IVOID_Module, IDisposable
		where T : VOID_Module, new()
	{
		/*
		 * Fields
		 * */
		protected string VoidName = "VOID";
		protected string VoidVersion;

		[AVOID_SaveValue("configValue")]
		protected VOID_SaveValue<int> _configVersion = VOIDCore.CONFIG_VERSION;

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

		protected Texture2D VOIDIconTexture;
		protected string VOIDIconOnActivePath;
		protected string VOIDIconOnInactivePath;
		protected string VOIDIconOffActivePath;
		protected string VOIDIconOffInactivePath;

		protected GUIStyle iconStyle;

		protected int windowBaseID = -96518722;
		protected int _windowID = 0;

		protected bool GUIStylesLoaded = false;

		protected CelestialBody _homeBody;

		[AVOID_SaveValue("togglePower")]
		public VOID_SaveValue<bool> togglePower = true;

		public override bool powerAvailable { get; protected set; }

		[AVOID_SaveValue("consumeResource")]
		protected VOID_SaveValue<bool> consumeResource = false;

		[AVOID_SaveValue("resourceName")]
		protected VOID_SaveValue<string> resourceName = "ElectricCharge";

		[AVOID_SaveValue("resourceRate")]
		protected VOID_SaveValue<float> resourceRate = 0.2f;

		[AVOID_SaveValue("updatePeriod")]
		protected VOID_SaveValue<double> _updatePeriod = 1001f / 15000f;
		protected string stringFrequency;

		[AVOID_SaveValue("vesselSimActive")]
		protected VOID_SaveValue<bool> vesselSimActive;

		// Vessel Type Housekeeping
		protected List<VesselType> _allVesselTypes = new List<VesselType>();
		protected bool vesselTypesLoaded = false;

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

		public override bool configDirty { get; set; }

		internal IButton ToolbarButton;

		internal ApplicationLauncherButton AppLauncherButton;

		/*
		 * Properties
		 * */
		public override int configVersion
		{
			get
			{
				return this._configVersion;
			}
		}

		public bool factoryReset
		{
			get;
			protected set;
		}

		public override List<IVOID_Module> Modules
		{
			get
			{
				return this._modules;
			}
		}

		public override GUISkin Skin
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

		public override int windowID
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

		public override List<CelestialBody> allBodies
		{
			get
			{
				return FlightGlobals.Bodies;
			}
		}

		public override List<CelestialBody> sortedBodyList
		{
			get;
			protected set;
		}

		public override CelestialBody HomeBody
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

		public override List<VesselType> allVesselTypes
		{
			get
			{
				return this._allVesselTypes;
			}
		}

		public override float updateTimer
		{
			get;
			protected set;
		}


		public override double updatePeriod
		{
			get
			{
				return this._updatePeriod;
			}
		}

		public override Stage[] Stages
		{
			get;
			protected set;
		}

		public override Stage LastStage
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
				return useToolbarManager & ToolbarManager.ToolbarAvailable;
			}
			set
			{
				if (useToolbarManager == value)
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

				useToolbarManager = value;
			}
		}

		protected virtual ApplicationLauncher.AppScenes appIconVisibleScenes
		{
			get
			{
				return HighLogic.LoadedScene.ToAppScenes();
			}
		}

		/*
		 * Events
		 * */
		public override event VOIDEventHandler onApplicationQuit;
		public override event VOIDEventHandler onSkinChanged;

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
					GUILayout.Width(250f),
					GUILayout.Height(50f),
					GUILayout.ExpandWidth(true),
					GUILayout.ExpandHeight(true)
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

		public virtual void Update()
		{
			this.LoadBeforeUpdate();

			if (
				this.vesselSimActive &&
				(
					this.vessel != null ||
					(
						HighLogic.LoadedSceneIsEditor &&
						EditorLogic.RootPart != null &&
						EditorLogic.SortedShipList.Count > 0
					)
				)
			)
			{
				Tools.PostDebugMessage(this, "Updating SimManager.");
				this.UpdateSimManager();
			}

			if (!this.guiRunning)
			{
				this.StartGUI();
			}

			foreach (IVOID_Module module in this.Modules)
			{
				if (
					!module.guiRunning &&
					module.toggleActive &&
					module.inValidScene &&
					(
						!HighLogic.LoadedSceneIsEditor ||
						(EditorLogic.RootPart != null && EditorLogic.SortedShipList.Count > 0)
					)
				)
				{
					module.StartGUI();
				}
				if (
					module.guiRunning &&
					(
						!module.toggleActive ||
					    !this.togglePower ||
						!module.inValidScene ||
					    this.factoryReset ||
						(
							HighLogic.LoadedSceneIsEditor &&
							(EditorLogic.RootPart == null || EditorLogic.SortedShipList.Count == 0)
						)
					)
				)
				{
					module.StopGUI();
				}

				if (module is IVOID_BehaviorModule)
				{
					((IVOID_BehaviorModule)module).Update();
				}
			}

			this.CheckAndSave();
			this.updateTimer += Time.deltaTime;
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

			this.Dispose();
		}

		public virtual void OnApplicationQuit()
		{
			if (this.onApplicationQuit != null)
			{
				this.onApplicationQuit(this);
			}

			this.OnDestroy();
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

			if (this.powerAvailable || !HighLogic.LoadedSceneIsFlight)
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

				if (togglePower || !HighLogic.LoadedSceneIsFlight)
				{
					foreach (IVOID_Module module in this.Modules)
					{
						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

						module.toggleActive = GUILayout.Toggle(
							module.toggleActive,
							GUIContent.none,
							GUILayout.ExpandWidth(false)
						);

						GUILayout.Label(module.Name, GUILayout.ExpandWidth(true));

						GUILayout.EndHorizontal();
					}
				}
			}
			else
			{
				GUILayout.Label("-- POWER LOST --", VOID_Styles.labelRed);
			}

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

			this.configWindowMinimized.value = !GUILayout.Toggle(
				!this.configWindowMinimized,
				GUIContent.none,
				GUILayout.ExpandWidth(false)
			);

			GUILayout.Label("Configuration", GUILayout.ExpandWidth(true));

			GUILayout.EndHorizontal();

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

			this.factoryReset = GUILayout.Toggle(this.factoryReset, "Factory Reset");
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

		protected void LoadModulesOfType<U>()
		{
			Tools.DebugLogger sb = Tools.DebugLogger.New(this);
			sb.AppendLine("Loading modules...");

			foreach (AssemblyLoader.LoadedAssembly assy in AssemblyLoader.loadedAssemblies)
			{
				foreach (Type loadedType in assy.assembly.GetExportedTypes())
				{
					if (
						loadedType.IsInterface ||
						loadedType.IsAbstract ||
						!typeof(U).IsAssignableFrom(loadedType) ||
						typeof(VOIDCore).IsAssignableFrom(loadedType)
					)
					{
						continue;
					}

					sb.AppendFormat("Checking IVOID_Module type {0}...", loadedType.Name);

					GameScenes[] validScenes = null;

					foreach (var attr in loadedType.GetCustomAttributes(true))
					{
						if (attr is VOID_ScenesAttribute)
						{
							validScenes = ((VOID_ScenesAttribute)attr).ValidScenes;

							sb.Append("VOID_ScenesAttribute found;");

							break;
						}
					}

					if (validScenes == null)
					{
						validScenes = new GameScenes[] { GameScenes.FLIGHT };


						sb.Append("VOID_ScenesAttribute not found;");

					}

					sb.AppendFormat(
						" validScenes set to {0}.",
						string.Join(
							", ",
							validScenes.Select(s => Enum.GetName(typeof(GameScenes), s)).ToArray()
						)
					);

					if (!validScenes.Contains(HighLogic.LoadedScene))
					{
						sb.AppendFormat("  {0} not found in validScenes, skipping.",
							Enum.GetName(typeof(GameScenes), HighLogic.LoadedScene));
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
						sb.AppendFormat("Failed, caught {0}\n", ex.GetType().Name);

						#if DEBUG
						Debug.LogException(ex);
						#endif
					}
				}
			}

			this._modulesLoaded = true;

			sb.AppendFormat("Loaded {0} modules.\n", this.Modules.Count);

			sb.Print();
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

			var InstanceProperty = T.GetProperty(
				"Instance",
				System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public
			);

			object modInstance = null;
			IVOID_Module module;

			if (InstanceProperty != null)
			{
				modInstance = InstanceProperty.GetValue(null, null);
			}

			if (modInstance != null)
			{
				module = modInstance as IVOID_Module;
			}
			else
			{
				module = Activator.CreateInstance(T) as IVOID_Module;
			}

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

			if (this.onSkinChanged != null)
			{
				this.onSkinChanged(this);
			}

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

			this.ToolbarButton.Visible = true;

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

		protected virtual void CheckAndSave()
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

		public override void SaveConfig()
		{
			if (this.configNeedsUpdate && this is VOIDCore_Flight)
			{
				KSP.IO.File.Delete<T>("config.xml");
			}

			var config = KSP.IO.PluginConfiguration.CreateForType<T>();

			config.load();

			this._SaveToConfig(config);

			foreach (IVOID_Module module in this.Modules)
			{
				module._SaveToConfig(config);
			}

			config.save();

			this.configDirty = false;
		}

		public VOIDCore_Generic()
		{
			this.Name = "VOID Core";

			System.Version version = this.GetType().Assembly.GetName().Version;

			this.VoidVersion = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.MajorRevision);

			this.powerAvailable = true;

			this.toggleActive = true;

			this._skinName = this.defaultSkin;
			this._skinIdx = int.MinValue;

			this.VOIDIconOnActivePath = "VOID/Textures/void_icon_light_glow";
			this.VOIDIconOnInactivePath = "VOID/Textures/void_icon_dark_glow";
			this.VOIDIconOffActivePath = "VOID/Textures/void_icon_light";
			this.VOIDIconOffInactivePath = "VOID/Textures/void_icon_dark";

			this.saveTimer = 0f;
			this.updateTimer = 0f;

			this.vesselSimActive = true;

			this.UseToolbarManager = ToolbarManager.ToolbarAvailable;

			this.LoadConfig();

			this._configVersion = VOIDCore.CONFIG_VERSION;
			
			this.SetIconTexture(this.powerState | this.activeState);

			this.factoryReset = false;
		}

		public virtual void Dispose()
		{
			this.StopGUI();

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

			_instance = null;
			_initialized = false;
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

