using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 무기 테스트 및 디버그 도구
/// </summary>
public class WeaponDebugManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject debugPanel;
    [SerializeField] private Transform weaponButtonContainer;
    [SerializeField] private GameObject weaponButtonPrefab;
    [SerializeField] private TextMeshProUGUI infoText;

    [Header("Toggle")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;

    [Header("Weapon Drop")]
    [SerializeField] private GameObject weaponDropPrefab;
    [SerializeField] private Transform dropPoint;

    private WeaponData[] allWeapons;
    private PlayerWeapon playerWeapon;
    private bool isPanelActive = false;

    void Start()
    {
        // 플레이어 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerWeapon = player.GetComponent<PlayerWeapon>();
        }

        // 드롭 포인트 설정
        if (dropPoint == null && player != null)
        {
            dropPoint = player.transform;
        }

        // 모든 무기 로드
        LoadAllWeapons();

        // UI 생성
        CreateWeaponButtons();

        // 처음엔 숨김
        if (debugPanel != null)
        {
            debugPanel.SetActive(false);
        }

        Debug.Log("[WEAPON DEBUG] Debug Manager initialized");
    }

    void Update()
    {
        // F1키로 디버그 패널 토글
        if (Input.GetKeyDown(toggleKey))
        {
            TogglePanel();
        }

        // 단축키들
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SpawnRandomWeapon(WeaponRarity.Common);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SpawnRandomWeapon(WeaponRarity.Uncommon);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SpawnRandomWeapon(WeaponRarity.Rare);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SpawnRandomWeapon(WeaponRarity.Epic);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SpawnRandomWeapon(WeaponRarity.Legendary);
        }

        // 정보 업데이트
        UpdateInfoText();
    }

    /// <summary>
    /// 모든 무기 로드
    /// </summary>
    void LoadAllWeapons()
    {
        allWeapons = Resources.LoadAll<WeaponData>("ScriptableObjects/Weapons");
        Debug.Log($"[WEAPON DEBUG] Loaded {allWeapons.Length} weapons");
    }

    /// <summary>
    /// 무기 버튼 생성
    /// </summary>
    void CreateWeaponButtons()
    {
        if (weaponButtonContainer == null || weaponButtonPrefab == null)
        {
            Debug.LogWarning("[WEAPON DEBUG] Button container or prefab not assigned");
            return;
        }

        // 기존 버튼 삭제
        foreach (Transform child in weaponButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // 각 무기에 대한 버튼 생성
        foreach (WeaponData weapon in allWeapons)
        {
            GameObject buttonObj = Instantiate(weaponButtonPrefab, weaponButtonContainer);

            // 버튼 텍스트 설정
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"{weapon.weaponName}\n({weapon.GetRarityName()})";
                buttonText.color = weapon.GetRarityColor();
            }

            // 버튼 이벤트
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                WeaponData weaponData = weapon; // 클로저 문제 방지
                button.onClick.AddListener(() => OnWeaponButtonClick(weaponData));
            }
        }
    }

    /// <summary>
    /// 무기 버튼 클릭 시
    /// </summary>
    void OnWeaponButtonClick(WeaponData weapon)
    {
        if (weapon == null)
        {
            Debug.LogError("[WEAPON DEBUG] Weapon data is null");
            return;
        }

        Debug.Log($"[WEAPON DEBUG] Spawning weapon: {weapon.weaponName}");

        // 1. 직접 장착
        if (playerWeapon != null && Input.GetKey(KeyCode.LeftShift))
        {
            playerWeapon.EquipWeapon(weapon);
            Debug.Log($"[WEAPON DEBUG] Equipped directly: {weapon.weaponName}");
        }
        // 2. 드롭 생성
        else
        {
            SpawnWeaponDrop(weapon);
        }
    }

    /// <summary>
    /// 무기 드롭 생성
    /// </summary>
    void SpawnWeaponDrop(WeaponData weapon)
    {
        if (weapon == null || dropPoint == null)
        {
            Debug.LogError("[WEAPON DEBUG] Cannot spawn - weapon or dropPoint is null");
            return;
        }

        // 드롭 오브젝트 생성
        Vector3 spawnPos = dropPoint.position + new Vector3(2f, 0f, 0f); // 플레이어 오른쪽에 생성

        GameObject dropObj = new GameObject($"WeaponDrop_{weapon.weaponName}");
        dropObj.transform.position = spawnPos;
        dropObj.layer = LayerMask.NameToLayer("Default");

        // WeaponDrop 컴포넌트
        WeaponDrop drop = dropObj.AddComponent<WeaponDrop>();
        drop.Initialize(weapon);

        // SpriteRenderer
        SpriteRenderer sr = dropObj.AddComponent<SpriteRenderer>();
        if (weapon.weaponIcon != null)
        {
            sr.sprite = weapon.weaponIcon;
        }
        sr.sortingOrder = 10;
        sr.color = weapon.GetRarityColor();

        // Collider
        CircleCollider2D col = dropObj.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.8f;

        Debug.Log($"[WEAPON DEBUG] Spawned weapon drop: {weapon.weaponName} at {spawnPos}");
    }

    /// <summary>
    /// 랜덤 무기 생성 (등급별)
    /// </summary>
    void SpawnRandomWeapon(WeaponRarity rarity)
    {
        List<WeaponData> weapons = new List<WeaponData>();

        foreach (var weapon in allWeapons)
        {
            if (weapon.rarity == rarity)
            {
                weapons.Add(weapon);
            }
        }

        if (weapons.Count == 0)
        {
            Debug.LogWarning($"[WEAPON DEBUG] No weapons of rarity {rarity}");
            return;
        }

        WeaponData randomWeapon = weapons[Random.Range(0, weapons.Count)];
        SpawnWeaponDrop(randomWeapon);

        Debug.Log($"[WEAPON DEBUG] Spawned random {rarity} weapon: {randomWeapon.weaponName}");
    }

    /// <summary>
    /// 정보 텍스트 업데이트
    /// </summary>
    void UpdateInfoText()
    {
        if (infoText == null) return;

        string info = "=== 무기 디버그 도구 ===\n\n";
        info += $"[{toggleKey}] 패널 토글\n";
        info += "[1-5] 등급별 랜덤 무기 생성\n";
        info += "[Shift + 클릭] 즉시 장착\n";
        info += "[클릭] 드롭 생성\n\n";

        if (playerWeapon != null)
        {
            // 현재 장착 무기 정보는 PlayerWeapon에서 가져올 수 없으므로 생략
            info += "현재 장착: (확인 필요)\n";
        }

        info += $"총 무기 수: {allWeapons.Length}개\n";

        infoText.text = info;
    }

    /// <summary>
    /// 패널 토글
    /// </summary>
    void TogglePanel()
    {
        if (debugPanel == null) return;

        isPanelActive = !isPanelActive;
        debugPanel.SetActive(isPanelActive);

        // 패널이 열릴 때 게임 일시정지 (선택사항)
        // Time.timeScale = isPanelActive ? 0f : 1f;

        Debug.Log($"[WEAPON DEBUG] Panel {(isPanelActive ? "opened" : "closed")}");
    }

    /// <summary>
    /// 모든 무기 정보 출력 (디버그용)
    /// </summary>
    [ContextMenu("Print All Weapons")]
    public void PrintAllWeapons()
    {
        Debug.Log("=== ALL WEAPONS ===");

        foreach (var weapon in allWeapons)
        {
            Debug.Log($"{weapon.weaponName} ({weapon.GetRarityName()}) - DMG:{weapon.damage}, SPD:{weapon.attackSpeed}");
        }
    }
}
