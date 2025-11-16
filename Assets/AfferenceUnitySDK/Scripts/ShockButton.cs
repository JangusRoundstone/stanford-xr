using UnityEngine;

public class ShockButton : MonoBehaviour
{
    [Tooltip("How strong the shock should be. Tune this based on your calibration.")]
    public float shockValue = 1.0f;

    [Tooltip("How long to keep sending haptic samples (seconds).")]
    public float shockDuration = 0.2f;

    private bool _isShocking = false;

    // This is called by the UI Button OnClick
    public void TriggerShock()
    {
        if (HapticManager.Instance == null)
        {
            Debug.LogWarning("[ShockButton] No HapticManager.Instance found in scene.");
            return;
        }

        if (!HapticManager.Instance.stimActive)
        {
            Debug.LogWarning("[ShockButton] stimActive is false. Make sure ToggleStim() was called.");
            return;
        }

        if (!_isShocking)
            StartCoroutine(ShockRoutine());
    }

    private System.Collections.IEnumerator ShockRoutine()
    {
        _isShocking = true;

        float endTime = Time.time + shockDuration;

        // Send haptic samples every frame during the duration
        while (Time.time < endTime)
        {
            HapticManager.Instance.SendHaptic(shockValue);
            yield return null;
        }

        _isShocking = false;
    }
}
