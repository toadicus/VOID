// VOID
//
// VOID_EditorCore.cs
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

using Engineer.VesselSimulator;
using KSP;
using System;
using System.Collections.Generic;
using System.Linq;
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	public class VOID_EditorCore : VOID_Core
	{
		/*
		 * Static Members
		 * */
		protected new static bool _initialized = false;
		public new static bool Initialized
		{
			get 
			{
				return _initialized;
			}
		}

		protected new static VOID_EditorCore _instance;
		public new static VOID_EditorCore Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new VOID_EditorCore();
					_initialized = true;
				}
				return _instance;
			}
		}

		public new static void Reset()
		{
			if (_initialized)
			{
				_instance.StopGUI();
				_instance = null;
				_initialized = false;
			}
		}

		protected override ApplicationLauncher.AppScenes appIconVisibleScenes
		{
			get
			{
				return ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH;
			}
		}

		public VOID_EditorCore() : base()
		{
			this._Name = "VOID Editor Core";
		}

		public new void OnGUI() {}

		public override void DrawGUI()
		{
			if (!this._modulesLoaded)
			{
				this.LoadModulesOfType<IVOID_EditorModule>();
			}

			Rect _iconPos = Tools.DockToWindow (this.VOIDIconPos, this.mainWindowPos);

			_iconPos = Tools.ClampRectToScreen (_iconPos, (int)_iconPos.width, (int)_iconPos.height);

			if (_iconPos != this.VOIDIconPos)
			{
				this.VOIDIconPos = _iconPos;
			}

			base.DrawGUI();
		}

		public new void Update()
		{
			foreach (IVOID_EditorModule module in this.Modules)
			{
				if (EditorLogic.startPod == null)
				{
					module.StopGUI();
					continue;
				}
				if (HighLogic.LoadedSceneIsEditor && module.toggleActive && EditorLogic.SortedShipList.Any())
				{
					module.StartGUI();
				}
				if (!HighLogic.LoadedSceneIsEditor || !module.toggleActive || !EditorLogic.SortedShipList.Any())
				{
					module.StopGUI();
				}
			}

			if (EditorLogic.startPod == null || !HighLogic.LoadedSceneIsEditor)
			{
				this.StopGUI();
				return;
			}
			else if (!this.guiRunning && HighLogic.LoadedSceneIsEditor)
			{
				this.StartGUI();
			}

			if (EditorLogic.SortedShipList.Count > 0 && this.vesselSimActive)
			{
				SimManager.Gravity = VOID_Data.KerbinGee;
				SimManager.TryStartSimulation();
			}
			else if (!this.vesselSimActive)
			{
				SimManager.ClearResults();
			}

			this.CheckAndSave ();
		}

		public new void FixedUpdate() {}
	}
}

