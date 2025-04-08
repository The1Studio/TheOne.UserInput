namespace TheOne.UserInput.Scripts
{
    using System.Collections.Generic;
    using GameFoundation.Signals;
    using TheOne.UserInput.Scripts.Signals;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.Scripting;
    using VContainer.Unity;

    public class UserInputSystem : ILateTickable
    {
        private readonly UserTouchDownSignal userTouchDownSignal = new();
        private readonly UserDragSignal      userDragSignal      = new();
        private readonly UserTouchUpSignal   userTouchUpSignal   = new();

        private Vector2 touchStartPosition;
        private bool    isStartTouchOverUI;

        private readonly SignalBus signalBus;

        [Preserve]
        private UserInputSystem(SignalBus signalBus)
        {
            this.signalBus = signalBus;
        }

        // UseLateTick instead of LateTick if you want to check Over UI
        // if use Tick, phase Began IsOverUI will always return false
        // if use LateTick, phase Ended IsOverUI will always return false
        public void LateTick()
        {
            #if UNITY_EDITOR
            this.UpdateOnEditor();
            #endif

            var touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    this.touchStartPosition                = touch.position;
                    this.userTouchDownSignal.TouchPosition = touch.position;
                    this.userTouchDownSignal.Touch         = touch;
                    this.userTouchDownSignal.IsTouchOverUI = IsTouchOverUI(touch);
                    this.isStartTouchOverUI                = IsTouchOverUI(touch);
                    this.signalBus.Fire(this.userTouchDownSignal);
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    this.userDragSignal.TouchPosition      = touch.position;
                    this.userDragSignal.TouchStartPosition = this.touchStartPosition;
                    this.userDragSignal.DeltaPosition      = touch.deltaPosition;
                    this.userDragSignal.IsStartTouchOverUI = this.isStartTouchOverUI;
                    this.signalBus.Fire(this.userDragSignal);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                default:
                    this.userTouchUpSignal.StartPosition      = this.touchStartPosition;
                    this.userTouchUpSignal.TouchPosition      = touch.position;
                    this.userTouchUpSignal.IsStartTouchOverUI = this.isStartTouchOverUI;
                    this.signalBus.Fire(this.userTouchUpSignal);
                    break;
            }
        }

        #if UNITY_EDITOR

        private Vector2 lastMousePosition;
        private void UpdateOnEditor()
        {
            if (Input.GetMouseButtonDown(0))
            {
                this.touchStartPosition                = Input.mousePosition;
                this.userTouchDownSignal.TouchPosition = Input.mousePosition;
                this.isStartTouchOverUI                = IsPointerOverUIObject();
                this.lastMousePosition                 = Input.mousePosition;
                this.signalBus.Fire(this.userTouchDownSignal);
                return;
            }

            if (Input.GetMouseButtonUp(0))
            {
                this.userTouchUpSignal.StartPosition      = this.touchStartPosition;
                this.userTouchUpSignal.TouchPosition      = Input.mousePosition;
                this.userTouchUpSignal.IsStartTouchOverUI = this.isStartTouchOverUI;
                this.signalBus.Fire(this.userTouchUpSignal);
                return;
            }

            if (Input.GetMouseButton(0))
            {
                this.userDragSignal.TouchPosition      = Input.mousePosition;
                this.userDragSignal.TouchStartPosition = this.touchStartPosition;
                this.userDragSignal.IsStartTouchOverUI = this.isStartTouchOverUI;
                this.userDragSignal.DeltaPosition      = (Vector2)Input.mousePosition - this.lastMousePosition;
                this.signalBus.Fire(this.userDragSignal);
                this.lastMousePosition = Input.mousePosition;
            }

            bool IsPointerOverUIObject()
            {
                var eventDataCurrentPosition = new PointerEventData(EventSystem.current) { position = new(Input.mousePosition.x, Input.mousePosition.y) };
                var results                  = new List<RaycastResult>();
                if (EventSystem.current) EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
                return results.Count > 0;
            }
        }
        #endif

        private static bool IsTouchOverUI(Touch touch)
        {
            var eventSystem = EventSystem.current;
            return eventSystem && eventSystem.IsPointerOverGameObject(touch.fingerId); // check null for game view
        }
    }
}