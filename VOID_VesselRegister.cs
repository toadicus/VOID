//
//  VOID_Orbital.cs
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
using KSP;
using System;
using System.Linq;
using UnityEngine;

namespace VOID
{
	public class VOID_VesselRegister : VOID_WindowModule
	{
		[AVOID_SaveValue("selectedBodyIdx")]
		protected VOID_SaveValue<int> selectedBodyIdx = 0;
		protected CelestialBody seletedBody;

		[AVOID_SaveValue("selectedVesselTypeIdx")]
		protected VOID_SaveValue<int> selectedVesselTypeIdx = 0;
		protected VesselType selectedVesselType;

		protected string vesselSituation = "Orbiting";

		protected Vector2 selectorScrollPos = new Vector2();

		protected Vessel _selectedVessel;

		public Vessel selectedVessel
		{
			get
			{
				return this._selectedVessel;
			}
		}

		public VOID_VesselRegister()
		{
			this._Name = "Vessel Register";

			this.WindowPos.x = 845;
			this.WindowPos.y = 275;
			this.defHeight = 375;
		}

		public override void ModuleWindow(int _)
		{
			if (!VOID_Core.Instance.allVesselTypes.Any())
			{
				return;
			}

			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			if (GUILayout.Button("<"))
			{
				selectedBodyIdx--;
				if (selectedBodyIdx < 0) selectedBodyIdx = VOID_Core.Instance.allBodies.Count - 1;
			}
			GUILayout.Label(VOID_Core.Instance.allBodies[selectedBodyIdx].bodyName, VOID_Core.Instance.LabelStyles["center_bold"], GUILayout.ExpandWidth(true));
			if (GUILayout.Button(">"))
			{
				selectedBodyIdx++;
				if (selectedBodyIdx > VOID_Core.Instance.allBodies.Count - 1) selectedBodyIdx = 0;
			}
			GUILayout.EndHorizontal();

			seletedBody = VOID_Core.Instance.allBodies[selectedBodyIdx];

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			if (GUILayout.Button("<"))
			{
				selectedVesselTypeIdx--;
				if (selectedVesselTypeIdx < 0) selectedVesselTypeIdx = VOID_Core.Instance.allVesselTypes.Count - 1;
			}
			GUILayout.Label(VOID_Core.Instance.allVesselTypes[selectedVesselTypeIdx].ToString(), VOID_Core.Instance.LabelStyles["center_bold"], GUILayout.ExpandWidth(true));
			if (GUILayout.Button(">"))
			{
				selectedVesselTypeIdx++;
				if (selectedVesselTypeIdx > VOID_Core.Instance.allVesselTypes.Count - 1) selectedVesselTypeIdx = 0;
			}
			GUILayout.EndHorizontal();

			selectedVesselType = VOID_Core.Instance.allVesselTypes[selectedVesselTypeIdx];

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			if (GUILayout.Button("Landed", GUILayout.ExpandWidth(true))) vesselSituation = "Landed";
			if (GUILayout.Button("Orbiting", GUILayout.ExpandWidth(true))) vesselSituation = "Orbiting";
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(
				Tools.UppercaseFirst(vesselSituation) + " " + selectedVesselType.ToString() + "s  @ " + seletedBody.bodyName,
				VOID_Core.Instance.LabelStyles["center"],
				GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();

			selectorScrollPos = GUILayout.BeginScrollView(selectorScrollPos, false, false);

			foreach (Vessel v in FlightGlobals.Vessels)
			{
				if (v != vessel && v.vesselType == selectedVesselType && v.mainBody == seletedBody)
				{
					if ((vesselSituation == "Landed" &&
					     (v.situation == Vessel.Situations.LANDED ||
					 v.situation == Vessel.Situations.PRELAUNCH ||
					 v.situation == Vessel.Situations.SPLASHED)) ||
					    (vesselSituation == "Orbiting" &&
					 (v.situation == Vessel.Situations.ESCAPING ||
					 v.situation == Vessel.Situations.FLYING ||
					 v.situation == Vessel.Situations.ORBITING ||
					 v.situation == Vessel.Situations.SUB_ORBITAL))
					    )
					{
						if (GUILayout.Button(v.vesselName, GUILayout.ExpandWidth(true)))
						{
							if (_selectedVessel != v)
							{
								_selectedVessel = v; //set clicked vessel as selected_vessel
								this._Active.value = true;    //turn bool on to open the window if closed
							}
							else
							{
								_selectedVessel = null;
							}
						}
					}
				}
			}

			GUILayout.EndScrollView();

			GUILayout.EndVertical();

			GUI.DragWindow();
		}
	}
}