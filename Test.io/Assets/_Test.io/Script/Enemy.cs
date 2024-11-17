using System.Collections;
using UnityEngine;
using Pathfinding;

public class Enemy : MonoBehaviour
{
    private Transform target;

    // Referencia al componente AIPath
    private AIPath aiPath => GetComponent<AIPath>();

    // Referencia al componente Animation
    [SerializeField] private Animation anim;

    // Serialized Fields para las animaciones
    [SerializeField] private string idleAnimation = "Idle";
    [SerializeField] private string runAnimation = "Run";
    [SerializeField] private string attackAnimation = "Attack";
    [SerializeField] private string flinchAnimation = "Flinch";
    [SerializeField] private string deathAnimation = "Flinch";


    private bool isDead;

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;

        // Configura la primera animación
        anim.CrossFade(idleAnimation);
    }

    void LateUpdate()
    {
        FollowTarget();
    }

    private void FollowTarget()
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

    public void PlayFlinchAnimation()
    {
        aiPath.canMove = false;
        InstantTransitionTo(flinchAnimation, returnTo: idleAnimation);
        StartCoroutine(ResetCanMove(flinchAnimation));
    }

    public void PlayDeathAnimation()
    {
       
        aiPath.canMove = false;
        anim.Stop();
        anim.Play(deathAnimation);
        isDead = true;
    }

    IEnumerator ResetCanMove(string animationName)
    {
        yield return new WaitForSeconds(GetAnimationClipLength(animationName));
        if(!isDead)
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