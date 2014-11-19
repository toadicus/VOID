// VOID
//
// VOID_DataLogger.cs
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
using System.Collections.Generic;
using System.Text;
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	public class VOID_DataLogger : VOID_WindowModule, IVOID_BehaviorModule
	{
		/*
		 * Fields
		 * */
		protected bool stopwatch1_running;

		protected bool csv_logging;
		protected bool first_write;

		protected double stopwatch1;

		protected string csv_log_interval_str;

		protected float csv_log_interval;

		protected double csvWriteTimer;
		protected double csvCollectTimer;

		protected System.Text.UTF8Encoding enc;

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

			this.stopwatch1_running = false;

			this.csv_logging = false;
			this.first_write = true;

			this.stopwatch1 = 0;
			this.csv_log_interval_str = "0.5";

			this.csvWriteTimer = 0;
			this.csvCollectTimer = 0;

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
			GUILayout.Label(VOID_Tools.ConvertInterval(stopwatch1));

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
			if (float.TryParse(csv_log_interval_str, out new_log_interval))
			{
				csv_log_interval = new_log_interval;
			}

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
			if (this.enc == null)
			{
				this.enc = new System.Text.UTF8Encoding(true, true);
			}

			KSP.IO.FileStream bFile;

			if (KSP.IO.File.Exists<VOID_Core>(vessel.vesselName + "_data.csv", null))
			{
				bFile = KSP.IO.File.Open<VOID_Core>(vessel.vesselName + "_data.csv", KSP.IO.FileMode.Append, null);
			}
			else
			{
				bFile = KSP.IO.File.Open<VOID_Core>(vessel.vesselName + "_data.csv", KSP.IO.FileMode.Create, null);

				byte[] bom = enc.GetPreamble();

				bFile.Write(bom, 0, bom.Length);
			}

			foreach (string line in csvArray)
			{
				byte[] lineBytes = enc.GetBytes(line);

				bFile.Write(lineBytes, 0, lineBytes.Length);
			}

			bFile.Dispose();
		}

		private void line_to_csvList()
		{
			//called if logging is on and interval has passed
			//writes one line to the csvList

			StringBuilder line = new StringBuilder();

			if (first_write)
			{
				first_write = false;
				line.Append(
					"\"Mission Elapsed Time (s)\t\"," +
					"\"Altitude ASL (m)\"," +
					"\"Altitude above terrain (m)\"," +
					"\"Surface Latitude (°)\"," +
					"\"Surface Longitude (°)\"," +
					"\"Orbital Velocity (m/s)\"," +
					"\"Surface Velocity (m/s)\"," +
					"\"Vertical Speed (m/s)\"," +
					"\"Horizontal Speed (m/s)\"," +
					"\"Gee Force (gees)\"," +
					"\"Temperature (°C)\"," +
					"\"Gravity (m/s²)\"," +
					"\"Atmosphere Density (g/m³)\"," +
					"\"Downrange Distance  (m)\"," +
					"\n"
				);
			}

			//Mission time
			line.Append(vessel.missionTime.ToString("F3"));
			line.Append(',');

			//Altitude ASL
			line.Append(vessel.orbit.altitude.ToString("F3"));
			line.Append(',');

			//Altitude (true)
			double alt_true = vessel.orbit.altitude - vessel.terrainAltitude;
			if (vessel.terrainAltitude < 0) alt_true = vessel.orbit.altitude;
			line.Append(alt_true.ToString("F3"));
			line.Append(',');

			// Surface Latitude
			line.Append('"');
			line.Append(VOID_Data.surfLatitude.Value);
			line.Append('"');
			line.Append(',');

			// Surface Longitude
			line.Append('"');
			line.Append(VOID_Data.surfLongitude.Value);
			line.Append('"');
			line.Append(',');

			//Orbital velocity
			line.Append(vessel.orbit.vel.magnitude.ToString("F3"));
			line.Append(',');

			//surface velocity
			line.Append(vessel.srf_velocity.magnitude.ToString("F3"));
			line.Append(',');

			//vertical speed
			line.Append(vessel.verticalSpeed.ToString("F3"));
			line.Append(',');

			//horizontal speed
			line.Append(vessel.horizontalSrfSpeed.ToString("F3"));
			line.Append(',');

			//gee force
			line.Append(vessel.geeForce.ToString("F3"));
			line.Append(',');

			//temperature
			line.Append(vessel.flightIntegrator.getExternalTemperature().ToString("F2"));
			line.Append(',');

			//gravity
			double r_vessel = vessel.mainBody.Radius + vessel.mainBody.GetAltitude(vessel.findWorldCenterOfMass());
			double g_vessel = (VOID_Core.Constant_G * vessel.mainBody.Mass) / (r_vessel * r_vessel);
			line.Append(g_vessel.ToString("F3"));
			line.Append(',');

			//atm density
			line.Append((vessel.atmDensity * 1000).ToString("F3"));
			line.Append(',');

			// Downrange Distance
			line.Append((VOID_Data.downrangeDistance.Value.ToString("G3")));

			line.Append('\n');

			csvList.Add(line.ToString());

			csvCollectTimer = 0f;
		}
	}
}

