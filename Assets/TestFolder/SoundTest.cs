using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundTester : MonoBehaviour
{
    // Variables to hold the current slider values for volume
    private float _masterVolume;
    private float _bgmVolume;
    private float _effectVolume;

    void Start()
    {
        // Set initial slider values from the SoundManager (or PlayerPrefs defaults)
        _masterVolume = SoundManager.Instance.GetVolume("MasterVolume");
        _bgmVolume = SoundManager.Instance.GetVolume("BgmVolume");
        _effectVolume = SoundManager.Instance.GetVolume("EffectVolume");

        // Automatically play the main theme on start
        Debug.Log("Playing ThemeSong on start.");
        //SoundManager.Instance.Play("ThemeSong", Sound.Bgm);
    }

    // OnGUI is used to create a simple test interface on the screen
    void OnGUI()
    {
        // Set up a basic layout area
        GUILayout.BeginArea(new Rect(10, 10, 400, 700));
        GUILayout.Box("SoundManager Test Panel");

        // === BGM Controls ===
        GUILayout.Label("--- Background Music (BGM) ---");

        if (GUILayout.Button("Play Theme Song"))
        {
            SoundManager.Instance.Play("ThemeSong", Sound.Bgm);
        }
        if (GUILayout.Button("Play Round 1 BGM (90bpm)"))
        {
            // Example of a simple play call
            SoundManager.Instance.Play("90bpm_Round1", Sound.Bgm);
        }
        if (GUILayout.Button("Fade In Round 2 BGM (95bpm)"))
        {
            // Example of using the FadeIn coroutine
            StartCoroutine(SoundManager.Instance.FadeInBGM("95bpm_Round2", 2.0f));
        }
        if (GUILayout.Button("Play Round 3 BGM (100bpm)"))
        {
            SoundManager.Instance.Play("100bpm_Round3", Sound.Bgm);
        }
        if (GUILayout.Button("Play All Clear Music"))
        {
            SoundManager.Instance.Play("Game_AllClear", Sound.Bgm);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Fade Out BGM (2s)"))
        {
            // Example of using the FadeOut coroutine
            StartCoroutine(SoundManager.Instance.FadeOutBGM(2.0f));
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Pause BGM")) SoundManager.Instance.PauseBGM();
        if (GUILayout.Button("UnPause BGM")) SoundManager.Instance.UnPauseBGM();
        if (GUILayout.Button("Stop BGM")) SoundManager.Instance.StopBGM();
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        // === Sound Effect (SFX) Controls ===
        GUILayout.Label("--- Sound Effects (SFX) ---");

        if (GUILayout.Button("Play Splash (Button/Attack)"))
        {
            SoundManager.Instance.Play("Splash", Sound.Effect);
        }
        if (GUILayout.Button("Play Back Button"))
        {
            SoundManager.Instance.Play("Back_button", Sound.Effect);
        }
        if (GUILayout.Button("Play Round Number Announce"))
        {
            SoundManager.Instance.Play("RoundNumber", Sound.Effect);
        }
        if (GUILayout.Button("Play Water Crashed"))
        {
            SoundManager.Instance.Play("Water_Crashed", Sound.Effect);
        }
        if (GUILayout.Button("Play Game Over"))
        {
            SoundManager.Instance.Play("GameOver", Sound.Effect);
        }

        GUILayout.Space(20);

        // === Volume Controls ===
        GUILayout.Label("--- Volume Controls ---");

        // Master Volume
        GUILayout.Label($"Master Volume: {(_masterVolume * 100):F0}");
        float newMasterVolume = GUILayout.HorizontalSlider(_masterVolume, 0.0f, 1.0f);
        if (newMasterVolume != _masterVolume)
        {
            _masterVolume = newMasterVolume;
            SoundManager.Instance.SetMasterVolume(_masterVolume);
        }

        // BGM Volume
        GUILayout.Label($"BGM Volume: {(_bgmVolume * 100):F0}");
        float newBgmVolume = GUILayout.HorizontalSlider(_bgmVolume, 0.0f, 1.0f);
        if (newBgmVolume != _bgmVolume)
        {
            _bgmVolume = newBgmVolume;
            SoundManager.Instance.SetBgmVolume(_bgmVolume);
        }

        // Effect Volume
        GUILayout.Label($"Effect Volume: {(_effectVolume * 100):F0}");
        float newEffectVolume = GUILayout.HorizontalSlider(_effectVolume, 0.0f, 1.0f);
        if (newEffectVolume != _effectVolume)
        {
            _effectVolume = newEffectVolume;
            SoundManager.Instance.SetEffectVolume(_effectVolume);
        }

        if (GUILayout.Button("Save Volume Settings"))
        {
            SoundManager.Instance.SaveVolumeSettings();
            Debug.Log("Volume settings saved!");
        }

        if (GUILayout.Button("ReloadScene"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        GUILayout.EndArea();
    }
}