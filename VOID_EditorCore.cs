//
//  VOID_EditorCore.cs
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
using Engineer.VesselSimulator;
using KSP;
using System;
using System.Collections.Generic;
using System.Linq;
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

			if (EditorLogic.SortedShipList.Count > 0)
			{
				SimManager.Gravity = VOID_Data.KerbinGee;
				SimManager.TryStartSimulation();
			}

			this.CheckAndSave ();
		}

		public new void FixedUpdate() {}
	}
}

