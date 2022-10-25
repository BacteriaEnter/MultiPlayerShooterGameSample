using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI ammoStatus;
    [SerializeField] TextMeshProUGUI hpStatus;
    public void UpdateAmmoStatus(int currentAmmoInMag,int currentAmmoInCarried)
    {
        ammoStatus.text = $"{currentAmmoInMag}/{currentAmmoInCarried}";
    }

    public void UpdateHpStatus(int currentHp)
    {
        hpStatus.text = $"{currentHp}";
    }
}
