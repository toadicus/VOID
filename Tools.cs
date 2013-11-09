//
//  Tools.cs
//
//  Author:
//       toadicus
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
//
//  This software uses VesselSimulator and Engineer.Extensions from Engineer Redux.
//  Engineer Redux (c) 2013 cybutek
//  Used by permission.
//
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using UnityEngine;

namespace VOID
{
	public static class VOIDLabels
	{
		public static string void_primary = "Primary";
		public static string void_altitude_asl = "Altitude (ASL)";
		public static string void_velocity = "Velocity";
		public static string void_apoapsis = "Apoapsis";
		public static string void_periapsis = "Periapsis";
	}

	public static class Tools
	{
		// Toadicus edit: Added re-implementation of the CBAttributeMap.GetAtt function that does not fire a debug message to the game screen.
		public static CBAttributeMap.MapAttribute Toadicus_GetAtt(Vessel vessel)
		{
			CBAttributeMap.MapAttribute mapAttribute;

			try
			{
				CBAttributeMap BiomeMap = vessel.mainBody.BiomeMap;
				double lat = vessel.latitude * Math.PI / 180d;
				double lon = vessel.longitude * Math.PI / 180d;

				lon -= Math.PI / 2d;

				if (lon < 0d)
				{
					lon += 2d * Math.PI;
				}

				float v = (float)(lat / Math.PI) + 0.5f;
				float u = (float)(lon / (2d * Math.PI));

				Color pixelBilinear = BiomeMap.Map.GetPixelBilinear (u, v);
				mapAttribute = BiomeMap.defaultAttribute;

				if (BiomeMap.Map != null)
				{
					if (BiomeMap.exactSearch)
					{
						for (int i = 0; i < BiomeMap.Attributes.Length; ++i)
						{
							if (pixelBilinear == BiomeMap.Attributes[i].mapColor)
							{
								mapAttribute = BiomeMap.Attributes[i];
							}
						}
					}
					else
					{
						float zero = 0;
						float num = 1 / zero;
						for (int j = 0; j < BiomeMap.Attributes.Length; ++j)
						{
							Color mapColor = BiomeMap.Attributes [j].mapColor;
							float sqrMagnitude = ((Vector4)(mapColor - pixelBilinear)).sqrMagnitude;
							if (sqrMagnitude < num)
							{
								bool testCase = true;
								if (BiomeMap.nonExactThreshold != -1)
								{
									testCase = (sqrMagnitude < BiomeMap.nonExactThreshold);
								}
								if (testCase)
								{
									mapAttribute = BiomeMap.Attributes[j];
									num = sqrMagnitude;
								}
							}
						}
					}
				}
			}
			catch (NullReferenceException)
			{
				mapAttribute = new CBAttributeMap.MapAttribute();
				mapAttribute.name = "N/A";
			}

			return mapAttribute;
		}

		public static string GetLongitudeString(Vessel vessel, string format="F4")
		{
			string dir_long = "W";
			double v_long = vessel.longitude;

			v_long = FixDegreeDomain(v_long);

			if (v_long < -180d)
			{
				v_long += 360d;
			}
			if (v_long >= 180)
			{
				v_long -= 360d;
			}

			if (v_long > 0) dir_long = "E";

			return string.Format("{0}° {1}", Math.Abs(v_long).ToString(format), dir_long);
		}

		public static string GetLatitudeString(Vessel vessel, string format="F4")
		{
			string dir_lat = "S";
			double v_lat = vessel.latitude;
			if (v_lat > 0) dir_lat = "N";

			return string.Format("{0}° {1}", Math.Abs(v_lat).ToString(format), dir_lat);
		}

        ///////////////////////////////////////////////////////////////////////////////

        //For MuMech_get_heading()
        public class MuMech_MovingAverage
        {
            private double[] store;
            private int storeSize;
            private int nextIndex = 0;

            public double value
            {
                get
                {
                    double tmp = 0;
                    foreach (double i in store)
                    {
                        tmp += i;
                    }
                    return tmp / storeSize;
                }
                set
                {
                    store[nextIndex] = value;
                    nextIndex = (nextIndex + 1) % storeSize;
                }
            }

            public MuMech_MovingAverage(int size = 10, double startingValue = 0)
            {
                storeSize = size;
                store = new double[size];
                force(startingValue);
            }

            public void force(double newValue)
            {
                for (int i = 0; i < storeSize; i++)
                {
                    store[i] = newValue;
                }
            }

            public static implicit operator double(MuMech_MovingAverage v)
            {
                return v.value;
            }

            public override string ToString()
            {
                return value.ToString();
            }

            public string ToString(string format)
            {
                return value.ToString(format);
            }
        }

