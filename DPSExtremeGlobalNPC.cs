using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Terraria.Localization;

namespace DPSExtreme
{
	internal class DPSExtremeGlobalNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		internal int[] damageDone;
		internal bool onDeathBed; // SP only flag for something?

		public DPSExtremeGlobalNPC()
		{
			damageDone = new int[256];
		}

		//public override GlobalNPC Clone()
		//{
		//	try
		//	{
		//		DPSExtremeGlobalNPC clone = (DPSExtremeGlobalNPC)base.Clone();
		//		clone.damageDone = new int[256];
		//		return clone;
		//	}
		//	catch (Exception e)
		//	{
		//		//ErrorLogger.Log("Clone" + e.Message);
		//	}
		//	return null;
		//}

		// question, in MP, is this called before or after last hit?
		public override void NPCLoot(NPC npc)
		{
			try
			{
				//System.Console.WriteLine("NPCLoot");

				if (npc.boss)
				{
					if (Main.netMode == NetmodeID.SinglePlayer)
					{
						onDeathBed = true;
					}
					else
					{
						SendStats(npc);
					}
				}
			}
			catch (Exception e)
			{
				//ErrorLogger.Log("NPCLoot" + e.Message);
			}
		}

		void SendStats(NPC npc)
		{
			try
			{
				//System.Console.WriteLine("SendStats");

				StringBuilder sb = new StringBuilder();
				sb.Append("Damage stats for " + Lang.GetNPCNameValue(npc.type) + ": ");
				for (int i = 0; i < 256; i++)
				{
					int playerDamage = damageDone[i];
					if (playerDamage > 0)
					{
						if (i == 255)
						{
							sb.Append($"Traps/TownNPC: {playerDamage}, ");
						}
						else
						{
							sb.Append($"{Main.player[i].name}: {playerDamage}, ");
						}
					}
				}
				sb.Length -= 2; // removes last ,
				Color messageColor = Color.Orange;

				if (Main.netMode == NetmodeID.Server)
				{
					NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(sb.ToString()), messageColor);

					var netMessage = mod.GetPacket();
					netMessage.Write((byte)DPSExtremeMessageType.InformClientsCurrentBossTotals);
					netMessage.Write(true);
					netMessage.Write((byte)npc.whoAmI);
					DPSExtremeGlobalNPC bossGlobalNPC = npc.GetGlobalNPC<DPSExtremeGlobalNPC>();
					byte count = 0;
					for (int i = 0; i < 256; i++)
					{
						if (bossGlobalNPC.damageDone[i] > 0)
						{
							count++;
						}
					}
					netMessage.Write(count);
					for (int i = 0; i < 256; i++)
					{
						if (bossGlobalNPC.damageDone[i] > 0)
						{
							netMessage.Write((byte)i);
							netMessage.Write(bossGlobalNPC.damageDone[i]);
						}
					}
					netMessage.Send();

					Dictionary<byte, int> stats = new Dictionary<byte, int>();
					for (int i = 0; i < 256; i++)
					{
						if (bossGlobalNPC.damageDone[i] > -1)
						{
							stats[(byte)i] = bossGlobalNPC.damageDone[i];
						}
					}
					DPSExtreme.instance.InvokeOnSimpleBossStats(stats);
				}
				else if (Main.netMode == NetmodeID.SinglePlayer)
				{
					Main.NewText(sb.ToString(), messageColor);
				}
				else if (Main.netMode == NetmodeID.MultiplayerClient)
				{
					// MP clients should just wait for message.
				}
			}
			catch (Exception e)
			{
				//ErrorLogger.Log("SendStats" + e.Message);
			}
		}

		// Things like townNPC and I think traps will trigger this in Server. In SP, all is done here.
		public override void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit)
		{
			try
			{
				//System.Console.WriteLine("OnHitByItem " + player.whoAmI);

				NPC damagedNPC = npc;
				if (npc.realLife >= 0)
				{
					damagedNPC = Main.npc[damagedNPC.realLife];
				}
				DPSExtremeGlobalNPC info = damagedNPC.GetGlobalNPC<DPSExtremeGlobalNPC>();
				info.damageDone[player.whoAmI] += damage;
				if (info.onDeathBed) // oh wait, is this the same as .active in this case? probably not.
				{
					info.SendStats(damagedNPC);
					info.onDeathBed = false; // multiple things can hit while on deathbed.
				}

				//damageDone[player.whoAmI] += damage;
				//if (onDeathBed)
				//{
				//	SendStats(npc);
				//}
			}
			catch (Exception e)
			{
				//ErrorLogger.Log("OnHitByItem" + e.Message);
			}
		}

		// Things like townNPC and I think traps will trigger this in Server. In SP, all is done here.
		public override void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit)
		{
			//TODO, owner could be -1?
			try
			{
				//System.Console.WriteLine("OnHitByProjectile " + projectile.owner);

				NPC damagedNPC = npc;
				if (npc.realLife >= 0)
				{
					damagedNPC = Main.npc[damagedNPC.realLife];
				}
				DPSExtremeGlobalNPC info = damagedNPC.GetGlobalNPC<DPSExtremeGlobalNPC>();
				info.damageDone[projectile.owner] += damage;
				if (info.onDeathBed)
				{
					info.SendStats(damagedNPC);
					info.onDeathBed = false;
				}

				//damageDone[projectile.owner] += damage;
				//if (onDeathBed)
				//{
				//	SendStats(npc);
				//}
			}
			catch (Exception e)
			{
				//ErrorLogger.Log("OnHitByProjectile" + e.Message);
			}
		}
	}
}

