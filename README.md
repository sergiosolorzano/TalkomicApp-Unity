## Project
Watching a sound wave or unrelated images for a podcast that is published in a platform that supports video and images can be dull.

We propose asking a trio of AI models, bundled in a [Unity](https://unity.com/) built app named Chatomic, that transcribe the audio to text and generate contexual images closely tied to the transcribed text.

![Compare-Podcast](https://github.com/sergiosolorzano/ChatomicApp-Unity/assets/24430655/0c19dd3a-9dd4-42a8-a59d-4838f2de49ba)

The AI models in the Unity project of this repo are powered by Microsoft's cross-platform [OnnxRuntime](https://onnxruntime.ai/).

We run two of the AI models locally, [Whisper-Tiny](https://huggingface.co/openai/whisper-tiny) and [stable diffusion in U-Net architecture](https://huggingface.co/CompVis/stable-diffusion-v1-4/tree/onnx), and access a third, [Chatgpt](https://learn.microsoft.com/en-us/azure/ai-services/openai/chatgpt-quickstart?tabs=command-line&pivots=programming-language-csharp), remotely via an API.

<img width="1448" alt="diagram-flow" src="https://github.com/sergiosolorzano/ChatomicApp-Unity/assets/24430655/e7e43d4b-1def-4324-8937-966ef7899f0c">

In a Unity scene we loop the AI Models over each podcast audio section to generate the contextual images.



Finally, we enhance with the Real-ESRGAN AI Model the details of the 512×512 images generated by the stable diffusion AI model to crisper 2048×2048 resolutions. Suggested implementation steps in our [blog](https://tapgaze.com/blog/podcast-to-image-slider/#real-esrgan).

## Proof of Concept: Chatomic - "<i>Chat</i> and Create a <i>Comic</i>"
I am thrilled and truly grateful to Maurizio Raffone at [Tech Shift F9 Podcast](https://linktr.ee/techshiftf9) for trusting me to run a proof of concept of the Chatomic app with the audio file of a fantastic episode in this podcast.

See the Tech Shift F9 "Chatomized" Trailer:

  ---ADD TRAILER HERE----
  <video src="https://user-images.githubusercontent.com/24430655/176890215-a7bb0a66-8046-4785-87d0-34494c17385b.mp4" controls="controls" muted="muted" playsinline="playsinline">
      </video>

Watch The Complete [<i>Chatomized</i> TechShift-F9 Podcast Episode here.](https://youtu.be/pWK4vFLD6_E)

View and download the [Podcast's AI Image Gallery](https://tapgaze.com/blog/techshift-f9-chatomic-images/#podcast-gallery)

Available In Augmented Reality: See the Podcasts' AI Images in AR with the [Tapgaze app](https://apps.apple.com/gb/app/tapgaze/id1534427791)

## Project Blog
Read [the Chatomic app blog](https://tapgaze.com/blog/podcast-to-image-slider/) for the suggested steps to build the project in Unity:
  + convert to Onnx using Olive the [whisper-tiny text-transcription AI model](https://tapgaze.com/blog/podcast-to-image-slider/#whisper-olive)
  + processing [chunked podcast audio](https://tapgaze.com/blog/podcast-to-image-slider/#whisper-chunks) for whisper
  + [chatgpt API Unitywebrequest](https://tapgaze.com/blog/podcast-to-image-slider/#chatgpt)
  + discussion on the implementation of the [stable diffusion model](https://tapgaze.com/blog/podcast-to-image-slider/#stable-diffusion) in Unity
  + get crisper images with [Real-ESRGAN AI model](https://tapgaze.com/blog/podcast-to-image-slider/#real-esrgan)
  + links to technical articles and much more

## Unity Project Features

* Podcast Audio Section List Required: Create in script ChatomicManager.cs coroutine GenerateSummaryAndTimesAudioQueueAndDirectories() a list for each section in the podcast audio with the section_name and its start time in minutes:seconds. See sample in the code.

  + Unity will generate an output directory for each section, save the transcribed text and chatgpt image description for each section, and the images generated.

---HERE OUTPUT-SNAPSHOT.PNG IMAGE---
  <img width="246" alt="Dicom-Images-Processed-Progress" src="https://user-images.githubusercontent.com/24430655/176902397-e3ed3745-2ba0-4c39-95e3-bd66d9aa6ad3.PNG">

* Podcast Audio Chunks: The Whisper model is designed to work on audio samples of up to 30s in duration. Hence [we chunk the podcast audio](https://tapgaze.com/blog/podcast-to-image-slider/#whisper-chunks) for each section in chunks of max 30 seconds but load these as a queue in Whisper-tiny for each podcast section.

* AI Generated Images: Shown in the scene along with the transcribed text and chatgpt image description:

---HERE SCENE-PROGRESS.PNG IMAGE---
  <img width="246" alt="Dicom-Images-Processed-Progress" src="https://user-images.githubusercontent.com/24430655/176902397-e3ed3745-2ba0-4c39-95e3-bd66d9aa6ad3.PNG">

* Scene Control Input Variables:
  * Script: ChatomicManager.cs:

    + pathToAudioFile: full path to podcast audio file. Audio file is in sync with the list of section names and start times created in coroutine GenerateSummaryAndTimesAudioQueueAndDirectories()

    + custom_chatgpt_pre_prompt: Text added at the front of the transcribed message sent to Chatgpt to guide chatgpt's response.

    + custom_diffuser_pre_prompt: Text added at the front of the stable diffusion prompt of the image to be created. It guides the stable diffusion result.

    + limitChatGPTResponseWordCount: Trims prompt to stable diffusion to 50 words to handle limit exception.

    + maxChatgptRequestedResponseWords: Max words requested for chatgpt to respond with.

    + numStableDiffusionImages: Number of images generated from a single chatgpt image description prompt.

    + steps: Number of stable diffusion denoising steps

    + ClassifierFreeGuidanceScaleValue: stable diffusion guidance scale

  * ChatGPT Scriptable Object Data: Script ChatgptCreds.cs

    + Enter credentials and request arguments. Create the object and add it as property to RunChatgpt.cs component in Hierarchy object "RunChatGPT"

    + Object data is consumed by coroutine GetImageDescriptionCoroutine() in script RunChatgpt.cs

    ---HERE SCRIPTABLE-OBJECT-SNAP.PNG IMAGE---
  <img width="246" alt="Dicom-Images-Processed-Progress" src="https://user-images.githubusercontent.com/24430655/176902397-e3ed3745-2ba0-4c39-95e3-bd66d9aa6ad3.PNG">
  
## Project Software
Unity version: Unity 2021.3.26f1. Only run on Editor, build not tested.

## License
This project is licensed under [CC Attribution 4.0 International License](http://creativecommons.org/licenses/by/4.0/). See [LICENSE]() for more information.

## Thank you
We thank [@sd-akashic](https://github.com/Maks-s/sd-akashic) [@Haoming02](https://github.com/Haoming02/stable-diffusion-for-unity) [@Microsoft](https://github.com/cassiebreviu/StableDiffusion) for helping to better understand onnxruntime implementation in Unity.

  
  If you find this helpful you can buy me a coffee :)

  <a href="https://www.buymeacoffee.com/sergiosolorzano" rel="nofollow">
            <img src="https://camo.githubusercontent.com/3ba8042b343d12b84b85d2e6563376af4150f9cd09e72428349c1656083c8b5a/68747470733a2f2f63646e2e6275796d6561636f666665652e636f6d2f627574746f6e732f64656661756c742d6f72616e67652e706e67" alt="Buy Me A Coffee" height="41" width="145" data-canonical-src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" style="max-width: 100%;">
            </a>
        