        //From http://svn.mumech.com/KSP/trunk/MuMechLib/VOID.vesselState.cs
        public static double MuMech_get_heading(Vessel vessel)
        {
            Vector3d CoM = vessel.findWorldCenterOfMass();
            Vector3d up = (CoM - vessel.mainBody.position).normalized;
            Vector3d north = Vector3d.Exclude(up, (vessel.mainBody.position + vessel.mainBody.transform.up * (float)vessel.mainBody.Radius) - CoM).normalized;

            Quaternion rotationSurface = Quaternion.LookRotation(north, up);
            Quaternion rotationvesselSurface = Quaternion.Inverse(Quaternion.Euler(90, 0, 0) * Quaternion.Inverse(vessel.transform.rotation) * rotationSurface);
            MuMech_MovingAverage vesselHeading = new MuMech_MovingAverage();
            vesselHeading.value = rotationvesselSurface.eulerAngles.y;
            return vesselHeading.value * 10;    // *10 by me
        }

        //From http://svn.mumech.com/KSP/trunk/MuMechLib/MuUtils.cs
        public static string MuMech_ToSI(double d)
        {
            int digits = 2;
            double exponent = Math.Log10(Math.Abs(d));
            if (Math.Abs(d) >= 1)
            {
                switch ((int)Math.Floor(exponent))
                {
                    case 0:
                    case 1:
                    case 2:
                        return d.ToString("F" + digits);
                    case 3:
                    case 4:
                    case 5:
                        return (d / 1e3).ToString("F" + digits) + "k";
                    case 6:
                    case 7:
                    case 8:
                        return (d / 1e6).ToString("F" + digits) + "M";
                    case 9:
                    case 10:
                    case 11:
                        return (d / 1e9).ToString("F" + digits) + "G";
                    case 12:
                    case 13:
                    case 14:
                        return (d / 1e12).ToString("F" + digits) + "T";
                    case 15:
                    case 16:
                    case 17:
                        return (d / 1e15).ToString("F" + digits) + "P";
                    case 18:
                    case 19:
                    case 20:
                        return (d / 1e18).ToString("F" + digits) + "E";
                    case 21:
                    case 22:
                    case 23:
                        return (d / 1e21).ToString("F" + digits) + "Z";
                    default:
                        return (d / 1e24).ToString("F" + digits) + "Y";
                }
            }
            else if (Math.Abs(d) > 0)
            {
                switch ((int)Math.Floor(exponent))
                {
                    case -1:
                    case -2:
                    case -3:
                        return (d * 1e3).ToString("F" + digits) + "m";
                    case -4:
                    case -5:
                    case -6:
                        return (d * 1e6).ToString("F" + digits) + "μ";
                    case -7:
                    case -8:
                    case -9:
                        return (d * 1e9).ToString("F" + digits) + "n";
                    case -10:
                    case -11:
                    case -12:
                        return (d * 1e12).ToString("F" + digits) + "p";
                    case -13:
                    case -14:
                    case -15:
                        return (d * 1e15).ToString("F" + digits) + "f";
                    case -16:
                    case -17:
                    case -18:
                        return (d * 1e18).ToString("F" + digits) + "a";
                    case -19:
                    case -20:
                    case -21:
                        return (d * 1e21).ToString("F" + digits) + "z";
                    default:
                        return (d * 1e24).ToString("F" + digits) + "y";
                }
            }
            else
            {
                return "0";
            }
        }

        public static string ConvertInterval(double seconds)
        {
            string format_1 = "{0:D1}y {1:D1}d {2:D2}h {3:D2}m {4:D2}.{5:D1}s";
            string format_2 = "{0:D1}d {1:D2}h {2:D2}m {3:D2}.{4:D1}s";
            string format_3 = "{0:D2}h {1:D2}m {2:D2}.{3:D1}s";

			TimeSpan interval;

			try
			{
            	interval = TimeSpan.FromSeconds(seconds);
			}
			catch (OverflowException)
			{
				return "NaN";
			}

            int years = interval.Days / 365;

            string output;
            if (years > 0)
            {
                output = string.Format(format_1,
                    years,
                    interval.Days - (years * 365), //  subtract years * 365 for accurate day count
                    interval.Hours,
                    interval.Minutes,
                    interval.Seconds,
                    interval.Milliseconds.ToString().Substring(0, 1));
            }
            else if (interval.Days > 0)
            {
                output = string.Format(format_2,
                    interval.Days,
                    interval.Hours,
                    interval.Minutes,
                    interval.Seconds,
                    interval.Milliseconds.ToString().Substring(0, 1));
            }
            else
            {
                output = string.Format(format_3,
                    interval.Hours,
                    interval.Minutes,
                    interval.Seconds,
                    interval.Milliseconds.ToString().Substring(0, 1));
            }
            return output;
        }

        public static string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        //transfer angles

