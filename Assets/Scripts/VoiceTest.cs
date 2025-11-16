using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Voice;

public class VoiceTest : MonoBehaviour
{
    public AppVoiceExperience voiceExperience;

    // Update is called once per frame
    public void StartDictation()
    {
        voiceExperience.Activate();
    }
}
