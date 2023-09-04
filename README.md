## Project
Watching a sound wave or unrelated images for a podcast that is published in a platform that supports video and images can be dull.

We propose asking a trio of AI models, bundled in a [Unity](https://unity.com/) project, to transcribe the audio to text and generate contexual images closely tied to the transcribed text.

<img width="1000" alt="diagram-flow" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/0c19dd3a-9dd4-42a8-a59d-4838f2de49ba">
<p>&nbsp;</p>

We run two of the AI models locally, [Whisper-Tiny](https://huggingface.co/openai/whisper-tiny) and [stable diffusion in U-Net architecture](https://huggingface.co/CompVis/stable-diffusion-v1-4/tree/onnx); we access a third, [Chatgpt](https://learn.microsoft.com/en-us/azure/ai-services/openai/chatgpt-quickstart?tabs=command-line&pivots=programming-language-csharp), remotely via an API.

<img width="600" alt="diagram-flow" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/e7e43d4b-1def-4324-8937-966ef7899f0c">
<p>&nbsp;</p>

In a Unity scene we loop the AI Models over each podcast audio section to generate the contextual images.

<video src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/aaa68429-4162-43c6-933a-97e2806c7554" controls="controls" muted="muted" playsinline="playsinline">
      </video>

<p>&nbsp;</p>

## Watch The [Trailer🎬](https://tapgaze.com/blog/podcast-to-image-slider/#podcast-trailer)
<video src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/42b56411-96ba-4df4-813a-738dcd5097ca" controls="controls" playsinline="playsinline">
      </video>

## Proof of Concept Results: Talkomic App Prototype - "<i>Talk</i> and Create a <i>Comic</i>"
Special thanks to Jason Gauci co-host at [Programming Throwdown](https://www.programmingthrowdown.com/) podcast [whose idea shared on the podcast](https://youtu.be/4FSG_SMNeuY?si=n9ShykYL7_IldcxS&t=2095) served as inspiration for this prototype.

I am thrilled and truly grateful to Maurizio Raffone at [Tech Shift F9 Podcast](https://linktr.ee/techshiftf9) for trusting me to run a proof of concept of the Talkomic app prototype with the audio file of a fantastic episode in this podcast.
+ Watch The <i>Talkomicd</i> [Complete Podcast📽️](https://youtu.be/pWK4vFLD6_E)
+ View and download the [Podcast's AI Image Gallery🎨](https://tapgaze.com/blog/techshift-f9-talkomic-images/#podcast-gallery)
+ See the Podcasts' AI Images in Augmented Reality😎 with the [Tapgaze app](https://apps.apple.com/gb/app/tapgaze/id1534427791)
<p>&nbsp;</p>

## Get Crisper Images with the ESRGAN AI Model
Finally, once the models have generated all images, we enhance these from 512×512 resolution to crisper 2048×2048 resolutions with the Real-ESRGAN AI Model. Suggested implementation steps in our [blog](https://tapgaze.com/blog/podcast-to-image-slider/#real-esrgan).

*512×512* <img width="246" alt="512×512_image" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/01e41039-8f9b-444a-bc57-a800d6db53c1"> *2048x2048* <img width="246" alt="512×512_image" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/9a76a919-2616-4bdd-b7b0-080d9d346847">
<p>&nbsp;</p>

## Project's Blog Post
This is a prototype repo for proof of concept. Read [the Talkomic app blog](https://tapgaze.com/blog/podcast-to-image-slider/) for the suggested steps to build the project in Unity:
  + Convert AI models to Onnx format using Olive the [whisper-tiny text-transcription AI model](https://tapgaze.com/blog/podcast-to-image-slider/#whisper-olive)
  + Processing [chunked podcast audio](https://tapgaze.com/blog/podcast-to-image-slider/#whisper-chunks) for whisper
  + [Chatgpt API request](https://tapgaze.com/blog/podcast-to-image-slider/#chatgpt)
  + Discussion on the implementation of the [stable diffusion model](https://tapgaze.com/blog/podcast-to-image-slider/#stable-diffusion) in Unity
  + Get crisper images with [Real-ESRGAN AI model](https://tapgaze.com/blog/podcast-to-image-slider/#real-esrgan)
  + Links to technical articles and much more
<p>&nbsp;</p>

## Unity Project Features and Setup
* The AI models in the Unity project of this repo are powered by Microsoft's cross-platform [OnnxRuntime](https://onnxruntime.ai/).
  
* Native dlls (Onnxruntime, NAudio etc) required files: Project should include the following packages to Visual Studio (tested in VS2022 v.17.7.3) and dlls to Unity's Assets/Plugins directory.

  <img width="252" alt="native-dlls" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/1fd10f26-bf85-400f-b3f1-609b20ebadee">

  <img width="512" alt="native-dlls_vs2022" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/f6f20f8f-5337-466c-b337-f999147c2cf4">
  
* Clone and save [weights.pb](https://huggingface.co/CompVis/stable-diffusion-v1-4/tree/onnx/unet) weights file into Assets/Models/unet/ . Step also required for this repo's Release package (file too large).
  Fail model download availability, try [here](https://drive.google.com/file/d/1NvYhoGyw_fuYx9n6KdzWc24n5q6VavOH/view).
  
* Podcast Audio Section List Required: Create in script TalkomicManager.cs at GenerateSummaryAndTimesAudioQueueAndDirectories() a list for each section in the podcast audio with the section_name and its start time in minutes:seconds.

  + Unity will generate an output directory for each section, save the transcribed text and chatgpt image description for each section, and the images generated.

    <img width="400" alt="output-snapshot" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/4cb4a278-83b8-493f-b4b1-3af8d62faeb6">

* Podcast Audio Chunks: The Whisper model is designed to work on audio samples of up to 30s in duration. Hence we chunk the podcast audio for each section in chunks of max 30 seconds but load these as a queue in Whisper-tiny for each podcast section.
  
  <img width="400" alt="audio-chunks" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/502ad067-00db-466c-8f9c-48d466021905">

* AI Generated Images: Shown in the scene along with the transcribed text and chatgpt image description:

  <img width="1000" alt="scene-progress" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/67c2f52b-6535-4f9d-a1cb-7f8372b2071a">

* Scene Control Input Variables:
  * Script: TalkomicManager.cs:

    + pathToAudioFile: full path to podcast audio file. Audio file is in sync with the list of section names and start times created in coroutine GenerateSummaryAndTimesAudioQueueAndDirectories()

    + custom_chatgpt_pre_prompt: Text added at the front of the transcribed message sent to Chatgpt to guide chatgpt's response.

    + custom_diffuser_pre_prompt: Text added at the front of the stable diffusion prompt of the image to be created. It guides the stable diffusion result.

    + limitChatGPTResponseWordCount: Trims prompt to stable diffusion to 50 words to handle limit exception.

    + maxChatgptRequestedResponseWords: Max words requested for chatgpt to respond with.

    + numStableDiffusionImages: Number of images generated from a single chatgpt image description prompt.

    + steps: Number of stable diffusion denoising steps

    + ClassifierFreeGuidanceScaleValue: stable diffusion guidance scale

  * ChatGPT Scriptable Object API Credentials and Request Arguments Data: Script ChatgptCreds.cs

    + Create the object and add it as property to RunChatgpt.cs component in Hierarchy object "RunChatGPT"

      <img width="400" alt="scriptable-object-snap" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/d3f25819-28f3-491b-80d9-bdf1d8fa5a0e">

    + Enter credentials and request arguments

      <img width="419" alt="scriptable-credentials-example" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/9b8ed4b2-cd5c-4f80-a100-0c20031d0f4c">

## Prototype Software
Unity version: Unity 2021.3.26f1. Only run on Editor, build not tested.
<p>&nbsp;</p>

## License
This project is licensed under the MIT License. See LICENSE.txt for more information.
<p>&nbsp;</p>

## Thank you
Special thanks to Jason Gauci co-host at [Programming Throwdown](https://www.programmingthrowdown.com/) podcast [whose idea shared on the podcast](https://youtu.be/4FSG_SMNeuY?si=n9ShykYL7_IldcxS&t=2095) served as inspiration for this prototype.

We also thank [@sd-akashic](https://github.com/Maks-s/sd-akashic) [@Haoming02](https://github.com/Haoming02/stable-diffusion-for-unity) [@Microsoft](https://github.com/cassiebreviu/StableDiffusion) for helping to better understand onnxruntime implementation in Unity,

and [ai-forever](https://huggingface.co/ai-forever/Real-ESRGAN) for the [git repo for Real-ESRGAN](https://github.com/ai-forever/Real-ESRGAN).

  <p>&nbsp;</p>
  If you find this helpful you can buy me a coffee :)
<p>&nbsp;</p>
  <a href="https://www.buymeacoffee.com/sergiosolorzano" rel="nofollow">
            <img src="https://camo.githubusercontent.com/3ba8042b343d12b84b85d2e6563376af4150f9cd09e72428349c1656083c8b5a/68747470733a2f2f63646e2e6275796d6561636f666665652e636f6d2f627574746f6e732f64656661756c742d6f72616e67652e706e67" alt="Buy Me A Coffee" height="41" width="145" data-canonical-src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" style="max-width: 100%;">
            </a>
        
