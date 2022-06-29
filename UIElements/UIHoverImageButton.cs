using Terraria;
using Terraria.GameContent.UI.Elements;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace DPSExtreme.UIElements
{
	internal class UIHoverImageButton : UIImageButton
	{
		internal string hoverText;

		public UIHoverImageButton(Asset<Texture2D> texture, string hoverText) : base(texture)
		{
			this.hoverText = hoverText;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			if (IsMouseHovering)
			{
				Main.hoverItemName = hoverText;

				Item fakeItem = new Item();
				fakeItem.SetDefaults(0, noMatCheck: true);
				string textValue = Main.hoverItemName;
				fakeItem.SetNameOverride(textValue);
				fakeItem.type = 1;
				fakeItem.scale = 0f;
				fakeItem.rare = 8;
				fakeItem.value = -1;
				Main.HoverItem = fakeItem;
				Main.instance.MouseText("", 0, 0);
				Main.mouseText = true;
			}
		}
	}
}

