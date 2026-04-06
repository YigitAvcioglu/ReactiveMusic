using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// PKCE auth flow for Spotify.
/// Uses HttpListener with 127.0.0.1 to comply with Spotify's new secure redirect URI rules.
/// </summary>
public class PKCE_Authentification : MonoBehaviour, IServiceAuthenticator
{
    public event Action<object> OnAuthenticatorComplete;

    public PKCE_AuthConfig PKCEConfig;

    private string _clientID;
    private string _redirectUri;
    private int _serverPort;
    private PKCETokenResponse _pkceToken;
    private PKCEAuthenticator _pkceAuthenticator;

    private HttpListener _httpListener;
    private CancellationTokenSource _cts;
    private string _pendingVerifier;

    private List<Action> _dispatcher = new List<Action>();

    private void Update()
    {
        if (_dispatcher.Count > 0)
        {
            foreach (Action actn in _dispatcher)
                actn.Invoke();
            _dispatcher.Clear();
        }
    }

    public void Configure(object config)
    {
        if (config is AuthorizationConfig authConfig)
        {
            _clientID = authConfig.ClientID;
            _redirectUri = authConfig.RedirectUri;
            _serverPort = authConfig.ServerPort;
        }

        if (config is PKCE_AuthConfig pkceConfig)
        {
            PKCEConfig = pkceConfig;
        }
        
        // MCP port 8080 ile çakışmaması için portu 8888 yapıyoruz!
        _serverPort = 8888;
        _redirectUri = "http://127.0.0.1:8888/callback";
    }

    public void StartAuthentification()
    {
        if (HasPreviousAuthentification())
        {
            _pkceToken = LoadPKCEToken();
            if (_pkceToken != null)
            {
                SetAuthenticator(_pkceToken);
                if (!_pkceToken.IsExpired)
                {
                    DateTime expireDT = S4UUtility.GetTokenExpiry(_pkceToken.CreatedAt, _pkceToken.ExpiresIn);
                    Debug.Log($"PKCE token loaded | Expires at '{expireDT.ToLocalTime()}'");
                }
            }
        }
        else
        {
            GetFreshAuth();
        }
    }

    public void DeauthorizeUser()
    {
        StopListener();
        _pkceToken = null;
    }

    public void RemoveSavedAuth()
    {
        if (PKCEConfig != null)
        {
            if (!string.IsNullOrEmpty(PKCEConfig.TokenPath) && File.Exists(PKCEConfig.TokenPath))
                File.Delete(PKCEConfig.TokenPath);

            if (!string.IsNullOrEmpty(PKCEConfig.PlayerPrefsKey) && PlayerPrefs.HasKey(PKCEConfig.PlayerPrefsKey))
                PlayerPrefs.DeleteKey(PKCEConfig.PlayerPrefsKey);
        }
    }

    public bool HasPreviousAuthentification()
    {
        if (PKCEConfig != null)
        {
            _pkceToken = LoadPKCEToken();
            return _pkceToken != null;
        }
        return false;
    }

    private void GetFreshAuth()
    {
        if (PKCEConfig == null) return;

        var (verifier, challenge) = LoadConfigPKCECodes();
        _pendingVerifier = verifier;

        StartHttpListener();

        Uri redirectUri = new Uri(_redirectUri);
        LoginRequest request = new LoginRequest(redirectUri, _clientID, LoginRequest.ResponseType.Code)
        {
            CodeChallenge = challenge,
            CodeChallengeMethod = "S256",
            Scope = PKCEConfig.APIScopes,
        };

        Uri uri = request.ToUri();
        try
        {
            BrowserUtil.Open(uri);
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception opening browser for auth: '{e}'");
        }
    }

    private void StartHttpListener()
    {
        StopListener();

        _cts = new CancellationTokenSource();

        string prefix = $"http://127.0.0.1:{_serverPort}/";
        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add(prefix);

        try
        {
            _httpListener.Start();
            Debug.Log($"[SpotifyAuth] HTTP listener started on {prefix}");
            Task.Run(() => ListenForCallback(_cts.Token));
        }
        catch (HttpListenerException ex)
        {
            Debug.LogError($"[SpotifyAuth] Failed to start HTTP listener: {ex.Message}");
        }
    }

    private async Task ListenForCallback(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested && _httpListener != null && _httpListener.IsListening)
            {
                var contextTask = _httpListener.GetContextAsync();
                
                using (ct.Register(() => _httpListener.Stop()))
                {
                    HttpListenerContext context;
                    try
                    {
                        context = await contextTask;
                    }
                    catch (ObjectDisposedException) { break; }
                    catch (HttpListenerException) { break; }

                    string path = context.Request.Url.AbsolutePath;

                    if (path == "/callback")
                    {
                        string code = context.Request.QueryString["code"];
                        string error = context.Request.QueryString["error"];

                        string responseHtml = "<html><body style='font-family:Arial;text-align:center;padding-top:50px;background:#191414;color:#1DB954;'>" +
                            "<h1>&#10003; Spotify Authorization Successful!</h1>" +
                            "<p style='color:#b3b3b3;'>You can close this window and return to Unity.</p></body></html>";

                        if (!string.IsNullOrEmpty(error))
                        {
                            responseHtml = "<html><body style='font-family:Arial;text-align:center;padding-top:50px;background:#191414;color:#e22134;'>" +
                                $"<h1>Authorization Failed</h1><p style='color:#b3b3b3;'>{error}</p></body></html>";
                        }

                        byte[] buffer = Encoding.UTF8.GetBytes(responseHtml);
                        context.Response.ContentLength64 = buffer.Length;
                        context.Response.ContentType = "text/html";
                        await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                        context.Response.Close();

                        if (!string.IsNullOrEmpty(code))
                        {
                            await HandleAuthCode(code);
                        }
                        else
                        {
                            Debug.LogError($"[SpotifyAuth] Authorization error: {error}");
                        }
                        break;
                    }
                    else
                    {
                        context.Response.StatusCode = 404;
                        context.Response.Close();
                    }
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Debug.LogError($"[SpotifyAuth] Listener error: {ex.Message}");
        }
    }

