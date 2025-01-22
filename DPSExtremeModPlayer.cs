using DPSExtreme.Combat.Stats;
using DPSExtreme.Config;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using static DPSExtreme.Combat.DPSExtremeCombat;

namespace DPSExtreme
{
	internal class DPSExtremeModPlayer : ModPlayer
	{
		internal static List<int> ourConnectedPlayers = new List<int>();
		public override void PlayerDisconnect() {
			if (Main.netMode != NetmodeID.Server)
				return;

			foreach (int playerIndex in ourConnectedPlayers) {
				if (Main.player[playerIndex].active)
					continue;

				ourConnectedPlayers.Remove(playerIndex);
				DPSExtreme.instance?.combatTracker.OnPlayerLeft(playerIndex);

				break;
			}
		}

		public override void PostUpdate() {
			if (Main.GameUpdateCount % DPSExtreme.UPDATEDELAY != 0)
				return;

			if (DPSExtreme.instance.combatTracker.myActiveCombat == null)
				return;

			if (Player.whoAmI != Main.myPlayer)
				return;

			if (!Player.accDreamCatcher)
				return;

			int dps = Player.getDPS();
			if (!Player.dpsStarted)
				dps = 0;

			CombatStats stats = DPSExtreme.instance.combatTracker.myActiveCombat.myStats;

			ProtocolReqShareCurrentDPS req = new ProtocolReqShareCurrentDPS();
			req.myPlayer = Player.whoAmI;
			req.myDPS = dps;
			req.myDamageDoneBreakdown = stats.myDamageDone[Player.whoAmI];
			req.myMinionDamageDoneBreakdown = stats.myMinionDamageDone[Player.whoAmI];

			foreach ((int enemyType, DPSExtremeStatList<DPSExtremeStatDictionary<int, DamageStatValue>> stat) in stats.myEnemyDamageTaken) {
				req.myEnemyDamageTakenByMeBreakdown[enemyType] = stat[Player.whoAmI];
			}

			DPSExtreme.instance.packetHandler.SendProtocol(req);
		}

		public override void OnEnterWorld() {
			DPSExtreme.instance.combatTracker.OnEnterWorld();
			DPSExtremeUI.instance.OnEnterWorld();
		}

		public override void OnHurt(Player.HurtInfo aHurtInfo) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			DamageSource damageSource = new DamageSource();
			if (aHurtInfo.DamageSource.SourceOtherIndex != -1) {
				damageSource.mySourceType = DamageSource.SourceType.Other;
				damageSource.myDamageCauserId = (int)DamageSource.SourceType.Other;
				damageSource.myDamageCauserAbility = aHurtInfo.DamageSource.SourceOtherIndex + 1; //+1 since we want the 0 index to be "Other", but fall damage is already assigned to 0
			}
			else if (aHurtInfo.DamageSource.SourceNPCIndex != -1) {
				damageSource.mySourceType = DamageSource.SourceType.NPC;
				damageSource.myDamageCauserId = Main.npc[aHurtInfo.DamageSource.SourceNPCIndex].type;
				damageSource.myDamageCauserAbility = ProjectileID.None;
			}
			else if (aHurtInfo.DamageSource.SourceProjectileType != -1) {
				damageSource.mySourceType = DamageSource.SourceType.Projectile;

				Projectile projectile = Main.projectile[aHurtInfo.DamageSource.SourceProjectileLocalIndex];
				DPSExtremeModProjectile dpsProjectile = projectile.GetGlobalProjectile<DPSExtremeModProjectile>();
				int owner = dpsProjectile.whoIsMyParent;

				if (owner == -1) {
					DPSExtreme.instance.DebugMessage($"No owner found for projectile of type: {projectile.type} and local index {aHurtInfo.DamageSource.SourceProjectileLocalIndex}");
					return;
				}

				if (owner == (int)InfoListIndices.Traps) {
					damageSource.myDamageCauserId = dpsProjectile.myParentItemType + (int)DamageSource.SourceType.Traps;
					damageSource.myDamageCauserAbility = projectile.type;
				}
				else {
					NPC parentNPC = Main.npc[owner];

					if (!parentNPC.active) {
						DPSExtreme.instance.DebugMessage("Projectile owner npc is not active");
						return;
					}

					damageSource.myDamageCauserId = parentNPC.type;
					damageSource.myDamageCauserAbility = projectile.type;
				}
			}

