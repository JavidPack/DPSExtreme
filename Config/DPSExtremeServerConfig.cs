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
		public int RefreshRate;

		[Increment(1)]
		[Range(0, 256)]
		[DefaultValue(10)]
		public int PostCombatDamageDonePrintLineCount;

		[Header("CombatConfigHeader")]
		[DefaultValue(true)]
		public bool TrackGenericCombat;

		[DefaultValue(5)]
		[Increment(1)]
		public int GenericCombatTimeout;

		[DefaultValue(false)]
		public bool EndGenericCombatsWhenUpgraded;

		[DefaultValue(true)]
		public bool TrackEvents;

		[DefaultValue(true)]
		public bool TrackInvasions;

		[DefaultValue(true)]
		public bool TrackBosses;

		[DefaultValue(false)]
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
			DPSExtremeUI.instance?.OnServerConfigLoad();

			DPSExtreme.instance?.OnServerConfigLoad();
		}
	}
}