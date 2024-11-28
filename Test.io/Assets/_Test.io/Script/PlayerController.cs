using System;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

public class PlayerController : MonoBehaviour, MMEventListener<TopDownEngineEvent>, MMEventListener<AddCurrencyEvent>
{
   private CharacterMovement characterMovement => GetComponent<CharacterMovement>();
   private CharacterHandleWeapon characterHandleWeapon => GetComponent<CharacterHandleWeapon>();
   private string standAnimationParameter = "Stand";
   private int money;
   private int greenGems;
   [SerializeField] private Animator animator;
   [SerializeField] private PlayerStats stats;

   private void Start()
   {
      InitializeStats();
      UpdateMoney();
      UpdateGreenGems();
   }

   private void InitializeStats()
   {
      money = stats.money;
      greenGems = stats.greenGem;
   }

   public void EnableMovement()
   {
      characterMovement.AbilityPermitted = true;
      characterHandleWeapon.enabled = true;
      animator.SetBool(standAnimationParameter, true);
   }
   
   private void OnEnable()
   {
      this.MMEventStartListening<TopDownEngineEvent>();
      this.MMEventStartListening<AddCurrencyEvent>();
   }

   private void OnDisable()
   {
      this.MMEventStopListening<TopDownEngineEvent>();
      this.MMEventStopListening<AddCurrencyEvent>();
   }

   public void OnMMEvent(TopDownEngineEvent eventType)
   {
      if (eventType.EventType == TopDownEngineEventTypes.GameStart)
      {
         EnableMovement();
      }

   }

   public int GetMoney()
   {
      return money;
   }

   public int GetGreenGems()
   {
      return greenGems;
   }

   public void UpdateMoney(int amount = 0)
   {
      money += amount;
      if(GUIManager.Instance is not null)
         GUIManager.Instance.UpdateMoneyText(money);
   }

   public void UpdateGreenGems(int amount = 0)
   {
      greenGems += amount;
      if(GUIManager.Instance is not null)
         GUIManager.Instance.UpdateGreenGemsText(greenGems);
   }

   public void OnMMEvent(AddCurrencyEvent eventType)
   {
      switch (eventType.CurrencyType)
      {
         case CurrencyType.Coin:
            UpdateMoney(eventType.Amount);
            break;
         case CurrencyType.GreenGem:
            UpdateGreenGems(eventType.Amount);
            break;
         default:
            throw new ArgumentOutOfRangeException();
      }
   }
}
