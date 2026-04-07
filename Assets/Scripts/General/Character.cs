using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField]
    private List<Weapon> weapons;
    [SerializeField]
    private List<ValueBase> valuesBase;

    public List<ValueBase> ValueBases => valuesBase;

    private List<Ability> currentAbilities;
    private Animator animator;
    private int currentWeapon;

    public enum State
    {
        Normal,
        Attacking,
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
        currentWeapon = (currentWeapon + 1) % weapons.Count;
        ChangeAbilities();
    }

    // Change entity current ability
    private void ChangeAbilities()
    {
        // Clear all current abilities
        if (currentAbilities != null) currentAbilities.Clear();
        // Insert all new abilities to the character
        currentAbilities = weapons[currentWeapon].Abilities;
    }

    // Use the current ability
    public void UseAbility(int abilityIdx)
    {
        // Perform the ability if it is not null
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
}