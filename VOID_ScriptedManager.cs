// VOID
//
// VOID_ScriptedManager.cs
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

using System;
using System.Collections.Generic;
using ToadicusTools;
using UnityEngine;
using VOID_ScriptedPanels;

namespace VOID
{
	public class VOID_ScriptedManager : VOID_SingletonWindow<VOID_ScriptedManager>
	{
		private List<VOID_ScriptedPanel> validPanels;

		public IList<VOID_ScriptedPanel> ValidPanels
		{
			get
			{
				return this.validPanels.AsReadOnly();
			}
		}

		public override bool InValidGame
		{
			get
			{
				return true;
			}
		}

		public override bool InValidScene
		{
			get
			{
				return true;
			}
		}

		public override void DrawGUI()
		{
			base.DrawGUI();

			/*foreach (var panel in this.ValidPanels)
			{
				if (panel.Active)
				{
					panel.DrawGUI();
				}
			}*/
		}

		public override void ModuleWindow(int id)
		{
			if (this.ValidPanels.Count == 0)
			{
				GUILayout.BeginHorizontal();

				GUILayout.Label("No valid scripted panels for this scene and game type.");

				GUILayout.EndHorizontal();

				base.ModuleWindow(id);

				return;
			}

			foreach (var panel in this.ValidPanels)
			{
				panel.Active = GUITools.Toggle(panel.Active, panel.Name);
			}

			if (GUILayout.Button("Reload Panel Configs"))
			{
				VOID_ScriptedPanel.LoadScriptedPanels();

				this.core.onUpdate += onUpdateHandler;
			}

			base.ModuleWindow(id);
		}

		public override void Save(KSP.IO.PluginConfiguration config, string sceneKey)
		{
			base.Save(config, sceneKey);

			foreach (var panel in this.validPanels)
			{
				panel.Save(config, sceneKey);
			}
		}

		private void onUpdateHandler (object sender)
		{
			if (this.timeToUpdate)
			{
				this.validPanels.Clear();

				foreach (var panel in VOID_ScriptedPanel.Panels)
				{
					if (panel.InValidGame && panel.InValidScene)
					{
						this.validPanels.Add(panel);
					}
				}

				this.core.onUpdate -= onUpdateHandler;
			}
		}

		private void refreshValidPanels(GameScenes data)
		{
			this.core.onUpdate += onUpdateHandler;
		}

		private void refreshValidPanels(ConfigNode data)
		{
			this.core.onUpdate += onUpdateHandler;
		}

		public VOID_ScriptedManager() : base()
		{
			this.Name = "Scripted Panel Manager";

			this.validPanels = new List<VOID_ScriptedPanel>();

			VOID_ScriptedPanel.LoadScriptedPanels();

			GameEvents.onGameStateLoad.Add(this.refreshValidPanels);
			GameEvents.onGameSceneLoadRequested.Add(this.refreshValidPanels);

			this.core.onUpdate += onUpdateHandler;
		}

		public override void Dispose()
		{
			GameEvents.onGameStateLoad.Remove(this.refreshValidPanels);
			GameEvents.onGameSceneLoadRequested.Remove(this.refreshValidPanels);

			base.Dispose();
		}

		~VOID_ScriptedManager()
		{
			this.Dispose();
		}
	}
}

