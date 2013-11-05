//
//  VOID_Core.cs
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
using System;
using KSP;
using UnityEngine;

namespace VOID
{
	public class VOID_Core : IVOID_Module
	{
		/*
		 * Static Members
		 * */
		protected static VOID_Core _instance;

		public static IVOID_Module Instance {
			get {
				if (_instance == null) {
					Tools.PostDebugMessage ("Instantiating VOID_HUD");
					_instance = new VOID_Core ();
				}
				Tools.PostDebugMessage ("Returning VOID_HUD instance.");
				return _instance;
			}
		}

		/*
		 * Fields
		 * */
		protected Rect main_window_pos = new Rect(Screen.width / 2, Screen.height / 2, 10f, 10f); //VOID main

		protected Rect main_icon_pos = new Rect(Screen.width / 2 - 80, Screen.height - 32, 30f, 30f);
		protected Texture2D void_icon_off = new Texture2D(30, 30, TextureFormat.ARGB32, false);
		protected Texture2D void_icon_on = new Texture2D(30, 30, TextureFormat.ARGB32, false);
		protected Texture2D void_icon = new Texture2D(30, 30, TextureFormat.ARGB32, false);

		protected bool _Active = true;
		protected bool _Running = false;
		/*
		 * Properties
		 * */
		public bool hasGUIConfig
		{
			get
			{
				return false;
			}
		}

		public bool toggleActive
		{
			get
			{
				return this._Active;
			}
			set
			{
				this._Active = value;
			}
		}

		public bool guiRunning
		{
			get
			{
				return this._Running;
			}
		}

		/*
		 * Methods
		 * */
		public void DrawGUI()
		{

		}

		public void StartGUI()
		{
			RenderingManager.AddToPostDrawQueue(3, this.DrawGUI);
		}

		public void StopGUI()
		{
			RenderingManager.RemoveFromPostDrawQueue(3, this.DrawGUI);
		}

		public void LoadConfig()
		{
			var config = KSP.IO.PluginConfiguration.CreateForType<VOID_Core>();
			config.load();
			this.main_window_pos = config.GetValue("main_window_pos", this.main_window_pos);
			this.main_icon_pos = config.GetValue("main_icon_pos", this.main_icon_pos);
		}

		public void SaveConfig()
		{
		}
	}
}

