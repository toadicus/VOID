//
//  VOID_Hud.cs
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
//

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

			this._Active.value = true;

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

			if (SimManager.LastStage == null)
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

			Rect hudPos = new Rect (hudLeft, 48, 300, 32);

			hudString = new StringBuilder();

			// GUI.skin = AssetBase.GetGUISkin("KSP window 2");

			labelStyle.normal.textColor = textColors [ColorIndex];

			hudString.Append("Total Mass: ");
			hudString.Append(SimManager.LastStage.totalMass.ToString("F3"));
			hudString.Append('t');

			hudString.Append(' ');

			hudString.Append("Part Count: ");
			hudString.Append(EditorLogic.SortedShipList.Count);

			hudString.Append('\n');

			hudString.Append("Total Delta-V: ");
			hudString.Append(Tools.MuMech_ToSI(SimManager.LastStage.totalDeltaV));
			hudString.Append("m/s");

			hudString.Append('\n');

			hudString.Append("Bottom Stage Delta-V");
			hudString.Append(Tools.MuMech_ToSI(SimManager.LastStage.deltaV));
			hudString.Append("m/s");

			hudString.Append('\n');

			hudString.Append("Bottom Stage T/W Ratio: ");
			hudString.Append(SimManager.LastStage.thrustToWeight.ToString("F3"));

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
