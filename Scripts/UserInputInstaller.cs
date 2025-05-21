namespace TheOne.UserInput.Scripts
{
    using GameFoundation.Signals;
    using TheOne.UserInput.Scripts.Signals;
    using UnityEngine;
    using VContainer;

    public static class UserInputInstaller
    {
        public static void RegisterUserInput(this IContainerBuilder builder, UserInputConfig config = null)
        {
            builder.DeclareSignal<UserTouchDownSignal>();
            builder.DeclareSignal<UserDragSignal>();
            builder.DeclareSignal<UserTouchUpSignal>();

            config ??= new(Vector2.zero, Vector2.one);

            builder.Register<UserInputSystem>(Lifetime.Singleton)
                .WithParameter(config)
                .AsImplementedInterfaces().AsSelf();
        }
    }
}