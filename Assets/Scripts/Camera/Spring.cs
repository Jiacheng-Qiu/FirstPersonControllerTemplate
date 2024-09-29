using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spring
{
    public enum SpringType
    {
        PositionOverride,
        PositionAdditive,
        RotationOverride,
        RotationAdditive,
        ScaleOverride,
        ScaleAdditive
    }

    private SpringType m_springType;
    private Transform m_transform;
    private Vector3 m_stiffness;
    private Vector3 m_damping;
    private Vector3 m_velocity;
    private Vector3 m_position;
    private Vector3 m_lerpPosition; // The smoothed position per frame based on calculation from fixedUpdate

    private Vector3[] m_distributedForce = new Vector3[100]; // Up to 100 future calculations of effective force, apply starting from next calculation

    public Spring(SpringType type, Transform transform, Vector3 stiff, Vector3 damp)
    {
        m_springType = type;
        m_transform = transform;
        m_stiffness = stiff;
        m_damping = damp;
    }


    /// <summary>
    /// Reset the spring to original position and velocity
    /// </summary>
    public void Reset()
    {
        m_position = Vector3.zero;
        m_velocity = Vector3.zero;

        for (int i = 0; i < m_distributedForce.Count(); i++)
        {
            m_distributedForce[i] = Vector3.zero;
        }
    }

    /// <summary>
    /// Come along with normal Update method, call this within Update() to affect motion by one frame
    /// </summary>
    public void UpdateFrame()
    {
        m_lerpPosition = Vector3.Lerp(m_lerpPosition, m_position, 30 * Time.deltaTime);
        UpdateTransform();
    }

    /// <summary>
    /// Come along with FixedUpdate, call this within FixedUpdate() to affect physics calculation
    /// </summary>
    public void PhysicsUpdate()
    {
        if (m_distributedForce[0] != Vector3.zero)
        {
            AddForce(m_distributedForce[0]);
            for (int i = 0; i < m_distributedForce.Count(); i++)
            {
                m_distributedForce[i] = i < 99 ? m_distributedForce[i + 1] : Vector3.zero;
                if (m_distributedForce[i] == Vector3.zero)
                    break;
            }
        }
        UpdateSpring();
        UpdatePosition();
    }

    /// <summary>
    /// Apply instant force onto current spring
    /// </summary>
    /// <param name="force"></param>
    public void AddForce(Vector3 force)
    {
        m_velocity += force;
        UpdatePosition();
    }

    /// <summary>
    /// Apply distributed force onto spring, default as influencing one frame
    /// </summary>
    /// <param name="force"></param>
    public void AddDistributedForce(Vector3 force, int distribution = 1)
    {
        if (force == Vector3.zero)
            return;
        distribution = Mathf.Clamp(distribution, 1, 100);
        Vector3 distributedForce = force / distribution;
        for(int i = 0; i < distribution; i++)
        {
            m_distributedForce[i] += distributedForce;
        }
    }

    private void UpdateSpring()
    {
        m_velocity += Vector3.Scale(-m_position, m_stiffness);
        m_velocity = Vector3.Scale(m_velocity, Vector3.one - m_damping);
    }

    private void UpdatePosition()
    {
        m_position += m_velocity;
    }

    private void UpdateTransform()
    {
        switch(m_springType)
        {
            case SpringType.PositionOverride:
                m_transform.localPosition = m_lerpPosition;
                break;
            case SpringType.PositionAdditive:
                m_transform.localPosition += m_lerpPosition;
                break;
            case SpringType.RotationOverride:
                m_transform.localEulerAngles = m_lerpPosition;
                break;
            case SpringType.RotationAdditive:
                m_transform.localEulerAngles += m_lerpPosition;
                break;
            case SpringType.ScaleOverride:
                m_transform.localScale = m_lerpPosition;
                break;
            case SpringType.ScaleAdditive:
                m_transform.localScale += m_lerpPosition;
                break;
        }
    }
}