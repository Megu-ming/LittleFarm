using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("세팅 데이터")]
    [SerializeField] float _moveSpeed = 5f;

    [Header("컴포넌트 참조")]
    [SerializeField] Transform _cameraTransform;

    CharacterController _cc;
    PlayerInput _playerInput;
    Vector2 _moveInput;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        if (!_cameraTransform && Camera.main) _cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // 1) 입력 읽기 (Input System에서 "Player/Move" 액션)
        var moveAction = _playerInput.actions.FindAction("Move");
        if (moveAction != null) _moveInput = moveAction.ReadValue<Vector2>();
        else _moveInput = Vector2.zero;

        // 2) 카메라 기준 전/우 벡터
        Vector3 forward = Vector3.forward, right = Vector3.right;
        if (_cameraTransform)
        {
            forward = Vector3.Scale(_cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
            right = Vector3.Scale(_cameraTransform.right, new Vector3(1, 0, 1)).normalized;
        }

        // 3) 이동 방향(정규화) 계산
        Vector3 dir = (forward * _moveInput.y + right * _moveInput.x);
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        // 4) 캐릭터 이동 (중력/점프는 다음 스텝에서)
        _cc.Move(dir * _moveSpeed * Time.deltaTime);

        // 5) 바라보는 방향: 입력 있을 때만 회전
        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }
}
