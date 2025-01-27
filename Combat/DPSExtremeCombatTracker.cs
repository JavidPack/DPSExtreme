using DPSExtreme.Combat.Stats;
using DPSExtreme.Config;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using static DPSExtreme.Combat.DPSExtremeCombat;

namespace DPSExtreme.Combat
{
	internal class DPSExtremeCombatTracker
	{
		internal const int ourHistorySize = 10;

		private int myHistoryBufferZeroIndex = 0; //Ring buffer shit
		private DPSExtremeCombat[] myCombatHistory = new DPSExtremeCombat[ourHistorySize];

		internal DPSExtremeCombat myTotalCombat = new DPSExtremeCombat(CombatType.Generic, -1); //Stores accumulated data from all combats (even those erased from history)
		internal DPSExtremeCombat myActiveCombat = null;

		internal DPSExtremeStatsHandler myStatsHandler = new DPSExtremeStatsHandler();

		internal int myLastFrameInvasionType = InvasionID.None;
		internal int myLastFrameEventType = 0;

		public List<int> myJoiningPlayers = new List<int>();

		internal DPSExtremeCombat GetCombatHistory(int aIndex) {
			if (aIndex < 0)
				return null;

			return myCombatHistory[(aIndex + myHistoryBufferZeroIndex) % ourHistorySize];
		}

