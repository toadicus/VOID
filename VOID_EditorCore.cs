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

			Rect _mainPos = new Rect (this.mainWindowPos);

			Rect _iconPos = new Rect(this.VOIDIconPos);
			Vector2 _iconCtr = new Vector2 ();

			// HACK: This is really messy.  Clean it up.
			if (_mainPos.yMax > Screen.height - 30 ||
			    _mainPos.yMin < 30
			    )
			{
				if (_mainPos.xMax > Screen.width - 30 ||
				    _mainPos.xMin < 30
				    )
				{
					if (_mainPos.yMax < Screen.height / 2)
					{
						_iconCtr.y = _mainPos.yMax + 15;
					}
					else
					{
						_iconCtr.y = _mainPos.yMin - 15;
					}
				}
				else
				{
					if (_mainPos.yMax > Screen.height / 2)
					{
						_iconCtr.y = _mainPos.yMax - 15;
					}
					else
					{
						_iconCtr.y = _mainPos.yMin + 15;
					}
				}

				if (_mainPos.center.x < Screen.width / 2)
				{
					_iconCtr.x = _mainPos.xMin - 15;
				}
				else
				{
					_iconCtr.x = _mainPos.xMax + 15;
				}

			}
			else
			{
				if (_mainPos.xMax > Screen.width - 30)
				{
					_iconCtr.x = _mainPos.xMax - 15;
				}
				else if (_mainPos.xMin < 30)
				{
					_iconCtr.x = _mainPos.xMin + 15;
				}
				else
				{
					_iconCtr.x = _mainPos.center.x;
				}

				_iconCtr.y = _mainPos.yMin - 15;
			}

			_iconCtr = Tools.ClampV2ToScreen (_iconCtr, 15);
			_iconPos.center = _iconCtr;

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

				this.CheckAndSave ();
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
				SimManager.Instance.Gravity = 9.08665;
				SimManager.Instance.TryStartSimulation();
			}
		}

		public new void FixedUpdate() {}
	}
}

