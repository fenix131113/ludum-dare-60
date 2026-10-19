using GameAssembly.LootLocation;
using GameAssembly.LootLocation.Generation;
using GameAssembly.Player;
using GameAssembly.Player._2D;
using GameAssembly.Player.Data;
using GameAssembly.Player.Input;
using GameAssembly.ShelterSystem;
using GameAssembly.TraderSystem;
using GameAssembly.TraderSystem.View;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GameAssembly.Core
{
    public class GameInstaller : LifetimeScope
    {
        [SerializeField] private PlayerSettings playerSettings;
        
        private static IObjectResolver _resolver;

        protected void Start()
        {
            _resolver = Container;
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(playerSettings);
            
            builder.Register<InputSystem_Actions>(Lifetime.Scoped);
            builder.Register<ShelterInventory>(Lifetime.Scoped);
            builder.Register<PlayerBank>(Lifetime.Scoped);

            builder.RegisterComponentInHierarchy<PlayerCamera>();
            builder.RegisterComponentInHierarchy<LootPhaseEnd>();
            builder.RegisterComponentInHierarchy<Player2DMovement>();
            builder.RegisterComponentInHierarchy<PlayerMovement>();
            builder.RegisterComponentInHierarchy<LocationGenerator>();
            builder.RegisterComponentInHierarchy<TraderView>();
            builder.RegisterComponentInHierarchy<SellMinigame>();
            
            builder.Register<PlayerInventory>(Lifetime.Scoped)
                .As<IInitializable>()
                .AsSelf();
            
            builder.Register<PlayerState>(Lifetime.Scoped)
                .As<IInitializable>()
                .AsSelf();
            
            builder.Register<PlayerInput>(Lifetime.Scoped)
                .As<IInitializable>()
                .AsSelf();
        }

        public static void InjectGameObject(GameObject obj)
        {
            if(_resolver == null)
            {
                Debug.LogError("Dependencies Resolver Is Not Setup Yet!");
                return;
            }

            _resolver.InjectGameObject(obj);
        }
    }
}