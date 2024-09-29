using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerPosture;

/// <summary>
/// Store Player Based stats, so that all can access
/// </summary>
public class Player : CSingletonMono<Player>
{
    public bool MovementInputEnabled = true;
    private int m_movementInputControl = 0;
    public bool SightInputEnabled = true;
    private int m_sightInputControl = 0;

    public Camera PlayerCamera;
    public Vector2 LookInput;
    public PostureState CurPosture;
    public Vector3 LookDirection;
    public float StaminaMaxLimit = 100;
    public float Stamina = 100;
    public Vector2 MoveInput;
    public float SpeedMultiplier = 1; // Based on buff and debuff
    public bool SilentWalk = false;
    public bool Sprint = false;
    public bool IsJumping = false;
    public bool IsClimbing = false;

    public bool CanInteract()
    {
        return !IsJumping && !IsClimbing;
    }

    /// <summary>
    /// When multiple source adjust input states, player might be able to move when he shouldn't, store all requests
    /// </summary>
    public void AddMovementInputLimit()
    {
        m_movementInputControl++;
        MovementInputEnabled = false;
    }

    public void RemoveMovementInputLimit()
    {
        m_movementInputControl--;
        if (m_movementInputControl < 0)
        {
            Debug.LogWarning("State change recording for movement went wrong");
            m_movementInputControl = 0;
        }
        if (m_movementInputControl == 0)
        {
            MovementInputEnabled = true;
        }
    }
    
    /// <summary>
    /// When multiple source adjust input states, player might be able to move when he shouldn't
    /// </summary>
    public void AddSightInputLimit()
    {
        m_sightInputControl++;
        SightInputEnabled = false;
    }

    public void RemoveSightInputLimit()
    {
        m_sightInputControl--;
        if (m_sightInputControl < 0)
        {
            Debug.LogWarning("State change recording for sight went wrong");
            m_sightInputControl = 0;
        }
        if (m_sightInputControl == 0)
        {
            SightInputEnabled = true;
        }
    }
}
