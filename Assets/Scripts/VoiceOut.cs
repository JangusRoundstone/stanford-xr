using UnityEngine;
using System.Threading.Tasks;
using System;
#if WINDOWS_UWP
using Windows.Media.SpeechSynthesis;
using Windows.Storage.Streams;
using Windows.Foundation;
using System.Runtime.InteropServices.WindowsRuntime;
#endif

[RequireComponent(typeof(AudioSource))]
public class VoiceOut : MonoBehaviour
{
#if WINDOWS_UWP
    private SpeechSynthesizer synthesizer;
#endif
    public AudioSource audioSource;

    void Awake()
    {
        // audioSource = GetComponent<AudioSource>();
#if WINDOWS_UWP
        synthesizer = new SpeechSynthesizer();
#endif
    }

    public void Speak(string text)
    {
#if WINDOWS_UWP
        // Run in background thread
        Task.Run(async () =>
        {
            try
            {
                SpeechSynthesisStream stream = await synthesizer.SynthesizeTextToStreamAsync(text);

                byte[] buffer = new byte[stream.Size];
                using (var reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(buffer);
                }

                // Marshal back to Unity main thread
                UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                {
                    var clip = WavUtility.ToAudioClip(buffer, "Speech");
                    audioSource.clip = clip;
                    audioSource.Play();
                }, false);
            }
            catch (Exception ex)
            {
                Debug.LogError("Speech synthesis failed: " + ex.Message);
            }
        });
#else
        Debug.LogWarning("Speech synthesis is only supported on UWP/HoloLens 2 build.");
#endif
    }
}
