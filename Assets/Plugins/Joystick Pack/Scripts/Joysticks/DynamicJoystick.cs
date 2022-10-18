using System;
using Packages.Joystick_Pack.Scripts.Base;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Packages.Joystick_Pack.Scripts.Joysticks
{
    public class DynamicJoystick : Joystick
    {
        [SerializeField] private float moveThreshold = 1;

        public float MoveThreshold
        {
            get => moveThreshold;
            set => moveThreshold = Mathf.Abs(value);
        }

        protected override void Start()
        {
            MoveThreshold = moveThreshold;
            base.Start();
            background.gameObject.SetActive(false);
        }

        public event Action PlayerTouched;
        public event Action TouchReleased;

        public override void OnPointerDown(PointerEventData eventData)
        {
            PlayerTouched?.Invoke();
            background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
            background.gameObject.SetActive(true);
            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            TouchReleased?.Invoke();
            background.gameObject.SetActive(false);
            base.OnPointerUp(eventData);
        }

        protected override void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
        {
            if (magnitude > moveThreshold)
            {
                var difference = normalised * (magnitude - moveThreshold) * radius;
                background.anchoredPosition += difference;
            }

            base.HandleInput(magnitude, normalised, radius, cam);
        }
    }
}