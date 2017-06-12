using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnrealMono
{
	//Should be abstract
	public class State
	{
		public Texture2D spriteSheet;
		private List<AActor> actors, actorsToRegisterUpdate, actorsToUnregisterUpdate;
		private List<WeakReference<AActor>>[] actorsUpdateGroups;
		private int spawnedActorsCount;
		private List<WeakReference<CSceneComponent>> sceneComponents; //This is just kept for performance and comfort reasons
		private ASceneActor scene; //Holds all the components that do not need to be in a specific actor (e.g. particle fx)
		public ASceneActor Scene { get { return scene; } }
		private float timeScale = 1F;
		public float TimeScale { get { return timeScale; } set { timeScale = value; } }
		private const float actorsDrawDepthDelta = 0.05F;
		public const float componentsDrawDepthDelta = 0.00005F;
		public const float componentsDrawDepthMultiplier = 0.0000001F;
		public const float maxUniqueDrawDepthComponents = (int)(componentsDrawDepthDelta / componentsDrawDepthMultiplier);
		public const int maxUniqueDrawDepthActors = (int)(actorsDrawDepthDelta / componentsDrawDepthDelta);

		private bool iteratingActors;
		public bool IteratingActors { get { return iteratingActors; } }

		public virtual void Initialize()
		{
			actorsUpdateGroups = new List<WeakReference<AActor>>[(int)ActorUpdateGroup.MAX];
			for (int i = 0; i < actorsUpdateGroups.Length; ++i)
			{
				actorsUpdateGroups[i] = new List<WeakReference<AActor>>();
			}
			sceneComponents = new List<WeakReference<CSceneComponent>>();
			actors = new List<AActor>();
			actorsToRegisterUpdate = new List<AActor>();
			actorsToUnregisterUpdate = new List<AActor>();
			scene = new ASceneActor();
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
						actor.UniqueId = spawnedActorsCount;
						spawnedActorsCount++;
					}
					else
					{
						throw new System.ArgumentException("Actor was already contained in actorsToRegisterUpdate list");
					}
				}
				else
				{
					RegisterActorUpdate(actor);
					actor.UniqueId = spawnedActorsCount;
					spawnedActorsCount++;
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
						if (actorsToRegisterUpdate.Contains(actor)) //In case it is destroyed before actually spawning, don't even spawn it
						{
							actorsToRegisterUpdate.Remove(actor);
						}
						else
						{
							actorsToUnregisterUpdate.Add(actor);
						}
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

		public List<T> GetAllActorsOfType<T>() where T : AActor
		{
			List<T> foundActors = new List<T>();
			foreach (T actor in actors.OfType<T>())
			{
				foundActors.Add(actor);
			}
			return foundActors;
		}

		#endregion Actors

		#region SceneComponents

		public void AddSceneComponent(CSceneComponent sceneComponent)
		{
			if (!sceneComponents.Contains(new WeakReference<CSceneComponent>(sceneComponent)))
			{
				sceneComponents.Add(new WeakReference<CSceneComponent>(sceneComponent));
			}
			else
			{
				throw new System.ArgumentException("SceneComponents was already contained in sceneComponents list");
			}
		}

		public void RemoveSceneComponent(CSceneComponent sceneComponent)
		{
			for (int i = 0; i < sceneComponents.Count; ++i)
			{
				CSceneComponent weakTarget;
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

			//if (!sceneComponents.Remove(new WeakReference<CSceneComponent>(sceneComponent)))
			//{
			//    throw new System.ArgumentException("sceneComponent was not contained in sceneComponents list");
			//}
		}

		public List<T> GetAllSceneComponentsOfType<T>() where T : CSceneComponent
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

		public List<CSceneComponent> GetAllSceneComponents()
		{
			List<CSceneComponent> foundSceneComponents = new List<CSceneComponent>();
			foreach (WeakReference<CSceneComponent> weakSceneComponentRef in sceneComponents)
			{
				CSceneComponent sceneComponentRef;
				if (weakSceneComponentRef.TryGetTarget(out sceneComponentRef))
				{
					foundSceneComponents.Add(sceneComponentRef);
				}
			}
			return foundSceneComponents;
		}

		/// Deprecated. Should be the same as GetAllSceneComponents()
		public List<T> GetAllSceneComponentsFromActorsOfType<T>() where T : CSceneComponent
		{
			List<T> foundSceneComponents = new List<T>();
			foreach (AActor actor in actors)
			{
				foundSceneComponents.AddRange(actor.GetSceneComponentsByClass<T>());
			}
			return foundSceneComponents;
		}

		/// Deprecated. Should be the same as GetAllSceneComponents<T>()
		public List<CSceneComponent> GetAllSceneComponentsFromActors()
		{
			List<CSceneComponent> foundSceneComponents = new List<CSceneComponent>();
			foreach (AActor actor in actors)
			{
				foundSceneComponents.AddRange(actor.GetSceneComponents());
			}
			return foundSceneComponents;
		}

		#endregion SceneComponents

		public virtual void OnSetAsCurrentState()
		{
		}

		public virtual void Update(GameTime gameTime)
		{
			//if (timeScale == 0F)
			//{
			//    return;
			//}

			iteratingActors = true;

			//Pre-Physics
			for (int i = 0; i < actorsUpdateGroups.Length - 2; ++i)
			{
				UpdateActorsGroup(gameTime, i);
			}

			UpdatePhysics(gameTime);

			//Post-Physics
			for (int i = actorsUpdateGroups.Length - 3; i < actorsUpdateGroups.Length; ++i)
			{
				UpdateActorsGroup(gameTime, i);
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
				//TO1 update overlapping actor collisions? (Already done???)
			}
			actorsToRegisterUpdate.Clear();
		}

		public virtual void UpdatePhysics(GameTime gameTime)
		{
		}

		public virtual void UpdateActorsGroup(GameTime gameTime, int i)
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

		public virtual void BeginDraw()
		{
			UnrealMonoGame.Get.GraphicsDevice.SetRenderTarget(null);
			UnrealMonoGame.Get.GraphicsDevice.Clear(Color.Black);
			UnrealMonoGame.spriteBatch.Begin();
		}

		public virtual void EndDraw()
		{
#if DEBUG
			UnrealMonoGame.DrawDebugStrings();
#endif
			UnrealMonoGame.spriteBatch.End();
		}

		public virtual void Draw()
		{
			BeginDraw();
			DrawActors();
			EndDraw();
		}

		public virtual void DrawActors()
		{
			foreach (AActor actor in actors)
			{
				actor.Draw();
			}
		}
	}
}