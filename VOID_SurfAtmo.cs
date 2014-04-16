// VOID
//
// VOID_SurfAtmo.cs
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

using KSP;
using System;
using ToadicusTools;
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
		public static readonly VOID_DoubleValue trueAltitude = new VOID_DoubleValue(
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

		public static readonly VOID_StrValue surfLatitude = new VOID_StrValue(
			"Latitude",
			new Func<string> (() => Tools.GetLatitudeString(VOID_Core.Instance.vessel))
		);

		public static readonly VOID_StrValue surfLongitude = new VOID_StrValue(
			"Longitude",
			new Func<string> (() => Tools.GetLongitudeString(VOID_Core.Instance.vessel))
		);

		public static readonly VOID_StrValue vesselHeading = new VOID_StrValue(
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

		public static readonly VOID_DoubleValue terrainElevation = new VOID_DoubleValue(
			"Terrain elevation",
			new Func<double> (() => VOID_Core.Instance.vessel.terrainAltitude),
			"m"
		);

		public static readonly VOID_DoubleValue surfVelocity = new VOID_DoubleValue(
			"Surface velocity",
			new Func<double> (() => VOID_Core.Instance.vessel.srf_velocity.magnitude),
			"m/s"
		);

		public static readonly VOID_DoubleValue vertVelocity = new VOID_DoubleValue(
			"Vertical speed",
			new Func<double> (() => VOID_Core.Instance.vessel.verticalSpeed),
			"m/s"
		);

		public static readonly VOID_DoubleValue horzVelocity = new VOID_DoubleValue(
			"Horizontal speed",
			new Func<double> (() => VOID_Core.Instance.vessel.horizontalSrfSpeed),
			"m/s"
		);

		public static readonly VOID_FloatValue temperature = new VOID_FloatValue(
			"Temperature",
			new Func<float> (() => VOID_Core.Instance.vessel.flightIntegrator.getExternalTemperature()),
			"°C"
		);

		public static readonly VOID_DoubleValue atmDensity = new VOID_DoubleValue (
			"Atmosphere Density",
			new Func<double> (() => VOID_Core.Instance.vessel.atmDensity * 1000f),
			"g/m³"
		);

		public static readonly VOID_DoubleValue atmPressure = new VOID_DoubleValue (
			"Pressure",
			new Func<double> (() => VOID_Core.Instance.vessel.staticPressure),
			"atm"
		);

		public static readonly VOID_FloatValue atmLimit = new VOID_FloatValue(
			"Atmosphere Limit",
			new Func<float> (() => VOID_Core.Instance.vessel.mainBody.maxAtmosphereAltitude),
			"m"
		);

		public static readonly VOID_StrValue currBiome = new VOID_StrValue(
			"Biome",
			new Func<string> (() => Tools.GetBiome(VOID_Core.Instance.vessel).name)
		);

	}
}