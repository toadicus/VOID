// VOID
//
// VOID_HUD.cs
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

using Engineer.VesselSimulator;
using KSP;
using System;
using System.Collections.Generic;
using System.Text;
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	public class VOID_HUD : VOID_Module, IVOID_Module
	{
		/*
		 * Fields
		 * */
		[AVOID_SaveValue("colorIndex")]
		protected VOID_SaveValue<int> _colorIndex;

		protected List<Color> textColors;

		protected Rect leftHUDdefaultPos;
		protected Rect rightHUDdefaultPos;

		[AVOID_SaveValue("leftHUDPos")]
		protected VOID_SaveValue<Rect> leftHUDPos;
		[AVOID_SaveValue("rightHUDPos")]
		protected VOID_SaveValue<Rect> rightHUDPos;

		[AVOID_SaveValue("positionsLocked")]
		protected VOID_SaveValue<bool> positionsLocked;

		/*
		 * Properties
		 * */
		public int ColorIndex
		{
			get
			{
				return this._colorIndex;
			}
			set
			{
				if (this._colorIndex >= this.textColors.Count - 1)
				{
					this._colorIndex = 0;
					return;
				}

				this._colorIndex = value;
			}
		}

		/* 
		 * Methods
		 * */
		public VOID_HUD() : base()
		{
			this._Name = "Heads-Up Display";

			this.toggleActive = true;

			this._colorIndex = 0;

			this.textColors = new List<Color>();

			this.textColors.Add(Color.green);
			this.textColors.Add(Color.black);
			this.textColors.Add(Color.white);
			this.textColors.Add(Color.red);
			this.textColors.Add(Color.blue);
			this.textColors.Add(Color.yellow);
			this.textColors.Add(Color.gray);
			this.textColors.Add(Color.cyan);
			this.textColors.Add(Color.magenta);

			this.leftHUDdefaultPos = new Rect(Screen.width * .375f - 300f, 0f, 300f, 90f);
			this.leftHUDPos = new Rect(this.leftHUDdefaultPos);

			this.rightHUDdefaultPos = new Rect(Screen.width * .625f, 0f, 300f, 90f);
			this.rightHUDPos = new Rect(this.rightHUDdefaultPos);

			this.positionsLocked = true;

			Tools.PostDebugMessage ("VOID_HUD: Constructed.");
		}

		protected void leftHUDWindow(int id)
		{
			StringBuilder leftHUD;

			leftHUD = new StringBuilder();

			this.core.LabelStyles["hud"].alignment = TextAnchor.UpperRight;

			if (this.core.powerAvailable)
			{
				leftHUD.AppendFormat("Primary: {0} Inc: {1}",
					VOID_Data.primaryName.ValueUnitString(),
					VOID_Data.orbitInclination.ValueUnitString("F3")
				);
				leftHUD.AppendFormat("\nObt Alt: {0} Obt Vel: {1}",
					VOID_Data.orbitAltitude.ToSIString(),
					VOID_Data.orbitVelocity.ToSIString()
				);
				leftHUD.AppendFormat("\nAp: {0} ETA {1}",
					VOID_Data.orbitApoAlt.ToSIString(),
					VOID_Data.timeToApo.ValueUnitString()
				);
				leftHUD.AppendFormat("\nPe: {0} ETA {1}",
					VOID_Data.oribtPeriAlt.ToSIString(),
					VOID_Data.timeToPeri.ValueUnitString()
				);
				leftHUD.AppendFormat("\nTot Δv: {0} Stg Δv: {1}",
					VOID_Data.totalDeltaV.ToSIString(2),
					VOID_Data.stageDeltaV.ToSIString(2)
				);
			}
			else
			{
				this.core.LabelStyles["hud"].normal.textColor = Color.red;
				leftHUD.Append(string.Intern("-- POWER LOST --"));
			}

			GUILayout.Label(leftHUD.ToString(), this.core.LabelStyles["hud"], GUILayout.ExpandWidth(true));

			if (!this.positionsLocked)
			{
				GUI.DragWindow();
			}

			GUI.BringWindowToBack(id);
		}

		protected void rightHUDWindow(int id)
		{
			StringBuilder rightHUD;

			rightHUD = new StringBuilder();

			this.core.LabelStyles["hud"].alignment = TextAnchor.UpperLeft;

			if (this.core.powerAvailable)
			{
				rightHUD.AppendFormat("Biome: {0} Sit: {1}",
					VOID_Data.currBiome.ValueUnitString(),
					VOID_Data.expSituation.ValueUnitString()
				);
				rightHUD.AppendFormat("\nSrf Alt: {0} Srf Vel: {1}",
					VOID_Data.trueAltitude.ToSIString(),
					VOID_Data.surfVelocity.ToSIString()
				);
				rightHUD.AppendFormat("\nVer: {0} Hor: {1}",
					VOID_Data.vertVelocity.ToSIString(),
					VOID_Data.horzVelocity.ToSIString()
				);
				rightHUD.AppendFormat("\nLat: {0} Lon: {1}",
					VOID_Data.surfLatitude.ValueUnitString(),
					VOID_Data.surfLongitude.ValueUnitString()
				);
				rightHUD.AppendFormat("\nHdg: {0} Pit: {1}",
					VOID_Data.vesselHeading.ValueUnitString(),
					VOID_Data.vesselPitch.ToSIString(2)
				);

				if (
					this.core.vessel.mainBody == this.core.Kerbin &&
					(
						this.core.vessel.situation == Vessel.Situations.FLYING ||
						this.core.vessel.situation == Vessel.Situations.SUB_ORBITAL ||
						this.core.vessel.situation == Vessel.Situations.LANDED ||
						this.core.vessel.situation == Vessel.Situations.SPLASHED
					)
				)
				{
					rightHUD.AppendFormat("\nRange to KSC: {0}", VOID_Data.downrangeDistance.ValueUnitString(2));
				}
			}
			else
			{
				this.core.LabelStyles["hud"].normal.textColor = Color.red;
				rightHUD.Append(string.Intern("-- POWER LOST --"));
			}


			GUILayout.Label(rightHUD.ToString(), this.core.LabelStyles["hud"], GUILayout.ExpandWidth(true));

			if (!this.positionsLocked)
			{
				GUI.DragWindow();
			}

			GUI.BringWindowToBack(id);
		}

		public override void DrawGUI()
		{
			if (!this.core.LabelStyles.ContainsKey("hud"))
			{
				this.core.LabelStyles["hud"] = new GUIStyle(GUI.skin.label);
			}

			this.core.LabelStyles["hud"].normal.textColor = textColors [ColorIndex];

			GUI.skin = this.core.Skin;

			if ((TimeWarp.WarpMode == TimeWarp.Modes.LOW) || (TimeWarp.CurrentRate <= TimeWarp.MaxPhysicsRate))
			{
				SimManager.RequestSimulation();
			}

			this.leftHUDPos.value = GUI.Window(
				this.core.windowID,
				this.leftHUDPos,
				VOID_Tools.GetWindowHandler(this.leftHUDWindow),
				GUIContent.none,
				GUIStyle.none
			);

			this.rightHUDPos.value = GUI.Window(
				this.core.windowID,
				this.rightHUDPos,
				VOID_Tools.GetWindowHandler(this.rightHUDWindow),
				GUIContent.none,
				GUIStyle.none
			);
		}

		public override void DrawConfigurables()
		{
			if (GUILayout.Button (string.Intern("Change HUD color"), GUILayout.ExpandWidth (false)))
			{
				++this.ColorIndex;
			}

			if (GUILayout.Button(string.Intern("Reset HUD Positions"), GUILayout.ExpandWidth(false)))
			{
				this.leftHUDPos = new Rect(this.leftHUDdefaultPos);
				this.rightHUDPos = new Rect(this.rightHUDdefaultPos);
			}

			this.positionsLocked = GUILayout.Toggle(this.positionsLocked,
				string.Intern("Lock HUD Positions"),
				GUILayout.ExpandWidth(false));
		}
	}

	public static partial class VOID_Data
	{
		public static readonly VOID_StrValue expSituation = new VOID_StrValue(
			"Situation",
			new Func<string> (() => core.vessel.GetExperimentSituation().HumanString())
		);

		public static readonly VOID_DoubleValue vesselPitch = new VOID_DoubleValue(
			"Pitch",
			() => core.vessel.getSurfacePitch(),
			"°"
		);

		public static readonly VOID_DoubleValue downrangeDistance = new VOID_DoubleValue(
			"Downrange Distance",
			delegate() {

			if (core.vessel == null ||
				Planetarium.fetch == null ||
				core.vessel.mainBody != Planetarium.fetch.Home
			)
			{
				return double.NaN;
			}

			double vesselLongitude = core.vessel.longitude * Math.PI / 180d;
			double vesselLatitude = core.vessel.latitude * Math.PI / 180d;

			const double kscLongitude = 285.442323427289 * Math.PI / 180d;
			const double kscLatitude = -0.0972112860655246 * Math.PI / 180d;

			double diffLon = vesselLongitude - kscLongitude;
			double diffLat = vesselLatitude - kscLatitude;

			double sinHalfDiffLat = Math.Sin(diffLat / 2d);
			double sinHalfDiffLon = Math.Sin(diffLon / 2d);

			double cosVesselLon = Math.Cos(vesselLongitude);
			double cosKSCLon = Math.Cos(kscLongitude);

			double haversine =
				sinHalfDiffLat * sinHalfDiffLat +
				cosVesselLon * cosKSCLon * sinHalfDiffLon * sinHalfDiffLon;

			double arc = 2d * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1d - haversine));

			return core.vessel.mainBody.Radius * arc;
		},
			"m"
		);
	}
}
