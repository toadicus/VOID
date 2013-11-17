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

		[AVOID_SaveValue("precisionValues")]
		protected IntCollection precisionValues = new IntCollection(4, 230584300921369395);

		protected double orbitAltitude;
		protected double orbitVelocity;
		protected double orbitApoAlt;
		protected double oribtPeriAlt;
		protected string timeToApo;
		protected string timeToPeri;
		protected double orbitInclination;
		protected double gravityAccel;
		protected string orbitPeriod;
		protected double semiMajorAxis;
		protected double eccentricity;
		protected double meanAnomaly;
		protected double trueAnomaly;
		protected double eccAnomaly;
		protected double longitudeAscNode;
		protected double argumentPeriapsis;
		protected double localSiderealLongitude;

		public VOID_Orbital()
		{
			this._Name = "Orbital Information";

			this.WindowPos.x = Screen.width - 520f;
			this.WindowPos.y = 250f;
		}

		public override void ModuleWindow(int _)
		{
			if (VOID_Core.Instance.updateTimer >= this.lastUpdate + VOID_Core.Instance.updatePeriod)
			{
				this.lastUpdate = VOID_Core.Instance.updateTimer;

				this.orbitAltitude = vessel.orbit.altitude;
				this.orbitVelocity = vessel.orbit.vel.magnitude;
				this.orbitApoAlt = vessel.orbit.ApA;
				this.oribtPeriAlt = vessel.orbit.PeA;
				this.timeToApo = Tools.ConvertInterval(vessel.orbit.timeToAp);
				this.timeToPeri = Tools.ConvertInterval(vessel.orbit.timeToPe);
				this.orbitInclination = vessel.orbit.inclination;

				double orbitRadius = vessel.mainBody.Radius + vessel.mainBody.GetAltitude(vessel.findWorldCenterOfMass());
				this.gravityAccel = (VOID_Core.Constant_G * vessel.mainBody.Mass) / Math.Pow(orbitRadius, 2);

				this.orbitPeriod = Tools.ConvertInterval(vessel.orbit.period);
				this.semiMajorAxis = vessel.orbit.semiMajorAxis;
				this.eccentricity = vessel.orbit.eccentricity;
				this.meanAnomaly = vessel.orbit.meanAnomaly * 180d / Math.PI;
				this.trueAnomaly = vessel.orbit.trueAnomaly;
				this.eccAnomaly = vessel.orbit.eccentricAnomaly * 180d / Math.PI;
				this.longitudeAscNode = vessel.orbit.LAN;
				this.argumentPeriapsis = vessel.orbit.argumentOfPeriapsis;
				this.localSiderealLongitude =
					Tools.FixDegreeDomain(vessel.longitude + vessel.orbit.referenceBody.rotationAngle);
			}

			int idx = 0;

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label(VOIDLabels.void_primary + ":");
			GUILayout.FlexibleSpace();
            GUILayout.Label(vessel.mainBody.bodyName, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(VOIDLabels.void_altitude_asl + ":", GUILayout.ExpandWidth(true));
			GUILayout.FlexibleSpace();
			GUILayout.Label(
				Tools.MuMech_ToSI(this.orbitAltitude, this.precisionValues[idx]) + "m",
				GUILayout.ExpandWidth(false)
				);
			if (GUILayout.Button ("P")) {
				this.precisionValues [idx] = (ushort)((this.precisionValues[idx] + 3) % 15);
			}
            GUILayout.EndHorizontal();
			idx++;

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(VOIDLabels.void_velocity + ":");
			GUILayout.FlexibleSpace();
			GUILayout.Label(Tools.MuMech_ToSI(this.orbitVelocity, this.precisionValues [idx]) + "m/s", GUILayout.ExpandWidth(false));
			
			if (GUILayout.Button ("P")) {
				this.precisionValues [idx] = (ushort)((this.precisionValues[idx] + 3) % 15);
			}
            GUILayout.EndHorizontal();
			idx++;

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(VOIDLabels.void_apoapsis + ":");
			GUILayout.FlexibleSpace();
			GUILayout.Label(Tools.MuMech_ToSI(this.orbitApoAlt, this.precisionValues [idx]) + "m", GUILayout.ExpandWidth(false));
			
			if (GUILayout.Button ("P")) {
				this.precisionValues [idx] = (ushort)((this.precisionValues[idx] + 3) % 15);
			}
            GUILayout.EndHorizontal();
			idx++;

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Time to Ap:");
			GUILayout.FlexibleSpace();
			GUILayout.Label(this.timeToApo, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(VOIDLabels.void_periapsis + ":");
			GUILayout.FlexibleSpace();
			GUILayout.Label(
				Tools.MuMech_ToSI(this.oribtPeriAlt, this.precisionValues [idx]) + "m",
				GUILayout.ExpandWidth(false)
				);
			
			if (GUILayout.Button ("P")) {
				this.precisionValues [idx] = (ushort)((this.precisionValues[idx] + 3) % 15);
			}
            GUILayout.EndHorizontal();
			idx++;

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Time to Pe:");
			GUILayout.FlexibleSpace();
            GUILayout.Label(this.timeToPeri, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Inclination:");
			GUILayout.FlexibleSpace();
            GUILayout.Label(this.orbitInclination.ToString("F3") + "°", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Gravity:");
			GUILayout.FlexibleSpace();
			GUILayout.Label(
				Tools.MuMech_ToSI(gravityAccel, this.precisionValues[idx]) + "m/s²",
				GUILayout.ExpandWidth(false)
				);
			
			if (GUILayout.Button ("P")) {
				this.precisionValues [idx] = (ushort)((this.precisionValues[idx] + 3) % 15);
			}
            GUILayout.EndHorizontal();
			idx++;

			this.toggleExtended = GUILayout.Toggle(this.toggleExtended, "Extended info");

			if (this.toggleExtended)
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.Label("Period:");
				GUILayout.FlexibleSpace();
                GUILayout.Label(this.orbitPeriod, GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.Label("Semi-major axis:");
				GUILayout.FlexibleSpace();
				GUILayout.Label(
					Tools.MuMech_ToSI(this.semiMajorAxis, this.precisionValues [idx]) + "m",
					GUILayout.ExpandWidth(false)
					);
				
				if (GUILayout.Button ("P")) {
					this.precisionValues [idx] = (ushort)((this.precisionValues[idx] + 3) % 15);
				}
                GUILayout.EndHorizontal();
				idx++;

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.Label("Eccentricity:");
				GUILayout.FlexibleSpace();
				GUILayout.Label(this.eccentricity.ToString("F4"), GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

				// Toadicus edit: convert mean anomaly into degrees.
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.Label("Mean anomaly:");
				GUILayout.FlexibleSpace();
                GUILayout.Label(this.meanAnomaly.ToString("F3") + "°", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.Label("True anomaly:");
				GUILayout.FlexibleSpace();
				GUILayout.Label(this.trueAnomaly.ToString("F3") + "°", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

				// Toadicus edit: convert eccentric anomaly into degrees.
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.Label("Eccentric anomaly:");
				GUILayout.FlexibleSpace();
                GUILayout.Label(this.eccAnomaly.ToString("F3") + "°", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.Label("Long. ascending node:");
				GUILayout.FlexibleSpace();
                GUILayout.Label(this.longitudeAscNode.ToString("F3") + "°", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.Label("Arg. of periapsis:");
				GUILayout.FlexibleSpace();
                GUILayout.Label(this.argumentPeriapsis.ToString("F3") + "°", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

				// Toadicus edit: added local sidereal longitude.
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.Label("Local Sidereal Longitude:");
				GUILayout.FlexibleSpace();
				GUILayout.Label(this.localSiderealLongitude.ToString("F3") + "°", VOID_Core.Instance.LabelStyles["right"]);
				GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
		}
	}
}