		internal void Update() {
			if (Main.netMode != NetmodeID.SinglePlayer && Main.netMode != NetmodeID.Server)
				return;

			if (Main.netMode == NetmodeID.Server) {
				var temp = myJoiningPlayers.ToList();
				 
				foreach (int player in temp) {
					OnPlayerJoined(player);
					myJoiningPlayers.Remove(player);
				}
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

		internal void OnEnterWorld() {
			myActiveCombat = null;
			myCombatHistory = new DPSExtremeCombat[ourHistorySize];
		}

		public void OnPlayerJoined(int aPlayer) {
			DPSExtremeModPlayer.ourConnectedPlayers.Add(aPlayer);

			if (myActiveCombat == null) {
				DPSExtreme.instance.DebugMessage("OnPlayerJoined - No Combat");
				myJoiningPlayers.Clear();
				return;
			}

			DPSExtreme.instance.DebugMessage(String.Format("OnPlayerJoined - Player: {0}", aPlayer));

			ProtocolPushStartCombat push = new ProtocolPushStartCombat();
			push.myCombatType = myActiveCombat.myHighestCombatType;
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

		internal void UpdateCombatDuration() {
			if (myActiveCombat == null)
				return;

			myActiveCombat.myDurationInTicks++;
			myActiveCombat.myTicksSinceLastActivity++;

			myTotalCombat.myDurationInTicks++;
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

			SendEndCombat(CombatType.Event);
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

			SendEndCombat(CombatType.Invasion);
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

			SendEndCombat(CombatType.BossFight);
		}

		void UpdateGenericCombatTimeoutCheck() {
			if (myActiveCombat == null)
				return;

			if (((myActiveCombat.myCombatTypeFlags & CombatType.Generic) == 0))
				return;

			if (myActiveCombat.myTimeSinceLastActivity.TotalSeconds < DPSExtremeServerConfig.Instance.GenericCombatTimeout)
				return;

			SendEndCombat(CombatType.Generic);
		}

		//Called when damage is dealt or bosses spawn etc
		internal void TriggerCombat(CombatType aCombatType, int aBossOrInvasionOrEventType = -1) {
			switch (aCombatType) {
				case CombatType.Generic:
					if (!DPSExtremeServerConfig.Instance.TrackGenericCombat) { return; }
					break;
				case CombatType.Event:
					if (!DPSExtremeServerConfig.Instance.TrackEvents) { return; }
					break;
				case CombatType.Invasion:
					if (!DPSExtremeServerConfig.Instance.TrackInvasions) { return; }
					break;
				case CombatType.BossFight:
					if (!DPSExtremeServerConfig.Instance.TrackBosses) { return; }
					break;
				default:
					break;
			}

			if (myActiveCombat != null) {
				UpgradeCombat(aCombatType, aBossOrInvasionOrEventType);
				myActiveCombat.myTicksSinceLastActivity = 0;
				return;
			}

			ProtocolPushStartCombat push = new ProtocolPushStartCombat();
			push.myCombatType = aCombatType;
			push.myBossOrInvasionOrEventType = aBossOrInvasionOrEventType;

			if (Main.netMode == NetmodeID.MultiplayerClient) {
				DPSExtreme.instance.packetHandler.HandleStartCombatPush(push);
				return;
			}

			if (Main.netMode == NetmodeID.Server)
				DPSExtreme.instance.packetHandler.HandleStartCombatPush(push);

			DPSExtreme.instance.packetHandler.SendProtocol(push);
		}

		//Called from TriggerCombat or from server pushing combat status
		internal void StartCombat(CombatType aCombatType, int aBossOrInvasionOrEventType = -1) {
			if (myActiveCombat != null) {
				UpgradeCombat(aCombatType, aBossOrInvasionOrEventType);
				return;
			}

			DPSExtreme.instance.DebugMessage(String.Format("Started combat of type: {0}", aCombatType.ToString()));
			myActiveCombat = new DPSExtremeCombat(aCombatType, aBossOrInvasionOrEventType);

			DPSExtremeUI.instance?.OnCombatStarted(myActiveCombat);
		}

		//Boss fight starts during an invastion etc
		internal void UpgradeCombat(CombatType aCombatType, int aBossOrInvasionOrEventType = -1) {
			if (DPSExtremeServerConfig.Instance.EndGenericCombatsWhenUpgraded) {
				if (aCombatType != CombatType.Generic && myActiveCombat.myHighestCombatType == CombatType.Generic) {
					SendEndCombat(CombatType.Generic);
					TriggerCombat(aCombatType, aBossOrInvasionOrEventType);
					return;
				}
			}

			int oldHighestCombat = (int)myActiveCombat.myHighestCombatType;
			myActiveCombat.myHighestCombatType = (CombatType)Math.Max((int)myActiveCombat.myHighestCombatType, (int)aCombatType);
			myActiveCombat.myCombatTypeFlags |= aCombatType;

			if ((int)myActiveCombat.myHighestCombatType <= oldHighestCombat)
				return;

			DPSExtreme.instance.DebugMessage(String.Format("Upgraded combat from {0} to {1}", ((CombatType)oldHighestCombat).ToString(), aCombatType.ToString()));

			myActiveCombat.myBossOrInvasionOrEventType = aBossOrInvasionOrEventType;

			DPSExtremeUI.instance?.OnCombatUpgraded(myActiveCombat);

			if (Main.netMode == NetmodeID.Server) {
				ProtocolPushUpgradeCombat push = new ProtocolPushUpgradeCombat();
				push.myCombatType = myActiveCombat.myHighestCombatType;
				push.myBossOrInvasionOrEventType = myActiveCombat.myBossOrInvasionOrEventType;

				DPSExtreme.instance.packetHandler.SendProtocol(push);
			}
		}

		internal void SendEndCombat(CombatType aCombatType) {
			ProtocolPushEndCombat push = new ProtocolPushEndCombat();
			push.myCombatType = aCombatType;

			//Needs to happen before it gets sent to clients
			if (Main.netMode == NetmodeID.Server)
				DPSExtreme.instance.packetHandler.HandleEndCombatPush(push);

			DPSExtreme.instance.packetHandler.SendProtocol(push);
		}

		internal void EndCombat(CombatType aCombatType) {
			if (myActiveCombat == null)
				return;

			myActiveCombat.myCombatTypeFlags &= ~aCombatType;

			//if combat contains both boss + invasion, wait for both to finish before ending
			if (myActiveCombat.myCombatTypeFlags > CombatType.Generic)
				return;

			DPSExtreme.instance.DebugMessage("Ended combat");

			int historyCount = 0;
			for (int i = 0; i < ourHistorySize; i++) {
				if (myCombatHistory[i] == null)
					continue;

				historyCount++;
			}

			myCombatHistory[(historyCount + myHistoryBufferZeroIndex) % ourHistorySize] = myActiveCombat;

			if (historyCount >= ourHistorySize)
				myHistoryBufferZeroIndex++;

			myActiveCombat.OnEnd();
			myActiveCombat = null;

			DPSExtremeUI.instance?.OnCombatEnded();
		}
	}
}