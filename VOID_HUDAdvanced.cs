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
using System.Linq;
using System.Text;
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	public class VOID_HUDAdvanced : VOID_Module, IVOID_Module
	{
		/*
		 * Fields
		 * */
		protected VOID_HUD primaryHUD;

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
				if (this.primaryHUD == null)
				{
					return 0;
				}

				return this.primaryHUD.ColorIndex;
			}
		}

		/* 
		 * Methods
		 * */
		public VOID_HUDAdvanced() : base()
		{
			this._Name = "Advanced Heads-Up Display";

			this.toggleActive = true;

			this.leftHUDdefaultPos = new Rect(
				Screen.width * .5f - (float)GameSettings.UI_SIZE * .25f - 300f,
				Screen.height - 200f,
				300f, 90f
			);
			this.leftHUDPos = new Rect(this.leftHUDdefaultPos);

			this.rightHUDdefaultPos = new Rect(
				Screen.width * .5f + (float)GameSettings.UI_SIZE * .25f,
				Screen.height - 200f,
				300f, 90f
			);
			this.rightHUDPos = new Rect(this.rightHUDdefaultPos);

			this.positionsLocked = true;

			Tools.PostDebugMessage (this, "Constructed.");
		}

		protected void leftHUDWindow(int id)
		{
			StringBuilder leftHUD;

			leftHUD = new StringBuilder();

			VOID_Styles.labelHud.alignment = TextAnchor.UpperRight;

			if (this.core.powerAvailable)
			{
				leftHUD.AppendFormat(
					string.Intern("Mass: {0}\n"),
					VOID_Data.totalMass.ToSIString(2)
				);

				if (VOID_Data.vesselCrewCapacity > 0)
				{
					leftHUD.AppendFormat(
						string.Intern("Crew: {0} / {1}\n"),
						VOID_Data.vesselCrewCount.Value,
						VOID_Data.vesselCrewCapacity.Value
					);
				}

				leftHUD.AppendFormat(
					string.Intern("Acc: {0} T:W: {1}\n"),
					VOID_Data.vesselAccel.ToSIString(2),
					VOID_Data.currThrustWeight.Value.ToString("f2")
				);

				leftHUD.AppendFormat(
					string.Intern("Ang Vel: {0}\n"),
					VOID_Data.vesselAngularVelocity.ToSIString(2)
				);

				if (VOID_Data.stageNominalThrust != 0d)
				{
					leftHUD.AppendFormat(
						string.Intern("Thrust Offset: {0}\n"),
						VOID_Data.vesselThrustOffset.Value.ToString("F1")
					);
				}
			}
			else
			{
				VOID_Styles.labelHud.normal.textColor = Color.red;
				leftHUD.Append(string.Intern("-- POWER LOST --"));
			}

			GUILayout.Label(leftHUD.ToString(), VOID_Styles.labelHud, GUILayout.ExpandWidth(true));

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

			VOID_Styles.labelHud.alignment = TextAnchor.UpperLeft;

			if (this.core.powerAvailable)
			{
				rightHUD.AppendFormat(
					"Burn Δv (Rem/Tot): {0} / {1}\n",
					VOID_Data.currManeuverDVRemaining.ValueUnitString("f2"),
					VOID_Data.currManeuverDeltaV.ValueUnitString("f2")
				);

				if (VOID_Data.upcomingManeuverNodes > 1)
				{
					rightHUD.AppendFormat("Next Burn Δv: {0}\n",
						VOID_Data.nextManeuverDeltaV.ValueUnitString("f2")
					);
				}

				rightHUD.AppendFormat("Burn Time (Rem/Total): {0} / {1}\n",
					VOID_Tools.ConvertInterval(VOID_Data.currentNodeBurnRemaining.Value),
					VOID_Tools.ConvertInterval(VOID_Data.currentNodeBurnDuration.Value)
				);

				if (VOID_Data.burnTimeDoneAtNode.Value != string.Empty)
				{
					rightHUD.AppendFormat("{0} (done @ node)\n",
						VOID_Data.burnTimeDoneAtNode.Value
					);

					rightHUD.AppendFormat("{0} (½ done @ node)",
						VOID_Data.burnTimeHalfDoneAtNode.Value
					);
				}
				else
				{
					rightHUD.Append("Node is past");
				}
			}
			else
			{
				VOID_Styles.labelHud.normal.textColor = Color.red;
				rightHUD.Append(string.Intern("-- POWER LOST --"));
			}

			GUILayout.Label(rightHUD.ToString(), VOID_Styles.labelHud, GUILayout.ExpandWidth(true));

			if (!this.positionsLocked)
			{
				GUI.DragWindow();
			}

			GUI.BringWindowToBack(id);
		}

		public override void DrawGUI()
		{
			if (this.primaryHUD == null)
			{
				foreach (IVOID_Module module in this.core.Modules)
				{
					if (module is VOID_HUD)
					{
						this.primaryHUD = module as VOID_HUD;
					}
				}
			}
			else
			{
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

				if (VOID_Data.upcomingManeuverNodes > 0)
				{
					this.rightHUDPos.value = GUI.Window(
						this.core.windowID,
						this.rightHUDPos,
						VOID_Tools.GetWindowHandler(this.rightHUDWindow),
						GUIContent.none,
						GUIStyle.none
					);
				}
			}
		}

		public override void DrawConfigurables()
		{
			this.positionsLocked = GUILayout.Toggle(this.positionsLocked,
				string.Intern("Lock Advanced HUD Positions"),
				GUILayout.ExpandWidth(false));
		}
	}
}
