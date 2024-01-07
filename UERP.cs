#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor;
using System.Threading.Tasks;
using Discord;
using Debug = UnityEngine.Debug;
using System.Diagnostics;

// With help of MarshMello0's code : https://github.com/MarshMello0/Editor-Rich-Presence

[InitializeOnLoad]
public static class UERP
{
    private const string applicationId = "846826015904497714";
    private static Discord.Discord discord;

    private static long startTimestamp;
    private static bool playMode = false;

    #region Initialization
    static UERP()
    {
        DelayStart();
    }
    public static async void DelayStart(int delay = 1000)
    {
        await Task.Delay(delay);
        if (DiscordRunning())
        {
            Init();
        }
    }

    public static void Init()
    {
        // Start Discord plugin
        try
        {
            discord = new Discord.Discord(long.Parse(applicationId), (long)CreateFlags.Default);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            return;
        }

        // Get start timestamp
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(EditorAnalyticsSessionInfo.elapsedTime);
        startTimestamp = DateTimeOffset.Now.Subtract(timeSpan).ToUnixTimeSeconds();

        // Update activity
        EditorApplication.update += Update;
        EditorApplication.playModeStateChanged += PlayModeChanged;
        UpdateActivity();
    }
    #endregion

    #region Update
    private static void Update()
    {
        if(discord != null) discord.RunCallbacks();
    }

    private static void PlayModeChanged (PlayModeStateChange state)
    {
        if (EditorApplication.isPlaying != playMode)
        {
            playMode = EditorApplication.isPlaying;

            UpdateActivity();
        }
    }

    public static void UpdateActivity()
    {
        if (discord == null)
        {
            Init();
            return;
        }

        Activity activity = new Activity
        {
            State = EditorSceneManager.GetActiveScene().name + " scene",
            Details = Application.productName,
            Timestamps = { Start = startTimestamp },
            Assets =
                {
                    LargeImage = "unity-icon",
                    LargeText = "Unity " + Application.unityVersion,
                    SmallImage = EditorApplication.isPlaying ? "play-mode" : "edit-mode",
                    SmallText = EditorApplication.isPlaying ? "Play mode" : "Edit mode",
                },
        };

        discord.GetActivityManager().UpdateActivity(activity, result =>
        {
            if (result != Result.Ok) Debug.LogError(result.ToString());
        });

    }
    #endregion

    private static bool DiscordRunning()
    {
        if (Process.GetProcessesByName("Discord").Length == 0)
            if (Process.GetProcessesByName("DiscordPTB").Length == 0)
                if (Process.GetProcessesByName("DiscordCanary").Length == 0)
                    return false;

        return true;
    }

}
#endif
