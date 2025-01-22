using DPSExtreme.Combat.Stats;
using System;
using Terraria.UI;

namespace DPSExtreme.UIElements.Displays
{


	internal abstract class UICombatInfoDisplay : UIDisplay
	{
		internal enum DisplayContainerType
		{
			Dictionary,
			List
		}

		internal int myHighestValue = -1;
		internal int myTotal = 0;

		internal UICombatInfoDisplay myBreakdownDisplay = null;
		internal int myBreakdownAccessor = -1;
		protected bool myIsInBreakdown {
			get { return myBreakdownAccessor != -1; }
		}

		internal abstract DisplayContainerType myContainerType { get; }

		internal UICombatInfoDisplay myParentDisplay = null;

		internal UICombatInfoDisplay(ListDisplayMode aDisplayMode, System.Type aContainerType)
			: base(aDisplayMode) {
			myClickEntryCallback += OnClickBaseEntry;
			myEntryCreator = () => { return new UIStatDisplayEntry(); };

			if (aContainerType == null)
				return;

			if (aContainerType == typeof(StatValue) ||
				aContainerType.IsSubclassOf(typeof(StatValue))) {
				if (aContainerType == typeof(TimeStatValue))
					myFormat = StatFormat.Time;

				UICombatInfoDisplay parent = myParentDisplay;
				while (parent != null) {
					parent.myFormat = myFormat;
					parent = parent.myParentDisplay;
				}

				return;
			}

			System.Type[] typeArguments = aContainerType.GetGenericArguments();
			System.Type nextContainerType = typeArguments[typeArguments.Length - 1];

			if (aContainerType.GetGenericTypeDefinition() == typeof(DPSExtremeStatDictionary<,>)) {
				Type nextDisplayType = typeof(UIStatDictionaryDisplay<>).MakeGenericType(nextContainerType);
				AddBreakdown((UICombatInfoDisplay)Activator.CreateInstance(nextDisplayType, myDisplayMode));
			}
			else if (aContainerType.GetGenericTypeDefinition() == typeof(DPSExtremeStatList<>)) {
				Type nextDisplayType = typeof(UIListDisplay<>).MakeGenericType(nextContainerType);
				AddBreakdown((UICombatInfoDisplay)Activator.CreateInstance(nextDisplayType, myDisplayMode, myFormat));
			}
		}

		internal abstract void RecalculateTotals();
		internal abstract void UpdateValues();

		internal UICombatInfoDisplay AddBreakdown(UICombatInfoDisplay aDisplay) {
			if (myBreakdownDisplay != null) {
				myBreakdownDisplay.AddBreakdown(aDisplay);
				return aDisplay;
			}

			aDisplay.myParentDisplay = this;
			myBreakdownDisplay = aDisplay;
			myBreakdownDisplay.OnRightClick += OnRightClickBreakdownDisplay;

			return aDisplay;
		}

		internal void EnterBreakdown(UIStatDisplayEntry aEntry) {
			if (aEntry.myBaseKey != -1)
				myBreakdownAccessor = aEntry.myBaseKey;
			else if (aEntry.myParticipantIndex != -1)
				myBreakdownAccessor = aEntry.myParticipantIndex;

			Clear();
			Add(myBreakdownDisplay);

			if (myNameCallback != null)
				myLabelOverride = myNameCallback(myBreakdownAccessor);

			DPSExtremeUI.instance.RefreshLabel();
			DPSExtremeUI.instance.updateNeeded = true;
		}

		protected void OnClickBaseEntry(UIMouseEvent evt, UIElement listeningElement) {
			if (myBreakdownDisplay == null)
				return;

			UIStatDisplayEntry entry = listeningElement as UIStatDisplayEntry;
			EnterBreakdown(entry);
		}

		protected void OnRightClickBreakdownDisplay(UIMouseEvent evt, UIElement listeningElement) {
			myBreakdownDisplay.Clear();
			Remove(myBreakdownDisplay);

			myLabelOverride = null;
			myBreakdownAccessor = -1;

			DPSExtremeUI.instance.RefreshLabel();
			DPSExtremeUI.instance.updateNeeded = true;
		}
	}
}