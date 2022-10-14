using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;

public class SimpleCharacterMovement : MonoBehaviour
{
    [Header("Move")]
    public float movingSpeed = 5;
    public float runningSpeedFactor = 1.5f;
    public float rotationSpeed = 3;
    public bool holdShiftToRun = false;
    private bool isRunning = false;
    private Vector2 direction;

    private Animator _animator;
    
    
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (direction.magnitude != 0)
        {
            _animator.SetFloat("Speed", isRunning?5:3);
            float speed = movingSpeed * (isRunning ? runningSpeedFactor : 1);
            transform.Translate(0, 0, speed*direction.y*Time.deltaTime);
            transform.Rotate( 0,rotationSpeed*direction.x*Time.deltaTime, 0);
        }
        else
        {
            _animator.SetFloat("Speed", 0);
        }
    }

    public void Move(InputAction.CallbackContext ctx)
    {
        direction = ctx.ReadValue<Vector2>();
    }

    public void Run(InputAction.CallbackContext ctx)
    {
        isRunning = holdShiftToRun && ctx.ReadValueAsButton();
    }

}
