using UnityEngine;

public class TreeToolTarget : MonoBehaviour, IToolTarget, IInteractable
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
        if(context.toolType == ToolType.Axe)
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
        var db = GameInitializer.Instance.Database;
        if(db == null )
        {
            Debug.LogWarning("[TreeToolTarget] ItemDatabase가 없습니다.");
            return;
        }

        if(string.IsNullOrEmpty(_dropItemKey ) )
        {
            Debug.LogWarning("[TreeToolTarget] 드랍 아이템 Key가 없습니다.");
            return;
        }

        ItemSpec spec = db.GetByKey(_dropItemKey);
        if(spec == null)
        {
            Debug.LogWarning($"[TreeToolTarget] 드랍 아이템을 찾을 수 없습니다. key = {_dropItemKey}");
            return;
        }

        int itemId = spec.id;

        int dropCount = Random.Range(_dropCountMin, _dropCountMax + 1);
        if (dropCount <= 0) return;

        GameObject prefab = Resources.Load<GameObject>($"ItemDrops/{spec.key}");
        if(prefab == null)
        {
            Debug.LogWarning($"[TreeToolTarget] 프리팹을 찾을 수 없습니다: Resources/ItemDrops/{spec.key}");
            return;
        }

        for(int i = 0; i < dropCount; i++)
        {
            Vector2 rand = Random.insideUnitCircle * 0.3f;
            Vector3 pos = transform.position + _dropOffset + new Vector3(rand.x, 0, rand.y);

            GameObject go = Instantiate(prefab, pos, Quaternion.identity);
            
            var pickup = go.GetComponent<ItemPickup>();
            if(pickup != null)
            {
                pickup.Setup(itemId, 1);
                pickup.PlayDropEffect();
            }
        }

        Destroy(gameObject);
    }

    public void Interact(PlayerInteraction interactor)
    {
        _animator.SetTrigger("Hit");
    }
}
