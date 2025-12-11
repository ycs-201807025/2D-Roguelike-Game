using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아이템 드롭 오브젝트
/// </summary>
public class ItemDrop : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private ItemData itemData;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatHeight = 0.3f;

    private Vector3 startPosition;
    private float floatTimer = 0f;

    void Start()
    {
        startPosition = transform.position;

        // 스프라이트 설정
        if (spriteRenderer != null && itemData != null && itemData.icon != null)
        {
            spriteRenderer.sprite = itemData.icon;
        }
    }

    void Update()
    {
        // 위아래로 부드럽게 움직임
        floatTimer += Time.deltaTime * floatSpeed;
        float newY = startPosition.y + Mathf.Sin(floatTimer) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PickUp(collision.gameObject);
        }
    }

    void PickUp(GameObject player)
    {
        Debug.Log($"[ITEM DROP] Player picked up: {itemData.itemName}");

        // 인벤토리에 추가
        Inventory inventory = player.GetComponent<Inventory>();
        if (inventory != null)
        {
            inventory.AddItem(itemData);
        }

        // 아이템 제거
        Destroy(gameObject);
    }

    /// <summary>
    /// 초기화 (동적 생성 시 사용)
    /// </summary>
    public void Initialize(ItemData data)
    {
        itemData = data;

        if (spriteRenderer != null && data != null && data.icon != null)
        {
            spriteRenderer.sprite = data.icon;
        }
    }
}
