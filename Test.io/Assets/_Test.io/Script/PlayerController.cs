using System;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

public class PlayerController : MonoBehaviour, MMEventListener<TopDownEngineEvent>
{
   private CharacterMovement characterMovement => GetComponent<CharacterMovement>();
   private CharacterHandleWeapon characterHandleWeapon => GetComponent<CharacterHandleWeapon>();
   private string standAnimationParameter = "Stand";
   [SerializeField] private Animator animator;

   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.Space))
      {
         EnableMovement();
      }
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

   private void ondisable()
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
   
   
   
}
