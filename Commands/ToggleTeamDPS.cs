using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ID;

namespace DPSExtreme.Commands
{
	internal class ToggleTeamDPS : ModCommand
	{
		public override CommandType Type => CommandType.Chat;
		public override string Command => "teamdps"; // TODO: investigate if localized commands works.
		public override string Description => Language.GetTextValue(Mod.GetLocalizationKey("ToggleTeamDPSCommandDescription"));

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient && !DPSExtremeUI.instance.ShowTeamDPSPanel)
			{
				Main.NewText(Language.GetTextValue(Mod.GetLocalizationKey("OnlyAvailableInMultiplayer")));
				return;
			}
			DPSExtremeUI.instance.ShowTeamDPSPanel = !DPSExtremeUI.instance.ShowTeamDPSPanel;
			//DPSExtreme.ShowTeamDPS = !DPSExtreme.ShowTeamDPS;
		}
	}
}

