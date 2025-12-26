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
    #region Constants
    private const float PAUSED_TIME_SCALE = 0f;
    private const float NORMAL_TIME_SCALE = 1f;
    #endregion

    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private GameObject eventPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject optionButtonPrefab;

    [Header("Event Data")]
    [SerializeField] private RandomEventData[] allEvents;
    #endregion

    #region State
    private RandomEventData currentEvent;
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        HidePanel();
    }
    #endregion

    #region Initialization
    /// <summary>
    /// 패널 초기 숨김
    /// </summary>
    private void HidePanel()
    {
        if (eventPanel != null)
        {
            eventPanel.SetActive(false);
        }
    }
    #endregion

    #region Event Triggering
    /// <summary>
    /// 랜덤 사건 시작
    /// </summary>
    public void TriggerRandomEvent()
    {
        if (!ValidateEvents())
        {
            return;
        }

        currentEvent = SelectRandomEvent();
        ShowEvent();

        Debug.Log($"[EVENT] Triggered: {currentEvent.eventTitle}");
    }

    /// <summary>
    /// 이벤트 유효성 검사
    /// </summary>
    private bool ValidateEvents()
    {
        if (allEvents == null || allEvents.Length == 0)
        {
            Debug.LogWarning("[EVENT] No events configured");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 랜덤 사건 선택
    /// </summary>
    private RandomEventData SelectRandomEvent()
    {
        return allEvents[Random.Range(0, allEvents.Length)];
    }
    #endregion

    #region UI Display
    /// <summary>
    /// 사건 표시
    /// </summary>
    private void ShowEvent()
    {
        if (currentEvent == null)
        {
            return;
        }

        ActivatePanel();
        DisplayEventInfo();
        ClearExistingButtons();
        CreateOptionButtons();
        PauseGame();
    }

    /// <summary>
    /// 패널 활성화
    /// </summary>
    private void ActivatePanel()
    {
        if (eventPanel != null)
        {
            eventPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 사건 정보 표시
    /// </summary>
    private void DisplayEventInfo()
    {
        SetTitle();
        SetDescription();
    }

    /// <summary>
    /// 제목 설정
    /// </summary>
    private void SetTitle()
    {
        if (titleText != null)
        {
            titleText.text = currentEvent.eventTitle;
        }
    }

    /// <summary>
    /// 설명 설정
    /// </summary>
    private void SetDescription()
    {
        if (descriptionText != null)
        {
            descriptionText.text = currentEvent.eventDescription;
        }
    }

    /// <summary>
    /// 기존 버튼 삭제
    /// </summary>
    private void ClearExistingButtons()
    {
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 선택지 버튼 생성
    /// </summary>
    private void CreateOptionButtons()
    {
        for (int i = 0; i < currentEvent.options.Length; i++)
        {
            CreateOptionButton(currentEvent.options[i], i);
        }
    }

    /// <summary>
    /// 개별 선택지 버튼 생성
    /// </summary>
    private void CreateOptionButton(EventOption option, int index)
    {
        if (optionButtonPrefab == null || buttonContainer == null)
        {
            return;
        }

        GameObject buttonObj = Instantiate(optionButtonPrefab, buttonContainer);

        SetButtonText(buttonObj, option);
        SetButtonEvent(buttonObj, option, index);
    }

    /// <summary>
    /// 버튼 텍스트 설정
    /// </summary>
    private void SetButtonText(GameObject buttonObj, EventOption option)
    {
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = option.optionText;
        }
    }

    /// <summary>
    /// 버튼 이벤트 설정
    /// </summary>
    private void SetButtonEvent(GameObject buttonObj, EventOption option, int index)
    {
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            int optionIndex = index; // 클로저 문제 방지
            button.onClick.AddListener(() => OnOptionSelected(option, optionIndex));
        }
    }

    /// <summary>
    /// 게임 일시정지
    /// </summary>
    private void PauseGame()
    {
        Time.timeScale = PAUSED_TIME_SCALE;
    }
    #endregion

    #region Option Selection
    /// <summary>
    /// 선택지 선택 시
    /// </summary>
    private void OnOptionSelected(EventOption option, int index)
    {
        Debug.Log($"[EVENT] Selected option {index}: {option.optionText}");

        if (!CanAffordOption(option))
        {
            return;
        }

        PayCosts(option);
        GiveRewards(option);
        CloseEvent();
    }

    /// <summary>
    /// 비용 지불 가능 여부
    /// </summary>
    private bool CanAffordOption(EventOption option)
    {
        if (PlayerStats.Instance == null)
        {
            return true;
        }

        if (!HasEnoughGold(option))
        {
            Debug.Log("[EVENT] Not enough gold!");
            return false;
        }

        if (!HasEnoughHealth(option))
        {
            Debug.Log("[EVENT] Not enough health!");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 골드 충분한지 확인
    /// </summary>
    private bool HasEnoughGold(EventOption option)
    {
        return option.goldCost <= 0 ||
               PlayerStats.Instance.Gold >= option.goldCost;
    }

    /// <summary>
    /// 체력 충분한지 확인
    /// </summary>
    private bool HasEnoughHealth(EventOption option)
    {
        return option.healthCost <= 0 ||
               PlayerStats.Instance.CurrentHealth > option.healthCost;
    }

    /// <summary>
    /// 비용 지불
    /// </summary>
    private void PayCosts(EventOption option)
    {
        if (PlayerStats.Instance == null)
        {
            return;
        }

        PayGoldCost(option);
        PayHealthCost(option);
    }

    /// <summary>
    /// 골드 비용 지불
    /// </summary>
    private void PayGoldCost(EventOption option)
    {
        if (option.goldCost != 0)
        {
            PlayerStats.Instance.AddGold(-option.goldCost);
        }
    }

    /// <summary>
    /// 체력 비용 지불
    /// </summary>
    private void PayHealthCost(EventOption option)
    {
        if (option.healthCost > 0)
        {
            PlayerStats.Instance.TakeDamage(option.healthCost);
        }
        else if (option.healthCost < 0)
        {
            PlayerStats.Instance.Heal(-option.healthCost);
        }
    }

    /// <summary>
    /// 보상 지급
    /// </summary>
    private void GiveRewards(EventOption option)
    {
        if (option.rewardItem == null)
        {
            return;
        }

        Inventory inventory = FindObjectOfType<Inventory>();
        if (inventory != null)
        {
            inventory.AddItem(option.rewardItem);
            Debug.Log($"[EVENT] Reward: {option.rewardItem.itemName}");
        }
    }
    #endregion

    #region Event Closing
    /// <summary>
    /// 사건 종료
    /// </summary>
    private void CloseEvent()
    {
        DeactivatePanel();
        ResumeGame();

        Debug.Log("[EVENT] Event closed");
    }

    /// <summary>
    /// 패널 비활성화
    /// </summary>
    private void DeactivatePanel()
    {
        if (eventPanel != null)
        {
            eventPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 게임 재개
    /// </summary>
    private void ResumeGame()
    {
        Time.timeScale = NORMAL_TIME_SCALE;
    }
    #endregion
}
