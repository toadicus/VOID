// VOID
//
// VOID_EditorHUD.cs
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

using KerbalEngineer.VesselSimulator;
using KSP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	public class VOID_EditorHUD : VOID_HUDModule, IVOID_EditorModule
	{
		/*
		 * Fields
		 * */
		[AVOID_SaveValue("ehudWindowPos")]
		protected VOID_SaveValue<Rect> ehudWindowPos;

		protected HUDWindow ehudWindow;
		protected EditorVesselOverlays _vesselOverlays;

		[AVOID_SaveValue("snapToLeft")]
		protected VOID_SaveValue<bool> snapToLeft;

		/*
		 * Properties
		 * */
		protected EditorVesselOverlays vesselOverlays
		{
			get
			{
				if (this._vesselOverlays == null)
				{
					this._vesselOverlays = (EditorVesselOverlays)Resources
						.FindObjectsOfTypeAll(typeof(EditorVesselOverlays))
						.FirstOrDefault();
				}

				return this._vesselOverlays;
			}
		}

		protected EditorMarker_CoM CoMmarker
		{
			get
			{
				if (this.vesselOverlays == null)
				{
					return null;
				}

				return this.vesselOverlays.CoMmarker;
			}
		}

		protected EditorMarker_CoT CoTmarker
		{
			get
			{
				if (this.vesselOverlays == null)
				{
					return null;
				}

				return this.vesselOverlays.CoTmarker;
			}
		}

		/* 
		 * Methods
		 * */
		public VOID_EditorHUD() : base()
		{
			this._Name = "Heads-Up Display";

			this.toggleActive = true;

			this.snapToLeft.value = true;

			this.ehudWindow = new HUDWindow(
				this.ehudWindowFunc,
				new Rect(EditorPanels.Instance.partsPanelWidth + 10f, 125f, 300f, 64f)
			);
			this.Windows.Add(this.ehudWindow);
			this.ehudWindowPos = this.ehudWindow.WindowPos;

			Tools.PostDebugMessage (this.GetType().Name + ": Constructed.");
		}

		public void ehudWindowFunc(int id)
		{
			StringBuilder hudString = new StringBuilder();

			if (this.core.LastStage == null)
			{
				return;
			}

			// GUI.skin = AssetBase.GetGUISkin("KSP window 2");

			hudString.Append("Total Mass: ");
			hudString.Append(this.core.LastStage.totalMass.ToString("F3"));
			hudString.Append('t');

			hudString.Append(' ');

			hudString.Append("Part Count: ");
			hudString.Append(EditorLogic.SortedShipList.Count);

			hudString.Append('\n');

			hudString.Append("Total Delta-V: ");
			hudString.Append(Tools.MuMech_ToSI(this.core.LastStage.totalDeltaV));
			hudString.Append("m/s");

			hudString.Append('\n');

			hudString.Append("Bottom Stage Delta-V");
			hudString.Append(Tools.MuMech_ToSI(this.core.LastStage.deltaV));
			hudString.Append("m/s");

			hudString.Append('\n');

			hudString.Append("Bottom Stage T/W Ratio: ");
			hudString.Append(this.core.LastStage.thrustToWeight.ToString("F3"));

			if (this.CoMmarker.gameObject.activeInHierarchy && this.CoTmarker.gameObject.activeInHierarchy)
			{
				hudString.Append('\n');

				hudString.Append("Thrust Offset: ");
				hudString.Append(
					Vector3.Cross(
						this.CoTmarker.dirMarkerObject.transform.forward,
						this.CoMmarker.posMarkerObject.transform.position - this.CoTmarker.posMarkerObject.transform.position
					).ToString("F3"));
			}

			GUILayout.Label(hudString.ToString(), VOID_Styles.labelHud, GUILayout.ExpandWidth(true));

			if (!this.positionsLocked)
			{
				GUI.DragWindow();
			}

			GUI.BringWindowToBack(id);
		}

		public override void DrawGUI()
		{
			float hudLeft;

			if (EditorLogic.fetch.editorScreen == EditorScreen.Parts)
			{
				hudLeft = EditorPanels.Instance.partsPanelWidth + 10f;
				hudLeft += EditorPartList.Instance.transformTopLeft.position.x -
					EditorPartList.Instance.transformTopLeft.parent.parent.position.x -
					72f;
			}
			else if (EditorLogic.fetch.editorScreen == EditorScreen.Actions)
			{
				hudLeft = EditorPanels.Instance.actionsPanelWidth + 10f;
			}
			else
			{
				return;
			}

			Tools.PostDebugMessage(this,
				"EditorPartList topLeft.parent.parent.position: {0}\n" +
				"EditorPartList topLeft.parent.position: {1}\n" +
				"EditorPartList topLeft.position: {2}\n" +
				"snapToEdge: {3} (pos.Xmin: {4}; hudLeft: {5})",
				EditorPartList.Instance.transformTopLeft.parent.parent.position,
				EditorPartList.Instance.transformTopLeft.parent.position,
				EditorPartList.Instance.transformTopLeft.position,
				this.snapToLeft, this.ehudWindowPos.value.xMin, hudLeft
			);

			base.DrawGUI();

			Rect hudPos = this.ehudWindow.WindowPos;

			if (this.snapToLeft && this.positionsLocked)
			{
				hudPos.xMin = hudLeft;
			}
			else
			{
				hudPos.xMin = Mathf.Max(hudLeft, hudPos.xMin);
			}

			hudPos.width = this.ehudWindow.defaultWindowPos.width;

			this.ehudWindowPos.value = hudPos;

			this.ehudWindow.WindowPos = this.ehudWindowPos;

			this.snapToLeft = Mathf.Abs(this.ehudWindowPos.value.xMin - hudLeft) < 15f;
		}
	}
}
