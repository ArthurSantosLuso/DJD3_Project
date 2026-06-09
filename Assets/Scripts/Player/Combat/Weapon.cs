using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    private List<Ability> abilities;

    public List<Ability> Abilities { get { return abilities; } }

    //public void InitializeAbility()
    //{

    //}
}
