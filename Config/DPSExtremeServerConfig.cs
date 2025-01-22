using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace DPSExtreme.Config
{
	internal class DPSExtremeServerConfig : ModConfig
	{
		public static DPSExtremeServerConfig Instance = null;

		public override ConfigScope Mode => ConfigScope.ServerSide;

		[Header("GeneralConfigHeader")]
		[Increment(5)]
		[Range(10, 120)]
		[DefaultValue(30)]
		[TooltipKey("$Mods.DPSExtreme.Configs.DPSExtremeServerConfig.RefreshRate.Tooltip")]
		public int RefreshRate;

		[Header("CombatConfigHeader")]
		[DefaultValue(true)]
		[TooltipKey("$Mods.DPSExtreme.Configs.DPSExtremeServerConfig.TrackGenericCombat.Tooltip")]
		public bool TrackGenericCombat;

		[DefaultValue(5)]
		[Increment(1)]
		[TooltipKey("$Mods.DPSExtreme.Configs.DPSExtremeServerConfig.GenericCombatTimeout.Tooltip")]
		public int GenericCombatTimeout;

		[DefaultValue(false)]
		[TooltipKey("$Mods.DPSExtreme.Configs.DPSExtremeServerConfig.EndGenericCombatsWhenUpgraded.Tooltip")]
		public bool EndGenericCombatsWhenUpgraded;

		[DefaultValue(true)]
		[TooltipKey("$Mods.DPSExtreme.Configs.DPSExtremeServerConfig.TrackEvents.Tooltip")]
		public bool TrackEvents;

		[DefaultValue(true)]
		[TooltipKey("$Mods.DPSExtreme.Configs.DPSExtremeServerConfig.TrackInvasions.Tooltip")]
		public bool TrackInvasions;

		[DefaultValue(true)]
		[TooltipKey("$Mods.DPSExtreme.Configs.DPSExtremeServerConfig.TrackBosses.Tooltip")]
		public bool TrackBosses;

		[DefaultValue(false)]
		[TooltipKey("$Mods.DPSExtreme.Configs.DPSExtremeServerConfig.IgnoreCritters.Tooltip")]
		public bool IgnoreCritters;

		[Header("BuffUptimesHeader")]
		[DefaultValue(false)]
		public bool IgnorePermanentBuffs;

		[DefaultValue(false)]
		public bool IgnoreMinionBuffs;

		[Header("Debug")]
		[DefaultValue(false)]
		public bool ShowDebugMessages;

		public override void OnChanged() {
			if (DPSExtremeUI.instance != null)
				DPSExtremeUI.instance.OnServerConfigLoad();

			if (DPSExtreme.instance != null)
				DPSExtreme.instance.OnServerConfigLoad();
		}
	}
}