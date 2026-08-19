using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LastPassenger
{
    public static class PrototypeInput
    {
        public static bool AccelerateHeld => Held(InputAction.Accelerate);
        public static bool BrakeHeld => Held(InputAction.Brake);
        public static bool LeftHeld => Held(InputAction.Left);
        public static bool RightHeld => Held(InputAction.Right);
        public static bool MirrorHeld => Held(InputAction.Mirror);

        public static bool RadioPressed => Pressed(InputAction.Radio);
        public static bool ConfirmPressed => Pressed(InputAction.Confirm);
        public static bool CancelPressed => Pressed(InputAction.Cancel);
        public static bool SkipToTruckPressed => Pressed(InputAction.SkipToTruck);
        public static bool SkipToJunctionPressed => Pressed(InputAction.SkipToJunction);
        public static bool SkipToAnomalyPressed => Pressed(InputAction.SkipToAnomaly);
        public static bool SkipToRoadFigurePressed => Pressed(InputAction.SkipToRoadFigure);
        public static bool SkipToEndingPressed => Pressed(InputAction.SkipToEnding);

        public static bool CameraLookHeld
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Mouse mouse = Mouse.current;
                return mouse != null && mouse.rightButton.isPressed;
#else
                return Input.GetMouseButton(1);
#endif
            }
        }

        public static Vector2 CameraLookDelta
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Mouse mouse = Mouse.current;
                return mouse != null ? mouse.delta.ReadValue() : Vector2.zero;
#else
                return new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * 12f;
#endif
            }
        }

        private enum InputAction
        {
            Accelerate,
            Brake,
            Left,
            Right,
            Mirror,
            Radio,
            Confirm,
            Cancel,
            SkipToTruck,
            SkipToJunction,
            SkipToAnomaly,
            SkipToRoadFigure,
            SkipToEnding
        }

        private static bool Held(InputAction action)
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return false;

            switch (action)
            {
                case InputAction.Accelerate: return keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed;
                case InputAction.Brake: return keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed;
                case InputAction.Left: return keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
                case InputAction.Right: return keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
                case InputAction.Mirror: return keyboard.rKey.isPressed;
                default: return false;
            }
#else
            switch (action)
            {
                case InputAction.Accelerate: return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
                case InputAction.Brake: return Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
                case InputAction.Left: return Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
                case InputAction.Right: return Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
                case InputAction.Mirror: return Input.GetKey(KeyCode.R);
                default: return false;
            }
#endif
        }

        private static bool Pressed(InputAction action)
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return false;

            switch (action)
            {
                case InputAction.Radio: return keyboard.mKey.wasPressedThisFrame;
                case InputAction.Confirm: return keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame;
                case InputAction.Cancel: return keyboard.escapeKey.wasPressedThisFrame;
                case InputAction.SkipToTruck: return keyboard.f8Key.wasPressedThisFrame || keyboard.tKey.wasPressedThisFrame;
                case InputAction.SkipToJunction: return keyboard.f9Key.wasPressedThisFrame;
                case InputAction.SkipToAnomaly: return keyboard.f10Key.wasPressedThisFrame || keyboard.gKey.wasPressedThisFrame;
                case InputAction.SkipToRoadFigure: return keyboard.hKey.wasPressedThisFrame;
                case InputAction.SkipToEnding: return keyboard.f11Key.wasPressedThisFrame || keyboard.yKey.wasPressedThisFrame;
                default: return false;
            }
#else
            switch (action)
            {
                case InputAction.Radio: return Input.GetKeyDown(KeyCode.M);
                case InputAction.Confirm: return Input.GetKeyDown(KeyCode.Return);
                case InputAction.Cancel: return Input.GetKeyDown(KeyCode.Escape);
                case InputAction.SkipToTruck: return Input.GetKeyDown(KeyCode.F8) || Input.GetKeyDown(KeyCode.T);
                case InputAction.SkipToJunction: return Input.GetKeyDown(KeyCode.F9);
                case InputAction.SkipToAnomaly: return Input.GetKeyDown(KeyCode.F10) || Input.GetKeyDown(KeyCode.G);
                case InputAction.SkipToRoadFigure: return Input.GetKeyDown(KeyCode.H);
                case InputAction.SkipToEnding: return Input.GetKeyDown(KeyCode.F11) || Input.GetKeyDown(KeyCode.Y);
                default: return false;
            }
#endif
        }
    }
}
