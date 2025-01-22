using DPSExtreme.Combat.Stats;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DPSExtreme.UIElements.Displays
{
	internal class UIDisplayEntry : UIElement
	{
		float ourTextScale {
			get {
				float textHeight = FontAssets.MouseText.Value.MeasureString(myNameText).Y;
				float unscaledEntryHeight = textHeight + PaddingTop + PaddingBottom;

				return ourEntryHeight / unscaledEntryHeight;
			}
		}
		internal const int ourEntryHeight = 22;

		internal string myNameText;

		internal UIDisplay myParentDisplay = null;

		internal List<string> myInfoBoxLines = new List<string>();

		internal string myRightText = string.Empty;
		internal StatFormat myFormat = StatFormat.RawNumber;

		internal Color myColor;

		public UIDisplayEntry() {
			Width.Percent = 1f;
			Height.Pixels = ourEntryHeight;
			MinHeight.Pixels = ourEntryHeight;

			Recalculate();
		}

		protected virtual int GetEntryWidth() {
			return (int)GetOuterDimensions().Width;
		}

		protected void DrawSelfBase(SpriteBatch spriteBatch) {
			Rectangle hitbox = GetOuterDimensions().ToRectangle();
			hitbox.Width = GetEntryWidth();

			myColor.A = ContainsPoint(Main.MouseScreen) ? (byte)(Math.Floor(255f * 0.75f)) : (byte)255; //Kinda inverse but it looked better this way

			Texture2D backdropTex = DPSExtreme.instance.Assets.Request<Texture2D>("DisplayEntry", AssetRequestMode.ImmediateLoad).Value;
			spriteBatch.Draw(backdropTex, hitbox, myColor);

			//Parent.Children does NOT get sorted with UpdateOrder. So access the outer _items list which DOES get sorted
			List<UIElement> sortedList = (Parent.Parent as UIGrid)._items;

			int entryIndex = -1;
			int index = 0;
			for (int i = 0; i < sortedList.Count; i++) {
				if (sortedList[i] != this) {
					index++;
					continue;
				}

				entryIndex = index;
				break;
			}

			DynamicSpriteFont fontMouseText = FontAssets.MouseText.Value;

			string leftText = (entryIndex + 1).ToString() + ". " + myNameText;
			Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, fontMouseText, leftText, GetOuterDimensions().ToRectangle().TopLeft() + new Vector2(4, 3), Color.White, 0f,
				new Vector2(0, 0), new Vector2(ourTextScale), -1f, 1.5f);

			Vector2 rightTextBounds = fontMouseText.MeasureString(myRightText);
			Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(spriteBatch, fontMouseText, myRightText, GetOuterDimensions().ToRectangle().TopRight() + new Vector2(-4, 2), Color.White, 0f,
				new Vector2(1f, 0) * rightTextBounds, new Vector2(ourTextScale), -1f, 1.5f);
		}
	}
}