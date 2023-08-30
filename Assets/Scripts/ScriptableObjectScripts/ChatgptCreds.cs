using UnityEngine;

[CreateAssetMenu(fileName = "Azure_Chatgpt_Credentials", menuName = "ChatGPT_Credentials/Azure_API")]
public class ChatgptCreds: ScriptableObject
{
    //Azure openai deployment id
    public string deployment_id = "";
    //Azure openai model name
    public string model = "";
    //Azure openai secret
    public string secret = "";
    //Azure chatgpt endpoint
    public string url = "";
    //azure openai api version
    public string api_version = "";
}
