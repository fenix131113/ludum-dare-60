using GameAssembly.Player.Data;
using GameAssembly.Player.Input;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GameAssembly.Core
{
    public class GameInstaller : LifetimeScope
    {
        [SerializeField] private PlayerSettings playerSettings;
        
        private static IObjectResolver _resolver;

        protected override void Awake()
        {
            _resolver = Container;
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(playerSettings);
            
            builder.Register<InputSystem_Actions>(Lifetime.Scoped);
            
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