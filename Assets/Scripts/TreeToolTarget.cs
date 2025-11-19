using UnityEngine;

public class TreeToolTarget : MonoBehaviour, IToolTarget
{
    [Header("설정 데이터")]
    [SerializeField] float _maxHealth = 3f;
    [SerializeField] bool _destroyOnZero = true;

    [Header("드랍 설정")]
    [SerializeField] GameObject _itemDropPrefab;
    [SerializeField] int _dropCount = 1;
    [SerializeField] Vector3 _dropOffset = new Vector3(0f, 0.5f, 0f);

    [Header("현재 설정")]
    [SerializeField] float _currentHealth;

    private void Awake()
    {
        if(_currentHealth <= 0f)
            _currentHealth = _maxHealth;
    }

    public void OnToolAction(ToolActionContext context)
    {
        if(context.toolType == ToolType.Axe)
        {
            _currentHealth -= context.power;
            Debug.Log($"[Tree] {name} 맞음! 남은 HP {_currentHealth}");

            if(_currentHealth <=0)
            {
                DropItems();
                if(_destroyOnZero)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    private void DropItems()
    {
        if (_itemDropPrefab == null || _dropCount <= 0)
            return;

        for(int i=0;i<_dropCount;i++)
        {
            Vector2 rand = Random.insideUnitCircle * 0.3f;
            Vector3 pos = transform.position + _dropOffset + new Vector3(rand.x, 0f, rand.y);

            Instantiate(_itemDropPrefab, pos, Quaternion.identity);
        }
    }
}
