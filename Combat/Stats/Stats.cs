using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;

namespace DPSExtreme.Combat.Stats
{
	enum StatFormat
	{
		RawNumber,
		Time
	}

	internal class CombatStats
	{
		internal DPSExtremeStatDictionary<int, DPSExtremeStatList<DPSExtremeStatDictionary<int, DamageStatValue>>> myEnemyDamageTaken = new();

		internal DPSExtremeStatList<DPSExtremeStatDictionary<int, DamageStatValue>> myDamageDone = new();
		internal DPSExtremeStatList<DPSExtremeStatDictionary<int, StatValue>> myMinionCounts = new(); //Helper for MinionDamageDone display
		internal DPSExtremeStatList<DPSExtremeStatDictionary<int, MinionDamageStatValue>> myMinionDamageDone = new();
		internal DPSExtremeStatList<DPSExtremeStatDictionary<int, DPSExtremeStatDictionary<int, DamageStatValue>>> myDamageTaken = new();
		internal DPSExtremeStatList<DeathStatValue> myDeaths = new();
		internal DPSExtremeStatList<DPSExtremeStatDictionary<int, StatValue>> myKills = new();
		internal DPSExtremeStatList<DPSExtremeStatDictionary<int, StatValue>> myManaUsed = new();

		internal DPSExtremeStatList<DPSExtremeStatDictionary<int, TimeStatValue>> myBuffUptimes = new();
		internal DPSExtremeStatList<DPSExtremeStatDictionary<int, TimeStatValue>> myDebuffUptimes = new();

		internal DPSExtremeStatList<StatValue> myDamagePerSecond = new();

		public void ToStream(BinaryWriter aWriter) {
			myEnemyDamageTaken.ToStream(aWriter);
			myDamagePerSecond.ToStream(aWriter);
			myDamageDone.ToStream(aWriter);
			myMinionCounts.ToStream(aWriter);
			myMinionDamageDone.ToStream(aWriter);
			myDamageTaken.ToStream(aWriter);
			myDeaths.ToStream(aWriter);
			myKills.ToStream(aWriter);
			myManaUsed.ToStream(aWriter);
			myBuffUptimes.ToStream(aWriter);
			myDebuffUptimes.ToStream(aWriter);
		}

		public void FromStream(BinaryReader aReader) {
			myEnemyDamageTaken.FromStream(aReader);
			myDamagePerSecond.FromStream(aReader);
			myDamageDone.FromStream(aReader);
			myMinionCounts.FromStream(aReader);
			myMinionDamageDone.FromStream(aReader);
			myDamageTaken.FromStream(aReader);
			myDeaths.FromStream(aReader);
			myKills.FromStream(aReader);
			myManaUsed.FromStream(aReader);
			myBuffUptimes.FromStream(aReader);
			myDebuffUptimes.FromStream(aReader);
		}

		internal void ReassignStats(int aFrom, int aTo) {
			foreach ((int npcType, DPSExtremeStatList<DPSExtremeStatDictionary<int, DamageStatValue>> damageInfo) in myEnemyDamageTaken) {
				myEnemyDamageTaken[npcType][aTo] = myEnemyDamageTaken[npcType][aFrom];
			}

			myDamagePerSecond[aTo] = myDamagePerSecond[aFrom];
			myDamageDone[aTo] = myDamageDone[aFrom];
			myMinionCounts[aTo] = myMinionCounts[aFrom];
			myMinionDamageDone[aTo] = myMinionDamageDone[aFrom];
			myDamageTaken[aTo] = myDamageTaken[aFrom];
			myDeaths[aTo] = myDeaths[aFrom];
			myKills[aTo] = myKills[aFrom];
			myManaUsed[aTo] = myManaUsed[aFrom];

			myBuffUptimes[aTo] = myBuffUptimes[aFrom];
			myDebuffUptimes[aTo] = myDebuffUptimes[aFrom];

			ClearStatsForPlayer(aFrom);
		}

