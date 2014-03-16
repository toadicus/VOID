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
	public class VOID_SurfAtmo : VOID_WindowModule
	{
		[AVOID_SaveValue("precisionValues")]
		protected long _precisionValues = 230584300921369395;
		protected IntCollection precisionValues;

		public VOID_SurfAtmo()
		{
			this._Name = "Surface & Atmospheric Information";

			this.WindowPos.x = Screen.width - 260f;
			this.WindowPos.y = 85;
		}

		public override void ModuleWindow(int _)
		{
			base.ModuleWindow (_);

			int idx = 0;

			GUILayout.BeginVertical();

			this.precisionValues [idx]= (ushort)VOID_Data.trueAltitude.DoGUIHorizontal (this.precisionValues [idx]);
			idx++;

			VOID_Data.surfLatitude.DoGUIHorizontal ();

			VOID_Data.surfLongitude.DoGUIHorizontal ();

			VOID_Data.vesselHeading.DoGUIHorizontal ();

			this.precisionValues [idx]= (ushort)VOID_Data.terrainElevation.DoGUIHorizontal (this.precisionValues [idx]);
			idx++;

			this.precisionValues [idx]= (ushort)VOID_Data.surfVelocity.DoGUIHorizontal (this.precisionValues [idx]);
			idx++;

			this.precisionValues [idx]= (ushort)VOID_Data.vertVelocity.DoGUIHorizontal (this.precisionValues [idx]);
			idx++;

			this.precisionValues [idx]= (ushort)VOID_Data.horzVelocity.DoGUIHorizontal (this.precisionValues [idx]);
			idx++;

			VOID_Data.temperature.DoGUIHorizontal ("F2");

			VOID_Data.atmDensity.DoGUIHorizontal (3);

			VOID_Data.atmPressure.DoGUIHorizontal ("F2");

			this.precisionValues [idx]= (ushort)VOID_Data.atmLimit.DoGUIHorizontal (this.precisionValues [idx]);
			idx++;

			// Toadicus edit: added Biome
			VOID_Data.currBiome.DoGUIHorizontal ();

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
		public static VOID_DoubleValue trueAltitude = new VOID_DoubleValue(
			"Altitude (true)",
			delegate()
			{
				double alt_true = VOID_Core.Instance.vessel.orbit.altitude - VOID_Core.Instance.vessel.terrainAltitude;
				// HACK: This assumes that on worlds with oceans, all water is fixed at 0 m,
				// and water covers the whole surface at 0 m.
				if (VOID_Core.Instance.vessel.terrainAltitude < 0 && VOID_Core.Instance.vessel.mainBody.ocean )
					alt_true = VOID_Core.Instance.vessel.orbit.altitude;
				return alt_true;
			},
			"m"
		);

		public static VOID_StrValue surfLatitude = new VOID_StrValue(
			"Latitude",
			new Func<string> (() => Tools.GetLatitudeString(VOID_Core.Instance.vessel))
		);

		public static VOID_StrValue surfLongitude = new VOID_StrValue(
			"Longitude",
			new Func<string> (() => Tools.GetLongitudeString(VOID_Core.Instance.vessel))
		);

		public static VOID_StrValue vesselHeading = new VOID_StrValue(
			"Heading",
			delegate()
		{
			double heading = core.vessel.getSurfaceHeading();
			string cardinal = Tools.get_heading_text(heading);

			return string.Format(
				"{0}° {1}",
				heading.ToString("F2"),
				cardinal
			);
		}
		);

		public static VOID_DoubleValue terrainElevation = new VOID_DoubleValue(
			"Terrain elevation",
			new Func<double> (() => VOID_Core.Instance.vessel.terrainAltitude),
			"m"
		);

		public static VOID_DoubleValue surfVelocity = new VOID_DoubleValue(
			"Surface velocity",
			new Func<double> (() => VOID_Core.Instance.vessel.srf_velocity.magnitude),
			"m/s"
		);

		public static VOID_DoubleValue vertVelocity = new VOID_DoubleValue(
			"Vertical speed",
			new Func<double> (() => VOID_Core.Instance.vessel.verticalSpeed),
			"m/s"
		);

		public static VOID_DoubleValue horzVelocity = new VOID_DoubleValue(
			"Horizontal speed",
			new Func<double> (() => VOID_Core.Instance.vessel.horizontalSrfSpeed),
			"m/s"
		);

		public static VOID_FloatValue temperature = new VOID_FloatValue(
			"Temperature",
			new Func<float> (() => VOID_Core.Instance.vessel.flightIntegrator.getExternalTemperature()),
			"°C"
		);

		public static VOID_DoubleValue atmDensity = new VOID_DoubleValue (
			"Atmosphere Density",
			new Func<double> (() => VOID_Core.Instance.vessel.atmDensity * 1000f),
			"g/m³"
		);

		public static VOID_DoubleValue atmPressure = new VOID_DoubleValue (
			"Pressure",
			new Func<double> (() => VOID_Core.Instance.vessel.staticPressure),
			"atm"
		);

		public static VOID_FloatValue atmLimit = new VOID_FloatValue(
			"Atmosphere Limit",
			new Func<float> (() => VOID_Core.Instance.vessel.mainBody.maxAtmosphereAltitude),
			"m"
		);

		public static VOID_StrValue currBiome = new VOID_StrValue(
			"Biome",
			new Func<string> (() => Tools.Toadicus_GetAtt(VOID_Core.Instance.vessel).name)
		);

	}
}