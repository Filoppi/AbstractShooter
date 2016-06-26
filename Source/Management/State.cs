using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System;

namespace AbstractShooter
{
    public abstract class State
    {
        public Texture2D spriteSheet;
        private List<AActor> actors, actorsToRegisterUpdate, actorsToUnregisterUpdate;
        private List<WeakReference<AActor>>[] actorsUpdateGroups;
        private List<WeakReference<SceneComponent>> sceneComponents; //This is just kept for performance and comfort reasons
        private AScene scene; //Holds all the components that do not need to be in a specific actor (e.g. particle fx)
        public AScene Scene { get { return scene; } }
        private float timeScale = 1;
        public float TimeScale { get { return timeScale; } set { timeScale = value; } }

        private bool iteratingActors;
        public bool IteratingActors { get { return iteratingActors; } }

        public virtual void Initialize()
        {
            actorsUpdateGroups = new List<WeakReference<AActor>>[(int)ActorUpdateGroup.MAX];
            for (int i = 0; i < actorsUpdateGroups.Length; ++i)
            {
                actorsUpdateGroups[i] = new List<WeakReference<AActor>>();
            }
            sceneComponents = new List<WeakReference<SceneComponent>>();
            actors = new List<AActor>();
            actorsToRegisterUpdate = new List<AActor>();
            actorsToUnregisterUpdate = new List<AActor>();
            scene = new AScene();
        }

#region Actors
        public void RegisterActor(AActor actor)
        {
            if (!actors.Contains(actor))
            {
                actors.Add(actor);

                if (iteratingActors)
                {
                    if (!actorsToRegisterUpdate.Contains(actor))
                    {
                        actorsToRegisterUpdate.Add(actor);
                    }
                    else
                    {
                        throw new System.ArgumentException("Actor was already contained in actorsToRegisterUpdate list");
                    }
                }
                else
                {
                    RegisterActorUpdate(actor);
                }
            }
            else
            {
                throw new System.ArgumentException("Actor was already contained in actors list");
            }
        }
        private void RegisterActorUpdate(AActor actor)
        {
            actorsUpdateGroups[(int)actor.UpdateGroup].Add(new WeakReference<AActor>(actor));
        }
        public void UnregisterActor(AActor actor)
        {
            if (actors.Contains(actor))
            {
                actors.Remove(actor);

                if (iteratingActors)
                {
                    if (!actorsToUnregisterUpdate.Contains(actor))
                    {
                        actorsToUnregisterUpdate.Add(actor);
                    }
                    else
                    {
                        throw new System.ArgumentException("Actor was already contained in actorsToUnregisterUpdate list");
                    }
                }
                else
                {
                    UnregisterActorUpdate(actor);
                }
            }
            else
            {
                throw new System.ArgumentException("Actor is not contained in actors list");
            }

        }
        private void UnregisterActorUpdate(AActor actor)
        {
            int updateGroup = (int)actor.UpdateGroup;
            for (int i = 0; i < actorsUpdateGroups[updateGroup].Count; ++i)
            {
                AActor weakTarget;
                if (actorsUpdateGroups[updateGroup][i].TryGetTarget(out weakTarget))
                {
                    if (weakTarget == actor)
                    {
                        actorsUpdateGroups[updateGroup].RemoveAt(i);
                        return;
                    }
                }
            }
            throw new System.ArgumentException("actor was not contained in actors update list");

            //actorsUpdateGroups[(int)actor.UpdateGroup].Remove(new WeakReference<AActor>(actor));
        }
        public List<AActor> GetAllActors()
        {
            return actors.ToList();
        }
        public List<T> GetAllActorsOfClass<T>() where T : AActor
        {
            List<T> foundActors = new List<T>();
            foreach (T actor in actors.OfType<T>())
            {
                foundActors.Add(actor);
            }
            return foundActors;
        }
#endregion

