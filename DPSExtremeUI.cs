using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using System;
using Terraria.UI;
using Terraria.ModLoader.UI.Elements;
using Terraria.GameContent.UI.Elements;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics;
using DPSExtreme.UIElements;

namespace DPSExtreme
{
	internal class DPSExtremeUI : UIModState
	{
		internal static DPSExtremeUI instance;

		internal UIDragablePanel teamDPSPanel;
		internal UIText label;
		internal UIGrid dpsList;
		internal UIGrid bossList;

		internal bool showPercent = true;
		internal bool showDPSPanel = true;
		internal int drawPlayer = -1;

		private bool showTeamDPSPanel;
		public bool ShowTeamDPSPanel
		{
			get { return showTeamDPSPanel; }
			set
			{
				if (value)
				{
					Append(teamDPSPanel);
				}
				else
				{
					RemoveChild(teamDPSPanel);
				}
				showTeamDPSPanel = value;
				if (value)
					updateNeeded = true;
			}
		}

		internal static Color[] chatColor = new Color[]{
			Color.LightBlue,
			Color.LightCoral,
			Color.LightCyan,
			Color.LightGoldenrodYellow,
			Color.LightGray,
			Color.LightPink,
			Color.LightSkyBlue,
			Color.LightYellow
		};

		public DPSExtremeUI(UserInterface ui) : base(ui)
		{
			instance = this;
		}

		Texture2D playerBackGroundTexture;
		public override void OnInitialize()
		{
			playerBackGroundTexture = TextureManager.Load("Images/UI/PlayerBackground");

			teamDPSPanel = new UIDragablePanel();
			teamDPSPanel.SetPadding(6);
			teamDPSPanel.Left.Set(-310f, 0f);
			teamDPSPanel.HAlign = 1f;
			teamDPSPanel.Top.Set(90f, 0f);
			teamDPSPanel.Width.Set(415f, 0f);
			teamDPSPanel.MinWidth.Set(50f, 0f);
			teamDPSPanel.MaxWidth.Set(500f, 0f);
			teamDPSPanel.Height.Set(350, 0f);
			teamDPSPanel.MinHeight.Set(50, 0f);
			teamDPSPanel.MaxHeight.Set(300, 0f);
			teamDPSPanel.BackgroundColor = new Color(73, 94, 171);
			//Append(favoritePanel);

			label = new UIText("DPS");
			label.OnClick += Label_OnClick;
			teamDPSPanel.Append(label);
			teamDPSPanel.AddDragTarget(label);

			//var togglePercentButton = new UIHoverImageButton(Main.itemTexture[ItemID.SuspiciousLookingEye], "Toggle %");
			var togglePercentButton = new UIHoverImageButton(DPSExtreme.instance.GetTexture("PercentButton"), "Toggle %");
			togglePercentButton.OnClick += (a, b) => showPercent = !showPercent;
			togglePercentButton.Left.Pixels = 174;
			togglePercentButton.Top.Pixels = -4;
			//toggleCompletedButton.Top.Pixels = spacing;
			teamDPSPanel.Append(togglePercentButton);

			var labelDimensions = label.GetInnerDimensions();
			int top = (int)labelDimensions.Height + 4;

			dpsList = new UIGrid();
			dpsList.Width.Set(0, 1f);
			dpsList.Height.Set(-top, 1f);
			dpsList.Top.Set(top, 0f);
			dpsList.ListPadding = 0f;
			teamDPSPanel.Append(dpsList);
			teamDPSPanel.AddDragTarget(dpsList);

			var type = Assembly.GetAssembly(typeof(Mod)).GetType("Terraria.ModLoader.UI.Elements.UIGrid");
			FieldInfo loadModsField = type.GetField("_innerList", BindingFlags.Instance | BindingFlags.NonPublic);
			teamDPSPanel.AddDragTarget((UIElement)loadModsField.GetValue(dpsList)); // list._innerList

			bossList = new UIGrid();
			bossList.Width.Set(0, 1f);
			bossList.Height.Set(-top, 1f);
			bossList.Top.Set(top, 0f);
			bossList.ListPadding = 0f;
			//teamDPSPanel.Append(bossList);
			teamDPSPanel.AddDragTarget(bossList);
			teamDPSPanel.AddDragTarget((UIElement)loadModsField.GetValue(bossList));

			var scrollbar = new InvisibleFixedUIScrollbar(userInterface);
			scrollbar.SetView(100f, 1000f);
			scrollbar.Height.Set(0, 1f);
			scrollbar.Left.Set(-20, 1f);
			teamDPSPanel.Append(scrollbar);
			dpsList.SetScrollbar(scrollbar);

			scrollbar = new InvisibleFixedUIScrollbar(userInterface);
			scrollbar.SetView(100f, 1000f);
			scrollbar.Height.Set(0, 1f);
			scrollbar.Left.Set(-20, 1f);
			teamDPSPanel.Append(scrollbar);
			bossList.SetScrollbar(scrollbar);

			//updateNeeded = true;
		}

		internal bool updateNeeded;
		internal bool bossUpdateNeeded;
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			//drawPlayer = -1;
			if (!updateNeeded) { return; }
			updateNeeded = false;
			UpdateDamageLists();
		}

