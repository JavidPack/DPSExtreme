using System.IO;

namespace DPSExtreme.CombatTracking
{
	//Placed in partial class because it was complaining about being unable to box/unbox types and I can't be bothered figuring that mess out
	internal partial class DPSExtremeCombat
	{
		public interface IDPSExtremeCombatInfo
		{
			public bool HasData();

			public bool ToStream(BinaryWriter aWriter);
			public bool FromStream(BinaryReader aReader);
		}

		public struct DamageDealtInfo : IDPSExtremeCombatInfo
		{
			internal int myDamage = -1;

			public DamageDealtInfo() {
			}

			public bool HasData() { return myDamage != -1; }

			public bool ToStream(BinaryWriter aWriter) {
				if (myDamage == -1)
					return false;

				aWriter.Write(myDamage);
				return true;
			}

			public bool FromStream(BinaryReader aReader) {
				myDamage = aReader.ReadInt32();

				return true;
			}
		}
	}
}
