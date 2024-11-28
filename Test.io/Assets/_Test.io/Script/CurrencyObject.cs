using System;
using MoreMountains.TopDownEngine;
using UnityEngine;

public class CurrencyObject : MonoBehaviour
{
    [SerializeField] private CurrencyType currencyType;
    private string playerTagName = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag(playerTagName)) return;
        AddCurrencyEvent.Trigger(currencyType,100);
        gameObject.SetActive(false);

    }
}
