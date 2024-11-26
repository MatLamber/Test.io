using System;
using UnityEngine;
using Unity.Cinemachine;

public class CinemachineCameraRotation : MonoBehaviour
{
    [SerializeField] private Transform target;  // Transfrom to rotate around
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private CinemachineOrbitalFollow orbitalFollow;

    void Update()
    {
        if (target != null)
        {
            transform.LookAt(target);
            orbitalFollow.HorizontalAxis.Value += rotationSpeed * Time.deltaTime;
        }
    }

    private void LateUpdate()
    {
        
            
    }
}
