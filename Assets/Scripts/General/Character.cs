using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField]
    private bool shouldConsiderInteractable;
    [SerializeField]
    private List<Weapon> weapons;
    [SerializeField]
    private List<ValueBase> valuesBase;

    public List<ValueBase> ValueBases => valuesBase;
    public bool ShouldConsiderInteractable => shouldConsiderInteractable;

    private List<Ability> currentAbilities;
    private Animator animator;
    private int currentWeapon;
    private int currentAbilityInUseIdx = -1;

    public enum State
    {
        Normal,
        Attacking,
        Lunging,
        Dodging,
    }

    public State CharacterState { get; private set; }

    private void Start()
    {
        animator = GetComponent<Animator>();
        currentWeapon = -1;
        ChangeToNextWeapon();
    }

    public void ChangeToNextWeapon()
    {
        /// Go to the next weapon
        /// Example: if current weapon idx is 2 out of 4, go to 3
        /// Example: if current weapon idx is 1 out of 2, go to 0 again
        if (CharacterState != State.Normal) return;

        currentWeapon = (currentWeapon + 1) % weapons.Count;
        ChangeAbilities();
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
}