using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using static DPSExtreme.CombatTracking.DPSExtremeCombat;

namespace DPSExtreme.CombatTracking
{
	internal class DPSExtremeCombatTracker
	{
		const int ourHistorySize = 5;
		const int ourGenericCombatTimeout = 5;

		private int myCurrentHistoryIndex = 0;
		private DPSExtremeCombat[] myCombatHistory = new DPSExtremeCombat[ourHistorySize];
		internal DPSExtremeCombat myActiveCombat = null;

		internal int myLastFrameInvasionType = InvasionID.None;
		internal int myLastFrameEventType = 0;

		public List<int> myJoiningPlayers = new List<int>();

		internal DPSExtremeCombat GetCombatHistory(int aIndex) {
			return myCombatHistory[(aIndex + myCurrentHistoryIndex) % ourHistorySize];
		}

		internal void Update() {
			if (Main.netMode != NetmodeID.SinglePlayer && Main.netMode != NetmodeID.Server)
				return;

			if (Main.netMode == NetmodeID.Server) {
				foreach (int player in myJoiningPlayers)
					OnPlayerJoined(player);

				myJoiningPlayers.Clear();
			}

			UpdateEventCheckStart();
			UpdateEventCheckEnd();
			myLastFrameEventType = GetActiveEventType();

			UpdateInvasionCheckStart();
			UpdateInvasionCheckEnd();
			myLastFrameInvasionType = GetActiveInvasionType();

			UpdateAllBossesDeadCheck();
			UpdateGenericCombatTimeoutCheck();
		}

		public void OnPlayerJoined(int aPlayer) {
			DPSExtremeModPlayer.ourConnectedPlayers.Add(aPlayer);

			if (myActiveCombat == null) {
				if (DPSExtremeServerConfig.Instance.DebugLogging)
					ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("OnPlayerJoined - No Combat"), Color.Orange);
				myJoiningPlayers.Clear();
				return;
			}

			if (DPSExtremeServerConfig.Instance.DebugLogging)
				ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(String.Format("OnPlayerJoined - {0}", aPlayer)), Color.Orange);

			ProtocolPushStartCombat push = new ProtocolPushStartCombat();
			push.myCombatType = myActiveCombat.myCombatTypeFlags;
			push.myBossOrInvasionOrEventType = myActiveCombat.myBossOrInvasionOrEventType;

