using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// 체력바 UI View
/// 순수하게 표시만 담당
/// </summary>
public class HealthBarView : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image fillImage;

    [Header("Colors")]
    [SerializeField] private Color highHealthColor = Color.green;
    [SerializeField] private Color mediumHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;

    /// <summary>
    /// 체력 업데이트 (Presenter에서 호출)
    /// </summary>
    public void UpdateHealth(int current, int max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }

        if (healthText != null)
        {
            healthText.text = $"{current} / {max}";
        }

        // 체력 비율에 따라 색상 변경
        if (fillImage != null)
        {
            float ratio = max > 0 ? (float)current / max : 0f;

            if (ratio > 0.5f)
            {
                fillImage.color = highHealthColor;
            }
            else if (ratio > 0.25f)
            {
                fillImage.color = mediumHealthColor;
            }
            else
            {
                fillImage.color = lowHealthColor;
            }
        }
    }
}
