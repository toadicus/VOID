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
using Engineer.VesselSimulator;
using Engineer.Extensions;

namespace VOID
{
	public class VOID_VesselInfo : VOID_WindowModule
	{
		protected VOID_DoubleValue geeForce = new VOID_DoubleValue(
			"G-force",
			new Func<double>(() => VOID_Core.Instance.vessel.geeForce),
			"gees"
		);

		protected VOID_IntValue partCount = new VOID_IntValue(
			"Parts",
			new Func<int>(() => VOID_Core.Instance.vessel.Parts.Count),
			""
		);

		protected VOID_DoubleValue totalMass = new VOID_DoubleValue(
			"Total Mass",
			new Func<double> (() => SimManager.Instance.TryGetLastMass()),
			"tons"
		);

		protected VOID_DoubleValue resourceMass = new VOID_DoubleValue(
			"Resource Mass",
			delegate()
			{
				double rscMass = 0;
				foreach (Part part in VOID_Core.Instance.vessel.Parts)
				{
					rscMass += part.GetResourceMass();
				}
				return rscMass;
			},
			"tons"
		);

		protected VOID_DoubleValue stageDeltaV = new VOID_DoubleValue(
			"DeltaV (Current Stage)",
			delegate()
			{
				if (SimManager.Instance.Stages == null ||
			    	SimManager.Instance.Stages.Length <= Staging.lastStage
			    )
					return double.NaN;
				return SimManager.Instance.Stages[Staging.lastStage].deltaV;
			},
			"m/s"
		);

		protected VOID_DoubleValue totalDeltaV = new VOID_DoubleValue(
			"DeltaV (Total)",
			delegate()
			{
				if (SimManager.Instance.Stages == null)
					return double.NaN;
				return SimManager.Instance.LastStage.totalDeltaV;
			},
			"m/s"
		);

		protected VOID_FloatValue mainThrottle = new VOID_FloatValue(
			"Throttle",
			new Func<float>(() => VOID_Core.Instance.vessel.ctrlState.mainThrottle * 100f),
			"%"
		);

		protected VOID_StrValue currmaxThrust = new VOID_StrValue(
			"Thrust (curr/max)",
			delegate()
			{
				if (SimManager.Instance.Stages == null)
					return "N/A";

				double currThrust = SimManager.Instance.LastStage.actualThrust;
				double maxThrust = SimManager.Instance.LastStage.thrust;

				return string.Format(
					"{0} / {1}",
					currThrust.ToString("F1"),
					maxThrust.ToString("F1")
				);
			}
		);

		protected VOID_StrValue currmaxThrustWeight = new VOID_StrValue(
			"T:W (curr/max)",
			delegate()
			{
				if (SimManager.Instance.Stages == null)
					return "N/A";

				double currThrust = SimManager.Instance.LastStage.actualThrust;
				double maxThrust = SimManager.Instance.LastStage.thrust;
				double mass = SimManager.Instance.TryGetLastMass();
				double gravity = VOID_Core.Instance.vessel.mainBody.gravParameter /
				                 Math.Pow(
					                 VOID_Core.Instance.vessel.mainBody.Radius + VOID_Core.Instance.vessel.altitude,
					                 2
					                );
				double weight = mass * gravity;

				return string.Format(
					"{0} / {1}",
					(currThrust / weight).ToString("F2"),
					(maxThrust / weight).ToString("F2")
				);
			}
		);

		protected VOID_DoubleValue surfaceThrustWeight = new VOID_DoubleValue(
			"Max T:W @ surface",
			delegate()
			{
				if (SimManager.Instance.Stages == null)
					return double.NaN;

				double maxThrust = SimManager.Instance.LastStage.thrust;
				double mass = SimManager.Instance.TryGetLastMass();
				double gravity = (VOID_Core.Constant_G * VOID_Core.Instance.vessel.mainBody.Mass) /
				                 Math.Pow(VOID_Core.Instance.vessel.mainBody.Radius, 2);
				double weight = mass * gravity;

				return maxThrust / weight;
			},
			""
		);

		protected VOID_StrValue intakeAirStatus = new VOID_StrValue(
			"Intake Air (Curr / Req)",
			delegate()
			{
				double currentAmount;
				double currentRequirement;

				currentAmount = 0d;
				currentRequirement = 0d;

				foreach (Part part in VOID_Core.Instance.vessel.Parts)
				{
					if (part.HasModule<ModuleEngines>() && part.enabled)
					{
						foreach (Propellant propellant in part.GetModule<ModuleEngines>().propellants)
						{
							if (propellant.name == "IntakeAir")
							{
								// currentAmount += propellant.currentAmount;
								currentRequirement += propellant.currentRequirement / TimeWarp.fixedDeltaTime;
								break;
							}
						}
					}

					if (part.HasModule<ModuleResourceIntake>() && part.enabled)
					{
						ModuleResourceIntake intakeModule = part.GetModule<ModuleResourceIntake>();

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

		public VOID_VesselInfo() : base()
		{
			this._Name = "Vessel Information";

			this.WindowPos.x = Screen.width - 260;
			this.WindowPos.y = 450;
		}

		public override void ModuleWindow(int _)
		{
			base.ModuleWindow (_);

			if ((TimeWarp.WarpMode == TimeWarp.Modes.LOW) || (TimeWarp.CurrentRate <= TimeWarp.MaxPhysicsRate))
			{
				SimManager.Instance.RequestSimulation();
			}

			GUILayout.BeginVertical();

			GUILayout.Label(
				vessel.vesselName,
				VOID_Core.Instance.LabelStyles["center_bold"],
				GUILayout.ExpandWidth(true));

			this.geeForce.DoGUIHorizontal ("F2");

			this.partCount.DoGUIHorizontal ();

			this.totalMass.DoGUIHorizontal ("F1");

			this.resourceMass.DoGUIHorizontal ("F1");

			this.stageDeltaV.DoGUIHorizontal (3, false);

			this.totalDeltaV.DoGUIHorizontal (3, false);

			this.mainThrottle.DoGUIHorizontal ("F0");

			this.currmaxThrust.DoGUIHorizontal ();

			this.currmaxThrustWeight.DoGUIHorizontal ();

			this.surfaceThrustWeight.DoGUIHorizontal ("F2");

			this.intakeAirStatus.DoGUIHorizontal();

			GUILayout.EndVertical();
			GUI.DragWindow();
		}
	}
}

