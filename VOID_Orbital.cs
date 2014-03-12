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
		protected long _precisionValues = 230584300921369395;
		protected IntCollection precisionValues;

		public VOID_Orbital()
		{
			this._Name = "Orbital Information";

			this.WindowPos.x = Screen.width - 520f;
			this.WindowPos.y = 250f;
		}

		public override void ModuleWindow(int _)
		{
			base.ModuleWindow (_);

			int idx = 0;

            GUILayout.BeginVertical();

			VOID_Data.primaryName.DoGUIHorizontal ();

			this.precisionValues [idx]= (ushort)VOID_Data.orbitAltitude.DoGUIHorizontal (this.precisionValues [idx]);
			idx++;

			this.precisionValues [idx]= (ushort)VOID_Data.orbitVelocity.DoGUIHorizontal (this.precisionValues [idx]);
			idx++;

			this.precisionValues [idx]= (ushort)VOID_Data.orbitApoAlt.DoGUIHorizontal (this.precisionValues [idx]);
			idx++;

			VOID_Data.timeToApo.DoGUIHorizontal();

			this.precisionValues [idx]= (ushort)VOID_Data.oribtPeriAlt.DoGUIHorizontal (this.precisionValues [idx]);
			idx++;

			VOID_Data.timeToPeri.DoGUIHorizontal();

			VOID_Data.orbitInclination.DoGUIHorizontal("F3");

			this.precisionValues [idx]= (ushort)VOID_Data.gravityAccel.DoGUIHorizontal (this.precisionValues [idx]);
			idx++;

			this.toggleExtended.value = GUILayout.Toggle(this.toggleExtended, "Extended info");

			if (this.toggleExtended)
            {
				VOID_Data.orbitPeriod.DoGUIHorizontal();

				this.precisionValues [idx]= (ushort)VOID_Data.semiMajorAxis.DoGUIHorizontal (this.precisionValues [idx]);
				idx++;

				VOID_Data.eccentricity.DoGUIHorizontal("F4");

				VOID_Data.meanAnomaly.DoGUIHorizontal("F3");

				VOID_Data.trueAnomaly.DoGUIHorizontal("F3");

				VOID_Data.eccAnomaly.DoGUIHorizontal("F3");

				VOID_Data.longitudeAscNode.DoGUIHorizontal("F3");

				VOID_Data.argumentPeriapsis.DoGUIHorizontal("F3");

				VOID_Data.localSiderealLongitude.DoGUIHorizontal("F3");
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
		}

		public override void LoadConfig ()
		{
			base.LoadConfig ();

			this.precisionValues = new IntCollection (4, this._precisionValues);
		}

		public override void _SaveToConfig (KSP.IO.PluginConfiguration config)
		{
			this._precisionValues = this.precisionValues.collection;

			base._SaveToConfig (config);
		}
	}


	public static partial class VOID_Data
	{
		public static VOID_StrValue primaryName = new VOID_StrValue (
			VOIDLabels.void_primary,
			new Func<string> (() => VOID_Core.Instance.vessel.mainBody.name)
		);

		public static VOID_DoubleValue orbitAltitude = new VOID_DoubleValue (
			"Altitude (ASL)",
			new Func<double> (() => VOID_Core.Instance.vessel.orbit.altitude),
			"m"
		);

		public static VOID_DoubleValue orbitVelocity = new VOID_DoubleValue (
			VOIDLabels.void_velocity,
			new Func<double> (() => VOID_Core.Instance.vessel.orbit.vel.magnitude),
			"m/s"
		);

		public static VOID_DoubleValue orbitApoAlt = new VOID_DoubleValue(
			VOIDLabels.void_apoapsis,
			new Func<double>(() => VOID_Core.Instance.vessel.orbit.ApA),
			"m"
		);

		public static VOID_DoubleValue oribtPeriAlt = new VOID_DoubleValue(
			VOIDLabels.void_periapsis,
			new Func<double>(() => VOID_Core.Instance.vessel.orbit.PeA),
			"m"
		);

		public static VOID_StrValue timeToApo = new VOID_StrValue(
			"Time to Apoapsis",
			new Func<string>(() => Tools.ConvertInterval(VOID_Core.Instance.vessel.orbit.timeToAp))
		);

		public static VOID_StrValue timeToPeri = new VOID_StrValue(
			"Time to Periapsis",
			new Func<string>(() => Tools.ConvertInterval(VOID_Core.Instance.vessel.orbit.timeToPe))
		);

		public static VOID_DoubleValue orbitInclination = new VOID_DoubleValue(
			"Inclination",
			new Func<double>(() => VOID_Core.Instance.vessel.orbit.inclination),
			"°"
		);

		public static VOID_DoubleValue gravityAccel = new VOID_DoubleValue(
			"Gravity",
			delegate()
		{
			double orbitRadius = VOID_Core.Instance.vessel.mainBody.Radius +
				VOID_Core.Instance.vessel.mainBody.GetAltitude(VOID_Core.Instance.vessel.findWorldCenterOfMass());
			return (VOID_Core.Constant_G * VOID_Core.Instance.vessel.mainBody.Mass) /
				Math.Pow(orbitRadius, 2);
		},
			"m/s²"
		);

		public static VOID_StrValue orbitPeriod = new VOID_StrValue(
			"Period",
			new Func<string>(() => Tools.ConvertInterval(VOID_Core.Instance.vessel.orbit.period))
		);

		public static VOID_DoubleValue semiMajorAxis = new VOID_DoubleValue(
			"Semi-Major Axis",
			new Func<double>(() => VOID_Core.Instance.vessel.orbit.semiMajorAxis),
			"m"
		);

		public static VOID_DoubleValue eccentricity = new VOID_DoubleValue(
			"Eccentricity",
			new Func<double>(() => VOID_Core.Instance.vessel.orbit.eccentricity),
			""
		);

		public static VOID_DoubleValue meanAnomaly = new VOID_DoubleValue(
			"Mean Anomaly",
			new Func<double>(() => VOID_Core.Instance.vessel.orbit.meanAnomaly * 180d / Math.PI),
			"°"
		);

		public static VOID_DoubleValue trueAnomaly = new VOID_DoubleValue(
			"True Anomaly",
			new Func<double>(() => VOID_Core.Instance.vessel.orbit.trueAnomaly),
			"°"
		);

		public static VOID_DoubleValue eccAnomaly = new VOID_DoubleValue(
			"Eccentric Anomaly",
			new Func<double>(() => VOID_Core.Instance.vessel.orbit.eccentricAnomaly * 180d / Math.PI),
			"°"
		);

		public static VOID_DoubleValue longitudeAscNode = new VOID_DoubleValue(
			"Long. Ascending Node",
			new Func<double>(() => VOID_Core.Instance.vessel.orbit.LAN),
			"°"
		);

		public static VOID_DoubleValue argumentPeriapsis = new VOID_DoubleValue(
			"Argument of Periapsis",
			new Func<double>(() => VOID_Core.Instance.vessel.orbit.argumentOfPeriapsis),
			"°"
		);

		public static VOID_DoubleValue localSiderealLongitude = new VOID_DoubleValue(
			"Local Sidereal Longitude",
			new Func<double>(() => Tools.FixDegreeDomain(
				VOID_Core.Instance.vessel.longitude + VOID_Core.Instance.vessel.orbit.referenceBody.rotationAngle)),
			"°"
		);
	}
}

