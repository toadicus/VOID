// VOID
//
// cs
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
// This file contains concepts and code dervied from the KSPPluginFramework, © 2014 TriggerAu.
// Used under the terms of The MIT License (MIT).

using System;
using System.Collections.Generic;
using UnityEngine;

namespace VOID
{
	public static class VOID_Styles
	{
		public static bool Ready
		{
			get;
			private set;
		}

		public static GUIStyle labelDefault
		{
			get;
			private set;
		}

		public static GUIStyle labelLink
		{
			get;
			private set;
		}

		public static GUIStyle labelCenter
		{
			get;
			private set;
		}

		public static GUIStyle labelCenterBold
		{
			get;
			private set;
		}

		public static GUIStyle labelGreen
		{
			get;
			private set;
		}

		public static GUIStyle labelHud
		{
			get;
			private set;
		}

		public static GUIStyle labelRight
		{
			get;
			private set;
		}

		public static GUIStyle labelRed
		{
			get;
			private set;
		}

		public static GUIStyle currentTooltip
		{
			get;
			private set;
		}

		public static GUISkin defUnitySkin
		{
			get;
			private set;
		}

		public static GUISkin defKSPSkin
		{
			get;
			private set;
		}

		public static void OnSkinChanged()
		{
			if (defUnitySkin == null && GUI.skin != null)
			{
				defUnitySkin = GUI.skin;
			}

			if (defKSPSkin == null && HighLogic.Skin != null)
			{
				defKSPSkin = HighLogic.Skin;
			}

			labelDefault = new GUIStyle(GUI.skin.label);

			labelLink = new GUIStyle(GUI.skin.label);
			labelLink.fontStyle = FontStyle.Italic;
			labelLink.fontSize = (int)((float)labelLink.fontSize * .8f);

			labelCenter = new GUIStyle(GUI.skin.label);
			labelCenter.normal.textColor = Color.white;
			labelCenter.alignment = TextAnchor.UpperCenter;

			labelCenterBold = new GUIStyle(GUI.skin.label);
			labelCenterBold.normal.textColor = Color.white;
			labelCenterBold.alignment = TextAnchor.UpperCenter;
			labelCenterBold.fontStyle = FontStyle.Bold;

			labelHud = new GUIStyle(labelDefault);

			labelRight = new GUIStyle(GUI.skin.label);
			labelRight.normal.textColor = Color.white;
			labelRight.alignment = TextAnchor.UpperRight;

			labelRed = new GUIStyle(GUI.skin.label);
			labelRed.normal.textColor = Color.red;

			labelGreen = new GUIStyle(GUI.skin.label);
			labelGreen.normal.textColor = Color.green;

			if (VOID_Data.CoreInitialized)
			{
				SetCurrentTooltip();
			}

			Ready = true;
		}

		static VOID_Styles()
		{
			tooltipCache = new Dictionary<GUISkin, GUIStyle>();

			OnSkinChanged();

			Ready = false;
		}

		private static Dictionary<GUISkin, GUIStyle> tooltipCache;

		/// <summary>
		/// Private routine that sets the tooltip based on the Current skin
		/// </summary>
		private static void SetCurrentTooltip()
		{
			//Use the custom skin if it exists
			if (!tooltipCache.ContainsKey(VOID_Data.Core.Skin))
			{
				//otherwise lets build a style for the defaults or take the label style otherwise
				if (VOID_Data.Core.Skin == defUnitySkin)
					tooltipCache[VOID_Data.Core.Skin] = new GUIStyle(defUnitySkin.box);
				/*else if (VOID_Data.Core.Skin == defKSPSkin)
					tooltipCache[VOID_Data.Core.Skin] = GenDefKSPTooltip();*/
				else
				{
					tooltipCache[VOID_Data.Core.Skin] = genGenericTooltip();
				}
			}

			currentTooltip = tooltipCache[VOID_Data.Core.Skin];
		}

		/// <summary>
		/// private function that creates a tooltip style for KSP
		/// </summary>
		/// <returns>New Default Tooltip style for KSP</returns>
		private static GUIStyle GenDefKSPTooltip()
		{
			//build a new style to return
			GUIStyle retStyle = new GUIStyle(defKSPSkin.label);
			//build the background
			Texture2D texBack = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			texBack.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f, 0.95f));
			texBack.Apply();
			retStyle.normal.background = texBack;
			//set some text defaults
			retStyle.normal.textColor = new Color32(224, 224, 224, 255);
			retStyle.padding = new RectOffset(3, 3, 3, 3);
			retStyle.alignment = TextAnchor.MiddleCenter;
			return retStyle;
		}

		private static GUIStyle genGenericTooltip()
		{
			GUIStyle genericStyle = new GUIStyle(VOID_Data.Core.Skin.textField);
			genericStyle.fontStyle = FontStyle.Normal;
			genericStyle.normal.textColor = (Color)XKCDColors.OffWhite;

			genericStyle.padding = new RectOffset(3, 3, 3, 3);

			return genericStyle;
		}
	}
}

