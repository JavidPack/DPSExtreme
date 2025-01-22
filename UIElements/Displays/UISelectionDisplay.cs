using System;
using Terraria.GameInput;
using Terraria.UI;

namespace DPSExtreme.UIElements.Displays
{
	internal abstract class UISelectionDisplay : UIDisplay
	{
		internal UISelectionDisplay(ListDisplayMode aDisplayMode, Func<int, string> aNameCallback)
			: base(aDisplayMode) {
			myNameCallback = aNameCallback;

			myClickEntryCallback += OnClickEntry;
			myEntryCreator = () => { return new UISelectionDisplayEntry(); };
		}

		internal override void Update() {
			PopulateEntries();
		}

		protected abstract void PopulateEntries();
		protected abstract void OnSelect(int aSelectedIndex);

		protected void OnClickEntry(UIMouseEvent evt, UIElement listeningElement) {
			UISelectionDisplayEntry entry = listeningElement as UISelectionDisplayEntry;
			if (entry == null)
				return;

			OnSelect(entry.myIndex);
			Clear();
		}
	}
}