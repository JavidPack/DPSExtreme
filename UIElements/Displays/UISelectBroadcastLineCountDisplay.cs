using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;

namespace DPSExtreme.UIElements.Displays
{
	internal class UISelectBroadcastLineCountDisplay : UISelectionDisplay
	{
		internal static string GetName(int aIndex) {
			return aIndex.ToString() + " " + Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("Lines"));
		}

		internal UISelectBroadcastLineCountDisplay()
			: base(ListDisplayMode.ChatBroadcastLineCountSelect, GetName) {

		}

		protected override void PopulateEntries() {
			int entryIndex = 0;

			UISelectionDisplayEntry allLinesEntry = CreateEntry(entryIndex) as UISelectionDisplayEntry;
			allLinesEntry.myColor = DPSExtremeUI.chatColor[Math.Abs(entryIndex) % DPSExtremeUI.chatColor.Length];
			allLinesEntry.myIndex = int.MaxValue;
			allLinesEntry.myNameText = Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("AllLines"));
			entryIndex++;

			for (int i = 0; i < 4; i++) {
				UISelectionDisplayEntry entry = CreateEntry(entryIndex) as UISelectionDisplayEntry;
				entry.myColor = DPSExtremeUI.chatColor[Math.Abs(entryIndex) % DPSExtremeUI.chatColor.Length];
				entry.myIndex = i + 1;
				entry.myNameText = myNameCallback != null ? myNameCallback(entry.myIndex) : "No name callback";
				entryIndex++;
			}

			for (int i = 0; i < 5; i++) {
				UISelectionDisplayEntry entry = CreateEntry(entryIndex) as UISelectionDisplayEntry;
				entry.myColor = DPSExtremeUI.chatColor[Math.Abs(entryIndex) % DPSExtremeUI.chatColor.Length];
				entry.myIndex = (i + 1) * 5;
				entry.myNameText = myNameCallback != null ? myNameCallback(entry.myIndex) : "No name callback";
				entryIndex++;
			}

			Recalculate();
		}

		protected override void OnSelect(int aSelectedIndex) {
			if (DPSExtremeUI.instance.myDisplayedCombat != null) {
				DPSExtremeUI.instance.myDisplayedCombat.PrintStats(Main.LocalPlayer.name, DPSExtremeUI.instance.myPreviousDisplayMode, aSelectedIndex);
			}
			else {
				Main.NewText(Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("NoDataToBroadcast")));
			}

			DPSExtremeUI.instance.myDisplayMode = DPSExtremeUI.instance.myPreviousDisplayMode;
		}
	}
}