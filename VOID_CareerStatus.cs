// VOID
//
// VOID_CareerStatus.cs
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
using System.Text;
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	public class VOID_CareerStatus : VOID_WindowModule
	{
		public static VOID_CareerStatus Instance
		{
			get;
			private set;
		}

		public static string formatDelta(double delta)
		{
			if (delta > 0)
			{
				return string.Format(string.Intern("<color='green'>{0:#,#.##}↑</color>"), delta);
			}
			else if (delta < 0)
			{
				return string.Format(string.Intern("<color='red'>{0:#,#.##}↓</color>"), delta);
			}
			else
			{
				return string.Intern("0");
			}
		}

		public static string formatDelta(float delta)
		{
			return formatDelta((double)delta);
		}

		private int fontSize;
		private Texture2D origFundsGreen;
		private Texture2D origFundsRed;
		private Texture2D origRepGreen;
		private Texture2D origRepRed;
		private Texture2D origScience;

		public Texture2D fundsIconGreen
		{
			get;
			private set;
		}

		public Texture2D fundsIconRed
		{
			get;
			private set;
		}

		public Texture2D reputationIconGreen
		{
			get;
			private set;
		}

		public Texture2D reputationIconRed
		{
			get;
			private set;
		}

		public Texture2D scienceIcon
		{
			get;
			private set;
		}

		public double lastFundsChange
		{
			get;
			private set;
		}

		public float lastRepChange
		{
			get;
			private set;
		}

		public float lastScienceChange
		{
			get;
			private set;
		}

		public double currentFunds
		{
			get
			{
				if (Funding.Instance != null)
				{
					return Funding.Instance.Funds;
				}
				else
				{
					return double.NaN;
				}
			}
		}

		public float currentReputation
		{
			get
			{
				if (Reputation.Instance != null)
				{
					return Reputation.Instance.reputation;
				}
				else
				{
					return float.NaN;
				}
			}
		}

		public float currentScience
		{
			get
			{
				if (ResearchAndDevelopment.Instance != null)
				{
					return ResearchAndDevelopment.Instance.Science;
				}
				else
				{
					return float.NaN;
				}
			}
		}

		public override void ModuleWindow(int _)
		{

			/*if (this.fontSize != this.core.Skin.label.fontSize)
			{
				this.fontSize = this.core.Skin.label.fontSize;

				this.fundsIconGreen = this.origFundsGreen.Clone().ResizeByHeight(this.fontSize);
				this.fundsIconRed = this.origFundsRed.Clone().ResizeByHeight(this.fontSize);
				this.reputationIconGreen = this.origRepGreen.Clone().ResizeByHeight(this.fontSize);
				this.reputationIconRed = this.origRepRed.Clone().ResizeByHeight(this.fontSize);
				this.scienceIcon = this.origScience.Clone().ResizeByHeight(this.fontSize);

				GC.Collect();
			}*/

			GUILayout.BeginVertical();

			// VOID_Data.fundingStatus.DoGUIHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

			GUILayout.Label(VOID_Data.fundingStatus.Label);
			GUILayout.FlexibleSpace();
			// GUILayout.Label(this.fundsIconGreen);
			GUILayout.Label(VOID_Data.fundingStatus.Value, GUILayout.ExpandWidth(false));

			GUILayout.EndHorizontal();

			VOID_Data.reputationStatus.DoGUIHorizontal();

			VOID_Data.scienceStatus.DoGUIHorizontal();

			GUILayout.EndVertical();

			GUI.DragWindow();
		}

		private void onFundsChange(double delta)
		{
			this.lastFundsChange = delta;
		}

		private void onRepChange(float delta)
		{
			this.lastRepChange = delta;
		}

		private void onScienceChange(float delta)
		{
			this.lastScienceChange = delta;
		}

		/*
		 *  MissionRecoveryDialog::fundsIconGreen.name: UiElements_05
         *  MissionRecoveryDialog::fundsIconRed.name: UiElements_06
         *  MissionRecoveryDialog::reputationIconGreen.name: UiElements_07
         *  MissionRecoveryDialog::reputationIconRed.name: UiElements_08
         *  MissionRecoveryDialog::scienceIcon.name: UiElements_12
		 * */
		public VOID_CareerStatus() : base()
		{
			VOID_CareerStatus.Instance = this;

			this._Name = "Career Status";
			this.fontSize = int.MinValue;

			GameEvents.OnFundsChanged.Add(this.onFundsChange);
			GameEvents.OnReputationChanged.Add(this.onRepChange);
			GameEvents.OnScienceChanged.Add(this.onScienceChange);

			foreach (Texture2D tex in Resources.FindObjectsOfTypeAll<Texture2D>())
			{
				if (
					this.origFundsGreen != null &&
					this.origFundsRed != null &&
					this.origRepGreen != null &&
					this.origRepRed != null &&
					this.origScience != null
				)
				{
					break;
				}

				switch (tex.name)
				{
					case "UiElements_05":
						this.fundsIconGreen = this.origFundsGreen = tex;
						break;
					case "UiElements_06":
						this.fundsIconRed = this.origFundsRed = tex;
						break;
					case "UiElements_07":
						this.reputationIconGreen = this.origRepGreen = tex;
						break;
					case "UiElements_08":
						this.reputationIconRed = this.origRepRed = tex;
						break;
					case "UiElements_12":
						this.scienceIcon = this.origScience = tex;
						break;
					default:
						continue;
				}
			}
		}

		~VOID_CareerStatus()
		{
			GameEvents.OnFundsChanged.Remove(this.onFundsChange);
			GameEvents.OnReputationChanged.Remove(this.onRepChange);
			GameEvents.OnScienceChanged.Remove(this.onScienceChange);

			VOID_CareerStatus.Instance = null;
		}
	}

	public static partial class VOID_Data
	{
		public static readonly VOID_StrValue fundingStatus = new VOID_StrValue(
			string.Intern("Funds"),
			delegate()
		{
			if (VOID_CareerStatus.Instance == null)
			{
				return string.Empty;
			}

			return string.Format("√{0} ({1})",
				VOID_CareerStatus.Instance.currentFunds,
				VOID_CareerStatus.formatDelta(VOID_CareerStatus.Instance.lastFundsChange)
			);
		}
		);

		public static readonly VOID_StrValue reputationStatus = new VOID_StrValue(
			string.Intern("Reputation"),
			delegate()
		{
			if (VOID_CareerStatus.Instance == null)
			{
				return string.Empty;
			}

			return string.Format("{0} ({1})",
				VOID_CareerStatus.Instance.currentReputation,
				VOID_CareerStatus.formatDelta(VOID_CareerStatus.Instance.lastRepChange)
			);
		}
		);

		public static readonly VOID_StrValue scienceStatus = new VOID_StrValue(
			string.Intern("Science"),
			delegate()
		{
			if (VOID_CareerStatus.Instance == null)
			{
				return string.Empty;
			}

			return string.Format("{0} ({1})",
				VOID_CareerStatus.Instance.currentScience,
				VOID_CareerStatus.formatDelta(VOID_CareerStatus.Instance.lastScienceChange)
			);
		}
		);
	}
}

