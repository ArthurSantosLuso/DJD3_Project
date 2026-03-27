using System;
using UnityEngine;

public abstract class ValueBase : MonoBehaviour
{
    // Script base for systems that the main objective is to use a value for something.
    // Example: Health and Stamina

    [SerializeField]
    protected float maxValue = 100;
    [SerializeField]
    protected float currentValue = 100;

    public float MaxValue => maxValue;
    public float CurrentValue => currentValue;

    public Action<int, float, float> OnValueChanged;

    protected virtual void Awake()
    {
        currentValue = maxValue;
    }

    protected virtual void ReduceValue(float amount)
    {
        currentValue -= amount;
        currentValue = Mathf.Clamp(currentValue, 0, maxValue);
    }

    protected virtual void IncreaseValue(float amount)
    {
        currentValue += amount;
        currentValue = Mathf.Clamp(currentValue, 0, maxValue);
    }

    public void ModifyValue(float amount)
    {
        currentValue = Mathf.Clamp(currentValue + amount, 0, maxValue);
    }

    protected virtual void ReduceMaximumValue(float amount)
    {
        maxValue -= amount;
        maxValue = Mathf.Max(0, maxValue);
        currentValue = Mathf.Clamp(currentValue, 0, maxValue);
    }

    protected virtual void IncreaseMaximumValue(float amount)
    {
        maxValue += amount;
    }

}

