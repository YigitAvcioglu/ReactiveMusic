using UnityEngine;
using UnityEngine.UI;
using SpotifyAPI.Web;

public class SpotifySimpleUIControl : MonoBehaviour
{
    public Button playButton;
    public Button pauseButton;
    public Button previousButton;
    public Button nextButton;

    private void Start()
    {
        // Butonlara tıklama (OnClick) işlevlerini kod üzerinden atıyoruz
        if (playButton != null)
            playButton.onClick.AddListener(PlayMusic);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseMusic);

        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousTrack);

        if (nextButton != null)
            nextButton.onClick.AddListener(NextTrack);
    }

    private async void PlayMusic()
    {
        var client = SpotifyService.Instance.GetSpotifyClient();
        if (client != null)
        {
            try
            {
                // Rastgele seçilmiş rahatlatıcı bir çalma listesi (Lofi Beats vb.)
                // "spotify:playlist:37i9dQZF1DWWQRwui0ExPn"
                var playRequest = new PlayerResumePlaybackRequest
                {
                    ContextUri = "spotify:playlist:37i9dQZF1DWWQRwui0ExPn"
                };

                // Asenkron olarak çalmaya başlat
                await client.Player.ResumePlayback(playRequest);
                Debug.Log("Spotify: Oynatılıyor (Lofi Beats)");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Oynatma hatası: Lütfen bilgisayarında Spotify uygulamasının AÇIK olduğundan emin ol. Hata: " + ex.Message);
            }
        }
    }

    private void PauseMusic()
    {
        var client = SpotifyService.Instance.GetSpotifyClient();
        if (client != null)
        {
            client.Player.PausePlayback();
            Debug.Log("Spotify: Pause");
        }
    }

    private void PreviousTrack()
    {
        var client = SpotifyService.Instance.GetSpotifyClient();
        if (client != null)
        {
            client.Player.SkipPrevious();
            Debug.Log("Spotify: Previous Track");
        }
    }

    private void NextTrack()
    {
        var client = SpotifyService.Instance.GetSpotifyClient();
        if (client != null)
        {
            client.Player.SkipNext();
            Debug.Log("Spotify: Next Track");
        }
    }

    private void Update()
    {
        // VR Simulator (XR) boşluk ve harf tuşlarını engellediği/kullandığı için sayıları (1, 2, 3, 4) atadık:
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            if (UnityEngine.InputSystem.Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                Debug.Log("Klavye Algılandı: 1 (Play/Resume)");
                PlayMusic();
            }
            if (UnityEngine.InputSystem.Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                Debug.Log("Klavye Algılandı: 2 (Pause)");
                PauseMusic();
            }
            if (UnityEngine.InputSystem.Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                Debug.Log("Klavye Algılandı: 3 (Next)");
                NextTrack();
            }
            if (UnityEngine.InputSystem.Keyboard.current.digit4Key.wasPressedThisFrame)
            {
                Debug.Log("Klavye Algılandı: 4 (Previous)");
                PreviousTrack();
            }
        }
    }
}
