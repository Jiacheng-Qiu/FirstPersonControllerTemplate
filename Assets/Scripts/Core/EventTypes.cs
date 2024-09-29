using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventTypes
{
    MouseCursor,

    LoadingProgress,
    PlayerPostureInput,
    PlayerPostureChange,
    LockPlayerSightX,
    Jump,
    SilentWalk,
    Sprint,
    Climb,
    Interact,
    Inventory,

    JumpAction,
    SilentWalkAction,
    ClimbAction,
    SprintAction,
    FallAction,
    StepAction,

    InteractionTextPopup,
    InteractionTextHide,
    CursorGridSelect,
    CursorItemSelect,
}
