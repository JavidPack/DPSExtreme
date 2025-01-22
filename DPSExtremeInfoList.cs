using System.IO;
using static DPSExtreme.CombatTracking.DPSExtremeCombat;

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
		Traps = 255,
		NPCs = 255
	}

	internal class DPSExtremeInfoList<T> where T : IDPSExtremeCombatInfo, new()
	{
		internal T[] myInfos;

		public DPSExtremeInfoList() {
			myInfos = new T[Size()];

			for (int i = 0; i < Size(); i++) {
				myInfos[i] = new T();
			}
		}

		public ref T this[int i] {
			get { return ref myInfos[i]; }
		}

		public int Size() { return 256; }

		public void Clear() {
			for (int i = 0; i < Size(); i++) {
				myInfos[i] = default;
			}
		}

		public void ToStream(BinaryWriter aWriter) {
			long startPos = aWriter.BaseStream.Position;

			aWriter.Write((byte)123); //placeholder for count

			byte count = 0;
			for (int i = 0; i < Size(); i++) {
				if (!myInfos[i].HasData())
					continue;

				aWriter.Write((byte)i);
				myInfos[i].ToStream(aWriter);
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
				byte damageDealerIndex = aReader.ReadByte();
				myInfos[damageDealerIndex].FromStream(aReader);
			}
		}
	}
}
