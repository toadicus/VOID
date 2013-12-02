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
using System.Collections.Generic;
using UnityEngine;

namespace VOID
{
	public class VOID_CBInfoBrowser : VOID_WindowModule
	{
		[AVOID_SaveValue("selectedBodyIdx1")]
		protected VOID_SaveValue<int> selectedBodyIdx1 = 1;

		[AVOID_SaveValue("selectedBodyIdx2")]
		protected VOID_SaveValue<int> selectedBodyIdx2 = 2;

		protected CelestialBody selectedBody1;
		protected CelestialBody selectedBody2;

		[AVOID_SaveValue("toggleOrbital")]
		protected VOID_SaveValue<bool> toggleOrbital = false;

		[AVOID_SaveValue("togglePhysical")]
		protected VOID_SaveValue<bool> togglePhysical = false;

		public VOID_CBInfoBrowser()
		{
			this._Name = "Celestial Body Information Browser";

			this.WindowPos.x = 10;
			this.WindowPos.y = 85;
		}

		public override void ModuleWindow(int _)
		{
			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

			GUILayout.BeginVertical(GUILayout.Width(150));
			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("", GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();

			GUILayout.BeginVertical(GUILayout.Width(150));

			selectedBody1 = VOID_Core.Instance.allBodies[selectedBodyIdx1];
			selectedBody2 = VOID_Core.Instance.allBodies[selectedBodyIdx2];

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			if (GUILayout.Button("<", GUILayout.ExpandWidth(false)))
			{
				selectedBodyIdx1--;
				if (selectedBodyIdx1 < 0) selectedBodyIdx1 = VOID_Core.Instance.allBodies.Count - 1;
			}
			GUILayout.Label(VOID_Core.Instance.allBodies[selectedBodyIdx1].bodyName, VOID_Core.Instance.LabelStyles["center_bold"], GUILayout.ExpandWidth(true));
			if (GUILayout.Button(">", GUILayout.ExpandWidth(false)))
			{
				selectedBodyIdx1++;
				if (selectedBodyIdx1 > VOID_Core.Instance.allBodies.Count - 1) selectedBodyIdx1 = 0;
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUILayout.BeginVertical(GUILayout.Width(150));
			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			if (GUILayout.Button("<", GUILayout.ExpandWidth(false)))
			{
				selectedBodyIdx2--;
				if (selectedBodyIdx2 < 0) selectedBodyIdx2 = VOID_Core.Instance.allBodies.Count - 1;
			}
			GUILayout.Label(VOID_Core.Instance.allBodies[selectedBodyIdx2].bodyName, VOID_Core.Instance.LabelStyles["center_bold"], GUILayout.ExpandWidth(true));
			if (GUILayout.Button(">", GUILayout.ExpandWidth(false)))
			{
				selectedBodyIdx2++;
				if (selectedBodyIdx2 > VOID_Core.Instance.allBodies.Count - 1) selectedBodyIdx2 = 0;
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();

			//}

			//toggle for orbital info chunk
			if (GUILayout.Button("Orbital Characteristics", GUILayout.ExpandWidth(true))) toggleOrbital.value = !toggleOrbital;

			if (toggleOrbital)
			{
				//begin orbital into horizontal chunk
				//print("begin orbital info section...");
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

				//begin orbital value labels column
				GUILayout.BeginVertical(GUILayout.Width(150));

				//print("printing row labels...");

				GUILayout.Label("Apoapsis:");
				GUILayout.Label("Time to Ap:");
				GUILayout.Label("Periapsis:");
				GUILayout.Label("Time to Pe:");
				GUILayout.Label("Semi-major axis:");
				GUILayout.Label("Eccentricity:");
				GUILayout.Label("Orbital period:");
				GUILayout.Label("Rotational period:");
				GUILayout.Label("Velocity:");
				GUILayout.Label("Mean anomaly:");
				GUILayout.Label("True anomaly:");
				GUILayout.Label("Eccentric anomaly:");
				GUILayout.Label("Inclination:");
				GUILayout.Label("Long. ascending node:");
				GUILayout.Label("Arg. of periapsis:");
				GUILayout.Label("Tidally locked:");

				//end orbital value labels column
				GUILayout.EndVertical();

				//begin primary orbital values column
				GUILayout.BeginVertical(GUILayout.Width(150));

				body_OP_show_orbital_info(selectedBody1);

				//end primary orbital values column
				GUILayout.EndVertical();

				//begin secondary orbital values column
				GUILayout.BeginVertical(GUILayout.Width(150));

				body_OP_show_orbital_info(selectedBody2);

				//end secondary orbital values column
				GUILayout.EndVertical();

				//end orbital info horizontal chunk
				GUILayout.EndHorizontal();
			}

			//toggle for physical info chunk
			if (GUILayout.Button("Physical Characteristics", GUILayout.ExpandWidth(true))) togglePhysical.value = !togglePhysical;

			if (togglePhysical)
			{
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

				//begin physical info value label column
				GUILayout.BeginVertical(GUILayout.Width(150));

				GUILayout.Label("Radius:");
				GUILayout.Label("Surface area:");
				GUILayout.Label("Volume:");
				GUILayout.Label("Mass:");
				GUILayout.Label("Density:");
				GUILayout.Label("Sphere of influence:");
				GUILayout.Label("Natural satellites:");
				GUILayout.Label("Artificial satellites:");
				GUILayout.Label("Surface gravity:");
				GUILayout.Label("Atmosphere altitude:");
				GUILayout.Label("Atmospheric O\u2082:");
				GUILayout.Label("Has ocean:");

				//end physical info value label column
				GUILayout.EndVertical();

				//begin primary physical values column
				GUILayout.BeginVertical(GUILayout.Width(150));

				body_OP_show_physical_info(selectedBody1);

				//end primary physical column
				GUILayout.EndVertical();

				//begin secondary physical values column
				GUILayout.BeginVertical(GUILayout.Width(150));

				body_OP_show_physical_info(selectedBody2);

				//end target physical values column
				GUILayout.EndVertical();

				//end physical value horizontal chunk
				GUILayout.EndHorizontal();
			}

			GUI.DragWindow();
		}

		private void body_OP_show_orbital_info(CelestialBody body)
		{
		    //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		    if (body.bodyName == "Sun") GUILayout.Label("N/A", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label((body.orbit.ApA / 1000).ToString("##,#") + "km", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    //GUILayout.EndHorizontal();

		    //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		    if (body.bodyName == "Sun") GUILayout.Label("N/A", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label(Tools.ConvertInterval(body.orbit.timeToAp), VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    //GUILayout.EndHorizontal();

		    //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		    if (body.bodyName == "Sun") GUILayout.Label("N/A", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label((body.orbit.PeA / 1000).ToString("##,#") + "km", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    //GUILayout.EndHorizontal();

		    //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		    if (body.bodyName == "Sun") GUILayout.Label("N/A", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label(Tools.ConvertInterval(body.orbit.timeToPe), VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    //GUILayout.EndHorizontal();

		    //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		    if (body.bodyName == "Sun") GUILayout.Label("N/A", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label((body.orbit.semiMajorAxis / 1000).ToString("##,#") + "km", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    //GUILayout.EndHorizontal();

		    //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		    if (body.bodyName == "Sun") GUILayout.Label("N/A", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label(body.orbit.eccentricity.ToString("F4") + "", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    //GUILayout.EndHorizontal();

		    //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		    if (body.bodyName == "Sun") GUILayout.Label("N/A", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label(Tools.ConvertInterval(body.orbit.period), VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    //GUILayout.EndHorizontal();

		    //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		    if (body.bodyName == "Sun") GUILayout.Label("N/A", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label(Tools.ConvertInterval(body.rotationPeriod), VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    //GUILayout.EndHorizontal();

		    //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		    if (body.bodyName == "Sun") GUILayout.Label("N/A", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label((body.orbit.orbitalSpeed / 1000).ToString("F2") + "km/s", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    //GUILayout.EndHorizontal();

		    //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			// Toadicus edit: convert mean anomaly into degrees.
		    if (body.bodyName == "Sun") GUILayout.Label("N/A", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label((body.orbit.meanAnomaly * 180d / Math.PI).ToString("F3") + "°", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    //GUILayout.EndHorizontal();

		    //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		    if (body.bodyName == "Sun") GUILayout.Label("N/A", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label(body.orbit.trueAnomaly.ToString("F3") + "°", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    //GUILayout.EndHorizontal();

		    //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			// Toadicus edit: convert eccentric anomaly into degrees.
		    if (body.bodyName == "Sun") GUILayout.Label("N/A", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label((body.orbit.eccentricAnomaly * 180d / Math.PI).ToString("F3") + "°", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    //GUILayout.EndHorizontal();

		    //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		    if (body.bodyName == "Sun") GUILayout.Label("N/A", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label(body.orbit.inclination.ToString("F3") + "°", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    //GUILayout.EndHorizontal();

		    //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		    if (body.bodyName == "Sun") GUILayout.Label("N/A", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label(body.orbit.LAN.ToString("F3") + "°", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    //GUILayout.EndHorizontal();

		    //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		    if (body.bodyName == "Sun") GUILayout.Label("N/A", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else GUILayout.Label(body.orbit.argumentOfPeriapsis.ToString("F3") + "°", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    //GUILayout.EndHorizontal();

		    //GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		    if (body.bodyName == "Sun") GUILayout.Label("N/A", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    else
		    {
		        string body_tidally_locked = "No";
		        if (body.tidallyLocked) body_tidally_locked = "Yes";
		        GUILayout.Label(body_tidally_locked, VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
		    }
		    //GUILayout.EndHorizontal();
		}

		private void body_OP_show_physical_info(CelestialBody body)
		{
			//GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label((body.Radius / 1000).ToString("##,#") + "km", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
			//GUILayout.EndHorizontal();

			//GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(((Math.Pow((body.Radius), 2) * 4 * Math.PI) / 1000).ToString("0.00e+00") + "km²", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
			//GUILayout.EndHorizontal();

			//GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			// divide by 1000 to convert m to km
			GUILayout.Label((((4d / 3) * Math.PI * Math.Pow(body.Radius, 3)) / 1000).ToString("0.00e+00") + "km³", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
			//GUILayout.Label(((4 / 3) * Math.PI * Math.Pow((vessel.mainBody.Radius / 1000), 3)).ToString(), right, GUILayout.ExpandWidth(true));
			//GUILayout.EndHorizontal();

			//GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(body.Mass.ToString("0.00e+00") + "kg", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
			//GUILayout.EndHorizontal();

			double p = body.Mass / (Math.Pow(body.Radius, 3) * (4d / 3) * Math.PI);
			//GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(p.ToString("##,#") + "kg/m³", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
			//GUILayout.EndHorizontal();

			//GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			if (body.bodyName == "Sun") GUILayout.Label(Tools.MuMech_ToSI(body.sphereOfInfluence), VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
			else GUILayout.Label(Tools.MuMech_ToSI(body.sphereOfInfluence), VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
			//GUILayout.EndHorizontal();

			//GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(body.orbitingBodies.Count.ToString(), VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
			//GUILayout.EndHorizontal();

			//show # artificial satellites
			int num_art_sats = 0;
			foreach (Vessel v in FlightGlobals.Vessels)
			{
				if (v.mainBody == body && v.situation.ToString() == "ORBITING") num_art_sats++;
			}

			//GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(num_art_sats.ToString(), VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
			//GUILayout.EndHorizontal();

			double g_ASL = (VOID_Core.Constant_G * body.Mass) / Math.Pow(body.Radius, 2);
			//GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(Tools.MuMech_ToSI(g_ASL) + "m/s²", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
			//GUILayout.EndHorizontal();

			//GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("≈ " + Tools.MuMech_ToSI(body.maxAtmosphereAltitude) + "m", VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
			//GUILayout.EndHorizontal();

			//GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			string O2 = "No";
			if (body.atmosphereContainsOxygen == true) O2 = "Yes";
			GUILayout.Label(O2, VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
			//GUILayout.EndHorizontal();

			//GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			string ocean = "No";
			if (body.ocean == true) ocean = "Yes";
			GUILayout.Label(ocean, VOID_Core.Instance.LabelStyles["right"], GUILayout.ExpandWidth(true));
			//GUILayout.EndHorizontal();
		}
	}
}