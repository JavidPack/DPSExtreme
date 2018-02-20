using Terraria;
using Terraria.ModLoader;

namespace DPSExtreme.Commands
{
	internal class ToggleTeamDPS : ModCommand
	{
		public override CommandType Type => CommandType.Chat;
		public override string Command => "teamdps";
		public override string Description => "Toggle Team DPS display";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			if (Main.netMode != 1 && !DPSExtremeUI.instance.ShowTeamDPSPanel)
			{
				Main.NewText("Team DPS only available in Multiplayer game.");
				return;
			}
			DPSExtremeUI.instance.ShowTeamDPSPanel = !DPSExtremeUI.instance.ShowTeamDPSPanel;
			//DPSExtreme.ShowTeamDPS = !DPSExtreme.ShowTeamDPS;
		}
	}
}

