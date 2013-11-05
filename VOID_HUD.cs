//
//  VOID_Hud.cs
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
//

using KSP;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace VOID
{
	public class VOID_HUD : IVOID_Module
	{
		/*
		 * Static Members
		 * */
		protected static VOID_HUD _instance;

		public static IVOID_Module Instance {
			get {
				if (_instance == null) {
					Tools.PostDebugMessage ("Instantiating VOID_HUD");
					_instance = new VOID_HUD ();
				}
				Tools.PostDebugMessage ("Returning VOID_HUD instance.");
				return _instance;
			}
		}

		/*
		 * Fields
		 * */
		protected List<Color> textColors = new List<Color>();

		protected GUIStyle labelStyle;

		protected int _counterTextColor = 0;

		protected Vessel vessel = null;

		public bool guiRunning = false;

		/*
		 * Properties
		 * */
		public int ColorIndex
		{
			get
			{
				return this._counterTextColor;
			}
			set
			{
				if (this._counterTextColor >= this.textColors.Count - 1)
				{
					this._counterTextColor = 0;
					return;
				}

				this._counterTextColor = value;
			}
		}

		/* 
		 * Methods
		 * */
		private VOID_HUD() : base()
		{
			Tools.PostDebugMessage ("Constructing VOID_HUD");
			try
			{
				this.textColors.Add(Color.green);
				this.textColors.Add(Color.black);
				this.textColors.Add(Color.white);
				this.textColors.Add(Color.red);
				this.textColors.Add(Color.blue);
				this.textColors.Add(Color.yellow);
				this.textColors.Add(Color.gray);
				this.textColors.Add(Color.cyan);
				this.textColors.Add(Color.magenta);
			}
			catch (NullReferenceException)
			{
				Tools.PostDebugMessage ("Caught NRE while adding colors.");
			}

			this.labelStyle = new GUIStyle ();
			this.labelStyle.normal.textColor = this.textColors [this.ColorIndex];

			Tools.PostDebugMessage ("VOID_HUD Constructed.");
		}

		~VOID_HUD()
		{
			Tools.PostDebugMessage ("Destructing VOID_HUD");
			this.SaveConfig();
			System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace();

			Tools.PostDebugMessage (trace.ToString ());
		}

		public void DrawGUI()
		{
			Tools.PostDebugMessage ("VOID_HUD: Drawing GUI.");
			if (vessel == null)
			{
				vessel = VOIDFlightMaster.Instance.vessel;
			}

			if (vessel != FlightGlobals.ActiveVessel)
			{
				return;
			}

			if (VOIDFlightMaster.Instance.power_toggle)
			{
				if (VOIDFlightMaster.Instance.power_available || VOIDFlightMaster.Instance.disable_power_usage)
				{
					labelStyle.normal.textColor = textColors [ColorIndex];

					GUI.Label (
						new Rect ((Screen.width * .2083f), 0, 300f, 70f),
						"Obt Alt: " + Tools.MuMech_ToSI (vessel.orbit.altitude) + "m" +
						" Obt Vel: " + Tools.MuMech_ToSI (vessel.orbit.vel.magnitude) + "m/s" +
						"\nAp: " + Tools.MuMech_ToSI (vessel.orbit.ApA) + "m" +
						" ETA " + Tools.ConvertInterval (vessel.orbit.timeToAp) +
						"\nPe: " + Tools.MuMech_ToSI (vessel.orbit.PeA) + "m" +
						" ETA " + Tools.ConvertInterval (vessel.orbit.timeToPe) +
						"\nInc: " + vessel.orbit.inclination.ToString ("F3") + "°",
						labelStyle);
					// Toadicus edit: Added "Biome: " line to surf/atmo HUD
					GUI.Label (
						new Rect ((Screen.width * .625f), 0, 300f, 90f),
						"Srf Alt: " + Tools.MuMech_ToSI (Tools.TrueAltitude (vessel)) + "m" +
						" Srf Vel: " + Tools.MuMech_ToSI (vessel.srf_velocity.magnitude) + "m/s" +
						"\nVer: " + Tools.MuMech_ToSI (vessel.verticalSpeed) + "m/s" +
						" Hor: " + Tools.MuMech_ToSI (vessel.horizontalSrfSpeed) + "m/s" +
						"\nLat: " + Tools.GetLatitudeString (vessel, "F3") +
						" Lon: " + Tools.GetLongitudeString (vessel, "F3") +
						"\nHdg: " + Tools.MuMech_get_heading (vessel).ToString ("F2") + "° " +
						Tools.get_heading_text (Tools.MuMech_get_heading (vessel)) +
						"\nBiome: " + Tools.Toadicus_GetAtt (vessel).name,
						labelStyle);
				}
				else
				{
					labelStyle.normal.textColor = Color.red;
					GUI.Label (new Rect ((Screen.width * .2083f), 0, 300f, 70f), "-- POWER LOST --", labelStyle);
					GUI.Label (new Rect ((Screen.width * .625f), 0, 300f, 70f), "-- POWER LOST --", labelStyle);
				}
			}
		}

		public void StartGUI()
		{
			Tools.PostDebugMessage ("Adding VOID_HUD to the draw queue.");
			RenderingManager.AddToPostDrawQueue (3, new Callback(this.DrawGUI));
			this.guiRunning = true;
		}

		public void StopGUI()
		{
			Tools.PostDebugMessage ("Removing VOID_HUD from the draw queue.");
			RenderingManager.RemoveFromPostDrawQueue (3, new Callback(this.DrawGUI));
			this.guiRunning = false;
		}

		public void SaveConfig()
		{
			Tools.PostDebugMessage ("VOID_HUD: Saving Config.");
			var config = KSP.IO.PluginConfiguration.CreateForType<VOID_HUD> ();
			config.load ();
			config.SetValue ("ColorIndex", this.ColorIndex);
			config.save ();
		}

		public void LoadConfig()
		{
			Tools.PostDebugMessage ("VOID_HUD: Loading Config.");
			var config = KSP.IO.PluginConfiguration.CreateForType<VOID_HUD> ();
			config.load ();
			this.ColorIndex = config.GetValue ("ColorIndex", 0);
		}
	}
}
