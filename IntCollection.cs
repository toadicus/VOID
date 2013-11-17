//
//  IntCollection.cs
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
	public class IntCollection : IVOID_SaveValue
	{
		public static implicit operator long(IntCollection c)
		{
			return c.collection;
		}

		protected long mask;

		public long collection { get; protected set; }
		public ushort maxCount { get; protected set; }
		public ushort wordLength { get; protected set; }

		public Type type { get { return typeof(long); } }
		public object AsType { get { return this.collection; } }

		public void SetValue(object v)
		{
			this.collection = (long)v;
		}

		public IntCollection (ushort wordLength = 4, long initialCollection = 0)
		{
			this.collection = initialCollection;
			this.wordLength = wordLength;
			this.maxCount = (ushort)((sizeof(long) * 8 - 1) / wordLength);
			this.mask = ((1 << this.wordLength) - 1);
		}

		public ushort this[int idx]
		{
			get {
				if (idx < 0) {
					idx += this.maxCount;
				}

				if (idx >= maxCount || idx < 0) {
					throw new IndexOutOfRangeException ();
				}

				idx *= wordLength;

				return (ushort)((this.collection & (this.mask << idx)) >> idx);
			}
			set {
				Console.WriteLine (value);
				if (idx < 0) {
					idx += this.maxCount;
				}

				if (idx >= maxCount || idx < 0) {
					throw new IndexOutOfRangeException ();
				}

				idx *= wordLength;

				long packvalue = value & this.mask;
				Console.WriteLine (packvalue);

				this.collection &= ~(this.mask << idx);
				this.collection |= packvalue << idx;
			}
		}
	}
}