			damageSource.myIsCrit = false;
			damageSource.myDamageAmount = aHurtInfo.Damage;

			DPSExtreme.instance.combatTracker.TriggerCombat(CombatType.Generic);
			DPSExtreme.instance.combatTracker.myStatsHandler.AddDamageTaken(Player, damageSource);
		}

		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			DPSExtreme.instance.combatTracker.TriggerCombat(CombatType.Generic);
			DPSExtreme.instance.combatTracker.myStatsHandler.AddDeath(Player);
		}

		public override void OnConsumeMana(Item item, int manaConsumed) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			if (DPSExtreme.instance.combatTracker.myActiveCombat == null)
				return;

			DPSExtreme.instance.combatTracker.myStatsHandler.AddConsumedMana(Player, item, manaConsumed);
		}

		public override void PostUpdateBuffs() {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			if (DPSExtreme.instance.combatTracker.myActiveCombat == null)
				return;

			for (int i = 0; i < Player.MaxBuffs; i++) {
				if (Player.buffType[i] == 0)
					continue;

				bool isDebuff = Main.debuff[Player.buffType[i]];
				bool isPermanent = Player.buffTime[i] == 1;
				bool isMinion = Player.buffTime[i] == 18000;

				if (isPermanent && DPSExtremeServerConfig.Instance.IgnorePermanentBuffs)
					continue;

				if (isMinion && DPSExtremeServerConfig.Instance.IgnoreMinionBuffs)
					continue;

				switch (Player.buffType[i]) {
					case BuffID.Werewolf:
					case BuffID.Merfolk:
					case BuffID.PaladinsShield:
					case BuffID.Honey:
					case BuffID.LeafCrystal:
					case BuffID.IceBarrier:
					case BuffID.Campfire:
					case BuffID.HeartLamp:
					case BuffID.BeetleMight1:
					case BuffID.BeetleMight2:
					case BuffID.BeetleMight3:
					case BuffID.Sunflower:
					case BuffID.MonsterBanner:
					case BuffID.StarInBottle:
					case BuffID.Sharpened:
					case BuffID.DryadsWard:
					case BuffID.SolarShield1:
					case BuffID.SolarShield2:
					case BuffID.SolarShield3:
					case BuffID.NebulaUpLife1:
					case BuffID.NebulaUpLife2:
					case BuffID.NebulaUpLife3:
					case BuffID.NebulaUpMana1:
					case BuffID.NebulaUpMana2:
					case BuffID.NebulaUpMana3:
					case BuffID.NebulaUpDmg1:
					case BuffID.NebulaUpDmg2:
					case BuffID.NebulaUpDmg3:
					case BuffID.SugarRush:
					case BuffID.ParryDamageBuff:
					case BuffID.Lucky:
					case BuffID.TitaniumStorm:
						isDebuff = false;
						break;
					default:
						break;
				}

				if (isDebuff)
					DPSExtreme.instance.combatTracker.myStatsHandler.AddDebuffUptime(Player, Player.buffType[i]);
				else
					DPSExtreme.instance.combatTracker.myStatsHandler.AddBuffUptime(Player, Player.buffType[i]);
			}
		}

		public override void ProcessTriggers(TriggersSet triggersSet) {
			if (DPSExtreme.instance.ToggleTeamDPSHotKey.JustPressed) {
				DPSExtremeUI.instance.ShowTeamDPSPanel = !DPSExtremeUI.instance.ShowTeamDPSPanel;
			}
		}
	}
}