namespace TheOne.UserInput.Scripts.Signals
{
    using UnityEngine;

    public class UserDragSignal
    {
        public Vector2 TouchPosition      { get; set; }
        public Vector2 TouchStartPosition { get; set; }
        public Vector2 DeltaPosition      { get; set; }
        public bool    IsStartTouchOverUI { get; set; }

        public UserDragSignal() { }

        public UserDragSignal(Vector2 touchPosition, Vector2 touchStartPosition, Vector2 deltaPosition)
        {
            this.TouchPosition      = touchPosition;
            this.TouchStartPosition = touchStartPosition;
            this.DeltaPosition      = deltaPosition;
        }
    }
}