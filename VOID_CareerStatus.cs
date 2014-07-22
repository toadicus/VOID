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
				return string.Format("<color='green'>{0:#,#.##}↑</color>", delta);
			}
			else if (delta < 0)
			{
				return string.Format("<color='red'>{0:#,#.##}↓</color>", delta);
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

		private GUIContent fundsContent;
		private GUIContent repContent;
		private GUIContent scienceContent;

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
			get;
			private set;
		}

		public float currentReputation
		{
			get;
			private set;
		}

		public float currentScience
		{
			get;
			private set;
		}

		public override void ModuleWindow(int _)
		{
			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(VOID_Data.fundingStatus.Label);
			GUILayout.FlexibleSpace();
			this.fundsContent.text = VOID_Data.fundingStatus.Value;
			GUILayout.Label(this.fundsContent, GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(VOID_Data.reputationStatus.Label);
			GUILayout.FlexibleSpace();
			this.repContent.text = VOID_Data.reputationStatus.Value;
			GUILayout.Label(this.repContent, GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(VOID_Data.scienceStatus.Label);
			GUILayout.FlexibleSpace();
			this.scienceContent.text = VOID_Data.scienceStatus.Value;
			GUILayout.Label(this.scienceContent, GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();

			GUI.DragWindow();
		}

		private void onFundsChange(double newValue)
		{
			this.lastFundsChange = newValue - this.currentFunds;
			this.currentFunds = newValue;
		}

		private void onRepChange(float newValue)
		{
			this.lastRepChange = newValue - this.currentReputation;
			this.currentReputation = newValue;
		}

		private void onScienceChange(float newValue)
		{
			this.lastScienceChange = newValue - this.currentScience;
			this.currentScience = newValue;
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

			GameEvents.OnFundsChanged.Add(this.onFundsChange);
			GameEvents.OnReputationChanged.Add(this.onRepChange);
			GameEvents.OnScienceChanged.Add(this.onScienceChange);

			string texturePath = string.Format(
				"{0}/../Textures/",
				System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
			);

			this.fundsIconGreen = new Texture2D(10, 18);
			this.fundsIconGreen.LoadImage(System.IO.File.ReadAllBytes(texturePath + "fundsgreen.png"));

			this.fundsIconRed = new Texture2D(10, 18);
			this.fundsIconRed.LoadImage(System.IO.File.ReadAllBytes(texturePath + "fundsred.png"));

			this.reputationIconGreen = new Texture2D(16, 18);
			this.reputationIconGreen.LoadImage(System.IO.File.ReadAllBytes(texturePath + "repgreen.png"));

			this.reputationIconRed = new Texture2D(16, 18);
			this.reputationIconRed.LoadImage(System.IO.File.ReadAllBytes(texturePath + "repred.png"));

			this.scienceIcon = new Texture2D(16, 18);
			this.scienceIcon.LoadImage(System.IO.File.ReadAllBytes(texturePath + "science.png"));

			this.fundsContent = new GUIContent(this.fundsIconGreen);
			this.repContent = new GUIContent(this.reputationIconGreen);
			this.scienceContent = new GUIContent(this.scienceIcon);

			this.currentFunds = Funding.Instance.Funds;
			this.currentReputation = Reputation.Instance.reputation;
			this.currentScience = ResearchAndDevelopment.Instance.Science;
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

			return string.Format("{0} ({1})",
				VOID_CareerStatus.Instance.currentFunds.ToString("#,#.##"),
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
				VOID_CareerStatus.Instance.currentReputation.ToString("#,#.##"),
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
				VOID_CareerStatus.Instance.currentScience.ToString("#,#.##"),
				VOID_CareerStatus.formatDelta(VOID_CareerStatus.Instance.lastScienceChange)
			);
		}
		);
	}
}

