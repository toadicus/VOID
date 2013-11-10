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
	public class VOID_DataLogger : VOID_WindowModule, IVOID_BehaviorModule
	{
		/*
		 * Fields
		 * */
		protected bool stopwatch1_running = false;

		protected bool csv_logging = false;
		protected bool first_write = true;

		protected double stopwatch1 = 0;

		protected string csv_log_interval_str = "0.5";

		protected float csv_log_interval;

		protected double csvWriteTimer = 0;
		protected double csvCollectTimer = 0;

		protected List<string> csvList = new List<string>();

		/*
		 * Properties
		 * */


		/*
		 * Methods
		 * */
		public VOID_DataLogger()
		{
			this._Name = "CSV Data Logger";

			this.WindowPos.x = Screen.width - 520;
			this.WindowPos.y = 85;
		}

		public override void ModuleWindow(int _)
		{
			GUIStyle txt_white = new GUIStyle(GUI.skin.label);
			txt_white.normal.textColor = txt_white.focused.textColor = Color.white;
			txt_white.alignment = TextAnchor.UpperRight;
			GUIStyle txt_green = new GUIStyle(GUI.skin.label);
			txt_green.normal.textColor = txt_green.focused.textColor = Color.green;
			txt_green.alignment = TextAnchor.UpperRight;
			GUIStyle txt_yellow = new GUIStyle(GUI.skin.label);
			txt_yellow.normal.textColor = txt_yellow.focused.textColor = Color.yellow;
			txt_yellow.alignment = TextAnchor.UpperRight;

			GUILayout.BeginVertical();

			GUILayout.Label("System time: " + DateTime.Now.ToString("HH:mm:ss"));
			GUILayout.Label(Tools.ConvertInterval(stopwatch1));

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			if (GUILayout.Button("Start"))
			{
				if (stopwatch1_running == false) stopwatch1_running = true;
			}
			if (GUILayout.Button("Stop"))
			{
				if (stopwatch1_running == true) stopwatch1_running = false;
			}
			if (GUILayout.Button("Reset"))
			{
				if (stopwatch1_running == true) stopwatch1_running = false;
				stopwatch1 = 0;
			}
			GUILayout.EndHorizontal();

			GUIStyle label_style = txt_white;
			string log_label = "Inactive";
			if (csv_logging && vessel.situation.ToString() == "PRELAUNCH")
			{
				log_label = "Awaiting launch";
				label_style = txt_yellow;
			}
			if (csv_logging && vessel.situation.ToString() != "PRELAUNCH")
			{
				log_label = "Active";
				label_style = txt_green;
			}
			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			csv_logging = GUILayout.Toggle(csv_logging, "Data logging: ", GUILayout.ExpandWidth(false));
			GUILayout.Label(log_label, label_style, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Interval: ", GUILayout.ExpandWidth(false));
			csv_log_interval_str = GUILayout.TextField(csv_log_interval_str, GUILayout.ExpandWidth(true));
			GUILayout.Label("s", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			float new_log_interval;
			if (Single.TryParse(csv_log_interval_str, out new_log_interval)) csv_log_interval = new_log_interval;

			GUILayout.EndVertical();
			GUI.DragWindow();
		}

		public void Update()
		{
			// CSV Logging
			// from ISA MapSat
			if (csv_logging)
			{
				//data logging is on
				//increment timers
				csvWriteTimer += Time.deltaTime;
				csvCollectTimer += Time.deltaTime;

				if (csvCollectTimer >= csv_log_interval && vessel.situation != Vessel.Situations.PRELAUNCH)
				{
					//data logging is on, vessel is not prelaunch, and interval has passed
					//write a line to the list
					line_to_csvList();  //write to the csv
				}

				if (csvList.Count != 0 && csvWriteTimer >= 15f)
				{
					// csvList is not empty and interval between writings to file has elapsed
					//write it
					string[] csvData;
					csvData = (string[])csvList.ToArray();
					Innsewerants_writeData(csvData);
					csvList.Clear();
					csvWriteTimer = 0f;
				}
			}
			else
			{
				//data logging is off
				//reset any timers and clear anything from csvList
				csvWriteTimer = 0f;
				csvCollectTimer = 0f;
				if (csvList.Count > 0) csvList.Clear();
			}

			if (stopwatch1_running)
			{
				stopwatch1 += Time.deltaTime;
			}
		}

		public void FixedUpdate() {}

		private void Innsewerants_writeData(string[] csvArray)
		{
			var efile = KSP.IO.File.AppendText<VOID_Core>(vessel.vesselName + "_data.csv", null);
			foreach (string line in csvArray)
			{
				efile.Write(line);
			}
			efile.Close();
		}

		private void line_to_csvList()
		{
			//called if logging is on and interval has passed
			//writes one line to the csvList

			string line = "";
			if (first_write && !KSP.IO.File.Exists<VOID_Core>(vessel.vesselName + "_data.csv", null))
			{
				first_write = false;
				line += "Mission Elapsed Time (s);Altitude ASL (m);Altitude above terrain (m);Orbital Velocity (m/s);Surface Velocity (m/s);Vertical Speed (m/s);Horizontal Speed (m/s);Gee Force (gees);Temperature (°C);Gravity (m/s²);Atmosphere Density (g/m³);\n";
			}
			//Mission time
			line += vessel.missionTime.ToString("F3") + ";";
			//Altitude ASL
			line += vessel.orbit.altitude.ToString("F3") + ";";
			//Altitude (true)
			double alt_true = vessel.orbit.altitude - vessel.terrainAltitude;
			if (vessel.terrainAltitude < 0) alt_true = vessel.orbit.altitude;
			line += alt_true.ToString("F3") + ";";
			//Orbital velocity
			line += vessel.orbit.vel.magnitude.ToString("F3") + ";";
			//surface velocity
			line += vessel.srf_velocity.magnitude.ToString("F3") + ";";
			//vertical speed
			line += vessel.verticalSpeed.ToString("F3") + ";";
			//horizontal speed
			line += vessel.horizontalSrfSpeed.ToString("F3") + ";";
			//gee force
			line += vessel.geeForce.ToString("F3") + ";";
			//temperature
			line += vessel.flightIntegrator.getExternalTemperature().ToString("F2") + ";";
			//gravity
			double r_vessel = vessel.mainBody.Radius + vessel.mainBody.GetAltitude(vessel.findWorldCenterOfMass());
			double g_vessel = (VOID_Core.Constant_G * vessel.mainBody.Mass) / Math.Pow(r_vessel, 2);
			line += g_vessel.ToString("F3") + ";";
			//atm density
			line += (vessel.atmDensity * 1000).ToString("F3") + ";";
			line += "\n";
			if (csvList.Contains(line) == false) csvList.Add(line);
			csvCollectTimer = 0f;
		}
	}
}