using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerPosture;

/// <summary>
/// Adjust player rotation based on mouse input
/// </summary>
public class PlayerSight : MonoBehaviour
{
    public float Sensitivity;
    public float CrouchYSensitivityMultiplier = 1;
    public float ProneYSensitivityMultiplier = 1;
    public bool InvertInput = false;

    [SerializeField]
    private Transform m_cameraRoot; // For rotation on X
    [SerializeField]
    private Transform m_playerRoot; // For rotation on Y
    [SerializeField]
    private Vector2 m_standLookXLimit = new Vector2(-60f, 80f);
    [SerializeField]
    private Vector2 m_crouchLookXLimit = new Vector2(-60f, 80f);
    [SerializeField]
    private Vector2 m_proneLookXLimit = new Vector2(-5f, 30f);
    [SerializeField, Range(0f, 1f)]
    private float m_forceLookAdjustTime = 0.5f;

    private Vector2 m_curLookXLimit;

    private Vector2 m_curLookAngle;
    private bool m_lockPlayerSightX = false;
    private float m_curSensitivityMultiplier;

    private void Start()
    {
        m_curSensitivityMultiplier = 1;
        m_curLookXLimit = m_standLookXLimit;
        m_curLookAngle = new Vector2(m_cameraRoot.localEulerAngles.x, m_playerRoot.localEulerAngles.y);
        EventBus.AddListener(EventTypes.PlayerPostureChange, LookYLimitChange);
    }

    private void LateUpdate()
    {
        if (!m_lockPlayerSightX)
        {
            m_curLookAngle.x += Player.Instance.LookInput.y * Sensitivity * (InvertInput ? 1 : -1);
            m_curLookAngle.x = ClampAngle(m_curLookAngle.x, m_curLookXLimit.x, m_curLookXLimit.y);
            m_cameraRoot.localRotation = Quaternion.Euler(m_curLookAngle.x, 0, 0);
        }
        m_curLookAngle.y += Player.Instance.LookInput.x * Sensitivity * (InvertInput ? -1 : 1) * m_curSensitivityMultiplier;

        m_playerRoot.localRotation = Quaternion.Euler(0, m_curLookAngle.y, 0);
        Player.Instance.LookDirection = (m_cameraRoot.forward);
    }

    private float ClampAngle(float angle, float min, float max)
    {
        angle %= 360;
        return Mathf.Clamp(angle, min, max);
    }

    private void LookYLimitChange()
    {
        switch(Player.Instance.CurPosture)
        {
            case PostureState.Stand:
                m_curLookXLimit = m_standLookXLimit;
                m_curSensitivityMultiplier = 1;
                break;
            case PostureState.Crouch:
                m_curLookXLimit = m_crouchLookXLimit;
                m_curSensitivityMultiplier = CrouchYSensitivityMultiplier;
                break;
            case PostureState.Prone:
                m_curLookXLimit = m_proneLookXLimit;
                m_curSensitivityMultiplier = ProneYSensitivityMultiplier;
                break;
        }
        // If current angle exceed limit, lerp it back
        StartCoroutine(LookAngleForceLimit());
    }

    private IEnumerator LookAngleForceLimit()
    {
        float startingAngle = m_curLookAngle.x;
        startingAngle %= 360;
        if (startingAngle >= m_curLookXLimit.x && startingAngle <= m_curLookXLimit.y)
        {
            yield break;
        }
        EventBus.Broadcast<bool>(EventTypes.LockPlayerSightX, true);
        m_lockPlayerSightX = true;

        float targetAngle = 0;
        if (startingAngle < m_curLookXLimit.x)
        {
            targetAngle = m_curLookXLimit.x;
        } 
        else if (startingAngle > m_curLookXLimit.y)
        {
            targetAngle = m_curLookXLimit.y;
        }

        float e = 0;
        while (e < m_forceLookAdjustTime)
        {
            m_curLookAngle.x = Mathf.Lerp(startingAngle, targetAngle, e / m_forceLookAdjustTime);
            m_cameraRoot.localRotation = Quaternion.Euler(m_curLookAngle.x, 0, 0);
            e += Time.deltaTime;
            yield return null;
        }

        EventBus.Broadcast<bool>(EventTypes.LockPlayerSightX, false);
        m_lockPlayerSightX = false;

        yield return null;
    }
}
