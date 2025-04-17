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
        private Vector2 lastTouchPosition;
        private bool    isStartTouchOverUI;
        private int?    fingerId = null;
        private bool    isTrackingTouch;

        #region Inject

        private readonly SignalBus       signalBus;
        private readonly UserInputConfig userInputConfig;

        [Preserve]
        private UserInputSystem(SignalBus signalBus, UserInputConfig userInputConfig)
        {
            this.signalBus       = signalBus;
            this.userInputConfig = userInputConfig;
        }

        #endregion

        // UseLateTick instead of LateTick if you want to check Over UI
        // if use Tick, phase Began IsOverUI will always return false
        // if use LateTick, phase Ended IsOverUI will always return false
        public void LateTick()
        {
            #if UNITY_EDITOR
            this.UpdateOnEditor();
            #endif

            // Handle touch input
            this.HandleTouchInput();
        }

        private void HandleTouchInput()
        {
            // If we're not tracking a touch and there are no touches, do nothing
            if (!this.isTrackingTouch && Input.touchCount == 0) return;

            // If we're tracking a touch but it's no longer active, reset tracking
            if (this.isTrackingTouch && this.fingerId.HasValue)
            {
                var touchStillExists = false;
                for (var i = 0; i < Input.touchCount; i++)
                {
                    if (Input.GetTouch(i).fingerId == this.fingerId.Value)
                    {
                        touchStillExists = true;
                        break;
                    }
                }

                if (!touchStillExists)
                {
                    // Touch was lost, fire touch up signal
                    this.userTouchUpSignal.StartPosition      = this.touchStartPosition;
                    this.userTouchUpSignal.TouchPosition      = this.lastTouchPosition; // Use last known position
                    this.userTouchUpSignal.IsStartTouchOverUI = this.isStartTouchOverUI;
                    this.signalBus.Fire(this.userTouchUpSignal);

                    // Reset tracking
                    this.isTrackingTouch = false;
                    this.fingerId        = null;
                    return;
                }
            }

            // Start tracking a new touch if we're not already tracking one
            if (!this.isTrackingTouch && Input.touchCount > 0)
            {
                // Find the first touch in the activation area
                for (var i = 0; i < Input.touchCount; i++)
                {
                    var touch = Input.GetTouch(i);
                    if (this.IsInsideActivationArea(touch.position))
                    {
                        this.fingerId        = touch.fingerId;
                        this.isTrackingTouch = true;
                        break;
                    }
                }

                // If no valid touch found, exit
                if (!this.isTrackingTouch) return;
            }

            // Process the tracked touch
            if (this.isTrackingTouch && this.fingerId.HasValue)
            {
                // Find the current touch with our fingerId
                var currentTouch = new Touch();
                var touchFound   = false;

                for (var i = 0; i < Input.touchCount; i++)
                {
                    var touch = Input.GetTouch(i);
                    if (touch.fingerId == this.fingerId.Value)
                    {
                        currentTouch = touch;
                        touchFound   = true;
                        break;
                    }
                }

                // If we can't find our touch, reset tracking
                if (!touchFound)
                {
                    this.isTrackingTouch = false;
                    this.fingerId        = null;
                    return;
                }

                // Process the touch based on its phase
                switch (currentTouch.phase)
                {
                    case TouchPhase.Began:
                        this.touchStartPosition                = currentTouch.position;
                        this.lastTouchPosition                 = currentTouch.position;
                        this.userTouchDownSignal.TouchPosition = currentTouch.position;
                        this.userTouchDownSignal.Touch         = currentTouch;
                        this.userTouchDownSignal.IsTouchOverUI = IsTouchOverUI(currentTouch);
                        this.isStartTouchOverUI                = IsTouchOverUI(currentTouch);
                        this.signalBus.Fire(this.userTouchDownSignal);
                        break;

                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        this.lastTouchPosition                 = currentTouch.position;
                        this.userDragSignal.TouchPosition      = currentTouch.position;
                        this.userDragSignal.TouchStartPosition = this.touchStartPosition;
                        this.userDragSignal.DeltaPosition      = currentTouch.deltaPosition;
                        this.userDragSignal.IsStartTouchOverUI = this.isStartTouchOverUI;
                        this.signalBus.Fire(this.userDragSignal);
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        this.lastTouchPosition                    = currentTouch.position;
                        this.userTouchUpSignal.StartPosition      = this.touchStartPosition;
                        this.userTouchUpSignal.TouchPosition      = currentTouch.position;
                        this.userTouchUpSignal.IsStartTouchOverUI = this.isStartTouchOverUI;
                        this.signalBus.Fire(this.userTouchUpSignal);

                        // Reset tracking
                        this.isTrackingTouch = false;
                        this.fingerId        = null;
                        break;
                }
            }
        }

        #if UNITY_EDITOR

        private Vector2 lastMousePosition;
        private bool    isStartValidTouch;
        private void UpdateOnEditor()
        {
            if (Input.GetMouseButtonDown(0))
            {
                this.isStartValidTouch = this.IsInsideActivationArea(Input.mousePosition);
                if (!this.isStartValidTouch) return;

                this.touchStartPosition                = Input.mousePosition;
                this.userTouchDownSignal.TouchPosition = Input.mousePosition;
                this.isStartTouchOverUI                = IsPointerOverUIObject();
                this.lastMousePosition                 = Input.mousePosition;
                this.signalBus.Fire(this.userTouchDownSignal);
                return;
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (!this.isStartValidTouch) return;
                this.userTouchUpSignal.StartPosition      = this.touchStartPosition;
                this.userTouchUpSignal.TouchPosition      = Input.mousePosition;
                this.userTouchUpSignal.IsStartTouchOverUI = this.isStartTouchOverUI;
                this.signalBus.Fire(this.userTouchUpSignal);
                this.isStartValidTouch = false;
                return;
            }

            if (Input.GetMouseButton(0))
            {
                if (!this.isStartValidTouch) return;
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

        private bool IsInsideActivationArea(Vector2 touchPosition)
        {
            var activationArea = new Rect(this.userInputConfig.MinActivationArea, this.userInputConfig.MaxActivationArea - this.userInputConfig.MinActivationArea);
            return activationArea.Contains(new Vector2(touchPosition.x / Screen.width, touchPosition.y / Screen.height));
        }
    }
}