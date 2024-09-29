using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A specified list of interactions that player can cast on items, including scale, rotation, positon changes
/// </summary>
public class InteractionSpecifiedHandler : MonoBehaviour
{
    // Call this function to start a new rotation
    public void DoorRotation(float angle)
    {
        // Start a new rotation coroutine without interrupting other rotations
        Coroutine rotationCoroutine = StartCoroutine(RotateOverTime(0.5f, angle, Vector3.up));
    }

    private IEnumerator RotateOverTime(float time, float angle, Vector3 axis)
    {
        Quaternion initial = transform.rotation;
        Quaternion deltaRotation = Quaternion.AngleAxis(angle, axis);  // Incremental rotation to apply
        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            // Apply the rotation incrementally
            transform.rotation *= Quaternion.Slerp(Quaternion.identity, deltaRotation, Time.deltaTime / time);
            elapsedTime += Time.deltaTime;

            yield return null;
        }
        // To make sure its not under or overly rotated
        transform.rotation = initial * deltaRotation;
    }
}
