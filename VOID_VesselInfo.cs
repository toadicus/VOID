// VOID
//
// VOID_VesselInfo.cs
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
using Engineer.Extensions;
using KSP;
using System;
using System.Collections.Generic;
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	public class VOID_VesselInfo : VOID_WindowModule
	{
		public VOID_VesselInfo() : base()
		{
			this._Name = "Vessel Information";

			this.WindowPos.x = Screen.width - 260;
			this.WindowPos.y = 450;
		}

		public override void ModuleWindow(int _)
		{
			if ((TimeWarp.WarpMode == TimeWarp.Modes.LOW) || (TimeWarp.CurrentRate <= TimeWarp.MaxPhysicsRate))
			{
				SimManager.RequestSimulation();
			}

			GUILayout.BeginVertical();

			GUILayout.Label(
				vessel.vesselName,
				core.LabelStyles["center_bold"],
				GUILayout.ExpandWidth(true));

			VOID_Data.geeForce.DoGUIHorizontal ("F2");

			VOID_Data.partCount.DoGUIHorizontal ();

			VOID_Data.totalMass.DoGUIHorizontal ("F3");

			VOID_Data.comboResourceMass.DoGUIHorizontal ();

			VOID_Data.stageDeltaV.DoGUIHorizontal (3, false);

			VOID_Data.totalDeltaV.DoGUIHorizontal (3, false);

			VOID_Data.mainThrottle.DoGUIHorizontal ("F0");

			VOID_Data.currmaxThrust.DoGUIHorizontal ();

			VOID_Data.currmaxThrustWeight.DoGUIHorizontal ();

			VOID_Data.surfaceThrustWeight.DoGUIHorizontal ("F2");

			VOID_Data.intakeAirStatus.DoGUIHorizontal();

			GUILayout.EndVertical();

			GUI.DragWindow();
		}
	}

	public static partial class VOID_Data
	{
		public static readonly VOID_DoubleValue geeForce = new VOID_DoubleValue(
			"G-force",
			new Func<double>(() => core.vessel.geeForce),
			"gees"
		);

		public static readonly VOID_IntValue partCount = new VOID_IntValue(
			"Parts",
			new Func<int>(() => core.vessel.Parts.Count),
			""
		);

		public static readonly VOID_DoubleValue totalMass = new VOID_DoubleValue(
			"Total Mass",
			delegate()
		{
			if (SimManager.Stages == null || SimManager.LastStage == null)
			{
				return double.NaN;
			}

			return SimManager.LastStage.totalMass;
		},
			"tons"
		);

		public static readonly VOID_DoubleValue resourceMass = new VOID_DoubleValue(
			"Resource Mass",
			delegate()
			{
				if (SimManager.Stages == null || SimManager.LastStage == null)
				{
					return double.NaN;
				}

				return SimManager.LastStage.totalMass - SimManager.LastStage.totalBaseMass;
			},
			"tons"
		);

		public static readonly VOID_DoubleValue stageResourceMass = new VOID_DoubleValue(
			"Resource Mass (Current Stage)",
			delegate()
			{
				if (SimManager.LastStage == null)
				{
					return double.NaN;
				}

				return SimManager.LastStage.mass - SimManager.LastStage.baseMass;
			},
			"tons"
		);

		public static readonly VOID_StrValue comboResourceMass = new VOID_StrValue(
			"Resource Mass (curr / total)",
			delegate()
		{
			return string.Format("{0} / {1}", stageResourceMass.ValueUnitString(), resourceMass.ValueUnitString());
		}
		);

		public static readonly VOID_DoubleValue stageDeltaV = new VOID_DoubleValue(
			"DeltaV (Current Stage)",
			delegate()
			{
				if (SimManager.Stages == null || SimManager.LastStage == null)
					return double.NaN;
				return SimManager.LastStage.deltaV;
			},
			"m/s"
		);

		public static readonly VOID_DoubleValue totalDeltaV = new VOID_DoubleValue(
			"DeltaV (Total)",
			delegate()
			{
				if (SimManager.Stages == null || SimManager.LastStage == null)
					return double.NaN;
				return SimManager.LastStage.totalDeltaV;
			},
			"m/s"
		);

		public static readonly VOID_FloatValue mainThrottle = new VOID_FloatValue(
			"Throttle",
			new Func<float>(() => core.vessel.ctrlState.mainThrottle * 100f),
			"%"
		);

		public static readonly VOID_StrValue currmaxThrust = new VOID_StrValue(
			"Thrust (curr/max)",
			delegate()
			{
				if (SimManager.Stages == null || SimManager.LastStage == null)
					return "N/A";

				double currThrust = SimManager.LastStage.actualThrust;
				double maxThrust = SimManager.LastStage.thrust;

				return string.Format(
					"{0} / {1}",
					currThrust.ToString("F1"),
					maxThrust.ToString("F1")
				);
			}
		);

		public static readonly VOID_DoubleValue currThrustWeight = new VOID_DoubleValue(
			"T:W Ratio",
			delegate()
		{
			if (SimManager.LastStage == null)
			{
				return double.NaN;
			}

			return SimManager.LastStage.actualThrustToWeight;
		},
			""
		);

		public static readonly VOID_DoubleValue maxThrustWeight = new VOID_DoubleValue(
			"T:W Ratio",
			delegate()
		{
			if (SimManager.LastStage == null)
			{
				return double.NaN;
			}

			return SimManager.LastStage.thrustToWeight;
		},
			""
		);

		public static readonly VOID_StrValue currmaxThrustWeight = new VOID_StrValue(
			"T:W (curr/max)",
			delegate()
			{
				if (SimManager.Stages == null || SimManager.LastStage == null)
					return "N/A";

				return string.Format(
					"{0} / {1}",
					(VOID_Data.currThrustWeight.Value).ToString("F2"),
					(VOID_Data.maxThrustWeight.Value).ToString("F2")
				);
			}
		);

		public static readonly VOID_DoubleValue surfaceThrustWeight = new VOID_DoubleValue(
			"Max T:W @ surface",
			delegate()
			{
			if (SimManager.Stages == null || SimManager.LastStage == null)
					return double.NaN;

				double maxThrust = SimManager.LastStage.thrust;
				double mass = SimManager.LastStage.totalMass;
				double gravity = (VOID_Core.Constant_G * core.vessel.mainBody.Mass) /
				(core.vessel.mainBody.Radius * core.vessel.mainBody.Radius);
				double weight = mass * gravity;

				return maxThrust / weight;
			},
			""
		);

		public static readonly VOID_StrValue intakeAirStatus = new VOID_StrValue(
			"Intake Air (Curr / Req)",
			delegate()
			{
				double currentAmount;
				double currentRequirement;

				currentAmount = 0d;
				currentRequirement = 0d;

				foreach (Part part in core.vessel.Parts)
				{
					if (part.enabled)
					{
						ModuleEngines engineModule;
						ModuleEnginesFX enginesFXModule;
						List<Propellant> propellantList = null;

						if (part.tryGetFirstModuleOfType<ModuleEngines>(out engineModule))
						{
							propellantList = engineModule.propellants;
						}
						else if (part.tryGetFirstModuleOfType<ModuleEnginesFX>(out enginesFXModule))
						{
							propellantList = enginesFXModule.propellants;
						}
							
						if (propellantList != null)
						{
							foreach (Propellant propellant in propellantList)
							{
								if (propellant.name == "IntakeAir")
								{
									currentRequirement += propellant.currentRequirement / TimeWarp.fixedDeltaTime;
									break;
								}
							}
						}
					}

					ModuleResourceIntake intakeModule;

					if (part.enabled && part.tryGetFirstModuleOfType<ModuleResourceIntake>(out intakeModule))
					{
						if (intakeModule.resourceName == "IntakeAir")
						{
							currentAmount += intakeModule.airFlow;
						}
					}
				}

				if (currentAmount == 0 && currentRequirement == 0)
				{
					return "N/A";
				}

				return string.Format("{0:F3} / {1:F3}", currentAmount, currentRequirement);
			}
		);
	}
}
