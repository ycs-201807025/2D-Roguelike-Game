using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SynergyEffect
{
    // 공격 관련
    public float atkMultiplier = 1f;        // 공격력 배율
    public float doubleDamageChance = 0f;   // 2배 대미지 확률

    // 속도 관련
    public float spdMultiplier = 1f;        // 이동속도 배율
    public float atkSpdMultiplier = 1f;     // 공격속도 배율

    // 드롭 관련
    public float dropRateMultiplier = 1f;   // 드롭률 배율

    // 마법사 관련
    public int extraAttacks = 0;            // 추가 공격 횟수
    public float tripleAttackChance = 0f;   // 추가 공격 확률

    // 광전사 관련
    public float berserkerBonus = 0f;       // 광전사 보너스

    // 암살자 관련
    public float critChance = 0f;           // 치명타 확률
    public float critDamage = 1f;           // 치명타 대미지 배율
}
