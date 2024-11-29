using System;
using System.Collections;
using DG.Tweening;
using MoreMountains.TopDownEngine;
using TMPro;
using UnityEngine;

public class ChestUpgradeManager : MonoBehaviour
{
    [SerializeField] private GameObject upgradeCostPanel;
    [SerializeField] private int price;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Animator chestAnimator;
    private string playerTagName = "Player";
    private PlayerController playerController;
    private bool claimed;
    private string vibrateParameter = "Vibrate";
    private string openParameter = "Open";
    
    private void Start()
    {
        SetPriceText();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTagName)) return;
        ShowUpgradeCostPanel();
        playerController = other.GetComponent<PlayerController>();

    }


    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag(playerTagName)) return;
        CheckIfCanOpen();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTagName)) return;
        HideUpgradeCostPanel();
    }

    public void ShowUpgradeCostPanel()
    {
        if(claimed) return;
        upgradeCostPanel.SetActive(true);
        upgradeCostPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
    }
    
    public void HideUpgradeCostPanel()
    {
        chestAnimator.SetBool(vibrateParameter, false);
        upgradeCostPanel.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() => upgradeCostPanel.SetActive(false));
    }

    public void CheckIfCanOpen()
    {
        if(playerController.GetGreenGems() < price || price <= 0 || claimed) return;
        price--;
        playerController.UpdateGreenGems(-1);
        SetPriceText();
        chestAnimator.SetBool(vibrateParameter, true);
        if (price == 0)
        {
            claimed = true;
            chestAnimator.SetBool(vibrateParameter, false);
            chestAnimator.SetTrigger(openParameter);
            HideUpgradeCostPanel();
            StartCoroutine(ShowPopUpPanel());
        }
    }

    IEnumerator ShowPopUpPanel()
    {
        yield return new WaitForSeconds(1f);
        GUIManager.Instance.ShowPlayerUpgradePanel();
    }

    public void SetPriceText()
    {
        priceText.text = price.ToString();
    }
}
