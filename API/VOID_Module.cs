// VOID
//
// VOID_Module.cs
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	public abstract class VOID_Module : IVOID_Module
	{
		/*
		 * Fields
		 * */
		[AVOID_SaveValue("Active")]
		protected VOID_SaveValue<bool> _Active = false;
		private GameScenes[] validScenes;

		protected float lastUpdate = 0;

		/*
		 * Properties
		 * */
		protected virtual VOIDCore core
		{
			get
			{
				return VOID_Data.Core;
			}
		}

		public virtual bool toggleActive
		{
			get
			{
				return this._Active;
			}
			set
			{
				this._Active.value = value;
			}
		}

		public virtual bool guiRunning
		{
			get
			{
				if (
					RenderingManager.fetch == null ||
					RenderingManager.fetch.postDrawQueue == null ||
					RenderingManager.fetch.postDrawQueue.Length < 4
				)
				{
					return false;
				}
				else
				{
					Delegate callback = RenderingManager.fetch.postDrawQueue[3];
					if (callback == null)
					{
						return false;
					}

					return callback.GetInvocationList().Contains((Callback)this.DrawGUI);
				}
			}
		}

		public virtual GameScenes[] ValidScenes
		{
			get
			{
				if (this.validScenes == null)
				{
					Tools.PostDebugMessage(this, "validScenes is null when checking inValidScene; fetching attribute.");
					foreach (var attr in this.GetType().GetCustomAttributes(false))
					{
						if (attr is VOID_ScenesAttribute)
						{
							VOID_ScenesAttribute addonAttr = (VOID_ScenesAttribute)attr;

							this.validScenes = addonAttr.ValidScenes;

							Tools.PostDebugMessage("Found VOID_ScenesAttribute; validScenes set.");

							break;
						}
					}

					if (this.validScenes == null)
					{
						this.validScenes = new GameScenes[] { GameScenes.FLIGHT };
						Tools.PostDebugMessage("No VOID_ScenesAttribute found; validScenes defaulted to flight.");
					}
				}

				return this.validScenes;
			}
		}

		public virtual bool inValidScene
		{
			get
			{

				Tools.PostDebugMessage(
					this,
					"Checking if scene is valid: LoadedScene={0}, ValidScenes={1}, inValidScene={2}",
					Enum.GetName(typeof(GameScenes), HighLogic.LoadedScene),
					string.Join(", ", this.ValidScenes.Select(s => Enum.GetName(typeof(GameScenes), s)).ToArray()),
					this.ValidScenes.Contains(HighLogic.LoadedScene)
				);

				return this.ValidScenes.Contains(HighLogic.LoadedScene);
			}
		}

		public virtual string Name
		{
			get;
			protected set;
		}

		public virtual Vessel vessel
		{
			get
			{
				return FlightGlobals.ActiveVessel;
			}
		}

		/*
		 * Methods
		 * */
		public void StartGUI()
		{
			if (!this.toggleActive || this.guiRunning)
			{
				return;
			}

			Tools.PostDebugMessage (string.Format("Adding {0} to the draw queue.", this.GetType().Name));
			RenderingManager.AddToPostDrawQueue (3, this.DrawGUI);
		}

		public void StopGUI()
		{
			if (!this.guiRunning)
			{
				return;
			}
			Tools.PostDebugMessage (string.Format("Removing {0} from the draw queue.", this.GetType().Name));
			RenderingManager.RemoveFromPostDrawQueue (3, this.DrawGUI);
		}

		public abstract void DrawGUI();

		public virtual void DrawConfigurables() {}

		public virtual void LoadConfig()
		{
			var config = KSP.IO.PluginConfiguration.CreateForType<VOID_Module> ();
			config.load ();

			if (this is VOIDCore)
			{
				int configVersion = config.GetValue("VOID_Core_configValue", 2);

				if (configVersion < VOIDCore.CONFIG_VERSION)
				{
					((VOIDCore)this).configNeedsUpdate = true;
				}
			}

			foreach (var field in this.GetType().GetMembers(
				BindingFlags.NonPublic |
				BindingFlags.Public |
				BindingFlags.Instance |
				BindingFlags.FlattenHierarchy
			))
			{
				if (!(field is FieldInfo || field is PropertyInfo))
				{
					continue;
				}

				if (field is PropertyInfo && (field as PropertyInfo).GetIndexParameters().Length > 0)
				{
					continue;
				}

				object[] attrs = field.GetCustomAttributes(typeof(AVOID_SaveValue), false);

				if (attrs.Length == 0) {
					continue;
				}

				AVOID_SaveValue attr = attrs.FirstOrDefault () as AVOID_SaveValue;

				string fieldName = string.Empty;

				if (this is VOIDCore || this.core.configNeedsUpdate)
				{
					string typeName = this.GetType().Name;;

					if (this is VOIDCore && ((VOIDCore)this).configNeedsUpdate)
					{
						if (this is VOIDCore_Flight)
						{
							typeName = "VOID_Core";
						}
						else if (this is VOIDCore_Editor)
						{
							typeName = "VOID_EditorCore";
						}
					}

					fieldName = string.Format("{0}_{1}", typeName, attr.Name);
				}
				else
				{
					fieldName = string.Format(
						"{0}_{1}_{2}",
						this.GetType().Name,
						Enum.GetName(typeof(GameScenes), HighLogic.LoadedScene),
						attr.Name
					);
				}

				Tools.PostDebugMessage(string.Format("{0}: Loading field {1}.", this.GetType().Name, fieldName));

				object fieldValue;

				if (field is FieldInfo)
				{
					fieldValue = (field as FieldInfo).GetValue(this);
				}
				else
				{
					fieldValue = (field as PropertyInfo).GetValue(this, null);
				}

				bool convertBack = false;
				if (fieldValue is IVOID_SaveValue)
				{
					fieldValue = (fieldValue as IVOID_SaveValue).value;
					convertBack = true;
				}

				fieldValue = config.GetValue(fieldName, fieldValue);

				if (convertBack)
				{
					Type type = typeof(VOID_SaveValue<>).MakeGenericType (fieldValue.GetType ());
					IVOID_SaveValue convertValue = Activator.CreateInstance (type) as IVOID_SaveValue;
					convertValue.SetValue (fieldValue);
					fieldValue = convertValue;
				}

				if (field is FieldInfo)
				{
					(field as FieldInfo).SetValue(this, fieldValue);
				}
				else
				{
					(field as PropertyInfo).SetValue(this, fieldValue, null);
				}

				Tools.PostDebugMessage(string.Format("{0}: Loaded field {1}.", this.GetType().Name, fieldName));
			}
		}

		public virtual void _SaveToConfig(KSP.IO.PluginConfiguration config)
		{
			foreach (var field in this.GetType().GetMembers(
				BindingFlags.Instance |
				BindingFlags.NonPublic |
				BindingFlags.Public |
				BindingFlags.FlattenHierarchy
				))
			{
				object[] attrs = field.GetCustomAttributes(typeof(AVOID_SaveValue), false);

				if (attrs.Length == 0) {
					continue;
				}

				AVOID_SaveValue attr = attrs.FirstOrDefault () as AVOID_SaveValue;

				string fieldName;

				if (this is VOIDCore)
				{
					fieldName = string.Format("{0}_{1}", this.GetType().Name, attr.Name);
				}
				else
				{
					fieldName = string.Format(
						"{0}_{1}_{2}",
						this.GetType().Name,
						Enum.GetName(typeof(GameScenes), HighLogic.LoadedScene),
						attr.Name
					);
				}

				object fieldValue;

				if (field is FieldInfo)
				{
					fieldValue = (field as FieldInfo).GetValue(this);
				}
				else
				{
					fieldValue = (field as PropertyInfo).GetValue(this, null);
				}

				if (fieldValue is IVOID_SaveValue)
				{
					fieldValue = (fieldValue as IVOID_SaveValue).value;
				}

				config.SetValue(fieldName, fieldValue);

				Tools.PostDebugMessage(string.Format("{0}: Saved field {1}.", this.GetType().Name, fieldName));
			}
		}
	}

	public abstract class VOID_WindowModule : VOID_Module
	{
		[AVOID_SaveValue("WindowPos")]
		protected Rect WindowPos;
		protected float defWidth;
		protected float defHeight;

		protected string inputLockName;

		public VOID_WindowModule() : base()
		{
			this.defWidth = 250f;
			this.defHeight = 50f;

			this.inputLockName = string.Concat(this.Name, "_edlock");

			this.WindowPos = new Rect(Screen.width / 2, Screen.height / 2, this.defWidth, this.defHeight);
		}

		public abstract void ModuleWindow(int _);

		public override void DrawGUI()
		{
			GUI.skin = this.core.Skin;

			Rect _Pos = this.WindowPos;

			_Pos = GUILayout.Window(
				this.core.windowID,
				_Pos,
				VOID_Tools.GetWindowHandler(this.ModuleWindow),
				this.Name,
				GUILayout.Width(this.defWidth),
				GUILayout.Height(this.defHeight)
			);

			bool cursorInWindow = _Pos.Contains(Mouse.screenPos);

			switch (HighLogic.LoadedScene)
			{
				case GameScenes.EDITOR:
					if (cursorInWindow)
					{
						InputLockManager.SetControlLock(
							ControlTypes.EDITOR_ICON_HOVER | ControlTypes.EDITOR_ICON_PICK |
							ControlTypes.EDITOR_PAD_PICK_COPY | ControlTypes.EDITOR_PAD_PICK_COPY,
							this.inputLockName
						);
						EditorLogic.fetch.Lock(false, false, false, this.inputLockName);
					}
					else
					{
						EditorLogic.fetch.Unlock(this.inputLockName);
					}
					break;
				case GameScenes.FLIGHT:
					if (cursorInWindow)
					{
						InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS, this.inputLockName);
					}
					else if (InputLockManager.GetControlLock(this.inputLockName) != ControlTypes.None)
					{
						InputLockManager.RemoveControlLock(this.inputLockName);
					}
					break;
				case GameScenes.SPACECENTER:
					if (cursorInWindow)
					{
						InputLockManager.SetControlLock(
							ControlTypes.KSC_FACILITIES | ControlTypes.CAMERACONTROLS,
							this.inputLockName
						);
					}
					else if (InputLockManager.GetControlLock(this.inputLockName) != ControlTypes.None)
					{
						InputLockManager.RemoveControlLock(this.inputLockName);
					}
					break;
			}

			if (HighLogic.LoadedSceneIsEditor)
			{
				_Pos = Tools.ClampRectToEditorPad(_Pos);
			}
			else
			{
				_Pos = Tools.ClampRectToScreen(_Pos);
			}

			if (_Pos != this.WindowPos)
			{
				this.WindowPos = _Pos;
				this.core.configDirty = true;
			}
		}
	}
}

