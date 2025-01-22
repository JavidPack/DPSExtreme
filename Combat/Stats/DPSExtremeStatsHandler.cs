using DPSExtreme.Config;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;

namespace DPSExtreme.Combat.Stats
{
	internal struct DamageSource
	{
		internal enum SourceType
		{
			NPC,
			NPCEnd = Projectile - 1,

			Projectile = 20000,
			ProjectileEnd = Item - 1,

			Item = 30000,
			ItemEnd = DOT - 1,

			DOT = 39000,
			DOTEnd = Traps - 1,

			Traps = 40000,
			TrapsEnd = Buffs - 1,

			Buffs = 41000,
			BuffsEnd = Other - 1,

			Other = 42000,
			OtherEnd = 43000,

			Unknown = OtherEnd + 1,
		}

		private SourceType _mySourceType = SourceType.Unknown;
		internal SourceType mySourceType {
			get => _mySourceType;
			set {
				_myDamageCauserAbility -= (int)_mySourceType; //Remove old source type addition
				_mySourceType = value;
				myDamageCauserAbility = _myDamageCauserAbility; //Add new one
			}
		}

		internal int myDamageAmount = 0;
		internal bool myIsCrit = false;

		internal int myDamageCauserId = -1;  //NPC id/Player index etc

		private int _myDamageCauserAbility = -1;
		internal int myDamageCauserAbility //Projectile/Item id etc
		{
			get {
				switch (mySourceType) {
					case SourceType.NPC:
					case SourceType.Projectile:
					case SourceType.Item:
					case SourceType.DOT:
					case SourceType.Traps:
					case SourceType.Buffs:
					case SourceType.Other:
						return _myDamageCauserAbility;
					default:
						return -1;
				}
			}
			set {
				switch (mySourceType) {
					case SourceType.NPC:
						_myDamageCauserAbility = value + (int)SourceType.Projectile; //Projectile is intended. Bit hacky for "Melee"
						break;
					case SourceType.Projectile:
						_myDamageCauserAbility = value + (int)SourceType.Projectile;
						break;
					case SourceType.Item:
						_myDamageCauserAbility = value + (int)SourceType.Item;
						break;
					case SourceType.DOT:
						_myDamageCauserAbility = value;
						break;
					case SourceType.Traps:
						_myDamageCauserAbility = value + (int)SourceType.Traps;
						break;
					case SourceType.Buffs:
						_myDamageCauserAbility = value + (int)SourceType.Buffs;
						break;
					case SourceType.Other:
						_myDamageCauserAbility = value + (int)SourceType.Other;
						break;
					default:
						_myDamageCauserAbility = -1;
						DPSExtreme.instance.DebugMessage("Invalid source");
						break;
				}
			}
		}

		public DamageSource(SourceType aSourceType) {
			mySourceType = aSourceType;
		}

		public static string GetAbilityName(int aAbilityId) {
			if (IsInSourceTypeRange(SourceType.NPC, aAbilityId))
				return Lang.GetNPCNameValue(aAbilityId);

			if (aAbilityId == (int)SourceType.Projectile) //ProjectileID.None passed. Kinda hacky, Let's see if it causes problems
				return Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("Melee"));

			if (IsInSourceTypeRange(SourceType.Projectile, aAbilityId))
				return Lang.GetProjectileName(aAbilityId - (int)SourceType.Projectile).Value;

			if (IsInSourceTypeRange(SourceType.Item, aAbilityId))
				return Lang.GetItemNameValue(aAbilityId - (int)SourceType.Item);

			if (IsInSourceTypeRange(SourceType.DOT, aAbilityId))
				return Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("DamageOverTime"));

