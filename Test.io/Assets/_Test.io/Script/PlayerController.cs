using System;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

public class PlayerController : MonoBehaviour, MMEventListener<TopDownEngineEvent>
{
   private CharacterMovement characterMovement => GetComponent<CharacterMovement>();
   private CharacterHandleWeapon characterHandleWeapon => GetComponent<CharacterHandleWeapon>();
   private string standAnimationParameter = "Stand";
   private int money;
   [SerializeField] private Animator animator;
   [SerializeField] private PlayerStats stats;

   private void Start()
   {
      InitializeStats();
      UpdateMoney();
   }

   private void InitializeStats()
   {
      money = stats.money;
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
   }

   private void OnDisable()
   {
      this.MMEventStopListening<TopDownEngineEvent>();
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

   public void UpdateMoney(int amount = 0)
   {
      money += amount;
      if(GUIManager.Instance is not null)
         GUIManager.Instance.UpdateMoneyText(money);
   }
   
}
