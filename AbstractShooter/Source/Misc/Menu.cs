using InputManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using UnrealMono;

namespace AbstractShooter
{
	public class Menu
	{
		public List<MenuEntry> choices;
		public int currentChoiceIndex;
		private Menu currentMenu;
		public Menu CurrentMenu
		{
			get { return currentMenu; }
		}
		private Menu parent;
		public Menu Parent
		{
			set
			{
				parent = value;
				if (parent != null)
				{
					currentMenu = parent.currentMenu;
				}
			}
		}
		private Menu child;
		public Menu Child
		{
			set
			{
				child = value;
				Menu newCurrentMenu;
				if (child == null)
				{
					newCurrentMenu = this;
				}
				else
				{
					newCurrentMenu = child;
					child.currentMenu = child;
				}
				currentMenu = newCurrentMenu;
				Menu menu = this;
				while (menu.parent != null)
				{
					menu.parent.currentMenu = newCurrentMenu;
					menu = menu.parent;
				}
			}
		}
		public Color selectedColor = Color.White;
		public Color unselectedColor = Color.Black;
		public Texture2D backgorundScreen;
		public float startingYAlpha = 0.5F;
		public float yAlphaBetweenLines = 0.05F;
		public float stringScale = 1F;

		private Point lastCursorHoverLocation = new Point(-1, -1);
		//private List<MenuButton> buttons;

		public Menu()
		{
			choices = new List<MenuEntry>();
		}

		public Menu(List<MenuEntry> menuChoices, Color newSelectedColor, Color newUnselectedColor, Texture2D newBackgorundScreen, float newStartingYAlpha, float newYAlphaBetweenLines, float newStringScale)
		{
			currentMenu = this;
			choices = menuChoices;
			selectedColor = newSelectedColor;
			unselectedColor = newUnselectedColor;
			backgorundScreen = newBackgorundScreen;
			startingYAlpha = newStartingYAlpha;
			yAlphaBetweenLines = newYAlphaBetweenLines;
			stringScale = newStringScale;

			foreach (MenuEntry c in choices)
			{
				foreach (MenuEntryChoice sc in c.subChoices)
				{
					sc.owner = this;
				}
				c.InitButtons(stringScale, startingYAlpha + (yAlphaBetweenLines * choices.IndexOf(c)));
			}

			//buttons = new List<MenuButton>();
		}

		public bool CheckCursorHovering(Point pointScreenLocation, ref int hitChoiceIndex)
		{
			for (int i = 0; i < choices.Count; ++i)
			{
				string name = choices[i].subChoices[choices[i].CurrentSubChoiceIndex].Name;
				Vector2 stringSize = UnrealMonoGame.smallerFont.MeasureString(name) * UnrealMonoGame.defaultFontScale * stringScale * UnrealMonoGame.ResolutionScale;

				if (pointScreenLocation.X > (UnrealMonoGame.currentResolution.X / 2F) - (stringSize.X / 2F)
					&& pointScreenLocation.X < (UnrealMonoGame.currentResolution.X / 2F) + (stringSize.X / 2F)
					&& pointScreenLocation.Y > (startingYAlpha + (yAlphaBetweenLines * i)) * UnrealMonoGame.currentResolution.Y
					&& pointScreenLocation.Y < ((startingYAlpha + (yAlphaBetweenLines * i)) * UnrealMonoGame.currentResolution.Y) + stringSize.Y)
				{
					hitChoiceIndex = i;
					if (lastCursorHoverLocation != pointScreenLocation)
					{
						lastCursorHoverLocation = pointScreenLocation;
						return true;
					}
					return false;
				}
			}
			return false;
		}
		
		public bool Enter()
		{
			if (choices != null && choices.Count > 0)
			{
				choices[currentChoiceIndex].Enter();
				return true;
			}
			return Exit(); //false
		}

		public bool Exit()
		{
			if (parent != null)
			{
				parent.Child = null;
				return true;
			}
			StateManager.Unpause();
			return false;
		}

		public bool SubNext()
		{
			choices[currentChoiceIndex].Next();
			return choices[currentChoiceIndex].subChoices.Count > 1;
		}

		public bool SubPrevious()
		{
			choices[currentChoiceIndex].Previous();
			return choices[currentChoiceIndex].subChoices.Count > 1;
		}

		public bool Next()
		{
			currentChoiceIndex++;
			if (choices != null && currentChoiceIndex >= choices.Count)
				currentChoiceIndex = 0;
			return choices != null && choices.Count > 1;
		}

		public bool Previous()
		{
			currentChoiceIndex--;
			if (currentChoiceIndex < 0 && choices != null)
				currentChoiceIndex = choices.Count - 1;
			return choices != null && choices.Count > 1;
		}

