
using DPSExtreme.Config;
using DPSExtreme.UIElements.Displays;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace DPSExtreme.UIElements
{
	internal class UIStatInfoPopup : UIElement
	{
		internal UIStatInfoPopup() {
			PaddingTop = 6;
			PaddingBottom = 6;
			PaddingLeft = 10;
			PaddingRight = 10;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			UIStatDisplayEntry entry = DPSExtremeUI.instance.myCurrentDisplay.GetElementAt(Main.MouseScreen) as UIStatDisplayEntry;
			if (entry == null)
				return;

			if (entry.myInfoBoxLines.Count == 0)
				return;

			float panelWidth = 0;
			float panelHeight = 0;

			float textScale = 0.6f;

			DynamicSpriteFont fontMouseText = FontAssets.MouseText.Value;

			for (int i = 0; i < entry.myInfoBoxLines.Count; i++) {
				Vector2 textDimensions = fontMouseText.MeasureString(entry.myInfoBoxLines[i]);

				panelWidth = Math.Max(panelWidth, textDimensions.X * textScale);
				panelHeight += textDimensions.Y * textScale * 0.8f;
			}

			panelWidth += PaddingLeft + PaddingRight;
			panelHeight += PaddingTop + PaddingBottom;

			Rectangle rootPanelRect = DPSExtremeUI.instance.myRootPanel.GetOuterDimensions().ToRectangle();
			Vector2 panelPos = new Vector2(rootPanelRect.Right - panelWidth, rootPanelRect.Top - panelHeight);

			if (DPSExtremeClientConfig.Instance.SnapAdditionalInfoBoxToMouse)
				panelPos = new Vector2(Main.MouseScreen.X, Main.MouseScreen.Y - panelHeight);

			Rectangle destRect = new Rectangle((int)panelPos.X, (int)panelPos.Y, (int)panelWidth, (int)panelHeight);
			spriteBatch.Draw(DPSExtremeUI.instance.playerBackGroundTexture.Value, destRect, Color.White);

			int lineHeight = (int)(fontMouseText.MeasureString("A").Y * textScale * 0.8f);

			for (int i = 0; i < entry.myInfoBoxLines.Count; i++) {
				string line = entry.myInfoBoxLines[i];
				Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, fontMouseText, line, destRect.TopLeft() + new Vector2(PaddingLeft, PaddingTop + (lineHeight * i)), Color.White, 0f,
					new Vector2(0, 0), new Vector2(textScale), -1f, 1.5f);
			}
		}
	}
}