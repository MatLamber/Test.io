using System;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;
using Unity.Cinemachine;

public class VirtualCamerasManagers : MonoBehaviour, MMEventListener<TopDownEngineEvent>
{
    [SerializeField] private List<CinemachineCamera> virtualCameras;
    [SerializeField] private int selectedCameraIndex = 0;

    public void SetCameraPriority(int cameraIndex, int priority)
    {
        if (cameraIndex < 0 || cameraIndex >= virtualCameras.Count)
        {
            Debug.LogWarning("Índice de cámara fuera de rango");
            return;
        }

        for (int i = 0; i < virtualCameras.Count; i++)
        {
            virtualCameras[i].Priority = (i == cameraIndex) ? priority : 0;
        }
    }

    public void SetSelectedCamera(int cameraIndex)
    {
        if (cameraIndex < 0 || cameraIndex >= virtualCameras.Count)
        {
            Debug.LogWarning("Índice de cámara fuera de rango");
            return;
        }

        selectedCameraIndex = cameraIndex;
    }

    private void Start()
    {
        // Establecer la prioridad solo a la cámara seleccionada
        SetCameraPriority(selectedCameraIndex, 10);
    }


    private void Update()
    {
        if(Input.GetKeyDown(key: KeyCode.Space))
            SetCameraPriority(1,10);
    }

    private void OnEnable()
    {
        this.MMEventStartListening<TopDownEngineEvent>();
    }

    private void ondisable()
    {
        this.MMEventStopListening<TopDownEngineEvent>();
    }

    public void OnMMEvent(TopDownEngineEvent eventType)
    {
        if (eventType.EventType == TopDownEngineEventTypes.GameStart)
        {
            SetCameraPriority(1,10);
        }

    }
}