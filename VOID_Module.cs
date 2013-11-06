//
//  VOID_Module.cs
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

namespace VOID
{
	public abstract class VOID_Module
	{
		/*
		 * Fields
		 * */
		protected bool _Active = true;
		protected bool _Running = false;
		protected bool _hasGUICfg = false;
		protected bool configDirty = false;

		protected string _Name;

		/*
		 * Properties
		 * */
		public virtual bool hasGUIConfig
		{
			get
			{
				return this._hasGUICfg;
			}
		}

		public virtual bool toggleActive
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

		public virtual bool guiRunning
		{
			get
			{
				return this._Running;
			}
		}

		public virtual string Name
		{
			get
			{
				return this._Name;
			}
		}

		/*
		 * Methods
		 * */
		public void StartGUI()
		{
			Tools.PostDebugMessage (string.Format("Adding {0} to the draw queue.", this.GetType().Name));
			RenderingManager.AddToPostDrawQueue (3, this.DrawGUI);
			this._Running = true;
		}

		public void StopGUI()
		{
			Tools.PostDebugMessage (string.Format("Removing {0} from the draw queue.", this.GetType().Name));
			RenderingManager.RemoveFromPostDrawQueue (3, this.DrawGUI);
			this._Running = false;
		}

		public abstract void DrawGUI();

		public abstract void LoadConfig();

		public abstract void SaveConfig();

		~VOID_Module()
		{
			this.SaveConfig ();
		}
	}
}

