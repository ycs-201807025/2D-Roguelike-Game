using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 방 전환 포탈/문
/// </summary>
public class RoomPortal : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] private bool isNextRoom = true; // true면 다음방, false면 이전방
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color inactiveColor = Color.gray;

    private bool playerInRange = false;
    private DungeonManager dungeonManager;
    private bool isActive = false;

    void Start()
    {
        dungeonManager = FindObjectOfType<DungeonManager>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        SetActive(false); // 처음엔 비활성

        if (dungeonManager == null)
        {
            Debug.LogError("DungeonManager not found in scene!");
        }
    }

    void Update()
    {
        // 플레이어가 범위 내에 있고, E키를 누르면
        if (playerInRange && isActive && Input.GetKeyDown(interactKey))
        {
            UsePortal();
        }
    }

    /// <summary>
    /// 포탈 활성화/비활성화
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = active ? activeColor : inactiveColor;
        }

        Debug.Log($"Portal {gameObject.name} → {(active ? "ACTIVE" : "inactive")}");
    }

    /// <summary>
    /// 포탈 사용
    /// </summary>
    private void UsePortal()
    {
        if (dungeonManager == null)
        {
            Debug.LogError("DungeonManager is NULL! Cannot use portal");
            return;
        }

        Debug.Log($"[PORTAL] Using: {(isNextRoom ? "Next" : "Previous")} room");

        if (isNextRoom)
        {
            dungeonManager.MoveToNextRoom();
        }
        else
        {
            dungeonManager.MoveToPreviousRoom();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[PORTAL] Trigger Enter: {collision.name}, Tag: {collision.tag}");

        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log($"[PORTAL] Player in range! Active: {isActive}");

            if (isActive)
            {
                Debug.Log($"[PORTAL] Press [{interactKey}] to {(isNextRoom ? "continue" : "go back")}");
            }
            else
            {
                Debug.Log("[PORTAL] Portal is inactive - clear room first!");
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("[PORTAL] Player left range");
        }
    }

    // UI 표시 (임시)
    void OnGUI()
    {
        if (playerInRange && isActive)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2);

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 18;
            style.normal.textColor = Color.yellow;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;

            string text = $"[{interactKey}] {(isNextRoom ? "다음 방" : "이전 방")}";
            GUI.Label(new Rect(screenPos.x - 60, Screen.height - screenPos.y - 30, 120, 40), text, style);
        }
    }
}
