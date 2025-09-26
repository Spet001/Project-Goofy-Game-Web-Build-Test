

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class WheelchairController : MonoBehaviour
{
    [Header("References")]
    public WheelchairManager manager;
    public Rigidbody hipsRigidbody;
    public Transform[] wheels;
    public Transform seatPosition;
    public Transform riderTransform;
    public CameraFollow cameraFollow;

    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float acceleration = 2f;
    public float turnSpeed = 80f;
    public float tiltRecoverySpeed = 5f;
    public float wheelRotationSpeed = 180f;
    public float maxTiltAngle = 25f;

    [Header("Jump/Float Settings")]
    public float jumpForce = 5f;
    public float floatForce = 2f;
    public float floatDuration = 3.3f;

    [Header("Ragdoll Settings")]
    public float ejectionForce = 8f;
    public float ragdollDuration = 3f;

    private Rigidbody rb;
    private PlayerInputActions controls;
    private Vector2 input;
    private ConfigurableJoint hipsJoint;
    private float currentSpeed;
    private bool isFloating;
    private float floatTimer;
    private bool isRagdoll;
    private Vector3 riderLocalPos = new Vector3(-0.02f, -0.664f, 0.39f);

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controls = new PlayerInputActions();

        controls.Cadeirante.Move.performed += ctx => input = ctx.ReadValue<Vector2>();
        controls.Cadeirante.Move.canceled += ctx => input = Vector2.zero;
        controls.Cadeirante.Jump.performed += ctx => StartJump();
        controls.Cadeirante.Jump.canceled += ctx => StopFloat();
    }

    void Start()
    {
        InitializePositions();
        MountRider();
    }

    void InitializePositions()
    {
        riderTransform.localPosition = riderLocalPos;
        riderTransform.localRotation = Quaternion.identity;
    }

    void FixedUpdate()
    {
        if (isRagdoll) return;

        HandleMovement();
        HandleWheelRotation();
        HandleTilt();
        HandleFloat();
    }

    void Update()
    {
        if (!isRagdoll && Vector3.Angle(Vector3.up, transform.up) > 60f)
        {
            EjectRider();
        }
    }

    void HandleMovement()
    {
        if (isFloating) return;

        float targetSpeed = input.y * moveSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector3(
            transform.forward.x * currentSpeed,
            rb.linearVelocity.y,
            transform.forward.z * currentSpeed
        );

        rb.AddTorque(transform.up * (input.x * turnSpeed * Time.fixedDeltaTime), ForceMode.VelocityChange);
    }

    void HandleWheelRotation()
    {
        float rotation = currentSpeed * wheelRotationSpeed * Time.fixedDeltaTime;
        foreach (var wheel in wheels)
        {
            if (wheel) wheel.Rotate(rotation, 0, 0);
        }
    }

    void HandleTilt()
    {
        float tilt = isFloating ? 0 : Mathf.Clamp(currentSpeed * 2f, -maxTiltAngle, maxTiltAngle);
        Quaternion targetRot = Quaternion.Euler(tilt, rb.rotation.eulerAngles.y, 0);
        rb.MoveRotation(Quaternion.Lerp(rb.rotation, targetRot, tiltRecoverySpeed * Time.fixedDeltaTime));
    }

    void StartJump()
    {
        if (isFloating || isRagdoll || !IsGrounded()) return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isFloating = true;
        floatTimer = floatDuration;
    }

    void HandleFloat()
    {
        if (!isFloating) return;

        rb.AddForce(Vector3.up * floatForce, ForceMode.Acceleration);
        if ((floatTimer -= Time.fixedDeltaTime) <= 0) StopFloat();
    }

    void StopFloat() => isFloating = false;

    bool IsGrounded() => Physics.Raycast(transform.position, Vector3.down, 0.2f);

    void MountRider()
    {
        if (hipsJoint) Destroy(hipsJoint);

        hipsJoint = hipsRigidbody.gameObject.AddComponent<ConfigurableJoint>();
        hipsJoint.connectedBody = rb;
        hipsJoint.connectedAnchor = seatPosition.localPosition;

        // Configuração de posição
        hipsJoint.xMotion = hipsJoint.yMotion = hipsJoint.zMotion = ConfigurableJointMotion.Locked;
        JointDrive drive = new JointDrive
        {
            positionSpring = 20000f,
            positionDamper = 1000f,
            maximumForce = Mathf.Infinity
        };
        hipsJoint.xDrive = hipsJoint.yDrive = hipsJoint.zDrive = drive;

        // Configuração de rotação
        hipsJoint.angularXMotion = hipsJoint.angularYMotion = hipsJoint.angularZMotion = ConfigurableJointMotion.Free;
        hipsJoint.rotationDriveMode = RotationDriveMode.Slerp;
        hipsJoint.slerpDrive = new JointDrive
        {
            positionSpring = 500f,
            positionDamper = 50f,
            maximumForce = Mathf.Infinity
        };
    }

    public void EjectRider()
    {
        if (isRagdoll) return;

        isRagdoll = true;
        Destroy(hipsJoint);

        // Aplica força de ejeção
        Vector3 dir = (hipsRigidbody.position - transform.position).normalized + Vector3.up;
        hipsRigidbody.AddForce(dir * ejectionForce, ForceMode.Impulse);

        // Desativa controles
        controls.Disable();

        // Configura câmera
        if (cameraFollow)
        {
            cameraFollow.SwitchToPlayerFocus();
            cameraFollow.smoothSpeed = 3f;
        }

        StartCoroutine(RespawnAfterDelay());
    }

    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(ragdollDuration);
        manager.RespawnWheelchair();
    }

    void OnDestroy() => controls.Disable();
    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();
}