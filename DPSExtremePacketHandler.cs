using DPSExtreme.Combat;
using DPSExtreme.Combat.Stats;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static DPSExtreme.Combat.DPSExtremeCombat;

namespace DPSExtreme
{
	internal class DPSExtremePacketHandler
	{
		//Allows us to unify SP & MP code flows into a single function call
		public void SendProtocol(DPSExtremeProtocol aProtocol, int aTargetClient = -1) {
			if (Main.netMode == NetmodeID.SinglePlayer) {
				HandleProtocol(aProtocol.GetDelimiter(), aProtocol);
			}
			else {
				ModPacket netMessage = DPSExtreme.instance.GetPacket();
				aProtocol.ToStream(netMessage);
				netMessage.Send(aTargetClient);
			}
		}

		public bool HandlePacket(BinaryReader reader, int whoAmI) {
			DPSExtremeMessageType delimiter = (DPSExtremeMessageType)reader.ReadByte();

			DPSExtremeProtocol protocol = null;

			switch (delimiter) {
				case DPSExtremeMessageType.StartCombatPush: {
						protocol = new ProtocolPushStartCombat();
						if (!protocol.FromStream(reader))
							return false;

						break;
					}
				case DPSExtremeMessageType.UpgradeCombatPush: {
						protocol = new ProtocolPushUpgradeCombat();
						if (!protocol.FromStream(reader))
							return false;

						break;
					}
				case DPSExtremeMessageType.EndCombatPush: {
						protocol = new ProtocolPushEndCombat();
						if (!protocol.FromStream(reader))
							return false;

						break;
					}
				case DPSExtremeMessageType.ShareCurrentDPSReq: {
						protocol = new ProtocolReqShareCurrentDPS();
						if (!protocol.FromStream(reader))
							return false;

						break;
					}
				case DPSExtremeMessageType.CurrentDPSsPush: {
						protocol = new ProtocolPushClientDPSs();
						if (!protocol.FromStream(reader))
							return false;

						break;
					}
				case DPSExtremeMessageType.CurrentCombatTotalsPush: {
						protocol = new ProtocolPushCombatStats();
						if (!protocol.FromStream(reader))
							return false;

						break;
					}
				default:
					DPSExtreme.instance.Logger.Warn("DPSExtreme: Unknown Message type: " + delimiter);
					break;
			}

			if (protocol == null) {
				DPSExtreme.instance.DebugMessage("null protocol for message type: " + delimiter.ToString());
			}
			else {
				HandleProtocol(delimiter, protocol);
			}

			return true;
		}

		public bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber) {
			try {
				if (Main.netMode != NetmodeID.Server)
					return false;

				if (messageType == MessageID.PlayerSpawn) {
					if (Netplay.Clients[playerNumber].State == 3) //Only handle it when player is joining. Not on respawns etc
						DPSExtreme.instance.combatTracker.myJoiningPlayers.Add(playerNumber);
				}

				if (messageType == MessageID.DamageNPC) {
					int npcIndex = reader.ReadInt16();
					int damage = reader.Read7BitEncodedInt();
					if (damage < 0)
						return false;
					//ErrorLogger.Log("HijackGetData StrikeNPC: " + npcIndex + " " + damage + " " + playerNumber);

					//System.Console.WriteLine("HijackGetData StrikeNPC: " + npcIndex + " " + damage + " " + playerNumber);
					NPC damagedNPC = Main.npc[npcIndex];
					if (damagedNPC.realLife >= 0) {
						damagedNPC = Main.npc[damagedNPC.realLife];
					}

					DamageSource damageSource = new DamageSource(DamageSource.SourceType.DOT);
					damageSource.myDamageAmount = damage;
					damageSource.myDamageCauserId = playerNumber;
					damageSource.myDamageCauserAbility = -1; //Unknown what item/projectile it is. Clients will pass this info themselves

					DPSExtreme.instance.combatTracker.TriggerCombat(CombatType.Generic);
					DPSExtreme.instance.combatTracker.myStatsHandler.AddDealtDamage(damagedNPC, damageSource);

					// TODO: Reimplement DPS with ring buffer for accurate?  !!! or send 0?
					// TODO: Verify real life adjustment
				}
			}
			catch (Exception) {
				//ErrorLogger.Log("HijackGetData StrikeNPC " + e.Message);
			}
			return false;
		}

		private void HandleProtocol(DPSExtremeMessageType aDelimiter, DPSExtremeProtocol aProtocol) {
			switch (aDelimiter) {
				case DPSExtremeMessageType.StartCombatPush:
					HandleStartCombatPush(aProtocol as ProtocolPushStartCombat);
					break;
				case DPSExtremeMessageType.UpgradeCombatPush:
					HandleUpgradeCombatPush(aProtocol as ProtocolPushUpgradeCombat);
					break;
				case DPSExtremeMessageType.EndCombatPush:
					HandleEndCombatPush(aProtocol as ProtocolPushEndCombat);
					break;
				case DPSExtremeMessageType.ShareCurrentDPSReq:
					HandleInformServerDPSReq(aProtocol as ProtocolReqShareCurrentDPS);
					break;
				case DPSExtremeMessageType.CurrentDPSsPush:
					HandleClientDPSsPush(aProtocol as ProtocolPushClientDPSs);
					break;
				case DPSExtremeMessageType.CurrentCombatTotalsPush:
					HandleCombatStatsPush(aProtocol as ProtocolPushCombatStats);
					break;
				default:
					DPSExtreme.instance.Logger.Warn("DPSExtreme: Unknown Message type: " + aDelimiter);
					break;
			}
		}

