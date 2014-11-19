// VOID
//
// VOID_Data.cs
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
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	public static class VOID_Data
	{
		private static Dictionary<int, IVOID_DataValue> dataValues = new Dictionary<int, IVOID_DataValue>();
		public static Dictionary<int, IVOID_DataValue> DataValues
		{
			get
			{
				return dataValues;
			}
		}

		#region Constants

		private static double kerbinGee;

		public static double KerbinGee
		{
			get
			{
				if (kerbinGee == default(double))
				{
					kerbinGee = core.HomeBody.gravParameter / (core.HomeBody.Radius * core.HomeBody.Radius);
				}

				return kerbinGee;
			}
		}

		#endregion

		#region Core Data

		public static VOID_Core core
		{
			get
			{
				if (HighLogic.LoadedSceneIsEditor)
				{
					return VOID_EditorCore.Instance;
				}
				else
				{
					return VOID_Core.Instance;
				}
			}
		}

		#endregion

		#region Atmosphere

		public static readonly VOID_DoubleValue atmDensity =
			new VOID_DoubleValue(
				"Atmosphere Density",
				new Func<double>(() => core.vessel.atmDensity * 1000f),
				"g/m³"
			);

		public static readonly VOID_FloatValue atmLimit =
			new VOID_FloatValue(
				"Atmosphere Limit",
				new Func<float>(() => core.vessel.mainBody.maxAtmosphereAltitude),
				"m"
			);

		public static readonly VOID_DoubleValue atmPressure =
			new VOID_DoubleValue(
				"Pressure",
				new Func<double>(() => core.vessel.staticPressure),
				"atm"
			);

		public static readonly VOID_FloatValue temperature =
			new VOID_FloatValue(
				"Temperature",
				new Func<float>(() => core.vessel.flightIntegrator.getExternalTemperature()),
				"°C"
			);

		#endregion

		#region Attitude

		public static readonly VOID_StrValue vesselHeading =
			new VOID_StrValue(
				"Heading",
				delegate()
				{
					double heading = core.vessel.getSurfaceHeading();
					string cardinal = VOID_Tools.get_heading_text(heading);

					return string.Format(
						"{0}° {1}",
						heading.ToString("F2"),
						cardinal
					);
				}
			);

		public static readonly VOID_DoubleValue vesselPitch =
			new VOID_DoubleValue(
				"Pitch",
				() => core.vessel.getSurfacePitch(),
				"°"
			);

		#endregion

		#region Career

		public static readonly VOID_StrValue fundingStatus =
			new VOID_StrValue(
				string.Intern("Funds"),
				delegate()
				{
					if (VOID_CareerStatus.Instance == null)
					{
						return string.Empty;
					}

					return string.Format("{0} ({1})",
						VOID_CareerStatus.Instance.currentFunds.ToString("#,#.##"),
						VOID_CareerStatus.formatDelta(VOID_CareerStatus.Instance.lastFundsChange)
					);
				}
			);

		public static readonly VOID_StrValue reputationStatus =
			new VOID_StrValue(
				string.Intern("Reputation"),
				delegate()
				{
					if (VOID_CareerStatus.Instance == null)
					{
						return string.Empty;
					}

					return string.Format("{0} ({1})",
						VOID_CareerStatus.Instance.currentReputation.ToString("#,#.##"),
						VOID_CareerStatus.formatDelta(VOID_CareerStatus.Instance.lastRepChange)
					);
				}
			);

		public static readonly VOID_StrValue scienceStatus =
			new VOID_StrValue(
				string.Intern("Science"),
				delegate()
				{
					if (VOID_CareerStatus.Instance == null)
					{
						return string.Empty;
					}

					return string.Format("{0} ({1})",
						VOID_CareerStatus.Instance.currentScience.ToString("#,#.##"),
						VOID_CareerStatus.formatDelta(VOID_CareerStatus.Instance.lastScienceChange)
					);
				}
			);

		#endregion

		#region Control

		public static readonly VOID_FloatValue mainThrottle =
			new VOID_FloatValue(
				"Throttle",
				new Func<float>(() => core.vessel.ctrlState.mainThrottle * 100f),
				"%"
			);

		#endregion

		#region Engineering

		public static readonly VOID_IntValue partCount =
			new VOID_IntValue(
				"Parts",
				new Func<int>(() => core.vessel.Parts.Count),
				""
			);

		#region Mass

		public static readonly VOID_StrValue comboResourceMass =
			new VOID_StrValue(
				"Resource Mass (curr / total)",
				delegate()
				{
					return string.Format("{0} / {1}",
						stageResourceMass.ValueUnitString("F3"),
						resourceMass.ValueUnitString("F3")
					);
				}
			);

		public static readonly VOID_DoubleValue resourceMass =
			new VOID_DoubleValue(
				"Resource Mass",
				delegate()
				{
					if (core.Stages == null || core.LastStage == null)
					{
						return double.NaN;
					}

					return core.LastStage.totalMass - core.LastStage.totalBaseMass;
				},
				"tons"
			);

		public static readonly VOID_DoubleValue stageResourceMass =
			new VOID_DoubleValue(
				"Resource Mass (Stage)",
				delegate()
				{
					if (core.LastStage == null)
					{
						return double.NaN;
					}

					return core.LastStage.mass - core.LastStage.baseMass;
				},
				"tons"
			);

		public static readonly VOID_DoubleValue totalMass =
			new VOID_DoubleValue(
				"Total Mass",
				delegate()
				{
					if (core.Stages == null || core.LastStage == null)
					{
						return double.NaN;
					}

					return core.LastStage.totalMass;
				},
				"tons"
			);

		#endregion

		#region DeltaV

		public static readonly VOID_DoubleValue stageDeltaV =
			new VOID_DoubleValue(
				"DeltaV (Current Stage)",
				delegate()
				{
					if (core.Stages == null || core.LastStage == null)
						return double.NaN;
					return core.LastStage.deltaV;
				},
				"m/s"
			);

		public static readonly VOID_DoubleValue totalDeltaV =
			new VOID_DoubleValue(
				"DeltaV (Total)",
				delegate()
				{
					if (core.Stages == null || core.LastStage == null)
						return double.NaN;
					return core.LastStage.totalDeltaV;
				},
				"m/s"
			);

		#endregion

		#region Propulsion

		public static readonly VOID_StrValue currmaxThrustWeight =
			new VOID_StrValue(
				"T:W (curr/max)",
				delegate()
				{
					if (core.Stages == null || core.LastStage == null)
						return "N/A";

					return string.Format(
						"{0} / {1}",
						(VOID_Data.currThrustWeight.Value).ToString("F2"),
						(VOID_Data.maxThrustWeight.Value).ToString("F2")
					);
				}
			);

		public static readonly VOID_StrValue currmaxThrust =
			new VOID_StrValue(
				"Thrust (curr/max)",
				delegate()
				{
					if (core.Stages == null || core.LastStage == null)
						return "N/A";

					double currThrust = core.LastStage.actualThrust;
					double maxThrust = core.LastStage.thrust;

					return string.Format(
						"{0} / {1}",
						currThrust.ToString("F1"),
						maxThrust.ToString("F1")
					);
				}
			);

		public static readonly VOID_DoubleValue stageMassFlow =
			new VOID_DoubleValue(
				"Stage Mass Flow",
				delegate()
				{
					if (core.LastStage == null)
					{
						return double.NaN;
					}

					double stageIsp = core.LastStage.isp;
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

		public static readonly VOID_DoubleValue stageNominalThrust =
			new VOID_DoubleValue(
				"Nominal Stage Thrust",
				delegate()
				{
					if (core.LastStage == null)
					{
						return double.NaN;
					}

					if (core.LastStage.actualThrust == 0d)
					{
						return core.LastStage.thrust;
					}
					else
					{
						return core.LastStage.actualThrust;
					}
				},
				"kN"
			);

		#endregion

		#region Kinetics

		public static readonly VOID_DoubleValue currThrustWeight =
			new VOID_DoubleValue(
				"T:W Ratio",
				delegate()
				{
					if (core.LastStage == null)
					{
						return double.NaN;
					}

					return core.LastStage.actualThrustToWeight;
				},
				""
			);



		public static readonly VOID_DoubleValue maxThrustWeight =
			new VOID_DoubleValue(
				"T:W Ratio",
				delegate()
				{
					if (core.LastStage == null)
					{
						return double.NaN;
					}

					return core.LastStage.thrustToWeight;
				},
				""
			);

		public static readonly VOID_DoubleValue nominalThrustWeight =
			new VOID_DoubleValue(
				"Thrust-to-Weight Ratio",
				delegate()
				{
					if (HighLogic.LoadedSceneIsEditor || currThrustWeight.Value == 0d)
					{
						return maxThrustWeight.Value;
					}

					return currThrustWeight.Value;
				},
				""
			);

		public static readonly VOID_DoubleValue surfaceThrustWeight =
			new VOID_DoubleValue(
				"Max T:W @ surface",
				delegate()
				{
					if (core.Stages == null || core.LastStage == null)
						return double.NaN;

					double maxThrust = core.LastStage.thrust;
					double mass = core.LastStage.totalMass;
					double gravity = (VOID_Core.Constant_G * core.vessel.mainBody.Mass) /
					               (core.vessel.mainBody.Radius * core.vessel.mainBody.Radius);
					double weight = mass * gravity;

					return maxThrust / weight;
				},
				""
			);

		public static readonly VOID_Vector3dValue vesselThrustOffset =
			new VOID_Vector3dValue(
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

					Transform vesselTransform = core.vessel.transform;

					thrustPos = vesselTransform.InverseTransformPoint(thrustPos);
					thrustDir = vesselTransform.InverseTransformDirection(thrustDir);

					Vector3d thrustOffset = VectorTools.PointDistanceToLine(
						                      thrustPos, thrustDir.normalized, core.vessel.findLocalCenterOfMass());

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

		#endregion

		#region Air Breathing

		public static readonly VOID_StrValue intakeAirStatus =
			new VOID_StrValue(
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

		#endregion

		#region Crew

		public static readonly VOID_IntValue vesselCrewCount =
			new VOID_IntValue(
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

		public static readonly VOID_IntValue vesselCrewCapacity =
			new VOID_IntValue(
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

		#endregion

		#endregion

		#region Location

		public static readonly VOID_DoubleValue downrangeDistance =
			new VOID_DoubleValue(
				"Downrange Distance",
				delegate()
				{

					if (core.vessel == null ||
					  Planetarium.fetch == null ||
					  core.vessel.mainBody != Planetarium.fetch.Home)
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

		public static readonly VOID_StrValue surfLatitude =
			new VOID_StrValue(
				"Latitude",
				new Func<string>(() => VOID_Tools.GetLatitudeString(core.vessel))
			);

		public static readonly VOID_StrValue surfLongitude =
			new VOID_StrValue(
				"Longitude",
				new Func<string>(() => VOID_Tools.GetLongitudeString(core.vessel))
			);

		public static readonly VOID_DoubleValue trueAltitude =
			new VOID_DoubleValue(
				"Altitude (true)",
				delegate()
				{
					double alt_true = core.vessel.orbit.altitude - core.vessel.terrainAltitude;
					// HACK: This assumes that on worlds with oceans, all water is fixed at 0 m,
					// and water covers the whole surface at 0 m.
					if (core.vessel.terrainAltitude < 0 && core.vessel.mainBody.ocean)
						alt_true = core.vessel.orbit.altitude;
					return alt_true;
				},
				"m"
			);

		#endregion

		#region Kinematics

		public static readonly VOID_DoubleValue geeForce =
			new VOID_DoubleValue(
				"G-force",
				new Func<double>(() => core.vessel.geeForce),
				"gees"
			);

		public static readonly VOID_DoubleValue horzVelocity =
			new VOID_DoubleValue(
				"Horizontal speed",
				new Func<double>(() => core.vessel.horizontalSrfSpeed),
				"m/s"
			);

		public static readonly VOID_DoubleValue surfVelocity =
			new VOID_DoubleValue(
				"Surface velocity",
				new Func<double>(() => core.vessel.srf_velocity.magnitude),
				"m/s"
			);

		public static readonly VOID_DoubleValue vertVelocity =
			new VOID_DoubleValue(
				"Vertical speed",
				new Func<double>(() => core.vessel.verticalSpeed),
				"m/s"
			);

		public static readonly VOID_DoubleValue vesselAccel =
			new VOID_DoubleValue(
				"Acceleration",
				() => geeForce * KerbinGee,
				"m/s²"
			);

		public static readonly VOID_DoubleValue vesselAngularVelocity =
			new VOID_DoubleValue(
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

		#endregion

		#region Navigation

		public static int upcomingManeuverNodes
		{
			get
			{
				if (core.vessel == null ||
				    core.vessel.patchedConicSolver == null ||
				    core.vessel.patchedConicSolver.maneuverNodes == null)
				{
					return 0;
				}

				return core.vessel.patchedConicSolver.maneuverNodes.Count;
			}
		}

		public static readonly VOID_StrValue burnTimeDoneAtNode =
			new VOID_StrValue(
				"Full burn time to be half done at node",
				delegate()
				{
					if (core.LastStage == null && upcomingManeuverNodes < 1)
					{
						return "N/A";
					}

					ManeuverNode node = core.vessel.patchedConicSolver.maneuverNodes[0];

					if ((node.UT - Planetarium.GetUniversalTime()) < 0)
					{
						return string.Empty;
					}

					double interval = (node.UT - currentNodeBurnDuration) - Planetarium.GetUniversalTime();

					if (double.IsNaN(interval))
					{
						return string.Intern("NaN");
					}

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

		public static readonly VOID_StrValue burnTimeHalfDoneAtNode =
			new VOID_StrValue(
				"Full burn time to be half done at node",
				delegate()
				{
					if (core.LastStage == null && upcomingManeuverNodes < 1)
					{
						return "N/A";
					}

					ManeuverNode node = core.vessel.patchedConicSolver.maneuverNodes[0];

					if ((node.UT - Planetarium.GetUniversalTime()) < 0)
					{
						return string.Empty;
					}

					double interval = (node.UT - currentNodeHalfBurnDuration) - Planetarium.GetUniversalTime();

					if (double.IsNaN(interval))
					{
						return string.Intern("NaN");
					}

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

		public static readonly VOID_DoubleValue currManeuverDeltaV =
			new VOID_DoubleValue(
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

		public static readonly VOID_DoubleValue currManeuverDVRemaining =
			new VOID_DoubleValue(
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

		public static readonly VOID_DoubleValue currentNodeBurnDuration =
			new VOID_DoubleValue(
				"Total Burn Time",
				delegate()
				{
					if (core.LastStage == null || currManeuverDeltaV.Value == double.NaN)
					{
						return double.NaN;
					}

					double stageThrust = stageNominalThrust;

					return burnTime(currManeuverDeltaV.Value, totalMass, stageMassFlow, stageThrust);
				},
				"s"
			);

		public static readonly VOID_DoubleValue currentNodeBurnRemaining =
			new VOID_DoubleValue(
				"Burn Time Remaining",
				delegate()
				{
					if (core.LastStage == null || currManeuverDVRemaining == double.NaN)
					{
						return double.NaN;
					}

					double stageThrust = stageNominalThrust;

					return burnTime(currManeuverDVRemaining, totalMass, stageMassFlow, stageThrust);
				},
				"s"
			);

		public static readonly VOID_DoubleValue currentNodeHalfBurnDuration =
			new VOID_DoubleValue(
				"Half Burn Time",
				delegate()
				{
					if (core.LastStage == null || currManeuverDeltaV.Value == double.NaN)
					{
						return double.NaN;
					}

					double stageThrust = stageNominalThrust;

					return burnTime(currManeuverDeltaV.Value / 2d, totalMass, stageMassFlow, stageThrust);
				},
				"s"
			);

		public static readonly VOID_DoubleValue nextManeuverDeltaV =
			new VOID_DoubleValue(
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

		#endregion

		#region Orbits

		public static readonly VOID_StrValue primaryName =
			new VOID_StrValue(
				VOID_Localization.void_primary,
				delegate()
				{
					if (core.vessel == null)
					{
						return string.Empty;
					}
					return core.vessel.mainBody.name;
				}
			);

		public static readonly VOID_DoubleValue orbitAltitude =
			new VOID_DoubleValue(
				"Altitude (ASL)",
				new Func<double>(() => core.vessel.orbit.altitude),
				"m"
			);

		public static readonly VOID_DoubleValue orbitVelocity =
			new VOID_DoubleValue(
				VOID_Localization.void_velocity,
				new Func<double>(() => core.vessel.orbit.vel.magnitude),
				"m/s"
			);

		public static readonly VOID_DoubleValue orbitApoAlt =
			new VOID_DoubleValue(
				VOID_Localization.void_apoapsis,
				new Func<double>(() => core.vessel.orbit.ApA),
				"m"
			);

		public static readonly VOID_DoubleValue oribtPeriAlt =
			new VOID_DoubleValue(
				VOID_Localization.void_periapsis,
				new Func<double>(() => core.vessel.orbit.PeA),
				"m"
			);

		public static readonly VOID_StrValue timeToApo =
			new VOID_StrValue(
				"Time to Apoapsis",
				new Func<string>(() => VOID_Tools.ConvertInterval(core.vessel.orbit.timeToAp))
			);

		public static readonly VOID_StrValue timeToPeri =
			new VOID_StrValue(
				"Time to Periapsis",
				new Func<string>(() => VOID_Tools.ConvertInterval(core.vessel.orbit.timeToPe))
			);

		public static readonly VOID_DoubleValue orbitInclination =
			new VOID_DoubleValue(
				"Inclination",
				new Func<double>(() => core.vessel.orbit.inclination),
				"°"
			);

		public static readonly VOID_DoubleValue gravityAccel =
			new VOID_DoubleValue(
				"Gravity",
				delegate()
				{
					double orbitRadius = core.vessel.mainBody.Radius +
					                   core.vessel.mainBody.GetAltitude(core.vessel.findWorldCenterOfMass());
					return (VOID_Core.Constant_G * core.vessel.mainBody.Mass) /
					(orbitRadius * orbitRadius);
				},
				"m/s²"
			);

		public static readonly VOID_StrValue orbitPeriod =
			new VOID_StrValue(
				"Period",
				new Func<string>(() => VOID_Tools.ConvertInterval(core.vessel.orbit.period))
			);

		public static readonly VOID_DoubleValue semiMajorAxis =
			new VOID_DoubleValue(
				"Semi-Major Axis",
				new Func<double>(() => core.vessel.orbit.semiMajorAxis),
				"m"
			);

		public static readonly VOID_DoubleValue eccentricity =
			new VOID_DoubleValue(
				"Eccentricity",
				new Func<double>(() => core.vessel.orbit.eccentricity),
				""
			);

		public static readonly VOID_DoubleValue meanAnomaly =
			new VOID_DoubleValue(
				"Mean Anomaly",
				new Func<double>(() => core.vessel.orbit.meanAnomaly * 180d / Math.PI),
				"°"
			);

		public static readonly VOID_DoubleValue trueAnomaly = 
			new VOID_DoubleValue(
				"True Anomaly",
				new Func<double>(() => core.vessel.orbit.trueAnomaly),
				"°"
			);

		public static readonly VOID_DoubleValue eccAnomaly =
			new VOID_DoubleValue(
				"Eccentric Anomaly",
				new Func<double>(() => core.vessel.orbit.eccentricAnomaly * 180d / Math.PI),
				"°"
			);

		public static readonly VOID_DoubleValue longitudeAscNode =
			new VOID_DoubleValue(
				"Long. Ascending Node",
				new Func<double>(() => core.vessel.orbit.LAN),
				"°"
			);

		public static readonly VOID_DoubleValue argumentPeriapsis =
			new VOID_DoubleValue(
				"Argument of Periapsis",
				new Func<double>(() => core.vessel.orbit.argumentOfPeriapsis),
				"°"
			);

		public static readonly VOID_DoubleValue localSiderealLongitude =
			new VOID_DoubleValue(
				"Local Sidereal Longitude",
				new Func<double>(() => VOID_Tools.FixDegreeDomain(
						core.vessel.longitude + core.vessel.orbit.referenceBody.rotationAngle)),
				"°"
			);

		#endregion

		#region Science

		public static readonly VOID_StrValue expSituation =
			new VOID_StrValue(
				"Situation",
				new Func<string>(() => core.vessel.GetExperimentSituation().HumanString())
			);

		public static readonly VOID_StrValue currBiome =
			new VOID_StrValue(
				"Biome",
				new Func<string>(() => VOID_Tools.GetBiome(core.vessel).name)
			);

		#endregion

		#region Surface

		public static readonly VOID_DoubleValue terrainElevation =
			new VOID_DoubleValue(
				"Terrain elevation",
				new Func<double>(() => core.vessel.terrainAltitude),
				"m"
			);

		#endregion

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
			return initialMass / massFlow * (1d - Math.Exp(-deltaV * massFlow / thrust));
		}
	}
}

