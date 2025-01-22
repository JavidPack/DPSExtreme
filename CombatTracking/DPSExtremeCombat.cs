using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;

namespace DPSExtreme.CombatTracking
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
			Count
		}

		internal enum EventType
		{
			BloodMoon = InvasionType.Count,
			Eclipse,
			SlimeRain,
			//No support for modded events
		}

		internal CombatType myCombatTypeFlags;
		internal CombatType myHighestCombatType;
		internal int myBossOrInvasionOrEventType = -1;

		internal DateTime myStartTime;
		internal DateTime myLastActivityTime;

		public Dictionary<int, DPSExtremeInfoList<DamageDealtInfo>> myDamageDealtPerNPCType = new Dictionary<int, DPSExtremeInfoList<DamageDealtInfo>>();

		internal DPSExtremeInfoList<DamageDealtInfo> myTotalDamageDealtList = new DPSExtremeInfoList<DamageDealtInfo>();
		internal DPSExtremeInfoList<DamageDealtInfo> myDPSList = new DPSExtremeInfoList<DamageDealtInfo>();

		public DPSExtremeCombat(CombatType aCombatType, int aBossOrInvasionOrEventType) {
			myCombatTypeFlags = aCombatType;
			myHighestCombatType = aCombatType;
			myBossOrInvasionOrEventType = aBossOrInvasionOrEventType;
			myStartTime = DateTime.Now;
			myLastActivityTime = myStartTime;
		}

		internal void AddDealtDamage(NPC aDamagedNPC, int aDamageDealer, int aDamage) {
			int npcRemainingHealth = 0;
			int npcMaxHealth = 0;
			aDamagedNPC.GetLifeStats(out npcRemainingHealth, out npcMaxHealth);
			npcRemainingHealth += aDamage; //damage has already been applied when we reach this point. But we're interested in the value pre-damage

			//Not sure why this happens. Seems like there are multiple hits in a single frame for massive overkills
			if (npcRemainingHealth < 0)
				return;

			int clampedDamageAmount = Math.Clamp(aDamage, 0, npcRemainingHealth); //Avoid overkill

			//Merge all penguin ids into single id etc
			int consolidatedType = NPCID.FromLegacyName(Lang.GetNPCNameValue(aDamagedNPC.type));
			int npcType = consolidatedType > 0 ? consolidatedType : aDamagedNPC.type;

			if (!myDamageDealtPerNPCType.ContainsKey(npcType))
				myDamageDealtPerNPCType.Add(npcType, new DPSExtremeInfoList<DamageDealtInfo>());

			myDamageDealtPerNPCType[npcType][aDamageDealer].myDamage += clampedDamageAmount;
			myTotalDamageDealtList[aDamageDealer].myDamage += clampedDamageAmount;
		}

		internal void OnPlayerLeft(int aPlayer) {
			//Move player's stats into designated part of the buffer for disconnected players
			for (int i = (int)InfoListIndices.DisconnectedPlayersStart; i < (int)InfoListIndices.DisconnectedPlayersEnd; i++) {
				if (myTotalDamageDealtList[i].myDamage > -1)
					continue;

				ReassignStats(aPlayer, i);
				break;
			}
		}

		internal void ReassignStats(int aFrom, int aTo) {
			myTotalDamageDealtList[aTo] = myTotalDamageDealtList[aFrom];

			foreach ((int npcType, DPSExtremeInfoList<DamageDealtInfo> damageInfo) in myDamageDealtPerNPCType) {
				myDamageDealtPerNPCType[npcType][aTo] = myDamageDealtPerNPCType[npcType][aFrom];
			}

			myDPSList[aTo] = myDPSList[aFrom];

			ClearStatsForPlayer(aFrom);
		}

		internal void ClearStatsForPlayer(int aPlayer) {
			myTotalDamageDealtList[aPlayer] = new DamageDealtInfo();

			foreach ((int npcType, DPSExtremeInfoList<DamageDealtInfo> damageInfo) in myDamageDealtPerNPCType)
				myDamageDealtPerNPCType[npcType][aPlayer] = new DamageDealtInfo();

			myDPSList[aPlayer] = new DamageDealtInfo();
		}

		internal void SendStats() {
			try {
				if (Main.netMode != NetmodeID.Server)
					return;

				ProtocolPushCombatStats push = new ProtocolPushCombatStats();
				push.myCombatIsActive = true;
				push.myDamageDealtPerNPCType = myDamageDealtPerNPCType;
				push.myTotalDamageDealtList = myTotalDamageDealtList;

				DPSExtreme.instance.packetHandler.SendProtocol(push);

				if (Main.netMode == NetmodeID.Server) {
					Dictionary<byte, int> stats = new Dictionary<byte, int>();
					for (int i = 0; i < 256; i++) {
						if (myTotalDamageDealtList[i].myDamage > -1) {
							stats[(byte)i] = myTotalDamageDealtList[i].myDamage;
						}
					}

					DPSExtreme.instance.InvokeOnSimpleBossStats(stats);
				}

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
				DamageDealtInfo damageInfo = myTotalDamageDealtList[i];

				if (damageInfo.myDamage > 0) {
					if (i == (int)InfoListIndices.NPCs) {
						sb.Append(string.Format("{0}: {1}, ", Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("TrapsTownNPC")), damageInfo.myDamage));
					}
					if (i == (int)InfoListIndices.DOTs) {
						sb.Append(string.Format("{0}: {1}, ", Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("DamageOverTime")), damageInfo.myDamage));
					}
					else {
						sb.Append(string.Format("{0}: {1}, ", Main.player[i].name, damageInfo.myDamage));
					}
				}
			}

			if (sb.Length > 2)
				sb.Length -= 2; // removes last ,

			Color messageColor = Color.Orange;

			if (Main.netMode == NetmodeID.Server) {
				ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(sb.ToString()), messageColor);
			}
			else if (Main.netMode == NetmodeID.SinglePlayer) {
				Main.NewText(sb.ToString(), messageColor);
			}
		}
	}
}
