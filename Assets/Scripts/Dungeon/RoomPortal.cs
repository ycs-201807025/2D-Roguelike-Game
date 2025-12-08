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

        SetActive(false);
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
    }

    /// <summary>
    /// 포탈 사용
    /// </summary>
    private void UsePortal()
    {
        if (dungeonManager == null)
        {
            Debug.LogError("DungeonManager not found! 포탈이 작동 안 함");
            return;
        }

        Debug.Log($"Using portal: {(isNextRoom ? "Next" : "Previous")} room");

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
        Debug.Log($"Portal trigger: {collision.name}, Tag: {collision.tag}");

        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log($"Player in range! Active: {isActive}");

            if (isActive)
            {
                Debug.Log($"Press {interactKey} to {(isNextRoom ? "continue" : "go back")}");
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    // UI 표시 (임시)
    void OnGUI()
    {
        if (playerInRange && isActive)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2);

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 14;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;

            string text = $"[{interactKey}] {(isNextRoom ? "다음 방" : "이전 방")}";
            GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 20, 100, 30), text, style);
        }
    }
}
