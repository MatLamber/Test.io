using UnityEngine;
using Pathfinding;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;

[AddComponentMenu("TopDown Engine/Character/Abilities/Character Follow Path")]
public class CharacterFollowPath : CharacterAbility
{
    [Header("Path Settings")]
    [Tooltip("The distance at which the character considers it has reached a waypoint.")]
    [SerializeField]
    private float nextWaypointDistance = 1f;

    [Tooltip("Interval in seconds to recalculate the path to the target.")] [SerializeField]
    private float repathInterval = 1f;

    [Tooltip("The distance at which the character considers it has reached its destination.")] [SerializeField]
    private float distanceToStop = 0.5f;

    private CharacterMovement _characterMovementInstance;
    private Path _path;
    private int _currentWaypoint = 0;
    private float _lastRepathTime;
    private Seeker _seeker;
    private Transform target;
    private Enemy enemy => GetComponent<Enemy>();

    public float NextWaypointDistance => nextWaypointDistance;
    private CharacterController characterController => GetComponent<CharacterController>();

    protected override void Initialization()
    {
        base.Initialization();
        _characterMovementInstance = GetComponent<CharacterMovement>();
        _seeker = GetComponent<Seeker>();

        PathManager pathManager = FindObjectOfType<PathManager>();
        if (pathManager != null)
        {
            target = pathManager.Target;
        }

        RecalculatePath();
    }

    private void OnEnable()
    {
        // Ensure initial states are reset correctly when the game object is enabled
        ResetCharacterState();

        if (target != null)
        {
            RecalculatePath();
        }
    }

    private void ResetCharacterState()
    {
        _path = null;
        _currentWaypoint = 0;
        _lastRepathTime = 0;

        if (enemy != null)
        {
            enemy.IsDead = false; // Reset death state if applicable
        }
    }

    public override void ProcessAbility()
    {
        base.ProcessAbility();

        if (Time.time > _lastRepathTime + repathInterval)
        {
            RecalculatePath();
            _lastRepathTime = Time.time;
        }

        FollowPath();
    }

    protected virtual void RecalculatePath()
    {
        if (target == null)
        {
            _path = null;
            return;
        }

        if (_seeker != null)
        {
            Debug.Log("Recalculating path...");
            _seeker.StartPath(transform.position, target.position, OnPathComplete);
        }

        if (enemy != null)
        {
            enemy.ManageMovementAnimation(HasReachedDestination());
        }
    }

    protected virtual void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            _path = p;
            _currentWaypoint = 0;
        }
    }

    protected virtual void FollowPath()
    {
        if (_path == null || _characterMovementInstance == null || _currentWaypoint >= _path.vectorPath.Count)
        {
            return;
        }

        if (enemy != null && enemy.IsDead)
        {
            //_characterMovementInstance.SetMovement(Vector2.zero);
            Debug.Log("Character dead. Movement stopped.");
            return;
        }

        Vector3 direction = ((Vector3)_path.vectorPath[_currentWaypoint] - transform.position).normalized;
        Vector2 movement = new Vector2(direction.x, direction.z);
        _characterMovementInstance.SetMovement(movement);

        Debug.Log($"Moving to waypoint {_currentWaypoint}: {movement}");

        float distance = Vector3.Distance(transform.position, _path.vectorPath[_currentWaypoint]);

        if (distance < nextWaypointDistance)
        {
            _currentWaypoint++;
        }
    }

    public bool HasReachedDestination()
    {
        if (_path == null || target == null)
        {
            return false;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        return distanceToTarget < distanceToStop;
    }
}