//
//  VOID_Hud.cs
//
//  Author:
//       toadicus <>
//
//  Copyright (c) 2013 toadicus
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using KSP;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace VOID
{
	public class VOID_EditorHUD : VOID_Module, IVOID_EditorModule
	{
		/*
		 * Fields
		 * */
		[AVOID_SaveValue("colorIndex")]
		protected VOID_SaveValue<int> _colorIndex = 0;

		protected List<Color> textColors = new List<Color>();

		protected GUIStyle labelStyle;

		/*
		 * Properties
		 * */
		public int ColorIndex
		{
			get
			{
				return this._colorIndex;
			}
			set
			{
				if (this._colorIndex >= this.textColors.Count - 1)
				{
					this._colorIndex = 0;
					return;
				}

				this._colorIndex = value;
			}
		}

		/* 
		 * Methods
		 * */
		public VOID_EditorHUD() : base()
		{
			this._Name = "Heads-Up Display";

			this._Active = true;

			this.textColors.Add(Color.green);
			this.textColors.Add(Color.black);
			this.textColors.Add(Color.white);
			this.textColors.Add(Color.red);
			this.textColors.Add(Color.blue);
			this.textColors.Add(Color.yellow);
			this.textColors.Add(Color.gray);
			this.textColors.Add(Color.cyan);
			this.textColors.Add(Color.magenta);

			this.labelStyle = new GUIStyle ();
			this.labelStyle.normal.textColor = this.textColors [this.ColorIndex];

			Tools.PostDebugMessage (this.GetType().Name + ": Constructed.");
		}

		public override void DrawGUI()
		{
			GUI.skin = AssetBase.GetGUISkin("KSP window 2");

			labelStyle.normal.textColor = textColors [ColorIndex];

			GUI.Label (
				new Rect (Screen.width - 310, 80, 300f, 90f),
				"Sample HUD Line 1\nSample HUD Line 2\nSample HUD Line 3\nSample HUD Line 4\n",
				labelStyle);
		}

		public override void DrawConfigurables()
		{
			if (GUILayout.Button ("Change HUD color", GUILayout.ExpandWidth (false)))
			{
				++this.ColorIndex;
			}
		}
	}
}
