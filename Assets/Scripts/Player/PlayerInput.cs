using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerPosture;

public class PlayerInput : MonoBehaviour
{
    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        EventBus.AddListener<bool>(EventTypes.MouseCursor, SwitchMouseState);
    }

    public void SwitchMouseState(bool onLock)
    {
        if (onLock)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void Update()
    {

        if (Player.Instance.MovementInputEnabled)
        {
            Player.Instance.MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (Input.GetButtonDown("Crouch"))
            {
                EventBus.Broadcast<PostureState>(EventTypes.PlayerPostureInput, PostureState.Crouch);
            }
            if (Input.GetButtonDown("Prone"))
            {
                EventBus.Broadcast<PostureState>(EventTypes.PlayerPostureInput, PostureState.Prone);
            }
            if (Input.GetButtonDown("Climb"))
            {
                EventBus.Broadcast(EventTypes.Climb);
            }
            if (Input.GetButtonDown("Jump"))
            {
                EventBus.Broadcast(EventTypes.Jump);
            }
            if (Input.GetButtonDown("SilentWalk"))
            {
                EventBus.Broadcast(EventTypes.SilentWalk);
            }
            if (Input.GetButtonDown("Sprint"))
            {
                EventBus.Broadcast(EventTypes.Sprint);
            }
            if (Input.GetButtonDown("Interact"))
            {
                EventBus.Broadcast(EventTypes.Interact);
            }
        } else
        {
            Player.Instance.MoveInput = Vector2.zero;
        }

        if (Player.Instance.SightInputEnabled)
        {
            Player.Instance.LookInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        } else
        {
            Player.Instance.LookInput = Vector2.zero;
        }
    }

}
