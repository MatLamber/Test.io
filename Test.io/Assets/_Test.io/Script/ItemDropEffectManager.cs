using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class ItemDropEffectManager : MonoBehaviour
{
    [SerializeField] private List<Transform> explosionObjects = new List<Transform>();
    [SerializeField] private float fallTime = 2.0f;
    [SerializeField] private float spreadRadius = 5.0f;
    [SerializeField] private float moveToPlayerTime = 1.5f;

    private List<Vector3> initialPositions = new List<Vector3>();
    private List<Quaternion> initialRotations = new List<Quaternion>();
    private Transform playerTransform;

   [SerializeField] private bool moveToPlayerAllowed;

    private void OnEnable()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("El objeto jugador no fue encontrado. Asegúrate de que el jugador tenga el tag 'Player'.");
        }

        initialPositions.Clear();
        initialRotations.Clear();
        foreach (Transform obj in explosionObjects)
        {
            initialPositions.Add(obj.position);
            initialRotations.Add(obj.rotation);
        }
        
        StartExplosion();
    }

    public void StartExplosion()
    {
        if (playerTransform == null)
        {
            Debug.LogError("No se puede iniciar la explosión porque no se encontró el Transform del jugador.");
            return;
        }

        foreach (Transform obj in explosionObjects)
        {
            Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
            Vector3 targetPosition = obj.position + randomDirection * Random.Range(0, spreadRadius);
            targetPosition.y = 0;

            obj.DOMove(targetPosition, fallTime).SetEase(Ease.OutBounce);

        }

        Invoke(nameof(AllowMoveToPlayer), fallTime + 0.3f); // Permitir mover al jugador después de la explosión

    }


    private void AllowMoveToPlayer()
    {
        Debug.Log("Move");
        foreach (Transform obj in explosionObjects)
        {
            obj.GetComponent<Rigidbody>().isKinematic = true;
            obj.GetComponent<Collider>().isTrigger = true;
        }
        moveToPlayerAllowed = true;
    }

    private void Update()
    {
        
        gameObject.SetActive(!AreAllChildrenInactive());
        if (!moveToPlayerAllowed || playerTransform == null)
            return;

        foreach (Transform obj in explosionObjects)
        {
            obj.position = Vector3.MoveTowards(
                obj.position,
                playerTransform.position,
                (Time.deltaTime / moveToPlayerTime) * Vector3.Distance(obj.position, playerTransform.position)
            );
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < explosionObjects.Count; i++)
        {
            explosionObjects[i].position = initialPositions[i];
            explosionObjects[i].rotation = initialRotations[i];
            explosionObjects[i].GetComponent<Rigidbody>().isKinematic = false;
            explosionObjects[i].GetComponent<Collider>().isTrigger = false;
            explosionObjects[i].gameObject.SetActive(true);
        }
        
        moveToPlayerAllowed = false;
    }
    
    private bool AreAllChildrenInactive()
    {
        // Recorrer todos los hijos
        foreach (Transform child in transform)
        {
            // Si algún hijo está activo, devolver falso
            if (child.gameObject.activeSelf)
            {
                return false;
            }
        }
        // Si todos están desactivados, devolver verdadero
        return true;
    }
}