using Terraria;
using Terraria.ModLoader;

namespace DPSExtreme
{
	// Takes care of regularly sending out DPS values to clients.
	// Do we even need to send?
	internal class DPSExtremeModWorld : ModSystem
	{
		public override void PostUpdateWorld()
		{
			if (Main.netMode == 2 && Main.GameUpdateCount % DPSExtreme.UPDATEDELAY == 0)
			{
				var netMessage = Mod.GetPacket();
				netMessage.Write((byte)DPSExtremeMessageType.InformClientsCurrentDPSs);
				byte count = 0;
				for (int i = 0; i < 256; i++)
				{
					if (Main.player[i].active && Main.player[i].accDreamCatcher)
					{
						count++;
					}
				}
				netMessage.Write(count);
				for (int i = 0; i < 256; i++)
				{
					if (Main.player[i].active && Main.player[i].accDreamCatcher)
					{
						netMessage.Write((byte)i);
						netMessage.Write(DPSExtreme.dpss[i]);
					}
				}
				netMessage.Send();


				byte bossIndex = 255;
				float maxProgress = -1f;
				for (byte i = 0; i < 200; i++)
				{
					NPC npc = Main.npc[i];
					if (npc.active && npc.boss && (npc.realLife == -1 || npc.realLife == npc.whoAmI))
					{
						//NPC realNPC = npc.realLife >= 0 ? Main.npc[npc.realLife] : npc;
						float deathProgress = 1f - ((float)npc.life / npc.lifeMax);
						if (deathProgress > maxProgress)
						{
							maxProgress = deathProgress;
							bossIndex = i;
						}
					}
				}
				if (bossIndex != 255)
				{
					netMessage = Mod.GetPacket();
					netMessage.Write((byte)DPSExtremeMessageType.InformClientsCurrentBossTotals);
					netMessage.Write(true);
					netMessage.Write(bossIndex);
					DPSExtremeGlobalNPC bossGlobalNPC = Main.npc[bossIndex].GetGlobalNPC<DPSExtremeGlobalNPC>();
					count = 0;
					for (int i = 0; i < 256; i++)
					{
						if (bossGlobalNPC.damageDone[i] > 0)
						{
							count++;
						}
					}
					netMessage.Write(count);
					for (int i = 0; i < 256; i++)
					{
						if (bossGlobalNPC.damageDone[i] > 0)
						{
							netMessage.Write((byte)i);
							netMessage.Write(bossGlobalNPC.damageDone[i]);
						}
					}
					netMessage.Send();
				}
			}
		}
	}
}

