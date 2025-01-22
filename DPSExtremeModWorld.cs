using DPSExtreme.Combat.Stats;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DPSExtreme
{
	internal class DPSExtremeModWorld : ModSystem
	{
		public override void PostUpdateWorld() {
			DPSExtreme.instance.combatTracker.UpdateCombatDuration();

			if ((Main.GameUpdateCount % DPSExtreme.UPDATEDELAY) != 0)
				return;

			DPSExtreme.instance.combatTracker.Update();

			if (DPSExtreme.instance.combatTracker.myActiveCombat == null)
				return;

			UpdateDOTDPS();
			UpdateBroadcasts();
		}

		void UpdateBroadcasts() {
			if (Main.netMode != NetmodeID.SinglePlayer && Main.netMode != NetmodeID.Server)
				return;

			ProtocolPushClientDPSs push = new ProtocolPushClientDPSs();
			push.myDamagePerSecond = DPSExtreme.instance.combatTracker.myActiveCombat.myStats.myDamagePerSecond;

			DPSExtreme.instance.packetHandler.SendProtocol(push);
			DPSExtreme.instance.combatTracker.myActiveCombat.SendStats();
		}

		void UpdateDOTDPS() {
			if (Main.netMode != NetmodeID.SinglePlayer && Main.netMode != NetmodeID.Server)
				return;

			Combat.DPSExtremeCombat activeCombat = DPSExtreme.instance.combatTracker.myActiveCombat;

			//Best-effort DOT DPS approx.
			int totalDotDPS = 0;

			foreach (NPC npc in Main.ActiveNPCs) {
				int dotDPS = -1 * npc.lifeRegen / 2;
				totalDotDPS += dotDPS;

				//Since the dot hook doesn't seem to work in SP, add damage here to the best of our abilities
				if (Main.netMode == NetmodeID.SinglePlayer) {
					float ratio = DPSExtreme.UPDATEDELAY / 60f;
					//TODO: Handle remainder
					int dealtDamage = (int)(dotDPS * ratio);

					DamageSource damageSource = new DamageSource(DamageSource.SourceType.DOT);
					damageSource.myDamageAmount = dealtDamage;
					damageSource.myDamageCauserId = (int)InfoListIndices.DOTs;
					damageSource.myDamageCauserAbility = (int)DamageSource.SourceType.DOT; //Unknown what dots it is

					DPSExtreme.instance.combatTracker.myStatsHandler.AddDealtDamage(npc, damageSource);
				}
			}

			activeCombat.myStats.myDamagePerSecond[(int)InfoListIndices.DOTs] = totalDotDPS;
		}
	}
}