// VOID
//
// VOID_Orbital.cs
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
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	public class VOID_Orbital : VOID_WindowModule
	{
		[AVOID_SaveValue("toggleExtended")]
		protected VOID_SaveValue<bool> toggleExtended = false;

		[AVOID_SaveValue("precisionValues")]
		protected long _precisionValues = 230584300921369395;
		protected IntCollection precisionValues;

		public VOID_Orbital()
		{
			this.Name = "Orbital Information";

			this.WindowPos.x = Screen.width - 520f;
			this.WindowPos.y = 250f;
		}

		public override void ModuleWindow(int _)
		{
			int idx = 0;

            GUILayout.BeginVertical();

			VOID_Data.primaryName.DoGUIHorizontal ();

			this.precisionValues [idx]= (ushort)VOID_Data.orbitAltitude.DoGUIHorizontal (this.precisionValues [idx]);
			idx++;

			this.precisionValues [idx]= (ushort)VOID_Data.orbitVelocity.DoGUIHorizontal (this.precisionValues [idx]);
			idx++;

			this.precisionValues [idx]= (ushort)VOID_Data.orbitApoAlt.DoGUIHorizontal (this.precisionValues [idx]);
			idx++;

			VOID_Data.timeToApo.DoGUIHorizontal();

			this.precisionValues [idx]= (ushort)VOID_Data.oribtPeriAlt.DoGUIHorizontal (this.precisionValues [idx]);
			idx++;

			VOID_Data.timeToPeri.DoGUIHorizontal();

			VOID_Data.orbitInclination.DoGUIHorizontal("F3");

			this.precisionValues [idx]= (ushort)VOID_Data.gravityAccel.DoGUIHorizontal (this.precisionValues [idx]);
			idx++;

			this.toggleExtended.value = GUITools.Toggle(this.toggleExtended, "Extended info");

			if (this.toggleExtended)
            {
				VOID_Data.orbitPeriod.DoGUIHorizontal();

				this.precisionValues [idx]= (ushort)VOID_Data.semiMajorAxis.DoGUIHorizontal (this.precisionValues [idx]);
				idx++;

				VOID_Data.eccentricity.DoGUIHorizontal("F4");

				VOID_Data.meanAnomaly.DoGUIHorizontal("F3");

				VOID_Data.trueAnomaly.DoGUIHorizontal("F3");

				VOID_Data.eccAnomaly.DoGUIHorizontal("F3");

				VOID_Data.longitudeAscNode.DoGUIHorizontal("F3");

				VOID_Data.argumentPeriapsis.DoGUIHorizontal("F3");

				VOID_Data.localSiderealLongitude.DoGUIHorizontal("F3");
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
		}

		public override void LoadConfig ()
		{
			base.LoadConfig ();

			this.precisionValues = new IntCollection (4, this._precisionValues);
		}

		public override void _SaveToConfig (KSP.IO.PluginConfiguration config)
		{
			this._precisionValues = this.precisionValues.collection;

			base._SaveToConfig (config);
		}
	}
}

