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

			this._Active.value = true;

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

			VOID_Core.Instance.LabelStyles["hud"].alignment = TextAnchor.UpperRight;

			if (VOID_Core.Instance.powerAvailable)
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

				if (VOID_Data.stageNominalThrust != 0)
				{
					leftHUD.AppendFormat(
						string.Intern("Thrust Offset: {0}\n"),
						VOID_Data.vesselThrustOffset.Value.ToString("F1")
					);
				}
			}
			else
			{
				VOID_Core.Instance.LabelStyles["hud"].normal.textColor = Color.red;
				leftHUD.Append(string.Intern("-- POWER LOST --"));
			}

			GUILayout.Label(leftHUD.ToString(), VOID_Core.Instance.LabelStyles["hud"], GUILayout.ExpandWidth(true));

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

			VOID_Core.Instance.LabelStyles["hud"].alignment = TextAnchor.UpperLeft;

			if (VOID_Core.Instance.powerAvailable)
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
				VOID_Core.Instance.LabelStyles["hud"].normal.textColor = Color.red;
				rightHUD.Append(string.Intern("-- POWER LOST --"));
			}

			GUILayout.Label(rightHUD.ToString(), VOID_Core.Instance.LabelStyles["hud"], GUILayout.ExpandWidth(true));

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
				foreach (IVOID_Module module in VOID_Core.Instance.Modules)
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
					VOID_Core.Instance.windowID,
					this.leftHUDPos,
					this.leftHUDWindow,
					GUIContent.none,
					GUIStyle.none
				);

				if (VOID_Data.upcomingManeuverNodes > 0)
				{
					this.rightHUDPos.value = GUI.Window(
						VOID_Core.Instance.windowID,
						this.rightHUDPos,
						this.rightHUDWindow,
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

	public static partial class VOID_Data
	{
		public static int upcomingManeuverNodes
		{
			get
			{
				if (core.vessel == null ||
					core.vessel.patchedConicSolver == null ||
					core.vessel.patchedConicSolver.maneuverNodes == null
				)
				{
					return 0;
				}

				return core.vessel.patchedConicSolver.maneuverNodes.Count;
			}
		}

		public static readonly VOID_Vector3dValue vesselThrustOffset = new VOID_Vector3dValue(
			"Thrust Offset",
			delegate()
		{
			if (core.vessel == null)
			{
				return Vector3d.zero;
			}

			List<PartModule> engineModules = core.vessel.getModulesOfType<PartModule>();

			Vector3d thrustPos = Vector3d.zero;
			Vector3d thrustDir = Vector3d.zero;
			float thrust = 0;

			foreach (PartModule engine in engineModules)
			{
				float moduleThrust = 0;

				switch (engine.moduleName)
				{
					case "ModuleEngines":
					case "ModuleEnginesFX":
						break;
					default:
						continue;
				}

				if (!engine.isEnabled)
				{
					continue;
				}

				CenterOfThrustQuery cotQuery = new CenterOfThrustQuery();

				if (engine is ModuleEngines)
				{
					ModuleEngines engineModule = engine as ModuleEngines;

					moduleThrust = engineModule.finalThrust;

					engineModule.OnCenterOfThrustQuery(cotQuery);
				}
				else // engine is ModuleEnginesFX
				{
					ModuleEnginesFX engineFXModule = engine as ModuleEnginesFX;

					moduleThrust = engineFXModule.finalThrust;

					engineFXModule.OnCenterOfThrustQuery(cotQuery);
				}

				if (moduleThrust != 0d)
				{
					cotQuery.thrust = moduleThrust;
				}

				thrustPos += cotQuery.pos * cotQuery.thrust;
				thrustDir += cotQuery.dir * cotQuery.thrust;
				thrust += cotQuery.thrust;
			}

			if (thrust != 0)
			{
				thrustPos /= thrust;
				thrustDir /= thrust;
			}

			Vector3d thrustOffset = VectorTools.PointDistanceToLine(
				thrustPos, thrustDir.normalized, core.vessel.findWorldCenterOfMass());

			Tools.PostDebugMessage(typeof(VOID_Data), "vesselThrustOffset:\n" +
				"\tthrustPos: {0}\n" +
				"\tthrustDir: {1}\n" +
				"\tthrustOffset: {2}\n" +
				"\tvessel.CoM: {3}",
				thrustPos,
				thrustDir.normalized,
				thrustOffset,
				core.vessel.findWorldCenterOfMass()
			);

			return thrustOffset;
		},
			"m"
		);

		public static readonly VOID_DoubleValue vesselAccel = new VOID_DoubleValue(
			"Acceleration",
			() => geeForce * KerbinGee,
			"m/s"
		);

		public static readonly VOID_IntValue vesselCrewCount = new VOID_IntValue(
			"Crew Onboard",
			delegate()
			{
				if (core.vessel != null)
				{
					return core.vessel.GetCrewCount();
				}
				else
				{
					return 0;
				}
			},
			""
		);

		public static readonly VOID_IntValue vesselCrewCapacity = new VOID_IntValue(
			"Crew Capacity",
			delegate()
		{
			if (core.vessel != null)
			{
				return core.vessel.GetCrewCapacity();
			}
			else
			{
				return 0;
			}
		},
			""
		);

		public static readonly VOID_DoubleValue vesselAngularVelocity = new VOID_DoubleValue(
			"Angular Velocity",
			delegate()
		{
			if (core.vessel != null)
			{
				return core.vessel.angularVelocity.magnitude;
			}
			else
			{
				return double.NaN;
			}
		},
			"rad/s"
		);

		public static readonly VOID_DoubleValue stageNominalThrust = new VOID_DoubleValue(
			"Nominal Stage Thrust",
			delegate()
		{
			if (SimManager.LastStage == null)
			{
				return double.NaN;
			}

			if (SimManager.LastStage.actualThrust == 0d)
			{
				return SimManager.LastStage.thrust;
			}
			else
			{
				return SimManager.LastStage.actualThrust;
			}
		},
			"kN"
		);

		public static readonly VOID_DoubleValue stageMassFlow = new VOID_DoubleValue(
			"Stage Mass Flow",
			delegate()
			{
				if (SimManager.LastStage == null)
				{
					return double.NaN;
				}

			double stageIsp = SimManager.LastStage.isp;
			double stageThrust = stageNominalThrust;

			Tools.PostDebugMessage(typeof(VOID_Data), "calculating stageMassFlow from:\n" +
				"\tstageIsp: {0}\n" +
				"\tstageThrust: {1}\n" +
				"\tKerbinGee: {2}\n",
				stageIsp,
				stageThrust,
				KerbinGee
			);

				return stageThrust / (stageIsp * KerbinGee);
			},
			"Mg/s"
		);

		public static readonly VOID_DoubleValue currManeuverDeltaV = new VOID_DoubleValue(
			"Current Maneuver Delta-V",
			delegate()
			{
				if (upcomingManeuverNodes > 0)
				{
				return core.vessel.patchedConicSolver.maneuverNodes[0].DeltaV.magnitude;
				}
				else
				{
					return double.NaN;
				}
			},
			"m/s"
		);

		public static readonly VOID_DoubleValue currManeuverDVRemaining = new VOID_DoubleValue(
			"Remaining Maneuver Delta-V",
			delegate()
			{
				if (upcomingManeuverNodes > 0)
				{
					return core.vessel.patchedConicSolver.maneuverNodes[0].GetBurnVector(core.vessel.orbit).magnitude;
				}
				else
				{
					return double.NaN;
				}
			},
			"m/s"
		);

		public static readonly VOID_DoubleValue nextManeuverDeltaV = new VOID_DoubleValue(
			"Current Maneuver Delta-V",
			delegate()
		{
			if (upcomingManeuverNodes > 1)
			{
				return core.vessel.patchedConicSolver.maneuverNodes[1].DeltaV.magnitude;
			}
			else
			{
				return double.NaN;
			}
		},
			"m/s"
		);

		public static readonly VOID_DoubleValue currentNodeBurnDuration = new VOID_DoubleValue(
			"Total Burn Time",
			delegate()
			{
				if (SimManager.LastStage == null || currManeuverDeltaV.Value == double.NaN)
				{
					return double.NaN;
				}
			    
				double stageThrust = stageNominalThrust;

				return burnTime(currManeuverDeltaV.Value, totalMass, stageMassFlow, stageThrust);
			},
			"s"
		);

		public static readonly VOID_DoubleValue currentNodeBurnRemaining = new VOID_DoubleValue(
			"Burn Time Remaining",
			delegate()
			{
				if (SimManager.LastStage == null || currManeuverDVRemaining == double.NaN)
				{
					return double.NaN;
				}

				double stageThrust = stageNominalThrust;

				return burnTime(currManeuverDVRemaining, totalMass, stageMassFlow, stageThrust);
			},
			"s"
		);

		public static readonly VOID_DoubleValue currentNodeHalfBurnDuration = new VOID_DoubleValue(
			"Half Burn Time",
			delegate()
		{
			if (SimManager.LastStage == null || currManeuverDeltaV.Value == double.NaN)
			{
				return double.NaN;
			}

			double stageThrust = stageNominalThrust;

			return burnTime(currManeuverDeltaV.Value / 2d, totalMass, stageMassFlow, stageThrust);
		},
			"s"
		);

		public static readonly VOID_StrValue burnTimeDoneAtNode = new VOID_StrValue(
			"Full burn time to be half done at node",
			delegate()
		{
			if (SimManager.LastStage == null && upcomingManeuverNodes < 1)
			{
				return "N/A";
			}

			ManeuverNode node = core.vessel.patchedConicSolver.maneuverNodes[0];

			if ((node.UT - Planetarium.GetUniversalTime()) < 0)
			{
				return string.Empty;
			}

			double interval = (node.UT - currentNodeBurnDuration) - Planetarium.GetUniversalTime();

			int sign = Math.Sign(interval);
			interval = Math.Abs(interval);

			string format;

			if (sign >= 0)
			{
				format = string.Intern("T - {0}");
			}
			else
			{
				format = string.Intern("T + {0}");
			}

			return string.Format(format, VOID_Tools.ConvertInterval(interval));
		}
		);

		public static readonly VOID_StrValue burnTimeHalfDoneAtNode = new VOID_StrValue(
			"Full burn time to be half done at node",
			delegate()
			{
			if (SimManager.LastStage == null && upcomingManeuverNodes < 1)
			{
				return "N/A";
			}

			ManeuverNode node = core.vessel.patchedConicSolver.maneuverNodes[0];

			if ((node.UT - Planetarium.GetUniversalTime()) < 0)
			{
				return string.Empty;
			}

			double interval = (node.UT - currentNodeHalfBurnDuration) - Planetarium.GetUniversalTime();

			int sign = Math.Sign(interval);
			interval = Math.Abs(interval);

			string format;

			if (sign >= 0)
			{
				format = string.Intern("T - {0}");
			}
			else
			{
				format = string.Intern("T + {0}");
			}

			return string.Format(format, VOID_Tools.ConvertInterval(interval));
			}
		);

		private static double burnTime(double deltaV, double initialMass, double massFlow, double thrust)
		{
			Tools.PostDebugMessage(typeof(VOID_Data), "calculating burnTime from:\n" +
				"\tdeltaV: {0}\n" +
				"\tinitialMass: {1}\n" +
				"\tmassFlow: {2}\n" +
				"\tthrust: {3}\n",
				deltaV,
				initialMass,
				massFlow,
				thrust
			);
			return initialMass / massFlow * (Math.Exp(deltaV * massFlow / thrust) - 1d);
		}
	}
}
