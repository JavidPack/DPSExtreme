using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameInput;
using Terraria.Localization;

namespace DPSExtreme
{
	internal class DPSExtremeModPlayer : ModPlayer
	{
		public override void PostUpdate()
		{
			// only do this in MP
			if (Main.GameUpdateCount % DPSExtreme.UPDATEDELAY == 0)
			{
				if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer && Player.accDreamCatcher)
				{
					int dps = Player.getDPS();
					if (!Player.dpsStarted)
						dps = 0;

					ModPacket packet = Mod.GetPacket();
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
				if (Main.netMode != NetmodeID.MultiplayerClient && !DPSExtremeUI.instance.ShowTeamDPSPanel)
				{
					Main.NewText(Language.GetTextValue(Mod.GetLocalizationKey("OnlyAvailableInMultiplayer")));
					return;
				}
				DPSExtremeUI.instance.ShowTeamDPSPanel = !DPSExtremeUI.instance.ShowTeamDPSPanel;
			}
		}
	}
}

