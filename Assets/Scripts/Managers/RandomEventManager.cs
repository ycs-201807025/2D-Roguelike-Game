using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 랜덤 사건 관리
/// </summary>
public class RandomEventManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject eventPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject optionButtonPrefab;

    [Header("Event Data")]
    [SerializeField] private RandomEventData[] allEvents;

    private RandomEventData currentEvent;

    void Start()
    {
        if (eventPanel != null)
        {
            eventPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 랜덤 사건 시작
    /// </summary>
    public void TriggerRandomEvent()
    {
        if (allEvents == null || allEvents.Length == 0)
        {
            Debug.LogWarning("[EVENT] No events configured");
            return;
        }

        // 랜덤 사건 선택
        currentEvent = allEvents[Random.Range(0, allEvents.Length)];

        Debug.Log($"[EVENT] Triggered: {currentEvent.eventTitle}");

        ShowEvent();
    }

    void ShowEvent()
    {
        if (currentEvent == null) return;

        // UI 표시
        if (eventPanel != null)
        {
            eventPanel.SetActive(true);
        }

        // 제목과 설명
        if (titleText != null)
        {
            titleText.text = currentEvent.eventTitle;
        }

        if (descriptionText != null)
        {
            descriptionText.text = currentEvent.eventDescription;
        }

        // 기존 버튼 삭제
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // 선택지 버튼 생성
        for (int i = 0; i < currentEvent.options.Length; i++)
        {
            CreateOptionButton(currentEvent.options[i], i);
        }

        // 게임 일시정지
        Time.timeScale = 0f;
    }

    void CreateOptionButton(EventOption option, int index)
    {
        if (optionButtonPrefab == null || buttonContainer == null) return;

        GameObject buttonObj = Instantiate(optionButtonPrefab, buttonContainer);

        // 텍스트 설정
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = option.optionText;
        }

        // 버튼 이벤트
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            int optionIndex = index; // 클로저 문제 방지
            button.onClick.AddListener(() => OnOptionSelected(option, optionIndex));
        }
    }

    void OnOptionSelected(EventOption option, int index)
    {
        Debug.Log($"[EVENT] Selected option {index}: {option.optionText}");

        // 비용 체크
        bool canAfford = true;

        if (PlayerStats.Instance != null)
        {
            if (option.goldCost > 0 && PlayerStats.Instance.Gold < option.goldCost)
            {
                Debug.Log("[EVENT] Not enough gold!");
                canAfford = false;
            }

            if (option.healthCost > 0 && PlayerStats.Instance.CurrentHealth <= option.healthCost)
            {
                Debug.Log("[EVENT] Not enough health!");
                canAfford = false;
            }
        }

        if (!canAfford)
        {
            // 구매 불가 메시지 (선택사항)
            return;
        }

        // 비용 지불
        if (PlayerStats.Instance != null)
        {
            if (option.goldCost != 0)
            {
                PlayerStats.Instance.AddGold(-option.goldCost);
            }

            if (option.healthCost > 0)
            {
                PlayerStats.Instance.TakeDamage(option.healthCost);
            }
            else if (option.healthCost < 0)
            {
                PlayerStats.Instance.Heal(-option.healthCost);
            }
        }

        // 보상 지급
        if (option.rewardItem != null)
        {
            Inventory inventory = FindObjectOfType<Inventory>();
            if (inventory != null)
            {
                inventory.AddItem(option.rewardItem);
                Debug.Log($"[EVENT] Reward: {option.rewardItem.itemName}");
            }
        }

        // UI 닫기
        CloseEvent();
    }

    void CloseEvent()
    {
        if (eventPanel != null)
        {
            eventPanel.SetActive(false);
        }

        // 게임 재개
        Time.timeScale = 1f;

        Debug.Log("[EVENT] Event closed");
    }
}
