using UnityEngine;

public interface IDamageable
{
    public bool CanDamage();

    public void Damage(float damageValue);

    public void DamageNoStagger(float damageValue);
}
