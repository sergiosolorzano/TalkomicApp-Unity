using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class RunChatgpt : MonoBehaviour
{
    public TextMeshProUGUI TranscriptionAndchatgpt_response;
    [HideInInspector]
    public UnityEvent<string> chatgptResponseCallback;
    public ChatgptCreds AzureChatgptCredentials;

    string gpt_prompt;

    #region Classes
    [Serializable]
    public class ChatResponse
    {
        public string id;
        public string model;
        public ChatOption[] choices;
    }
    [Serializable]
    public class ChatOption
    {
        public ChatMessage message;
        public int index;
        public string finish_reason;
    }
    [Serializable]
    public class ChatMessage
    {
        public string role;
        public string content;
    }
    #endregion

    public void SendTranscribedTextToChatGPT(string transcribedText)
    {
        gpt_prompt = ChatomicManager.custom_chatgpt_pre_prompt + transcribedText;
        GetPaintingDescription(gpt_prompt, chatgptResponseCallback);
    }

    public void GetPaintingDescription(string input, UnityEvent<string> callback)
    {
        //ChatomicManager.userMessagingText.text = "Asking ChatGPT for an Image Description Of The Audio Text:\n"+ chatgpt_prompt.text;
        StartCoroutine(GetPaintingDescriptionCoroutine(input, callback));
    }

    private IEnumerator GetPaintingDescriptionCoroutine(string input, UnityEvent<string> chatgptResponseCallback)
    {
        string submitText = cleanForJSON(input);
        Debug.Log("Chatgpt clean prompt:" + submitText);
        string deployment_id = AzureChatgptCredentials.deployment_id;
        string model = AzureChatgptCredentials.model;
        string secret = AzureChatgptCredentials.secret;

        string url = $"{AzureChatgptCredentials.url}/{deployment_id}/chat/completions?api-version={AzureChatgptCredentials.api_version}";
        string json = "{" +
            "\"messages\":[{\"role\": \"system\", \"content\": \"" + "You are a helpful assistant." + "\"}," +
            "{" + "\"role\": \"user\", \"content\": \"" + submitText + "\"}]," +
            "\"temperature\": 0.7," +
            "\"max_tokens\": 4096," +
            //"\"top_p\": 1," +
            "\"frequency_penalty\": 0.75," +  //This parameter is used to discourage the model from repeating the same words or phrases too frequently within the generated text. It is a value that is added to the log-probability of a token each time it occurs in the generated text. A higher frequency_penalty value will result in the model being more conservative in its use of repeated tokens.
            "\"presence_penalty\": 0.75," + //This parameter is used to encourage the model to include a diverse range of tokens in the generated text. It is a value that is subtracted from the log-probability of a token each time it is generated. A higher presence_penalty value will result in the model being more likely to generate tokens that have not yet been included in the generated text.
            "\"model\": \"" + model + "\"" +
            "}";
        //Debug.Log("url:" + url);
        //Debug.Log("body:"+json);

        using (UnityWebRequest request = new UnityWebRequest(url))
        {
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.SetRequestHeader("Content-Type", "application/json");
            //request.SetRequestHeader("Authorization", "Bearer " + secret);
            request.SetRequestHeader("api-key", secret);
            request.disposeUploadHandlerOnDispose = true;
            request.disposeDownloadHandlerOnDispose = true;
            yield return request.SendWebRequest();


            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Chatgpt Request Error: " + request.error);
            }
            else
            {
                ChatResponse chatResponse = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);

                //Send event notice to ChatomicManager with chatgpt response
                //Debug.Log("Chatgpt Painting Description Received:" + chatResponse.choices[0].message.content);
                IEnumerator showResponse = gpt_response_text_ienum(chatResponse.choices[0].message.content);
                yield return showResponse;
                
                chatgptResponseCallback.Invoke(chatResponse.choices[0].message.content);
            }
        }

        yield return null;
    }

    IEnumerator gpt_response_text_ienum(string gptResponse)
    {
        Debug.Log("ChatGPT Response - Painting Description: " + gptResponse);
        
        TranscriptionAndchatgpt_response.text += "\n\n<b>Chatgpt Painting Description</b>: " + gptResponse;
        
        yield return new WaitForSeconds(0.25f);
    }

    public static string cleanForJSON(string s)
    {
        if (s == null || s.Length == 0)
        {
            return "";
        }


        char c = '\0';
        int i;
        int len = s.Length;
        StringBuilder sb = new StringBuilder(len + 4);
        String t;


        for (i = 0; i < len; i += 1)
        {
            c = s[i];
            switch (c)
            {
                case '\\':
                case '"':
                    sb.Append('\\');
                    sb.Append(c);
                    break;
                case '/':
                    sb.Append('\\');
                    sb.Append(c);
                    break;
                case '\b':
                    sb.Append("\\b");
                    break;
                case '\t':
                    sb.Append("\\t");
                    break;
                case '\n':
                    sb.Append("\\n");
                    break;
                case '\f':
                    sb.Append("\\f");
                    break;
                case '\r':
                    sb.Append("\\r");
                    break;
                default:
                    if (c < ' ')
                    {
                        t = "000" + String.Format("X", c);
                        sb.Append("\\u" + t.Substring(t.Length - 4));
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;
            }
        }
        return sb.ToString();
    }
}