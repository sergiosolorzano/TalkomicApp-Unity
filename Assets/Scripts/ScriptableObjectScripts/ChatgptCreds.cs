//using System.Net.NetworkInformation;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
//using static UnityEditor.PlayerSettings;
//using static UnityEngine.UIElements.UxmlAttributeDescription;

[CreateAssetMenu(fileName = "Azure_Chatgpt_Credentials", menuName = "ChatGPT_Credentials/Azure_API")]
public class ChatgptCreds: ScriptableObject
{
    [Header("Credentials Settings")]
    //Azure openai deployment id
    public string deployment_id = "";
    //Azure openai model name
    public string model = "";
    //Azure openai secret
    public string secret = "";
    //Azure chatgpt endpoint
    public string url = "";
    //Azure chatgpt endpoint
    public string api_version = "";

    [Header("Request Settings")]
    //system role content
    public string systemContent = "You are a helpful assistant.";
    public float temperature = 0.7f;
    public int maxTokens = 4096;
    //used to discourage the model from repeating the same words or phrases too frequently within the generated text.It is a value that is added to the log-probability of a token each time it occurs in the generated text.A higher frequency_penalty value will result in the model being more conservative in its use of repeated tokens.
    public float frequencyPenalty = 0.75f;
    //to encourage the model to include a diverse range of tokens in the generated text. It is a value that is subtracted from the log-probability of a token each time it is generated. A higher presence_penalty value will result in the model being more likely to generate tokens that have not yet been included in the generated text.
    public float presencePenalty = 0.75f;
}
