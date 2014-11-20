// VOID
//
// VOIDEditorMaster.cs
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

using Engineer.VesselSimulator;
using KSP;
using System;
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	[KSPAddon(KSPAddon.Startup.EditorAny, false)]
	public class VOIDEditorMaster : MonoBehaviour
	{
		protected VOID_EditorCore Core;

		public void Awake()
		{
			Tools.PostDebugMessage ("VOIDEditorMaster: Waking up.");
			this.Core = VOID_EditorCore.Instance;
			this.Core.ResetGUI ();
			Tools.PostDebugMessage ("VOIDEditorMaster: Awake.");
		}

		public void Update()
		{
			if (!HighLogic.LoadedSceneIsEditor && this.Core != null)
			{
				this.Core.SaveConfig ();
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

		public void OnDestroy()
		{
			if (this.Core == null)
			{
				return;
			}

			this.Core.OnDestroy();
		}

		public void OnApplicationQuit()
		{
			if (this.Core == null)
			{
				return;
			}

			this.Core.OnApplicationQuit();
		}
	}
}