		public void HandleStartCombatPush(ProtocolPushStartCombat aPush) {
			DPSExtreme.instance.combatTracker.StartCombat(aPush.myCombatType, aPush.myBossOrInvasionOrEventType);
		}

		public void HandleUpgradeCombatPush(ProtocolPushUpgradeCombat aPush) {
			DPSExtreme.instance.combatTracker.UpgradeCombat(aPush.myCombatType, aPush.myBossOrInvasionOrEventType);
		}

		public void HandleEndCombatPush(ProtocolPushEndCombat aPush) {
			DPSExtreme.instance.combatTracker.EndCombat(aPush.myCombatType);
		}

		public void HandleInformServerDPSReq(ProtocolReqShareCurrentDPS aReq) {
			DPSExtremeCombat activeCombat = DPSExtreme.instance.combatTracker.myActiveCombat;
			if (activeCombat == null)
				return;

			activeCombat.myStats.myDamagePerSecond[aReq.myPlayer] = aReq.myDPS;
			activeCombat.myStats.myDamageDone[aReq.myPlayer] = aReq.myDamageDoneBreakdown;
			activeCombat.myStats.myMinionDamageDone[aReq.myPlayer] = aReq.myMinionDamageDoneBreakdown;

			foreach ((int enemyType, DPSExtremeStatDictionary<int, DamageStatValue> stat) in aReq.myEnemyDamageTakenByMeBreakdown) {
				activeCombat.myStats.myEnemyDamageTaken[enemyType][aReq.myPlayer] = stat;
			}
		}

		public void HandleClientDPSsPush(ProtocolPushClientDPSs aPush) {
			if (DPSExtreme.instance.combatTracker.myActiveCombat == null)
				return;

			DPSExtreme.instance.combatTracker.myActiveCombat.myStats.myDamagePerSecond = aPush.myDamagePerSecond;

			DPSExtremeUI.instance.updateNeeded = true;
		}

		public void HandleCombatStatsPush(ProtocolPushCombatStats aPush) {
			if (DPSExtreme.instance.combatTracker.myActiveCombat == null)
				return;

			{
				DPSExtremeCombat activeCombat = DPSExtreme.instance.combatTracker.myActiveCombat;
				activeCombat.myDurationInTicks = aPush.myActiveCombatDurationInTicks;

				var myPrevLocalDamage = activeCombat.myStats.myDamageDone[Main.LocalPlayer.whoAmI];
				var myPrevLocalMinionDamage = activeCombat.myStats.myMinionDamageDone[Main.LocalPlayer.whoAmI];
				var myPrevLocalEnemyDamageTaken = activeCombat.myStats.myEnemyDamageTaken;
				activeCombat.myStats = aPush.myStats;
				//Sync remote player damage, but don't overwrite local
				activeCombat.myStats.myDamageDone[Main.LocalPlayer.whoAmI] = myPrevLocalDamage;
				activeCombat.myStats.myMinionDamageDone[Main.LocalPlayer.whoAmI] = myPrevLocalMinionDamage;

				foreach ((int enemyType, DPSExtremeStatList<DPSExtremeStatDictionary<int, DamageStatValue>> stat) in myPrevLocalEnemyDamageTaken) {
					activeCombat.myStats.myEnemyDamageTaken[enemyType][Main.LocalPlayer.whoAmI] = stat[Main.LocalPlayer.whoAmI];
				}
			}
			{
				DPSExtremeCombat totalCombat = DPSExtreme.instance.combatTracker.myTotalCombat;
				totalCombat.myDurationInTicks = aPush.myTotalCombatDurationInTicks;

				var myPrevLocalTotalDamage = totalCombat.myStats.myDamageDone[Main.LocalPlayer.whoAmI];
				var myPrevLocalMinionTotalDamage = totalCombat.myStats.myMinionDamageDone[Main.LocalPlayer.whoAmI];
				totalCombat.myStats = aPush.myTotalStats;
				//Sync remote total player damage, but don't overwrite local
				totalCombat.myStats.myDamageDone[Main.LocalPlayer.whoAmI] = myPrevLocalTotalDamage;
				totalCombat.myStats.myMinionDamageDone[Main.LocalPlayer.whoAmI] = myPrevLocalMinionTotalDamage;
			}

			DPSExtremeUI.instance.updateNeeded = true;

			//if (!aPush.myCombatIsActive)
			//{
			//    Dictionary<byte, int> stats = new Dictionary<byte, int>();
			//    for (int i = 0; i < 256; i++)
			//    {
			//        if (DPSExtreme.bossDamage[i] > -1)
			//        {
			//            stats[(byte)i] = DPSExtreme.bossDamage[i];
			//        }
			//    }
			//    DPSExtreme.instance.OnSimpleBossStats?.Invoke(stats);
			//}
		}
	}
}