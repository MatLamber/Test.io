using System.Collections.Generic;
using MoreMountains.TopDownEngine;
using UnityEngine;
using UnityEngine.UI;

public class TurretManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> turrets;
    [SerializeField] private List<int> turretPrices;
    [SerializeField] private GameObject unlockableSignal;
    [SerializeField] private GameObject buyPopUp;
    [SerializeField] private int turretID;
    private int currentTurret;
    private int currentTurretPrice;
    private string playerTagName = "Player";
    private Button upgradeButton;

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
        for (int i = 0; i < turrets.Count; i++)
            turrets[i].SetActive(i == currentTurret);
        currentTurret++;
        if (currentTurret < turrets.Count)
            currentTurretPrice = turretPrices[currentTurret];
        
    }
     
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTagName)) return;
       if (Camera.main != null) GUIManager.Instance.ShowPopUpPanel(Camera.main.WorldToScreenPoint(transform.position),turretID );
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTagName)) return;
        GUIManager.Instance.HidePopUpPanel();

    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTagName)) return;
        if (Camera.main != null) GUIManager.Instance.ShowPopUpPanel(Camera.main.WorldToScreenPoint(transform.position), turretID);
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