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
		public VOID_SurfAtmo()
		{
			this._Name = "Surface & Atmospheric Information";

			this.WindowPos.x = Screen.width - 260f;
			this.WindowPos.y = 85;
		}

		public override void ModuleWindow(int _)
		{
			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Altitude (true):");
			double alt_true = vessel.orbit.altitude - vessel.terrainAltitude;
			// HACK: This assumes that on worlds with oceans, all water is fixed at 0 m, and water covers the whole surface at 0 m.
			if (vessel.terrainAltitude < 0 && vessel.mainBody.ocean ) alt_true = vessel.orbit.altitude;
			GUILayout.Label(Tools.MuMech_ToSI(alt_true) + "m", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Latitude:");
			GUILayout.Label(Tools.GetLatitudeString(vessel), GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Longitude:");
			GUILayout.Label(Tools.GetLongitudeString(vessel), GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Heading:");
			GUILayout.Label(Tools.MuMech_get_heading(vessel).ToString("F2") + "° " + Tools.get_heading_text(Tools.MuMech_get_heading(vessel)), GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Terrain elevation:");
			GUILayout.Label(Tools.MuMech_ToSI(vessel.terrainAltitude) + "m", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Surface velocity:");
			GUILayout.Label(Tools.MuMech_ToSI(vessel.srf_velocity.magnitude) + "m/s", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Vertical speed:");
			GUILayout.Label(Tools.MuMech_ToSI(vessel.verticalSpeed) + "m/s", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Horizontal speed:");
			GUILayout.Label(Tools.MuMech_ToSI(vessel.horizontalSrfSpeed) + "m/s", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Temperature:");
			GUILayout.Label(vessel.flightIntegrator.getExternalTemperature().ToString("F2") + "° C", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Atmosphere density:");
			GUILayout.Label(Tools.MuMech_ToSI(vessel.atmDensity * 1000) + "g/m³", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Pressure:");
			GUILayout.Label(vessel.staticPressure.ToString("F2") + " atms", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Atmosphere limit:");
			GUILayout.Label("≈ " + Tools.MuMech_ToSI(vessel.mainBody.maxAtmosphereAltitude) + "m", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			// Toadicus edit: added Biome
			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Biome:");
			GUILayout.Label(Tools.Toadicus_GetAtt(vessel).name, VOID_Core.Instance.LabelStyles["txt_right"]);
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
			GUI.DragWindow();
		}
	}
}