using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace DPSExtreme
{
	internal class DPSExtremeModProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		public int whoIsMyParent = -1;

		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			if (source is EntitySource_Parent parent) {
				if (parent.Entity is NPC) {
					whoIsMyParent = parent.Entity.whoAmI;
				}
			}

			if (source is EntitySource_Wiring wiring) {
				whoIsMyParent = (int)InfoListIndices.Traps;
			}
		}
	}
}
