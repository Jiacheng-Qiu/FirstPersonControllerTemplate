using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Adjust character controller height of player
/// </summary>
public class PlayerPosture : MonoBehaviour
{
    public enum PostureState
    {
        Stand,
        Crouch,
        Prone
    }

    private static readonly Vector3[] DETECTDIRECTIONS = new Vector3[] { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
    [SerializeField]
    private CharacterController m_characterController;
    [SerializeField]
    [Range(-1.8f, 0f)]
    private float m_crouchHeightOffset;
    [SerializeField]
    private float m_crouchTime;
    [SerializeField]
    [Range(-1.8f, 0f)]
    private float m_proneHeightOffset;
    [SerializeField]
    private float m_proneTime;


    private float m_heightToHeadTip = 0.2f;
    private float m_initialHeight;
    private PostureState m_curState;
    private float m_curHeightOffset;
    private float m_characterHeight;

    [SerializeField]
    private AnimationCurve m_verticalMovementAnim;

    private void Start()
    {
        m_characterHeight = m_characterController.height;
        m_initialHeight = transform.localPosition.y;
        m_heightToHeadTip = m_characterHeight - m_initialHeight + 0.01f;
        m_curHeightOffset = 0;
        m_curState = PostureState.Stand;

        Player.Instance.CurPosture = m_curState;

        EventBus.AddListener<PostureState>(EventTypes.PlayerPostureInput, ChangePosture);
    }

    private void ChangePosture(PostureState state)
    {
        // During jump, posture change is not allowed
        if (Player.Instance.IsJumping)
        {
            return;
        }

        float offset = 0;
        float duration = 0;

        // Actual State change is dependent on curent state, if target state is same as current state (meaning player clicked same button to reset), then return to stand
        if (state == m_curState && m_curState == PostureState.Stand)
        {
            return;
        }
        if (state == m_curState)
        {
            state = PostureState.Stand;
        }
        switch (state)
        {
            case PostureState.Stand:
                float heightOffset = (m_curState == PostureState.Crouch ? m_crouchHeightOffset : m_proneHeightOffset);
                foreach (Vector3 dir in DETECTDIRECTIONS)
                {
                    if (Physics.Raycast(transform.position + dir * m_characterController.radius, transform.up, m_heightToHeadTip - heightOffset))
                    {
                        Debug.LogWarning("Player Get Up Blocked By Obstacle");
                        return;
                    }
                }
                offset = 0;
                duration = (m_curState == PostureState.Crouch) ? m_crouchTime : m_proneTime;
                break;
            case PostureState.Crouch:
                if (Player.Instance.Stamina < 1)
                    return;
                if (m_curState == PostureState.Prone)
                {
                    heightOffset = m_proneHeightOffset - m_crouchHeightOffset;
                    foreach (Vector3 dir in DETECTDIRECTIONS)
                    {
                        if (Physics.Raycast(transform.position + dir * m_characterController.radius, transform.up, m_heightToHeadTip - heightOffset))
                        {
                            Debug.LogWarning("Player Get Up Blocked By Obstacle");
                            return;
                        }
                    }
                }
                offset = m_crouchHeightOffset;
                duration = m_crouchTime;
                break;
            case PostureState.Prone:
                if (Player.Instance.Stamina < 1)
                    return;
                offset = m_proneHeightOffset;
                duration = m_proneTime;
                break;
        }
        m_curState = state;
        Player.Instance.CurPosture = m_curState;
        // First change posture, then broadcast change
        EventBus.Broadcast(EventTypes.PlayerPostureChange);
        StopAllCoroutines();
        StartCoroutine(SetHeightOffset(offset, duration));
    }

    public PostureState GetCurrentPosture()
    {
        return m_curState;
    }

    private IEnumerator SetHeightOffset(float offset, float duration)
    {
        float e = 0;
        float startingHeight = m_curHeightOffset;
        while (e < duration)
        {
            m_curHeightOffset = Mathf.Lerp(startingHeight, offset, m_verticalMovementAnim.Evaluate(Mathf.Clamp01(e / duration)));
            transform.localPosition = (m_initialHeight + m_curHeightOffset) * Vector3.up;
            // Also adjust character controller height
            m_characterController.height = m_characterHeight + m_curHeightOffset;
            m_characterController.center = m_characterController.height / 2 * Vector3.up;
            e += Time.deltaTime;
            yield return null;
        }
    }

}
