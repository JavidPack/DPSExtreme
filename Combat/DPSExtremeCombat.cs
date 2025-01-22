using DPSExtreme.Combat.Stats;
using DPSExtreme.UIElements.Displays;
using System;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.Localization;

namespace DPSExtreme.Combat
{
	internal partial class DPSExtremeCombat
	{
		[Flags]
		internal enum CombatType
		{
			//Order matters. Highest value is considered most important and will be used when displaying title of combat
			Generic = 1 << 0,
			Event = 1 << 1,
			Invasion = 1 << 2,
			BossFight = 1 << 3
		}

		internal enum InvasionType
		{
			None = 0,

			//Matching Terraria.InvasionID
			GoblinArmy = 1,
			SnowLegion = 2,
			PirateInvasion = 3,
			MartianMadness = 4,

			ModdedInvasionsStart = 5,
			//Range for modded invasions
			ModdedInvasionsEnd = 1000,

			//Outside of Terraria.InvasionID. Gets set based on bools
			PumpkinMoon = 1001,
			FrostMoon = 1002,
			OldOnesArmy = 1003,
			End
		}

		internal enum EventType
		{
			BloodMoon = InvasionType.End,
			Eclipse,
			SlimeRain,
			//No support for modded events
		}

		internal CombatType myCombatTypeFlags;
		internal CombatType myHighestCombatType;
		internal int myBossOrInvasionOrEventType = -1;

		internal int myTicksSinceLastActivity = 0;
		internal TimeSpan myTimeSinceLastActivity => new TimeSpan(0, 0, myTicksSinceLastActivity / 60);

		internal int myDurationInTicks = 0;
		internal TimeSpan myDuration => new TimeSpan(0, 0, myDurationInTicks / 60);

		internal string myFormattedDuration => String.Format("{0:D2}:{1:D2}", (int)Math.Floor(myDuration.TotalMinutes), myDuration.Seconds);

		internal CombatStats myStats = new CombatStats();

		public DPSExtremeCombat(CombatType aCombatType, int aBossOrInvasionOrEventType) {
			myCombatTypeFlags = aCombatType;
			myHighestCombatType = aCombatType;
			myBossOrInvasionOrEventType = aBossOrInvasionOrEventType;
		}

		internal object GetInfoContainer(ListDisplayMode aDisplayMode) {
			switch (aDisplayMode) {
				case ListDisplayMode.DamagePerSecond:
					return myStats.myDamagePerSecond;
				case ListDisplayMode.DamageDone:
					return myStats.myDamageDone;
				case ListDisplayMode.MinionDamageDone:
					return myStats.myMinionDamageDone;
				case ListDisplayMode.DamageTaken:
					return myStats.myDamageTaken;
				case ListDisplayMode.EnemyDamageTaken:
					return myStats.myEnemyDamageTaken;
				case ListDisplayMode.Deaths:
					return myStats.myDeaths;
				case ListDisplayMode.Kills:
					return myStats.myKills;
				case ListDisplayMode.ManaUsed:
					return myStats.myManaUsed;
				case ListDisplayMode.BuffUptime:
					return myStats.myBuffUptimes;
				case ListDisplayMode.DebuffUptime:
					return myStats.myDebuffUptimes;
				default:
					return null;
			}
		}

		internal void OnPlayerLeft(int aPlayer) {
			//Move player's stats into designated part of the buffer for disconnected players
			for (int i = (int)InfoListIndices.DisconnectedPlayersStart; i < (int)InfoListIndices.DisconnectedPlayersEnd; i++) {
				if (myStats.myDamageDone[i].HasStats())
					continue;

				myStats.ReassignStats(aPlayer, i);
				break;
			}
		}

		internal void OnEnd() {
			SendStats();
			PrintStats();

			for (int i = 0; i < 256; i++) {
				if (i >= (int)InfoListIndices.DisconnectedPlayersEnd)
					break;

				Player player = Main.player[i];

				foreach ((int itemType, MinionDamageStatValue stat) in myStats.myMinionDamageDone[i]) {
					Item summonItem = ContentSamples.ItemsByType[itemType - (int)DamageSource.SourceType.Item];

					if (summonItem == null)
						continue;

					Projectile projectile = ContentSamples.ProjectilesByType[summonItem.shoot];

					myStats.myMinionCounts[player.whoAmI][projectile.type] = player.ownedProjectileCounts[projectile.type];
				}
			}
		}

