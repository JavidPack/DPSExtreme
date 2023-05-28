/* Experiment: Want to toggle the UI by clicking the text, but no such hook.
using Terraria.ModLoader;

namespace DPSExtreme
{
	class DPSExtremeGlobalInfoDisplay : GlobalInfoDisplay {
		public override void ModifyDisplayValue(InfoDisplay currentDisplay, ref string displayValue) {
			// TODO: Some way to toggle this directly, this doesn't work. Maybe check accDreamCatcher
			if(currentDisplay == InfoDisplay.DPSMeter) {
				displayValue = displayValue + " [i:23]";
			}
		}
	}
}
*/

