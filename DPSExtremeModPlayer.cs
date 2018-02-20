using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameInput;

namespace DPSExtreme
{
	internal class DPSExtremeModPlayer : ModPlayer
	{
		public override void PostUpdate()
		{
			// only do this in MP
			if (Main.GameUpdateCount % DPSExtreme.UPDATEDELAY == 0)
			{
				if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer && player.accDreamCatcher)
				{
					int dps = player.getDPS();
					if (!player.dpsStarted)
						dps = 0;

					ModPacket packet = mod.GetPacket();
					packet.Write((byte)DPSExtremeMessageType.InformServerCurrentDPS);
					packet.Write(dps);
					packet.Send();
				}
			}
		}

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			if (DPSExtreme.instance.ToggleTeamDPSHotKey.JustPressed)
			{
				if (Main.netMode != 1 && !DPSExtremeUI.instance.ShowTeamDPSPanel)
				{
					Main.NewText("Team DPS only available in Multiplayer game.");
					return;
				}
				DPSExtremeUI.instance.ShowTeamDPSPanel = !DPSExtremeUI.instance.ShowTeamDPSPanel;
			}
		}
	}
}

