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
using System;
using System.Collections.Generic;
using KSP;
using UnityEngine;

namespace VOID
{
	public struct VOID_ConfigValue<T> : IVOID_ConfigValue
	{
		private T _value;
		private Type _type;

		public T value
		{
			get
			{
				return this._value;
			}
			set
			{
				this._value = value;
			}
		}

		public Type type
		{
			get
			{
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
			this._value = (T)v;
		}

		public static implicit operator T(VOID_ConfigValue<T> v)
		{
			return v.value;
		}

		public static implicit operator VOID_ConfigValue<T>(T v)
		{
			VOID_ConfigValue<T> r = new VOID_ConfigValue<T>();
			r.value = v;
			r.type = v.GetType();
			if (VOID_Core.Initialized)
			{
				VOID_Core.Instance.configDirty = true;
			}
			return r;
		}
	}

	public interface IVOID_ConfigValue
	{
		Type type { get; }
		object AsType { get; }
		void SetValue(object v);
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class AVOID_ConfigValue : Attribute
	{
		protected string _name;

		public string Name
		{
			get
			{
				return this._name;
			}
		}

		public AVOID_ConfigValue(string fieldName)
		{
			this._name = fieldName;
		}
	}
}

