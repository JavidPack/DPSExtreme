using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DPSExtreme
{
	// Takes care of regularly sending out DPS values to clients.
	// Do we even need to send?
	internal class DPSExtremeModWorld : ModSystem
	{
		public override void PostUpdateWorld() {
			if ((Main.GameUpdateCount % DPSExtreme.UPDATEDELAY) != 0)
				return;

			DPSExtreme.instance.combatTracker.Update();

			if (DPSExtreme.instance.combatTracker.myActiveCombat == null)
				return;

			if (Main.netMode == NetmodeID.Server || Main.netMode == NetmodeID.SinglePlayer) {
				ProtocolPushClientDPSs push = new ProtocolPushClientDPSs();
				push.myDPSList = DPSExtreme.instance.combatTracker.myActiveCombat.myDPSList;

				DPSExtreme.instance.packetHandler.SendProtocol(push);
				DPSExtreme.instance.combatTracker.myActiveCombat.SendStats();
			}
		}
	}
}

