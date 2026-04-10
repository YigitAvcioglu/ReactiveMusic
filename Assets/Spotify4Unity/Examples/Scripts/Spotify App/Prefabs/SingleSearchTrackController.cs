using SpotifyAPI.Web;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SingleSearchTrackController : MonoBehaviour
{
    [SerializeField]
    private Text _name, _artist, _duration;

    [SerializeField]
    private Button _playBtn;

    private FullTrack _track;

    public void SetTrack(FullTrack t)
    {
        _track = t;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_name != null)
        {
            _name.text = _track.Name;
        }
        if (_artist != null)
        {
            _artist.text = S4UUtility.ArtistsToSeparatedString(", ", _track.Artists);
        }
        if (_duration != null)
        {
            _duration.text = S4UUtility.MsToTimeString(_track.DurationMs);
        }
        if (_playBtn != null)
        {
            _playBtn.onClick.AddListener(async () =>
            {
                var client = SpotifyService.Instance.GetSpotifyClient();
                if (client != null)
                {
                    PlayerResumePlaybackRequest request = new PlayerResumePlaybackRequest()
                    {
                        Uris = new List<string>() { _track.Uri },
                    };
                    
                    try 
                    {
                        bool success = await client.Player.ResumePlayback(request);
                    }
                    catch (System.Exception)
                    {
                        // If there is no active device, fetch available devices and try to play on the first one
                        try 
                        {
                            var devices = await client.Player.GetAvailableDevices();
                            if (devices != null && devices.Devices.Count > 0)
                            {
                                // Try finding an active device first, else use the first available
                                var targetDevice = devices.Devices.Find(d => d.IsActive) ?? devices.Devices[0];
                                request.DeviceId = targetDevice.Id;
                                await client.Player.ResumePlayback(request);
                            }
                        }
                        catch (System.Exception ex2)
                        {
                            Debug.LogWarning("Spotify App | Failed to play searched song even with available devices fallback. " + ex2.Message);
                        }
                    }

                    Debug.Log($"Spotify App | Playing searched song '{S4UUtility.ArtistsToSeparatedString(", ", _track.Artists)} - {_track.Name}'");
                }
            });
        }
    }
}
