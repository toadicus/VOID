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
	public class VOID_TWR : VOID_WindowModule
	{
		public VOID_TWR() : base()
		{
			this._Name = "IP Thrust-to-Weight Ratios";
		}

		public override void ModuleWindow(int _)
		{
			if (
				HighLogic.LoadedSceneIsEditor ||
				(TimeWarp.WarpMode == TimeWarp.Modes.LOW) ||
				(TimeWarp.CurrentRate <= TimeWarp.MaxPhysicsRate)
			)
			{
				Engineer.VesselSimulator.SimManager.RequestSimulation();
			}

			GUILayout.BeginVertical();

			if (core.sortedBodyList == null)
			{
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

				GUILayout.Label("Unavailable");

				GUILayout.EndHorizontal();
			}
			else
			{
				foreach (CelestialBody body in core.sortedBodyList)
				{
					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

					GUILayout.Label(body.bodyName);
					GUILayout.FlexibleSpace();
					GUILayout.Label(
						(VOID_Data.nominalThrustWeight.Value / body.GeeASL).ToString("0.0##"),
						GUILayout.ExpandWidth(true)
					);

					GUILayout.EndHorizontal();
				}
			}

			GUILayout.EndVertical();

			GUI.DragWindow();
		}
	}

	public class VOID_EditorTWR : VOID_TWR, IVOID_EditorModule {}

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

