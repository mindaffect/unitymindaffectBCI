using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class DetectionModule : MonoBehaviour
{
    [Tooltip("The point representing the source of target-detection raycasts for the enemy AI")]
    public Transform detectionSourcePoint;
    [Tooltip("The max distance at which the enemy can see targets")]
    public float detectionRange = 20f;
    [Tooltip("The max distance at which the enemy can attack its target")]
    public float attackRange = 10f;
    [Tooltip("Time before an enemy abandons a known target that it can't see anymore")]
    public float knownTargetTimeout = 4f;

    public UnityAction onDetectedTarget;
    public UnityAction onLostTarget;

    public GameObject knownDetectedTarget { get; private set; }
    public bool isTargetInAttackRange { get; private set; }
    public bool isSeeingTarget { get; private set; }
    public bool hadKnownTarget { get; private set; }

    float m_TimeLastSeenTarget = Mathf.NegativeInfinity;

    ActorsManager m_ActorsManager;

    private void Start()
    {
        m_ActorsManager = FindObjectOfType<ActorsManager>();
        DebugUtility.HandleErrorIfNullFindObject<ActorsManager, EnemyController>(m_ActorsManager, this);
    }

    public void HandleTargetDetection(Actor actor, Collider[] selfColliders)
    {
        // Handle known target detection timeout
        if (knownDetectedTarget && !isSeeingTarget && (Time.time - m_TimeLastSeenTarget) > knownTargetTimeout)
        {
            knownDetectedTarget = null;
        }

        // Find the closest visible hostile actor
        float sqrDetectionRange = detectionRange * detectionRange;
        isSeeingTarget = false;
        float closestSqrDistance = Mathf.Infinity;
        foreach (Actor otherActor in m_ActorsManager.actors)
        {
            if (otherActor.affiliation != actor.affiliation)
            {
                float sqrDistance = (otherActor.transform.position - detectionSourcePoint.position).sqrMagnitude;
                if (sqrDistance < sqrDetectionRange && sqrDistance < closestSqrDistance)
                {
                    // Check for obstructions
                    RaycastHit[] hits = Physics.RaycastAll(detectionSourcePoint.position, (otherActor.aimPoint.position - detectionSourcePoint.position).normalized, detectionRange, -1, QueryTriggerInteraction.Ignore);
                    RaycastHit closestValidHit = new RaycastHit();
                    closestValidHit.distance = Mathf.Infinity;
                    bool foundValidHit = false;
                    foreach (var hit in hits)
                    {
                        if (!selfColliders.Contains(hit.collider) && hit.distance < closestValidHit.distance)
                        {
                            closestValidHit = hit;
                            foundValidHit = true;
                        }
                    }

                    if (foundValidHit)
                    {
                        Actor hitActor = closestValidHit.collider.GetComponentInParent<Actor>();
                        if (hitActor == otherActor)
                        {
                            isSeeingTarget = true;
                            closestSqrDistance = sqrDistance;

                            m_TimeLastSeenTarget = Time.time;
                            knownDetectedTarget = otherActor.aimPoint.gameObject;
                        }
                    }
                }
            }
        }

        isTargetInAttackRange = knownDetectedTarget != null && Vector3.Distance(transform.position, knownDetectedTarget.transform.position) <= attackRange;

        // Detection events
        if (!hadKnownTarget &&
            knownDetectedTarget != null &&
            onDetectedTarget != null)
        {
            onDetectedTarget.Invoke();
        }
        if (hadKnownTarget &&
            knownDetectedTarget == null &&
            onLostTarget != null)
        {
            onLostTarget.Invoke();
        }

        // Remember if we already knew a target (for next frame)
        hadKnownTarget = knownDetectedTarget != null;
    }

    public void OnDamaged(GameObject damageSource)
    {
        m_TimeLastSeenTarget = Time.time;
        knownDetectedTarget = damageSource;
    }
}
