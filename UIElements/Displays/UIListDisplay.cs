
using DPSExtreme.Combat.Stats;
using System.Linq;
using Terraria;
using Terraria.Localization;

namespace DPSExtreme.UIElements.Displays
{
	internal class UIListDisplay<T> : UICombatInfoDisplay
		where T : IStatContainer, new()
	{
		internal override DisplayContainerType myContainerType => DisplayContainerType.List;

		internal DPSExtremeStatList<T> myInfoList {
			get {
				try {
					if (myParentDisplay != null) {
						UIStatDictionaryDisplay<DPSExtremeStatList<T>> parent = myParentDisplay as UIStatDictionaryDisplay<DPSExtremeStatList<T>>;
						if (parent != null)
							return parent.myInfoLookup[myParentDisplay.myBreakdownAccessor];
					}

					return DPSExtremeUI.instance.myDisplayedCombat?.GetInfoContainer(myDisplayMode) as DPSExtremeStatList<T>;
				}
				catch (System.Exception) {

					throw;
				}
			}
		}

		public UIListDisplay(ListDisplayMode aDisplayMode, StatFormat aFormat = StatFormat.RawNumber)
			: base(aDisplayMode, typeof(T)) {
			myNameCallback = DPSExtremeStatListHelper.GetNameFromIndex;
			myFormat = aFormat;
		}

		internal override void Update() {
			if (myInfoList == null)
				return;

			RecalculateTotals();
			UpdateValues();

			Recalculate();
		}

		internal override void RecalculateTotals() {
			if (myIsInBreakdown) {
				myBreakdownDisplay.RecalculateTotals();
				return;
			}

			myInfoList?.GetMaxAndTotal(out myHighestValue, out myTotal);
		}

		internal override void UpdateValues() {
			if (myIsInBreakdown) {
				myBreakdownDisplay.UpdateValues();
				return;
			}

			if (!myInfoList.HasStats()) {
				Clear();
				return;
			}

			int entryIndex = 0;

			for (int i = 0; i < DPSExtremeStatList<T>.Size; i++) {
				T value = myInfoList[i];
				if (value.HasStats()) {
					int max = 0;
					int total = 0;
					value.GetMaxAndTotal(out max, out total);

					string name = "Missing name callback";
					if (myNameCallback != null)
						name = myNameCallback.Invoke(i);

					if (name.Length == 0)
						name = $"Error - id: {i}";

					UIStatDisplayEntry entry = CreateEntry(entryIndex) as UIStatDisplayEntry;
					entry.myParticipantIndex = i;
					entry.myColor = DPSExtremeUI.chatColor[i % DPSExtremeUI.chatColor.Length];
					entry.myNameText = name;
					entry.myInfoBoxLines = value.GetInfoBoxLines();
					entry.SetValues(total, myHighestValue, myTotal);
					entryIndex++;
				}
			}

			//Remove unused entries
			while (entryIndex < Children.ElementAt(0).Children.Count()) {
				Remove(Children.ElementAt(0).Children.ElementAt(entryIndex));
				entryIndex++;
			}

			//In case no new entries were added but they need to be re-sorted
			UpdateOrder();
		}
	}
}