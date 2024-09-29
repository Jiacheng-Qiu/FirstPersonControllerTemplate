using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static PlayerPosture;

public class PlayerMovement : MonoBehaviour
{
    private static readonly Vector3 GRAVITY = new Vector3(0, -19, 0);
    [SerializeField]
    private CharacterController m_controller;
    [SerializeField]
    private AnimationCurve m_slopeDeceleration;

    [SerializeField, Range(0f, 20f)]
    private float m_acceleration = 5f;
    [SerializeField, Range(0f, 20f)]
    private float m_forwardSpeed = 2.5f;
    [SerializeField, Range(0f, 20f)]
    private float m_sideSpeed = 2f;
    [SerializeField, Range(0f, 20f)]
    private float m_backSpeed = 1f;

    [Space]
    [SerializeField, Range(0f, 3f)]
    private float m_sprintMultiplier = 1.5f;
    [SerializeField, Range(0f, 3f)]
    private float m_silentWalkMultiplier = .7f;
    [SerializeField, Range(0f, 3f)]
    private float m_crouchMultiplier = .7f;
    [SerializeField, Range(0f, 3f)]
    private float m_proneMultiplier = .3f;

    [Space]
    [SerializeField, Range(0f, 2f)]
    private float m_stepSize = 0.8f;
    [SerializeField, Range(0f, 2f)]
    private float m_sprintStepSize = 1.2f;
    [SerializeField, Range(0f, 2f)]
    private float m_crouchStepSize = 0.4f;
    [SerializeField, Range(0f, 2f)]
    private float m_proneStepSize = 0.2f;

    [Space, SerializeField, Range(0f, 20f)]
    private float m_jumpHeight = 8f;
    [SerializeField, Range(0f, 3f)]
    private float m_jumpCD = 0.5f;

    [SerializeField, Range(0.1f, 3f)]
    private float m_climbSpeed = 0.5f;

    [Space, SerializeField, Range(0f, 5f)]
    private float m_slopeSmoothness = 1.5f;

    [Space, SerializeField]
    private float m_staminaStaticRecovery;
    [SerializeField]
    private float m_staminaWalkRecovery;
    [SerializeField]
    private float m_staminaSprintConsumption;
    [SerializeField]
    private float m_staminaCrouchConsumption;
    [SerializeField]
    private float m_staminaProneConsumption;
    [SerializeField]
    private float m_staminaJumpConsumption;
    [SerializeField]
    private float m_staminaClimbConsumption;
    [SerializeField]
    private float m_staminaRecoveryCD = 1f;


    private bool m_isGrounded;
    private Vector3 m_yVelocity = new Vector3(0, 1f, 0);
    private Vector3 m_curVelocity = Vector3.zero;
    private float m_lastJumpEndTime = 0;
    private Vector3 m_groundNormal = Vector3.zero;
    private float m_leftGround = 0;

    // Step animation related
    private float m_curStepLength;
    private Vector3 m_positionLastFrame;

    //Climbing related
    private List<GameObject> m_climbables = new();
    private List<GameObject> m_climbablesOverHeight = new();
    private GameObject m_onClimbObject;
    private Vector3 m_onClimbDirection;

    //Stamina related
    private bool m_staminaConsumedThisFrame; // If consumption is used this frame, then no recovery will happen
    private float m_staminaLastConsumptionTime = float.MinValue;

    private float m_leftGroundYPos = float.MinValue;

    private void Start()
    {
        m_positionLastFrame = transform.position;
        EventBus.AddListener(EventTypes.Sprint, SprintSwitch);
        EventBus.AddListener(EventTypes.Jump, TryJump);
        EventBus.AddListener(EventTypes.Climb, TryClimb);
        EventBus.AddListener(EventTypes.PlayerPostureChange, StopRun);
        EventBus.AddListener(EventTypes.SilentWalk, SilentWalkSwitch);
    }

    public bool AddNewClimbable(GameObject obj)
    {
        if (!m_climbables.Contains(obj))
        {
            m_climbables.Add(obj);
            return true;
        }
        return false;
    }

