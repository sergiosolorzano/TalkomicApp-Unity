//using AudioNoteTranscription.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioNoteTranscription.Whisper;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using UnityEngine;

namespace AudioNoteTranscription.Model
{
    public class TranscriptionModel
    {
        public TranscriptionModel() { }

        //Add await once is all hooked.
        public async Task<string> TranscribeAsync(string modelFilePath, string audioFilePath, bool useCloudInference)
        {
            if (string.IsNullOrEmpty(audioFilePath))
            {
                UnityEngine.Debug.Log("Audio File Missing");
                return String.Empty;
            }

            if (string.IsNullOrEmpty(modelFilePath))
            {
                UnityEngine.Debug.Log("Model File Missing");
                return String.Empty;
            }

            //UnityEngine.Debug.Log("About to run Transciption. AudioFilePath:" + audioFilePath);
            var result = await Task.Run(() =>
            {
                var config = new WhisperConfig();
                config.WhisperOnnxPath = modelFilePath;
                config.TestAudioPath = audioFilePath;

                var whisperResult = Whisper.Inference.Run(config, useCloudInference);
                return whisperResult;

            });

            //UnityEngine.Debug.Log("Ended running Transciption. Text:" + result);
            return result;
        }
    }
}
