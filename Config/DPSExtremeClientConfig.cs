using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace DPSExtreme.Config
{
	internal class DPSExtremeClientConfig : ModConfig
	{
		public static DPSExtremeClientConfig Instance = null;

		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Header("GeneralConfigHeader")]
		[DefaultValue(false)]
		public bool ShowPercentages;

		[DefaultValue(false)]
		public bool SnapAdditionalInfoBoxToMouse;

		[Header("UserInterface")]
		public bool ShowUIByDefault;

		[DefaultValue(1f)]
		[Increment(0.05f)]
		public float UITransparency;

		public override void OnChanged() {
			DPSExtremeUI.instance?.OnClientConfigLoad();
		}
	}
}