using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적 데이터 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName = "Slime";
    public Sprite Sprite;

    [Header("Stats")]
    public int maxHealth = 30;
    public int damage = 5;
    public float moveSpeed = 2f;

    [Header("AI Settings")]
    public float detectionRange = 5f;//플레이어 감지 범위
    public float attackRange = 1f;//공격 범위
    public float attackCooldown = 1f;//공격 쿨타임

    [Header("Rewards")]
    public int goldDrop = 5;//인게임 재화
    public int soulDrop = 2;//영구 강화 재화
}
