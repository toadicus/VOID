// VOID
//
// VOID_Rendezvous.cs
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
using System.Linq;
using ToadicusTools;
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

			if (this.RegisterModule == null)
			{
				this.RegisterModule = this.core.Modules.Where(m => typeof(VOID_VesselRegister).IsAssignableFrom(m.GetType())).FirstOrDefault() as VOID_VesselRegister;
			}

			GUILayout.BeginVertical();

			//display both
			//Show Target Info
			GUILayout.Label("Target:", VOID_Styles.labelCenterBold);
			if (FlightGlobals.fetch.VesselTarget != null)
			{
				//a KSP Target (body or vessel) is selected
				if (FlightGlobals.fetch.vesselTargetMode == VesselTargetModes.Direction)
				{
					//a Body is selected
					rendezbody = vessel.patchedConicSolver.targetBody;
					display_rendezvous_info(null, rendezbody);
				}
				else if (FlightGlobals.fetch.vesselTargetMode == VesselTargetModes.DirectionAndVelocity)
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
				GUILayout.Label("No Target Selected", VOID_Styles.labelCenterBold);
			}

			//Show Vessel Register vessel info
			if (untoggleRegisterInfo == false && this.RegisterModule != default(IVOID_Module))
			{
				GUILayout.Label("Vessel Register:", VOID_Styles.labelCenterBold);
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
					GUILayout.Label("No Vessel Selected", VOID_Styles.labelCenterBold);
				}
			}

			untoggleRegisterInfo.value = GUILayout.Toggle(untoggleRegisterInfo, "Hide Vessel Register Info");

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(" ", GUILayout.ExpandWidth(true));
			if (GUILayout.Button("Close", GUILayout.ExpandWidth(false))) this.toggleActive = false;
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
				GUILayout.Label(v.vesselName, VOID_Styles.labelCenterBold, GUILayout.ExpandWidth(true));
				GUILayout.EndHorizontal();

				if (v.situation == Vessel.Situations.ESCAPING || v.situation == Vessel.Situations.FLYING || v.situation == Vessel.Situations.ORBITING || v.situation == Vessel.Situations.SUB_ORBITAL)
				{
					// Toadicus edit: added local sidereal longitude.
					// Toadicus edit: added local sidereal longitude.
					double LSL = v.longitude + v.orbit.referenceBody.rotationAngle;
					LSL = VOID_Tools.FixDegreeDomain (LSL);

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

					// Toadicus edit: added local sidereal longitude.
					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					GUILayout.Label("Local Sidereal Longitude:");
					GUILayout.Label(LSL.ToString("F3") + "°", VOID_Styles.labelRight);
					GUILayout.EndHorizontal();

					toggleExtendedOrbital.value = GUILayout.Toggle(toggleExtendedOrbital, "Extended info");

					if (toggleExtendedOrbital)
					{
						GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
						GUILayout.Label("Period:");
						GUILayout.Label(VOID_Tools.ConvertInterval(v.orbit.period), GUILayout.ExpandWidth(false));
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
					}
				}
				else
				{
					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					GUILayout.Label("Latitude:");
					GUILayout.Label(VOID_Tools.GetLatitudeString(vessel), GUILayout.ExpandWidth(false));
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					GUILayout.Label("Longitude:");
					GUILayout.Label(VOID_Tools.GetLongitudeString(vessel), GUILayout.ExpandWidth(false));
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
				GUILayout.Label(cb.bodyName, VOID_Styles.labelCenterBold);

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
					VOID_Tools.display_transfer_angles_SUN2PLANET(cb, vessel);
					//if (debugging) Debug.Log("[VOID] SUN2PLANET OK");
				}

				//PLANET2PLANET
				else if (vessel.mainBody.referenceBody.bodyName == "Sun" && cb.referenceBody == vessel.mainBody.referenceBody)
				{
					VOID_Tools.display_transfer_angles_PLANET2PLANET(cb, vessel);
					//if (debugging) Debug.Log("[VOID] PLANET2PLANET OK");
				}

				//PLANET2MOON
				else if (vessel.mainBody.referenceBody.bodyName == "Sun" && cb.referenceBody == vessel.mainBody)
				{
					VOID_Tools.display_transfer_angles_PLANET2MOON(cb, vessel);
					//if (debugging) Debug.Log("[VOID] PLANET2MOON OK");
				}

				//MOON2MOON
				else if (vessel.mainBody.referenceBody.referenceBody.bodyName == "Sun" && cb.referenceBody == vessel.mainBody.referenceBody)
				{
					VOID_Tools.display_transfer_angles_MOON2MOON(cb, vessel);
					//if (debugging) Debug.Log("[VOID] MOON2MOON OK");
				}

				//else GUILayout.Label("Transfer angle information\nunavailable for this target");

			}
		}
	}
}