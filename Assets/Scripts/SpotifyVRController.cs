using SpotifyAPI.Web;
using System;
using System.Collections;
using UnityEngine;

public class SpotifyVRController : SpotifyPlayerListener
{
    [Header("Connection")]
    public bool AutoRetryConnection = true;
    public float RetryInterval = 10f;

    [Header("Debug")]
    public bool EnableDebugLogs = true;

    private bool _connected = false;
    private SpotifyClient _client;
    private string _trackName = "";
    private string _artistName = "";
    private string _albumName = "";
    private string _albumArtUrl = "";
    private float _durationMs = 0f;

    public event Action<string, string, string> OnTrackInfoChanged;
    public event Action<bool> OnConnectionStateChanged;
    public event Action<string> OnAlbumArtUrlChanged;

    public bool IsSpotifyConnected => _connected;
    public string CurrentTrackName => _trackName;
    public string CurrentArtistName => _artistName;
    public string CurrentAlbumName => _albumName;
    public string CurrentAlbumArtUrl => _albumArtUrl;
    public float CurrentDurationMs => _durationMs;

    protected override void Awake()
    {
        base.Awake();
        Log("SpotifyVRController initialized.");
    }

    protected override void OnSpotifyConnectionChanged(SpotifyClient client)
    {
        base.OnSpotifyConnectionChanged(client);
        _client = client;
        _connected = client != null;

        if (_connected)
        {
            Log("Spotify connected!");
            StartCoroutine(LoadProfile());
        }
        else
        {
            Log("Spotify disconnected.");
            ClearTrack();
            if (AutoRetryConnection)
                StartCoroutine(Retry());
        }
        OnConnectionStateChanged?.Invoke(_connected);
    }

    protected override void PlayingItemChanged(IPlayableItem item)
    {
        base.PlayingItemChanged(item);
        if (item == null) { ClearTrack(); return; }

        if (item is FullTrack t)
        {
            _trackName = t.Name;
            _artistName = S4UUtility.ArtistsToSeparatedString(", ", t.Artists);
            _albumName = t.Album != null ? t.Album.Name : "";
            _durationMs = t.DurationMs;
            if (t.Album != null && t.Album.Images != null && t.Album.Images.Count > 0)
            {
                _albumArtUrl = t.Album.Images[0].Url;
                OnAlbumArtUrlChanged?.Invoke(_albumArtUrl);
            }
            Log("Now Playing: " + _trackName + " - " + _artistName);
            OnTrackInfoChanged?.Invoke(_trackName, _artistName, _albumName);
        }
        else if (item is FullEpisode ep)
        {
            _trackName = ep.Name;
            _artistName = ep.Show != null ? ep.Show.Publisher : "";
            _albumName = ep.Show != null ? ep.Show.Name : "";
            _durationMs = ep.DurationMs;
            Log("Now Playing: " + _trackName);
            OnTrackInfoChanged?.Invoke(_trackName, _artistName, _albumName);
        }
    }

    private IEnumerator LoadProfile()
    {
        if (_client == null) yield break;
        var task = _client.UserProfile.Current();
        yield return new WaitUntil(() => task.IsCompleted);
        if (task.IsCompletedSuccessfully)
            Log("Logged in as: " + task.Result.DisplayName);
    }

    private IEnumerator Retry()
    {
        yield return new WaitForSeconds(RetryInterval);
        if (!_connected && SpotifyService.Instance != null)
            SpotifyService.Instance.AuthorizeUser();
    }

    private void ClearTrack()
    {
        _trackName = ""; _artistName = ""; _albumName = "";
        _durationMs = 0f; _albumArtUrl = "";
        OnTrackInfoChanged?.Invoke("", "", "");
    }

    public SpotifyClient GetClient() => _client;

    public void Connect()
    {
        if (SpotifyService.Instance != null && !_connected)
            SpotifyService.Instance.AuthorizeUser();
    }

    public void Disconnect()
    {
        if (SpotifyService.Instance != null && _connected)
            SpotifyService.Instance.DeauthorizeUser();
    }

    private void Log(string m)
    {
        if (EnableDebugLogs) Debug.Log("[SpotifyVR] " + m);
    }

    private void OnDestroy() => StopAllCoroutines();
}