    public bool RemoveClimbable(GameObject obj)
    {
        if (Player.Instance.IsClimbing && m_onClimbObject == obj)
        {
            // When the climbing object exits the climb detecting trigger, meaning that player should be already above it, try to use gravity instead
            m_curVelocity = m_onClimbDirection + 2 * Time.fixedDeltaTime * GRAVITY;
        }
        return m_climbables.Remove(obj);
    }

    /// <summary>
    /// List of items within climbable range that are too tall
    /// </summary>
    public void SetCurOverHeightList(List<GameObject> objs)
    {
        m_climbablesOverHeight = objs;
    }

    /// <summary>
    /// Everytime player try to run, stop crouch, prone, and silent walk
    /// </summary>
    private void SprintSwitch()
    {
        if (!Player.Instance.Sprint)
        {
            // Run can only start when player is pressing movement keys
            if (Player.Instance.MoveInput.sqrMagnitude == 0)
            {
                return;
            }
            // Then check for silent walk
            if (Player.Instance.SilentWalk)
            {
                Player.Instance.SilentWalk = false;
                EventBus.Broadcast(EventTypes.SilentWalkAction);
            }
            // Check for player posture, adjust and return
            if (Player.Instance.CurPosture != PostureState.Stand)
            {
                EventBus.Broadcast<PostureState>(EventTypes.PlayerPostureInput, PostureState.Stand);
                return;
            }
            Player.Instance.Sprint = true;
            EventBus.Broadcast(EventTypes.SprintAction);
        }
        else
        {
            Player.Instance.Sprint = false;
            EventBus.Broadcast(EventTypes.SprintAction);
        }
    }

    private void SilentWalkSwitch()
    {
        if (!Player.Instance.SilentWalk && Player.Instance.Sprint)
        {
            Player.Instance.Sprint = false;
            EventBus.Broadcast(EventTypes.SprintAction);
        }
        Player.Instance.SilentWalk = !Player.Instance.SilentWalk;
        EventBus.Broadcast(EventTypes.SilentWalkAction);
    }

    private void StopRun()
    {
        if (Player.Instance.CurPosture != PostureState.Stand)
            Player.Instance.Sprint = false;

        if (Player.Instance.CurPosture == PostureState.Crouch)
        {
            Player.Instance.Stamina -= m_staminaCrouchConsumption;
            m_staminaConsumedThisFrame = true;
            m_staminaLastConsumptionTime = Time.time;
        } else if (Player.Instance.CurPosture == PostureState.Prone)
        {
            Player.Instance.Stamina -= m_staminaProneConsumption;
            m_staminaConsumedThisFrame = true;
            m_staminaLastConsumptionTime = Time.time;
        }
    }

    /// <summary>
    /// Jump/Climb will not happen simultaneously
    /// </summary>
    private void TryClimb()
    {
        if (Player.Instance.Stamina <= 0.5f)
            return;
        if (m_isGrounded)
        {
            // Check for silent walk first, disable if its on
            if (Player.Instance.SilentWalk)
            {
                Player.Instance.SilentWalk = false;
                EventBus.Broadcast(EventTypes.SilentWalkAction);
            }

            // If player is crouching or proning, this input session will return the posture to stand and do nothing
            if (Player.Instance.CurPosture != PostureState.Stand)
            {
                EventBus.Broadcast<PostureState>(EventTypes.PlayerPostureInput, PostureState.Stand);
                return;
            }

            // The first object in the climbables list that is not overheight will be processed as target
            if (m_climbables.Count > 0 && !m_climbablesOverHeight.Contains(m_climbables[0]))
            {
                // Disable player input until climb finish
                Player.Instance.AddMovementInputLimit();
                Player.Instance.AddSightInputLimit();

                m_onClimbObject = m_climbables[0];
                m_onClimbDirection = Vector3.Normalize(new Vector3(m_onClimbObject.transform.position.x - transform.position.x, 0, m_onClimbObject.transform.position.z - transform.position.z));
                m_curVelocity = m_onClimbDirection + m_climbSpeed * Vector3.up;
                Player.Instance.IsClimbing = true;
                EventBus.Broadcast(EventTypes.ClimbAction);
                Player.Instance.Stamina -= m_staminaClimbConsumption;
                m_staminaConsumedThisFrame = true;
                m_staminaLastConsumptionTime = Time.time;
            }
        }
    }

