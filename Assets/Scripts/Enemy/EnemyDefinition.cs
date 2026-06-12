using System;
using UnityEngine;

[Serializable]
public struct EnemyDefinition
{
    [SerializeField] EnemyType type;
    [SerializeField] EnemyBaseAI prefab;

    public EnemyType Type => type;
    public EnemyBaseAI Prefab => prefab;
}
