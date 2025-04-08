namespace TheOne.UserInput.Scripts
{
    using UnityEngine;

    public class UserInputConfig
    {
        public Vector2 MinActivationArea { get; }
        public Vector2 MaxActivationArea { get; }

        /// <param name="minActivationArea">from 0f to 1f</param>
        /// <param name="maxActivationArea">from 0f to 1f</param>
        public UserInputConfig(Vector2 minActivationArea, Vector2 maxActivationArea)
        {
            this.MinActivationArea = minActivationArea;
            this.MaxActivationArea = maxActivationArea;
        }
    }
}