    private async Task HandleAuthCode(string code)
    {
        try
        {
            StopListener();

            Uri redirectUri = new Uri(_redirectUri);
            _pkceToken = await new OAuthClient().RequestToken(
                new PKCETokenRequest(_clientID, code, redirectUri, _pendingVerifier)
            );

            SavePKCEToken(_pkceToken);

            Debug.Log("[SpotifyAuth] PKCE: Received Auth Code via HTTP 127.0.0.1");

            _dispatcher.Add(() => SetAuthenticator(_pkceToken));
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SpotifyAuth] Token request failed: {ex.Message}");
        }
    }

    private void StopListener()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        if (_httpListener != null)
        {
            try
            {
                if (_httpListener.IsListening)
                    _httpListener.Stop();
                _httpListener.Close();
            }
            catch { }
            _httpListener = null;
        }
    }

    private void SetAuthenticator(PKCETokenResponse token)
    {
        _pkceAuthenticator = new PKCEAuthenticator(_clientID, token);
        _pkceAuthenticator.TokenRefreshed += this.OnTokenRefreshed;
        OnAuthenticatorComplete?.Invoke(_pkceAuthenticator);
    }

    private void OnTokenRefreshed(object sender, PKCETokenResponse token)
    {
        DateTime expireDT = S4UUtility.GetTokenExpiry(token.CreatedAt, token.ExpiresIn);
        Debug.Log($"PKCE token refreshed | Expires at '{expireDT.ToLocalTime()}'");

        bool triggerEvent = _pkceToken.IsExpired && !token.IsExpired;
        _pkceToken = token;

        if (PKCEConfig != null)
            SavePKCEToken(_pkceToken);

        if (triggerEvent)
        {
            Debug.Log("PKCE: Success in refreshing expired token into new token");
            OnAuthenticatorComplete?.Invoke(_pkceAuthenticator);
        }
    }

    private (string, string) LoadConfigPKCECodes()
    {
        if (PKCEConfig != null)
        {
            if (!string.IsNullOrEmpty(PKCEConfig.Verifier))
                return PKCEUtil.GenerateCodes(PKCEConfig.Verifier);
            else if (PKCEConfig.Length > 0)
                return PKCEUtil.GenerateCodes(PKCEConfig.Length);
        }
        return PKCEUtil.GenerateCodes();
    }

    private PKCETokenResponse LoadPKCEToken()
    {
        if (PKCEConfig.TokenSaveType == PKCETokenSaveType.File)
        {
            if (!string.IsNullOrEmpty(PKCEConfig.TokenPath))
            {
                if (!File.Exists(PKCEConfig.TokenPath))
                    return null;
                string previousToken = File.ReadAllText(PKCEConfig.TokenPath);
                if (string.IsNullOrEmpty(previousToken))
                    return null;
                return JsonConvert.DeserializeObject<PKCETokenResponse>(previousToken);
            }
        }
        else if (PKCEConfig.TokenSaveType == PKCETokenSaveType.PlayerPrefs)
        {
            string tokenStr = PlayerPrefs.GetString(PKCEConfig.PlayerPrefsKey);
            if (string.IsNullOrEmpty(tokenStr))
                return null;
            return JsonConvert.DeserializeObject<PKCETokenResponse>(tokenStr);
        }
        return null;
    }

    private void SavePKCEToken(PKCETokenResponse token)
    {
        if (token != null)
        {
            string json = JsonConvert.SerializeObject(token);
            if (PKCEConfig.TokenSaveType == PKCETokenSaveType.File)
            {
                File.WriteAllText(PKCEConfig.TokenPath, json);
            }
            else if (PKCEConfig.TokenSaveType == PKCETokenSaveType.PlayerPrefs)
            {
                _dispatcher.Add(() =>
                {
                    PlayerPrefs.SetString(PKCEConfig.PlayerPrefsKey, json);
                });
            }
        }
    }

    public PKCETokenResponse GetPKCEToken()
    {
        return _pkceToken;
    }

    public DateTime GetExpiryDateTime()
    {
        if (_pkceToken != null)
            return S4UUtility.GetTokenExpiry(_pkceToken.CreatedAt, _pkceToken.ExpiresIn);
        return DateTime.MinValue;
    }

    private void OnDestroy()
    {
        StopListener();
    }
}
