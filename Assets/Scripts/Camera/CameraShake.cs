using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField]
    private float m_springStiffness;
    [SerializeField]
    private float m_springDamping;
    [SerializeField]
    private Vector3 m_stepRotationForce;
    [SerializeField]
    private int m_stepRotationDistribution;
    [SerializeField]
    private Vector3 m_jumpRotationForce;
    [SerializeField]
    private int m_jumpRotationDistribution;
    [SerializeField]
    private Vector3 m_climbRotationForce;
    [SerializeField]
    private int m_climbRotationDistribution;
    [SerializeField]
    private Vector3 m_fallRotationForce;
    [SerializeField]
    private int m_fallRotationDistribution;

    private Spring m_positionSpring;
    private Spring m_rotationSpring;

    private bool isLeftStep;

    private void Awake()
    {
        m_positionSpring = new Spring(Spring.SpringType.PositionOverride, transform, m_springStiffness * Vector3.one, m_springDamping * Vector3.one);
        m_rotationSpring = new Spring(Spring.SpringType.RotationOverride, transform, m_springStiffness * Vector3.one, m_springDamping * Vector3.one);
    }

    private void Start()
    {
        EventBus.AddListener<float>(EventTypes.StepAction, StepAnimation);
        EventBus.AddListener(EventTypes.JumpAction, JumpAnimation);
        EventBus.AddListener<float>(EventTypes.FallAction, FallAnimation);
        EventBus.AddListener(EventTypes.ClimbAction, ClimbAnimation);
        EventBus.AddListener(EventTypes.SprintAction, SprintAnimation);
        EventBus.AddListener(EventTypes.SilentWalkAction, SilentWalkAction);
        EventBus.AddListener(EventTypes.PlayerPostureChange, PostureAction);
    }

    private void Update()
    {
        m_positionSpring.UpdateFrame();
        m_rotationSpring.UpdateFrame();
    }

    private void FixedUpdate()
    {
        m_positionSpring.PhysicsUpdate();
        m_rotationSpring.PhysicsUpdate();
    }

    private void StepAnimation(float intensity)
    {
        int stepInvert = (isLeftStep ? -1 : 1);
        m_rotationSpring.AddDistributedForce(intensity * new Vector3(m_stepRotationForce.x, stepInvert * m_stepRotationForce.y, stepInvert * m_stepRotationForce.z), m_stepRotationDistribution);
        isLeftStep = !isLeftStep;
    }

    private void JumpAnimation()
    {
        if (Player.Instance.IsJumping)
        {
            m_rotationSpring.AddDistributedForce(m_jumpRotationForce, m_jumpRotationDistribution);
        }
    }

    private void FallAnimation(float forceMultiplier)
    {
        // If fall force is too small, ignore
        if (forceMultiplier < 0.1f)
            return;
        m_rotationSpring.AddDistributedForce(m_fallRotationForce * forceMultiplier, m_fallRotationDistribution);
    }

    private void ClimbAnimation()
    {
        if (Player.Instance.IsClimbing)
        {
            m_rotationSpring.AddDistributedForce(m_climbRotationForce, m_climbRotationDistribution);
        }
        else
        {
            FallAnimation(1);
        }
    }

    private void SprintAnimation()
    {

    }

    private void SilentWalkAction()
    {

    }

    private void PostureAction()
    {

    }
}
