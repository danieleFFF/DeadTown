using UnityEngine;
using System.Collections;

public class CrouchCoroutine : MonoBehaviour
{
    public Transform cameraHolder;

    public float standingY = 0.46f;
    public float crouchY = 0.30f;
    public float transitionTime = 0.25f;
    public bool IsCrouching { get; private set; }
    Coroutine currentRoutine;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
            StartCrouch(true);

        if (Input.GetKeyUp(KeyCode.LeftControl))
            StartCrouch(false);
    }

    void StartCrouch(bool crouch)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(
            CrouchRoutine(crouch ? crouchY : standingY)
        );
        IsCrouching = crouch;
    }

    IEnumerator CrouchRoutine(float targetY)
    {
        float startY = cameraHolder.localPosition.y;
        float elapsed = 0f;

        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionTime;

            float y = Mathf.Lerp(startY, targetY, t);
            cameraHolder.localPosition = new Vector3(
                cameraHolder.localPosition.x,
                y,
                cameraHolder.localPosition.z
            );

            yield return null;
        }

        // assicurati che arrivi esattamente
        cameraHolder.localPosition = new Vector3(
            cameraHolder.localPosition.x,
            targetY,
            cameraHolder.localPosition.z
        );
    }
}