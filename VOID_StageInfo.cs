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
		private Table.Column<double> stageMassCol;

		private bool stylesApplied;

		public VOID_StageInfo() : base()
		{
			this._Name = "Stage Information";

			this.stylesApplied = false;

			this.stageTable = new Table();

			this.stageNumberCol = new Table.Column<int>(40f);
			this.stageTable.Add(this.stageNumberCol);

			this.stageDeltaVCol = new Table.Column<double>(80f);
			this.stageDeltaVCol.Format = "S2";
			this.stageTable.Add(this.stageDeltaVCol);

			this.stageMassCol = new Table.Column<double>(80f);
			this.stageMassCol.Format = "G2";
			this.stageTable.Add(this.stageMassCol);
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
				this.stageNumberCol.Style = core.LabelStyles["center"];
				this.stageDeltaVCol.Style = core.LabelStyles["center"];
				this.stageMassCol.Style = core.LabelStyles["center"];
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
				this.stageNumberCol.Add(stage.number);

				this.stageDeltaVCol.Add(stage.deltaV);

				this.stageMassCol.Add(stage.mass);
			}

			this.stageTable.Render();

			GUI.DragWindow();
		}
	}

	public class VOID_StageInfoEditor : VOID_StageInfo, IVOID_EditorModule {}
}

