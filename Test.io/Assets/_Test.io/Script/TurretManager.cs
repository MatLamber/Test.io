using System;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.TopDownEngine;
using UnityEngine;
using UnityEngine.EventSystems;

public class TurretManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> turrets;
    [SerializeField] private List<int> turretPrices;
    [SerializeField] private GameObject unlockableSignal;
    [SerializeField] private GameObject buyPopUp;
    private int currentTurret;
    private int currentTurretPrice;
    private string playerTagName = "Player";

    private void Start()
    {
        currentTurretPrice = turretPrices[currentTurret];
    }

    private void Update()
    {
        
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Mouse Click Detected: " + Input.mousePosition);
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    Debug.Log("Pointer is over a UI element.");
                }
                else
                {
                    Debug.Log("Pointer is not over any UI element.");
                }
            }
        
    }

     public void UpgradeTurret()
    {
        if (turrets == null || turrets.Count == 0) return;

        if (currentTurret < 0 || currentTurret >= turrets.Count) return;
        
        unlockableSignal.SetActive(false);
        for (int i = 0; i < turrets.Count; i++)
            turrets[i].SetActive(i == currentTurret);
        currentTurret++;
        if (currentTurret < turrets.Count)
            currentTurretPrice = turretPrices[currentTurret];
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTagName)) return;
       // buyPopUp.SetActive(true);
       // buyPopUp.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
       GUIManager.Instance.ShowPopUpPanel(  Camera.main.WorldToScreenPoint(transform.position));
    }

    private void OnTriggerExit(Collider other)
    {
       // if (!other.CompareTag(playerTagName)) return;
        //buyPopUp.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() => buyPopUp.SetActive(false));
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTagName)) return;
        GUIManager.Instance.ShowPopUpPanel(  Camera.main.WorldToScreenPoint(transform.position));
        /*PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController == null) return;
        if (playerController.GetMoney() < currentTurretPrice) return;
        currentTurretPrice -= 1;
        playerController.UpdateMoney(-1);
        if (currentTurretPrice == 0)
        {
            UpgradeTurret();
        }*/

        
    }
}