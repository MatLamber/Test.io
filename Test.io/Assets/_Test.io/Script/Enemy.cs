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
    
    private AgentAuthoring agentAuthoring => GetComponent<AgentAuthoring>();

    // Referencia al componente Animation
    [SerializeField] private Animation anim;

    // Serialized Fields para las animaciones
    [SerializeField] private string idleAnimation = "Idle";
    [SerializeField] private string runAnimation = "Run";
    [SerializeField] private string attackAnimation = "Attack";
    [SerializeField] private string flinchAnimation = "Flinch";
    [SerializeField] private string deathAnimation = "Flinch";
    [SerializeField] private string fallingPoseAnimation = "FallingPose";   
    [SerializeField] private string lyingPoseAnimation = "LyingPose";   

    [SerializeField] private Transform enemyModel;
    [SerializeField] private Transform landingPosition;
 

    private bool isDead;

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;

        // Configura la primera animación
        anim.CrossFade(idleAnimation);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            PlayDeathAnimation();
        }
    }

    private void OnDisable()
    {
     //   GetComponent<TopDownController3D>().enabled = true;
        GetComponent<CharacterController>().enabled = true;
       GetComponent<Rigidbody>().isKinematic = false;
       enemyModel.localPosition = Vector3.zero;
       enemyModel.localRotation = Quaternion.Euler(Vector3.zero);
        isDead = false;
        if(aiPath != null)
            aiPath.canMove = true;
    }

    public  void FollowTarget()
    {
        if(isDead) return;

        if (aiPath != null && target != null)
        {
        
      
            aiPath.destination = target.transform.position;

            // Ejemplo: Cambiar animación a 'Run' si se mueve
            if (!aiPath.isStopped)
            {
                if (anim.IsPlaying(idleAnimation))
                {
                   
                    InstantTransitionTo(runAnimation);
                }
            }
            else
            {
                if (anim.IsPlaying(runAnimation))
                {
                    InstantTransitionTo(idleAnimation);
                }
            }
        }
    }
    
    public void FollowAgentAuthoring()
    {
        if (agentAuthoring != null && target != null)
        {
            /*var body = agentAuthoring.EntityBody;
            body.Destination = target.position;
            body.IsStopped = false;
            agentAuthoring.EntityBody = body;*/
            agentAuthoring.SetDestination(target.position);
        }
    }

    public void PlayFlinchAnimation()
    {
        if(aiPath != null)
            aiPath.canMove = false;
        InstantTransitionTo(flinchAnimation, returnTo: idleAnimation);
        StartCoroutine(ResetCanMove(flinchAnimation));
    }

    public void PlayDeathAnimation()
    {
        if (aiPath != null) aiPath.canMove = false;
        GetComponent<Rigidbody>().isKinematic = true;
        anim.Stop();

        // Eleva al enemigo y añade rotación
        int jumpHeight = Random.Range(6,23) ;
        float jumpDuration = Random.Range(0.6f, 1.8f) ;
        float fallDuration = jumpDuration / 2;
        float rotationDuration = jumpDuration + fallDuration/2; // Duración total incluyendo la bajada
        float delayBeforeDescent = Random.Range(1f, 1.3f); // Tiempo que se espera antes de descender;

        // Animación de elevarse
        enemyModel.DOMoveY(enemyModel.localPosition.y + jumpHeight, jumpDuration)
            .OnStart((() => anim.Play(fallingPoseAnimation)))
            .OnComplete(() =>
            {
                // Reproduce la animación de muerte después de la elevación
                // anim.Play(deathAnimation);
            });

        // Animación de descenso con retraso
        enemyModel.DOMoveY(enemyModel.localPosition.y, fallDuration).SetDelay(delayBeforeDescent).SetEase(ease: Ease.InQuad).OnComplete((() =>
        {
            anim.Play(lyingPoseAnimation);
        }));

        int rotatioDirection = Random.Range(0, 2);

        float rotationY = enemyModel.rotation.eulerAngles.y;
        switch (rotatioDirection)
        {
            case 0:
                // Animación de rotación continua durante la elevación y el descenso
                enemyModel.DORotate(new Vector3(1080,rotationY , enemyModel.localRotation.z), rotationDuration, RotateMode.FastBeyond360)
                    .SetEase(Ease.OutQuad).OnComplete((() => anim.Play(lyingPoseAnimation)));
                break;
            case 1:
                // Animación de rotación continua durante la elevación y el descenso
                enemyModel.DORotate(new Vector3(1080,rotationY , enemyModel.localRotation.z), rotationDuration, RotateMode.FastBeyond360)
                    .SetEase(Ease.OutQuad).OnComplete((() => anim.Play(lyingPoseAnimation)));
                break;
        }
        /*float rotationY = transform.rotation.eulerAngles.y;
        switch (rotatioDirection)
        {
            case 0:
                // Animación de rotación continua durante la elevación y el descenso
                transform.DORotate(new Vector3(transform.rotation.x + 1800, rotationY, transform.rotation.z), rotationDuration,
                        RotateMode.FastBeyond360)
                    .SetEase(Ease.InOutQuad);
                break;
            case 1:
                // Animación de rotación continua durante la elevación y el descenso
                transform.DORotate(new Vector3(transform.rotation.x, rotationY, transform.rotation.z + 1800), rotationDuration,
                        RotateMode.FastBeyond360)
                    .SetEase(Ease.InOutQuad);
                break;
        }*/


        isDead = true;
        EnemyDeathEvent.Trigger(this);
    }

    IEnumerator ResetCanMove(string animationName)
    {
        yield return new WaitForSeconds(GetAnimationClipLength(animationName));
        if(!isDead && aiPath != null)
            aiPath.canMove = true;
    }

    // Método para manejar la transición instantánea
    private void InstantTransitionTo(string newAnimation, string returnTo = null)
    {
        if (!string.IsNullOrEmpty(returnTo))
        {
            StartCoroutine(PlayTemporaryAnimationCoroutine(newAnimation, returnTo));
        }
        else
        {
            anim.CrossFade(newAnimation);
        }
    }

    // Corutina para ejecutar una animación temporal y luego volver
    private IEnumerator PlayTemporaryAnimationCoroutine(string tempAnimation, string returnAnimation)
    {
        // Cambiar a la animación temporal
        anim.CrossFade(tempAnimation);

        // Esperar hasta que la animación temporal termine
        while (anim[tempAnimation].enabled)
        {
            yield return null;
        }

        if (!isDead)
        {
            // Cambiar de vuelta a la animación original
            anim.CrossFade(returnAnimation);
        }
     
  
    }
    
    private float GetAnimationClipLength(string clipName)
    {
        AnimationClip clip = anim.GetClip(clipName);

        if (clip != null)
        {
            return clip.length;
        }
        else
        {
            Debug.LogWarning($"Clip de animación '{clipName}' no encontrado.");
            return 0f;
        }
    }
}