namespace TheOne.UserInput.Scripts.Signals
{
    using UnityEngine;

    public class UserTouchUpSignal
    {
        public Vector2 TouchPosition      { get; set; }
        public Vector2 StartPosition      { get; set; }
        public bool    IsStartTouchOverUI { get; set; }
    }
}