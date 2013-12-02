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
using System.Linq;
using UnityEngine;

namespace VOID
{
	public class VOID_Rendezvous : VOID_WindowModule
	{
		[AVOID_SaveValue("untoggleRegisterInfo")]
		protected VOID_SaveValue<bool> untoggleRegisterInfo = false;

		[AVOID_SaveValue("toggleExtendedOrbital")]
		protected VOID_SaveValue<bool> toggleExtendedOrbital = false;

		protected VOID_VesselRegister RegisterModule;

		public VOID_Rendezvous()
		{
			this._Name = "Rendezvous Information";

			this.WindowPos.x = 845;
			this.WindowPos.y = 85;
		}

		public override void ModuleWindow(int _)
		{
			Vessel rendezvessel = new Vessel();
			CelestialBody rendezbody = new CelestialBody();

			this.RegisterModule = VOID_Core.Instance.Modules.Where(m => typeof(VOID_VesselRegister).IsAssignableFrom(m.GetType())).FirstOrDefault() as VOID_VesselRegister;

			GUILayout.BeginVertical();

			//display both
			//Show Target Info
			GUILayout.Label("Target:", VOID_Core.Instance.LabelStyles["center_bold"]);
			if (FlightGlobals.fetch.VesselTarget != null)
			{
				//a KSP Target (body or vessel) is selected
				if (FlightGlobals.fetch.vesselTargetMode == FlightGlobals.VesselTargetModes.Direction)
				{
					//a Body is selected
					rendezbody = vessel.patchedConicSolver.targetBody;
					display_rendezvous_info(null, rendezbody);
				}
				else if (FlightGlobals.fetch.vesselTargetMode == FlightGlobals.VesselTargetModes.DirectionAndVelocity)
				{
					//a Vessel is selected
					rendezvessel = FlightGlobals.fetch.VesselTarget.GetVessel();
					display_rendezvous_info(rendezvessel, null);
				}
				//Show Unset button for both options above
				if (GUILayout.Button("Unset Target", GUILayout.ExpandWidth(false)))
				{
					FlightGlobals.fetch.SetVesselTarget(null);
					Tools.PostDebugMessage("VOID_Rendezvous: KSP Target set to null");
				}

			}
			else
			{
				//no KSP Target selected
				GUILayout.Label("No Target Selected", VOID_Core.Instance.LabelStyles["center_bold"]);
			}

			//Show Vessel Register vessel info
			if (untoggleRegisterInfo == false && this.RegisterModule != default(IVOID_Module))
			{
				GUILayout.Label("Vessel Register:", VOID_Core.Instance.LabelStyles["center_bold"]);
				if (this.RegisterModule.selectedVessel != null)
				{
					rendezvessel = this.RegisterModule.selectedVessel;
					display_rendezvous_info(rendezvessel, null);

					//show set/unset buttons
					if (FlightGlobals.fetch.VesselTarget == null || (FlightGlobals.fetch.VesselTarget != null && FlightGlobals.fetch.VesselTarget.GetVessel() != this.RegisterModule.selectedVessel))
					{
						//no Tgt set or Tgt is not this vessel
						//show a Set button
						if (GUILayout.Button("Set Target", GUILayout.ExpandWidth(false)))
						{
							FlightGlobals.fetch.SetVesselTarget(rendezvessel);
							Tools.PostDebugMessage("[VOID] KSP Target set to " + rendezvessel.vesselName);
						}
					}
				}
				else
				{
					//vesreg Vessel is null
					//targ = null;
					GUILayout.Label("No Vessel Selected", VOID_Core.Instance.LabelStyles["center_bold"]);
				}
			}

			untoggleRegisterInfo.value = GUILayout.Toggle(untoggleRegisterInfo, "Hide Vessel Register Info");

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(" ", GUILayout.ExpandWidth(true));
			if (GUILayout.Button("Close", GUILayout.ExpandWidth(false))) this._Active = false;
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();
			GUI.DragWindow();
		}

