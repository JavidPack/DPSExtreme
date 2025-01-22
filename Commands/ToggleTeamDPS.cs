using Terraria.Localization;
using Terraria.ModLoader;

namespace DPSExtreme.Commands
{
	internal class ToggleTeamDPS : ModCommand
	{
		public override CommandType Type => CommandType.Chat;
		public override string Command => "teamdps"; // TODO: investigate if localized commands works.
		public override string Description => Language.GetTextValue(Mod.GetLocalizationKey("ToggleTeamDPSCommandDescription"));

		public override void Action(CommandCaller caller, string input, string[] args) {
			DPSExtremeUI.instance.ShowTeamDPSPanel = !DPSExtremeUI.instance.ShowTeamDPSPanel;
			//DPSExtreme.ShowTeamDPS = !DPSExtreme.ShowTeamDPS;
		}
	}
}