using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Character : PausableMonoBehaviour
{
    [SerializeField]
    private bool shouldConsiderInteractable;
    [SerializeField]
    private bool shouldAttack;
    [SerializeField]
    private bool shouldUseStamina;
    [SerializeField]
    private bool shouldUseHealth;
    [SerializeField]
    private List<Weapon> weapons;
    [SerializeField]
    private List<ValueBase> valuesBase;

    public List<ValueBase> ValueBases => valuesBase;
    public bool ShouldConsiderInteractable => shouldConsiderInteractable;
    public bool ShouldAttack => shouldAttack;
    public bool ShouldUseStamina => shouldUseStamina;
    public bool ShouldUseHealth => shouldUseHealth;

    private List<Ability> currentAbilities;
    private Animator animator;
    private int currentWeapon;
    private int currentAbilityInUseIdx = -1;

    public enum State
    {
        Normal,
        Attacking,
        Dodging,
    }

    public State CharacterState { get; private set; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        currentWeapon = -1;

        foreach (Weapon weapon in weapons)
        {
            foreach(Ability ability in weapon.Abilities)
            {
                ability.Initialize(this, animator);
            }
        }

        if (ShouldAttack) ChangeToNextWeapon();
    }

    //private void Start()
    //{
    //    animator = GetComponent<Animator>();
    //    currentWeapon = -1;
    //    if (ShouldAttack) ChangeToNextWeapon();
    //}

    public bool ChangeToNextWeapon()
    {
        /// Go to the next weapon
        /// Example: if current weapon idx is 2 out of 4, go to 3
        /// Example: if current weapon idx is 1 out of 2, go to 0 again
        if (CharacterState != State.Normal) return false;

        currentWeapon = (currentWeapon + 1) % weapons.Count;
        ChangeAbilities();
        return true;
    }

    // Change entity current ability
    private void ChangeAbilities()
    {
        for (int i = 0; i < weapons.Count; i++)
            weapons[i].gameObject.SetActive(i == currentWeapon);

        currentAbilities = weapons[currentWeapon].Abilities;
    }

    // Use the current ability
    public void UseAbility(int abilityIdx)
    {
        // Perform the ability if it is not null
        currentAbilityInUseIdx = abilityIdx;
        currentAbilities[abilityIdx]?.Perform();
    }

    public void PlayAnimation(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    public void ChangeState(State state)
    {
        CharacterState = state;
    }

    public void EnableHitbox()
    {
        currentAbilities[currentAbilityInUseIdx].EnableHitbox();
    }

    public void DisableHitbox()
    {
        currentAbilities[currentAbilityInUseIdx].DisableHitbox();
    }

    public void DeployProjectile()
    {
        currentAbilities[currentAbilityInUseIdx].DeployProjectile();
    }
}