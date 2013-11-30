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

		protected VOID_DoubleValue trueAltitude = new VOID_DoubleValue(
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

		protected VOID_StrValue surfLatitude = new VOID_StrValue(
			"Latitude",
			new Func<string> (() => Tools.GetLatitudeString(VOID_Core.Instance.vessel))
		);

		protected VOID_StrValue surfLongitude = new VOID_StrValue(
			"Longitude",
			new Func<string> (() => Tools.GetLongitudeString(VOID_Core.Instance.vessel))
		);

		protected VOID_StrValue vesselHeading = new VOID_StrValue(
			"Heading",
			delegate()
			{
				double heading = Tools.MuMech_get_heading(VOID_Core.Instance.vessel);
				string cardinal = Tools.get_heading_text(heading);

				return string.Format(
					"{0}° {1}",
					heading.ToString("F2"),
					cardinal
				);
			}
		);

		protected VOID_DoubleValue terrainElevation = new VOID_DoubleValue(
			"Terrain elevation",
			new Func<double> (() => VOID_Core.Instance.vessel.terrainAltitude),
			"m"
		);

		protected VOID_DoubleValue surfVelocity = new VOID_DoubleValue(
			"Surface velocity",
			new Func<double> (() => VOID_Core.Instance.vessel.srf_velocity.magnitude),
			"m/s"
		);

		protected VOID_DoubleValue vertVelocity = new VOID_DoubleValue(
			"Vertical speed",
			new Func<double> (() => VOID_Core.Instance.vessel.verticalSpeed),
			"m/s"
		);

		protected VOID_DoubleValue horzVelocity = new VOID_DoubleValue(
			"Horizontal speed",
			new Func<double> (() => VOID_Core.Instance.vessel.horizontalSrfSpeed),
			"m/s"
		);

		protected VOID_FloatValue temperature = new VOID_FloatValue(
			"Temperature",
			new Func<float> (() => VOID_Core.Instance.vessel.flightIntegrator.getExternalTemperature()),
			"°C"
		);

		protected VOID_DoubleValue atmDensity = new VOID_DoubleValue (
			"Atmosphere Density",
			new Func<double> (() => VOID_Core.Instance.vessel.atmDensity * 1000f),
			"g/m³"
		);

		protected VOID_DoubleValue atmPressure = new VOID_DoubleValue (
			"Pressure",
			new Func<double> (() => VOID_Core.Instance.vessel.staticPressure),
			"atm"
		);

		protected VOID_FloatValue atmLimit = new VOID_FloatValue(
			"Atmosphere Limit",
			new Func<float> (() => VOID_Core.Instance.vessel.mainBody.maxAtmosphereAltitude),
			"m"
		);

		protected VOID_StrValue currBiome = new VOID_StrValue(
			"Biome",
			new Func<string> (() => Tools.Toadicus_GetAtt(VOID_Core.Instance.vessel).name)
		);

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

			this.precisionValues [idx]= (ushort)this.trueAltitude.DoGUIHorizontal (this.precisionValues [idx]);
			idx++;

			this.surfLatitude.DoGUIHorizontal ();

			this.surfLongitude.DoGUIHorizontal ();

			this.vesselHeading.DoGUIHorizontal ();

			this.precisionValues [idx]= (ushort)this.terrainElevation.DoGUIHorizontal (this.precisionValues [idx]);
			idx++;

			this.precisionValues [idx]= (ushort)this.surfVelocity.DoGUIHorizontal (this.precisionValues [idx]);
			idx++;

			this.precisionValues [idx]= (ushort)this.vertVelocity.DoGUIHorizontal (this.precisionValues [idx]);
			idx++;

			this.precisionValues [idx]= (ushort)this.horzVelocity.DoGUIHorizontal (this.precisionValues [idx]);
			idx++;

			this.temperature.DoGUIHorizontal ("F2");

			this.atmDensity.DoGUIHorizontal (3);

			this.atmPressure.DoGUIHorizontal ("F2");

			this.precisionValues [idx]= (ushort)this.atmLimit.DoGUIHorizontal (this.precisionValues [idx]);
			idx++;

			// Toadicus edit: added Biome
			this.currBiome.DoGUIHorizontal ();

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
}