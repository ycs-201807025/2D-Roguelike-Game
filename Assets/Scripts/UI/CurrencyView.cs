using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 재화 표시 UI View
/// </summary>
public class CurrencyView : MonoBehaviour
{
    [Header("Gold")]
    [SerializeField] private TextMeshProUGUI goldText;

    [Header("Souls")]
    [SerializeField] private TextMeshProUGUI soulsText;

    /// <summary>
    /// 골드 업데이트
    /// </summary>
    public void UpdateGold(int amount)
    {
        if (goldText != null)
        {
            goldText.text = $"Gold: {amount}";
        }
    }

    /// <summary>
    /// 영혼 업데이트
    /// </summary>
    public void UpdateSouls(int amount)
    {
        if (soulsText != null)
        {
            soulsText.text = $"Souls: {amount}";
        }
    }
}
