using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace DPSExtreme
{
	internal class DPSExtremeModProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		public int whoIsMyParent = -1;
		public int myParentItemType = -1;

		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			if (source is EntitySource_Parent parentSource) {
				if (parentSource.Entity is NPC parentNPC) {
					if (projectile.friendly)
						whoIsMyParent = (int)InfoListIndices.NPCs;
					else
						whoIsMyParent = parentNPC.whoAmI;
				}
				else if (parentSource.Entity is Player parentplayer) {
					if (parentSource is EntitySource_ItemUse itemSource)
						myParentItemType = itemSource.Item.type;
				}
				else if (parentSource.Entity is Projectile parentProj) {
					myParentItemType = parentProj.GetGlobalProjectile<DPSExtremeModProjectile>().myParentItemType;
					whoIsMyParent = parentProj.whoAmI;
				}
			}

			if (source is EntitySource_Wiring wiring) {
				whoIsMyParent = (int)InfoListIndices.Traps;
				int row = Main.tile[wiring.TileCoords].TileFrameY / 18;
				if (row == 0)
					myParentItemType = ItemID.DartTrap;
				else if (row == 1)
					myParentItemType = ItemID.SuperDartTrap;
				else if (row == 2)
					myParentItemType = ItemID.FlameTrap;
				else if (row == 3)
					myParentItemType = ItemID.SpikyBallTrap;
				else if (row == 4)
					myParentItemType = ItemID.SpearTrap;
				else if (row == 5)
					myParentItemType = ItemID.VenomDartTrap;
				else
					myParentItemType = (int)InfoListIndices.Traps; //Clump all modded traps together
			}
		}
	}
}