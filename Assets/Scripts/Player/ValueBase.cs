using UnityEngine;

public class ValueBase : MonoBehaviour
{
    // Script base for systems that the main objective is to use a value for something.
    // Example: Health and Stamina

    protected int _maxValue;
    protected int _currentValue;
    
    public virtual void ReduceValue(int valueToReduce) 
    {
        _currentValue -= valueToReduce;
    }

    public virtual void ReduceMaximumValue(int valueToReduce)
    {
        _maxValue = valueToReduce;
    }

}
