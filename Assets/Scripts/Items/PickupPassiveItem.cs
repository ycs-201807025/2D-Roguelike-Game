using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 패시브 아이템 드롭 오브젝트
/// </summary>
public class PickupPassiveItem : MonoBehaviour
{
    [Header("Item Data")]
    public PassiveItemData passiveItem;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatHeight = 0.3f;
    [SerializeField] private float pickupRange = 2f;

    [Header("Effects")]
    [SerializeField] private GameObject pickupEffectPrefab;

    private Transform player;
    private Vector3 startPosition;
    private float floatTimer = 0f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        startPosition = transform.position;

        // 스프라이트 설정
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null && passiveItem != null && passiveItem.icon != null)
        {
            spriteRenderer.sprite = passiveItem.icon;
        }
    }

    void Update()
    {
        // 위아래로 부드럽게 움직임
        floatTimer += Time.deltaTime * floatSpeed;
        float newY = startPosition.y + Mathf.Sin(floatTimer) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // 플레이어가 가까우면 자동으로 이동
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist < pickupRange)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    player.position,
                    5f * Time.deltaTime
                );
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PickUp();
        }
    }

    void PickUp()
    {
        if (passiveItem == null)
        {
            Debug.LogError("[PICKUP PASSIVE] passiveItem is null!");
            Destroy(gameObject);
            return;
        }

        Debug.Log($"[PICKUP PASSIVE] Player picked up: {passiveItem.itemName}");

        // 사운드
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayItemPickupSFX();
        }

        // 이펙트
        if (pickupEffectPrefab != null)
        {
            GameObject effect = Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }

        // PassiveItemManager에 추가
        if (PassiveItemManager.Instance != null)
        {
            PassiveItemManager.Instance.AddItem(passiveItem);
        }
        else
        {
            Debug.LogError("[PICKUP PASSIVE] PassiveItemManager.Instance is null!");
        }

        // 오브젝트 제거
        Destroy(gameObject);
    }
}
