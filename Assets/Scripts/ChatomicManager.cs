using AudioNoteTranscription.Whisper;
using StableDiffusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChatomicManager : MonoBehaviour
{
    public RunWhisper runWhisperScript;
    public RunChatgpt runChatgptScript;
    public RunDiffusion runDiffusionScript;
    public CreateByteChunks createByteChunksScript;
    public FitPaintingsInCanvas fitPaintingsInCanvasScript;

    public Button whisperTranscribeButton;
    public static TextMeshProUGUI userMessagingText;

    bool textureCompletedCallBackListenerOn = false;

    //path to podcast wav
    string pathToAudioFile = @"C:\AR-VR-Github\UnitySentisStableDiffusion-And-Whisper\Assets\Audio\sampleaudio.wav";
    
    Queue<KeyValuePair<int, Tuple<string, string>>> summaryAndTimesAudioQueue = new Queue<KeyValuePair<int, Tuple<string, string>>>();
    KeyValuePair<int, Tuple<string, string>> summaryAndTimesAudioQueueElement;

    Queue<KeyValuePair<int, Tuple<int, int, int, string>>> chunkTimeAndPathQueue = new Queue<KeyValuePair<int, Tuple<int, int, int, string>>>();
    
    //<int, Tuple<int, int, int, string>> <index, Tuple<audioChapter, chapterStartTime, chapterEndTime, initialChunkPath>>
    KeyValuePair<int, Tuple<int, int, int, string>> chunkTimeAndPathElement = new KeyValuePair<int, Tuple<int, int, int, string>>();

    [HideInInspector]
    public UnityEvent queueNextItemCallback;
    [HideInInspector]
    public UnityEvent chunkAudioCompleteCallback;
    [HideInInspector]
    public UnityEvent<string> whisperTranscribeCompleteResponseCallback;

    //directories
    string outputDir = "Output";
    string section_dir_name;
    string textFileName;
    string audioChunkDir = "Chunks";

    //whisper input text preprompt
    public static string custom_diffuser_pre_prompt;

    //chatgpt
    int maxChatgptRequestedResponseWords = 41;
    public static string custom_chatgpt_pre_prompt;
    int limitChatGPTResponseWordCount = 48;

    //stable diffusion
    int numStableDiffusionImages = 4;
    int currImage = 0;
    public static int steps =50; //default 50
    public static float ClassifierFreeGuidanceScaleValue = 7.5f;

    [SerializeField]
    public string paintingPrefabGOName;
    public GameObject[] paintingGOArray;

    void Awake()
    {
        //Debug.Log("Steps awake:" + steps);
        paintingPrefabGOName = "PaintingImagePrefab";
        LoadAndInstantiatePrefabs();

        userMessagingText = GameObject.FindWithTag("UserMessagesTextMeshPro").GetComponent<TextMeshProUGUI>();

        custom_chatgpt_pre_prompt = $"Provide a concise answer of no more than {maxChatgptRequestedResponseWords} words: Describe an abstract image for this conversation. Do not use names of people or persons. Do not say in the description it is an image of a person. The center or protagonist of the image you describe is not a person. Do not explain the image you describe, only describe the image. This is the conversation:";
        custom_diffuser_pre_prompt = "Paint Cyberpunk style.";

        //send transcribed audio to chatgpt
        whisperTranscribeCompleteResponseCallback.AddListener(SendTranscribedTextToChatGPT);
        //register when chatgpt completes to send painting description to run stable diffusion inference
        runChatgptScript.chatgptResponseCallback.AddListener(RunStableDiffusion);
        //register when chunks creation complete
        chunkAudioCompleteCallback.AddListener(RegisterChunkCompleteAudio);
        //items queue listener
        queueNextItemCallback.AddListener(DequeItem);
        //Create summary and times queue + chunks
        StartCoroutine(AllQueuesAndChunkCreationManager());
    }

    void LoadAndInstantiatePrefabs()
    {
        paintingGOArray = new GameObject[numStableDiffusionImages];
        GameObject prefab = Resources.Load<GameObject>("Prefabs/" + paintingPrefabGOName);
        var panelGO = GameObject.FindWithTag("ImagePanel");

        if (prefab != null)
        {
            for (int i = 0; i < numStableDiffusionImages; i++)
            {
                paintingGOArray[i] = Instantiate(prefab);
                paintingGOArray[i].transform.SetParent(panelGO.transform);
                //paintingGOArray[i].SetActive(false);
            }
        }
        else
        {
            Debug.LogError("Prefab not found in Resources/Prefabs folder.");
        }

        fitPaintingsInCanvasScript.AdjustGridLayout(numStableDiffusionImages, panelGO);
        fitPaintingsInCanvasScript.ScaleObjectToFitCanvas(paintingGOArray, numStableDiffusionImages);
    }

    IEnumerator AllQueuesAndChunkCreationManager()
    {
        userMessagingText.text = "Creating Directories and Text Output Files.";
        //create list of summary with audio times and the output directories
        IEnumerator genSummaryAndTimesAndDirs =  GenerateSummaryAndTimesAudioQueueAndDirectories();
        yield return (genSummaryAndTimesAndDirs);
        //create audio chunks
        IEnumerator createChunks =CreateChunks();
        yield return (createChunks);
        //import files to database
        //Debug.Log("Setting Importing Audio chunks");
        userMessagingText.text = "Importing Audio Chunks, Refreshing Database.";
        yield return new WaitForSeconds(0.25f);
#if UNITY_EDITOR
        AssetDatabase.ImportAsset($"Assets/{outputDir}/{audioChunkDir}");
        UnityEditor.AssetDatabase.Refresh();
#endif
        //load audio and inference models
        LoadAssets();
        //lazy hack before I implement models loaded callback
        userMessagingText.text = "Press Button To Start.";
        whisperTranscribeButton.gameObject.SetActive(true);
    }

    IEnumerator CreateChunks()
    {
        userMessagingText.text = "Chunking Audio File.";
        //create AudioChunkDir
        IEnumerator createDir= CreateDir($"Assets/{outputDir}/{audioChunkDir}");
        yield return (createDir);
        //create list with timespans and chunk path
        IEnumerator genChunks = GenerateChunkTimeAndFilePath();
        yield return (genChunks);
        //chunk audio and store

        while(chunkTimeAndPathQueue.Count>0)
        {
            var e = chunkTimeAndPathQueue.Dequeue();
            IEnumerator createChunk = createByteChunksScript.ReadAudioChunk(pathToAudioFile, e.Value.Item2, e.Value.Item3, e.Value.Item4, e.Key);
            yield return (createChunk);
        }

        //release queue memory
        chunkTimeAndPathQueue = null;

        chunkAudioCompleteCallback.Invoke();
    }

    IEnumerator GenerateChunkTimeAndFilePath()
    {
        var summaryAndTimesAudioList = summaryAndTimesAudioQueue.ToList();

        for (int i = 0; i < summaryAndTimesAudioList.Count; i++)
        {
            string filenameHead = string.Join("_", summaryAndTimesAudioList[i].Key.ToString(), string.Join("_", summaryAndTimesAudioList[i].Value.Item1.Split(' ').Take(3)));
            string chunkPath = $"Assets/{outputDir}/{audioChunkDir}/{filenameHead}";
            int startTimeSeconds = 0;
            int endTimeSeconds = 0;

            startTimeSeconds = ConvertTimeSpanToSeconds(summaryAndTimesAudioList[i].Value.Item2);

            if (i < summaryAndTimesAudioList.Count-1)
            {
                endTimeSeconds = ConvertTimeSpanToSeconds(summaryAndTimesAudioList[i + 1].Value.Item2);
            }
            else
                endTimeSeconds = -1;

            chunkTimeAndPathElement = new KeyValuePair<int, Tuple<int, int, int, string>>(i, new Tuple<int, int, int, string>(summaryAndTimesAudioList[i].Key, startTimeSeconds, endTimeSeconds, chunkPath));
            chunkTimeAndPathQueue.Enqueue(chunkTimeAndPathElement);
            Debug.Log("Created chunk:" + chunkTimeAndPathElement.Value.Item1 + " StartTime:" + chunkTimeAndPathElement.Value.Item2 + " EndTime:" + chunkTimeAndPathElement.Value.Item3 + " AudioChunkPath:" + chunkTimeAndPathElement.Value.Item4);
        }
        yield return null;
    }

    private int ConvertTimeSpanToSeconds(string timeString)
    {
        string[] startParts = timeString.Split(":");

        if (startParts.Length != 2)
        {
            throw new Exception("StartTimeString invalid");
        }

        int minutes = int.Parse(startParts[0]);
        int seconds = int.Parse(startParts[1]);

        return (minutes * 60) + seconds;
    }

    public void LoadAssets()
    {
        //load Inference Models
        runDiffusionScript.LoadModels();
    }

    public void StartChatomic()
    {
        whisperTranscribeButton.interactable = false;

        Debug.Log("Sections Size:"+summaryAndTimesAudioQueue.Count);

        //Load full audio and play
        runWhisperScript.LoadAudio(pathToAudioFile);

        //enable showing text on screen for transcription and chatgpt response
        runWhisperScript.transcribedText.gameObject.SetActive(true);

        if (summaryAndTimesAudioQueue.Count > 0)
            DequeItem();
    }

    private async void DequeItem()
    {
        currImage = 0;
        runWhisperScript.transcribedText.text = "";

        if (summaryAndTimesAudioQueue.Count > 0)
        {
            summaryAndTimesAudioQueueElement = summaryAndTimesAudioQueue.Dequeue();

            Debug.Log("Deque new Item:" + summaryAndTimesAudioQueueElement.Value.Item1 + " currImage " + currImage);

            var joinedChunksStringBuilder = new StringBuilder();
            int thisSection = summaryAndTimesAudioQueueElement.Key;
            section_dir_name = string.Join("_", summaryAndTimesAudioQueueElement.Key.ToString(), string.Join("_", summaryAndTimesAudioQueueElement.Value.Item1.Split(' ').Take(3)));
            
            Debug.Log("I've set section_dir_name to " + section_dir_name);
            foreach (var path in createByteChunksScript.chunkPathsPerSectionDict[thisSection])
            {
                Debug.Log("Process Chunk Section:" + thisSection + "_" + summaryAndTimesAudioQueueElement.Value.Item1 + " audioPath " + path);
                joinedChunksStringBuilder.Append(await runWhisperScript.OnAudioClipLoadedTranscribe(path));
            }

            //send Event notice to ChatomicManager with transcribed Text
            string result = joinedChunksStringBuilder.ToString();

            runWhisperScript.transcribedText.text += "<b>Sending transcribed text to chatgpt</b>: " + result;
            await Task.Delay(250);

            //Debug.Log($"Whisper StringBuilder Complete - Section {thisSection} - send transcribed text to Manager:" + result);
            textFileName = section_dir_name + ".txt";
            Debug.Log("Calling Transcribe callback - at section_dir_name: " + section_dir_name + " and textfilename:" + textFileName);
            whisperTranscribeCompleteResponseCallback.Invoke(result);

            //show text on screen
            //runWhisperScript.transcribedText.text = result;

            AssetDatabase.Refresh();
        }
    }

    void RegisterChunkCompleteAudio()
    {
        Debug.Log("Chunk creation Complete.");
    }

    public void SendTranscribedTextToChatGPT(string transcribedText)
    {
        userMessagingText.text = "Writing Transcribed Result To Text File.";

        //write transcribedText To file
        string transcribedTextFilePath = $"{Application.dataPath}/{outputDir}/{section_dir_name}/{textFileName}";

        string AudioSectionEndTime;
        if (summaryAndTimesAudioQueue.Count == 0)
            AudioSectionEndTime = ConvertAudioSecondsFormat(runWhisperScript.audioClipTotalLength);
        else
        {
            var nextElement = summaryAndTimesAudioQueue.Peek();
            AudioSectionEndTime = nextElement.Value.Item2;
        }

        string audioTranscribedToInsertInFile = $"Audio Transcribed: Section {summaryAndTimesAudioQueueElement.Key}-{summaryAndTimesAudioQueueElement.Value.Item1}-Start Time: {summaryAndTimesAudioQueueElement.Value.Item2}-EndTime: {AudioSectionEndTime}: " + transcribedText;
        WriteToFile(audioTranscribedToInsertInFile, transcribedTextFilePath, false);

        //received transcribedText
        whisperTranscribeButton.gameObject.SetActive(false);

        runChatgptScript.SendTranscribedTextToChatGPT(transcribedText);
    }

    public async void RunStableDiffusion(string paintingDescription)
    {
        //Debug.Log("Manager receives chatgpt painting description" + paintingDescription);

        //write transcribedText To file
        string paintingDescriptionTextFilePath = $"{Application.dataPath}/{outputDir}/{section_dir_name}/{textFileName}";
        string paintingDescriptionToInsertInFile = "ChatGPT Painting Description (Response):" + paintingDescription;
        WriteToFile(paintingDescriptionToInsertInFile, paintingDescriptionTextFilePath, true);

        //add texture completed listener if not in place
        //todo at awake when models have loaded
        if (!textureCompletedCallBackListenerOn)
        {
            Unet.TextureCompletedCallback.AddListener(StableDiffusionEnded);
            textureCompletedCallBackListenerOn = true;
        }

        //set all tex to black as a new loop of images is created
        foreach (var img in paintingGOArray)
        {
            img.GetComponent<Image>().color = Color.black;
        }

        for (int i = 0;i<numStableDiffusionImages;i++)
        {
            int thisImageNoText = i + 1;
            userMessagingText.text = $"Creating {numStableDiffusionImages} Images for Section {summaryAndTimesAudioQueueElement.Value.Item1}\nSave Text and Run Stable Diffusion To Create Image " + thisImageNoText;
            await Task.Delay(250);

            await runDiffusionScript.StartDiffusionProcess(paintingDescription, limitChatGPTResponseWordCount);
            Debug.Log("Ended Painting " + i);

            //clear Stable Diffusion data
            Debug.Log("Clear Diffusion data for painting " + i);
            await StableDiffusion.Main.ClearUncondInput_and_TextEmbeddings();
        }

        queueNextItemCallback.Invoke();
    }

    void StableDiffusionEnded(Texture2D texture2D)
    {
        string imageFullPath = "";
        
        foreach (var g in paintingGOArray)
        {
            if (g.activeSelf == false)
                g.SetActive(true);
        }

        //show image
        Debug.Log("Shwoing image " + currImage);
        paintingGOArray[currImage].SetActive(true);
        float pixelsPerUnit = 100.0f;
        Rect rect = new Rect(0, 0, texture2D.width, texture2D.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        Sprite newSprite = Sprite.Create(texture2D, rect, pivot, pixelsPerUnit);
        paintingGOArray[currImage].GetComponent<Image>().color = Color.white;
        paintingGOArray[currImage].GetComponent<Image>().sprite = newSprite;

        //save painting image to file
        imageFullPath = $"Assets/{outputDir}/{section_dir_name}/{section_dir_name}_{currImage}.png";

        SaveImageToDisk(texture2D, imageFullPath);
        currImage+=1;
    }

    private static void WriteToFile(string text, string filePath, bool addLine)
    {
        Debug.Log("****At WriteToFile Filepath" + filePath);
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            if (addLine)
            {
                writer.WriteLine();
                writer.WriteLine();
            }
            writer.WriteLine(text);
        }
    }

    void SaveImageToDisk(Texture2D texture2D, string image_path)
    {
        Debug.Log("Saving Image " +  image_path);
        byte[] pngBytes = texture2D.EncodeToPNG();

         System.IO.File.WriteAllBytes(image_path, pngBytes);

#if UNITY_EDITOR
        AssetDatabase.ImportAsset(image_path);
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    private IEnumerator CreateDir(string folderPath)
    {
        string[] folders = folderPath.Split('/');

        string currentPath = "";
        for (int i = 0; i < folders.Length; i++)
        {
            currentPath = Path.Combine(currentPath, folders[i]);
            if (!AssetDatabase.IsValidFolder(currentPath))
            {
                string parentFolder = Path.GetDirectoryName(currentPath);
                string newFolderName = Path.GetFileName(currentPath);
                AssetDatabase.CreateFolder(parentFolder, newFolderName);
#if UNITY_EDITOR
                AssetDatabase.ImportAsset(newFolderName);
                AssetDatabase.Refresh();
#endif
            }
        }

        yield return null;
    }

    private void CreateOutputTextFile(string filePath)
    {
        System.IO.File.WriteAllText(filePath, string.Empty);
#if UNITY_EDITOR
        AssetDatabase.ImportAsset(filePath);
        AssetDatabase.Refresh();
#endif
    }

    IEnumerator GenerateSummaryAndTimesAudioQueueAndDirectories()
    {
        int i = 0;

        //Add each section with start time. Each section will be generated as a directory in Assets/Output. Example below:
        var element = new KeyValuePair<int, Tuple<string, string>>(i++, new Tuple<string, string>("Section Example", "0:00"));
        summaryAndTimesAudioQueue.Enqueue(element);

        //Example TechShift F9 episode 9
        /*var element = new KeyValuePair<int, Tuple<string, string>>(i++, new Tuple<string, string>("Introduction", "0:00"));
        summaryAndTimesAudioQueue.Enqueue(element);
        element = new KeyValuePair<int, Tuple<string, string>>(i++, new Tuple<string, string>("This week’s guest", "0:32"));
        summaryAndTimesAudioQueue.Enqueue(element);
        element = new KeyValuePair<int, Tuple<string, string>>(i++, new Tuple<string, string>("Start of the interview", "1:13"));
        summaryAndTimesAudioQueue.Enqueue(element);
        element = new KeyValuePair<int, Tuple<string, string>>(i++, new Tuple<string, string>("How Chaya began her writing career", "1:34"));
        summaryAndTimesAudioQueue.Enqueue(element);
        element = new KeyValuePair<int, Tuple<string, string>>(i++, new Tuple<string, string>("Mental approach to starting a whole new career", "3:02"));
        summaryAndTimesAudioQueue.Enqueue(element);
        element = new KeyValuePair<int, Tuple<string, string>>(i++, new Tuple<string, string>("Was writing always something you wanted to do?", "4:47"));
        summaryAndTimesAudioQueue.Enqueue(element);
        element = new KeyValuePair<int, Tuple<string, string>>(i++, new Tuple<string, string>("How technology is impacting Chaya’s writing career", "6:30"));
        summaryAndTimesAudioQueue.Enqueue(element);
        element = new KeyValuePair<int, Tuple<string, string>>(i++, new Tuple<string, string>("Discussion on Artificial Intelligence and the tools used by Chaya", "8:50"));
        summaryAndTimesAudioQueue.Enqueue(element);
        element = new KeyValuePair<int, Tuple<string, string>>(i++, new Tuple<string, string>("Best media for books", "14:30"));
        summaryAndTimesAudioQueue.Enqueue(element);
        element = new KeyValuePair<int, Tuple<string, string>>(i++, new Tuple<string, string>("The entrepreneurial aspects of being an author", "15:45"));
        summaryAndTimesAudioQueue.Enqueue(element);
        element = new KeyValuePair<int, Tuple<string, string>>(i++, new Tuple<string, string>("Experiencing a new industry after Chaya’s career pivot", "17:53"));
        summaryAndTimesAudioQueue.Enqueue(element);
        element = new KeyValuePair<int, Tuple<string, string>>(i++, new Tuple<string, string>("Chaya’s advice to people looking at a career change", "18:45"));
        summaryAndTimesAudioQueue.Enqueue(element);
        element = new KeyValuePair<int, Tuple<string, string>>(i++, new Tuple<string, string>("Chaya’s future plans", "19:54"));
        summaryAndTimesAudioQueue.Enqueue(element);
        element = new KeyValuePair<int, Tuple<string, string>>(i++, new Tuple<string, string>("Outro", "21:11"));*/

        IEnumerator genDirsAndTextFiles = GenerateOutputDirectoriesAndTextFiles();
        yield return genDirsAndTextFiles;
    }

    IEnumerator GenerateOutputDirectoriesAndTextFiles()
    {
        //create output dir
        IEnumerator createDirCo = CreateDir($"Assets/{outputDir}");
        yield return (createDirCo);

        //create sections dirs and files
        foreach (var e in summaryAndTimesAudioQueue)
        {
            section_dir_name = string.Join("_", e.Key.ToString(), string.Join("_", e.Value.Item1.Split(' ').Take(3)));
            createDirCo = CreateDir($"Assets/{outputDir}/{section_dir_name}");
            yield return (createDirCo);

            textFileName = section_dir_name + ".txt";
            CreateOutputTextFile($"Assets/{outputDir}/{section_dir_name}/{textFileName}");
        }
    }

    void OnDisable()
    {
        if(whisperTranscribeCompleteResponseCallback!=null)
            whisperTranscribeCompleteResponseCallback.RemoveListener(SendTranscribedTextToChatGPT);
        if(runChatgptScript.chatgptResponseCallback!=null)
        runChatgptScript.chatgptResponseCallback.RemoveListener(RunStableDiffusion);
        if (Unet.TextureCompletedCallback!=null)
            Unet.TextureCompletedCallback.RemoveListener(StableDiffusionEnded);
        if(queueNextItemCallback!=null)
            queueNextItemCallback.RemoveListener(DequeItem);
        chunkAudioCompleteCallback.RemoveListener(RegisterChunkCompleteAudio);
    }

    private string ConvertAudioSecondsFormat(float totalSeconds)
    {
        int minutes = (int)totalSeconds / 60;
        int seconds = (int)totalSeconds % 60;

        return string.Format("{0}:{1:D2}", minutes, seconds);
    }
}
