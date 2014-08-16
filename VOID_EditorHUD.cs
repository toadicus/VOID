// VOID
//
// VOID_EditorHUD.cs
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
using System.Text;
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	public class VOID_EditorHUD : VOID_Module, IVOID_EditorModule
	{
		/*
		 * Fields
		 * */
		[AVOID_SaveValue("colorIndex")]
		protected VOID_SaveValue<int> _colorIndex = 0;

		protected List<Color> textColors = new List<Color>();

		protected GUIStyle labelStyle;

		protected EditorVesselOverlays _vesselOverlays;

		/*
		 * Properties
		 * */
		public int ColorIndex
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

		protected EditorVesselOverlays vesselOverlays
		{
			get
			{
				if (this._vesselOverlays == null)
				{
					this._vesselOverlays = (EditorVesselOverlays)Resources
						.FindObjectsOfTypeAll(typeof(EditorVesselOverlays))
						.FirstOrDefault();
				}

				return this._vesselOverlays;
			}
		}

		protected EditorMarker_CoM CoMmarker
		{
			get
			{
				if (this.vesselOverlays == null)
				{
					return null;
				}

				return this.vesselOverlays.CoMmarker;
			}
		}

		protected EditorMarker_CoT CoTmarker
		{
			get
			{
				if (this.vesselOverlays == null)
				{
					return null;
				}

				return this.vesselOverlays.CoTmarker;
			}
		}

		/* 
		 * Methods
		 * */
		public VOID_EditorHUD() : base()
		{
			this._Name = "Heads-Up Display";

			this.toggleActive = true;

			this.textColors.Add(Color.green);
			this.textColors.Add(Color.black);
			this.textColors.Add(Color.white);
			this.textColors.Add(Color.red);
			this.textColors.Add(Color.blue);
			this.textColors.Add(Color.yellow);
			this.textColors.Add(Color.gray);
			this.textColors.Add(Color.cyan);
			this.textColors.Add(Color.magenta);

			this.labelStyle = new GUIStyle ();
			// this.labelStyle.alignment = TextAnchor.UpperRight;
			this.labelStyle.normal.textColor = this.textColors [this.ColorIndex];

			Tools.PostDebugMessage (this.GetType().Name + ": Constructed.");
		}

		public override void DrawGUI()
		{
			SimManager.RequestSimulation();

			if (this.core.LastStage == null)
			{
				return;
			}

			float hudLeft;
			StringBuilder hudString;

			if (EditorLogic.fetch.editorScreen == EditorLogic.EditorScreen.Parts)
			{
				hudLeft = EditorPanels.Instance.partsPanelWidth + 10;
			}
			else if (EditorLogic.fetch.editorScreen == EditorLogic.EditorScreen.Actions)
			{
				hudLeft = EditorPanels.Instance.actionsPanelWidth + 10;
			}
			else
			{
				return;
			}

			GUI.skin = this.core.Skin;

			Rect hudPos = new Rect (hudLeft, 48, 300, 32);

			hudString = new StringBuilder();

			// GUI.skin = AssetBase.GetGUISkin("KSP window 2");

			labelStyle.normal.textColor = textColors [ColorIndex];

			hudString.Append("Total Mass: ");
			hudString.Append(this.core.LastStage.totalMass.ToString("F3"));
			hudString.Append('t');

			hudString.Append(' ');

			hudString.Append("Part Count: ");
			hudString.Append(EditorLogic.SortedShipList.Count);

			hudString.Append('\n');

			hudString.Append("Total Delta-V: ");
			hudString.Append(Tools.MuMech_ToSI(this.core.LastStage.totalDeltaV));
			hudString.Append("m/s");

			hudString.Append('\n');

			hudString.Append("Bottom Stage Delta-V");
			hudString.Append(Tools.MuMech_ToSI(this.core.LastStage.deltaV));
			hudString.Append("m/s");

			hudString.Append('\n');

			hudString.Append("Bottom Stage T/W Ratio: ");
			hudString.Append(this.core.LastStage.thrustToWeight.ToString("F3"));

			if (this.CoMmarker.gameObject.activeInHierarchy && this.CoTmarker.gameObject.activeInHierarchy)
			{
				hudString.Append('\n');

				hudString.Append("Thrust Offset: ");
				hudString.Append(
					Vector3.Cross(
						this.CoTmarker.dirMarkerObject.transform.forward,
						this.CoMmarker.posMarkerObject.transform.position - this.CoTmarker.posMarkerObject.transform.position
					).ToString("F3"));
			}

			GUI.Label (
				hudPos,
				hudString.ToString(),
				labelStyle);
		}

		public override void DrawConfigurables()
		{
			if (GUILayout.Button ("Change HUD color", GUILayout.ExpandWidth (false)))
			{
				++this.ColorIndex;
			}
		}
	}
}
