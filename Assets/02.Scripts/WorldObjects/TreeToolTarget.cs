using UnityEngine;

public class TreeToolTarget : PlacedObject, IToolTarget, IInteractable
{
    [Header("설정 데이터")]
    [SerializeField] float _maxHealth = 3f;
    [SerializeField] bool _destroyOnZero = true;

    [Header("드랍 설정")]
    [SerializeField] string _dropItemKey = "WOOD";
    [SerializeField] int _dropCountMin = 1;
    [SerializeField] int _dropCountMax = 3;
    [SerializeField] Vector3 _dropOffset = new Vector3(0f, 0.5f, 0f);

    [Header("현재 설정")]
    [SerializeField] float _currentHealth;

    Animator _animator;

    private void Awake()
    {
        if(_currentHealth <= 0f)
            _currentHealth = _maxHealth;
        _animator = GetComponent<Animator>();
    }

    public void OnToolAction(ToolActionContext context)
    {
        if(context.toolType == ToolType.Axe && _currentHealth > 0)
        {
            _currentHealth -= context.power;
            _animator.SetTrigger("Hit");
            Debug.Log($"[Tree] {name} 맞음! 남은 HP {_currentHealth}");

            if(_currentHealth <=0)
            {
                //DropItems();
                if(_destroyOnZero)
                {
                    _animator.SetTrigger("Fallen");
                }
            }
        }
    }

    /// <summary>
    /// 애니메이션 끝에서 호출됨
    /// 아이템 드랍하면서 오브젝트 삭제까지
    /// </summary>
    private void DropItems()
    {
        GameManager.Instance.ObjectManager.DropItems
            (_dropItemKey, transform.position, _dropOffset, _dropCountMin, _dropCountMax);

        Destroy(gameObject);
    }

    public void Interact(PlayerInteraction interactor)
    {
        _animator.SetTrigger("Hit");
    }
}
