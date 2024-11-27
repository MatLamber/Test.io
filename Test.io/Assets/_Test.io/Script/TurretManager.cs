using System;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;
using UnityEngine.UI;

public class TurretManager : MonoBehaviour, MMEventListener<WaveStartEvent>, MMEventListener<WaveEndedEvent>
{
    [SerializeField] private List<GameObject> turrets;
    [SerializeField] private List<int> turretPrices;
    [SerializeField] private GameObject unlockableSignal;
    [SerializeField] private SphereCollider upgradeZoneCollider;
    [SerializeField] private BoxCollider turretCollider;
    [SerializeField] private int turretID;
    private int currentTurret;
    private int currentTurretPrice;
    private string playerTagName = "Player";
    private Button upgradeButton;
    private bool isUpgradeable;
    private PlayerController playerController;

    private void Start()
    {
        currentTurretPrice = turretPrices[currentTurret];
        upgradeButton = GUIManager.Instance.UpgradeTurretButton;
        upgradeButton.onClick.AddListener(UpgradeTurret);
    }

     public void UpgradeTurret()
    {
        if (GUIManager.Instance.CurrentTurretID != turretID) return;
        
        if (turrets == null || turrets.Count == 0) return;

        if (currentTurret < 0 || currentTurret >= turrets.Count) return;
        
        unlockableSignal.SetActive(false);
        UpdatePlayerMoney(-currentTurretPrice);
        for (int i = 0; i < turrets.Count; i++)
            turrets[i].SetActive(i == currentTurret);
        currentTurret++;
        if (currentTurret < turrets.Count)
            currentTurretPrice = turretPrices[currentTurret];
        
    }
     
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTagName)) return;
        PassInfoToUI();
        playerController = other.GetComponent<PlayerController>();
        if (playerController == null) return;
        CheckIfUpgradeable(playerController);
    }



    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTagName)) return;
        GUIManager.Instance.HidePopUpPanel();

    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTagName)) return;
        PassInfoToUI(); 
        playerController = other.GetComponent<PlayerController>();
        if (playerController == null) return;
        CheckIfUpgradeable(playerController);
        /*if (playerController.GetMoney() < currentTurretPrice) return;
        currentTurretPrice -= 1;
        playerController.UpdateMoney(-1);
        if (currentTurretPrice == 0)
        {
            UpgradeTurret();
        }*/
    }
    
    private void PassInfoToUI()
    {
        string turretLevelText;
        string turretPriceText;
        if (currentTurret >= turrets.Count)
        {
            turretLevelText = $"TURRET LV MAX";
            turretPriceText = $"MAX";
        }
        else
        {
            turretLevelText = $"TURRET LV {currentTurret + 1}";
            turretPriceText = $"{currentTurretPrice}";
        }
        
        if (Camera.main != null) GUIManager.Instance.ShowPopUpPanel(Camera.main.WorldToScreenPoint(transform.position),turretID, turretLevelText, turretPriceText );
    }
    
    private void CheckIfUpgradeable(PlayerController playerController)
    {
        isUpgradeable =!((currentTurret < 0 || currentTurret >= turrets.Count) || playerController.GetMoney() < currentTurretPrice) ;
        upgradeButton.interactable = isUpgradeable;
    }

    private void UpdatePlayerMoney(int price)
    {
        if (playerController == null) return;
        playerController.UpdateMoney(price);
    }

    private void OnEnable()
    {
        this.MMEventStartListening<WaveStartEvent>();
        this.MMEventStartListening<WaveEndedEvent>();
    }

    private void OnDisable()
    {
        this.MMEventStopListening<WaveStartEvent>();
        this.MMEventStopListening<WaveEndedEvent>();
    }

    public void OnMMEvent(WaveStartEvent eventType)
    {
        unlockableSignal.transform.DOScale(Vector3.zero, 0.1f).SetEase(Ease.InBack).OnComplete(() => unlockableSignal.SetActive(false));
        upgradeZoneCollider.enabled = false;
    }

    public void OnMMEvent(WaveEndedEvent eventType)
    {
        if (currentTurret == 0)
        {
            unlockableSignal.SetActive(true);
            unlockableSignal.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }
        upgradeZoneCollider.enabled = true;
    }
}