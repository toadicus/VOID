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

		/* 
		 * Methods
		 * */
		public VOID_EditorHUD() : base()
		{
			this._Name = "Heads-Up Display";

			this._Active = true;

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
			SimManager.Instance.RequestSimulation();

			if (SimManager.Instance.LastStage == null)
			{
				return;
			}

			float hudLeft;

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

			// GUI.skin = AssetBase.GetGUISkin("KSP window 2");

			labelStyle.normal.textColor = textColors [ColorIndex];

			GUI.Label (
				hudPos,
				"Total Mass: " + SimManager.Instance.LastStage.totalMass.ToString("F3") + "t" +
				" Part Count: " + EditorLogic.SortedShipList.Count +
				"\nTotal Delta-V: " + Tools.MuMech_ToSI(SimManager.Instance.LastStage.totalDeltaV) + "m/s" +
				"\nBottom Stage Delta-V: " + Tools.MuMech_ToSI(SimManager.Instance.LastStage.deltaV) + "m/s" +
				"\nBottom Stage T/W Ratio: " + SimManager.Instance.LastStage.thrustToWeight.ToString("F3"),
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
