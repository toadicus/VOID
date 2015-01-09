// VOID
//
// IVOID_Core.cs
//
// Copyright © 2015, toadicus
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
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	public abstract class VOIDCore : VOID_Module, IVOID_Module
	{
		public const double Constant_G = 6.674e-11;

		public abstract int windowID { get; }
		public abstract bool configDirty { get; set; }
		public abstract bool powerAvailable	{ get; protected set; }

		public abstract List<IVOID_Module> Modules { get; }

		public abstract float updateTimer { get; protected set; }
		public abstract double updatePeriod { get; }

		public virtual float saveTimer { get; protected set; }

		public abstract GUISkin Skin { get; }

		public abstract CelestialBody HomeBody { get; }
		public abstract List<CelestialBody> allBodies { get; }
		public abstract List<CelestialBody> sortedBodyList { get; protected set; }

		public abstract List<VesselType> allVesselTypes { get; }

		public abstract Stage LastStage { get; protected set; }
		public abstract Stage[] Stages { get; protected set; }

		public virtual event VOIDEventHandler onApplicationQuit;

		public virtual void OnGUI() {}

		public virtual void OnApplicationQuit()
		{
			if (this.onApplicationQuit != null)
			{
				this.onApplicationQuit(this);
			}
		}

		public abstract void SaveConfig();
	}

	public delegate void VOIDEventHandler(object sender);
}