		public void Reset()
		{
			if (child != null)
			{
				child.Reset();
			}
			currentChoiceIndex = 0;
		}

		public void Draw()
		{
			if (backgorundScreen != null)
			{
				UnrealMonoGame.spriteBatch.Draw(backgorundScreen, new Rectangle(0, 0, UnrealMonoGame.currentResolution.X, UnrealMonoGame.currentResolution.Y), Color.White);
			}

			for (int i = 0; i < choices.Count; ++i)
			{
				string name = choices[i].subChoices[choices[i].CurrentSubChoiceIndex].Name;
				Vector2 stringSize = UnrealMonoGame.smallerFont.MeasureString(name) * UnrealMonoGame.defaultFontScale * stringScale * UnrealMonoGame.ResolutionScale;
				Color drawColor;
				if (i == currentChoiceIndex)
				{
					drawColor = selectedColor;

					if (choices[i].subChoices.Count > 1)
					{
						float largestSubChoiceNameSizeX = 0F;
						for (int k = 0; k < choices[i].subChoices.Count; ++k)
						{
							Vector2 subChoiceNameSize = UnrealMonoGame.smallerFont.MeasureString(choices[i].subChoices[k].Name);
							if (subChoiceNameSize.X > largestSubChoiceNameSizeX)
							{
								largestSubChoiceNameSizeX = subChoiceNameSize.X;
							}
						}

						largestSubChoiceNameSizeX *= UnrealMonoGame.defaultFontScale * stringScale * UnrealMonoGame.ResolutionScale;
						Vector2 stringSizeButton = UnrealMonoGame.smallerFont.MeasureString("<") * UnrealMonoGame.defaultFontScale * stringScale * UnrealMonoGame.ResolutionScale;
						UnrealMonoGame.spriteBatch.DrawString(UnrealMonoGame.smallerFont, "<", new Vector2((UnrealMonoGame.currentResolution.X / 2F) - (largestSubChoiceNameSizeX / 2F) - stringSizeButton.X, (startingYAlpha + (yAlphaBetweenLines * i)) * UnrealMonoGame.currentResolution.Y), drawColor, 0F, Vector2.Zero, UnrealMonoGame.ResolutionScale * UnrealMonoGame.defaultFontScale * stringScale, SpriteEffects.None, 0F);
						UnrealMonoGame.spriteBatch.DrawString(UnrealMonoGame.smallerFont, ">", new Vector2((UnrealMonoGame.currentResolution.X / 2F) + (largestSubChoiceNameSizeX / 2F), (startingYAlpha + (yAlphaBetweenLines * i)) * UnrealMonoGame.currentResolution.Y), drawColor, 0F, Vector2.Zero, UnrealMonoGame.ResolutionScale * UnrealMonoGame.defaultFontScale * stringScale, SpriteEffects.None, 0F);
					}
				}
				else
				{
					drawColor = unselectedColor;
				}
				UnrealMonoGame.spriteBatch.DrawString(UnrealMonoGame.smallerFont, name, new Vector2((UnrealMonoGame.currentResolution.X / 2F) - (stringSize.X / 2F), (startingYAlpha + (yAlphaBetweenLines * i)) * UnrealMonoGame.currentResolution.Y), drawColor, 0F, Vector2.Zero, UnrealMonoGame.ResolutionScale * UnrealMonoGame.defaultFontScale * stringScale, SpriteEffects.None, 0F);
			}

			DrawMenu();
		}

