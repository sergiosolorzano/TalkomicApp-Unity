using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class InputsIntoScriptObject : MonoBehaviour
{
    //scriptable object for credentials
    public ChatgptCreds AzureChatgptCredentials;

    //button to close inputs box
    public Button closeInputsBoxButton;

    //inputs GO
    public GameObject chatGPTApiInputsGO;

    public TMP_InputField deploymentIdField;

    public TMP_InputField modelField;

    public TMP_InputField secretField;

    public TMP_InputField urlField;

    public TMP_InputField apiversionField;

    public TMP_InputField systemContentField;

    public TMP_InputField temperatureField;

    public TMP_InputField maxTokensField;

    public TMP_InputField frequencyPenaltyField;

    public TMP_InputField presencePenaltyField;

    void Start()
    {
        // Get data from Input Field
        AzureChatgptCredentials.deployment_id = deploymentIdField.text;
        AzureChatgptCredentials.model = modelField.text;
        AzureChatgptCredentials.secret = secretField.text;
        AzureChatgptCredentials.url = urlField.text;
        AzureChatgptCredentials.api_version = apiversionField.text;
        AzureChatgptCredentials.systemContent = systemContentField.text;
        AzureChatgptCredentials.temperature = float.Parse(temperatureField.text);
        AzureChatgptCredentials.maxTokens = Int32.Parse(maxTokensField.text);
        AzureChatgptCredentials.frequencyPenalty = float.Parse(frequencyPenaltyField.text);
        AzureChatgptCredentials.presencePenalty = float.Parse(presencePenaltyField.text);

        //add listeners
        deploymentIdField.onValueChanged.AddListener(delegate { ValueChangeUpdate(); });
        modelField.onValueChanged.AddListener(delegate { ValueChangeUpdate(); });
        secretField.onValueChanged.AddListener(delegate { ValueChangeUpdate(); });
        urlField.onValueChanged.AddListener(delegate { ValueChangeUpdate(); });
        apiversionField.onValueChanged.AddListener(delegate { ValueChangeUpdate(); });
        systemContentField.onValueChanged.AddListener(delegate { ValueChangeUpdate(); });
        temperatureField.onValueChanged.AddListener(delegate { ValueChangeUpdate(); });
        maxTokensField.onValueChanged.AddListener(delegate { ValueChangeUpdate(); });
        frequencyPenaltyField.onValueChanged.AddListener(delegate { ValueChangeUpdate(); });
        presencePenaltyField.onValueChanged.AddListener(delegate { ValueChangeUpdate(); });
    }

    public void ValueChangeUpdate()
    {
        // Get data from Input Field
        AzureChatgptCredentials.deployment_id = deploymentIdField.text;
        AzureChatgptCredentials.model = modelField.text;
        AzureChatgptCredentials.secret = secretField.text;
        AzureChatgptCredentials.url = urlField.text;
        AzureChatgptCredentials.api_version = apiversionField.text;
        AzureChatgptCredentials.systemContent = systemContentField.text;
        AzureChatgptCredentials.temperature = float.Parse(temperatureField.text);
        AzureChatgptCredentials.maxTokens = Int32.Parse(maxTokensField.text);
        AzureChatgptCredentials.frequencyPenalty = float.Parse(frequencyPenaltyField.text);
        AzureChatgptCredentials.presencePenalty = float.Parse(presencePenaltyField.text);
    }

    public void CloseChatGptCredentialsInputBox()
    {
        chatGPTApiInputsGO.SetActive(false);
        //Debug.Log("AzureChatgptCredentials:" + AzureChatgptCredentials);
    }
}
