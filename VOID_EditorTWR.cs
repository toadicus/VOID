// VOID © 2014 toadicus
//
// This work is licensed under the Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License. To view a
// copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/

using KSP;
using System;
using System.Collections.Generic;
using System.Linq;
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	public class VOID_EditorTWR : VOID_WindowModule, IVOID_EditorModule
	{
		private Dictionary<string, double> bodyGeeValues;
		private List<CelestialBody> sortedBodyList;

		public VOID_EditorTWR() : base()
		{
			this._Name = "IP Thrust-to-Weight Ratios";
		}

		public override void ModuleWindow(int _)
		{
			if (Event.current.type != EventType.Layout)
			{
				return;
			}

			Engineer.VesselSimulator.SimManager.RequestSimulation();

			GUILayout.Label(
				this._Name,
				VOID_EditorCore.Instance.LabelStyles["center_bold"],
				GUILayout.ExpandWidth(true));

			if (bodyGeeValues == null)
			{
				if (FlightGlobals.Bodies != null && FlightGlobals.Bodies.Count > 0)
				{
					this.bodyGeeValues = new Dictionary<string, double>();

					foreach (CelestialBody body in FlightGlobals.Bodies)
					{
						this.bodyGeeValues[body.name] = body.GeeASL;
					}

					this.sortedBodyList = new List<CelestialBody>(FlightGlobals.Bodies);
					this.sortedBodyList.Sort(new CBListComparer());
					this.sortedBodyList.Reverse();

					Debug.Log(string.Format("sortedBodyList: {0}", string.Join("\n\t", this.sortedBodyList.Select(b => b.bodyName).ToArray())));
				}
				else
				{
					GUILayout.BeginVertical();
					GUILayout.BeginHorizontal();
					GUILayout.Label("Unavailable.");
					GUILayout.EndHorizontal();
					GUILayout.EndVertical();
				}
			}
			else
			{
				GUILayout.BeginVertical();

				foreach (CelestialBody body in this.sortedBodyList)
				{
					Tools.PostDebugMessage(this, "Doing label for {0}", body.bodyName);
					GUILayout.BeginHorizontal();

					GUILayout.Label(body.bodyName);
					GUILayout.FlexibleSpace();
					GUILayout.Label(
						(VOID_Data.currThrustWeight.Value / this.bodyGeeValues[body.bodyName]).ToString("0.0##"),
						GUILayout.ExpandWidth(true)
					);

					GUILayout.EndHorizontal();
				}

				GUILayout.EndVertical();
			}

			GUI.DragWindow();
		}
	}

	public class CBListComparer : IComparer<CelestialBody>
	{
		public int Compare(CelestialBody bodyA, CelestialBody bodyB)
		{
			Tools.PostDebugMessage(this, "got bodyA: {0} & bodyB: {1}", bodyA, bodyB);

			if (bodyA == null && bodyB == null)
			{
				Tools.PostDebugMessage(this, "both bodies are null, returning 0");
				return 0;
			}
			if (bodyA == null)
			{
				Tools.PostDebugMessage(this, "bodyA is null, returning -1");
				return -1;
			}
			if (bodyB == null)
			{
				Tools.PostDebugMessage(this, "bodyB is null, returning 1");
				return 1;
			}

			Tools.PostDebugMessage(this, "bodies are not null, carrying on");

			if (object.ReferenceEquals(bodyA, bodyB))
			{
				Tools.PostDebugMessage(this, "bodies are equal, returning 0");
				return 0;
			}

			Tools.PostDebugMessage(this, "bodies are not equal, carrying on");

			if (bodyA.orbitDriver == null)
			{
				Tools.PostDebugMessage(this, "bodyA.orbit is null (bodyA is the sun, returning 1");
				return 1;
			}
			if (bodyB.orbitDriver == null)
			{
				Tools.PostDebugMessage(this, "bodyB.orbit is null (bodyB is the sun, returning -1");
				return -1;
			}

			Tools.PostDebugMessage(this, "orbits are not null, carrying on");

			if (bodyA.orbit.referenceBody == bodyB.orbit.referenceBody)
			{
				Tools.PostDebugMessage(this, "bodies share a parent, comparing SMAs");
				return -bodyA.orbit.semiMajorAxis.CompareTo(bodyB.orbit.semiMajorAxis);
			}

			Tools.PostDebugMessage(this, "orbits do not share a parent, carrying on");

			if (bodyA.hasAncestor(bodyB))
			{
				Tools.PostDebugMessage(this, "bodyA is a moon or sub-moon of bodyB, returning -1");
				return -1;
			}
			if (bodyB.hasAncestor(bodyA))
			{
				Tools.PostDebugMessage(this, "bodyA is a moon or sub-moon of bodyB, returning 1");
				return 1;
			}

			Tools.PostDebugMessage(this, "bodies do not have an obvious relationship, searching for one");

			if (VOID_Tools.NearestRelatedParents(ref bodyA, ref bodyB))
			{
				Tools.PostDebugMessage(this, "good relation {0} and {1}, comparing", bodyA.bodyName, bodyB.bodyName);
				return this.Compare(bodyA, bodyB);
			}

			Tools.PostDebugMessage(this, "bad relation {0} and {1}, giving up", bodyA.bodyName, bodyB.bodyName);

			return 0;
		}
	}

	public static partial class VOID_Data
	{
		public static readonly VOID_DoubleValue nominalThrustWeight = new VOID_DoubleValue(
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
	}
}

