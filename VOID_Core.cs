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

		/*
		 * Fields
		 * */
		protected string VoidName = "VOID";
		protected string VoidVersion = "0.9.9";

		[AVOID_ConfigValue("configValue")]
		protected VOID_ConfigValue<int> configVersion = 1;

		protected List<VOID_Module> _modules = new List<VOID_Module>();

		[AVOID_ConfigValue("mainWindowPos")]
		protected VOID_ConfigValue<Rect> mainWindowPos = new Rect(Screen.width / 2, Screen.height / 2, 10f, 10f);

		[AVOID_ConfigValue("mainGuiMinimized")]
		protected VOID_ConfigValue<bool> mainGuiMinimized = false;

		[AVOID_ConfigValue("configWindowPos")]
		protected VOID_ConfigValue<Rect> configWindowPos = new Rect(Screen.width / 2, Screen.height  /2, 10f, 10f);

		[AVOID_ConfigValue("configWindowMinimized")]
		protected VOID_ConfigValue<bool> configWindowMinimized = true;

		[AVOID_ConfigValue("VOIDIconPos")]
		protected VOID_ConfigValue<Rect> VOIDIconPos = new Rect(Screen.width / 2 - 200, Screen.height - 30, 30f, 30f);
		protected Texture2D VOIDIconOff = new Texture2D(30, 30, TextureFormat.ARGB32, false);
		protected Texture2D VOIDIconOn = new Texture2D(30, 30, TextureFormat.ARGB32, false);
		protected Texture2D VOIDIconTexture;
		protected string VOIDIconOnPath = "VOID/Textures/void_icon_on";
		protected string VOIDIconOffPath = "VOID/Textures/void_icon_off";

		protected int windowBaseID = -96518722;

		[AVOID_ConfigValue("togglePower")]
		public VOID_ConfigValue<bool> togglePower = true;

		public bool powerAvailable = true;

		[AVOID_ConfigValue("consumeResource")]
		protected VOID_ConfigValue<bool> consumeResource = false;

		[AVOID_ConfigValue("resourceName")]
		protected VOID_ConfigValue<string> resourceName = "ElectricCharge";

		[AVOID_ConfigValue("resourceRate")]
		protected VOID_ConfigValue<float> resourceRate = 0.2f;

		public float saveTimer = 0;

		protected string defaultSkin = "KSP window 2";
		protected VOID_ConfigValue<string> _skin;

		public bool configDirty;

		/*
		 * Properties
		 * */
		public List<VOID_Module> Modules
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
				if (this._skin == null)
				{
					this._skin = this.defaultSkin;
				}
				return AssetBase.GetGUISkin(this._skin);
			}
		}

		public Vessel vessel
		{
			get
			{
				return FlightGlobals.ActiveVessel;
			}
		}

		/*
		 * Methods
		 * */
		protected VOID_Core()
		{
			this._Name = "VOID Core";

			this.VOIDIconOn = GameDatabase.Instance.GetTexture (this.VOIDIconOnPath, false);
			this.VOIDIconOff = GameDatabase.Instance.GetTexture (this.VOIDIconOffPath, false);

			// HACK: This is modular but not extensible.  We need to look outside our assembly or move this to modules.
			foreach (Type T in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
			{
				Tools.PostDebugMessage (string.Format ("VOID_Core: Testing type {0}", T.Name));
				if (typeof(IVOID_Module).IsAssignableFrom(T) &&
				    !T.IsAbstract  &&
				    !typeof(VOID_Core).IsAssignableFrom(T))
				{
					this.LoadModule (T);
					Tools.PostDebugMessage(string.Format("VOID_Core: Found module {0}.", T.Name));
				}
			}

			Tools.PostDebugMessage (string.Format ("VOID_Core: Loaded {0} modules.", this.Modules.Count));

			this.LoadConfig ();
		}

		public void LoadModule(Type T)
		{
			this._modules.Add (Activator.CreateInstance (T) as VOID_Module);
		}

		public void Update()
		{
			this.saveTimer += Time.deltaTime;

			if (!this.guiRunning)
			{
				this.StartGUI ();
			}

			if (!HighLogic.LoadedSceneIsFlight && this.guiRunning)
			{
				this.StopGUI ();
			}

			foreach (VOID_Module module in this.Modules)
			{
				if (!module.guiRunning && module.toggleActive)
				{
					module.StartGUI ();
				}
				if (module.guiRunning && !module.toggleActive || !this.togglePower || !HighLogic.LoadedSceneIsFlight)
				{
					module.StopGUI();
				}
			}

			if (this.saveTimer > 15f)
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
					foreach (VOID_Module module in this.Modules)
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

			this.consumeResource = GUILayout.Toggle (this.consumeResource, "Consume Resources");

			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}

		public override void DrawGUI()
		{
			GUI.skin = this.Skin;

			int windowID = this.windowBaseID;

            this.VOIDIconTexture = this.VOIDIconOff;  //icon off default
			if (this.togglePower) this.VOIDIconTexture = this.VOIDIconOn;     //or on if power_toggle==true
			if (GUI.Button(new Rect(VOIDIconPos), VOIDIconTexture, new GUIStyle()))
			{
				this.mainGuiMinimized = !this.mainGuiMinimized;
			}

			if (!this.mainGuiMinimized)
			{
				Rect _mainWindowPos = this.mainWindowPos;

				_mainWindowPos = GUILayout.Window (
					++windowID,
					_mainWindowPos,
					this.VOIDMainWindow,
					string.Join (" ", this.VoidName, this.VoidVersion),
					GUILayout.Width (250),
					GUILayout.Height (50)
				);

				if (_mainWindowPos != this.mainWindowPos.value)
				{
					this.mainWindowPos = _mainWindowPos;
				}
			}

			if (!this.configWindowMinimized)
			{
				Rect _configWindowPos = this.configWindowPos;

				this.configWindowPos = GUILayout.Window (
					++windowID,
					_configWindowPos,
					this.VOIDConfigWindow,
					string.Join (" ", this.VoidName, "Configuration"),
					GUILayout.Width (250),
					GUILayout.Height (50)
				);

				if (_configWindowPos != this.configWindowPos.value)
				{
					this.mainWindowPos = _configWindowPos;
				}
			}
		}

		public override void LoadConfig()
		{
			base.LoadConfig ();

			foreach (VOID_Module module in this.Modules)
			{
				module.LoadConfig ();
			}
		}

		public override void SaveConfig()
		{
			if (!this.configDirty)
			{
				return;
			}

			base.SaveConfig ();

			foreach (VOID_Module module in this.Modules)
			{
				module.SaveConfig ();
			}

			this.configDirty = false;
		}
	}
}

