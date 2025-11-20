using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("세팅 데이터")]
    [SerializeField] float _moveSpeed = 5f;
    float _cacheMoveSpeed;
    [SerializeField] float _rotationSpeed = 10f;

    [Header("컴포넌트 참조")]
    [SerializeField] Player _player;
    [SerializeField] Animator _animator;

    CharacterController _cc;
    Vector2 _moveInput;
    bool _isInitialized = false;

    public void Initialize(Player player, CharacterController characterController, Animator animator)
    {
        _player = player;
        _cc = characterController;
        _animator = animator;

        _isInitialized = true;
    }

    void Update()
    {
        if (_isInitialized)
            HandleMove(Time.deltaTime);
    }

    public void Move(Vector2 moveInput)
    {
        _moveInput = moveInput;
    }

    void HandleMove(float deltaTime)
    {
        if (_player.CurrentState == PlayerState.Acting) return;

        Vector3 inputDir = new Vector3(_moveInput.x, 0f, _moveInput.y);

        if (inputDir.sqrMagnitude>1f)
            inputDir.Normalize();

        Vector3 horizontalMove = inputDir * _moveSpeed;
        _cc.Move(horizontalMove * deltaTime);

        if(inputDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _rotationSpeed * deltaTime);
        }
        if (_animator != null)
        {
            float speedParam = inputDir.magnitude;
            _animator.SetFloat("Speed", speedParam);
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _cacheMoveSpeed = _moveSpeed;
            _moveSpeed *= 0.5f; 
        }

        if (context.canceled)
            _moveSpeed = _cacheMoveSpeed;
    }
}