		internal void ClearStatsForPlayer(int aPlayer) {
			myDamageDone[aPlayer].Clear();
			myMinionDamageDone[aPlayer].Clear();

			foreach ((int npcType, DPSExtremeStatList<DPSExtremeStatDictionary<int, DamageStatValue>> damageInfo) in myEnemyDamageTaken)
				myEnemyDamageTaken[npcType][aPlayer] = new();

			myDamagePerSecond[aPlayer] = new();
			myDamageTaken[aPlayer].Clear();
			myDeaths[aPlayer] = new();
			myKills[aPlayer].Clear();
			myManaUsed[aPlayer].Clear();

			myBuffUptimes[aPlayer].Clear();
			myDebuffUptimes[aPlayer].Clear();
		}
	}

	internal interface IStatContainer
	{
		public abstract bool HasStats();
		public abstract void GetMaxAndTotal(out int aMax, out int aTotal);

		public abstract void ToStream(BinaryWriter aWriter);
		public abstract void FromStream(BinaryReader aReader);

		public abstract List<string> GetInfoBoxLines();
	}

	internal class StatValue : IStatContainer
	{
		internal int myValue = 0;

		public StatValue() {

		}

		public StatValue(int aValue) {
			myValue = aValue;
		}

		public bool HasStats() { return myValue > 0; }

		public virtual void GetMaxAndTotal(out int aMax, out int aTotal) {
			aMax = myValue;
			aTotal = myValue;
		}

		public virtual List<string> GetInfoBoxLines() { return new List<string>(); }

		internal static string FormatStatNumber(int aValue, StatFormat aFormat) {
			if (aFormat == StatFormat.RawNumber) {
				if (aValue >= 100000000)
					return FormatStatNumber(aValue / 1000000, aFormat) + "M";

				if (aValue >= 100000)
					return FormatStatNumber(aValue / 1000, aFormat) + "K";

				if (aValue >= 10000)
					return (aValue / 1000D).ToString("0.#") + "K";

				return aValue.ToString("#,0");
			}
			else if (aFormat == StatFormat.Time) {
				float seconds = (aValue / 60f) % 60;
				int minutes = (aValue / 60) / 60;
				if (minutes == 0)
					return seconds.ToString("#.0");
				else
					return string.Format("{0:D2}:{1:D2}", minutes, (int)seconds);
			}

			return "Invalid format";
		}

		public virtual void ToStream(BinaryWriter aWriter) {
			aWriter.Write7BitEncodedInt(myValue);
		}

		public virtual void FromStream(BinaryReader aReader) {
			myValue = aReader.Read7BitEncodedInt();
		}

		public static implicit operator StatValue(int aValue) { return new StatValue(aValue); }
		public static implicit operator int(StatValue aValue) { return aValue.myValue; }
		public static StatValue operator +(StatValue a, int b) { return new StatValue(a.myValue + b); }
		public static StatValue operator -(StatValue a, int b) { return new StatValue(a.myValue - b); }
		public static bool operator >(StatValue a, int b) { return a.myValue > b; }
		public static bool operator <(StatValue a, int b) { return a.myValue < b; }
	}

	internal class TimeStatValue : StatValue
	{
		//List<> myApplicationTimes
		public TimeStatValue() { }
		public TimeStatValue(int aValue) : base(aValue) { }

		public static TimeStatValue operator +(TimeStatValue a, int b) { return new TimeStatValue(a.myValue + b); }

		public override void GetMaxAndTotal(out int aMax, out int aTotal) {
			aMax = (int)DPSExtremeUI.instance.myDisplayedCombat.myDurationInTicks;
			aTotal = myValue;
		}
	}

	internal class DamageStatValue : StatValue
	{
		internal int myHitCount = 0;
		internal int myCritCount = 0;
		internal int myMaxHit = 0;

		public DamageStatValue() { }

		public void AddDamage(int aDamage, bool aCrit) {
			myHitCount += 1;
			myCritCount += aCrit ? 1 : 0;
			myValue += aDamage;
			myMaxHit = Math.Max(myMaxHit, aDamage);
		}

