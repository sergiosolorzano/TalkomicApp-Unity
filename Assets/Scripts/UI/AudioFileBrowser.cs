using UnityEngine;
using System.Collections;
using System.IO;
using SimpleFileBrowser;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class AudioFileBrowser : MonoBehaviour
{
    public UnityEngine.UI.Button goButton;

    [HideInInspector]
    public UnityEvent<string> audioLoadedCallback;

    // Warning: paths returned by FileBrowser dialogs do not contain a trailing '\' character
    // Warning: FileBrowser can only show 1 dialog at a time

    void Start()
    {
        // Set filters (optional)
        // It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
        // if all the dialogs will be using the same filters
        FileBrowser.SetFilters(true, new FileBrowser.Filter("AudioFiles", ".wav"));

        // Set default filter that is selected when the dialog is shown (optional)
        // Returns true if the default filter is set successfully
        // In this case, set Images filter as the default filter
        FileBrowser.SetDefaultFilter(".wav");

        // Set excluded file extensions (optional) (by default, .lnk and .tmp extensions are excluded)
        // Note that when you use this function, .lnk and .tmp extensions will no longer be
        // excluded unless you explicitly add them as parameters to the function
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

        // Add a new quick link to the browser (optional) (returns true if quick link is added successfully)
        // It is sufficient to add a quick link just once
        // Name: Users
        // Path: C:\Users
        // Icon: default (folder icon)
        FileBrowser.AddQuickLink("Users", "C:", null);

        // Show a save file dialog 
        // onSuccess event: not registered (which means this dialog is pretty useless)
        // onCancel event: not registered
        // Save file/folder: file, Allow multiple selection: false
        // Initial path: "C:\", Initial filename: "Screenshot.png"
        // Title: "Save As", Submit button text: "Save"
        // FileBrowser.ShowSaveDialog( null, null, FileBrowser.PickMode.Files, false, "C:\\", "Screenshot.png", "Save As", "Save" );

        // Show a select folder dialog 
        // onSuccess event: print the selected folder's path
        // onCancel event: print "Canceled"
        // Load file/folder: folder, Allow multiple selection: false
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Select Folder", Submit button text: "Select"
        // FileBrowser.ShowLoadDialog( ( paths ) => { Debug.Log( "Selected: " + paths[0] ); },
        //						   () => { Debug.Log( "Canceled" ); },
        //						   FileBrowser.PickMode.Folders, false, null, null, "Select Folder", "Select" );

    }

    public void LoadAudioFile()
    {
        // Coroutine example
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: both, Allow multiple selection: true
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Load File", Submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null, "Load Files and Folders", "Load");

        // Dialog is closed
        // Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            // Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
            for (int i = 0; i < FileBrowser.Result.Length; i++)
                Debug.Log(FileBrowser.Result[i]);

            // Read the bytes of the first file via FileBrowserHelpers
            // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
            byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);

            // Or, copy the first file to persistentDataPath
            //string destinationPath = Path.Combine(Application.streamingAssetsPath, "/Audio/", FileBrowserHelpers.GetFilename(FileBrowser.Result[0]));
            //string destinationPath = Path.Combine(Application.streamingAssetsPath, "/Audio/", FileBrowserHelpers.GetFilename(FileBrowser.Result[0]));

            string destinationPath = Application.streamingAssetsPath + $"/Audio/";
            if(!Directory.Exists(destinationPath))
                Directory.CreateDirectory(destinationPath);
            string destinationFilePath = Application.streamingAssetsPath + $"/Audio/" + FileBrowserHelpers.GetFilename(FileBrowser.Result[0]);
            //Debug.Log("DestPath " + destinationPath);
            FileBrowserHelpers.CopyFile(FileBrowser.Result[0], destinationFilePath);

#if UNITY_EDITOR
            AssetDatabase.ImportAsset(destinationFilePath);
            UnityEditor.AssetDatabase.Refresh();
#endif

            //give manager the audio file path
            audioLoadedCallback.Invoke(destinationFilePath);

            //enable go button
            goButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Go";
            goButton.interactable = true;
        }
    }
}