## Project
Watching a sound wave or unrelated images for a podcast that is published in a platform that supports video and images can be dull.

We propose asking a trio of AI models, bundled in a [Unity](https://unity.com/) project, to transcribe the audio to text and generate contexual images closely tied to the transcribed text.

<img width="1000" alt="diagram-flow" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/06527403-e51a-4daf-bd77-c8be51e0bfa8">
      <p>&nbsp;</p>

We run two of the AI models locally, [Whisper-Tiny](https://huggingface.co/openai/whisper-tiny) and [stable diffusion in U-Net architecture](https://huggingface.co/CompVis/stable-diffusion-v1-4/tree/onnx); we access a third, [Chatgpt](https://learn.microsoft.com/en-us/azure/ai-services/openai/chatgpt-quickstart?tabs=command-line&pivots=programming-language-csharp), remotely via an API.

<img width="600" alt="diagram-flow" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/455539a7-0925-46d0-9b60-23557c35b29a">
<p>&nbsp;</p>

In a Unity scene we loop the AI Models over each podcast audio section to generate the contextual images. 

<video src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/8723ce86-0567-4e1e-9fd0-72330878dc97" controls="controls" muted="muted" playsinline="playsinline">
      </video>

<p>&nbsp;</p>

## Watch The [Trailer🎬](https://tapgaze.com/blog/podcast-to-image-slider/#podcast-trailer)
<video src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/99c3459a-6733-437d-8e1d-6d81a82c4edb" controls="controls" playsinline="playsinline">
      </video>

<p>&nbsp;</p>

## Project Motivation
I am new to AI, keen to tinker and learn !💥 The prototype is a good starting point as proof of concept to test the ability of AI models to help audio media augment its reach.

<p>&nbsp;</p>

## Proof of Concept Results: Talkomic App Prototype - "<i>A chat</i> into a <i>Images</i>"
Special thanks to Jason Gauci co-host at [Programming Throwdown](https://www.programmingthrowdown.com/) podcast [whose idea shared on the podcast](https://youtu.be/4FSG_SMNeuY?si=n9ShykYL7_IldcxS&t=2095) served as inspiration for this prototype.

I am thrilled and truly grateful to Maurizio Raffone at [Tech Shift F9 Podcast](https://linktr.ee/techshiftf9) for trusting me to run a proof of concept of the Talkomic app prototype with the audio file of a fantastic episode in this podcast.
+ Watch The [Complete Podcast with AI Images 📽️](https://youtu.be/pWK4vFLD6_E)
+ View and download the [Podcast's AI Image Gallery🎨](https://tapgaze.com/blog/techshift-f9-talkomic-images/#podcast-gallery)
+ See the Podcasts' AI Images in Augmented Reality😎 with the [Tapgaze app](https://apps.apple.com/gb/app/tapgaze/id1534427791)
<p>&nbsp;</p>

## Get Crisper Images with the ESRGAN AI Model
Finally, once the models have generated all images, we enhance these from 512×512 resolution to crisper 2048×2048 resolutions with the Real-ESRGAN AI Model. Suggested implementation steps in our [blog](https://tapgaze.com/blog/podcast-to-image-slider/#real-esrgan).

*512×512* <img width="246" alt="512×512_image" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/3004baac-4280-44da-9d73-b1862a47fbe2"> *2048x2048* <img width="246" alt="2048×2048_image" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/2135f264-fd92-4535-82e8-ac07d5eefc89">
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

<img width="236" alt="native-dlls" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/869dd58e-e631-41c6-8c5c-0081d6e2184b">
<img width="1031" alt="native-dlls_vs2022" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/e3b37d0f-63d3-4084-adde-0378c393e8ce">

* Clone and save [weights.pb](https://huggingface.co/CompVis/stable-diffusion-v1-4/tree/onnx/unet) weights file into Assets/Models/unet/ . Step also required for this repo's Release package (file too large).
  Fail model download availability, try [here](https://drive.google.com/file/d/1NvYhoGyw_fuYx9n6KdzWc24n5q6VavOH/view).
  
* Podcast Audio Section List Required: Create in script TalkomicManager.cs at GenerateSummaryAndTimesAudioQueueAndDirectories() a list for each section in the podcast audio with the section_name and its start time in minutes:seconds.

  + Unity will generate an output directory for each section, save the transcribed text and chatgpt image description for each section, and the images generated.

    <img width="400" alt="output-snapshot" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/1867ed3f-ae61-46e4-a128-17577cc61c1e">

* Podcast Audio Chunks: The Whisper model is designed to work on audio samples of up to 30s in duration. Hence we chunk the podcast audio for each section in chunks of max 30 seconds but load these as a queue in Whisper-tiny for each podcast section.

  <img width="400" alt="audio-chunks" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/48a9d83c-1845-4fc8-905f-4fc50bcff946">

* AI Generated Images: Shown in the scene along with the transcribed text and chatgpt image description.

  N.B. - A black image is likely caused due to a not-safe filter triggered.
  
  <img width="1000" alt="scene-progress" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/148f09bf-6c33-4df9-813a-fbd37c3c341c">

* Scene Control Input Variables:
  * Script: TalkomicManager.cs:

    + pathToAudioFile: full path to podcast audio file. Audio file is in sync with the list of section names and start times created in coroutine GenerateSummaryAndTimesAudioQueueAndDirectories()
      For example, in the case of the Tech Shift F9 E8 podcast, the sections were broken out by the host as shown below, along with the start times.

      <img width="1000" alt="image" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/099d345d-5be0-4545-802f-e1c3c40853b0">

      You'd need to create these sections for your podcast file. Unity will chunk the audio for each section in max 30 sec wav files. Hence each section, with as many 30 second chunk audio files as it is required, will be patched together and transcribed for each section. Each transcribed section is then sent to chatgpt to generate a description of an image for the transcribed text section.

      <img width="250" alt="image" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/c339f59a-d7d3-46bb-874a-07c77fae1106">

    + custom_chatgpt_pre_prompt: Text added at the front of the transcribed message sent to Chatgpt to guide chatgpt's response.

    + custom_diffuser_pre_prompt: Text added at the front of the stable diffusion prompt of the image to be created. It guides the stable diffusion result.

    + limitChatGPTResponseWordCount: Trims prompt to stable diffusion to 50 words to handle limit exception.

    + maxChatgptRequestedResponseWords: Max words requested for chatgpt to respond with.

    + numStableDiffusionImages: Number of images generated from a single chatgpt image description prompt.

    + steps: Number of stable diffusion denoising steps

    + ClassifierFreeGuidanceScaleValue: stable diffusion guidance scale

  * ChatGPT Scriptable Object API Credentials and Request Arguments Data: Script ChatgptCreds.cs

    + Create the object and add it as property to RunChatgpt.cs component in Hierarchy object "RunChatGPT"


      <img width="400" alt="scriptable-object-snap" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/c75765d7-bdcb-438d-86c7-6a2f058da637">

    + Enter credentials and request arguments

      <img width="419" alt="scriptable-credentials-example" src="https://github.com/sergiosolorzano/TalkomicApp-Unity/assets/24430655/39641504-a6a5-442b-94dd-4096721c217b">

## Prototype Software
Unity version: Unity 2021.3.26f1.

This prototype has been tested on the Unity Editor and not a build.

Tested on Windows 11 system, 64GB RAM, GPU NVIDIA GeForce RTX 3090 Ti 24GB, CPU 12th Gen Intel i9-12900K, 3400Mhz, 16 cores.

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
        