    /// <summary>
    /// If Player is on the ground, try to make him jump
    /// </summary>
    private void TryJump()
    {
        if (Player.Instance.Stamina <= 0.5f || Time.time < m_lastJumpEndTime + m_jumpCD)
        {
            return;
        }
        if (m_isGrounded)
        {
            // Check for silent walk first, disable if its on
            if (Player.Instance.SilentWalk)
            {
                Player.Instance.SilentWalk = false;
                EventBus.Broadcast(EventTypes.SilentWalkAction);
            }

            // If player is crouching or proning, this input session will return the posture to stand and do nothing
            if (Player.Instance.CurPosture != PostureState.Stand)
            {
                EventBus.Broadcast<PostureState>(EventTypes.PlayerPostureInput, PostureState.Stand);
                return;
            }

            // Give an upward force
            m_yVelocity = new Vector3(0, Mathf.Sqrt(40 * m_jumpHeight), 0);
            Player.Instance.IsJumping = true;
            EventBus.Broadcast(EventTypes.JumpAction);
            Player.Instance.Stamina -= m_staminaJumpConsumption;
            m_staminaConsumedThisFrame = true;
            m_staminaLastConsumptionTime = Time.time;
        }
    }

    private void Update()
    {
        m_staminaConsumedThisFrame = false;
        m_isGrounded = m_controller.isGrounded;
        // Record the y position player left ground
        if (!m_isGrounded && m_leftGroundYPos == float.MinValue)
        {
            m_leftGround = Time.time;
            m_leftGroundYPos = transform.position.y;
        }

        // Slide player off a slope if it is exceeding the limit
        if (m_isGrounded && Vector3.Angle(Vector3.up, m_groundNormal) > m_controller.slopeLimit)
        {
            m_isGrounded = false;
            // Because controller always reset the grounded state on slope, make sure timer isnt reset
            if (m_leftGround != float.MaxValue)
            {
                m_leftGround = Time.time;
            }
            m_controller.Move(m_slopeSmoothness * Time.deltaTime * new Vector3(m_groundNormal.x, -1, m_groundNormal.z));
        }
        if (!Player.Instance.IsClimbing)
        {
            Vector3 targetVelocity = VelocityCalculation(Player.Instance.MoveInput) + m_yVelocity;
            m_curVelocity = Vector3.Lerp(m_curVelocity, targetVelocity, m_acceleration * Time.deltaTime);
        }
        CollisionFlags flags = m_controller.Move(m_curVelocity * Time.deltaTime);

        // Check if new step animation should be played when step length is reached
        if (m_isGrounded && Player.Instance.MoveInput.magnitude > 0)
        {
            m_curStepLength += Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(m_positionLastFrame.x, m_positionLastFrame.z));
            if (Player.Instance.Sprint && m_curStepLength > m_sprintStepSize)
            {
                EventBus.Broadcast<float>(EventTypes.StepAction, 2.5f);
                m_curStepLength = 0;
            } 
            else if(Player.Instance.CurPosture == PostureState.Prone && m_curStepLength > m_proneStepSize)
            {
                EventBus.Broadcast<float>(EventTypes.StepAction, 2);
                m_curStepLength = 0;
            } 
            else if(Player.Instance.CurPosture == PostureState.Crouch && m_curStepLength > m_crouchStepSize)
            {
                EventBus.Broadcast<float>(EventTypes.StepAction, 0.9f);
                m_curStepLength = 0;
            } 
            else if(Player.Instance.CurPosture == PostureState.Stand && !Player.Instance.Sprint && m_curStepLength > m_stepSize)
            {
                EventBus.Broadcast<float>(EventTypes.StepAction, 1);
                m_curStepLength = 0;
            }
        } else
        {
            m_curStepLength = 0;
        }
        

