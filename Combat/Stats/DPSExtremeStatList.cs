
using DPSExtreme.Combat.Stats;
using System;
using System.Collections.Generic;
using System.IO;

namespace DPSExtreme
{
	internal enum InfoListIndices
	{
		Players = 0,
		//0-... is players
		SupportedPlayerCount = 240,
		DisconnectedPlayersStart = 241,
		DisconnectedPlayersEnd = 250,
		DOTs = 253,
		Traps = 254,
		NPCs = 255
	}

	internal class DPSExtremeStatList<T> : IStatContainer
		where T : IStatContainer, new()
	{
		internal T[] myValues;

		public DPSExtremeStatList() {
			myValues = new T[Size];

			for (int i = 0; i < Size; i++) {
				myValues[i] = new T();
			}
		}

		public ref T this[int i] {
			get { return ref myValues[i]; }
		}

		public const int Size = 256;

		public void Clear() {
			for (int i = 0; i < Size; i++) {
				myValues[i] = default;
			}
		}

		public void GetMaxAndTotal(out int aMax, out int aTotal) {
			aMax = 0;
			aTotal = 0;

			int subMax = 0;
			int subTotal = 0;

			for (int i = 0; i < Size; i++) {
				IStatContainer stat = myValues[i];
				if (stat.HasStats()) {
					stat.GetMaxAndTotal(out subMax, out subTotal);

					aMax = Math.Max(aMax, subMax);
					aTotal += subTotal;
				}
			}
		}

		public virtual List<string> GetInfoBoxLines() { return new List<string>(); }

		public bool HasStats() {
			for (int i = 0; i < Size; i++) {
				if (myValues[i].HasStats())
					return true;
			}

			return false;
		}

		public void ToStream(BinaryWriter aWriter) {
			long startPos = aWriter.BaseStream.Position;

			aWriter.Write((byte)123); //placeholder for count

			byte count = 0;
			for (int i = 0; i < Size; i++) {
				if (!myValues[i].HasStats())
					continue;

				aWriter.Write((byte)i);
				myValues[i].ToStream(aWriter);
				count++;
			}

			long endPos = aWriter.BaseStream.Position;

			aWriter.BaseStream.Seek(startPos, SeekOrigin.Begin);
			aWriter.Write(count);
			aWriter.BaseStream.Seek(endPos, SeekOrigin.Begin);
		}

		public void FromStream(BinaryReader aReader) {
			int count = aReader.ReadByte();

			for (int i = 0; i < count; i++) {
				byte index = aReader.ReadByte();
				myValues[index].FromStream(aReader);
			}
		}
	}
}