		public bool Update(ref ActionBinding downAction,
			ref ActionBinding upAction,
			ref ActionBinding rightAction,
			ref ActionBinding leftAction,
			ref ActionBinding enterAction,
			ref ActionBinding cursorEnterAction,
			ref ActionBinding backAction,
			Point cursorPointScreenLocation)
		{
			int choiceIndexHovered = -1;
			if (CheckCursorHovering(cursorPointScreenLocation, ref choiceIndexHovered))
			{
				currentChoiceIndex = choiceIndexHovered;
			}
			else
			{
				if (downAction.CheckBindings())
				{
					if (currentMenu.Next())
					{
						return true;
					}
				}
				else if (upAction.CheckBindings())
				{
					if (currentMenu.Previous())
					{
						return true;
					}
				}
				else if (rightAction.CheckBindings())
				{
					if (currentMenu.SubNext())
					{
						return true;
					}
				}
				else if (leftAction.CheckBindings())
				{
					if (currentMenu.SubPrevious())
					{
						return true;
					}
				}
			}
			if (backAction.CheckBindings())
			{
				if (currentMenu.Exit())
				{
					return true;
				}
			}
			else if (cursorEnterAction.CheckBindings())
			{
				if (choiceIndexHovered >= 0)
				{
					currentChoiceIndex = choiceIndexHovered;
					if (currentMenu.Enter())
					{
						return true;
					}
				}
				else if (choices[currentChoiceIndex].Buttons.Count == 0 && (currentMenu.choices == null || currentMenu.choices.Count == 0))
				{
					if (currentMenu.Enter())
					{
						return true;
					}
				}
				else
				{
					for (int i = 0; i < choices[currentChoiceIndex].Buttons.Count; ++i)
					{
						if (choices[currentChoiceIndex].Buttons[i].screenSpaceRectangle.IntersectsPoint(cursorPointScreenLocation))
						{
							switch (choices[currentChoiceIndex].Buttons[i].action)
							{
								case MenuButtonAction.SubChoiceNext:
									{
										if (currentMenu.SubNext())
										{
											return true;
										}
										break;
									}
								case MenuButtonAction.SubChoicePrevious:
									{
										if (currentMenu.SubPrevious())
										{
											return true;
										}
										break;
									}
								case MenuButtonAction.Enter:
									{
										if (currentMenu.Enter())
										{
											return true;
										}
										break;
									}
								case MenuButtonAction.Exit:
									{
										if (currentMenu.Exit())
										{
											return true;
										}
										break;
									}
							}
						}
					}
				}
			}
			else if (enterAction.CheckBindings())
			{
				if (currentMenu.Enter())
				{
					return true;
				}
			}

			return false;
		}

		public virtual void DrawMenu()
		{
		}
	}

	public class MenuEntry
	{
		public List<MenuEntryChoice> subChoices;
		public int CurrentSubChoiceIndex = 0;
		public List<MenuButton> Buttons { get; }

		public MenuEntry(List<MenuEntryChoice> menuSubChoices)
		{
			subChoices = menuSubChoices;
			Buttons = new List<MenuButton>();
		}

		public void InitButtons(float stringScale, float screenAlpha)
		{
			if (subChoices.Count > 1)
			{
				float largestSubChoiceNameSizeX = 0F;
				for (int k = 0; k < subChoices.Count; ++k)
				{
					Vector2 subChoiceNameSize = UnrealMonoGame.smallerFont.MeasureString(subChoices[k].Name);
					if (subChoiceNameSize.X > largestSubChoiceNameSizeX)
					{
						largestSubChoiceNameSizeX = subChoiceNameSize.X;
					}
				}

				largestSubChoiceNameSizeX *= UnrealMonoGame.defaultFontScale * stringScale * UnrealMonoGame.ResolutionScale;
				Vector2 stringSizeButton = UnrealMonoGame.smallerFont.MeasureString("<") * UnrealMonoGame.defaultFontScale * stringScale * UnrealMonoGame.ResolutionScale;
				Buttons.Add(new MenuButton(new Rectangle(((UnrealMonoGame.currentResolution.X / 2F) - (largestSubChoiceNameSizeX / 2F) - stringSizeButton.X).Round(), (screenAlpha * UnrealMonoGame.currentResolution.Y).Round(), stringSizeButton.X.Round(), stringSizeButton.Y.Round()), MenuButtonAction.SubChoicePrevious));
				Buttons.Add(new MenuButton(new Rectangle(((UnrealMonoGame.currentResolution.X / 2F) + (largestSubChoiceNameSizeX / 2F)).Round(), (screenAlpha * UnrealMonoGame.currentResolution.Y).Round(), stringSizeButton.X.Round(), stringSizeButton.Y.Round()), MenuButtonAction.SubChoiceNext));
			}
		}

		public void Enter()
		{
			subChoices[CurrentSubChoiceIndex].Enter();
		}

		public void Next()
		{
			CurrentSubChoiceIndex++;
			if (CurrentSubChoiceIndex >= subChoices.Count)
				CurrentSubChoiceIndex = 0;
		}

		public void Previous()
		{
			CurrentSubChoiceIndex--;
			if (CurrentSubChoiceIndex < 0)
				CurrentSubChoiceIndex = subChoices.Count - 1;
		}
	}

	public class MenuEntryChoice
	{
		public Menu owner;
		public Menu child;
		public virtual string Name { get { return ""; } }

		public virtual void Enter()
		{
			if (child != null)
			{
				child.Parent = owner;
				owner.Child = child;
			}
		}

		//public virtual void Exit() { }
	}

	public enum MenuButtonAction
	{
		Enter,
		Exit,
		SubChoiceNext,
		SubChoicePrevious
	}

	public struct MenuButton
	{
		public Rectangle screenSpaceRectangle;
		public MenuButtonAction action;

		public MenuButton(Rectangle screenSpaceRectangle, MenuButtonAction action)
		{
			this.screenSpaceRectangle = screenSpaceRectangle;
			this.action = action;
		}
	}
}