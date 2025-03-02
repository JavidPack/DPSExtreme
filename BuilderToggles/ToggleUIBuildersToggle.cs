using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace DPSExtreme.BuilderToggles
{
	public class ToggleUIBuildersToggle : BuilderToggle
	{
		public static LocalizedText OnText { get; private set; }
		public static LocalizedText OffText { get; private set; }

		public override bool Active() => true;

		public override int NumberOfStates => 2;

		public override void SetStaticDefaults() {
			OnText = this.GetLocalization(nameof(OnText));
			OffText = this.GetLocalization(nameof(OffText));
		}

		public override string DisplayValue() {
			return CurrentState == 0 ? OnText.Value : OffText.Value;
		}

		public override bool OnLeftClick(ref SoundStyle? sound) {
			DPSExtremeUI.instance.ShowTeamDPSPanel = !DPSExtremeUI.instance.ShowTeamDPSPanel;

			// Should hotkey and chat also do this? Chat already has a sound from pressing enter in chat.
			if (DPSExtremeUI.instance.ShowTeamDPSPanel) {
				SoundEngine.PlaySound(SoundID.MenuOpen);
			}
			else {
				SoundEngine.PlaySound(SoundID.MenuClose);
			}

			return false; // ShowTeamDPSPanel already handles changing CurrentState, don't want to do it twice
		}

		public override bool Draw(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams) {
			drawParams.Frame = drawParams.Texture.Frame(1, 2, 0, CurrentState);
			return base.Draw(spriteBatch, ref drawParams);
		}
	}
}
