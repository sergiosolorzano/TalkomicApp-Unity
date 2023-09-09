using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputsIntoScriptObject : MonoBehaviour
{
    public TMP_InputField deploymentIdField;
    private string deploymentIdData;

    public TMP_InputField modelField;
    private string modelData;

    public TMP_InputField secretField;
    private string secretData;

    public TMP_InputField urlField;
    private string urlData;

    public TMP_InputField apiversionField;
    private string apiversionData;

    public TMP_InputField systemContentField;
    private string systemContentData;

    public TMP_InputField temperatureField;
    private string temperatureData;

    public TMP_InputField maxTokensField;
    private string maxTokensData;

    public TMP_InputField frequencyPenaltyField;
    private string frequencyPenaltyData;

    public TMP_InputField presencePenaltyField;
    private string presencePenaltyData;

    void Start()
    {
        // Get data from Input Field
        deploymentIdData = deploymentIdField.text;
        modelData = modelField.text;
        secretData = secretField.text;
        urlData = urlField.text;
        apiversionData = apiversionField.text;
        systemContentData = systemContentField.text;
        temperatureData = temperatureField.text;
        maxTokensData = maxTokensField.text;
        frequencyPenaltyData = frequencyPenaltyField.text;
        presencePenaltyData = presencePenaltyField.text;

        //add listeners
        deploymentIdField.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        modelField.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        secretField.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        urlField.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        apiversionField.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        systemContentField.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        temperatureField.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        maxTokensField.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        frequencyPenaltyField.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        presencePenaltyField.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
    }

    public void ValueChangeCheck()
    {
        // Get data from Input Field
        deploymentIdData = deploymentIdField.text;
        modelData = modelField.text;
        secretData = secretField.text;
        urlData = urlField.text;
        apiversionData = apiversionField.text;
        systemContentData = systemContentField.text;
        temperatureData = temperatureField.text;
        maxTokensData = maxTokensField.text;
        frequencyPenaltyData = frequencyPenaltyField.text;
        presencePenaltyData = presencePenaltyField.text;


        //Debug.Log("Value changed: " + deploymentIdField.text);
    }
}