		public override List<string> GetInfoBoxLines() {
			List<string> lines = new List<string>();
			lines.Add(string.Format("Total Damage: {0}", myValue));
			lines.Add(string.Format("Hits: {0}", myHitCount));
			lines.Add(string.Format("Max hit: {0}", myMaxHit));
			if (myHitCount > 0)
				lines.Add(string.Format("Average Damage: {0}", myValue / myHitCount));
			if (myCritCount > 0)
				lines.Add(string.Format("Crits: {0} ({1:P0})", myCritCount, myCritCount / (float)myHitCount));

			return lines;
		}

		public override void ToStream(BinaryWriter aWriter) {
			base.ToStream(aWriter);
			aWriter.Write7BitEncodedInt(myHitCount);
			aWriter.Write7BitEncodedInt(myCritCount);
			aWriter.Write7BitEncodedInt(myMaxHit);
		}

		public override void FromStream(BinaryReader aReader) {
			base.FromStream(aReader);
			myHitCount = aReader.Read7BitEncodedInt();
			myCritCount = aReader.Read7BitEncodedInt();
			myMaxHit = aReader.Read7BitEncodedInt();
		}
	}

	internal class MinionDamageStatValue : DamageStatValue
	{
		internal int myMinionType = -1;
		internal int myMinionOwner = -1;

		public MinionDamageStatValue() { }

		public override void GetMaxAndTotal(out int aMax, out int aTotal) {
			aMax = 0;
			aTotal = 0;

			Projectile minion = ContentSamples.ProjectilesByType[myMinionType];
			if (minion == null)
				return;

			Player ownerPlayer = Main.player[myMinionOwner];

			DPSExtremeCombat displayedCombat = DPSExtremeUI.instance.myDisplayedCombat;
			DPSExtremeCombat activeCombat = DPSExtreme.instance.combatTracker.myActiveCombat;

			int ownedProjectilesOfType =
				displayedCombat == activeCombat ?
				ownerPlayer.ownedProjectileCounts[minion.type] :
				displayedCombat.myStats.myMinionCounts[ownerPlayer.whoAmI][minion.type];

			float minionSlotsTakenByType = Math.Max(ownedProjectilesOfType * minion.minionSlots, 1); //Some minions like stardust dragon has 0

			float damagePerMinionSlot = myValue / minionSlotsTakenByType;

			aMax = (int)damagePerMinionSlot;
			aTotal = (int)damagePerMinionSlot;
		}

		public override void ToStream(BinaryWriter aWriter) {
			base.ToStream(aWriter);
			aWriter.Write7BitEncodedInt(myMinionType);
			aWriter.Write7BitEncodedInt(myMinionOwner);
		}

		public override void FromStream(BinaryReader aReader) {
			base.FromStream(aReader);
			myMinionType = aReader.Read7BitEncodedInt();
			myMinionOwner = aReader.Read7BitEncodedInt();
		}
	}

	internal class DeathStatValue : StatValue
	{
		public List<int> myDeathTimesInTicks = new();

		public DeathStatValue() { }

		public void AddDeath(int aDeathTime) {
			myValue++;
			myDeathTimesInTicks.Add(aDeathTime);
		}

		public override List<string> GetInfoBoxLines() {
			List<string> lines = new List<string>();

			for (int i = myDeathTimesInTicks.Count - 1; i >= 0; i--) {
				int deathTime = myDeathTimesInTicks[i];

				float seconds = (deathTime / 60f) % 60;
				int minutes = (deathTime / 60) / 60;

				lines.Add(string.Format("{0}. {1:D2}:{2:D2}", i + 1, minutes, (int)seconds));
			}

			return lines;
		}

		public override void ToStream(BinaryWriter aWriter) {
			base.ToStream(aWriter);

			aWriter.Write7BitEncodedInt(myDeathTimesInTicks.Count);
			foreach (int deathTime in myDeathTimesInTicks)
				aWriter.Write7BitEncodedInt(deathTime);
		}

		public override void FromStream(BinaryReader aReader) {
			base.FromStream(aReader);

			int count = aReader.Read7BitEncodedInt();
			for (int i = 0; i < count; i++)
				myDeathTimesInTicks.Add(aReader.Read7BitEncodedInt());
		}
	}
}