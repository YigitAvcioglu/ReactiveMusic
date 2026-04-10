using SpotifyAPI.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SinglePlaylistSelectableTrack : MonoBehaviour
{
    [SerializeField]
    Button _playTrackBtn;

    [SerializeField]
    Text _trackNameText, _trackArtistsText, _albumText, _durationText;

    [SerializeField]
    Button _addToQueueBtn;

    private string _contextUri;
    private FullTrack _track;
    private List<string> _playlistUrisContext;

    private void Start()
    {
        // Add btn listeners on start
        if (_playTrackBtn != null)
        {
            _playTrackBtn.onClick.AddListener(this.OnPlayTrack);
        }

        if (_addToQueueBtn != null)
        {
            _addToQueueBtn.onClick.AddListener(this.OnAddToQueue);
        }
    }

    public void SetTrack(FullTrack t, string contextUri, List<string> playlistUrisContext = null)
    {
        _contextUri = contextUri;
        _playlistUrisContext = playlistUrisContext;
        // Set track and Update
        _track = t;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_track != null)
        {
            if (_trackNameText != null)
            {
                _trackNameText.text = _track.Name;
            }
            if (_trackArtistsText != null)
            {
                _trackArtistsText.text = S4UUtility.ArtistsToSeparatedString(", ", _track.Artists);
            }
            if (_albumText != null)
            {
                _albumText.text = _track.Album.Name;
            }
            if (_durationText != null)
            {
                _durationText.text = S4UUtility.MsToTimeString(_track.DurationMs);
            }
        }
    }

    private async void OnPlayTrack()
    {
        if (_track != null)
        {
            SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();
            if (client != null)
            {
                PlayerResumePlaybackRequest request;
                if (!string.IsNullOrEmpty(_contextUri))
                {
                    // Play track in context of the playlist
                    request = new PlayerResumePlaybackRequest()
                    {
                        ContextUri = _contextUri,
                        OffsetParam = new PlayerResumePlaybackRequest.Offset() { Uri = _track.Uri },
                    };
                }
                else if (_playlistUrisContext != null && _playlistUrisContext.Count > 0)
                {
                    request = new PlayerResumePlaybackRequest()
                    {
                        Uris = _playlistUrisContext,
                        OffsetParam = new PlayerResumePlaybackRequest.Offset() { Uri = _track.Uri },
                    };
                }
                else
                {
                    // Liked songs have no context URI, play directly
                    request = new PlayerResumePlaybackRequest()
                    {
                        Uris = new List<string>() { _track.Uri },
                    };
                }
                
                try 
                {
                    await client.Player.ResumePlayback(request);
                }
                catch (System.Exception)
                {
                    try 
                    {
                        var devices = await client.Player.GetAvailableDevices();
                        if (devices != null && devices.Devices.Count > 0)
                        {
                            var targetDevice = devices.Devices.Find(d => d.IsActive) ?? devices.Devices[0];
                            request.DeviceId = targetDevice.Id;
                            await client.Player.ResumePlayback(request);
                        }
                    }
                    catch (System.Exception ex2)
                    {
                        Debug.LogWarning("Spotify App | Failed to play playlist song even with available devices fallback. " + ex2.Message);
                    }
                }

                LogTrackChange("Playing track");
            }
        }
    }


    private void OnAddToQueue()
    {
        if (_track != null)
        {
            SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();
            if (client != null)
            {
                PlayerAddToQueueRequest request = new PlayerAddToQueueRequest(_track.Uri);
                client.Player.AddToQueue(request);

                LogTrackChange("Added to queue");
            }
        }
    }

    private void LogTrackChange(string source)
    {
        Debug.Log($"Spotify App | {source} '{S4UUtility.ArtistsToSeparatedString(", ", _track.Artists)} - {_track.Name}'");
    }
}
