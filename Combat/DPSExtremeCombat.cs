using DPSExtreme.Combat.Stats;
using DPSExtreme.Config;
using DPSExtreme.UIElements.Displays;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.Chat;
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

			if (Main.netMode != NetmodeID.MultiplayerClient)
				if (myHighestCombatType != CombatType.Generic)
					PrintStats("DPSExtreme", ListDisplayMode.DamageDone, DPSExtremeServerConfig.Instance.PostCombatDamageDonePrintLineCount);

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

		internal void HandleServerSync(CombatStats aStats) {
			var myPrevLocalDamage = myStats.myDamageDone[Main.LocalPlayer.whoAmI];
			var myPrevLocalMinionDamage = myStats.myMinionDamageDone[Main.LocalPlayer.whoAmI];
			var myPrevLocalEnemyDamageTaken = myStats.myEnemyDamageTaken;
			myStats = aStats;

			//Sync remote stats, but don't overwrite local
			myStats.myDamageDone[Main.LocalPlayer.whoAmI] = myPrevLocalDamage;
			myStats.myMinionDamageDone[Main.LocalPlayer.whoAmI] = myPrevLocalMinionDamage;

			foreach ((int enemyType, DPSExtremeStatList<DPSExtremeStatDictionary<int, DamageStatValue>> stat) in myPrevLocalEnemyDamageTaken) {
				myStats.myEnemyDamageTaken[enemyType][Main.LocalPlayer.whoAmI] = stat[Main.LocalPlayer.whoAmI];
			}
		}

		internal void PrintStats(string aSenderName, ListDisplayMode aStat, int aLineCount) {
			if (aLineCount <= 0)
				return;

			string bannerMessage =
				$"{aSenderName}: " +
				$"[c/ffffff:{GetTitle()}] " +
				$"[[c/ffffff:{myFormattedDuration}]] " +
				$"- " +
				$"[c/ffffff:{Language.GetText(DPSExtreme.instance.GetLocalizationKey(aStat.ToString()))}]";

			ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(bannerMessage), Color.Orange);

			//Tuple is participantIndex, damageAmount
			List<Tuple<int, int>> entries = new List<Tuple<int, int>>();

			if (aStat == ListDisplayMode.EnemyDamageTaken) { //Dictionaries
				int max = 0;
				int damageTaken = 0;
				foreach ((int enemyType, DPSExtremeStatList<DPSExtremeStatDictionary<int, DamageStatValue>> stats) in myStats.myEnemyDamageTaken) {

					stats.GetMaxAndTotal(out max, out damageTaken);
					entries.Add(new Tuple<int, int>(enemyType + 1000, damageTaken));
				}
			}
			else { //Lists

				for (int i = 0; i < 256; i++) {
					int max = 0;
					int statValue = 0;

					switch (aStat) {
						case ListDisplayMode.DamagePerSecond:
							myStats.myDamagePerSecond[i].GetMaxAndTotal(out max, out statValue);
							break;
						case ListDisplayMode.DamageDone:
							myStats.myDamageDone[i].GetMaxAndTotal(out max, out statValue);
							break;
						case ListDisplayMode.MinionDamageDone:
							myStats.myMinionDamageDone[i].GetMaxAndTotal(out max, out statValue);
							break;
						case ListDisplayMode.DamageTaken:
							myStats.myDamageTaken[i].GetMaxAndTotal(out max, out statValue);
							break;
						case ListDisplayMode.Deaths:
							myStats.myDeaths[i].GetMaxAndTotal(out max, out statValue);
							break;
						case ListDisplayMode.Kills:
							myStats.myKills[i].GetMaxAndTotal(out max, out statValue);
							break;
						case ListDisplayMode.ManaUsed:
							myStats.myManaUsed[i].GetMaxAndTotal(out max, out statValue);
							break;
						case ListDisplayMode.BuffUptime:
							myStats.myBuffUptimes[i].GetMaxAndTotal(out max, out statValue);
							break;
						case ListDisplayMode.DebuffUptime:
							myStats.myDebuffUptimes[i].GetMaxAndTotal(out max, out statValue);
							break;
						default:
							break;
					}

					if (statValue <= 0)
						continue;

					entries.Add(new Tuple<int, int>(i, statValue));
				}
			}

			entries.Sort((a, b) => -a.Item2.CompareTo(b.Item2));

			if (entries.Count == 0) {
				if (Main.netMode != NetmodeID.Server)
					Main.NewText(Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("NoDataToBroadcast")));

				return;
			}

			for (int i = 0; i < entries.Count; i++) {
				if (i >= aLineCount)
					break;

				string name = "Unknown";
				if (entries[i].Item1 >= 1000)
					name = DamageSource.GetAbilityName(entries[i].Item1 - 1000);
				else
					name = DPSExtremeStatListHelper.GetNameFromIndex(entries[i].Item1);

				string message = string.Format("{0}. {1}: [c/ffffff:{2}]", (i + 1).ToString(), name, entries[i].Item2);
				ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), Color.Orange);
			}
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