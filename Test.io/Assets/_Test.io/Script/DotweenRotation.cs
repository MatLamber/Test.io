using System;
using DG.Tweening;
using UnityEngine;

public enum RotationAxis
{
    xAxis,
    yAxis,
    zAxis,
}

public class DotweenRotation : MonoBehaviour
{
    [SerializeField] private RotationAxis rotationAxis;
    [SerializeField] private float rotationSpeed;


    private void Start()
    {
        DoRotation();
    }

    private void DoRotation()
    {
        Vector3 rotationVector = Vector3.zero;

        switch (rotationAxis)
        {
            case RotationAxis.xAxis:
                rotationVector = new Vector3(360, 0, 0);
                break;
            case RotationAxis.yAxis:
                rotationVector = new Vector3(0, 360, 0);
                break;
            case RotationAxis.zAxis:
                rotationVector = new Vector3(0, 0, 360);
                break;
        }

        transform.DORotate(rotationVector, rotationSpeed, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);
    }
}