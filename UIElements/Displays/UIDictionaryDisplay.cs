using DPSExtreme.Combat.Stats;
using DPSExtreme.Config;
using System;
using Terraria.ID;

namespace DPSExtreme.UIElements.Displays
{
	internal class UIStatDictionaryDisplay<T> : UICombatInfoDisplay
		where T : IStatContainer, new()
	{
		internal override DisplayContainerType myContainerType => DisplayContainerType.Dictionary;

		internal DPSExtremeStatDictionary<int, T> myInfoLookup {
			get {
				try {
					if (myParentDisplay != null) {
						if (myParentDisplay.myContainerType == DisplayContainerType.List) {
							UIListDisplay<DPSExtremeStatDictionary<int, T>> parent = myParentDisplay as UIListDisplay<DPSExtremeStatDictionary<int, T>>;
							if (parent != null)
								return parent.myInfoList[myParentDisplay.myBreakdownAccessor];
						}
						else if (myParentDisplay.myContainerType == DisplayContainerType.Dictionary) {
							UIStatDictionaryDisplay<DPSExtremeStatDictionary<int, T>> parent = myParentDisplay as UIStatDictionaryDisplay<DPSExtremeStatDictionary<int, T>>;
							if (parent != null)
								return parent.myInfoLookup[myParentDisplay.myBreakdownAccessor];
						}
					}

					return DPSExtremeUI.instance.myDisplayedCombat?.GetInfoContainer(myDisplayMode) as DPSExtremeStatDictionary<int, T>;
				}
				catch (Exception) {

					throw;
				}
			}
		}

		public UIStatDictionaryDisplay(ListDisplayMode aDisplayMode)
			: base(aDisplayMode, typeof(T)) {
			myNameCallback = DamageSource.GetAbilityName;
		}

		internal override void Update() {
			if (myInfoLookup == null)
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

			myHighestValue = 0;
			myTotal = 0;

			foreach ((int npcType, T damageInfo) in myInfoLookup) {
				int listMax = 0;
				int listTotal = 0;

				damageInfo.GetMaxAndTotal(out listMax, out listTotal);
				myHighestValue = Math.Max(myHighestValue, listTotal); //Yes, listTotal is correct
				myTotal += listTotal;
			}

			if (myFormat == StatFormat.Time)
				myTotal = DPSExtremeUI.instance.myDisplayedCombat.myDurationInTicks;
		}

		internal override void UpdateValues() {
			if (myIsInBreakdown) {
				myBreakdownDisplay.UpdateValues();
				return;
			}

			if (!myInfoLookup.HasStats()) {
				Clear();
				return;
			}

			int entryIndex = 0;

			foreach ((int baseKey, T damageInfo) in myInfoLookup) {
				int listMax = 0;
				int listTotal = 0;

				damageInfo.GetMaxAndTotal(out listMax, out listTotal);

				if (listTotal > 0) {
					//Filter out critter entries if config is set
					if (DPSExtremeServerConfig.Instance.IgnoreCritters) {
						if (baseKey <= (int)DamageSource.SourceType.NPCEnd) {
							if (ContentSamples.NpcsByNetId[baseKey].CountsAsACritter)
								continue;
						}
					}

					string name = "Missing name callback";
					if (baseKey == -1 && myParentDisplay != null) //not breakdownable. Show same name as parent entry
					{
						name = myParentDisplay.myNameCallback.Invoke(myParentDisplay.myBreakdownAccessor);
					}
					else if (myNameCallback != null) {
						name = myNameCallback.Invoke(baseKey);
					}

					if (name.Length == 0)
						name = $"Error - id: {baseKey}";

					UIStatDisplayEntry entry = CreateEntry(entryIndex) as UIStatDisplayEntry;
					entry.myColor = DPSExtremeUI.chatColor[Math.Abs(baseKey) % DPSExtremeUI.chatColor.Length];
					entry.myNameText = name;
					entry.myBaseKey = baseKey;
					entry.myInfoBoxLines = damageInfo.GetInfoBoxLines();
					entry.SetValues(listTotal, myHighestValue, myTotal);
					entryIndex++;
				}
			}

			//In case no new entries were added but they need to be re-sorted
			UpdateOrder();
		}
	}
}