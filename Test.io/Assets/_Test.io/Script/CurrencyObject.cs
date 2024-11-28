using System;
using UnityEngine;

public class CurrencyObject : MonoBehaviour
{
    private string playerTagName = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag(playerTagName)) return;
        gameObject.SetActive(false);
    }
}
