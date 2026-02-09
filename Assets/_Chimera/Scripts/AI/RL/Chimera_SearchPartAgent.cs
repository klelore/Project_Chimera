using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

[RequireComponent(typeof(Rigidbody))]
public class Chimera_SearchPartAgent : Agent
{
    public enum AgentMode { Aggressive, Conservative }
    [Header("Strategy Settings")]
    public AgentMode currentMode = AgentMode.Aggressive;

    [Header("References")]
    public TrainingAreaController areaController; 
    public AnimationManager animManager;          
    public Transform targetTransform;             

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float turnSpeed = 200f;

    private Rigidbody rb;
    private float previousDistance;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();

        if (animManager == null) animManager = GetComponentInChildren<AnimationManager>();
    }

    public override void OnEpisodeBegin()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (areaController != null)
        {
            areaController.ResetArea();
        }

        // ³õÊ¼»¯¾àÀë¼ÇÂ¼
        if (targetTransform != null)
        {
            previousDistance = Vector3.Distance(transform.position, targetTransform.position);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (targetTransform != null)
        {
            Vector3 relativePosition = transform.InverseTransformPoint(targetTransform.position);
            sensor.AddObservation(relativePosition);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
        }

        sensor.AddObservation(transform.InverseTransformDirection(rb.velocity));

    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float moveSignal = actionBuffers.ContinuousActions[0];
        float rotateSignal = actionBuffers.ContinuousActions[1];

        Vector3 force = transform.forward * moveSignal * moveSpeed;
        rb.AddForce(force);
        transform.Rotate(0, rotateSignal * turnSpeed * Time.fixedDeltaTime, 0);

        if (animManager != null) animManager.UpdateMovementState(rb.velocity);

        if (targetTransform != null)
        {
            float currentDistance = Vector3.Distance(transform.position, targetTransform.position);
            float distanceDelta = previousDistance - currentDistance;

            float multiplier = (currentMode == AgentMode.Aggressive) ? 0.02f : 0.01f;
            AddReward(distanceDelta * multiplier);

            float velocityTowardsTarget = Vector3.Dot(rb.velocity, (targetTransform.position - transform.position).normalized);
            if (velocityTowardsTarget > 0.1f)
            {
                AddReward(velocityTowardsTarget * 0.001f);
            }

            previousDistance = currentDistance;
        }

        float stepPenalty = (currentMode == AgentMode.Aggressive) ? -0.002f : -0.0005f;
        AddReward(stepPenalty);
    }

    private void OnCollisionEnter(Collision collision)
    {

        float obstaclePenalty = (currentMode == AgentMode.Aggressive) ? -0.3f : -1.0f;
        float wallPenalty = -0.5f;
        float targetReward = 2.0f;

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            AddReward(obstaclePenalty);

            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(wallPenalty);
            EndEpisode();
        }
        else if (collision.gameObject.CompareTag("Target"))
        {
            AddReward(targetReward);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");   
        continuousActionsOut[1] = Input.GetAxis("Horizontal"); 
    }
}
