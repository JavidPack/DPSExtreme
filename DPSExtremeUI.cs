using DPSExtreme.CombatTracking;
using DPSExtreme.UIElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using System;
using System.Reflection;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DPSExtreme
{
	internal class DPSExtremeUI : UIModState
	{
		internal static DPSExtremeUI instance;

		internal DPSExtremeCombat myDisplayedCombat = null;

		internal UIDragablePanel teamDPSPanel;
		internal UIText label;
		internal UIGrid dpsList;
		internal UIGrid bossList;

		internal bool showPercent = true;
		internal bool showDPSPanel = false;
		internal int drawPlayer = -1;

				return _myDisplayedCombat;
			}
			set {
				_myDisplayedCombat = value;

				SetupDisplays();
				updateNeeded = true;
			}
		}

		internal UIDragablePanel myRootPanel;
		internal UIText myLabel;

		internal UIStatInfoPopup myStatInfoPopup = new UIStatInfoPopup();

		internal UIListDisplay<StatValue> myNeedDPSAccDisplay;
		internal UISelectDisplayModeDisplay mySelectDisplayModeDisplay;
		internal UICombatHistoryDisplay myCombatHistoryDisplay;

		internal UIListDisplay<StatValue> myDamagePerSecondDisplay;
		internal UIListDisplay<DPSExtremeStatDictionary<int, DamageStatValue>> myDamageDoneDisplay;
		internal UIListDisplay<DPSExtremeStatDictionary<int, MinionDamageStatValue>> myMinionDamageDoneDisplay;
		internal UIListDisplay<DPSExtremeStatDictionary<int, DPSExtremeStatDictionary<int, DamageStatValue>>> myDamageTakenDisplay;
		internal UIListDisplay<DeathStatValue> myDeathsDisplay;
		internal UIListDisplay<DPSExtremeStatDictionary<int, StatValue>> myKillsDisplay;
		internal UIListDisplay<DPSExtremeStatDictionary<int, StatValue>> myManaUsedDisplay;
		internal UIListDisplay<DPSExtremeStatDictionary<int, TimeStatValue>> myBuffUptimesDisplay;
		internal UIListDisplay<DPSExtremeStatDictionary<int, TimeStatValue>> myDebuffUptimesDisplay;

		internal UIStatDictionaryDisplay<DPSExtremeStatList<DPSExtremeStatDictionary<int, DamageStatValue>>> myEnemyDamageTakenDisplay;

		internal ListDisplayMode myPreviousDisplayMode = ListDisplayMode.DamageDone;

		private ListDisplayMode _myDisplayMode;
		internal ListDisplayMode myDisplayMode {
			get { return _myDisplayMode; }
			set {
				myRootPanel.RemoveChild(myCurrentDisplay);

				myPreviousDisplayMode = _myDisplayMode;
				_myDisplayMode = value;

				myRootPanel.Append(myCurrentDisplay);
				RefreshLabel();
				updateNeeded = true;
			}
		}

		internal UIDisplay myCurrentDisplay {
			get {
				switch (myDisplayMode) {
					case ListDisplayMode.NeedAccessory:
						return myNeedDPSAccDisplay;
					case ListDisplayMode.DisplayModeSelect:
						return mySelectDisplayModeDisplay;
					case ListDisplayMode.CombatHistory:
						return myCombatHistoryDisplay;
					case ListDisplayMode.StatDisplaysStart:
					case ListDisplayMode.StatDisplaysEnd:
					case ListDisplayMode.DamagePerSecond:
						return myDamagePerSecondDisplay;
					case ListDisplayMode.DamageDone:
						return myDamageDoneDisplay;
					case ListDisplayMode.MinionDamageDone:
						return myMinionDamageDoneDisplay;
					case ListDisplayMode.DamageTaken:
						return myDamageTakenDisplay;
					case ListDisplayMode.EnemyDamageTaken:
						return myEnemyDamageTakenDisplay;
					case ListDisplayMode.Deaths:
						return myDeathsDisplay;
					case ListDisplayMode.Kills:
						return myKillsDisplay;
					case ListDisplayMode.ManaUsed:
						return myManaUsedDisplay;
					case ListDisplayMode.BuffUptime:
						return myBuffUptimesDisplay;
					case ListDisplayMode.DebuffUptime:
						return myDebuffUptimesDisplay;
					default:
						return null;
				}
			}
			set { myCurrentDisplay = value; }
		}

		internal int myHoveredParticipant = -1;

		private bool showTeamDPSPanel;
		public bool ShowTeamDPSPanel {
			get { return showTeamDPSPanel; }
			set {
				if (value) {
					Append(teamDPSPanel);
				}
				else {
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
			Color.LightSeaGreen,
			Color.LightSkyBlue,
			Color.LightSteelBlue,
			Color.Linen,
			Color.MediumPurple,
			Color.MediumVioletRed,
			Color.MistyRose,
			Color.Olive,
			Color.Plum,
			Color.Salmon,
			Color.Orange,
			Color.Silver,
			Color.Tan,
			Color.LightYellow
		};

		public DPSExtremeUI(UserInterface ui) : base(ui) {
			instance = this;
		}

		Asset<Texture2D> playerBackGroundTexture;
		public override void OnInitialize() {
			playerBackGroundTexture = Main.Assets.Request<Texture2D>("Images/UI/PlayerBackground");

			//TODO: Save window position etc
			teamDPSPanel = new UIDragablePanel();
			teamDPSPanel.SetPadding(6);
			teamDPSPanel.Left.Set(-310f, 0f);
			teamDPSPanel.HAlign = 1;
			teamDPSPanel.Top.Set(90f, 0f);
			teamDPSPanel.Width.Set(415f, 0f);
			teamDPSPanel.MinWidth.Set(50f, 0f);
			teamDPSPanel.MaxWidth.Set(500f, 0f);
			teamDPSPanel.Height.Set(350, 0f);
			teamDPSPanel.MinHeight.Set(50, 0f);
			teamDPSPanel.MaxHeight.Set(300, 0f);
			teamDPSPanel.BackgroundColor = new Color(73, 94, 171);

			label = new UIText("", 0.8f);
			//Figure out why tf this doesn't work
			label.DynamicallyScaleDownToWidth = true;
			label.MaxWidth.Set(50, 0);

			label.OnLeftClick += Label_OnClick;
			teamDPSPanel.Append(label);
			teamDPSPanel.AddDragTarget(label);

			RefreshLabel();

			//var togglePercentButton = new UIHoverImageButton(Main.itemTexture[ItemID.SuspiciousLookingEye], "Toggle %");
			var togglePercentButton = new UIHoverImageButton(DPSExtreme.instance.Assets.Request<Texture2D>("PercentButton", AssetRequestMode.ImmediateLoad), Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("TogglePercent")));
			togglePercentButton.OnLeftClick += (a, b) => showPercent = !showPercent;
			togglePercentButton.Left.Set(-24, 1f);
			togglePercentButton.Top.Pixels = -4;
			//toggleCompletedButton.Top.Pixels = spacing;
			teamDPSPanel.Append(togglePercentButton);

			myLabel.Left.Pixels = 18;
			myLabel.Top.Pixels = 2;
			myLabel.OnLeftClick += Label_OnLeftClick;
			myLabel.OnRightClick += Label_OnRightClick;
			myRootPanel.Append(myLabel);
			myRootPanel.AddDragTarget(myLabel);

			dpsList = new UIGrid();
			dpsList.Width.Set(0, 1f);
			dpsList.Height.Set(-top, 1f);
			dpsList.Top.Set(top, 0f);
			dpsList.ListPadding = 0f;

			if (showDPSPanel)
				teamDPSPanel.Append(dpsList);

			teamDPSPanel.AddDragTarget(dpsList);


			bossList = new UIGrid();
			bossList.Width.Set(0, 1f);
			bossList.Height.Set(-top, 1f);
			bossList.Top.Set(top, 0f);
			bossList.ListPadding = 0f;

			if (!showDPSPanel)
				teamDPSPanel.Append(bossList);

			teamDPSPanel.AddDragTarget(bossList);
			teamDPSPanel.AddDragTarget((UIElement)loadModsField.GetValue(bossList));

			var combatHistoryButton = new UIHoverImageButton(DPSExtreme.instance.Assets.Request<Texture2D>("HistoryButton", AssetRequestMode.ImmediateLoad), Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("ShowCombatHistory")));
			combatHistoryButton.OnLeftClick += (a, b) => {
				if (myDisplayMode != ListDisplayMode.CombatHistory)
					myDisplayMode = ListDisplayMode.CombatHistory;
				else
					myDisplayMode = myPreviousDisplayMode;
			};
			combatHistoryButton.Left.Set(-16, 1f);
			combatHistoryButton.Top.Pixels = -1;
			combatHistoryButton.Recalculate();
			myRootPanel.Append(combatHistoryButton);

			ShowTeamDPSPanel = true;
			myDisplayMode = ListDisplayMode.DamageDone;
		}

		internal bool updateNeeded;

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			//drawPlayer = -1;
			if (!updateNeeded) { return; }
			updateNeeded = false;
			UpdateDamageLists();
		}

		internal void UpdateDamageLists() {
			//ShowFavoritePanel = favoritedRecipes.Count > 0;
			//	teamDPSPanel.RemoveAllChildren();

			mySelectDisplayModeDisplay = new UISelectDisplayModeDisplay();
			myCombatHistoryDisplay = new UICombatHistoryDisplay();

			//label.Recalculate();
			var labelDimensions = label.GetInnerDimensions();
			int top = (int)labelDimensions.Height + 4;
			if (showDPSPanel) {
				dpsList.Clear();
				int width = 1;
				int height = 0;
				float max = 1f;
				int total = 0;

				if (myDisplayedCombat != null) {
					for (int i = 0; i < myDisplayedCombat.myDPSList.Size(); i++) {
						int playerDPS = myDisplayedCombat.myDPSList[i].myDamage;
						if (playerDPS > 0) {
							max = Math.Max(max, playerDPS);
							total += playerDPS;
						}
					}

					for (int i = 0; i < myDisplayedCombat.myDPSList.Size(); i++) {
						int playerDPS = myDisplayedCombat.myDPSList[i].myDamage;
						if (playerDPS > 0) {
							UIPlayerDPS t = new UIPlayerDPS(i);
							t.SetDPS(playerDPS, max, total);
							t.Recalculate();
							var inner = t.GetInnerDimensions();
							t.Width.Set(250, 0);
							height += (int)(inner.Height + dpsList.ListPadding);
							width = Math.Max(width, (int)inner.Width);
							dpsList.Add(t);
							teamDPSPanel.AddDragTarget(t);
						}
					}

					if (!Main.LocalPlayer.accDreamCatcher) {
						UIText t = new UIText(Language.GetText(DPSExtreme.instance.GetLocalizationKey("NoDPSWearDPSMeter")));
						dpsList.Add(t);
						teamDPSPanel.AddDragTarget(t);
					}
				}


				dpsList.Recalculate();
				var fff = dpsList.GetTotalHeight();

			if (myDisplayedCombat == null ||
				myDisplayMode < ListDisplayMode.StatDisplaysStart) {
				myLabel.SetText(title);
				myLabel.Recalculate();
				return;
			}
			else {
				bossList.Clear();

				int height = 0;
				int max = 1;
				int total = 0;
				if (myDisplayedCombat != null) {
					for (int i = 0; i < myDisplayedCombat.myTotalDamageDealtList.Size(); i++) {
						int damageDealt = myDisplayedCombat.myTotalDamageDealtList[i].myDamage;
						if (damageDealt > -1) {
							max = Math.Max(max, damageDealt);
							total += damageDealt;
						}
					}

					for (int i = 0; i < myDisplayedCombat.myTotalDamageDealtList.Size(); i++) {
						int damageDealt = myDisplayedCombat.myTotalDamageDealtList[i].myDamage;
						if (damageDealt > -1) {
							UIPlayerDPS t = new UIPlayerDPS(i);
							t.SetDPS(damageDealt, max, total);
							t.Recalculate();
							var inner = t.GetInnerDimensions();
							t.Width.Set(250, 0);
							height += (int)(inner.Height + bossList.ListPadding);
							bossList.Add(t);
							teamDPSPanel.AddDragTarget(t);
						}
					}
				}

				bossList.Recalculate();
				var fff = bossList.GetTotalHeight();
				teamDPSPanel.Height.Pixels = top + /*height*/ fff + teamDPSPanel.PaddingBottom + teamDPSPanel.PaddingTop - dpsList.ListPadding;
				teamDPSPanel.Width.Pixels = 250 + teamDPSPanel.PaddingLeft + teamDPSPanel.PaddingRight;
				teamDPSPanel.Recalculate();
			}
		}

		internal void RefreshLabel() {
			string title = null;

			if (showDPSPanel)
				title = Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("DPS"));
			else
				title = Language.GetTextValue(DPSExtreme.instance.GetLocalizationKey("DamageDone"));

			if (myDisplayedCombat == null) {
				label.SetText(title);
				label.Recalculate();
				return;
			}

			title += " - ";

			switch (myDisplayedCombat.myHighestCombatType) {
				case DPSExtremeCombat.CombatType.BossFight:
					if (myDisplayedCombat.myBossOrInvasionOrEventType > -1) {
						string bossName = Lang.GetNPCNameValue(myDisplayedCombat.myBossOrInvasionOrEventType);
						title += Language.GetText(bossName);
					}
					else {
						title += Language.GetText(DPSExtreme.instance.GetLocalizationKey("NoBoss")).Value;
					}

					break;
				case DPSExtremeCombat.CombatType.Invasion:
					DPSExtremeCombat.InvasionType invasionType;
					if (myDisplayedCombat.myBossOrInvasionOrEventType >= (int)DPSExtremeCombat.InvasionType.ModdedInvasionsStart &&
						myDisplayedCombat.myBossOrInvasionOrEventType < (int)DPSExtremeCombat.InvasionType.ModdedInvasionsEnd) {
						invasionType = DPSExtremeCombat.InvasionType.ModdedInvasionsStart;
					}
					else {
						invasionType = (DPSExtremeCombat.InvasionType)myDisplayedCombat.myBossOrInvasionOrEventType;
					}

					switch (invasionType) {
						case DPSExtremeCombat.InvasionType.GoblinArmy:
							title += Language.GetTextValue("Bestiary_Invasions.Goblins");
							break;
						case DPSExtremeCombat.InvasionType.SnowLegion:
							title += Language.GetTextValue("Bestiary_Invasions.FrostLegion");
							break;
						case DPSExtremeCombat.InvasionType.PirateInvasion:
							title += Language.GetTextValue("Bestiary_Invasions.Pirates");
							break;
						case DPSExtremeCombat.InvasionType.MartianMadness:
							title += Language.GetTextValue("Bestiary_Invasions.Martian");
							break;
						case DPSExtremeCombat.InvasionType.PumpkinMoon:
							title += Language.GetTextValue("Bestiary_Invasions.PumpkinMoon");
							break;
						case DPSExtremeCombat.InvasionType.FrostMoon:
							title += Language.GetTextValue("Bestiary_Invasions.FrostMoon");
							break;
						case DPSExtremeCombat.InvasionType.OldOnesArmy:
							title += Language.GetTextValue("Bestiary_Invasions.OldOnesArmy");
							break;
						case DPSExtremeCombat.InvasionType.ModdedInvasionsStart:
							//TODO: Boss checklist support to fetch name?
							title += Language.GetTextValue("Invasion");
							break;
						default:
							title += Language.GetTextValue("Invasion");
							break;
					}
					break;
				case DPSExtremeCombat.CombatType.Event:
					switch ((DPSExtremeCombat.EventType)myDisplayedCombat.myBossOrInvasionOrEventType) {
						case DPSExtremeCombat.EventType.BloodMoon:
							title += Language.GetTextValue("Bestiary_Events.BloodMoon");
							break;
						case DPSExtremeCombat.EventType.Eclipse:
							title += Language.GetTextValue("Bestiary_Events.Eclipse");
							break;
						case DPSExtremeCombat.EventType.SlimeRain:
							title += Language.GetTextValue("Bestiary_Events.SlimeRain");
							break;
						default:
							title += Language.GetTextValue("Event");
							break;
					}
					break;
				case DPSExtremeCombat.CombatType.Generic:
					//Maybe display name of first npc hit?
					title += Language.GetTextValue("Combat");
					break;
				default:
					title += "Unknown combat type";
					break;
			}

			label.SetText(title);
			label.Recalculate();
		}

		internal void OnCombatStarted(DPSExtremeCombat aCombat) {
			myDisplayedCombat = aCombat; //TODO: Think about what should happen if you are currently viewing history. Setting to decide if we swap instantly or not?
			RefreshLabel();
		}

		internal void OnCombatUpgraded(DPSExtremeCombat aCombat) {
			RefreshLabel();
		}

		internal void OnCombatEnded() {
			//Should we change combat view here?
			//RefreshLabel();
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			//base.DrawSelf(spriteBatch);

			bool IsPlayer = drawPlayer >= 0 && drawPlayer < (int)InfoListIndices.SupportedPlayerCount;
			bool isNPC = drawPlayer == (int)InfoListIndices.NPCs;
			if (IsPlayer || isNPC) {
				Rectangle hitbox = DPSExtremeUI.instance.teamDPSPanel.GetOuterDimensions().ToRectangle();
				Rectangle r2 = new Rectangle(hitbox.X + hitbox.Width / 2 - 58 / 2, hitbox.Y - 58, 58, 58);
				spriteBatch.Draw(playerBackGroundTexture.Value, r2.TopLeft(), Color.White);

				if (isNPC) {
					NPC drawNPC = null;
					foreach (NPC npc in Main.ActiveNPCs) {
						if (!npc.townNPC)
							continue;

						drawNPC = npc;
						break;
					}

					if (drawNPC != null) {
						drawNPC.IsABestiaryIconDummy = true;
						var position = drawNPC.position;
						drawNPC.position = r2.Center.ToVector2() + new Vector2(-10, -21);
						Main.instance.DrawNPCDirect(spriteBatch, drawNPC, drawNPC.behindTiles, Vector2.Zero);
						drawNPC.position = position;
						drawNPC.IsABestiaryIconDummy = false;
					}
				}
				else {
					Main.PlayerRenderer.DrawPlayer(Main.Camera, Main.player[myHoveredParticipant], Main.screenPosition + r2.Center.ToVector2() + new Vector2(-10, -21), 0, Vector2.Zero);
				}
			}

			drawPlayer = -1;

			if (myLabel.IsMouseHovering) {
				string hoverText = Language.GetText(DPSExtreme.instance.GetLocalizationKey("ClickToChangeDisplay")).Value;

				Item fakeItem = new Item();
				fakeItem.SetDefaults(0, noMatCheck: true);
				string textValue = Main.hoverItemName;
				fakeItem.SetNameOverride(textValue);
				fakeItem.type = ItemID.IronPickaxe;
				fakeItem.scale = 0f;
				fakeItem.rare = ItemRarityID.Yellow;
				fakeItem.value = -1;
				Main.HoverItem = fakeItem;
				Main.instance.MouseText("", 0, 0);
				Main.mouseText = true;
			}
		}

		private void Label_OnClick(UIMouseEvent evt, UIElement listeningElement) {
			UIText text = (evt.Target as UIText);
			showDPSPanel = !showDPSPanel;
			if (showDPSPanel) {
				RefreshLabel();
				teamDPSPanel.RemoveChild(bossList);
				teamDPSPanel.Append(dpsList);
			}
			else {
				RefreshLabel();
				teamDPSPanel.RemoveChild(dpsList);
				teamDPSPanel.Append(bossList);
			}
			updateNeeded = true;
		}
	}
}

		private void Label_OnRightClick(UIMouseEvent evt, UIElement listeningElement) {
			if (myDisplayMode < ListDisplayMode.StatDisplaysStart || myDisplayMode > ListDisplayMode.StatDisplaysEnd)
				return;

			int next = ((int)myDisplayMode - 1);

			if (next <= (int)ListDisplayMode.StatDisplaysStart)
				next = (int)ListDisplayMode.StatDisplaysEnd - 1;

			myDisplayMode = (ListDisplayMode)next;
		}
	}
}