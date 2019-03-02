using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(InputMapper))]
public class Controller : MonoBehaviour {
    [Header("Mouse look")]
    public bool cursorLock = false;
    public Transform cameraChild;
    public Vector2 mouseSensitivity = Vector2.one;
    public Vector2 mouseSmoothing = Vector2.one;

    [Header("Planar movement")]
    public AnimationCurve accelerationCurve = null;
    public float timeToAccelerate = 1.0f;
    public float timeToDeccelerate = 0.5f;
    public float maxSpeed = 1.0f;

    [Header("Vertical movement")]
    public float jumpHeight = 1.0f;

    [Header("Misc.")]
    public LayerMask floorLayerMask;
    public Collidable footCollider;

    CharacterController m_characterController = null;
    InputMapper m_inputMapper = null;
    Vector2 m_inputInterpolator = Vector2.zero;
    Vector2 m_lastInputDown = Vector2.zero;
    Vector3 m_velocity = Vector3.zero;
    Vector3 m_mouseDirection;
    Vector3 m_cleanDirection;
    bool m_grounded = false;
    float m_jumpForce;

    void Start() {
        m_characterController = GetComponent<CharacterController>();
        m_inputMapper = GetComponent<InputMapper>();

        // from the SUVAT equation:
        // v^2 = u^2 + 2as where u = 0
        // thanks chillwell.
        m_jumpForce = Mathf.Sqrt(jumpHeight * Physics.gravity.y * -2.0f);
    }

    void Awake() {
        if (footCollider == null) throw new MissingReferenceException();
    }

    // calculate the magnitude from the current velocity
    float EvaluateAcceleration(float x) {
        return (x < 0.0f) ? (x < -1.0f) ? -1.0f : -accelerationCurve.Evaluate(-x) :
                            (x > +1.0f) ? +1.0f : +accelerationCurve.Evaluate(+x);
    }

    // detect grounded state
    bool IsGrounded() {
        return footCollider.IsColliding(floorLayerMask) || m_characterController.isGrounded;
    }

    Vector3 CalculateInputDirection() {
        // generate an input vector
        Vector2 input = new Vector2(m_inputMapper.HorizontalAxis, m_inputMapper.VerticalAxis);

        // however for keyboard movement there is a problem:
        // if the opposite direction keys are held down at the same time
        // e.g. s and w, then we expect to move according to that key which..
        // ..was pressed more recently, and hence need a store key state:
        if (m_inputMapper.InputMode == InputMapper.InputVarient.KeyboardOnly || input.sqrMagnitude == 0.0f) {

            float bool2Float(bool x) => x ? 1.0f : 0.0f;
            float assignNotZero(float was, float to) => (to == 0.0f) ? was : to;

            float isLeft(System.Func<KeyCode, bool> accessor) => bool2Float(accessor(KeyCode.LeftArrow) || accessor(KeyCode.A));
            float isRight(System.Func<KeyCode, bool> accessor) => bool2Float(accessor(KeyCode.RightArrow) || accessor(KeyCode.D));
            float isUp(System.Func<KeyCode, bool> accessor) => bool2Float(accessor(KeyCode.UpArrow) || accessor(KeyCode.W));
            float isDown(System.Func<KeyCode, bool> accessor) => bool2Float(accessor(KeyCode.DownArrow) || accessor(KeyCode.S));

            m_lastInputDown.x = assignNotZero(m_lastInputDown.x, isRight(Input.GetKeyDown) - isLeft(Input.GetKeyDown));
            m_lastInputDown.y = assignNotZero(m_lastInputDown.y, isUp(Input.GetKeyDown) - isDown(Input.GetKeyDown));

            // correct for double opposite direction inputs
            if (input.x == 0 && Input.GetButton("Horizontal")) input.x = m_lastInputDown.x;
            if (input.y == 0 && Input.GetButton("Vertical")) input.y = m_lastInputDown.y;
        }

        // need to not run through f(x) when changing direction across 0
        // in other words, the current interpolator and target input have the same sign
        int signF(float x) => (x == 0.0f) ? 0 : (x > 0.0f) ? 1 : -1;
        const int X_ = 0; const int Y_ = 1;
        bool[] isAccelerating = {
            signF(input.x) * signF(m_inputInterpolator.x) > 0,
            signF(input.y) * signF(m_inputInterpolator.y) > 0 }; // x, y

        // linearly interpolate toward the target input vector
        m_inputInterpolator = new Vector2(
            Mathf.Lerp(m_inputInterpolator.x, input.x, Time.deltaTime / (isAccelerating[X_] ? timeToAccelerate : timeToDeccelerate)),
            Mathf.Lerp(m_inputInterpolator.y, input.y, Time.deltaTime / (isAccelerating[Y_] ? timeToAccelerate : timeToDeccelerate)));

        // once we've reached it ensure we don't overshoot
        if (Mathf.Abs(m_inputInterpolator.x) > Mathf.Abs(input.x) && input.x != 0.0f) m_inputInterpolator.x = input.x;
        if (Mathf.Abs(m_inputInterpolator.y) > Mathf.Abs(input.y) && input.y != 0.0f) m_inputInterpolator.y = input.y;

        // ensure diagonal movement does not have a magnitude greater than 1
        float pythagoreanNormalizer = 1.0f;

        if (input.sqrMagnitude > 1.0f)
            pythagoreanNormalizer = 1 / input.magnitude;

        // generate the final input after:
        // running the interpolated vector through the acceleration whist accelerating
        // and normalizing;
        return new Vector3(
            (isAccelerating[X_]) ? EvaluateAcceleration(m_inputInterpolator.x) : m_inputInterpolator.x, 0,
            (isAccelerating[Y_]) ? EvaluateAcceleration(m_inputInterpolator.y) : m_inputInterpolator.y
        ) * pythagoreanNormalizer;
    }

