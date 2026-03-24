using UnityEngine;

public class PlayerStamina : ValueBase
{
    public override void ReduceValue(int valueToReduce)
    {
        base.ReduceValue(valueToReduce);

        if (_currentValue <= 0)
        {
            return;
        }
    }
}
