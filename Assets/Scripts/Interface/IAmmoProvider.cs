using System;
using UnityEngine;

public interface IAmmoProvider
{
    int CurrentAmmo {  get; }
    int MaxAmmo { get; }
    
    event Action<int, int> OnAmmoChanged;

}
