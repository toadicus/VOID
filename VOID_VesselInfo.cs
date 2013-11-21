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
	public class VOID_VesselInfo : VOID_WindowModule
	{
		[AVOID_SaveValue("toggleExtended")]
		protected VOID_SaveValue<bool> toggleExtended = false;

		public VOID_VesselInfo()
		{
			this._Name = "Vessel Information";

			this.WindowPos.x = Screen.width - 260;
			this.WindowPos.y = 450;
		}

		public override void ModuleWindow(int _)
		{
			if ((TimeWarp.WarpMode == TimeWarp.Modes.LOW) || (TimeWarp.CurrentRate <= TimeWarp.MaxPhysicsRate))
			{
				Engineer.VesselSimulator.SimManager.Instance.RequestSimulation();
			}

			Engineer.VesselSimulator.Stage[] stages = Engineer.VesselSimulator.SimManager.Instance.Stages;

			GUILayout.BeginVertical();

			GUILayout.Label(
				vessel.vesselName,
				VOID_Core.Instance.LabelStyles["center_bold"],
				GUILayout.ExpandWidth(true));

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("G-force:");
			GUILayout.Label(vessel.geeForce.ToString("F2") + " gees", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			int num_parts = 0;
			double total_mass = vessel.GetTotalMass();
			double resource_mass = 0;
			double max_thrust = 0;
			double final_thrust = 0;

			foreach (Part p in vessel.parts)
			{
			    num_parts++;
			    resource_mass += p.GetResourceMass();

			    foreach (PartModule pm in p.Modules)
			    {
			        if ((pm.moduleName == "ModuleEngines") &&
						((p.State == PartStates.ACTIVE) ||
							((Staging.CurrentStage > Staging.lastStage) && (p.inverseStage == Staging.lastStage)))
					)
			        {
			            max_thrust += ((ModuleEngines)pm).maxThrust;
			            final_thrust += ((ModuleEngines)pm).finalThrust;
			        }
			    }
			}

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Parts:");
			GUILayout.Label(num_parts.ToString("F0"), GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Total mass:");
			GUILayout.Label(total_mass.ToString("F1") + " tons", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Resource mass:");
			GUILayout.Label(resource_mass.ToString("F1") + " tons", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			if (stages.Length > Staging.lastStage)
			{
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.Label("DeltaV (Current Stage):");
				GUILayout.Label(
					Tools.MuMech_ToSI(stages[Staging.lastStage].deltaV).ToString() + "m/s",
					GUILayout.ExpandWidth(false));
				GUILayout.EndHorizontal();
			}

			if (stages.Length > 0)
			{
				double totalDeltaV = 0d;

				for (int i = 0; i < stages.Length; ++i)
				{
					totalDeltaV += stages [i].deltaV;
				}

				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.Label("DeltaV (Total):");
				GUILayout.Label(Tools.MuMech_ToSI(totalDeltaV).ToString() + "m/s", GUILayout.ExpandWidth(false));
				GUILayout.EndHorizontal();
			}

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Throttle:");
			GUILayout.Label((vessel.ctrlState.mainThrottle * 100f).ToString("F0") + "%", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Thrust (curr/max):");
			GUILayout.Label(
				final_thrust.ToString("F1") +
				" / " + max_thrust.ToString("F1") + " kN",
				GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			double gravity = vessel.mainBody.gravParameter / Math.Pow(vessel.mainBody.Radius + vessel.altitude, 2);
			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("T:W (curr/max):");
			GUILayout.Label(
				(final_thrust / (total_mass * gravity)).ToString("F2") +
				" / " + (max_thrust / (total_mass * gravity)).ToString("F2"),
				GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			double g_ASL = (VOID_Core.Constant_G * vessel.mainBody.Mass) / Math.Pow(vessel.mainBody.Radius, 2);
			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Max T:W @ surface:");
			GUILayout.Label((max_thrust / (total_mass * g_ASL)).ToString("F2"), GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
			GUI.DragWindow();
		}
	}
}