		private void display_rendezvous_info(Vessel v, CelestialBody cb)
		{
			if (cb == null && v != null)
			{
				//Display vessel rendezvous info
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.Label(v.vesselName, VOID_Core.Instance.LabelStyles["center_bold"], GUILayout.ExpandWidth(true));
				GUILayout.EndHorizontal();

				if (v.situation == Vessel.Situations.ESCAPING || v.situation == Vessel.Situations.FLYING || v.situation == Vessel.Situations.ORBITING || v.situation == Vessel.Situations.SUB_ORBITAL)
				{
					// Toadicus edit: added local sidereal longitude.
					// Toadicus edit: added local sidereal longitude.
					double LSL = vessel.longitude + vessel.orbit.referenceBody.rotationAngle;
					LSL = Tools.FixDegreeDomain (LSL);

					//display orbital info for orbiting/flying/suborbital/escaping vessels only
					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					GUILayout.Label("Ap/Pe:");
					GUILayout.Label(Tools.MuMech_ToSI(v.orbit.ApA) + "m / " + Tools.MuMech_ToSI(v.orbit.PeA) + "m", GUILayout.ExpandWidth(false));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					GUILayout.Label("Altitude:");
					GUILayout.Label(Tools.MuMech_ToSI(v.orbit.altitude) + "m", GUILayout.ExpandWidth(false));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					GUILayout.Label("Inclination:");
					GUILayout.Label(v.orbit.inclination.ToString("F3") + "°", GUILayout.ExpandWidth(false));
					GUILayout.EndHorizontal();

					if (vessel.mainBody == v.mainBody)
					{
						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
						GUILayout.Label("Relative inclination:");
						GUILayout.Label(Vector3d.Angle(vessel.orbit.GetOrbitNormal(), v.orbit.GetOrbitNormal()).ToString("F3") + "°", GUILayout.ExpandWidth(false));
						GUILayout.EndHorizontal();
					}
					//if (debugging) Debug.Log("[CHATR] v -> v relative incl OK");

					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					GUILayout.Label("Velocity:");
					GUILayout.Label(Tools.MuMech_ToSI(v.orbit.vel.magnitude) + "m/s", GUILayout.ExpandWidth(false));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					GUILayout.Label("Relative velocity:");
					GUILayout.Label(Tools.MuMech_ToSI(v.orbit.vel.magnitude - vessel.orbit.vel.magnitude) + "m/s", GUILayout.ExpandWidth(false));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					GUILayout.Label("Distance:");
					GUILayout.Label(Tools.MuMech_ToSI((vessel.findWorldCenterOfMass() - v.findWorldCenterOfMass()).magnitude) + "m", GUILayout.ExpandWidth(false));
					GUILayout.EndHorizontal();

					//target_vessel_extended_orbital_info = GUILayout.Toggle(target_vessel_extended_orbital_info, "Extended info");

					if (toggleExtendedOrbital)
					{
						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
						GUILayout.Label("Period:");
						GUILayout.Label(Tools.ConvertInterval(v.orbit.period), GUILayout.ExpandWidth(false));
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
						GUILayout.Label("Semi-major axis:");
						GUILayout.Label((v.orbit.semiMajorAxis / 1000).ToString("##,#") + "km", GUILayout.ExpandWidth(false));
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
						GUILayout.Label("Eccentricity:");
						GUILayout.Label(v.orbit.eccentricity.ToString("F4"), GUILayout.ExpandWidth(false));
						GUILayout.EndHorizontal();

						// Toadicus edit: convert mean anomaly into degrees.
						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
						GUILayout.Label("Mean anomaly:");
						GUILayout.Label((v.orbit.meanAnomaly * 180d / Math.PI).ToString("F3") + "°", GUILayout.ExpandWidth(false));
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
						GUILayout.Label("True anomaly:");
						GUILayout.Label(v.orbit.trueAnomaly.ToString("F3") + "°", GUILayout.ExpandWidth(false));
						GUILayout.EndHorizontal();

						// Toadicus edit: convert eccentric anomaly into degrees.
						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
						GUILayout.Label("Eccentric anomaly:");
						GUILayout.Label((v.orbit.eccentricAnomaly * 180d / Math.PI).ToString("F3") + "°", GUILayout.ExpandWidth(false));
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
						GUILayout.Label("Long. ascending node:");
						GUILayout.Label(v.orbit.LAN.ToString("F3") + "°", GUILayout.ExpandWidth(false));
						GUILayout.EndHorizontal();

						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
						GUILayout.Label("Arg. of periapsis:");
						GUILayout.Label(v.orbit.argumentOfPeriapsis.ToString("F3") + "°", GUILayout.ExpandWidth(false));
						GUILayout.EndHorizontal();

						// Toadicus edit: added local sidereal longitude.
						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
						GUILayout.Label("Local Sidereal Longitude:");
						GUILayout.Label(LSL.ToString("F3") + "°", VOID_Core.Instance.LabelStyles["right"]);
						GUILayout.EndHorizontal();
					}
				}
				else
				{
					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					GUILayout.Label("Latitude:");
					GUILayout.Label(Tools.GetLatitudeString(vessel), GUILayout.ExpandWidth(false));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					GUILayout.Label("Longitude:");
					GUILayout.Label(Tools.GetLongitudeString(vessel), GUILayout.ExpandWidth(false));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					GUILayout.Label("Distance:");
					GUILayout.Label(Tools.MuMech_ToSI((vessel.findWorldCenterOfMass() - v.findWorldCenterOfMass()).magnitude) + "m", GUILayout.ExpandWidth(false));
					GUILayout.EndHorizontal();
				}
			}
			else if (cb != null && v == null)
			{
				//Display CelstialBody rendezvous info
				GUILayout.Label(cb.bodyName, VOID_Core.Instance.LabelStyles["center_bold"]);

				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.Label("Ap/Pe:");
				GUILayout.Label(Tools.MuMech_ToSI(cb.orbit.ApA) + "m / " + Tools.MuMech_ToSI(cb.orbit.PeA) + "m", GUILayout.ExpandWidth(false));
				GUILayout.EndHorizontal();
				//if (debugging) Debug.Log("[VOID] Ap/Pe OK");

				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.Label("Inclination:");
				GUILayout.Label(cb.orbit.inclination.ToString("F3") + "°", GUILayout.ExpandWidth(false));
				GUILayout.EndHorizontal();
				//if (debugging) Debug.Log("[VOID] Inclination OK");

				if (cb.referenceBody == vessel.mainBody)
				{
					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					GUILayout.Label("Relative inclination:");
					GUILayout.Label(Vector3d.Angle(vessel.orbit.GetOrbitNormal(), cb.orbit.GetOrbitNormal()).ToString("F3") + "°", GUILayout.ExpandWidth(false));
					GUILayout.EndHorizontal();
					//if (debugging) Debug.Log("[VOID] cb Relative inclination OK");
				}

				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				GUILayout.Label("Distance:");
				GUILayout.Label(Tools.MuMech_ToSI((vessel.mainBody.position - cb.position).magnitude) + "m", GUILayout.ExpandWidth(false));
				GUILayout.EndHorizontal();

				//if (debugging) Debug.Log("[VOID] Distance OK");

				//SUN2PLANET:
				if (vessel.mainBody.bodyName == "Sun" && cb.referenceBody == vessel.mainBody)
				{
					Tools.display_transfer_angles_SUN2PLANET(cb, vessel);
					//if (debugging) Debug.Log("[VOID] SUN2PLANET OK");
				}

				//PLANET2PLANET
				else if (vessel.mainBody.referenceBody.bodyName == "Sun" && cb.referenceBody == vessel.mainBody.referenceBody)
				{
					Tools.display_transfer_angles_PLANET2PLANET(cb, vessel);
					//if (debugging) Debug.Log("[VOID] PLANET2PLANET OK");
				}

				//PLANET2MOON
				else if (vessel.mainBody.referenceBody.bodyName == "Sun" && cb.referenceBody == vessel.mainBody)
				{
					Tools.display_transfer_angles_PLANET2MOON(cb, vessel);
					//if (debugging) Debug.Log("[VOID] PLANET2MOON OK");
				}

				//MOON2MOON
				else if (vessel.mainBody.referenceBody.referenceBody.bodyName == "Sun" && cb.referenceBody == vessel.mainBody.referenceBody)
				{
					Tools.display_transfer_angles_MOON2MOON(cb, vessel);
					//if (debugging) Debug.Log("[VOID] MOON2MOON OK");
				}

				//else GUILayout.Label("Transfer angle information\nunavailable for this target");

			}
		}
	}
}