using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NAudio;
using NAudio.Wave;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using UnityEditor.Search;

public class CreateByteChunks : MonoBehaviour
{
    float paddingInSeconds = 0.5f;  // This is just an example, adjust as needed
    int absoluteChunkLength = 30;

    public Dictionary<int, List<string>> chunkPathsPerSectionDict = new Dictionary<int, List<string>>();

    public int GetChunkLengthInSeconds()
    {
        return absoluteChunkLength - 2*(int)Math.Ceiling(paddingInSeconds);
    }

    public IEnumerator ReadAudioChunk(string audioFilePath, int startTimeInSeconds, int endTimeInSeconds, string chunkBasePath, int section)
    {
        int chunkLengthInSeconds = GetChunkLengthInSeconds();

        using (var reader = new AudioFileReader(audioFilePath))
        {
            //add padding to chunks
            int paddingBytes = (int)(paddingInSeconds * reader.WaveFormat.AverageBytesPerSecond);

            // Calculate the byte position for the start and end time
            System.Int64 startByte;
            System.Int64 endByte;
            if (startTimeInSeconds == 0)
            {
                startByte = (long)(reader.WaveFormat.AverageBytesPerSecond * startTimeInSeconds);
                endByte = (endTimeInSeconds < 0) ? reader.Length : (long)(reader.WaveFormat.AverageBytesPerSecond * endTimeInSeconds);
            }
            else
            {
                startByte = (long)(reader.WaveFormat.AverageBytesPerSecond * startTimeInSeconds) - paddingBytes;
                endByte = (endTimeInSeconds < 0) ? reader.Length : (long)(reader.WaveFormat.AverageBytesPerSecond * endTimeInSeconds) + paddingBytes;
            }
            
            // Ensure the reader's position starts at the startByte
            reader.Position = startByte;

            var bytesPerChunk = reader.WaveFormat.AverageBytesPerSecond * chunkLengthInSeconds + 2 * paddingBytes;
            var buffer = new byte[bytesPerChunk];
            int bytesRead;
            int chunNum = 0;
            string chunkPath;

            List<string> chunkPathList = new List<string>();

            while (reader.Position < endByte)
            {
                chunkPath = chunkBasePath + "_" + chunNum + ".wav";

                TalkomicManager.userMessagingText.text = "Generate Audio Chunk\n" + chunkPath;

                // Check how many bytes are left
                long remainingBytes = endByte - reader.Position;

                if(remainingBytes < bytesPerChunk)
                {
                    bytesPerChunk = (int)remainingBytes;
                    buffer = new byte[bytesPerChunk];
                }

                bytesRead = reader.Read(buffer, 0, bytesPerChunk);

                if (bytesRead == 0)
                    break;

                chunkPathList.Add(chunkPath);

                using (var writer = new WaveFileWriter(chunkPath, reader.WaveFormat))
                {
                    writer.Write(buffer, 0, bytesRead);
                }
                chunNum++;
            }

            //add chunk paths to list
            chunkPathsPerSectionDict.Add(section,chunkPathList);

            yield return null;
        }
    }
}
