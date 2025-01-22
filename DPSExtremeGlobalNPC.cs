using DPSExtreme.Combat.Stats;
using DPSExtreme.Config;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static DPSExtreme.Combat.DPSExtremeCombat;

namespace DPSExtreme
{
	internal class DPSExtremeGlobalNPC : GlobalNPC
	{
		//Not needed anymore as we don't have instanced data?
		//public override bool InstancePerEntity => true;

		public DPSExtremeGlobalNPC() {
		}

		public override void Load() {
			Terraria.IL_NPC.UpdateNPC_BuffApplyDOTs += IL_NPC_UpdateNPC_BuffApplyDOTs;
		}

		private void IL_NPC_UpdateNPC_BuffApplyDOTs(MonoMod.Cil.ILContext il) {
			// with realLife, worm npc take more damage. Eater of worlds doesn't use realLife, each takes damage individually. Eventually need to account for this?

			var c = new ILCursor(il);

			c.GotoNext( // I guess before CombatText.NewText would also work...
				MoveType.After,
				i => i.MatchLdsfld<Main>(nameof(Main.npc)),
				i => i.MatchLdloc(18),
				i => i.MatchLdelemRef(),
				i => i.MatchDup(),
				i => i.MatchLdfld(typeof(NPC), nameof(NPC.life)),
				i => i.MatchLdloc(0),
				i => i.MatchSub(),
				i => i.MatchStfld(typeof(NPC), nameof(NPC.life))
			);
			c.Emit(Mono.Cecil.Cil.OpCodes.Ldloc_S, (byte)18);
			c.Emit(Mono.Cecil.Cil.OpCodes.Ldloc_0);
			c.EmitDelegate<Action<int, int>>((int whoAmI, int damage) => {
				if (Main.netMode != NetmodeID.Server)
					return;

				// whoAmI already accounts for realLife

				NPC npc = Main.npc[whoAmI];
				//TODO Verify that damage has already been applied when we reach this point (otherwise overkill calculation is incorrect)

				DamageSource damageSource = new DamageSource(DamageSource.SourceType.DOT);
				damageSource.myDamageAmount = damage;
				damageSource.myDamageCauserAbility = (int)InfoListIndices.DOTs;
				damageSource.myDamageCauserId = (int)InfoListIndices.DOTs;

				DPSExtreme.instance.combatTracker.TriggerCombat(CombatType.Generic);
				DPSExtreme.instance.combatTracker.myStatsHandler.AddDealtDamage(npc, damageSource);

				//Main.NewText($"Detected DOT: {Main.npc[whoAmI].FullName}, {damage}");
			});

			c.GotoNext(
				MoveType.After,
				i => i.MatchLdsfld<Main>(nameof(Main.npc)),
				i => i.MatchLdloc(19),
				i => i.MatchLdelemRef(),
				i => i.MatchDup(),
				i => i.MatchLdfld(typeof(NPC), nameof(NPC.life)),
				i => i.MatchLdcI4(1),
				i => i.MatchSub(),
				i => i.MatchStfld(typeof(NPC), nameof(NPC.life))
			);
			c.Emit(Mono.Cecil.Cil.OpCodes.Ldloc_S, (byte)19);
			c.Emit(Mono.Cecil.Cil.OpCodes.Ldc_I4_1);
			c.EmitDelegate<Action<int, int>>((int whoAmI, int damage) => {
				if (Main.netMode != NetmodeID.Server)
					return;

				// whoAmI already accounts for realLife
				NPC npc = Main.npc[whoAmI];

				DamageSource damageSource = new DamageSource(DamageSource.SourceType.DOT);
				damageSource.myDamageAmount = damage;
				damageSource.myDamageCauserAbility = (int)InfoListIndices.DOTs;
				damageSource.myDamageCauserId = (int)InfoListIndices.DOTs;

				DPSExtreme.instance.combatTracker.TriggerCombat(CombatType.Generic);
				DPSExtreme.instance.combatTracker.myStatsHandler.AddDealtDamage(npc, damageSource);

				//Main.NewText($"Detected DOT: {Main.npc[whoAmI].FullName}, {damage}");
			});
		}

		public override void OnSpawn(NPC npc, IEntitySource source) {
			if (npc.boss) {
				DPSExtreme.instance.combatTracker.TriggerCombat(CombatType.BossFight, npc.type);
			}
		}

		public override void OnKill(NPC npc) {
			try {
				if (Main.netMode == NetmodeID.MultiplayerClient)
					return;

				if (DPSExtremeServerConfig.Instance.IgnoreCritters)
					if (npc.CountsAsACritter)
						return;

				if (npc.friendly)
					return;

				DPSExtreme.instance.combatTracker.TriggerCombat(CombatType.Generic);
				DPSExtreme.instance.combatTracker.myStatsHandler.AddKill(npc, npc.lastInteraction);
			}
			catch (Exception) {
				//ErrorLogger.Log("NPCLoot" + e.Message);
			}
		}

		// Things like townNPC and I think traps will trigger this in Server. In SP, all is done here.
		public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone) {
			try {
				if (npc.friendly)
					return;

				if (DPSExtremeServerConfig.Instance.IgnoreCritters)
					if (npc.CountsAsACritter)
						return;

				DamageSource damageSource = new DamageSource(DamageSource.SourceType.Item);
				damageSource.myDamageAmount = damageDone;
				damageSource.myIsCrit = hit.Crit;
				damageSource.myDamageCauserAbility = item.type;
				damageSource.myDamageCauserId = player.whoAmI;

				DPSExtreme.instance.combatTracker.TriggerCombat(CombatType.Generic);
				DPSExtreme.instance.combatTracker.myStatsHandler.AddDealtDamage(npc, damageSource);
			}
			catch (Exception) {
				//ErrorLogger.Log("OnHitByItem" + e.Message);
			}
		}

		// Things like townNPC and I think traps will trigger this in Server. In SP, all is done here.
		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone) {
			//TODO, owner could be -1?
			try {
				if (npc.friendly)
					return;

				if (DPSExtremeServerConfig.Instance.IgnoreCritters)
					if (npc.CountsAsACritter)
						return;

				int projectileOwner = projectile.owner;

				DPSExtremeModProjectile dpsProjectile = projectile.GetGlobalProjectile<DPSExtremeModProjectile>();

				if (dpsProjectile.whoIsMyParent == (int)InfoListIndices.NPCs)
					projectileOwner = (int)InfoListIndices.NPCs;
				else if (dpsProjectile.whoIsMyParent == (int)InfoListIndices.Traps)
					projectileOwner = (int)InfoListIndices.Traps;

				DamageSource damageSource = new DamageSource(DamageSource.SourceType.Projectile);

				if (dpsProjectile.myParentItemType != -1)
					damageSource.mySourceType = DamageSource.SourceType.Item;

				damageSource.myDamageAmount = damageDone;
				damageSource.myIsCrit = hit.Crit;
				damageSource.myDamageCauserAbility = dpsProjectile.myParentItemType != -1 ? dpsProjectile.myParentItemType : projectile.type;
				damageSource.myDamageCauserId = projectileOwner;

				DPSExtreme.instance.combatTracker.TriggerCombat(CombatType.Generic);
				DPSExtreme.instance.combatTracker.myStatsHandler.AddDealtDamage(npc, damageSource);
			}
			catch (Exception) {
				//ErrorLogger.Log("OnHitByProjectile" + e.Message);
			}
		}
	}
}