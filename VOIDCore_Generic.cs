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

using KerbalEngineer.Editor;
using KerbalEngineer.Helpers;
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
	public abstract class VOIDCore_Generic<T> : VOID_SingletonCore<T>, IVOID_Module, IDisposable
		where T : VOID_Module, new()
	{
		/*
		 * Fields
		 * */
		protected string VoidName = "VOID";
		protected string VoidVersion;

		[AVOID_SaveValue("configValue")]
		protected VOID_SaveValue<int> configVersion = (VOID_SaveValue<int>)VOIDCore.CONFIG_VERSION;

		protected List<IVOID_Module> modules = new List<IVOID_Module>();
		protected bool modulesLoaded = false;

		protected Texture2D VOIDIconTexture;
		protected string VOIDIconOnActivePath;
		protected string VOIDIconOnInactivePath;
		protected string VOIDIconOffActivePath;
		protected string VOIDIconOffInactivePath;

		[AVOID_SaveValue("useToolbarManager")]
		protected VOID_SaveValue<bool> useToolbarManager;

		protected GUIStyle iconStyle;

		protected int windowBaseID = -96518722;
		protected int windowID = 0;

		protected bool GUIStylesLoaded = false;

		protected CelestialBody homeBody;

		[AVOID_SaveValue("togglePower")]
		public VOID_SaveValue<bool> togglePower = (VOID_SaveValue<bool>)true;

		public override bool powerAvailable { get; protected set; }

		[AVOID_SaveValue("consumeResource")]
		protected VOID_SaveValue<bool> consumeResource = (VOID_SaveValue<bool>)false;

		[AVOID_SaveValue("resourceName")]
		protected VOID_SaveValue<string> resourceName = (VOID_SaveValue<string>)"ElectricCharge";

		[AVOID_SaveValue("resourceRate")]
		protected VOID_SaveValue<float> resourceRate = (VOID_SaveValue<float>)0.2f;

		[AVOID_SaveValue("updatePeriod")]
		protected VOID_SaveValue<double> updatePeriod = (VOID_SaveValue<double>)(1001f / 15000f);
		protected string stringFrequency;

		[AVOID_SaveValue("vesselSimActive")]
		protected VOID_SaveValue<bool> vesselSimActive;

		[AVOID_SaveValue("timeScaleFlags")]
		protected VOID_SaveValue<UInt32> timeScaleFlags;

		// Load-up housekeeping
		protected bool vesselTypesLoaded = false;
		protected bool simManagerLoaded = false;

		protected string defaultSkin = "KSP window 2";

		[AVOID_SaveValue("defaultSkin")]
		protected VOID_SaveValue<string> skinName;
		protected int skinIdx;

		protected Dictionary<string, GUISkin> validSkins;
		protected string[] skinNames;
		protected string[] forbiddenSkins =
			{
				"PlaqueDialogSkin",
				"FlagBrowserSkin",
				"SSUITextAreaDefault",
				"ExperimentsDialogSkin",
				"ExpRecoveryDialogSkin",
				"KSP window 1",
				"KSP window 3",
				"KSP window 5",
				"KSP window 6",
				"PartTooltipSkin",
				"KSCContextMenuSkin"
			};
		protected bool skinsLoaded = false;

		public override bool configDirty { get; set; }

		protected IButton ToolbarButton;
		protected ApplicationLauncherButton AppLauncherButton;
		protected IconState iconState;

		/*
		 * Properties
		 * */
		public override bool Active
		{
			get
			{
				return base.Active;
			}
			set
			{
				if (value != base.Active)
				{
					this.SetIconTexture(this.powerState | this.activeState);
				}

				base.Active = value;
			}
		}
		public override IList<CelestialBody> AllBodies
		{
			get
			{
				return FlightGlobals.Bodies.AsReadOnly();
			}
		}

		public override VesselType[] AllVesselTypes
		{
			get;
			protected set;
		}

		public override int ConfigVersion
		{
			get
			{
				return this.configVersion;
			}
		}

		public bool FactoryReset
		{
			get;
			protected set;
		}

		public override CelestialBody HomeBody
		{
			get
			{
				if (this.homeBody == null)
				{
					if (Planetarium.fetch != null)
					{
						this.homeBody = Planetarium.fetch.Home;
					}
				}

				return this.homeBody;
			}
		}

		public override IList<IVOID_Module> Modules
		{
			get
			{
				return this.modules.AsReadOnly();
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
						return this.validSkins[this.skinName];
					}
					catch
					{
					}
				}

				return AssetBase.GetGUISkin(this.defaultSkin);
			}
		}

		public override List<CelestialBody> SortedBodyList
		{
			get;
			protected set;
		}

		public override double UpdatePeriod
		{
			get
			{
				return this.updatePeriod;
			}
		}

		public override float UpdateTimer
		{
			get;
			protected set;
		}

		public override int WindowID
		{
			get
			{
				if (this.windowID == 0)
				{
					this.windowID = this.windowBaseID;
				}
				return this.windowID++;
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

		public override VOID_TimeScale TimeScale
		{
			get
			{
				return (VOID_TimeScale)this.timeScaleFlags.value;
			}
			protected set
			{
				this.timeScaleFlags.value = (UInt32)value;
			}
		}

		protected IconState activeState
		{
			get
			{
				if (this.Active)
				{
					return IconState.Inactive;
				}
				else
				{
					return IconState.Active;
				}

			}
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
		// public 
		public override event VOIDEventHandler onApplicationQuit;
		public override event VOIDEventHandler onSkinChanged;
		public override event VOIDEventHandler onUpdate;

		/*
		 * Methods
		 * */
		public override void DrawGUI()
		{
			this.windowID = this.windowBaseID;

			if (!this.modulesLoaded)
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
					this.useToolbarManager);
			}

			if (
				this.iconState != (this.powerState | this.activeState) ||
				(this.VOIDIconTexture == null && this.AppLauncherButton != null)
			)
			{
				this.iconState = this.powerState | this.activeState;

				this.SetIconTexture(this.iconState);
			}

			if (this.Active)
			{
				base.DrawGUI();
			}
		}

		public virtual void Update()
		{
			this.LoadBeforeUpdate();

			if (
				this.vesselSimActive &&
				(
					this.Vessel != null ||
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

			if (!this.GUIRunning)
			{
				this.StartGUI();
			}

			foreach (IVOID_Module module in this.modules)
			{
				if (
					!module.GUIRunning &&
					module.Active &&
					module.InValidScene &&
					(
						!HighLogic.LoadedSceneIsEditor ||
						(EditorLogic.RootPart != null && EditorLogic.SortedShipList.Count > 0)
					)
				)
				{
					module.StartGUI();
				}
				if (
					module.GUIRunning &&
					(
						!module.Active ||
					    !this.togglePower ||
						!module.InValidScene ||
					    this.FactoryReset ||
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

			if (this.useToolbarManager)
			{
				if (this.AppLauncherButton != null)
				{
					ApplicationLauncher.Instance.RemoveModApplication(this.AppLauncherButton);
					this.AppLauncherButton = null;
				}

				if (this.ToolbarButton == null)
				{
					this.InitializeToolbarButton();
				}
			}
			else
			{
				if (this.ToolbarButton != null)
				{
					this.ToolbarButton.Destroy();
					this.ToolbarButton = null;
				}

				if (this.AppLauncherButton == null)
				{
					this.InitializeAppLauncherButton();
				}

			}

			this.saveTimer += Time.deltaTime;

			if (this.saveTimer > 2f)
			{
				if (this.configDirty)
				{

					Tools.PostDebugMessage(string.Format(
							"{0}: Time to save, checking if configDirty: {1}",
							this.GetType().Name,
							this.configDirty
						));

					this.SaveConfig();
					this.saveTimer = 0;
				}
			}

			this.UpdateTimer += Time.deltaTime;

			if (this.onUpdate != null)
			{
				this.onUpdate(this);
			}
		}

		public virtual void FixedUpdate()
		{
			bool newPowerState = this.powerAvailable;

			if (this.togglePower && this.consumeResource &&
			    this.Vessel.vesselType != VesselType.EVA &&
			    TimeWarp.deltaTime != 0)
			{
				float powerReceived = this.Vessel.rootPart.RequestResource(
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
				}
			}

			foreach (IVOID_Module module in this.modules)
			{
				if (module is IVOID_BehaviorModule)
				{
					((IVOID_BehaviorModule)module).FixedUpdate();
				}
			}
		}

		public void OnDestroy()
		{
			foreach (IVOID_Module module in this.modules)
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

		public override void StartGUI()
		{
			if (!this.GUIRunning)
			{
				RenderingManager.AddToPostDrawQueue(3, this.DrawGUI);
			}
		}

		public void ResetGUI()
		{
			this.StopGUI();

			foreach (IVOID_Module module in this.modules)
			{
				module.StopGUI();
				module.StartGUI();
			}

			this.StartGUI();
		}

		public override void ModuleWindow(int id)
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
					}
				}

				if (togglePower || !HighLogic.LoadedSceneIsFlight)
				{
					foreach (IVOID_Module module in this.modules)
					{
						if (module is VOID_ConfigWindow)
						{
							continue;
						}

						module.Active = GUITools.Toggle(module.Active, module.Name);
					}
				}
			}
			else
			{
				GUILayout.Label("-- POWER LOST --", VOID_Styles.labelRed);
			}

			VOID_ConfigWindow.Instance.Active = GUITools.Toggle(
				VOID_ConfigWindow.Instance.Active,
				"Configuration"
			);

			GUILayout.EndVertical();

			base.ModuleWindow(id);
		}

		public override void DrawConfigurables()
		{
			GUIContent _content;

			this.useToolbarManager.value = GUITools.Toggle(this.useToolbarManager, "Use Blizzy's Toolbar If Available");

			this.vesselSimActive.value = GUITools.Toggle(this.vesselSimActive.value,
				"Enable Engineering Calculations");

			bool useEarthTime = (this.TimeScale & VOID_TimeScale.KERBIN_TIME) == 0u;
			bool useSiderealTime = (this.TimeScale & VOID_TimeScale.SOLAR_DAY) == 0u;
			bool useRoundedScale = (this.TimeScale & VOID_TimeScale.ROUNDED_SCALE) != 0u;

			useEarthTime = GUITools.Toggle(useEarthTime, "Use Earth Time (changes KSP option)");

			GameSettings.KERBIN_TIME = !useEarthTime;

			useSiderealTime = GUITools.Toggle(
				useSiderealTime,
				string.Format(
					"Time Scale: {0}",
					useSiderealTime ? "Sidereal" : "Solar"
				)
			);

			useRoundedScale = GUITools.Toggle(
				useRoundedScale,
				string.Format(
					"Time Scale: {0}",
					useRoundedScale ? "Rounded" : "True"
				)
			);

			if (useEarthTime)
			{
				this.TimeScale &= ~VOID_TimeScale.KERBIN_TIME;
			}
			else
			{
				this.TimeScale |= VOID_TimeScale.KERBIN_TIME;
			}

			if (useSiderealTime)
			{
				this.TimeScale &= ~VOID_TimeScale.SOLAR_DAY;
			}
			else
			{
				this.TimeScale |= VOID_TimeScale.SOLAR_DAY;
			}

			if (useRoundedScale)
			{
				this.TimeScale |= VOID_TimeScale.ROUNDED_SCALE;
			}
			else
			{
				this.TimeScale &= ~VOID_TimeScale.ROUNDED_SCALE;
			}

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

			GUILayout.Label("Skin:", GUILayout.ExpandWidth(false));

			_content = new GUIContent();

			_content.text = "◄";
			_content.tooltip = "Select previous skin";
			if (GUILayout.Button(_content, GUILayout.ExpandWidth(true)))
			{
				this.skinIdx--;
				Tools.PostDebugMessage(string.Format(
					"{0}: new this.skinIdx = {1} :: skin_list.Count = {2}",
					this.GetType().Name,
					this.skinName,
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
				this.skinIdx++;
				Tools.PostDebugMessage(string.Format(
					"{0}: new this.skinIdx = {1} :: skin_list.Count = {2}",
					this.GetType().Name,
					this.skinName,
					this.validSkins.Count
				));
			}

			this.skinIdx %= this.skinNames.Length;
			if (this.skinIdx < 0)
			{
				this.skinIdx += this.skinNames.Length;
			}

			if (this.skinName != skinNames[this.skinIdx])
			{
				this.skinName.value = skinNames[this.skinIdx];
				this.GUIStylesLoaded = false;
			}

			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Update Rate (Hz):");
			if (this.stringFrequency == null)
			{
				this.stringFrequency = (1f / this.UpdatePeriod).ToString();
			}
			this.stringFrequency = GUILayout.TextField(this.stringFrequency.ToString(), 5, GUILayout.ExpandWidth(true));

			if (GUILayout.Button("Apply"))
			{
				double updateFreq = 1f / this.UpdatePeriod;
				double.TryParse(stringFrequency, out updateFreq);
				this.updatePeriod.value = 1 / updateFreq;
			}
			GUILayout.EndHorizontal();

			foreach (IVOID_Module mod in this.modules)
			{
				mod.DrawConfigurables();
			}

			this.FactoryReset = GUITools.Toggle(this.FactoryReset, "Factory Reset");
		}

		protected void UpdateSimManager()
		{
			if (HighLogic.LoadedSceneIsFlight)
			{
				double radius = this.Vessel.Radius();
				SimManager.Gravity = this.Vessel.mainBody.gravParameter / (radius * radius);
				SimManager.Atmosphere = this.Vessel.staticPressurekPa * PhysicsGlobals.KpaToAtmospheres;
				SimManager.Mach = this.Vessel.mach;
				BuildAdvanced.Altitude = this.Vessel.altitude;
				CelestialBodies.SelectedBody = this.Vessel.mainBody;
			}

			#if DEBUG
			SimManager.logOutput = true;
			#endif

			SimManager.TryStartSimulation();

			Tools.PostDebugMessage(this, "Started Engineer simulation with Atmosphere={0} atm and Gravity={1} m/s²",
				SimManager.Atmosphere,
				SimManager.Gravity
			);
		}

		protected void GetSimManagerResults()
		{
			Tools.PostDebugMessage(this, "VesselSimulator results ready, setting Stages.");

			this.Stages = SimManager.Stages;

			if (this.Stages != null)
			{
				this.LastStage = this.Stages.Last();
			}
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

			this.modulesLoaded = true;

			sb.AppendFormat("Loaded {0} modules.\n", this.Modules.Count);

			sb.Print();
		}

		protected void LoadModule(Type T)
		{
			var existingModules = this.modules.Where(mod => mod.GetType().Name == T.Name);
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
				System.Reflection.BindingFlags.Static |
				System.Reflection.BindingFlags.Public |
				System.Reflection.BindingFlags.FlattenHierarchy
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

			if (module.InValidGame && module.InValidScene)
			{
				module.LoadConfig();
				this.modules.Add(module);

				Tools.PostDebugMessage(string.Format(
						"{0}: loaded module {1}.",
						this.GetType().Name,
						T.Name
					));
			}
			else
			{
				if (module is IDisposable)
				{
					(module as IDisposable).Dispose();
				}
			}
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
				if (this.skinNames[i] == this.skinName)
				{
					this.skinIdx = i;
				}
				if (this.skinNames[i] == this.defaultSkin)
				{
					defaultIdx = i;
				}
				if (this.skinIdx != int.MinValue && defaultIdx != int.MinValue)
				{
					break;
				}
			}

			if (this.skinIdx == int.MinValue)
			{
				this.skinIdx = defaultIdx;
			}

			Tools.PostDebugMessage(string.Format(
				"{0}: _skinIdx = {1}.",
				this.GetType().Name,
				this.skinName.ToString()
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

		protected void LoadBeforeUpdate()
		{
			if (!this.vesselTypesLoaded)
			{
				this.AllVesselTypes = Enum.GetValues(typeof(VesselType)).OfType<VesselType>().ToArray();
				this.vesselTypesLoaded = true;
			}

			if (this.SortedBodyList == null && FlightGlobals.Bodies != null && FlightGlobals.Bodies.Count > 0)
			{
				this.SortedBodyList = new List<CelestialBody>(FlightGlobals.Bodies);
				this.SortedBodyList.Sort(new CBListComparer());
				this.SortedBodyList.Reverse();

				Debug.Log(string.Format("sortedBodyList: {0}", string.Join("\n\t", this.SortedBodyList.Select(b => b.bodyName).ToArray())));
			}

			// SimManager initialization that we don't necessarily want to repeat every Update.
			if (!this.simManagerLoaded && this.HomeBody != null)
			{
				SimManager.Gravity = VOID_Data.KerbinGee;
				SimManager.Atmosphere = 0d;
				SimManager.Mach = 1d;
				CelestialBodies.SelectedBody = this.HomeBody;
				BuildAdvanced.Altitude = 0d;
				SimManager.OnReady += this.GetSimManagerResults;

				this.simManagerLoaded = true;
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
			this.Active = !this.Active;
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
			if (texturePath == null)
			{
				return;
			}

			if (this.ToolbarButton != null)
			{
				this.ToolbarButton.TexturePath = texturePath;
			}

			if (this.AppLauncherButton != null)
			{
				this.VOIDIconTexture = GameDatabase.Instance.GetTexture(texturePath.Replace("icon", "appIcon"), false);

				this.AppLauncherButton.SetTexture(VOIDIconTexture);
			}
		}

		public override void LoadConfig()
		{
			base.LoadConfig();

			foreach (IVOID_Module module in this.modules)
			{
				module.LoadConfig();
			}

			this.TimeScale |= GameSettings.KERBIN_TIME ? VOID_TimeScale.KERBIN_TIME : 0u;
		}

		public override void SaveConfig()
		{
			if (this.configNeedsUpdate && this is VOIDCore_Flight)
			{
				KSP.IO.File.Delete<T>("config.xml");
			}

			var config = KSP.IO.PluginConfiguration.CreateForType<T>();

			config.load();

			this.Save(config);

			foreach (IVOID_Module module in this.modules)
			{
				module.Save(config);
			}

			config.save();

			this.configDirty = false;
		}

		public VOIDCore_Generic()
		{
			System.Version version = this.GetType().Assembly.GetName().Version;

			this.VoidVersion = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.MajorRevision);

			this.Name = string.Format("VOID {0}", this.VoidVersion.ToString());

			this.powerAvailable = true;

			this.Active = true;

			this.skinName = (VOID_SaveValue<string>)this.defaultSkin;
			this.skinIdx = int.MinValue;

			this.VOIDIconOnActivePath = "VOID/Textures/void_icon_light_glow";
			this.VOIDIconOnInactivePath = "VOID/Textures/void_icon_dark_glow";
			this.VOIDIconOffActivePath = "VOID/Textures/void_icon_light";
			this.VOIDIconOffInactivePath = "VOID/Textures/void_icon_dark";

			this.saveTimer = 0f;
			this.UpdateTimer = 0f;

			this.vesselSimActive = (VOID_SaveValue<bool>)true;

			this.useToolbarManager = (VOID_SaveValue<bool>)ToolbarManager.ToolbarAvailable;

			this.LoadConfig();

			this.configVersion = (VOID_SaveValue<int>)VOIDCore.CONFIG_VERSION;

			this.FactoryReset = false;
		}

		public override void Dispose()
		{
			this.StopGUI();

			this.onSkinChanged(this);

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
	}
}
