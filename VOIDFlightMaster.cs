///////////////////////////////////////////////////////////////////////////////
//
//    VOID - Vessel Orbital Information Display for Kerbal Space Program
//    Copyright (C) 2012 Iannic-ann-od
//    Copyright (C) 2013 Toadicus
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
///////////////////////////////////////////////////////////////////////////////
//
//  Much, much credit to Younata, Adammada, Nivvydaskrl and to all the authors
//  behind MechJeb, RemoteTech Relay Network, ISA MapSat, and Protractor for some
//  invaluable functions and making your nicely written code available to learn from.
//
///////////////////////////////////////////////////////////////////////////////
//
//  This software uses VesselSimulator and Engineer.Extensions from Engineer Redux.
//  Engineer Redux (c) 2013 cybutek
//  Used by permission.
//
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Engineer.VesselSimulator;

namespace VOID
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class VOIDFlightMaster : MonoBehaviour
	{
		protected VOID_Core Core;

		public void Awake()
		{
			Tools.PostDebugMessage ("VOIDFlightMaster: Waking up.");
			this.Core = (VOID_Core)VOID_Core.Instance;
			this.Core.StopGUI ();
			this.Core.StartGUI ();
			Tools.PostDebugMessage ("VOIDFlightMaster: Awake.");
		}

		public void Update()
		{
			if (!HighLogic.LoadedSceneIsFlight && this.Core != null)
			{
				this.Core = null;
				VOID_Core.Reset();
				return;
			}

			if (this.Core == null)
			{
				this.Awake();
			}

			this.Core.Update ();

			if (this.Core.vessel != null)
			{
				SimManager.Instance.Gravity = VOID_Core.Instance.vessel.mainBody.gravParameter / Math.Pow(VOID_Core.Instance.vessel.mainBody.Radius, 2);
				SimManager.Instance.TryStartSimulation();
			}

			if (this.Core.factoryReset)
			{
				KSP.IO.File.Delete<VOID_Core>("config.xml");
				this.Core = null;
				VOID_Core.Reset();
			}
		}

		public void FixedUpdate()
		{
			if (this.Core == null || !HighLogic.LoadedSceneIsFlight)
			{
				return;
			}

			this.Core.FixedUpdate ();
		}

		public void OnGUI()
		{
			if (this.Core == null)
			{
				return;
			}

			this.Core.OnGUI();
		}
    }

	[KSPAddon(KSPAddon.Startup.EditorAny, false)]
	public class VOIDEditorMaster : MonoBehaviour
	{
		protected VOID_EditorCore Core;

		public void Awake()
		{
			Tools.PostDebugMessage ("VOIDEditorMaster: Waking up.");
			this.Core = VOID_EditorCore.Instance;
			this.Core.StopGUI ();
			this.Core.StartGUI ();
			Tools.PostDebugMessage ("VOIDEditorMaster: Awake.");
		}

		public void Update()
		{
			if (!HighLogic.LoadedSceneIsEditor && this.Core != null)
			{
				this.Core = null;
				VOID_EditorCore.Reset();
				return;
			}

			if (this.Core == null)
			{
				this.Awake();
			}

			this.Core.Update ();

			if (this.Core.factoryReset)
			{
				KSP.IO.File.Delete<VOID_EditorCore>("config.xml");
				this.Core = null;
				VOID_EditorCore.Reset();
			}
		}

		public void FixedUpdate()
		{
			if (this.Core == null || !HighLogic.LoadedSceneIsEditor)
			{
				return;
			}

			this.Core.FixedUpdate ();
		}

		public void OnGUI()
		{
			if (this.Core == null)
			{
				return;
			}

			this.Core.OnGUI();
		}
	}
}
