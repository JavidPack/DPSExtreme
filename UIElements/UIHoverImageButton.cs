using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader.UI;

namespace DPSExtreme.UIElements
{
	internal class UIHoverImageButton : UIImageButton
	{
		internal string hoverText;

		public UIHoverImageButton(Asset<Texture2D> texture, string hoverText) : base(texture) {
			this.hoverText = hoverText;
			System.Reflection.FieldInfo fieldInfo = GetType().BaseType.GetField("_visibilityInactive", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			fieldInfo.SetValue(this, 0.6f);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			if (IsMouseHovering) {
				float mouseTextPulse = Main.mouseTextColor / 255f;
				UICommon.TooltipMouseText($"[c/{Utils.Hex3(Colors.RarityYellow * mouseTextPulse)}:{hoverText}]");
			}
		}
	}
}