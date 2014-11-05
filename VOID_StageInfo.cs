// VOID © 2014 toadicus
//
// This work is licensed under the Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License. To view a
// copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/

using Engineer.VesselSimulator;
using KSP;
using System;
using System.Collections.Generic;
using System.Linq;
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	public class VOID_StageInfo : VOID_WindowModule
	{
		private Table stageTable;

		private Table.Column<int> stageNumberCol;
		private Table.Column<double> stageDeltaVCol;
		private Table.Column<double> stageTotalDVCol;
		private Table.Column<double> stageMassCol;
		private Table.Column<double> stageTotalMassCol;
		private Table.Column<double> stageThrustCol;
		private Table.Column<double> stageTWRCol;

		private bool stylesApplied;
		private bool showBodyList;

		private Rect bodyListPos;

		private CelestialBody selectedBody;
		[AVOID_SaveValue("bodyIdx")]
		private VOID_SaveValue<int> bodyIdx;
		private int lastIdx;

		public VOID_StageInfo() : base()
		{
			this._Name = "Stage Information";
			this.defWidth = 200f;
			this.bodyIdx = 4;

			this.stylesApplied = false;
			this.showBodyList = false;

			this.bodyListPos = new Rect();

			this.stageTable = new Table();

			this.stageNumberCol = new Table.Column<int>("Stage", 40f);
			this.stageTable.Add(this.stageNumberCol);

			this.stageDeltaVCol = new Table.Column<double>("DeltaV [m/s]", 80f);
			this.stageDeltaVCol.Format = "S2";
			this.stageTable.Add(this.stageDeltaVCol);

			this.stageTotalDVCol = new Table.Column<double>("Total ΔV [m/s]", 80f);
			this.stageTotalDVCol.Format = "S2";
			this.stageTable.Add(this.stageTotalDVCol);

			this.stageMassCol = new Table.Column<double>("Mass [Mg]", 80f);
			this.stageMassCol.Format = "#.#";
			this.stageTable.Add(this.stageMassCol);

			this.stageTotalMassCol = new Table.Column<double>("Total Mass [Mg]", 80f);
			this.stageTotalMassCol.Format = "#.#";
			this.stageTable.Add(this.stageTotalMassCol);

			this.stageThrustCol = new Table.Column<double>("Thrust [N]", 80f);
			this.stageThrustCol.Format = "S2";
			this.stageTable.Add(this.stageThrustCol);

			this.stageTWRCol = new Table.Column<double>("T/W Ratio", 80f);
			this.stageTWRCol.Format = "#.#";
			this.stageTable.Add(this.stageTWRCol);
		}

		public override void DrawGUI()
		{
			base.DrawGUI();

			if (this.showBodyList)
			{
				GUILayout.Window(core.windowID, this.bodyListPos, this.BodyPickerWindow, string.Empty);
			}
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

			if (!this.stylesApplied)
			{
				this.stageTable.ApplyCellStyle(VOID_Styles.labelCenter);
				this.stageTable.ApplyHeaderStyle(VOID_Styles.labelCenterBold);
			}

			this.stageTable.ClearColumns();

			if (core.Stages == null || core.Stages.Length == 0)
			{
				GUILayout.BeginVertical();

				GUILayout.Label("No stage data!");

				GUILayout.EndVertical();

				return;
			}

			foreach (Stage stage in core.Stages)
			{
				if (stage.deltaV == 0 && stage.mass == 0)
				{
					continue;
				}

				this.stageNumberCol.Add(stage.number);

				this.stageDeltaVCol.Add(stage.deltaV);
				this.stageTotalDVCol.Add(stage.totalDeltaV);

				this.stageMassCol.Add(stage.mass);
				this.stageTotalMassCol.Add(stage.totalMass);      

				this.stageThrustCol.Add(stage.thrust * 1000f);
				this.stageTWRCol.Add(stage.thrustToWeight / (this.selectedBody ?? core.Kerbin).GeeASL);
			}

			this.stageTable.Render();

			if (core.sortedBodyList != null)
			{
				GUILayout.BeginHorizontal();

				if (GUILayout.Button("◄"))
				{
					this.bodyIdx--;
				}

				this.showBodyList = GUILayout.Toggle(this.showBodyList, (this.selectedBody ?? core.Kerbin).bodyName, GUI.skin.button);
				Rect bodyButtonPos = GUILayoutUtility.GetLastRect();

				if (Event.current.type == EventType.Repaint)
				{
					this.bodyListPos.width = bodyButtonPos.width;
					this.bodyListPos.x = bodyButtonPos.xMin + this.WindowPos.xMin;
					this.bodyListPos.y = bodyButtonPos.yMax + this.WindowPos.yMin;
				}

				if (GUILayout.Button("►"))
				{
					this.bodyIdx++;
				}

				this.bodyIdx %= core.sortedBodyList.Count;

				if (this.bodyIdx < 0)
				{
					this.bodyIdx += core.sortedBodyList.Count;
				}

				if (this.lastIdx != this.bodyIdx)
				{
					this.lastIdx = this.bodyIdx;
					this.selectedBody = core.sortedBodyList[this.bodyIdx];
				}

				GUILayout.EndHorizontal();
			}

			GUI.DragWindow();
		}

		private void BodyPickerWindow(int _)
		{
			foreach (CelestialBody body in core.sortedBodyList)
			{
				if (GUILayout.Button(body.bodyName, VOID_Styles.labelDefault))
				{
					Debug.Log("Picked new body focus: " + body.bodyName);
					this.bodyIdx = core.sortedBodyList.IndexOf(body);
					this.showBodyList = false;
				}
			}
		}
	}

	public class VOID_StageInfoEditor : VOID_StageInfo, IVOID_EditorModule {}
}

