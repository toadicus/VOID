// VOID
//
// VOID_HUDModule.cs
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
using System.Text;
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	public abstract class VOID_HUDModule : VOID_Module
	{
		[AVOID_SaveValue("colorIndex")]
		protected VOID_SaveValue<int> _colorIndex;

		protected List<Color> textColors;

		[AVOID_SaveValue("positionsLocked")]
		protected VOID_SaveValue<bool> positionsLocked;

		public virtual int ColorIndex
		{
			get
			{
				return this._colorIndex;
			}
			set
			{
				if (this._colorIndex >= this.textColors.Count - 1)
				{
					this._colorIndex = 0;
					return;
				}

				this._colorIndex = value;
			}
		}

		public virtual List<HUDWindow> Windows
		{
			get;
			protected set;
		}

		public VOID_HUDModule() : base()
		{
			this._colorIndex = 0;

			this.textColors = new List<Color>();

			this.textColors.Add(Color.green);
			this.textColors.Add(Color.black);
			this.textColors.Add(Color.white);
			this.textColors.Add(Color.red);
			this.textColors.Add(Color.blue);
			this.textColors.Add(Color.yellow);
			this.textColors.Add(Color.gray);
			this.textColors.Add(Color.cyan);
			this.textColors.Add(Color.magenta);

			this.positionsLocked = true;

			this.Windows = new List<HUDWindow>();
		}

		public override void DrawGUI()
		{
			VOID_Styles.labelHud.normal.textColor = textColors [ColorIndex];

			GUI.skin = this.core.Skin;

			if (HighLogic.LoadedSceneIsEditor ||
				(TimeWarp.WarpMode == TimeWarp.Modes.LOW) || (TimeWarp.CurrentRate <= TimeWarp.MaxPhysicsRate)
			)
			{
				SimManager.RequestSimulation();
			}

			foreach (HUDWindow window in this.Windows)
			{
				window.WindowPos = GUI.Window(
					this.core.windowID,
					window.WindowPos,
					VOID_Tools.GetWindowHandler(window.WindowFunction),
					GUIContent.none,
					GUIStyle.none
				);
			}
		}

		public override void DrawConfigurables()
		{
			if (GUILayout.Button (string.Intern("Change HUD color"), GUILayout.ExpandWidth (false)))
			{
				++this.ColorIndex;
			}

			if (GUILayout.Button(string.Intern("Reset HUD Positions"), GUILayout.ExpandWidth(false)))
			{
				foreach (HUDWindow window in this.Windows)
				{
					window.WindowPos = new Rect(window.defaultWindowPos);
				}
			}

			this.positionsLocked = GUILayout.Toggle(this.positionsLocked,
				string.Intern("Lock HUD Positions"),
				GUILayout.ExpandWidth(false));
		}
	}

	public class HUDWindow
	{
		public readonly Rect defaultWindowPos;

		public Action<int> WindowFunction
		{
			get;
			private set;
		}

		public Rect WindowPos
		{
			get;
			set;
		}

		private HUDWindow() {}

		public HUDWindow(Action<int> windowFunc, Rect defaultPos)
		{
			this.WindowFunction = windowFunc;
			this.defaultWindowPos = defaultPos;
			this.WindowPos = new Rect(this.defaultWindowPos);
		}
	}
}

