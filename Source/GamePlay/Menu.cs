using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AbstractShooter
{
    public class MenuChoices
    {
        public List<MenuChoice> Choices;
        public Int32 CurrentChoiceIndex = 0;
        public MenuChoices Parent = null;
        public MenuChoices Child = null;
        public Color selectedColor = Color.White;
        public Color unselectedColor = Color.Black;
        public Texture2D BackgorundScreen;
        public float StartingYAlpha = 0.5F;
        public float YAlphaBetweenLines = 0.05F;
        public float StringScale = 1F;

        public MenuChoices() { }
        public MenuChoices(List<MenuChoice> menuChoices, Color newSelectedColor, Color newUnselectedColor, Texture2D newBackgorundScreen, float newStartingYAlpha, float newYAlphaBetweenLines, float newStringScale)
        {
            Choices = menuChoices;
            foreach (MenuChoice c in Choices)
            {
                foreach (MenuChoiceChoice sc in c.SubChoices)
                {
                    sc.Owner = this;
                }
            }
            selectedColor = newSelectedColor;
            unselectedColor = newUnselectedColor;
            BackgorundScreen = newBackgorundScreen;
            StartingYAlpha = newStartingYAlpha;
            YAlphaBetweenLines = newYAlphaBetweenLines;
            StringScale = newStringScale;
        }

        public bool Enter()
        {
            if (Child != null)
            {
                return Child.Enter();
            }
            if (Choices != null && Choices.Count > 0)
            {
                Choices[CurrentChoiceIndex].Enter();
                return true;
            }
            return Exit(); //false
        }
        public bool Exit()
        {
            if (Child != null)
            {
                return Child.Exit();
            }
            if (Parent != null)
            {
                Parent.Child = null;
                return true;
            }
            return false;
        }
        public bool SubNext()
        {
            if (Child != null)
            {
                return Child.SubNext();
            }
            Choices[CurrentChoiceIndex].Next();
            return Choices[CurrentChoiceIndex].SubChoices.Count > 1;
        }
        public bool SubPrevious()
        {
            if (Child != null)
            {
                return Child.SubPrevious();
            }
            Choices[CurrentChoiceIndex].Previous();
            return Choices[CurrentChoiceIndex].SubChoices.Count > 1;
        }
        public bool Next()
        {
            if (Child != null)
            {
                return Child.Next();
            }
            CurrentChoiceIndex++;
            if (Choices != null && CurrentChoiceIndex >= Choices.Count)
                CurrentChoiceIndex = 0;
            return Choices != null && Choices.Count > 1;
        }
        public bool Previous()
        {
            if (Child != null)
            {
                return Child.Previous();
            }
            CurrentChoiceIndex--;
            if (CurrentChoiceIndex < 0 && Choices != null)
                CurrentChoiceIndex = Choices.Count - 1;
            return Choices != null && Choices.Count > 1;
        }
        public void Draw(GameTime gameTime, GraphicsDevice graphicsDevice, ContentManager content)
        {
            if (Child != null)
            {
                Child.Draw(gameTime, graphicsDevice, content);
                return;
            }

            if (BackgorundScreen != null)
            {
                StateManager.spriteBatch.Draw(
                    BackgorundScreen,
                    new Rectangle(0, 0, Game1.CurResolutionX, Game1.CurResolutionY),
                    Color.White);
            }

            for (Int32 i = 0; i < Choices.Count; ++i)
            {
                string tempName = Choices[i].SubChoices[Choices[i].CurrentSubChoiceIndex].Name;
                Color tempColor;
                if (i == CurrentChoiceIndex)
                {
                    tempColor = Color.Fuchsia;
                    if (Choices[i].SubChoices.Count > 1)
                    {
                        tempName = "< " + tempName + " >";
                    }
                }
                else
                {
                    tempColor = Color.Black;
                }
                Vector2 StringSize = Game1.smallerFont.MeasureString(tempName) * Game1.defaultFontScale * StringScale;
                StateManager.spriteBatch.DrawString(Game1.smallerFont, tempName, new Vector2((Game1.CurResolutionX / 2.0f) - ((StringSize.X / 2F) * Game1.ResolutionScale), (StartingYAlpha + (YAlphaBetweenLines * i)) * Game1.CurResolutionY), tempColor, 0, Vector2.Zero, Game1.ResolutionScale * Game1.defaultFontScale * StringScale, SpriteEffects.None, 0);
            }
        }
    }
    public class MenuChoice
    {
        public List<MenuChoiceChoice> SubChoices;
        public Int32 CurrentSubChoiceIndex = 0;

        public MenuChoice(List<MenuChoiceChoice> menuSubChoices)
        {
            SubChoices = menuSubChoices;
        }
        public void Enter()
        {
            SubChoices[CurrentSubChoiceIndex].Enter();
        }
        public void Next()
        {
            CurrentSubChoiceIndex++;
            if (CurrentSubChoiceIndex >= SubChoices.Count)
                CurrentSubChoiceIndex = 0;
        }
        public void Previous()
        {
            CurrentSubChoiceIndex--;
            if (CurrentSubChoiceIndex < 0)
                CurrentSubChoiceIndex = SubChoices.Count - 1;
        }
    }
    public class MenuChoiceChoice
    {
        public MenuChoices Owner = null;
        public MenuChoices Child = null;
        public virtual string Name { get { return ""; } }
        public virtual void Enter()
        {
            if (Child != null)
            {
                Child.Parent = Owner;
                Owner.Child = Child;
            }
        }
        //public virtual void Exit() { }
    }

}