		internal void SendStats() {
			try {
				if (Main.netMode != NetmodeID.Server)
					return;

				ProtocolPushCombatStats push = new ProtocolPushCombatStats();
				push.myActiveCombatDurationInTicks = myDurationInTicks;
				push.myStats = myStats;
				push.myTotalCombatDurationInTicks = DPSExtreme.instance.combatTracker.myTotalCombat.myDurationInTicks;
				push.myTotalStats = DPSExtreme.instance.combatTracker.myTotalCombat.myStats;

				DPSExtreme.instance.packetHandler.SendProtocol(push);

				//if (Main.netMode == NetmodeID.Server)
				//{
				//	Dictionary<byte, int> stats = new Dictionary<byte, int>();
				//	for (int i = 0; i < 256; i++)
				//	{
				//		if (myStats.myDamageDone[i].HasStats())
				//		{
				//			int max = 0;
				//			int participantTotal = 0;
				//			myStats.myDamageDone[i].GetMaxAndTotal(out max, out participantTotal);

				//			stats[(byte)i] = participantTotal;
				//		}
				//	}

				//	DPSExtreme.instance.InvokeOnSimpleBossStats(stats);
				//}

			}
			catch (Exception) {
				//ErrorLogger.Log("SendStats" + e.Message);
			}
		}

		internal void PrintStats() {
			StringBuilder sb = new StringBuilder();
			//sb.Append(Language.GetText(DPSExtreme.instance.GetLocalizationKey("DamageStatsForNPC")).Format(Lang.GetNPCNameValue(npc.type)));
			// Add DamageStatsForCombat line

			for (int i = 0; i < 256; i++) {
				int max = 0;
				int participantDamage = 0;
				myStats.myDamageDone[i].GetMaxAndTotal(out max, out participantDamage);

				if (participantDamage > 0) {
					if (i == (int)InfoListIndices.NPCs) {
						sb.Append(string.Format("{0}: {1}, ", Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("TownNPC")), participantDamage));
					}
					else if (i == (int)InfoListIndices.Traps) {
						sb.Append(string.Format("{0}: {1}, ", Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("Traps")), participantDamage));
					}
					else if (i == (int)InfoListIndices.DOTs) {
						sb.Append(string.Format("{0}: {1}, ", Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("DamageOverTime")), participantDamage));
					}
					else {
						sb.Append(string.Format("{0}: {1}, ", Main.player[i].name, participantDamage));
					}
				}
			}

			if (sb.Length > 2)
				sb.Length -= 2; // removes last ,

			DPSExtreme.instance.DebugMessage(sb.ToString());
		}

		internal string GetTitle() {
			switch (myHighestCombatType) {
				case CombatType.BossFight:
					if (myBossOrInvasionOrEventType > -1) {
						string bossName = Lang.GetNPCNameValue(myBossOrInvasionOrEventType);
						return Language.GetText(bossName).Value;
					}
					else {
						return Language.GetText(DPSExtreme.instance.GetLocalizationKey("NoBoss")).Value;
					}
				case CombatType.Invasion:
					InvasionType invasionType;
					if (myBossOrInvasionOrEventType >= (int)InvasionType.ModdedInvasionsStart &&
						myBossOrInvasionOrEventType < (int)InvasionType.ModdedInvasionsEnd) {
						invasionType = InvasionType.ModdedInvasionsStart;
					}
					else {
						invasionType = (InvasionType)myBossOrInvasionOrEventType;
					}

					switch (invasionType) {
						case InvasionType.GoblinArmy:
							return Language.GetTextValue("Bestiary_Invasions.Goblins");
						case InvasionType.SnowLegion:
							return Language.GetTextValue("Bestiary_Invasions.FrostLegion");
						case InvasionType.PirateInvasion:
							return Language.GetTextValue("Bestiary_Invasions.Pirates");
						case InvasionType.MartianMadness:
							return Language.GetTextValue("Bestiary_Invasions.Martian");
						case InvasionType.PumpkinMoon:
							return Language.GetTextValue("Bestiary_Invasions.PumpkinMoon");
						case InvasionType.FrostMoon:
							return Language.GetTextValue("Bestiary_Invasions.FrostMoon");
						case InvasionType.OldOnesArmy:
							return Language.GetTextValue("Bestiary_Invasions.OldOnesArmy");
						case InvasionType.ModdedInvasionsStart:
							//TODO: Boss checklist support to fetch name?
							goto default;
						default:
							return Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("Invasion"));
					}
				case CombatType.Event:
					switch ((EventType)myBossOrInvasionOrEventType) {
						case EventType.BloodMoon:
							return Language.GetTextValue("Bestiary_Events.BloodMoon");
						case EventType.Eclipse:
							return Language.GetTextValue("Bestiary_Events.Eclipse");
						case EventType.SlimeRain:
							return Language.GetTextValue("Bestiary_Events.SlimeRain");
						default:
							return Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("Event"));
					}
				case CombatType.Generic:
					//Maybe display name of first npc hit?
					return Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("Combat"));
				default:
					return "Unknown combat type";
			}
		}
	}
}