        #region SceneComponents
        public void AddSceneComponent(SceneComponent sceneComponent)
        {
            if (!sceneComponents.Contains(new WeakReference<SceneComponent>(sceneComponent)))
            {
                sceneComponents.Add(new WeakReference<SceneComponent>(sceneComponent));
            }
            else
            {
                throw new System.ArgumentException("SceneComponents was already contained in sceneComponents list");
            }
        }
        public void RemoveSceneComponent(SceneComponent sceneComponent)
        {
            for (int i = 0; i < sceneComponents.Count; ++i)
            {
                SceneComponent weakTarget;
                if (sceneComponents[i].TryGetTarget(out weakTarget))
                {
                    if (weakTarget == sceneComponent)
                    {
                        sceneComponents.RemoveAt(i);
                        return;
                    }
                }
            }
            throw new System.ArgumentException("sceneComponent was not contained in sceneComponents list");

            //if (!sceneComponents.Remove(new WeakReference<SceneComponent>(sceneComponent)))
            //{
            //    throw new System.ArgumentException("sceneComponent was not contained in sceneComponents list");
            //}
        }
        public List<T> GetAllSceneComponentsOfClass<T>() where T : SceneComponent
        {
            List<T> foundSceneComponents = new List<T>();
            foreach (WeakReference<T> weakSceneComponentRef in sceneComponents.OfType<WeakReference<T>>())
            {
                T sceneComponentRef;
                if (weakSceneComponentRef.TryGetTarget(out sceneComponentRef)) //&& is not PendingDestroy
                {
                    foundSceneComponents.Add(sceneComponentRef);
                }
            }
            return foundSceneComponents;
        }
        public List<SceneComponent> GetAllSceneComponents()
        {
            List<SceneComponent> foundSceneComponents = new List<SceneComponent>();
            foreach (WeakReference<SceneComponent> weakSceneComponentRef in sceneComponents)
            {
                SceneComponent sceneComponentRef;
                if (weakSceneComponentRef.TryGetTarget(out sceneComponentRef))
                {
                    foundSceneComponents.Add(sceneComponentRef);
                }
            }
            return foundSceneComponents;
        }
        /// <summary>
        /// Deprecated. Should be the same as GetAllSceneComponents()
        /// </summary>
        /// <returns></returns>
        public List<T> GetAllSceneComponentsFromActors<T>() where T : SceneComponent
        {
            List<T> foundSceneComponents = new List<T>();
            foreach (AActor actor in actors)
            {
                foundSceneComponents.AddRange(actor.GetSceneComponentsByClass<T>());
            }
            return foundSceneComponents;
        }
        /// <summary>
        /// Deprecated. Should be the same as GetAllSceneComponents<T>()
        /// </summary>
        /// <returns></returns>
        public List<SceneComponent> GetAllSceneComponentsFromActors()
        {
            List<SceneComponent> foundSceneComponents = new List<SceneComponent>();
            foreach (AActor actor in actors)
            {
                foundSceneComponents.AddRange(actor.GetSceneComponents());
            }
            return foundSceneComponents;
        }
#endregion

        public virtual void Update(GameTime gameTime)
        {
            iteratingActors = true;

            for (int i = 0; i < actorsUpdateGroups.Length; ++i)
            {
                for (int k = 0; k < actorsUpdateGroups[i].Count; ++k)
                {
                    AActor actor;
                    if (actorsUpdateGroups[i][k].TryGetTarget(out actor))
                    {
                        if (!actor.PendingDestroy)
                        {
                            actor.Update(gameTime);
                        }
                    }
                    else
                    {
                        throw new System.ArgumentException("Actor weak reference is not valid. This reference should have been removed already");
                    }
                }
            }

            iteratingActors = false;

            foreach (AActor actor in actorsToUnregisterUpdate)
            {
                UnregisterActorUpdate(actor);
            }
            actorsToUnregisterUpdate.Clear();

            foreach (AActor actor in actorsToRegisterUpdate)
            {
                RegisterActorUpdate(actor);
                actor.Update(gameTime);
                //To update overlapping actor collisions?
            }
            actorsToRegisterUpdate.Clear();
        }

        public virtual void Draw()
        {
            foreach (AActor actor in actors)
            {
                actor.Draw();
            }
        }
    }
}