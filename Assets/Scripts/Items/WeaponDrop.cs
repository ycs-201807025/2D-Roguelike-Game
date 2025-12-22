using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 무기 드롭 오브젝트
/// </summary>
public class WeaponDrop : MonoBehaviour
{
    [Header("Weapon Data")]
    [SerializeField] private WeaponData weaponData;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer glowRenderer; // 등급별 빛
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatHeight = 0.3f;
    [SerializeField] private float rotateSpeed = 50f;

    [Header("Effects")]
    [SerializeField] private GameObject pickupEffectPrefab;
    [SerializeField] private ParticleSystem rarityParticle; // 등급별 파티클

    private Vector3 startPosition;
    private float floatTimer = 0f;
    private bool isPickedUp = false;

    void Start()
    {
        startPosition = transform.position;

        // SpriteRenderer 자동 찾기
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        // 무기 아이콘 설정
        if (spriteRenderer != null && weaponData != null && weaponData.weaponIcon != null)
        {
            spriteRenderer.sprite = weaponData.weaponIcon;
        }

        // 등급별 빛 효과
        SetupRarityEffect();
    }

    void Update()
    {
        if (isPickedUp) return;

        // 위아래로 부드럽게 움직임
        floatTimer += Time.deltaTime * floatSpeed;
        float newY = startPosition.y + Mathf.Sin(floatTimer) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // 회전
        transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 등급별 효과 설정
    /// </summary>
    void SetupRarityEffect()
    {
        if (weaponData == null) return;

        Color rarityColor = weaponData.GetRarityColor();

        // 글로우 효과
        if (glowRenderer != null)
        {
            glowRenderer.color = rarityColor;
        }

        // 파티클 효과
        if (rarityParticle != null)
        {
            var main = rarityParticle.main;
            main.startColor = rarityColor;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPickedUp) return;

        if (collision.CompareTag("Player"))
        {
            PickUp(collision.gameObject);
        }
    }

    void PickUp(GameObject player)
    {
        if (isPickedUp) return;
        isPickedUp = true;

        if (weaponData == null)
        {
            Debug.LogError($"[WEAPON DROP] {gameObject.name} has no WeaponData!");
            Destroy(gameObject);
            return;
        }

        Debug.Log($"[WEAPON DROP] Player picked up: {weaponData.weaponName} ({weaponData.GetRarityName()})");

        //UI 표시 
        if (WeaponAcquiredUI.Instance != null)
        {
            WeaponAcquiredUI.Instance.ShowWeaponAcquired(weaponData);
        }

        // 사운드
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayItemPickupSFX();
        }

        // 이펙트
        if (pickupEffectPrefab != null)
        {
            GameObject effect = Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // 무기 장착
        PlayerWeapon playerWeapon = player.GetComponent<PlayerWeapon>();
        if (playerWeapon != null)
        {
            playerWeapon.EquipWeapon(weaponData);
        }
        else
        {
            Debug.LogError("[WEAPON DROP] Player has no PlayerWeapon component!");
        }

        // 오브젝트 제거
        Destroy(gameObject);
    }

    /// <summary>
    /// 초기화 (동적 생성 시)
    /// </summary>
    public void Initialize(WeaponData data)
    {
        if (data == null)
        {
            Debug.LogError("[WEAPON DROP] Initialize called with null WeaponData!");
            Destroy(gameObject);
            return;
        }

        weaponData = data;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null && data.weaponIcon != null)
        {
            spriteRenderer.sprite = data.weaponIcon;
        }

        SetupRarityEffect();

        Debug.Log($"[WEAPON DROP] Initialized: {weaponData.weaponName}");
    }
}
