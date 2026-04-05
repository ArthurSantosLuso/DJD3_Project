using System.Collections.Generic;
using System.Security;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon : MonoBehaviour
{
    [SerializeField]
    private List<Ability> abilities;

    public List<Ability> Abilities { get { return abilities; } }
}