        // When player reach ground, reset climb or fall or jump status
        if ((flags & CollisionFlags.Below) == CollisionFlags.Below && Time.time - m_leftGround > 0.05f)
        {
            if (Player.Instance.IsClimbing)
            {
                Player.Instance.IsClimbing = false;
                EventBus.Broadcast<float>(EventTypes.FallAction, 0.5f);
                Player.Instance.RemoveMovementInputLimit();
                Player.Instance.RemoveSightInputLimit();
            } 
            else if (Player.Instance.IsJumping)
            {
                Player.Instance.IsJumping = false;
                m_lastJumpEndTime = Time.time;
                EventBus.Broadcast(EventTypes.FallAction, (m_leftGroundYPos - transform.position.y) / m_jumpHeight + 1.2f);
            } else {
                // Falling event triggers same camera animation as jump-false effect
                EventBus.Broadcast(EventTypes.FallAction, (m_leftGroundYPos - transform.position.y) / m_jumpHeight);
            }

            m_leftGroundYPos = float.MinValue;
            m_leftGround = float.MaxValue;
            m_yVelocity = Time.fixedDeltaTime * GRAVITY;
        }
        // When head collides with upper ceiling, reverse the jumping force
        if ((flags & CollisionFlags.Above) == CollisionFlags.Above && !m_isGrounded && m_yVelocity.y > 0)
        {
            m_yVelocity.y = -m_yVelocity.y;
        }
        m_positionLastFrame = transform.position;

        if (!m_staminaConsumedThisFrame && Time.time > m_staminaLastConsumptionTime + m_staminaRecoveryCD)
        {
            m_staminaLastConsumptionTime = float.MinValue;
            Player.Instance.Stamina += (Player.Instance.MoveInput.magnitude > 0 ? m_staminaWalkRecovery : m_staminaStaticRecovery) * Time.deltaTime;
        }

        if (Player.Instance.Stamina < 0)
        {
            Player.Instance.Stamina = 0;
        }
        if (Player.Instance.Stamina > Player.Instance.StaminaMaxLimit)
        {
            Player.Instance.Stamina = Player.Instance.StaminaMaxLimit;
        }
    }

    private void FixedUpdate()
    {
        if (!m_isGrounded && !Player.Instance.IsClimbing)
        {
            m_yVelocity += Time.fixedDeltaTime * GRAVITY;
        }
    }

    /// <summary>
    /// Calculate the actual speed this frame based on posture and direction from input
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private Vector3 VelocityCalculation(Vector2 input)
    {
        input = Vector2.ClampMagnitude(input, 1f);
        if (input.sqrMagnitude > 0)
        {
            float speed = m_forwardSpeed;
            if (input.y < 0)
            {
                speed = m_backSpeed;
            } else if (input.x != 0)
            {
                speed = m_sideSpeed;
            }
            if (Player.Instance.CurPosture == PostureState.Prone)
            {
                speed *= m_proneMultiplier;
            } 
            else if (Player.Instance.CurPosture == PostureState.Crouch)
            {
                speed *= m_crouchMultiplier;
            }

            if (Player.Instance.Sprint)
            {
                if (Player.Instance.Stamina > 0)
                {
                    Player.Instance.Stamina -= m_staminaSprintConsumption * Time.deltaTime;
                    m_staminaConsumedThisFrame = true;
                    m_staminaLastConsumptionTime = Time.time;
                } else
                {
                    Player.Instance.Sprint = false;
                    EventBus.Broadcast(EventTypes.SprintAction);
                }
            }


            Vector3 movingDirection = transform.forward * input.y + transform.right * input.x;
            float slopeDeceleration = 1;
            // The movespeed on slope depends on if player in moving up the slope
            // dot product > 0 = moving down or parallel
            if (Vector3.Dot(movingDirection, m_groundNormal) < 0)
            {
                slopeDeceleration = m_slopeDeceleration.Evaluate(Vector3.Angle(Vector3.up, m_groundNormal) / m_controller.slopeLimit);
            }
            return speed * slopeDeceleration * (Player.Instance.Sprint ? m_sprintMultiplier : 1) * (Player.Instance.SilentWalk? m_silentWalkMultiplier : 1) * Player.Instance.SpeedMultiplier * movingDirection;
        } else
        {
            // Auto stop sprint when player is not trying to move
            Player.Instance.Sprint = false;
            EventBus.Broadcast(EventTypes.SprintAction);
        }
        return Vector3.zero;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        m_groundNormal = hit.normal;
    }
}
