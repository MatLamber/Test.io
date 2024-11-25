
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

public class CharacterComboAttack : CharacterAbility
{
    protected const string attackAnimationParameterName = "Attack";
    protected int attackAnimationParameter;
    protected HashSet<int> animatorParameters;
    private int isAttacking;
    [SerializeField] private Collider kickCollider;

    protected override void Initialization()
    {
        base.Initialization();
        animatorParameters = new HashSet<int>();

        // Registrar y agregar el parámetro de ataque al conjunto de parámetros animados
        RegisterAnimatorParameter(attackAnimationParameterName, AnimatorControllerParameterType.Trigger,
            out attackAnimationParameter);
        MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, attackAnimationParameterName,
            out attackAnimationParameter, AnimatorControllerParameterType.Trigger, animatorParameters);
    }

    protected override void InitializeAnimatorParameters()
    {
        RegisterAnimatorParameter(attackAnimationParameterName, AnimatorControllerParameterType.Trigger,
            out attackAnimationParameter);
    }

    public override void ProcessAbility()
    {
        base.ProcessAbility();
    }

    public override void UpdateAnimator()
    {
        // Verifica si el Animator se ha inicializado para evitar errores
        if (_animator == null)
        {
            return;
        }
        attackAnimationParameter = isAttacking;
        MMAnimatorExtensions.UpdateAnimatorTrigger(_animator,  attackAnimationParameter, animatorParameters);
        isAttacking = 0;
    }
    
    public void StartAttack()
    {
        isAttacking = 1080829965;
        if(kickCollider)
            kickCollider.enabled = true;
         StartCoroutine(ResetAttack());
    }

    IEnumerator ResetAttack()
    {
       yield return new WaitForSeconds(1.1f);
       if(kickCollider)
           kickCollider.enabled = false;
 
    }


    protected override void HandleInput()
    {
        base.HandleInput();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Attack");
            StartAttack();
        }
    }
}
