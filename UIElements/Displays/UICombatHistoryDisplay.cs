
using DPSExtreme.Combat;
using Microsoft.Xna.Framework;
using System;
using Terraria.Localization;

namespace DPSExtreme.UIElements.Displays
{
	internal class UICombatHistoryDisplay : UISelectionDisplay
	{
		internal UICombatHistoryDisplay()
			: base(ListDisplayMode.CombatHistory, GetEntryName) {

		}

		internal static string GetEntryName(int aIndex) {
			DPSExtremeCombat combat = DPSExtreme.instance.combatTracker.GetCombatHistory(aIndex);

			if (combat == null)
				return "null combat";

			return combat.GetTitle();
		}

		protected override void PopulateEntries() {
			int entryIndex = 0;

			UISelectionDisplayEntry allCombatsEntry = CreateEntry(entryIndex) as UISelectionDisplayEntry;
			allCombatsEntry.myColor = Color.Orange;
			allCombatsEntry.myNameText = Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("AllCombats"));

			allCombatsEntry.myRightText = String.Format("{0:D2}:{1:D2}",
				(int)Math.Floor(DPSExtreme.instance.combatTracker.myTotalCombat.myDuration.TotalMinutes), DPSExtreme.instance.combatTracker.myTotalCombat.myDuration.Seconds);

			allCombatsEntry.myIndex = -2;
			entryIndex++;

			if (DPSExtreme.instance.combatTracker.myActiveCombat != null) {
				UISelectionDisplayEntry entry = CreateEntry(entryIndex) as UISelectionDisplayEntry;
				entry.myColor = Color.Yellow;
				entry.myNameText = Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("CurrentCombat"));
				entry.myRightText = DPSExtreme.instance.combatTracker.myActiveCombat.myFormattedDuration;
				entry.myIndex = -1;
				entryIndex++;
			}

			for (int i = DPSExtremeCombatTracker.ourHistorySize - 1; i >= 0; i--) {
				DPSExtremeCombat combat = DPSExtreme.instance.combatTracker.GetCombatHistory(i);

				if (combat == null)
					continue;

				UISelectionDisplayEntry entry = CreateEntry(entryIndex) as UISelectionDisplayEntry;
				entry.myColor = DPSExtremeUI.chatColor[Math.Abs(i) % DPSExtremeUI.chatColor.Length];
				entry.myNameText = myNameCallback != null ? myNameCallback(i) : "No name callback";
				entry.myRightText = combat.myFormattedDuration;
				entry.myIndex = i;
				entryIndex++;
			}

			Recalculate();
		}

		protected override void OnSelect(int aSelectedIndex) {
			DPSExtremeUI.instance.myDisplayMode = DPSExtremeUI.instance.myPreviousDisplayMode;

			DPSExtremeCombat combat = null;

			DPSExtremeUI.instance.myShowAllCombatTotals = false;

			if (aSelectedIndex == -2)
				DPSExtremeUI.instance.myShowAllCombatTotals = true;
			else if (aSelectedIndex == -1)
				combat = DPSExtreme.instance.combatTracker.myActiveCombat;
			else
				combat = DPSExtreme.instance.combatTracker.GetCombatHistory(aSelectedIndex);

			if (combat == null)
				return;

			DPSExtremeUI.instance.myDisplayedCombat = combat;
		}
	}
}