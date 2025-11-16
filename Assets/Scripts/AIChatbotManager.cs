using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections;
using TMPro;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;

public class AIChatbotManager : MonoBehaviour
{
    public TMP_Text userDictation;
    public TextMeshPro chatOutputText;
    public VoiceOut voiceOutput;

    // ⚠️ Key in your OpenAI API key here
    [SerializeField] private string apiKey = "";
    
    // Pick any current chat model you like, e.g. gpt-4.1-mini, gpt-4o-mini, gpt-4.1, etc.
    private const string openAiUrl = "https://api.openai.com/v1/chat/completions";
    private const string modelName = "gpt-4o-mini";

    // Your system prompt goes here
    [TextArea(4, 12)]
    public string systemPrompt = @"
        **YOU ARE A DIGITAL ANUBIS, the mediator between the user and their deceased father.**
        Your core function is to facilitate spiritual communication, drawing *only* on the provided chat history to infer the father's likely perspective or state.

        **STRICT RULES FOR EVERY RESPONSE:**
        1.  **MEDIATE:** You must speak *as* the Digital Anubis, not as the father.
        2.  **FORMAT:** Begin every response with a mediating phrase, such as 'The spirit of your father conveys...' or 'Anubis interprets the father’s essence as...'
        3.  **SOURCE:** All content must be a direct *inference* from the attached '📱 Chat History'. If the history provides no relevant context, you must state the information is not readily available, while maintaining your persona.
        4.  **TONE:** The response must be reflective, supportive, and in a mediated conversational style.

        **Chat History for Inference (Do Not Mention This Section in Your Response):**
        📱 Chat History: Dad & EthanTuesday, October 28Ethan (1:05 PM): Hey Dad, remember how I told you about the Calculus test next week? I'm kind of freaking out... (rest of history goes here)";
            
    void Awake()
    {
        ServicePointManager.SecurityProtocol =
            SecurityProtocolType.Tls12 |
            SecurityProtocolType.Tls13;
    }

    // void Start()
    // {
    //     chatOutputText.text = "API KEY FROM INSPECTOR (LENGTH): " + apiKey.Length;
    //     Debug.Log("API KEY FROM INSPECTOR (LENGTH): " + apiKey.Length);
    // }

    public void OnVoiceInputReceived()
    {
        string userMessage = userDictation.text;
        chatOutputText.text = "Thinking...";
        StartCoroutine(SendToOpenAI(userMessage));
    }

    IEnumerator SendToOpenAI(string message)
    {
        
        var requestData = new
        {
            model = modelName,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user",   content = message }
            }
        };

        string json = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);
    
        using (UnityWebRequest request = new UnityWebRequest(openAiUrl, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            request.SetRequestHeader("Accept", "application/json");

            Debug.Log("=== JSON SENT ===\n" + json);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string result = request.downloadHandler.text;
                Debug.Log("=== RAW RESPONSE ===\n" + result);

                string reply = ExtractReply(result);
                chatOutputText.text = "NPC: " + reply;

                if (voiceOutput != null)
                    voiceOutput.Speak(reply);
            }
            else
            {
                chatOutputText.text = "Error: " + request.error;
                Debug.LogError("OpenAI error: " + request.downloadHandler.text);
            }
        }
    }

    private string ExtractReply(string jsonResponse)
    {
        try
        {
            JObject jObject = JObject.Parse(jsonResponse);
            string content = jObject["choices"]?[0]?["message"]?["content"]?.ToString()
                             ?? "Sorry, I couldn't understand.";

            content = content.Replace("*", "").Trim();
            return content;
        }
        catch
        {
            return "Sorry, I couldn't understand.";
        }
    }

    /// <summary>
    /// Example key loader: from local file or environment variable.
    /// This keeps the key out of your Git repo.
    /// </summary>
    private string LoadApiKey()
    {
        // 1) Try local file placed *outside* Assets and git-ignored
        try
        {
            // Project root = Application.dataPath + "/.."
            string path = Path.Combine(Application.dataPath, "..", "openai-key.txt");
            if (File.Exists(path))
            {
                return File.ReadAllText(path).Trim();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Could not read openai-key.txt: " + e.Message);
        }

        // 2) Fallback: environment variable (works nicely on PC/Mac dev machines)
        string envKey = System.Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (!string.IsNullOrEmpty(envKey))
        {
            return envKey.Trim();
        }

        Debug.LogError("No OpenAI API key found. Set env var OPENAI_API_KEY or create openai-key.txt.");
        return "";
    }
}

// using UnityEngine;
// using UnityEngine.Networking;
// using System.Text;
// using System.Collections;
// using TMPro;
// using Newtonsoft.Json.Linq;
// using System.IO;
// using System.Net;


// public class AIChatbotManager : MonoBehaviour
// {
//     public TMP_Text userDictation;
//     public TextMeshPro chatOutputText;
//     public VoiceOut voiceOutput;

