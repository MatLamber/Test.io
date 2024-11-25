using UnityEngine;
using Pathfinding;

public class PathManager : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float updateInterval = 0.5f;

    private Seeker seeker;
    private Path path;
    
    public Transform Target  // Getter público
    {
        get { return target; }
    }

    // Singleton Instance
    public static PathManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        seeker = GetComponent<Seeker>();
        InvokeRepeating("UpdatePath", 0f, updateInterval);
    }

    void UpdatePath()
    {
        if (target != null && seeker.IsDone())
        {
            seeker.StartPath(transform.position, target.position, OnPathComplete);
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
        }
    }

    public Path GetCurrentPath()
    {
        return path;
    }
}