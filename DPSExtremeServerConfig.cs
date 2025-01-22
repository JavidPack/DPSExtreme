using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace DPSExtreme
{
	public class DPSExtremeServerConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		public static DPSExtremeServerConfig Instance;

		[DefaultValue(false)]
		public bool DebugLogging { get; set; }
	}
}
