using System;
using System.Collections.Generic;
using UnityEngine;

public class HapticTimelineTrigger
 : MonoBehaviour
{
    [Header("Timeline Settings")]
    public float videoDuration = 60f;
    public List<HapticEventMarker> hapticEvents = new();

    [Header("Haptic Playback")]
    public HapticEventPulse hapticEventPulse;

    private float currentTime = 0f;
    private int currentIndex = 0;
    private bool isPlaying = false;

    void Update()
    {
        if (!isPlaying || currentIndex >= hapticEvents.Count)
            return;

        currentTime += Time.deltaTime;

        while (currentIndex < hapticEvents.Count && currentTime >= hapticEvents[currentIndex].time)
        {
            TriggerHaptic(hapticEvents[currentIndex].intensity);
            currentIndex++;
        }

        if (currentTime >= videoDuration)
        {
            isPlaying = false;
        }
    }

    public void StartHapticTimeline()
    {
        currentTime = 0f;
        currentIndex = 0;
        isPlaying = true;
        Debug.Log("[HapticTimelineWithPulse] Started timeline.");
    }

    private void TriggerHaptic(float intensity)
    {
        if (hapticEventPulse == null)
        {
            Debug.LogWarning("[HapticTimelineWithPulse] Missing HapticEventPulse reference.");
            return;
        }

        // Set custom burst with just one point at 0ms delay and given intensity
        hapticEventPulse.useCustomBurst = true;
        hapticEventPulse.customBurst = new Vector2[] { new Vector2(0f, Mathf.Clamp01(intensity)) };

        hapticEventPulse.PlayHaptic();

        Debug.Log($"[HapticTimelineWithPulse] Haptic triggered at {currentTime:F2}s with intensity {intensity}");
    }
}

[Serializable]
public class HapticEventMarker
{
    public float time;

    [Range(0f, 1f)]
    public float intensity = 1f;
}
