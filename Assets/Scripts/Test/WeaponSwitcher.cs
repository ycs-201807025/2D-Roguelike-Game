using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 무기 변경 테스트용 스크립트
/// 나중에 UI나 아이템 획득으로 대체
/// </summary>
public class WeaponSwitcher : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] private WeaponData[] weapons; // 사용 가능한 무기들

    private PlayerWeapon playerWeapon;
    private int currentWeaponIndex = 0;

    void Awake()
    {
        playerWeapon = GetComponent<PlayerWeapon>();
    }

    void Start()
    {
        // 첫 번째 무기로 시작
        if (weapons.Length > 0)
        {
            playerWeapon.ChangeWeapon(weapons[0]);
        }
    }

    void Update()
    {
        // 숫자 키로 무기 변경
        if (Input.GetKeyDown(KeyCode.Alpha1) && weapons.Length > 0)
        {
            ChangeWeapon(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && weapons.Length > 1)
        {
            ChangeWeapon(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && weapons.Length > 2)
        {
            ChangeWeapon(2);
        }

        // 마우스 휠로 순환
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) // 휠 위로
        {
            NextWeapon();
        }
        else if (scroll < 0f) // 휠 아래로
        {
            PreviousWeapon();
        }
    }

    /// <summary>
    /// 특정 무기로 변경
    /// </summary>
    private void ChangeWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length) return;

        currentWeaponIndex = index;
        playerWeapon.ChangeWeapon(weapons[currentWeaponIndex]);

        Debug.Log($"무기 변경: {weapons[currentWeaponIndex].weaponName} (키: {index + 1})");
    }

    /// <summary>
    /// 다음 무기
    /// </summary>
    private void NextWeapon()
    {
        if (weapons.Length == 0) return;

        currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Length;
        playerWeapon.ChangeWeapon(weapons[currentWeaponIndex]);

        Debug.Log($"무기 변경: {weapons[currentWeaponIndex].weaponName}");
    }

    /// <summary>
    /// 이전 무기
    /// </summary>
    private void PreviousWeapon()
    {
        if (weapons.Length == 0) return;

        currentWeaponIndex--;
        if (currentWeaponIndex < 0)
        {
            currentWeaponIndex = weapons.Length - 1;
        }

        playerWeapon.ChangeWeapon(weapons[currentWeaponIndex]);

        Debug.Log($"무기 변경: {weapons[currentWeaponIndex].weaponName}");
    }

    // UI 표시 (임시)
    void OnGUI()
    {
        if (weapons.Length == 0) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 14;
        style.normal.textColor = Color.white;

        string weaponInfo = $"현재 무기: {weapons[currentWeaponIndex].weaponName}\n";
        weaponInfo += "무기 변경: 1, 2, 3 또는 마우스 휠";

        GUI.Label(new Rect(10, 70, 300, 50), weaponInfo, style);
    }
}
