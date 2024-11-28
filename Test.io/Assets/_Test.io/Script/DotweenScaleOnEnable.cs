using System;
using DG.Tweening;
using UnityEngine;

public class DotweenScaleOnEnable : MonoBehaviour
{
    private void OnEnable()
    {
        transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetDelay(0.15f);
    }

    private void OnDisable()
    {
        transform.localScale = Vector3.zero;
    }
}
