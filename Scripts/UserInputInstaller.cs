namespace TheOne.UserInput.Scripts
{
    using GameFoundation.Signals;
    using TheOne.UserInput.Scripts.Signals;
    using VContainer;

    public static class UserInputInstaller
    {
        public static void RegisterUserInput(this IContainerBuilder builder)
        {
            builder.DeclareSignal<UserTouchDownSignal>();
            builder.DeclareSignal<UserDragSignal>();
            builder.DeclareSignal<UserTouchUpSignal>();
            builder.DeclareSignal<UserZoomSignal>();

            builder.Register<UserInputSystem>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}