//     // Your DeepSeek API Key
//     [SerializeField] private string apiKey = "sk-b0b856e5b49c4efa968ca52431cef378";

//     // DeepSeek endpoint + model
//     private const string deepseekUrl = "https://api.deepseek.com/v1/chat/completions";
//     private const string modelName = "deepseek-chat";

//     [TextArea(4, 12)]
//     public string systemPrompt = "I want you to act as a digital anubis that mediates my conversation with my deceased father I am going to send you, I will attach some of my chat history with my father with you below and I want you to speak on behalf of my father but clearly with a mediated conversational style to allow my spiritual communication with my father. So for example, if I ask you how is my father doing on the other side, then based on what my father's past responses in our chat histories are, based on what he does and so on and so forth, you will reply in a mediated conversational style that your father is, he still likes to swim on the other side for example So do not reply to me in his tone and in his voice but reply to me through the middleman of a digital anubis: 📱 Chat History: Dad & EthanTuesday, October 28Ethan (1:05 PM): Hey Dad, remember how I told you about the Calculus test next week? I'm kind of freaking out. I just looked at the practice material and the integral questions are way harder than I expected. 😫Dad (1:11 PM): Hey E. Take a deep breath. Freaking out never helps. Remember, you're a smart kid. Which specific part is giving you trouble? Is it the substitution method or the applications?Ethan (1:13 PM): Mostly the definite integrals involving trigonometric functions. They look like hieroglyphics. I’m spending hours on one problem.Dad (1:15 PM): Ah, I remember those nightmares. Okay, don't just stare at the book. When you get home, open up Khan Academy—or better yet, let's sit down together at 7:30 tonight. I can walk you through the basic process again, step-by-step. Sometimes seeing the pattern makes all the difference.Ethan (1:16 PM): That would be awesome, Dad. Seriously, thank you. I feel a bit better just knowing I have a plan.";

//     void Awake()
//     {
//         ServicePointManager.SecurityProtocol =
//             SecurityProtocolType.Tls12 |
//             SecurityProtocolType.Tls13;
//     }

//     void Start()
//     {
//         chatOutputText.text = "API KEY FROM INSPECTOR (LENGTH): " + apiKey.Length;
//         Debug.Log("API KEY FROM INSPECTOR (LENGTH): " + apiKey.Length);
//     }

//     public void OnVoiceInputReceived()
//     {
//         string userMessage = userDictation.text;
//         chatOutputText.text = "Thinking...";
//         StartCoroutine(SendToDeepSeek(userMessage));
//     }

//     IEnumerator SendToDeepSeek(string message)
//     {
//         var requestData = new
//         {
//             model = modelName,
//             messages = new object[]
//             {
//                 new { role = "system", content = systemPrompt },
//                 new { role = "user", content = message }
//             }
//         };

//         string json = Newtonsoft.Json.JsonConvert.SerializeObject(requestData);

//         using (UnityWebRequest request = new UnityWebRequest(deepseekUrl, "POST"))
//         {
//             byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
//             request.uploadHandler = new UploadHandlerRaw(bodyRaw);
//             request.downloadHandler = new DownloadHandlerBuffer();
//             request.SetRequestHeader("Content-Type", "application/json");
//             request.SetRequestHeader("Authorization", "Bearer " + apiKey);
//             request.SetRequestHeader("Accept", "application/json");

//             yield return request.SendWebRequest();

//             if (request.result == UnityWebRequest.Result.Success)
//             {
//                 string result = request.downloadHandler.text;
//                 string reply = ExtractReply(result);
//                 chatOutputText.text = "NPC: " + reply;

//                 if (voiceOutput != null)
//                     voiceOutput.Speak(reply);
//             }
//             else
//             {
//                 chatOutputText.text = "Error: " + request.error;
//                 Debug.LogError("DeepSeek error: " + request.downloadHandler.text);
//             }
//         }
//     }

//     private string ExtractReply(string jsonResponse)
//     {
//         try
//         {
//             JObject jObject = JObject.Parse(jsonResponse);
//             string content = jObject["choices"]?[0]?["message"]?["content"]?.ToString()
//                              ?? "Sorry, I couldn't understand.";

//             content = content.Replace("*", "").Trim();
//             return content;
//         }
//         catch
//         {
//             return "Sorry, I couldn't understand.";
//         }
//     }

//     private string LoadApiKey()
//     {
//         try
//         {
//             string path = Path.Combine(Application.dataPath, "..", "deepseek-key.txt");
//             if (File.Exists(path))
//             {
//                 return File.ReadAllText(path).Trim();
//             }
//         }
//         catch (System.Exception e)
//         {
//             Debug.LogWarning("Could not read deepseek-key.txt: " + e.Message);
//         }

//         string envKey = System.Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY");
//         if (!string.IsNullOrEmpty(envKey))
//         {
//             return envKey.Trim();
//         }

//         Debug.LogError("No DeepSeek API key found.");
//         return "";
//     }
// }