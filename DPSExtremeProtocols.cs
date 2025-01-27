using DPSExtreme.Combat.Stats;
using System.IO;
using static DPSExtreme.Combat.DPSExtremeCombat;

namespace DPSExtreme
{
	enum DPSExtremeMessageType : byte
	{
		StartCombatPush,
		UpgradeCombatPush,
		EndCombatPush,
		ShareCurrentDPSReq,
		CurrentCombatTotalsPush,
	}

	abstract internal class DPSExtremeProtocol
	{
		abstract public void ToStream(BinaryWriter aWriter);
		abstract public bool FromStream(BinaryReader aReader);

		abstract public DPSExtremeMessageType GetDelimiter();
	}

	internal class ProtocolPushStartCombat : DPSExtremeProtocol
	{
		public override DPSExtremeMessageType GetDelimiter() { return DPSExtremeMessageType.StartCombatPush; }

		public override void ToStream(BinaryWriter aWriter) {
			aWriter.Write((byte)GetDelimiter());

			aWriter.Write((byte)myCombatType);
			aWriter.Write(myBossOrInvasionOrEventType);
		}

		public override bool FromStream(BinaryReader aReader) {
			myCombatType = (CombatType)aReader.ReadByte();
			myBossOrInvasionOrEventType = aReader.ReadInt32();

			return true;
		}

		public CombatType myCombatType = CombatType.Generic;
		public int myBossOrInvasionOrEventType = -1;
	}

	internal class ProtocolPushUpgradeCombat : DPSExtremeProtocol
	{
		public override DPSExtremeMessageType GetDelimiter() { return DPSExtremeMessageType.UpgradeCombatPush; }

		public override void ToStream(BinaryWriter aWriter) {
			aWriter.Write((byte)GetDelimiter());

			aWriter.Write((byte)myCombatType);
			aWriter.Write(myBossOrInvasionOrEventType);
		}

		public override bool FromStream(BinaryReader aReader) {
			myCombatType = (CombatType)aReader.ReadByte();
			myBossOrInvasionOrEventType = aReader.ReadInt32();

			return true;
		}

		public CombatType myCombatType = CombatType.Generic;
		public int myBossOrInvasionOrEventType = -1;
	}

	internal class ProtocolPushEndCombat : DPSExtremeProtocol
	{
		public override DPSExtremeMessageType GetDelimiter() { return DPSExtremeMessageType.EndCombatPush; }

		public override void ToStream(BinaryWriter aWriter) {
			aWriter.Write((byte)GetDelimiter());

			aWriter.Write((byte)myCombatType);
		}

		public override bool FromStream(BinaryReader aReader) {
			myCombatType = (CombatType)aReader.ReadByte();

			return true;
		}

		public CombatType myCombatType = CombatType.Generic;
	}

	internal class ProtocolPushCombatStats : DPSExtremeProtocol
	{
		public override DPSExtremeMessageType GetDelimiter() { return DPSExtremeMessageType.CurrentCombatTotalsPush; }

		public override void ToStream(BinaryWriter aWriter) {
			aWriter.Write((byte)GetDelimiter());

			aWriter.Write7BitEncodedInt(myActiveCombatDurationInTicks);
			myStats.ToStream(aWriter);
			aWriter.Write7BitEncodedInt(myTotalCombatDurationInTicks);
			myTotalStats.ToStream(aWriter);
		}

		public override bool FromStream(BinaryReader aReader) {
			myActiveCombatDurationInTicks = aReader.Read7BitEncodedInt();
			myStats.FromStream(aReader);
			myTotalCombatDurationInTicks = aReader.Read7BitEncodedInt();
			myTotalStats.FromStream(aReader);

			return true;
		}

		public int myActiveCombatDurationInTicks = 0;
		public CombatStats myStats = new CombatStats();
		public int myTotalCombatDurationInTicks = 0;
		public CombatStats myTotalStats = new CombatStats();
	}

	internal class ProtocolReqShareCurrentDPS : DPSExtremeProtocol
	{
		public override DPSExtremeMessageType GetDelimiter() { return DPSExtremeMessageType.ShareCurrentDPSReq; }

		public override void ToStream(BinaryWriter aWriter) {
			aWriter.Write((byte)GetDelimiter());

			aWriter.Write(myPlayer);
			aWriter.Write(myDPS);
			myDamageDoneBreakdown.ToStream(aWriter);
			myEnemyDamageTakenByMeBreakdown.ToStream(aWriter);
			myMinionDamageDoneBreakdown.ToStream(aWriter);
		}

		public override bool FromStream(BinaryReader aReader) {
			myPlayer = aReader.ReadInt32();
			myDPS = aReader.ReadInt32();
			myDamageDoneBreakdown.FromStream(aReader);
			myEnemyDamageTakenByMeBreakdown.FromStream(aReader);
			myMinionDamageDoneBreakdown.FromStream(aReader);

			return true;
		}

		public int myPlayer = 0;
		public int myDPS = 0;
		public DPSExtremeStatDictionary<int, DamageStatValue> myDamageDoneBreakdown = new();
		public DPSExtremeStatDictionary<int, DPSExtremeStatDictionary<int, DamageStatValue>> myEnemyDamageTakenByMeBreakdown = new();
		public DPSExtremeStatDictionary<int, MinionDamageStatValue> myMinionDamageDoneBreakdown = new();
	}
}