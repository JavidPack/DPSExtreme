
using DPSExtreme.Combat.Stats;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.Localization;
using Terraria;

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

	internal static class DPSExtremeStatListHelper {
		internal static string GetNameFromIndex(int aIndex) {
			if (aIndex < 0)
				return string.Format("Invalid index: {0}", aIndex);

			if (aIndex < (int)InfoListIndices.SupportedPlayerCount) {
				return Main.player[aIndex].name;
			}
			else if (aIndex >= (int)InfoListIndices.DisconnectedPlayersStart && aIndex <= (int)InfoListIndices.DisconnectedPlayersEnd) {
				return Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("DisconnectedPlayer"));
			}
			else if (aIndex == (int)InfoListIndices.NPCs) {
				return Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("TownNPC"));
			}
			else if (aIndex == (int)InfoListIndices.Traps) {
				return Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("Traps"));
			}
			else if (aIndex == (int)InfoListIndices.DOTs) {
				return Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("DamageOverTime"));
			}

			return string.Format("Invalid index: {0}", aIndex);
		}
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