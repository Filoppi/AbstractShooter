using System.Collections.Generic;
using InputManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AbstractShooter
{
    public class Menu
    {
        public List<MenuEntry> choices;
        public int currentChoiceIndex;
        public Menu parent;
        public Menu child;
        public Color selectedColor = Color.White;
        public Color unselectedColor = Color.Black;
        public Texture2D backgorundScreen;
        public float startingYAlpha = 0.5F;
        public float yAlphaBetweenLines = 0.05F;
        public float stringScale = 1F;

        private Point lastCursorHoverLocation = new Point(-1, -1);
        private List<MenuButton> buttons;

        public Menu()
        {
            choices = new List<MenuEntry>();
        }
        public Menu(List<MenuEntry> menuChoices, Color newSelectedColor, Color newUnselectedColor, Texture2D newBackgorundScreen, float newStartingYAlpha, float newYAlphaBetweenLines, float newStringScale)
        {
            choices = menuChoices;
            foreach (MenuEntry c in choices)
            {
                foreach (MenuEntryChoice sc in c.subChoices)
                {
                    sc.Owner = this;
                }
            }
            selectedColor = newSelectedColor;
            unselectedColor = newUnselectedColor;
            backgorundScreen = newBackgorundScreen;
            startingYAlpha = newStartingYAlpha;
            yAlphaBetweenLines = newYAlphaBetweenLines;
            stringScale = newStringScale;

            buttons = new List<MenuButton>();
        }

        public bool CheckCursorHovering(Point pointScreenLocation, ref int hitChoiceIndex)
        {
            if (child != null)
            {
                return child.CheckCursorHovering(pointScreenLocation, ref hitChoiceIndex);
            }
            
            for (int i = 0; i < choices.Count; ++i)
            {
                string name = choices[i].subChoices[choices[i].CurrentSubChoiceIndex].Name;
                Vector2 stringSize = Game1.smallerFont.MeasureString(name) * Game1.defaultFontScale * stringScale * Game1.ResolutionScale;

                if (pointScreenLocation.X > (Game1.currentResolution.X / 2F) - (stringSize.X / 2F)
                    && pointScreenLocation.X < (Game1.currentResolution.X / 2F) + (stringSize.X / 2F)
                    && pointScreenLocation.Y > (startingYAlpha + (yAlphaBetweenLines * i)) * Game1.currentResolution.Y
                    && pointScreenLocation.Y < ((startingYAlpha + (yAlphaBetweenLines * i)) * Game1.currentResolution.Y) + stringSize.Y)
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
            if (child != null)
            {
                return child.Enter();
            }
            if (choices != null && choices.Count > 0)
            {
                choices[currentChoiceIndex].Enter();
                return true;
            }
            return Exit(); //false
        }
        public bool Exit()
        {
            if (child != null)
            {
                return child.Exit();
            }
            if (parent != null)
            {
                parent.child = null;
                return true;
            }
            StateManager.Unpause();
            return false;
        }
        public bool SubNext()
        {
            if (child != null)
            {
                return child.SubNext();
            }
            choices[currentChoiceIndex].Next();
            return choices[currentChoiceIndex].subChoices.Count > 1;
        }
        public bool SubPrevious()
        {
            if (child != null)
            {
                return child.SubPrevious();
            }
            choices[currentChoiceIndex].Previous();
            return choices[currentChoiceIndex].subChoices.Count > 1;
        }
        public bool Next()
        {
            if (child != null)
            {
                return child.Next();
            }
            currentChoiceIndex++;
            if (choices != null && currentChoiceIndex >= choices.Count)
                currentChoiceIndex = 0;
            return choices != null && choices.Count > 1;
        }
        public bool Previous()
        {
            if (child != null)
            {
                return child.Previous();
            }
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
            if (child != null)
            {
                child.Draw();
                return;
            }

            if (backgorundScreen != null)
            {
                Game1.spriteBatch.Draw(backgorundScreen, new Rectangle(0, 0, Game1.currentResolution.X, Game1.currentResolution.Y), Color.White);
            }

            for (int i = 0; i < choices.Count; ++i)
            {
                string name = choices[i].subChoices[choices[i].CurrentSubChoiceIndex].Name;
                Vector2 stringSize = Game1.smallerFont.MeasureString(name) * Game1.defaultFontScale * stringScale * Game1.ResolutionScale;
                Color drawColor;
                if (i == currentChoiceIndex)
                {
                    drawColor = selectedColor;
                    
                    if (choices[i].subChoices.Count > 1)
                    {
                        float largestSubChoiceNameSizeX = 0F;
                        for (int k = 0; k < choices[i].subChoices.Count; ++k)
                        {
                            Vector2 subChoiceNameSize = Game1.smallerFont.MeasureString(choices[i].subChoices[k].Name);
                            if (subChoiceNameSize.X > largestSubChoiceNameSizeX)
                            {
                                largestSubChoiceNameSizeX = subChoiceNameSize.X;
                            }
                        }

                        largestSubChoiceNameSizeX *= Game1.defaultFontScale * stringScale * Game1.ResolutionScale;
                        Vector2 stringSizeButton = Game1.smallerFont.MeasureString("<") * Game1.defaultFontScale * stringScale * Game1.ResolutionScale;
                        Game1.spriteBatch.DrawString(Game1.smallerFont, "<", new Vector2((Game1.currentResolution.X / 2F) - (largestSubChoiceNameSizeX / 2F) - stringSizeButton.X, (startingYAlpha + (yAlphaBetweenLines * i)) * Game1.currentResolution.Y), drawColor, 0F, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0F);
                        Game1.spriteBatch.DrawString(Game1.smallerFont, ">", new Vector2((Game1.currentResolution.X / 2F) + (largestSubChoiceNameSizeX / 2F), (startingYAlpha + (yAlphaBetweenLines * i)) * Game1.currentResolution.Y), drawColor, 0F, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0F);
                    }
                }
                else
                {
                    drawColor = unselectedColor;
                }
                Game1.spriteBatch.DrawString(Game1.smallerFont, name, new Vector2((Game1.currentResolution.X / 2F) - (stringSize.X / 2F), (startingYAlpha + (yAlphaBetweenLines * i)) * Game1.currentResolution.Y), drawColor, 0F, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * stringScale, SpriteEffects.None, 0F);
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
                    if (Next())
                    {
                        return true;
                    }
                }
                else if (upAction.CheckBindings())
                {
                    if (Previous())
                    {
                        return true;
                    }
                }
                else if (rightAction.CheckBindings())
                {
                    if (SubNext())
                    {
                        return true;
                    }
                }
                else if (leftAction.CheckBindings())
                {
                    if (SubPrevious())
                    {
                        return true;
                    }
                }
            }
            if (backAction.CheckBindings())
            {
                if (Exit())
                {
                    return true;
                }
            }
            else if (cursorEnterAction.CheckBindings())
            {
                if (choiceIndexHovered >= 0)
                {
                    currentChoiceIndex = choiceIndexHovered;
                    if (Enter())
                    {
                        return true;
                    }
                }
                else
                {
                    buttons.Clear();
                    
                    if (choices[currentChoiceIndex].subChoices.Count > 1)
                    {
                        float largestSubChoiceNameSizeX = 0F;
                        for (int k = 0; k < choices[currentChoiceIndex].subChoices.Count; ++k)
                        {
                            Vector2 subChoiceNameSize = Game1.smallerFont.MeasureString(choices[currentChoiceIndex].subChoices[k].Name);
                            if (subChoiceNameSize.X > largestSubChoiceNameSizeX)
                            {
                                largestSubChoiceNameSizeX = subChoiceNameSize.X;
                            }
                        }

                        largestSubChoiceNameSizeX *= Game1.defaultFontScale * stringScale * Game1.ResolutionScale;
                        Vector2 stringSizeButton = Game1.smallerFont.MeasureString("<") * Game1.defaultFontScale * stringScale * Game1.ResolutionScale;
                        buttons.Add(new MenuButton(new Rectangle(((Game1.currentResolution.X / 2F) - (largestSubChoiceNameSizeX / 2F) - stringSizeButton.X).Round(), ((startingYAlpha + (yAlphaBetweenLines * currentChoiceIndex)) * Game1.currentResolution.Y).Round(), stringSizeButton.X.Round(), stringSizeButton.Y.Round()), MenuButtonAction.SubChoicePrevious));
                        buttons.Add(new MenuButton(new Rectangle(((Game1.currentResolution.X / 2F) + (largestSubChoiceNameSizeX / 2F)).Round(), ((startingYAlpha + (yAlphaBetweenLines * currentChoiceIndex)) * Game1.currentResolution.Y).Round(), stringSizeButton.X.Round(), stringSizeButton.Y.Round()), MenuButtonAction.SubChoiceNext));
                    }

                    for (int i = 0; i < buttons.Count; ++i)
                    {
                        if (buttons[i].screenSpaceRectangle.IntersectsPoint(cursorPointScreenLocation))
                        {
                            switch (buttons[i].action)
                            {
                                case MenuButtonAction.SubChoiceNext:
                                    {
                                        if (SubNext())
                                        {
                                            return true;
                                        }
                                        break;
                                    }
                                case MenuButtonAction.SubChoicePrevious:
                                    {
                                        if (SubPrevious())
                                        {
                                            return true;
                                        }
                                        break;
                                    }
                                case MenuButtonAction.Enter:
                                    {
                                        if (Enter())
                                        {
                                            return true;
                                        }
                                        break;
                                    }
                                case MenuButtonAction.Exit:
                                    {
                                        if (Exit())
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
                if (Enter())
                {
                    return true;
                }
            }

            return false;
        }

        public virtual void DrawMenu() { }
    }

    public class MenuEntry
    {
        public List<MenuEntryChoice> subChoices;
        public int CurrentSubChoiceIndex = 0;

        public MenuEntry(List<MenuEntryChoice> menuSubChoices)
        {
            subChoices = menuSubChoices;
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
        public Menu Owner;
        public Menu child;
        public virtual string Name { get { return ""; } }
        public virtual void Enter()
        {
            if (child != null)
            {
                child.parent = Owner;
                Owner.child = child;
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