			DPSExtreme.instance.packetHandler.SendProtocol(push, aPlayer);
		}

		public void OnPlayerLeft(int aPlayer) {
			if (myActiveCombat == null)
				return;

			myActiveCombat.OnPlayerLeft(aPlayer);
		}

		private int GetActiveEventType() {
			if (Main.eclipse)
				return (int)EventType.Eclipse;

			if (Main.bloodMoon)
				return (int)EventType.BloodMoon;

			if (Main.slimeRain)
				return (int)EventType.SlimeRain;

			return 0;
		}

		void UpdateEventCheckStart() {
			int eventType = GetActiveEventType();
			if (eventType == 0)
				return;

			if (myLastFrameEventType == eventType)
				return;

			//Wait for invasion delay?

			TriggerCombat(CombatType.Event, eventType);
		}

		void UpdateEventCheckEnd() {
			if (myActiveCombat == null)
				return;

			if ((myActiveCombat.myCombatTypeFlags & CombatType.Event) == 0)
				return;

			if (GetActiveEventType() != 0)
				return;

			ProtocolPushEndCombat push = new ProtocolPushEndCombat();
			push.myCombatType = CombatType.Event;

			//Needs to happen before it gets sent to clients
			if (Main.netMode == NetmodeID.Server)
				DPSExtreme.instance.packetHandler.HandleEndCombatPush(push);

			DPSExtreme.instance.packetHandler.SendProtocol(push);
		}

		private int GetActiveInvasionType() {
			if (Main.invasionType != InvasionID.None)
				return Main.invasionType;

			if (Main.snowMoon)
				return (int)InvasionType.FrostMoon;

			if (Main.pumpkinMoon)
				return (int)InvasionType.PumpkinMoon;

			return InvasionID.None;
		}

		void UpdateInvasionCheckStart() {
			//TODO: Fix issue with combat not being started if invasion is active when world loads

			if (GetActiveInvasionType() == InvasionID.None)
				return;

			if (myLastFrameInvasionType == GetActiveInvasionType())
				return;

			//Wait for invasion delay?

			TriggerCombat(CombatType.Invasion, GetActiveInvasionType());
		}

		void UpdateInvasionCheckEnd() {
			if (myActiveCombat == null)
				return;

			if ((myActiveCombat.myCombatTypeFlags & CombatType.Invasion) == 0)
				return;

			if (GetActiveInvasionType() != InvasionID.None)
				return;

			ProtocolPushEndCombat push = new ProtocolPushEndCombat();
			push.myCombatType = CombatType.Invasion;

			//Needs to happen before it gets sent to clients
			if (Main.netMode == NetmodeID.Server)
				DPSExtreme.instance.packetHandler.HandleEndCombatPush(push);

			DPSExtreme.instance.packetHandler.SendProtocol(push);
		}

		private void UpdateAllBossesDeadCheck() {
			if (myActiveCombat == null)
				return;

			if (((myActiveCombat.myCombatTypeFlags & CombatType.BossFight) == 0))
				return;

			bool bossAlive = false;

			foreach (NPC npc in Main.ActiveNPCs) {
				if (!npc.boss)
					continue;

				bossAlive = true;
			}

			if (bossAlive)
				return;

			ProtocolPushEndCombat push = new ProtocolPushEndCombat();
			push.myCombatType = CombatType.BossFight;

			//Needs to happen before it gets sent to clients
			if (Main.netMode == NetmodeID.Server)
				DPSExtreme.instance.packetHandler.HandleEndCombatPush(push);

			DPSExtreme.instance.packetHandler.SendProtocol(push);
		}

		void UpdateGenericCombatTimeoutCheck() {
			if (myActiveCombat == null)
				return;

			if (((myActiveCombat.myCombatTypeFlags & CombatType.Generic) == 0))
				return;

			TimeSpan elapsedSinceLastActivity = DateTime.Now - myActiveCombat.myLastActivityTime;

			if (elapsedSinceLastActivity.TotalSeconds < ourGenericCombatTimeout)
				return;

			ProtocolPushEndCombat push = new ProtocolPushEndCombat();
			push.myCombatType = CombatType.Generic;

			//Needs to happen before it gets sent to clients
			if (Main.netMode == NetmodeID.Server)
				DPSExtreme.instance.packetHandler.HandleEndCombatPush(push);

			DPSExtreme.instance.packetHandler.SendProtocol(push);
		}

		//Called when damage is dealt or bosses spawn etc
		internal void TriggerCombat(CombatType aCombatType, int aBossOrInvasionOrEventType = -1) {
			if (myActiveCombat != null) {
				UpgradeCombat(aCombatType, aBossOrInvasionOrEventType);
				myActiveCombat.myLastActivityTime = DateTime.Now;
				return;
			}

			ProtocolPushStartCombat push = new ProtocolPushStartCombat();
			push.myCombatType = aCombatType;
			push.myBossOrInvasionOrEventType = aBossOrInvasionOrEventType;

			if (Main.netMode == NetmodeID.Server)
				DPSExtreme.instance.packetHandler.HandleStartCombatPush(push);

			DPSExtreme.instance.packetHandler.SendProtocol(push);
		}

		//Called from TriggerCombat or from server pushing combat status
		internal void StartCombat(CombatType aCombatType, int aBossOrInvasionOrEventType = -1) {
			if (myActiveCombat != null) {
				UpgradeCombat(aCombatType, aBossOrInvasionOrEventType);
				Main.NewText("Upgrade through StartCombat. Should probably never happen");
				return;
			}

			if (DPSExtremeServerConfig.Instance.DebugLogging) {
				if (Main.netMode == NetmodeID.SinglePlayer || Main.netMode == NetmodeID.MultiplayerClient)
					Main.NewText(String.Format("Started combat of type: {0}", aCombatType.ToString()));
				else if (Main.netMode == NetmodeID.Server)
					ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Server started combat"), Color.Orange);
			}

			myActiveCombat = new DPSExtremeCombat(aCombatType, aBossOrInvasionOrEventType);

			DPSExtremeUI.instance?.OnCombatStarted(myActiveCombat);
		}

		//Boss fight starts during an invastion etc
		internal void UpgradeCombat(CombatType aCombatType, int aBossOrInvasionOrEventType = -1) {
			int oldHighestCombat = (int)myActiveCombat.myHighestCombatType;
			myActiveCombat.myHighestCombatType = (CombatType)Math.Max((int)myActiveCombat.myHighestCombatType, (int)aCombatType);
			myActiveCombat.myCombatTypeFlags |= aCombatType;

			if ((int)myActiveCombat.myHighestCombatType > oldHighestCombat) {
				if (DPSExtremeServerConfig.Instance.DebugLogging) {
					if (Main.netMode == NetmodeID.SinglePlayer || Main.netMode == NetmodeID.MultiplayerClient)
						Main.NewText(String.Format("Upgraded combat from {0} to {1}", ((CombatType)oldHighestCombat).ToString(), aCombatType.ToString()));
					else if (Main.netMode == NetmodeID.Server)
						ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(String.Format("Server upgraded combat from {0} to {1}", ((CombatType)oldHighestCombat).ToString(), aCombatType.ToString())), Color.Orange);
				}

				myActiveCombat.myBossOrInvasionOrEventType = aBossOrInvasionOrEventType;

				DPSExtremeUI.instance?.OnCombatUpgraded(myActiveCombat);

				if (Main.netMode == NetmodeID.Server) {
					ProtocolPushUpgradeCombat push = new ProtocolPushUpgradeCombat();
					push.myCombatType = myActiveCombat.myHighestCombatType;
					push.myBossOrInvasionOrEventType = myActiveCombat.myBossOrInvasionOrEventType;

					DPSExtreme.instance.packetHandler.SendProtocol(push);
				}
			}
		}

		internal void EndCombat(CombatType aCombatType) {
			if (myActiveCombat == null)
				return;

			myActiveCombat.myCombatTypeFlags &= ~aCombatType;

			//if combat contains both boss + invasion, wait for both to finish before ending
			if (myActiveCombat.myCombatTypeFlags > CombatType.Generic)
				return;

			if (DPSExtremeServerConfig.Instance.DebugLogging) {
				if (Main.netMode == NetmodeID.SinglePlayer)
					Main.NewText(String.Format("Ended combat"));
				else if (Main.netMode == NetmodeID.Server)
					ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Ended combat"), Color.White);
			}

			myCurrentHistoryIndex++;

			myActiveCombat.SendStats();
			myActiveCombat.PrintStats();
			myActiveCombat = null;

			DPSExtremeUI.instance?.OnCombatEnded();
		}
	}
}