        public static double Nivvy_CalcTransferPhaseAngle(double r_current, double r_target, double grav_param)
        {
            double T_target = (2 * Math.PI) * Math.Sqrt(Math.Pow((r_target / 1000), 3) / (grav_param / 1000000000));
            double T_transfer = (2 * Math.PI) * Math.Sqrt(Math.Pow((((r_target / 1000) + (r_current / 1000)) / 2), 3) / (grav_param / 1000000000));
            return 360 * (0.5 - (T_transfer / (2 * T_target)));
        }

        public static double Younata_DeltaVToGetToOtherBody(double mu, double r1, double r2)
        {
            /*
            def deltaVToGetToOtherBody(mu, r1, r2):
            # mu = gravity param of common orbiting body of r1 and r2
            # (e.g. for mun to minmus, mu is kerbin's gravity param
            # r1 = initial body's orbit radius
            # r2 = target body's orbit radius
		
            # return value is km/s
            sur1 = math.sqrt(mu / r1)
            sr1r2 = math.sqrt(float(2*r2)/float(r1+r2))
            mult = sr1r2 - 1
            return sur1 * mult
            */
            double sur1, sr1r2, mult;
            sur1 = Math.Sqrt(mu / r1);
            sr1r2 = Math.Sqrt((2 * r2) / (r1 + r2));
            mult = sr1r2 - 1;
            return sur1 * mult;
        }

        public static double Younata_DeltaVToExitSOI(double mu, double r1, double r2, double v)
        {
            /*
            def deltaVToExitSOI(mu, r1, r2, v):
            # mu = gravity param of current body
            # r1 = current orbit radius
            # r2 = SOI radius
            # v = SOI exit velocity
            foo = r2 * (v**2) - 2 * mu
            bar = r1 * foo + (2 * r2 * mu)
            r = r1*r2
            return math.sqrt(bar / r)
            */
            double foo = r2 * Math.Pow(v, 2) - 2 * mu;
            double bar = r1 * foo + (2 * r2 * mu);
            double r = r1 * r2;
            return Math.Sqrt(bar / r);
        }

        public static double Younata_TransferBurnPoint(double r, double v, double angle, double mu)
        {
            /*
            def transferBurnPoint(r, v, angle, mu):
            # r = parking orbit radius
            # v = ejection velocity
            # angle = phase angle (from function phaseAngle())
            # mu = gravity param of current body.
            epsilon = ((v**2)/2) - (mu / r)
            h = r * v * math.sin(angle)
            e = math.sqrt(1 + ((2 * epsilon * h**2)/(mu**2)))
            theta = math.acos(1.0 / e)
            degrees = theta * (180.0 / math.pi)
            return 180 - degrees
            */
            double epsilon, h, ee, theta, degrees;
            epsilon = (Math.Pow(v, 2) / 2) - (mu / r);
            h = r * v * Math.Sin(angle);
            ee = Math.Sqrt(1 + ((2 * epsilon * Math.Pow(h, 2)) / Math.Pow(mu, 2)));
            theta = Math.Acos(1.0 / ee);
            degrees = theta * (180.0 / Math.PI);
            return 180 - degrees;
            // returns the ejection angle
        }

        public static double Adammada_CurrrentPhaseAngle(double body_LAN, double body_orbitPct, double origin_LAN, double origin_orbitPct)
        {
            double angle = (body_LAN / 360 + body_orbitPct) - (origin_LAN / 360 + origin_orbitPct);
            if (angle > 1) angle = angle - 1;
            if (angle < 0) angle = angle + 1;
            if (angle > 0.5) angle = angle - 1;
            angle = angle * 360;
            return angle;
        }

        public static double Adammada_CurrentEjectionAngle(double vessel_long, double origin_rotAngle, double origin_LAN, double origin_orbitPct)
        {
            //double eangle = ((FlightGlobals.ActiveVOID.vessel.longitude + orbiting.rotationAngle) - (orbiting.orbit.LAN / 360 + orbiting.orbit.orbitPercent) * 360);
            double eangle = ((vessel_long + origin_rotAngle) - (origin_LAN / 360 + origin_orbitPct) * 360);

            while (eangle < 0) eangle = eangle + 360;
            while (eangle > 360) eangle = eangle - 360;
            if (eangle < 270) eangle = 90 - eangle;
            else eangle = 450 - eangle;
            return eangle;
        }