			if (aAbilityId == (int)SourceType.Traps)
				return Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("UnknownTraps"));

			if (IsInSourceTypeRange(SourceType.Traps, aAbilityId))
				return Lang.GetItemName(aAbilityId - (int)SourceType.Traps).Value;

			if (IsInSourceTypeRange(SourceType.Buffs, aAbilityId))
				return Lang.GetBuffName(aAbilityId - (int)SourceType.Buffs);

			if (IsInSourceTypeRange(SourceType.Other, aAbilityId)) {
				if (aAbilityId == (int)SourceType.Other)
					return Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("Other"));

				int otherType = aAbilityId - (int)SourceType.Other - 1;
				switch (otherType) {
					//Loc
					case 0:
						return Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("FallDamage"));
					case 1:
						return Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("Drowning"));

					//Tile
					case 2:
						return Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("Lava"));
					case 3:
						return Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("BlockContactDamage"));
					case 4:
						return Lang._mapLegendCache[MapHelper.TileToLookup(26, WorldGen.crimson ? 1 : 0)].Value;

					//Debuffs
					case 5:
						return Lang.GetBuffName(BuffID.Stoned);
					case 7:
						return Lang.GetBuffName(BuffID.Suffocation);
					case 8:
						return Lang.GetBuffName(BuffID.Burning);
					case 9: //poison and venom debuff
						return Lang.GetBuffName(BuffID.Poisoned);
					case 10:
						return Lang.GetBuffName(BuffID.Electrified);
					case 13: //chaos state (rod of discord) 
					case 14: //if (rand(2) == 0... chaos state, male (rod of discord) 
					case 15: //if (rand(2) == 0... chaos state, female (rod of discord) 
						return Lang.GetBuffName(BuffID.ChaosState);
					case 16:
						return Lang.GetBuffName(BuffID.CursedInferno);
					case 18:
						return Lang.GetBuffName(BuffID.Starving);

					//"Other"
					case 11: //WoF insta kill from being too far away
					case 12: //tongued
					case 19: //death by entering space on remixWorld
					default:
						return Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("Other"));
				}
			}

			return $"Accessor {aAbilityId} not in any range";
		}

		public static bool IsInSourceTypeRange(SourceType aSourceType, int aAbilityId) {
			object? parseResult;
			bool parseSuccess = Enum.TryParse(typeof(SourceType), aSourceType.ToString() + "End", true, out parseResult);

			SourceType end = parseSuccess ? (SourceType)parseResult : aSourceType;

			return aAbilityId >= (int)aSourceType && aAbilityId <= (int)end;
		}
	}

	internal class DPSExtremeStatsHandler
	{
		internal void AddDealtDamage(NPC aDamagedNPC, DamageSource aDamageSource) {
			if (DPSExtremeServerConfig.Instance.IgnoreCritters)
				if (aDamagedNPC.CountsAsACritter)
					return;

			NPC realDamagedNPC = aDamagedNPC;
			if (aDamagedNPC.realLife >= 0)
				realDamagedNPC = Main.npc[aDamagedNPC.realLife];

			int npcRemainingHealth = 0;
			int npcMaxHealth = 0;
			realDamagedNPC.GetLifeStats(out npcRemainingHealth, out npcMaxHealth);

			if (Main.netMode != NetmodeID.Server) //Damage has not been applied yet on server
				npcRemainingHealth += aDamageSource.myDamageAmount; //damage has already been applied when we reach this point. But we're interested in the value pre-damage

			//Not sure why this happens. Seems like there are multiple hits in a single frame for massive overkills
			if (npcRemainingHealth < 0)
				return;

			int clampedDamageAmount = Math.Clamp(aDamageSource.myDamageAmount, 0, npcRemainingHealth); //Avoid overkill

			//Merge all penguin ids into single id etc
			int consolidatedType = NPCID.FromLegacyName(Lang.GetNPCNameValue(realDamagedNPC.type));
			int npcType = consolidatedType > 0 ? consolidatedType : realDamagedNPC.type;

			if (DPSExtreme.instance.combatTracker.myActiveCombat == null) {
				DPSExtreme.instance.DebugMessage("Adding damage without active combat");
				return;
			}

			if (Main.netMode == NetmodeID.Server &&
				aDamageSource.myDamageCauserId < (int)InfoListIndices.SupportedPlayerCount) //MP clients sync their local damage so that we can include item/proj type
			{
				return;
			}

			DamageStatValue enemyDamageTakenStat = DPSExtreme.instance.combatTracker.myActiveCombat.myStats.myEnemyDamageTaken[npcType][aDamageSource.myDamageCauserId][aDamageSource.myDamageCauserAbility];
			enemyDamageTakenStat.AddDamage(clampedDamageAmount, aDamageSource.myIsCrit);

			DamageStatValue enemyDamageTakenTotalStat = DPSExtreme.instance.combatTracker.myTotalCombat.myStats.myEnemyDamageTaken[npcType][aDamageSource.myDamageCauserId][aDamageSource.myDamageCauserAbility];
			enemyDamageTakenTotalStat.AddDamage(clampedDamageAmount, aDamageSource.myIsCrit);

			if (aDamageSource.mySourceType == DamageSource.SourceType.Item) {
				var proectileItemSource = ContentSamples.ItemsByType[aDamageSource.myDamageCauserAbility - (int)DamageSource.SourceType.Item];
				if (proectileItemSource != null) {
					int projectileTypeId = proectileItemSource.shoot;
					if (ContentSamples.ProjectilesByType[projectileTypeId].minion) {
						MinionDamageStatValue minionDamageDealtStat = DPSExtreme.instance.combatTracker.myActiveCombat.myStats.myMinionDamageDone[aDamageSource.myDamageCauserId][aDamageSource.myDamageCauserAbility];
						minionDamageDealtStat.AddDamage(clampedDamageAmount, aDamageSource.myIsCrit);
						minionDamageDealtStat.myMinionType = projectileTypeId;
						minionDamageDealtStat.myMinionOwner = aDamageSource.myDamageCauserId;

						MinionDamageStatValue minionTotalDamageDealtStat = DPSExtreme.instance.combatTracker.myTotalCombat.myStats.myMinionDamageDone[aDamageSource.myDamageCauserId][aDamageSource.myDamageCauserAbility];
						minionTotalDamageDealtStat.AddDamage(clampedDamageAmount, aDamageSource.myIsCrit);
						minionTotalDamageDealtStat.myMinionType = projectileTypeId;
						minionTotalDamageDealtStat.myMinionOwner = aDamageSource.myDamageCauserId;
					}
				}
			}

			DamageStatValue damageDoneStat = DPSExtreme.instance.combatTracker.myActiveCombat.myStats.myDamageDone[aDamageSource.myDamageCauserId][aDamageSource.myDamageCauserAbility];
			damageDoneStat.AddDamage(clampedDamageAmount, aDamageSource.myIsCrit);

			DamageStatValue damageDoneTotalStat = DPSExtreme.instance.combatTracker.myTotalCombat.myStats.myDamageDone[aDamageSource.myDamageCauserId][aDamageSource.myDamageCauserAbility];
			damageDoneTotalStat.AddDamage(clampedDamageAmount, aDamageSource.myIsCrit);
		}

		internal void AddDamageTaken(Player aDamagedPlayer, DamageSource aDamageSource) {
			if (aDamagedPlayer.statLife <= 0)
				return;

			if (DPSExtreme.instance.combatTracker.myActiveCombat == null) {
				DPSExtreme.instance.DebugMessage("Adding damage taken without active combat");
				return;
			}

			int clampedDamageAmount = Math.Clamp(aDamageSource.myDamageAmount, 0, aDamagedPlayer.statLife); //Avoid overkill

			DamageStatValue stat = DPSExtreme.instance.combatTracker.myActiveCombat.myStats.myDamageTaken[aDamagedPlayer.whoAmI][aDamageSource.myDamageCauserId][aDamageSource.myDamageCauserAbility];
			stat.AddDamage(clampedDamageAmount, aDamageSource.myIsCrit);

			DamageStatValue totalStat = DPSExtreme.instance.combatTracker.myTotalCombat.myStats.myDamageTaken[aDamagedPlayer.whoAmI][aDamageSource.myDamageCauserId][aDamageSource.myDamageCauserAbility];
			totalStat.AddDamage(clampedDamageAmount, aDamageSource.myIsCrit);
		}

		internal void AddDeath(Player aKilledPlayer) {
			if (DPSExtreme.instance.combatTracker.myActiveCombat == null) {
				DPSExtreme.instance.DebugMessage("Adding death without active combat");
				return;
			}

			DPSExtreme.instance.combatTracker.myActiveCombat.myStats.myDeaths[aKilledPlayer.whoAmI].AddDeath(DPSExtreme.instance.combatTracker.myActiveCombat.myDurationInTicks);
			DPSExtreme.instance.combatTracker.myTotalCombat.myStats.myDeaths[aKilledPlayer.whoAmI].AddDeath(DPSExtreme.instance.combatTracker.myTotalCombat.myDurationInTicks);
		}

		internal void AddKill(NPC aKilledNPC, int aKiller) {
			if (DPSExtreme.instance.combatTracker.myActiveCombat == null) {
				DPSExtreme.instance.DebugMessage("Adding kill without active combat");
				return;
			}

			if (DPSExtremeServerConfig.Instance.IgnoreCritters)
				if (aKilledNPC.CountsAsACritter)
					return;

			DPSExtreme.instance.combatTracker.myActiveCombat.myStats.myKills[aKiller][aKilledNPC.type] += 1;
			DPSExtreme.instance.combatTracker.myTotalCombat.myStats.myKills[aKiller][aKilledNPC.type] += 1;
		}

		internal void AddConsumedMana(Player aPlayer, Item aUsedItem, int aManaAmount) {
			if (DPSExtreme.instance.combatTracker.myActiveCombat == null) {
				DPSExtreme.instance.DebugMessage("Adding mana used without active combat");
				return;
			}

			DPSExtreme.instance.combatTracker.myActiveCombat.myStats.myManaUsed[aPlayer.whoAmI][aUsedItem.type + (int)DamageSource.SourceType.Item] += aManaAmount;
			DPSExtreme.instance.combatTracker.myTotalCombat.myStats.myManaUsed[aPlayer.whoAmI][aUsedItem.type + (int)DamageSource.SourceType.Item] += aManaAmount;
		}

		internal void AddBuffUptime(Player aPlayer, int aBuffType) {
			if (DPSExtreme.instance.combatTracker.myActiveCombat == null) {
				DPSExtreme.instance.DebugMessage("Adding buff uptime without active combat");
				return;
			}

			DPSExtreme.instance.combatTracker.myActiveCombat.myStats.myBuffUptimes[aPlayer.whoAmI][aBuffType + (int)DamageSource.SourceType.Buffs] += 1;
			DPSExtreme.instance.combatTracker.myTotalCombat.myStats.myBuffUptimes[aPlayer.whoAmI][aBuffType + (int)DamageSource.SourceType.Buffs] += 1;
		}

		internal void AddDebuffUptime(Player aPlayer, int aDebuffType) {
			if (DPSExtreme.instance.combatTracker.myActiveCombat == null) {
				DPSExtreme.instance.DebugMessage("Adding debuff uptime without active combat");
				return;
			}

			DPSExtreme.instance.combatTracker.myActiveCombat.myStats.myDebuffUptimes[aPlayer.whoAmI][aDebuffType + (int)DamageSource.SourceType.Buffs] += 1;
			DPSExtreme.instance.combatTracker.myTotalCombat.myStats.myDebuffUptimes[aPlayer.whoAmI][aDebuffType + (int)DamageSource.SourceType.Buffs] += 1;
		}
	}
}