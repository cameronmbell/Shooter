using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InputMapper : MonoBehaviour {
    public enum InputVarient {
        KeyboardOnly,
        ControllerMainly
    };

    public InputVarient InputMode                   { get; private set; }
    [HideInInspector] public float HorizontalAxis   { get; private set; }
    [HideInInspector] public float VerticalAxis     { get; private set; } 
    [HideInInspector] public float MouseXAxis       { get; private set; }
    [HideInInspector] public float MouseYAxis       { get; private set; } 
    [HideInInspector] public bool JumpReleased      { get; private set; }
    [HideInInspector] public bool JumpPressed       { get; private set; }
    [HideInInspector] public bool JumpHeld          { get; private set; }

    void Update() {
        // reset
        HorizontalAxis = VerticalAxis = MouseXAxis = MouseYAxis = 0.0f;
        JumpHeld = JumpPressed = JumpReleased = false;

        // check if a controller is connected
        if (Input.GetJoystickNames().Length != 0) {
            InputMode = InputVarient.ControllerMainly;
            HorizontalAxis = Input.GetAxisRaw("XboxLeftStickX");
            VerticalAxis = Input.GetAxisRaw("XboxLeftStickY");
            MouseXAxis = Input.GetAxisRaw("XboxRightStickX");
            MouseYAxis = Input.GetAxisRaw("XboxRightStickY");

            JumpHeld = Input.GetButton("XboxA");
            JumpPressed = Input.GetButtonDown("XboxA");
            JumpReleased = Input.GetButtonUp("XboxA");
        } else InputMode = InputVarient.KeyboardOnly;

        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 1.0f) HorizontalAxis = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1.0f) VerticalAxis = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(Input.GetAxisRaw("Mouse X")) > 0.0f) MouseXAxis = Input.GetAxisRaw("Mouse X");
        if (Mathf.Abs(Input.GetAxisRaw("Mouse Y")) > 0.0f) MouseYAxis = Input.GetAxisRaw("Mouse Y");

        JumpHeld |= Input.GetButton("Jump");
        JumpPressed |= Input.GetButtonDown("Jump");
        JumpReleased |= Input.GetButtonUp("Jump");
    }
}
