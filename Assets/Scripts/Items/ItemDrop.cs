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

    [Header("Effects")]
    [SerializeField] private GameObject pickupEffectPrefab;

    private Vector3 startPosition;
    private float floatTimer = 0f;

    void Start()
    {
        startPosition = transform.position;

        // ★★★ SpriteRenderer 자동 찾기
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

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
        // ★★★ Null 체크 추가
        if (collision == null)
        {
            Debug.LogWarning("[ITEM DROP] Collision is null!");
            return;
        }
        if (collision.CompareTag("Player"))
        {
            PickUp(collision.gameObject);
        }
    }

    void PickUp(GameObject player)
    {
        // ★★★ player null 체크
        if (player == null)
        {
            Debug.LogError("[ITEM DROP] Player GameObject is null!");
            Destroy(gameObject);
            return;
        }

        // ★★★ itemData null 체크
        if (itemData == null)
        {
            Debug.LogError($"[ITEM DROP] {gameObject.name} PickUp called but itemData is null!");
            Destroy(gameObject);
            return;
        }
        Debug.Log($"[ITEM DROP] Player picked up: {itemData.itemName}");
        // ★★★ 아이템 획득 소리 추가 ★★★
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayItemPickupSFX();
        }
        // ★★★ 파티클 이펙트 추가 ★★★
        if (pickupEffectPrefab != null)
        {
            GameObject effect = Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }
        // 인벤토리에 추가
        Inventory inventory = player.GetComponent<Inventory>();
        if (inventory == null)
        {
            // 다시 찾기 시도
            inventory = player.GetComponentInChildren<Inventory>();
        }
        if (inventory == null)
        {
            // GameObject.Find로 마지막 시도
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                inventory = playerObj.GetComponent<Inventory>();
            }
        }
        if (inventory == null)
        {
            Debug.LogError($"[ITEM DROP] ❌❌❌ Player has no Inventory component! ❌❌❌");
            Debug.LogError("[ITEM DROP] Please add Inventory script to Player!");
            Debug.LogError("[ITEM DROP] Item will be destroyed without being added.");

            // 아이템은 제거 (무한 획득 방지)
            Destroy(gameObject);
            return;
        }
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
        // SpriteRenderer 찾기
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        if (spriteRenderer != null && data != null && data.icon != null)
        {
            spriteRenderer.sprite = data.icon;
        }
    }
}