        public static double mrenigma03_calcphase(Vessel vessel, CelestialBody target)   //calculates phase angle between the current body and target body
        {
            Vector3d vecthis = new Vector3d();
            Vector3d vectarget = new Vector3d();
            vectarget = target.orbit.getRelativePositionAtUT(Planetarium.GetUniversalTime());

            if ((vessel.mainBody.name == "Sun") || (vessel.mainBody.referenceBody.referenceBody.name == "Sun"))
            {
                vecthis = vessel.orbit.getRelativePositionAtUT(Planetarium.GetUniversalTime());
            }
            else
            {
                vecthis = vessel.mainBody.orbit.getRelativePositionAtUT(Planetarium.GetUniversalTime());
            }

            vecthis = Vector3d.Project(new Vector3d(vecthis.x, 0, vecthis.z), vecthis);
            vectarget = Vector3d.Project(new Vector3d(vectarget.x, 0, vectarget.z), vectarget);

            Vector3d prograde = new Vector3d();
            prograde = Quaternion.AngleAxis(90, Vector3d.forward) * vecthis;

            double phase = Vector3d.Angle(vecthis, vectarget);

            if (Vector3d.Angle(prograde, vectarget) > 90) phase = 360 - phase;

            return (phase + 360) % 360;
        }

		public static double FixAngleDomain(double Angle, bool Degrees = false)
		{
			double Extent = 2d * Math.PI;
			if (Degrees) {
				Extent = 360d;
			}

			Angle = Angle % (Extent);
			if (Angle < 0d)
			{
				Angle += Extent;
			}

			return Angle;
		}

		public static double FixDegreeDomain(double Angle)
		{
			return FixAngleDomain (Angle, true);
		}

        public static double adjustCurrPhaseAngle(double transfer_angle, double curr_phase)
        {
            if (transfer_angle < 0)
            {
                if (curr_phase > 0) return (-1 * (360 - curr_phase));
                else if (curr_phase < 0) return curr_phase;
            }
            else if (transfer_angle > 0)
            {
                if (curr_phase > 0) return curr_phase;
                else if (curr_phase < 0) return (360 + curr_phase);
            }
            return curr_phase;
        }

        public static double adjust_current_ejection_angle(double curr_ejection)
        {
            //curr_ejection WILL need to be adjusted once for all transfers as it returns values ranging -180 to 180
            // need 0-360 instead
            //
            // ie i have -17 in the screenshot
            // need it to show 343
            //
            // do this
            //
            // if < 0, add curr to 360  // 360 + (-17) = 343
            // else its good as it is

            if (curr_ejection < 0) return 360 + curr_ejection;
            else return curr_ejection;

        }

        public static double adjust_transfer_ejection_angle(double trans_ejection, double trans_phase)
        {
            // if transfer_phase_angle < 0 its a lower transfer
            //180 + curr_ejection
            // else if transfer_phase_angle > 0 its good as it is

            if (trans_phase < 0) return 180 + trans_ejection;
            else return trans_ejection;

        }

        public static double TrueAltitude(Vessel vessel)
		{
			double trueAltitude = vessel.orbit.altitude - vessel.terrainAltitude;

			// HACK: This assumes that on worlds with oceans, all water is fixed at 0 m, and water covers the whole surface at 0 m.
			if (vessel.terrainAltitude < 0 && vessel.mainBody.ocean )
			{
				trueAltitude = vessel.orbit.altitude;
			}

			return trueAltitude;
		}

		public static string get_heading_text(double heading)
		{
			if (heading > 348.75 || heading <= 11.25) return "N";
			else if (heading > 11.25 && heading <= 33.75) return "NNE";
			else if (heading > 33.75 && heading <= 56.25) return "NE";
			else if (heading > 56.25 && heading <= 78.75) return "ENE";
			else if (heading > 78.75 && heading <= 101.25) return "E";
			else if (heading > 101.25 && heading <= 123.75) return "ESE";
			else if (heading > 123.75 && heading <= 146.25) return "SE";
			else if (heading > 146.25 && heading <= 168.75) return "SSE";
			else if (heading > 168.75 && heading <= 191.25) return "S";
			else if (heading > 191.25 && heading <= 213.75) return "SSW";
			else if (heading > 213.75 && heading <= 236.25) return "SW";
			else if (heading > 236.25 && heading <= 258.75) return "WSW";
			else if (heading > 258.75 && heading <= 281.25) return "W";
			else if (heading > 281.25 && heading <= 303.75) return "WNW";
			else if (heading > 303.75 && heading <= 326.25) return "NW";
			else if (heading > 326.25 && heading <= 348.75) return "NNW";
			else return "";
		}
				
		private static ScreenMessage debugmsg = new ScreenMessage("", 2f, ScreenMessageStyle.UPPER_RIGHT);

		[System.Diagnostics.Conditional("DEBUG")]
		public static void PostDebugMessage(string Msg)
		{
			if (HighLogic.LoadedScene > GameScenes.SPACECENTER)
			{
				debugmsg.message = Msg;
				ScreenMessages.PostScreenMessage(debugmsg, true);
			}

			KSPLog.print(Msg);
		}
	}
}