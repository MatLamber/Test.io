using System;
using System.Collections;
using DG.Tweening;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;
using Pathfinding;
using ProjectDawn.Navigation.Hybrid;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public struct EnemyDeathEvent
{
    public static EnemyDeathEvent e;
    public Enemy Enemy;

    public static void Trigger(Enemy enemyData)
    {
        e.Enemy = enemyData;
        MMEventManager.TriggerEvent(e);
    }
}

public class Enemy : MonoBehaviour
{
    private Transform target;

    // Referencia al componente AIPath
    private AIPath aiPath => GetComponent<AIPath>();
    [SerializeField] private Animator animator;
    [SerializeField] private Transform enemyModel;
    [SerializeField] private Transform landingPosition;
    [Header("Parametros del animator")] private string walkParamater = "Walk";
    private string flinchParameter = "Flinch";
    private string flailParameter = "Flail";
    private string impactParameter = "Impact";
    private string dieParameter = "Die";
    private bool isDead;

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;

        // Configura la primera animación
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            PlayFlyAwayDeath();
        }
    }

    private void OnDisable()
    {
        GetComponent<CharacterController>().enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
        enemyModel.localPosition = Vector3.zero;
        enemyModel.localRotation = Quaternion.Euler(Vector3.zero);
        Transform enemyMesh = enemyModel.GetChild(0);
        enemyMesh.localPosition = Vector3.zero;
        enemyMesh.localRotation = Quaternion.Euler(Vector3.zero);
        isDead = false;
        if (aiPath != null)
            aiPath.canMove = true;
    }

    public void FollowTarget()
    {
        if (isDead) return;

        if (aiPath != null && target != null)
        {
            aiPath.destination = target.transform.position;
            ManageMovementAnimation();
        }
    }

    private void ManageMovementAnimation()
    {
        bool isWalking = animator.GetBool(walkParamater);

        // Ejemplo: Cambiar animación a 'Run' si se mueve
        if (!aiPath.isStopped)
        {
            if (!isWalking)
                animator.SetBool(walkParamater, true);
        }
        else
        {
            if (isWalking)
                animator.SetBool(walkParamater, false);
        }
    }

    public void PlayFlinchAnimation()
    {
        if (aiPath != null)
            aiPath.canMove = false;
        animator.SetTrigger(flinchParameter);

        StartCoroutine(ResetCanMove());
    }

    public void PlayFlyAwayDeath()
    {
        if (aiPath != null) aiPath.canMove = false;
        GetComponent<Rigidbody>().isKinematic = true;


        // Eleva al enemigo y añade rotación
        int jumpHeight = Random.Range(6, 23);
        float jumpDuration = Random.Range(0.6f, 1.8f);
        float fallDuration = jumpDuration / 2;
        float rotationDuration = jumpDuration + fallDuration / 2; // Duración total incluyendo la bajada
        float delayBeforeDescent = Random.Range(1f, 1.3f); // Tiempo que se espera antes de descender;

        // Animación de elevarse
        enemyModel.DOMoveY(enemyModel.localPosition.y + jumpHeight, jumpDuration)
            .OnStart((() => { animator.SetTrigger(flailParameter); }));

        // Animación de descenso con retraso
        enemyModel.DOMoveY(enemyModel.localPosition.y, fallDuration).SetDelay(delayBeforeDescent)
            .SetEase(ease: Ease.InQuad).OnComplete((() => { animator.SetTrigger(impactParameter); }));

        int rotatioDirection = Random.Range(0, 2);

        float rotationY = enemyModel.rotation.eulerAngles.y;
        switch (rotatioDirection)
        {
            case 0:
                // Animación de rotación continua durante la elevación y el descenso
                enemyModel.DORotate(new Vector3(1080, rotationY, enemyModel.localRotation.z), rotationDuration,
                        RotateMode.FastBeyond360)
                    .SetEase(Ease.OutQuad).OnComplete((() => { }));
                break;
            case 1:
                // Animación de rotación continua durante la elevación y el descenso
                enemyModel.DORotate(new Vector3(1080, rotationY, enemyModel.localRotation.z), rotationDuration,
                        RotateMode.FastBeyond360)
                    .SetEase(Ease.OutQuad).OnComplete((() => { }));
                break;
        }

        isDead = true;
        EnemyDeathEvent.Trigger(this);
    }

    public void PlayDeafultDeathAnimation()
    {
        if (aiPath != null) aiPath.canMove = false;
        GetComponent<Rigidbody>().isKinematic = true;
        animator.SetTrigger(dieParameter);
        enemyModel.DOJump(landingPosition.position, 1, 1, 0.5f).SetEase(Ease.OutQuad);
        isDead = true;
        EnemyDeathEvent.Trigger(this);
    }

    IEnumerator ResetCanMove()
    {
        yield return new WaitForSeconds(GetAnimationClipLength());
        if (!isDead && aiPath != null)
            aiPath.canMove = true;
    }


    private float GetAnimationClipLength()
    {
        if (animator == null) return 0f;

        // Obtiene la información del estado actual en la capa 0
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // La duración de la animación puede obtenerse con stateInfo.length
        return stateInfo.length;
    }
}