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
	public class VOID_Transfer : VOID_WindowModule
	{
		[AVOID_SaveValue("toggleExtended")]
		protected VOID_SaveValue<bool> toggleExtended = false;

		protected List<CelestialBody> selectedBodies = new List<CelestialBody>();

		public VOID_Transfer()
		{
			this._Name = "Transfer Angle Information";
		}

		public override void ModuleWindow(int _)
		{
			GUILayout.BeginVertical();

			if (vessel.mainBody.name == "Sun")  //Vessel is orbiting the Sun
			{
			    foreach (CelestialBody body in vessel.mainBody.orbitingBodies)
			    {
					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					if (GUILayout.Button(body.bodyName))
					{
						//add or remove this body to this list of bodies to display more info on
						if (selectedBodies.Contains(body)) selectedBodies.Remove(body);
						else selectedBodies.Add(body);
					}
					GUILayout.Label("Inclined " + body.orbit.inclination.ToString("F3") + "°", GUILayout.ExpandWidth(false));
					GUILayout.EndHorizontal();

					if (selectedBodies.Contains(body))
					{
						display_transfer_angles_SUN2PLANET(body);  //show phase angles for each selected body
						tad_targeting(body);    //display Set/Unset Target button for each selected body
					}
			    }
			}
			else if (vessel.mainBody.referenceBody.name == "Sun")	//Vessel is orbiting a planet
			{
			    foreach (CelestialBody body in vessel.mainBody.referenceBody.orbitingBodies)
			    {
			        if (body.name != vessel.mainBody.name)	// show other planets
			        {
			            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			            if (GUILayout.Button(body.bodyName))
			            {
			                //add or remove this body to this list of bodies to display more info on
			                if (selectedBodies.Contains(body)) selectedBodies.Remove(body);
			                else selectedBodies.Add(body);
			            }
			            GUILayout.Label("Inclined " + body.orbit.inclination.ToString("F3") + "°", GUILayout.ExpandWidth(false));
			            GUILayout.EndHorizontal();

			            if (selectedBodies.Contains(body))
			            {
			                display_transfer_angles_PLANET2PLANET(body);
			                tad_targeting(body);    //display Set/Unset Target button
			            }
			        }
			    }
			    foreach (CelestialBody body in vessel.mainBody.orbitingBodies)	// show moons
			    {
			        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			        if (GUILayout.Button(body.bodyName))
			        {
			            //add or remove this body to this list of bodies to display more info on
			            if (selectedBodies.Contains(body)) selectedBodies.Remove(body);
			            else selectedBodies.Add(body);
			        }
			        GUILayout.Label("Inclined " + body.orbit.inclination.ToString("F3") + "°", GUILayout.ExpandWidth(false));
			        GUILayout.EndHorizontal();

			        if (selectedBodies.Contains(body))
			        {
			            display_transfer_angles_PLANET2MOON(body);
			            tad_targeting(body);    //display Set/Unset Target button
			        }
			    }
			}
			else if (vessel.mainBody.referenceBody.referenceBody.name == "Sun")	// Vessel is orbiting a moon
			{
			    foreach (CelestialBody body in vessel.mainBody.referenceBody.orbitingBodies)
			    {
					if (body.name != vessel.mainBody.name)	// show other moons
					{
						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
						if (GUILayout.Button(body.bodyName))
						{
							//add or remove this body to this list of bodies to display more info on
							if (selectedBodies.Contains(body)) selectedBodies.Remove(body);
							else selectedBodies.Add(body);
						}
						GUILayout.Label("Inclined " + body.orbit.inclination.ToString("F3") + "°", GUILayout.ExpandWidth(false));
						GUILayout.EndHorizontal();

						if (selectedBodies.Contains(body))
						{
							display_transfer_angles_MOON2MOON(body);
							tad_targeting(body);    //display Set/Unset Target button
						}
					}
				}
			}
			GUILayout.EndVertical();
			GUI.DragWindow();
		}

		private void tad_targeting(CelestialBody body)
		{
		    //Target Set/Unset buttons
		    if (FlightGlobals.fetch.VesselTarget == null || (FlightGlobals.fetch.VesselTarget != null && FlightGlobals.fetch.VesselTarget.GetVessel() == null))
		    {
		        //No TGT set or TGT is a Body
		        if ((CelestialBody)FlightGlobals.fetch.VesselTarget != body)
		        {
		            if (GUILayout.Button("Set Target", GUILayout.ExpandWidth(false)))
		            {
		                FlightGlobals.fetch.SetVesselTarget(body);
						Tools.PostDebugMessage("[VOID] KSP Target set to CelestialBody " + body.bodyName);
		            }
		        }
		        else if ((CelestialBody)FlightGlobals.fetch.VesselTarget == body)
		        {
		            if (GUILayout.Button("Unset Target", GUILayout.ExpandWidth(false)))
		            {
		                FlightGlobals.fetch.SetVesselTarget(null);
		                Tools.PostDebugMessage("[VOID] KSP Target set to null");
		            }
		        }
		    }
		    else if (FlightGlobals.fetch.VesselTarget == null || (FlightGlobals.fetch.VesselTarget != null && FlightGlobals.fetch.VesselTarget.GetVessel() != null))
		    {
		        //No TGT or TGT is a vessel
		        if (GUILayout.Button("Set Target", GUILayout.ExpandWidth(false)))
		        {
		            FlightGlobals.fetch.SetVesselTarget(body);
		            Tools.PostDebugMessage("[VOID] KSP Target set to CelestialBody " + body.bodyName);
		        }
		    }
		}

		private void display_transfer_angles_SUN2PLANET(CelestialBody body)
		{
			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Phase angle (curr/trans):");
			GUILayout.Label(Tools.mrenigma03_calcphase(vessel, body).ToString("F3") + "° / " + Tools.Nivvy_CalcTransferPhaseAngle(vessel.orbit.semiMajorAxis, body.orbit.semiMajorAxis, vessel.mainBody.gravParameter).ToString("F3") + "°", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Transfer velocity:");
			GUILayout.Label((Tools.Younata_DeltaVToGetToOtherBody((vessel.mainBody.gravParameter / 1000000000), (vessel.orbit.semiMajorAxis / 1000), (body.orbit.semiMajorAxis / 1000)) * 1000).ToString("F2") + "m/s", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();
		}

		private void display_transfer_angles_PLANET2PLANET(CelestialBody body)
		{
			double dv1 = Tools.Younata_DeltaVToGetToOtherBody((vessel.mainBody.referenceBody.gravParameter / 1000000000), (vessel.mainBody.orbit.semiMajorAxis / 1000), (body.orbit.semiMajorAxis / 1000));
			double dv2 = Tools.Younata_DeltaVToExitSOI((vessel.mainBody.gravParameter / 1000000000), (vessel.orbit.semiMajorAxis / 1000), (vessel.mainBody.sphereOfInfluence / 1000), Math.Abs(dv1));

			double trans_ejection_angle = Tools.Younata_TransferBurnPoint((vessel.orbit.semiMajorAxis / 1000), dv2, (Math.PI / 2.0), (vessel.mainBody.gravParameter / 1000000000));
			double curr_ejection_angle = Tools.Adammada_CurrentEjectionAngle(FlightGlobals.ActiveVessel.longitude, FlightGlobals.ActiveVessel.orbit.referenceBody.rotationAngle, FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.LAN, FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.orbitPercent);

			double trans_phase_angle = Tools.Nivvy_CalcTransferPhaseAngle(vessel.mainBody.orbit.semiMajorAxis, body.orbit.semiMajorAxis, vessel.mainBody.referenceBody.gravParameter) % 360;
			double curr_phase_angle = Tools.Adammada_CurrrentPhaseAngle(body.orbit.LAN, body.orbit.orbitPercent, FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.LAN, FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.orbitPercent);

			double adj_phase_angle = Tools.adjustCurrPhaseAngle(trans_phase_angle, curr_phase_angle);
			double adj_trans_ejection_angle = Tools.adjust_transfer_ejection_angle(trans_ejection_angle, trans_phase_angle);
			double adj_curr_ejection_angle = Tools.adjust_current_ejection_angle(curr_ejection_angle);

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Phase angle (curr/trans):");
			GUILayout.Label(adj_phase_angle.ToString("F3") + "° / " + trans_phase_angle.ToString("F3") + "°", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Ejection angle (curr/trans):");
			GUILayout.Label(adj_curr_ejection_angle.ToString("F3") + "° / " + adj_trans_ejection_angle.ToString("F3") + "°", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Transfer velocity:");
			GUILayout.Label((dv2 * 1000).ToString("F2") + "m/s", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();
		}

		private void display_transfer_angles_PLANET2MOON(CelestialBody body)
		{
			double dv1 = Tools.Younata_DeltaVToGetToOtherBody((vessel.mainBody.gravParameter / 1000000000), (vessel.orbit.semiMajorAxis / 1000), (body.orbit.semiMajorAxis / 1000));
			
			double trans_phase_angle = Tools.Nivvy_CalcTransferPhaseAngle(vessel.orbit.semiMajorAxis, body.orbit.semiMajorAxis, vessel.mainBody.gravParameter);

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Phase angle (curr/trans):");
			GUILayout.Label(Tools.mrenigma03_calcphase(vessel, body).ToString("F3") + "° / " + trans_phase_angle.ToString("F3") + "°", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Transfer velocity:");
			GUILayout.Label((dv1 * 1000).ToString("F2") + "m/s", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();
		}

		private void display_transfer_angles_MOON2MOON(CelestialBody body)
		{
			double dv1 = Tools.Younata_DeltaVToGetToOtherBody((vessel.mainBody.referenceBody.gravParameter / 1000000000), (vessel.mainBody.orbit.semiMajorAxis / 1000), (body.orbit.semiMajorAxis / 1000));
			double dv2 = Tools.Younata_DeltaVToExitSOI((vessel.mainBody.gravParameter / 1000000000), (vessel.orbit.semiMajorAxis / 1000), (vessel.mainBody.sphereOfInfluence / 1000), Math.Abs(dv1));
			double trans_ejection_angle = Tools.Younata_TransferBurnPoint((vessel.orbit.semiMajorAxis / 1000), dv2, (Math.PI / 2.0), (vessel.mainBody.gravParameter / 1000000000));

			double curr_phase_angle = Tools.Adammada_CurrrentPhaseAngle(body.orbit.LAN, body.orbit.orbitPercent, FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.LAN, FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.orbitPercent);
			double curr_ejection_angle = Tools.Adammada_CurrentEjectionAngle(FlightGlobals.ActiveVessel.longitude, FlightGlobals.ActiveVessel.orbit.referenceBody.rotationAngle, FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.LAN, FlightGlobals.ActiveVessel.orbit.referenceBody.orbit.orbitPercent);

			double trans_phase_angle = Tools.Nivvy_CalcTransferPhaseAngle(vessel.mainBody.orbit.semiMajorAxis, body.orbit.semiMajorAxis, vessel.mainBody.referenceBody.gravParameter) % 360;

			double adj_phase_angle = Tools.adjustCurrPhaseAngle(trans_phase_angle, curr_phase_angle);
			//double adj_ejection_angle = adjustCurrEjectionAngle(trans_phase_angle, curr_ejection_angle);

			//new stuff
			//
			double adj_trans_ejection_angle = Tools.adjust_transfer_ejection_angle(trans_ejection_angle, trans_phase_angle);
			double adj_curr_ejection_angle = Tools.adjust_current_ejection_angle(curr_ejection_angle);
			//
			//

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Phase angle (curr/trans):");
			GUILayout.Label(adj_phase_angle.ToString("F3") + "° / " + trans_phase_angle.ToString("F3") + "°", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Ejection angle (curr/trans):");
			GUILayout.Label(adj_curr_ejection_angle.ToString("F3") + "° / " + adj_trans_ejection_angle.ToString("F3") + "°", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label("Transfer velocity:");
			GUILayout.Label((dv2 * 1000).ToString("F2") + "m/s", GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();
		}
	}
}

