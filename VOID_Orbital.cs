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
using UnityEngine;

namespace VOID
{
	public class VOID_Orbital : VOID_WindowModule
	{
		[AVOID_SaveValue("toggleExtended")]
		protected VOID_SaveValue<bool> toggleExtended = false;

		public VOID_Orbital()
		{
			this._Name = "Orbital Information";
		}

		public override void ModuleWindow(int _)
		{
			// Toadicus edit: added local sidereal longitude.
			double LSL = vessel.longitude + vessel.orbit.referenceBody.rotationAngle;
			LSL = Tools.FixDegreeDomain (LSL);

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label(VOIDLabels.void_primary + ":");
            GUILayout.Label(vessel.mainBody.bodyName, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(VOIDLabels.void_altitude_asl + ":");
			GUILayout.Label(Tools.MuMech_ToSI(vessel.orbit.altitude) + "m", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(VOIDLabels.void_velocity + ":");
			GUILayout.Label(Tools.MuMech_ToSI(vessel.orbit.vel.magnitude) + "m/s", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(VOIDLabels.void_apoapsis + ":");
			GUILayout.Label(Tools.MuMech_ToSI(vessel.orbit.ApA) + "m", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Time to Ap:");
            GUILayout.Label(Tools.ConvertInterval(vessel.orbit.timeToAp), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(VOIDLabels.void_periapsis + ":");
			GUILayout.Label(Tools.MuMech_ToSI(vessel.orbit.PeA) + "m", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Time to Pe:");
            GUILayout.Label(Tools.ConvertInterval(vessel.orbit.timeToPe), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Inclination:");
            GUILayout.Label(vessel.orbit.inclination.ToString("F3") + "°", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            double r_vessel = vessel.mainBody.Radius + vessel.mainBody.GetAltitude(vessel.findWorldCenterOfMass());
            double g_vessel = (VOID_Core.Constant_G * vessel.mainBody.Mass) / Math.Pow(r_vessel, 2);
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("Gravity:");
			GUILayout.Label(Tools.MuMech_ToSI(g_vessel) + "m/s²", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

			this.toggleExtended = GUILayout.Toggle(this.toggleExtended, "Extended info");

			if (this.toggleExtended)
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("Period:");
                GUILayout.Label(Tools.ConvertInterval(vessel.orbit.period), GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("Semi-major axis:");
                GUILayout.Label((vessel.orbit.semiMajorAxis / 1000).ToString("##,#") + "km", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("Eccentricity:");
                GUILayout.Label(vessel.orbit.eccentricity.ToString("F4"), GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

				// Toadicus edit: convert mean anomaly into degrees.
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("Mean anomaly:");
                GUILayout.Label((vessel.orbit.meanAnomaly * 180d / Math.PI).ToString("F3") + "°", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("True anomaly:");
                GUILayout.Label(vessel.orbit.trueAnomaly.ToString("F3") + "°", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

				// Toadicus edit: convert eccentric anomaly into degrees.
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("Eccentric anomaly:");
                GUILayout.Label((vessel.orbit.eccentricAnomaly * 180d / Math.PI).ToString("F3") + "°", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("Long. ascending node:");
                GUILayout.Label(vessel.orbit.LAN.ToString("F3") + "°", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                GUILayout.Label("Arg. of periapsis:");
                GUILayout.Label(vessel.orbit.argumentOfPeriapsis.ToString("F3") + "°", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

				// Toadicus edit: added local sidereal longitude.
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.Label("Local Sidereal Longitude:");
				GUILayout.Label(LSL.ToString("F3") + "°", VOID_Core.Instance.LabelStyles["txt_right"]);
				GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
		}
	}
}

