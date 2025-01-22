
using System;
using System.Collections.Generic;
using System.IO;

namespace DPSExtreme.Combat.Stats
{
	internal class DPSExtremeStatDictionary<Key, Value> : Dictionary<int, Value>, IStatContainer
		where Value : IStatContainer, new()
	{
		public new Value this[int aKey] {
			get {
				if (!ContainsKey(aKey))
					Add(aKey, new Value());

				return base[aKey];
			}
			set => base[aKey] = value;
		}

		public void GetMaxAndTotal(out int aMax, out int aTotal) {
			aMax = 0;
			aTotal = 0;

			int subMax = 0;
			int subTotal = 0;
			foreach ((int _, Value stat) in this) {
				stat.GetMaxAndTotal(out subMax, out subTotal);

				aMax = Math.Max(aMax, subMax);
				aTotal += subTotal;
			}
		}

		public virtual List<string> GetInfoBoxLines() { return new List<string>(); }

		public bool HasStats() {
			foreach ((int _, Value stat) in this)
				if (stat.HasStats())
					return true;

			return false;
		}

		public void ToStream(BinaryWriter aWriter) {
			aWriter.Write7BitEncodedInt(Count);

			foreach ((int key, Value stat) in this) {
				aWriter.Write7BitEncodedInt(key);
				stat.ToStream(aWriter);
			}
		}

		public void FromStream(BinaryReader aReader) {
			int count = aReader.Read7BitEncodedInt();

			for (int i = 0; i < count; i++) {
				int key = aReader.Read7BitEncodedInt();
				this[key].FromStream(aReader);
			}
		}
	}
}