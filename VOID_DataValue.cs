//
//  VOID_DataValue.cs
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
using UnityEngine;

namespace VOID
{
	public interface IVOID_DataValue
	{
		void Refresh();
		string ValueUnitString();
		void DoGUIHorizontal();
	}

	public class VOID_DataValue<T> : IVOID_DataValue
	{
		/*
		 * Static Members
		 * */
		public static implicit operator T(VOID_DataValue<T> v)
		{
			return (T)v.Value;
		}

		/*
		 * Instance Members
		 * */
		/*
		* Fields
		 * */
		protected T cache;
		protected Func<T> ValueFunc;
		protected float lastUpdate;

		/* 
		 * Properties
		 * */
		public string Label { get; protected set; }
		public string Units { get; protected set; }

		public T Value
		{
			get
			{
				if (
					(VOID_Core.Instance.updateTimer - this.lastUpdate > VOID_Core.Instance.updatePeriod) ||
					(this.lastUpdate > VOID_Core.Instance.updateTimer)
				)
				{
					this.Refresh();
				}
				return (T)this.cache;
			}
		}

		/*
		 * Methods
		 * */
		public VOID_DataValue(string Label, Func<T> ValueFunc, string Units = "")
		{
			this.Label = Label;
			this.Units = Units;
			this.ValueFunc = ValueFunc;
			this.lastUpdate = 0;
		}

		public void Refresh()
		{
			this.cache = this.ValueFunc.Invoke ();
			this.lastUpdate = VOID_Core.Instance.updateTimer;
		}

		public T GetFreshValue()
		{
			this.Refresh ();
			return (T)this.cache;
		}

		public string ValueUnitString() {
			return this.Value.ToString() + this.Units;
		}

		public virtual void DoGUIHorizontal()
		{
			GUILayout.BeginHorizontal (GUILayout.ExpandWidth (true));
			GUILayout.Label (this.Label + ":");
			GUILayout.FlexibleSpace ();
			GUILayout.Label (this.ValueUnitString(), GUILayout.ExpandWidth (false));
			GUILayout.EndHorizontal ();
		}

		public override string ToString()
		{
			return string.Format (
				"{0}: {1}{2}",
				this.Label,
				this.Value.ToString (),
				this.Units
			);
		}
	}

	public abstract class VOID_NumValue<T> : VOID_DataValue<T>
		where T : IFormattable, IConvertible, IComparable
	{
		public static implicit operator Double(VOID_NumValue<T> v)
		{
			return v.ToDouble();
		}

		public static implicit operator Int32(VOID_NumValue<T> v)
		{
			return v.ToInt32();
		}


		public static implicit operator Single(VOID_NumValue<T> v)
		{
			return v.ToSingle();
		}


		protected IFormatProvider formatProvider;

		public VOID_NumValue(string Label, Func<T> ValueFunc, string Units = "") : base(Label, ValueFunc, Units)
		{
			this.formatProvider = System.Globalization.CultureInfo.CurrentUICulture;
		}

		public virtual double ToDouble(IFormatProvider provider)
		{
			return this.Value.ToDouble(provider);
		}

		public virtual double ToDouble()
		{
			return this.ToDouble(this.formatProvider);
		}

		public virtual int ToInt32(IFormatProvider provider)
		{
			return this.Value.ToInt32(provider);
		}

		public virtual int ToInt32()
		{
			return this.ToInt32(this.formatProvider);
		}

		public virtual float ToSingle(IFormatProvider provider)
		{
			return this.Value.ToSingle(provider);
		}

		public virtual float ToSingle()
		{
			return this.ToSingle(this.formatProvider);
		}

		public virtual string ToString(string Format)
		{
			return string.Format (
				"{0}: {1}{2}",
				this.Label,
				this.Value.ToString(Format, this.formatProvider),
				this.Units
			);
		}

		public virtual string ToSIString(int digits = 3, int MinMagnitude = 0, int MaxMagnitude = int.MaxValue)
		{
			return string.Format (
				"{0}{1}",
				Tools.MuMech_ToSI (this, digits, MinMagnitude, MaxMagnitude),
				this.Units
			);
		}

		public virtual string ValueUnitString(string format)
		{
			return this.Value.ToString(format, this.formatProvider) + this.Units;
		}
		
		public virtual string ValueUnitString(int digits) {
			return Tools.MuMech_ToSI(this, digits) + this.Units;
		}

		public virtual string ValueUnitString(int digits, int MinMagnitude, int MaxMagnitude)
		{
			return Tools.MuMech_ToSI(this, digits, MinMagnitude, MaxMagnitude) + this.Units;
		}

		public virtual void DoGUIHorizontal(string format)
		{
			GUILayout.BeginHorizontal (GUILayout.ExpandWidth (true));
			GUILayout.Label (this.Label + ":");
			GUILayout.FlexibleSpace ();
			GUILayout.Label (this.ValueUnitString(format), GUILayout.ExpandWidth (false));
			GUILayout.EndHorizontal ();
		}

		public virtual int DoGUIHorizontal(int digits, bool precisionButton = true)
		{
			if (precisionButton)
			{
				return this.DoGUIHorizontalPrec(digits);
			}

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(this.Label + ":", GUILayout.ExpandWidth(true));
			GUILayout.FlexibleSpace();
			GUILayout.Label(this.ValueUnitString(digits), GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			return digits;
		}

		public virtual int DoGUIHorizontalPrec(int digits)
		{
			float magnitude;
			float magLimit;

			magnitude = (float)Math.Log10(Math.Abs(this));

			magLimit = Mathf.Max(magnitude, 6f);
			magLimit = Mathf.Round((float)Math.Ceiling(magLimit / 3f) * 3f);

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(this.Label + "ⁱ:", GUILayout.ExpandWidth(true));
			GUILayout.FlexibleSpace();

			GUILayout.Label(this.ValueUnitString(3, int.MinValue, (int)magnitude - digits), GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();

			if (Event.current.type == EventType.mouseUp)
			{
				Rect lastRect = GUILayoutUtility.GetLastRect();
				if (lastRect.Contains(Event.current.mousePosition))
				{
					if (Event.current.button == 0)
					{
						digits = (digits + 3) % (int)magLimit;
					}
					else if (Event.current.button == 1)
					{
						digits = (digits - 3) % (int)magLimit;
					}

					if (digits < 0)
					{
						digits = (int)magLimit - 3;
					}
				}
			}

			return digits;
		}
	}

	public class VOID_DoubleValue : VOID_NumValue<double>
	{
		public VOID_DoubleValue(string Label, Func<double> ValueFunc, string Units) : base(Label, ValueFunc, Units) {}
	}

	public class VOID_FloatValue : VOID_NumValue<float>
	{
		public VOID_FloatValue(string Label, Func<float> ValueFunc, string Units) : base(Label, ValueFunc, Units) {}
	}

	public class VOID_IntValue : VOID_NumValue<int>
	{
		public VOID_IntValue(string Label, Func<int> ValueFunc, string Units) : base(Label, ValueFunc, Units) {}
	}

	public class VOID_StrValue : VOID_DataValue<string>
	{
		public VOID_StrValue(string Label, Func<string> ValueFunc) : base(Label, ValueFunc, "") {}
	}
}

