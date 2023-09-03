using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Events;
using static RunChatgpt;

namespace AudioNoteTranscription.Whisper
{
    public class RunWhisper : MonoBehaviour
    {
        public TextMeshProUGUI transcribedText;
        private AudioSource audioSource;
        public RunChatgpt runChatgptScript;

        //string WhisperOnnxFilePath = Application.streamingAssetsPath + "/whisper/" + "model" + ".onnx";
        string WhisperOnnxFilePath = "";

        [HideInInspector]
        public float audioClipTotalLength = 0;

        public void Awake()
        {
            WhisperOnnxFilePath = Application.dataPath + $"/Models/whisper/model.onnx";
            audioSource = GetComponent<AudioSource>();
        }

        //Load Audio
        public void LoadAudio(string audioFilePath)
        {
            StartCoroutine(LoadAudioClipFromPathAndWait(audioFilePath));
        }

        IEnumerator LoadAudioClipFromPathAndWait(string audioPathFile)
        {
            bool isLoaded = false;

            TalkomicManager.userMessagingText.text = "Loading Full Audio";

            StartCoroutine(LoadAudioClipFromPath(audioPathFile, () => isLoaded = true));

            yield return new WaitUntil(() => isLoaded);
            
            Debug.Log("AudioClip finished loading.");
        }

        IEnumerator LoadAudioClipFromPath(string path, Action onComplete)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.UNKNOWN))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError("Error loading audio file: " + www.error);
                }
                else
                {
                    AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                    audioSource.clip = audioClip;
                    audioClipTotalLength = audioClip.length;
                    //audioSource.Play();
                    onComplete?.Invoke();
                    Debug.Log("Invoking Audio Oncomplete");
                }
            }
            //onComplete?.Invoke();
        }

        public async Task<string> OnAudioClipLoadedTranscribe(string audioPath)
        {
            TalkomicManager.userMessagingText.text = "Transcribing Audio Chunk:" + audioPath;
            await Task.Delay(250);
            string result = await Inference.Start(WhisperOnnxFilePath, audioPath);

            return result;
        }
    }
}
