namespace TheOne.UserInput.Scripts
{
    using System.Collections.Generic;
    using GameFoundation.Signals;
    using TheOne.UserInput.Scripts.Signals;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using VContainer.Unity;

    public class UserInputSystem : ILateTickable
    {
        private readonly UserTouchDownSignal userTouchDownSignal = new();
        private readonly UserDragSignal      userDragSignal      = new();
        private readonly UserTouchUpSignal   userTouchUpSignal   = new();
        private readonly UserZoomSignal      userZoomSignal      = new();

        private Vector2 touchStartPosition;
        private bool    isStartTouchOverUI;
        private bool    isZoomAction;

        private readonly SignalBus signalBus;

        private UserInputSystem(SignalBus signalBus)
        {
            this.signalBus = signalBus;
        }

        // UseLateTick instead of LateTick if you want to check Over UI
        // if use Tick, phase Began IsOverUI will always return false
        // if use LateTick, phase Ended IsOverUI will always return false
        public void LateTick()
        {
            this.UpdateOnEditor();

            switch (Input.touchCount)
            {
                case <= 0:
                    this.isZoomAction = false;
                    return;
                case > 1:
                {
                    var t0       = Input.GetTouch(0);
                    var t1       = Input.GetTouch(1);
                    var prevDist = (t0.position - t0.deltaPosition - (t1.position - t1.deltaPosition)).magnitude;
                    var currDist = (t0.position - t1.position).magnitude;
                    this.userZoomSignal.ZoomChangeAmount = currDist - prevDist;
                    this.signalBus.Fire(this.userZoomSignal);
                    this.isZoomAction = true;
                    return;
                }
                default:
                    if (this.isZoomAction) return;
                    this.UpdateTouch();
                    return;
            }
        }

        private void UpdateTouch()
        {
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

        private void UpdateOnEditor()
        {
            #if UNITY_EDITOR
            // For test on game window only
            if (Input.mouseScrollDelta.y != 0)
            {
                this.userZoomSignal.ZoomChangeAmount = Input.mouseScrollDelta.y * 50;
                this.signalBus.Fire(this.userZoomSignal);
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                this.touchStartPosition                = Input.mousePosition;
                this.userTouchDownSignal.TouchPosition = Input.mousePosition;
                this.isStartTouchOverUI                = IsPointerOverUIObject();
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
                this.signalBus.Fire(this.userDragSignal);
            }

            bool IsPointerOverUIObject()
            {
                var eventDataCurrentPosition = new PointerEventData(EventSystem.current) { position = new(Input.mousePosition.x, Input.mousePosition.y) };
                var results                  = new List<RaycastResult>();
                if (EventSystem.current) EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
                return results.Count > 0;
            }
            #endif
        }

        private static bool IsTouchOverUI(Touch touch)
        {
            var eventSystem = EventSystem.current;
            return eventSystem != null && eventSystem.IsPointerOverGameObject(touch.fingerId); // check null for editor
        }
    }
}