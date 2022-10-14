using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MarioKartController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float accelerationValue;
    [SerializeField] float speedCap, rotationSpeed;

    PlayerInputs _inputs;

    Rigidbody rb;

    GameObject cameraGameObject;


    private void Awake()
    {
        _inputs = new PlayerInputs();
        rb = GetComponent<Rigidbody>();

        cameraGameObject = GetComponentInChildren<Camera>().gameObject;
    }

    private void OnEnable()
    {
        _inputs.Enable();
        _inputs.Default.CameraSwitch.performed += FlipAndUnflipCamera;
        _inputs.Default.CameraSwitch.canceled += FlipAndUnflipCamera;


    }

    private void OnDisable()
    {
        _inputs.Disable();
        _inputs.Default.CameraSwitch.performed -= FlipAndUnflipCamera;
        _inputs.Default.CameraSwitch.canceled -= FlipAndUnflipCamera;
    }



    private void FixedUpdate()
    {
        var _inputMovement = _inputs.Default.Movement.ReadValue<Vector2>();

        bool shouldAccelerate = _inputs.Default.A_Button.ReadValue<float>() == 1 ? true : false;
        bool shouldDeccelerate = _inputs.Default.B_Button.ReadValue<float>() == 1 ? true : false;


        /*
            movement in Mario kart is :
                    - Accelerate in front
                    - Deccelerate (or move backwards)
                    - Turn if going on some position
                        (but can't be done without accelerating)
        */

        if (shouldAccelerate)
        {
            Accelerate(_inputMovement);




        }
        else if (shouldDeccelerate)
        {
            Deccelerate(_inputMovement);
        }


        // Reset rotation
        rb.angularVelocity = Vector3.zero;


        // Speed Cap
        if (rb.velocity.sqrMagnitude > (speedCap * speedCap))
        {
            rb.velocity = rb.velocity.normalized * speedCap;
        }

    }


    void Accelerate(Vector3 _inputMovement)
    {
        // Create the acceleration
        Vector3 accelerationVector = transform.forward * accelerationValue;


        // Check if the player wants to turn
        if (_inputMovement.x != 0)
        {
            // Check which direction
            float _movementDirection = Mathf.Sign(_inputMovement.x);


            // - => turn left       + => turn right
            transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed * _movementDirection);
        }

        rb.AddForce(accelerationVector);
    }

    void Deccelerate(Vector3 _inputMovement)
    {
        // Create the acceleration
        Vector3 accelerationVector = -transform.forward * accelerationValue * .25f;


        // Check if the player wants to turn
        if (_inputMovement.x != 0)
        {
            // Check which direction
            float _movementDirection = -Mathf.Sign(_inputMovement.x);


            // - => turn left       + => turn right
            transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed * _movementDirection);
        }

        rb.AddForce(accelerationVector);
    }

    void FlipAndUnflipCamera(InputAction.CallbackContext c)
    {
        cameraGameObject.transform.localPosition = new Vector3(cameraGameObject.transform.localPosition.x, cameraGameObject.transform.localPosition.y, -cameraGameObject.transform.localPosition.z);
        cameraGameObject.transform.eulerAngles = new Vector3(cameraGameObject.transform.eulerAngles.x, cameraGameObject.transform.eulerAngles.y + 180, cameraGameObject.transform.eulerAngles.z);
    }


}
