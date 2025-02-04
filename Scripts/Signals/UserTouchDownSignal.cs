namespace TheOne.UserInput.Scripts.Signals
{
    using UnityEngine;

    public class UserTouchDownSignal
    {
        public Vector2 TouchPosition { get; set; }
        public Touch   Touch         { get; set; }
        public bool    IsTouchOverUI { get; set; }
    }
}