//
//  VOID_Config.cs
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

using KSP;
using System;
using System.Collections.Generic;
using ToadicusTools;
using UnityEngine;

namespace VOID
{
	public struct VOID_SaveValue<T> : IVOID_SaveValue
	{
		private T _value;
		private Type _type;

		private VOID_Core Core
		{
			get
			{
				if (HighLogic.LoadedSceneIsEditor)
				{
					if (VOID_EditorCore.Initialized)
					{
						return VOID_EditorCore.Instance;
					}
				}
				else if (HighLogic.LoadedSceneIsFlight)
				{
					if (VOID_Core.Initialized)
					{
						return VOID_Core.Instance;
					}
				}
				return null;
			}
		}

		public T value
		{
			get
			{
				return this._value;
			}
			set
			{
				if (this.Core != null && !System.Object.Equals(this._value, value))
				{
					Tools.PostDebugMessage (string.Format (
						"VOID: Dirtying config for type {0} in method {1}." +
						"\n\t Old Value: {2}, New Value: {3}" +
						"\n\t Object.Equals(New, Old): {4}",
						this._type,
						new System.Diagnostics.StackTrace().GetFrame(1).GetMethod(),
						this._value,
						value,
						System.Object.Equals(this._value, value)
					));
					this.Core.configDirty = true;
				}
				this._value = value;
			}
		}

		public Type type
		{
			get
			{
				if (this._type == null && this._value != null)
				{
					this._type = this._value.GetType ();
				}
				return this._type;
			}
			set
			{
				this._type = value;
			}
		}

		public object AsType
		{
			get
			{
				return (T)this._value;
			}
		}

		public void SetValue(object v)
		{
			this.value = (T)v;
		}

		public static implicit operator T(VOID_SaveValue<T> v)
		{
			return (T)v.value;
		}

		public static implicit operator VOID_SaveValue<T>(T v)
		{
			VOID_SaveValue<T> r = new VOID_SaveValue<T>();
			r.type = v.GetType();
			r.value = v;

			return r;
		}

		public override string ToString()
		{
			return this.value.ToString();
		}
	}

	public interface IVOID_SaveValue
	{
		Type type { get; }
		object AsType { get; }
		void SetValue(object v);
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class AVOID_SaveValue : Attribute
	{
		protected string _name;

		public string Name
		{
			get
			{
				return this._name;
			}
		}

		public AVOID_SaveValue(string fieldName)
		{
			this._name = fieldName;
		}
	}
}