    Vector2 CalculateMouseDirection() {
        Vector2 _rawInput = Vector2.Scale(new Vector2(m_inputMapper.MouseXAxis, m_inputMapper.MouseYAxis), mouseSensitivity);

        // interpolate movements
        m_cleanDirection.x = Mathf.Lerp(m_cleanDirection.x, _rawInput.x, 1.0f / mouseSmoothing.x);
        m_cleanDirection.y = Mathf.Lerp(m_cleanDirection.y, _rawInput.y, 1.0f / mouseSmoothing.y);

        m_mouseDirection += m_cleanDirection;

        // clamp vertically
        m_mouseDirection.y = Mathf.Clamp(m_mouseDirection.y, -90.0f, 90.0f);

        return m_mouseDirection;
    }

    // fundamental idea from: https://medium.com/ironequal/unity-character-controller-vs-rigidbody-a1e243591483
    void Update() {

        // look for mouse direction
        Vector2 _mouseLook = CalculateMouseDirection();

        cameraChild.localRotation = Quaternion.AngleAxis(_mouseLook.y, Vector3.left) * Quaternion.Euler(Vector3.left);
        transform.rotation = Quaternion.AngleAxis(_mouseLook.x, Vector3.up) * Quaternion.Euler(Vector3.up);

        // locking
        if (cursorLock)
            Cursor.lockState = CursorLockMode.Locked;

        // move by planar vector
        Vector3 _input = CalculateInputDirection();
        Vector3 _move_dir = transform.rotation * _input;

        m_characterController.Move(
              _move_dir
            * maxSpeed
            * Time.deltaTime);

        // check grounded state
        m_grounded = IsGrounded();

        // gravity
        m_velocity.y += Physics.gravity.y * Time.deltaTime;

        if (m_grounded && m_velocity.y < 0.0f)
            m_velocity.y = 0.0f;

        // jumping
        if (m_grounded && m_inputMapper.JumpPressed)
            m_velocity.y += m_jumpForce;

        m_characterController.Move(m_velocity * Time.deltaTime);
    }
}
