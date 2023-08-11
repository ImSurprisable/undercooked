using System.Data;
using Discord;
using Unity.VisualScripting;
using UnityEngine;

public class DiscordRichPresence : MonoBehaviour
{

    public static DiscordRichPresence Instance { get; private set; }

    public long applicationID;
    public string details = "It's a very original game!";
    public string largeImage = "gamelogo";
    public string largeText = "Undercooked!";

    private long time;

    public Discord.Discord discord;



    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        discord = new Discord.Discord(applicationID, (System.UInt64)Discord.CreateFlags.NoRequireDiscord);
        time = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();

        UpdateStatus();
    }

    private void Update()
    {
        try {
            discord.RunCallbacks();
        } catch {
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        try {
            var activityManager = discord.GetActivityManager();
            var activity = new Discord.Activity {
                Details = details,
                Assets = {
                    LargeImage = largeImage,
                    LargeText = largeText
                },
                Timestamps = {
                    Start = time
                }
            };

            activityManager.UpdateActivity(activity, (res) => {
                if (res != Discord.Result.Ok) Debug.LogWarning("Failed connecting to discord!");
            });
        } catch {
            Destroy(gameObject);
        }
    }
}
