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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VOID
{
	public abstract class VOID_Module : IVOID_Module
	{
		/*
		 * Fields
		 * */
		[AVOID_SaveValue("Active")]
		protected VOID_SaveValue<bool> _Active = true;
		protected bool _Running = false;

		protected string _Name;

		/*
		 * Properties
		 * */
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

		public virtual Vessel vessel
		{
			get
			{
				return FlightGlobals.ActiveVessel;
			}
		}

		/*
		 * Methods
		 * */
		public void StartGUI()
		{
			if (!this.toggleActive)
			{
				return;
			}

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

		public virtual void DrawConfigurables() {}

		public virtual void LoadConfig()
		{
			var config = KSP.IO.PluginConfiguration.CreateForType<VOID_Core> ();
			config.load ();

			foreach (var field in this.GetType().GetFields(
				BindingFlags.NonPublic |
				BindingFlags.Public |
				BindingFlags.Instance
				))
			{
				object[] attrs = field.GetCustomAttributes(typeof(AVOID_SaveValue), false);

				if (attrs.Length == 0) {
					return;
				}

				AVOID_SaveValue attr = attrs.FirstOrDefault () as AVOID_SaveValue;

				string fieldName = string.Format("{0}_{1}", this.GetType().Name, attr.Name);

				object fieldValue = field.GetValue(this);

				bool convertBack = false;
				if (fieldValue is IVOID_SaveValue)
				{
					fieldValue = (fieldValue as IVOID_SaveValue).AsType;
					convertBack = true;
				}

				fieldValue = config.GetValue(fieldName, fieldValue);

				if (convertBack)
				{
					Type type = typeof(VOID_SaveValue<>).MakeGenericType (fieldValue.GetType ());
					IVOID_SaveValue convertValue = Activator.CreateInstance (type) as IVOID_SaveValue;
					convertValue.SetValue (fieldValue);
					fieldValue = convertValue;
				}

				field.SetValue (this, fieldValue);

				Tools.PostDebugMessage(string.Format("{0}: Loaded field {1}.", this.GetType().Name, fieldName));
			}
		}

		public virtual void SaveConfig()
		{
			if (!VOID_Core.Instance.configDirty)
			{
				return;
			}

			var config = KSP.IO.PluginConfiguration.CreateForType<VOID_Core> ();
			config.load ();

			foreach (var field in this.GetType().GetFields(
				BindingFlags.NonPublic |
				BindingFlags.Public |
				BindingFlags.Instance
				))
			{
				object[] attrs = field.GetCustomAttributes(typeof(AVOID_SaveValue), false);

				if (attrs.Length == 0) {
					return;
				}

				AVOID_SaveValue attr = attrs.FirstOrDefault () as AVOID_SaveValue;

				string fieldName = string.Format("{0}_{1}", this.GetType().Name, attr.Name);

				object fieldValue = field.GetValue(this);

				if (fieldValue is IVOID_SaveValue)
				{
					fieldValue = (fieldValue as IVOID_SaveValue).AsType;
				}

				config.SetValue(fieldName, fieldValue);

				Tools.PostDebugMessage(string.Format("{0}: Saved field {1}.", this.GetType().Name, fieldName));
			}

			config.save ();
		}
	}
}

