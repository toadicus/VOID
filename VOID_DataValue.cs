// VOID
//
// VOID_DataValue.cs
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

using System;
using ToadicusTools;
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
					HighLogic.LoadedSceneIsEditor ||
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

		public virtual string ValueUnitString() {
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
			double magnitude;
			double magLimit;

			magnitude = Math.Log10(Math.Abs((double)this));

			magLimit = Math.Max(Math.Abs(magnitude), 3d) + 3d;
			magLimit = Math.Round(Math.Ceiling(magLimit / 3f)) * 3d;

			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
			GUILayout.Label(this.Label + "ⁱ:", GUILayout.ExpandWidth(true));
			GUILayout.FlexibleSpace();

			if (magnitude >= 0)
			{
				GUILayout.Label(this.ValueUnitString(3, int.MinValue, (int)magnitude - digits), GUILayout.ExpandWidth(false));
			}
			else
			{
				GUILayout.Label(this.ValueUnitString(3, (int)magnitude + digits, int.MaxValue), GUILayout.ExpandWidth(false));
			}
			GUILayout.EndHorizontal();

			if (Event.current.type == EventType.mouseUp)
			{
				Rect lastRect = GUILayoutUtility.GetLastRect();
				if (lastRect.Contains(Event.current.mousePosition))
				{
					Tools.PostDebugMessage(string.Format("{0}: Changing digits from {1} within magLimit {2}.",
						this.GetType().Name,
						digits,
						magLimit));

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
						digits += (int)magLimit;
					}

					Tools.PostDebugMessage(string.Format("{0}: Changed digits to {1}." +
						"\n\tNew minMagnitude: {2}, maxMagnitude: {3}" +
						"\n\tMagnitude: {4}",
						this.GetType().Name,
						digits,
						magnitude >= 0 ? int.MinValue : (int)magnitude - 4 + digits,
						magnitude >= 0 ? (int)magnitude - digits : int.MaxValue,
						magnitude
					));
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

	public class VOID_Vector3dValue : VOID_DataValue<Vector3d>
	{
		public VOID_Vector3dValue(string Label, Func<Vector3d> ValueFunc, string Units)
			: base(Label, ValueFunc, Units)
		{}

		public string ToString(string format)
		{
			return string.Format("{0}: {1}{2}",
				this.Label,
				this.Value.ToString(format),
				this.Units
			);
		}

		public string ValueUnitString(string format) {
			return this.Value.ToString(format) + this.Units;
		}
	}
}