		internal void UpdateDamageLists()
		{
			//ShowFavoritePanel = favoritedRecipes.Count > 0;
			//	teamDPSPanel.RemoveAllChildren();

			//UIText label = new UIText("DPS");
			//label.OnClick += Label_OnClick;
			//teamDPSPanel.Append(label);

			//label.Recalculate();
			var labelDimensions = label.GetInnerDimensions();
			int top = (int)labelDimensions.Height + 4;
			if (showDPSPanel)
			{
				dpsList.Clear();
				int width = 1;
				int height = 0;
				float max = 1f;
				int total = 0;
				for (int i = 0; i < DPSExtreme.dpss.Length; i++)
				{
					int playerDPS = DPSExtreme.dpss[i];
					if (playerDPS > -1)
					{
						max = Math.Max(max, playerDPS);
						total += playerDPS;
					}
				}
				for (int i = 0; i < DPSExtreme.dpss.Length; i++)
				{
					int playerDPS = DPSExtreme.dpss[i];
					if (playerDPS > -1)
					{
						UIPlayerDPS t = new UIPlayerDPS(i, "", "");
						t.SetDPS(playerDPS, max, total);
						t.Recalculate();
						var inner = t.GetInnerDimensions();
						t.Width.Set(200, 0);
						height += (int)(inner.Height + dpsList.ListPadding);
						width = Math.Max(width, (int)inner.Width);
						dpsList.Add(t);
						teamDPSPanel.AddDragTarget(t);
					}
				}

				dpsList.Recalculate();
				var fff = dpsList.GetTotalHeight();

				width = 200;
				teamDPSPanel.Height.Pixels = top + /*height*/ fff + teamDPSPanel.PaddingBottom + teamDPSPanel.PaddingTop - dpsList.ListPadding;
				teamDPSPanel.Width.Pixels = width + teamDPSPanel.PaddingLeft + teamDPSPanel.PaddingRight;
				teamDPSPanel.Recalculate();
			}
			else
			{

				bossList.Clear();

				int height = 0;
				int max = 1;
				int total = 0;
				for (int i = 0; i < DPSExtreme.bossDamage.Length; i++)
				{
					int playerBossDamage = DPSExtreme.bossDamage[i];
					if (playerBossDamage > -1)
					{
						max = Math.Max(max, playerBossDamage);
						total += playerBossDamage;
					}
				}
				for (int i = 0; i < DPSExtreme.dpss.Length; i++)
				{
					int playerBossDamage = DPSExtreme.bossDamage[i];
					if (playerBossDamage > -1)
					{
						UIPlayerDPS t = new UIPlayerDPS(i, "", "");
						t.SetDPS(playerBossDamage, max, total);
						t.Recalculate();
						var inner = t.GetInnerDimensions();
						t.Width.Set(200, 0);
						height += (int)(inner.Height + bossList.ListPadding);
						bossList.Add(t);
						teamDPSPanel.AddDragTarget(t);
					}
				}

				if (bossUpdateNeeded)
				{
					string bossname = "No Boss";
					if (DPSExtreme.bossIndex > -1)
						bossname = Lang.GetNPCNameValue(Main.npc[DPSExtreme.bossIndex].type);
					label.SetText("Boss - " + bossname);
					bossUpdateNeeded = false;
				}

				bossList.Recalculate();
				var fff = bossList.GetTotalHeight();
				teamDPSPanel.Height.Pixels = top + /*height*/ fff + teamDPSPanel.PaddingBottom + teamDPSPanel.PaddingTop - dpsList.ListPadding;
				teamDPSPanel.Width.Pixels = 200 + teamDPSPanel.PaddingLeft + teamDPSPanel.PaddingRight;
				teamDPSPanel.Recalculate();
			}
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			if (drawPlayer > -1)
			{
				Rectangle hitbox = DPSExtremeUI.instance.teamDPSPanel.GetOuterDimensions().ToRectangle();
				Rectangle r2 = new Rectangle(hitbox.X + hitbox.Width / 2 - 58 / 2, hitbox.Y - 58, 58, 58);
				spriteBatch.Draw(playerBackGroundTexture, r2.TopLeft(), Color.White);
				Main.instance.DrawPlayer(Main.player[drawPlayer], Main.screenPosition + r2.Center.ToVector2() + new Vector2(-10, -21), 0, Vector2.Zero);
			}
			drawPlayer = -1;
		}

		private void Label_OnClick(UIMouseEvent evt, UIElement listeningElement)
		{
			UIText text = (evt.Target as UIText);
			showDPSPanel = !showDPSPanel;
			if (showDPSPanel)
			{
				text.SetText("DPS");
				teamDPSPanel.RemoveChild(bossList);
				teamDPSPanel.Append(dpsList);
			}
			else
			{
				string bossname = "No Boss";
				if (DPSExtreme.bossIndex > -1)
					bossname = Lang.GetNPCNameValue(Main.npc[DPSExtreme.bossIndex].type);
				text.SetText("Boss - " + bossname);
				teamDPSPanel.RemoveChild(dpsList);
				teamDPSPanel.Append(bossList);
			}
			updateNeeded = true;
		}
	}
}

