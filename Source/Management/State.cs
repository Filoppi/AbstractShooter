using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace AbstractShooter
{
    public abstract class State
    {
        public Texture2D spriteSheet;
        public Texture2D titleScreen;
        private List<AActor> actors;
        public List<AActor> Actors { get { return actors; } }
        private AActor scene; //Holds all the components that do not need to be in a specific actor (e.g. particle fx)
        public AActor Scene { get { return scene; } }

        public virtual void Initialize()
        {
            actors = new List<AActor>();
            scene = new AActor();
        }
        public void AddActor(AActor actor)
        {
            if (!actors.Contains(actor))
            {
                actors.Add(actor);
            }
        }
        public bool CanRemoveActor(AActor actor)
        {
            if (actor == scene)
            {
                //Cannot remove scene from actors
                return false;
            }
            return actors.Contains(actor);
        }
        public void RemoveActor(AActor actor)
        {
            if (CanRemoveActor(actor))
            {
                actors.Remove(actor);
            }
        }
        public List<SceneComponent> GetAllSceneComponents()
        {
            List<SceneComponent> sceneComponents = new List<SceneComponent>();
            foreach (AActor actor in actors)
            {
                actor.GetSceneComponents(ref sceneComponents);
            }
            return sceneComponents;
        }
        public List<T> GetAllSceneComponentsOfClass<T>() where T : SceneComponent
        {
            List<T> foundSceneComponents = new List<T>();
            foreach (AActor actor in actors)
            {
                foundSceneComponents.AddRange(actor.GetSceneComponents<T>());
            }
            return foundSceneComponents;
        }
        public List<T> GetAllActorsOfClass<T>() where T : AActor
        {
            List<T> foundActors = new List<T>();
            //foreach (T actor in actors)
            //{
            //    foundActors.Add(actor);
            //}
            foreach (AActor actor in actors)
            {
                if (actor is T)
                {
                    foundActors.Add((T)actor);
                }
            }
            return foundActors;
        }

        public virtual void Update(GameTime gameTime)
        {
            UpdateActors(gameTime, ActorUpdateGroup.Background);
            UpdateActors(gameTime, ActorUpdateGroup.Default);
            UpdateActors(gameTime, ActorUpdateGroup.PassiveObjects);
            UpdateActors(gameTime, ActorUpdateGroup.Characters);
            UpdateActors(gameTime, ActorUpdateGroup.Players);
            UpdateActors(gameTime, ActorUpdateGroup.PostPhysics);
            UpdateActors(gameTime, ActorUpdateGroup.Camera);
        }
        public virtual void UpdateActors(GameTime gameTime, ActorUpdateGroup updateGroup)
        {
            foreach (AActor actor in actors.ToList())
            {
                actor.Update(gameTime, updateGroup);
            }
            //scene.Update(gameTime, updateGroup);
        }
        public virtual void Draw()
        {
            //foreach (AActor actor in actors)
            //{
            //    actor.Draw();
            //}
            //scene.Draw();
        }
        public void DrawActors()
        {
            foreach (AActor actor in actors)
            {
                actor.Draw();
            }
        }
    }

    //public class AScene : AActor
    //{
    //    protected override void UpdateActor(GameTime gameTime)
    //    {
    //    }
    //    public void DrawDescendantsAndSelf()
    //    {
    //        Draw();
    //    }
    //    public override void Draw()
    //    {
    //    }
    //}
}