using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum Sound
{
    Bgm,
    Effect,
    SubEffect
}

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance = null;
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<SoundManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("@SoundManager");
                    _instance = obj.AddComponent<SoundManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    #region AudioMixer
    private AudioMixer _audioMixer;
    private AudioMixerGroup _masterGroup;
    private AudioMixerGroup _bgmGroup;
    private AudioMixerGroup _effectGroup;
    private AudioMixerGroup _subEffectGroup;

    private void SettingAudioMixer()
    {
        _audioMixer = Resources.Load<AudioMixer>("Sounds/AudioMixer");


        _audioMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Clamp(PlayerPrefs.GetFloat("MasterVolume", 0.5f), 0.0001f, 1f)));
        _audioMixer.SetFloat("BgmVolume", Mathf.Log10(Mathf.Clamp(PlayerPrefs.GetFloat("BgmVolume", 0.5f), 0.0001f, 1f)));
        _audioMixer.SetFloat("EffectVolume", Mathf.Log10(Mathf.Clamp(PlayerPrefs.GetFloat("EffectVolume", 0.5f), 0.0001f, 1f)));


        _masterGroup = _audioMixer.FindMatchingGroups("Master")[0];
        _bgmGroup = _audioMixer.FindMatchingGroups("Master/Bgm")[0];
        _effectGroup = _audioMixer.FindMatchingGroups("Master/Effect")[0];

        _bgmAudio.outputAudioMixerGroup = _bgmGroup;
        _subEffectAudio.outputAudioMixerGroup = _subEffectGroup;
        for (int i = 0; i < MAX_EFFECT_COUNT; i++)
        {
            _effectAudio[i].outputAudioMixerGroup = _effectGroup;
        }
    }

    public void SetMasterVolume(float volume)
    {
        volume = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        _audioMixer.SetFloat("MasterVolume", volume);
    }

    public void SetBgmVolume(float volume)
    {
        volume = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        _audioMixer.SetFloat("BgmVolume", volume);
    }

    public void SetEffectVolume(float volume)
    {
        volume = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        _audioMixer.SetFloat("EffectVolume", volume);
    }

    public float GetVolume(string key)
    {
        _audioMixer.GetFloat(key, out float volume);
        volume = Mathf.Pow(10, volume / 20);
        return volume;
    }


    public void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", GetVolume("MasterVolume"));
        PlayerPrefs.SetFloat("BgmVolume", GetVolume("BgmVolume"));
        PlayerPrefs.SetFloat("EffectVolume", GetVolume("EffectVolume"));

        PlayerPrefs.Save();
    }

    #endregion

    AudioSource _bgmAudio;
    AudioSource[] _effectAudio;
    AudioSource _subEffectAudio;

    Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

    private GameObject _root = null;

    private const int MAX_EFFECT_COUNT = 10;
    public void Init()
    {
        if (_root != null)
        {
            return;
        }

        _root = GameObject.Find("@SoundRoot");
        if (_root == null)
        {
            _root = new GameObject { name = "@SoundRoot" };
            UnityEngine.Object.DontDestroyOnLoad(_root);

            _bgmAudio = new GameObject { name = "BgmAudio" }.AddComponent<AudioSource>();
            _bgmAudio.transform.parent = _root.transform;
            _bgmAudio.playOnAwake = false;
            _bgmAudio.loop = true;

            _subEffectAudio = new GameObject { name = "SubEffectAudio" }.AddComponent<AudioSource>();
            _subEffectAudio.transform.parent = _root.transform;
            _subEffectAudio.playOnAwake = false;
            _subEffectAudio.loop = true;

            _effectAudio = new AudioSource[MAX_EFFECT_COUNT];

            for (int i = 0; i < MAX_EFFECT_COUNT; i++)
            {
                _effectAudio[i] = new GameObject { name = $"EffectAudio_{i}" }.AddComponent<AudioSource>();
                _effectAudio[i].transform.parent = _root.transform;
                _effectAudio[i].playOnAwake = false;
            }
        }

        SettingAudioMixer();
    }

    public void Clear()
    {
        _bgmAudio.Stop();
        _bgmAudio.clip = null;

        for (int i = 0; i < MAX_EFFECT_COUNT; i++)
        {
            _effectAudio[i].Stop();
            _effectAudio[i].clip = null;
        }

        _subEffectAudio.Stop();
        _subEffectAudio.clip = null;

        _audioClips.Clear();
    }

    public void PlaySubEffect(string Key, float volume)
    {
        AudioSource audioSource = _subEffectAudio;
        LoadAudioClip(Key, (audioClip) =>
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.loop = true;
            audioSource.Play();
        });
    }

    public AudioSource Play(string key, Sound type, float volume = 1f, float pitch = 1.0f, float time = 0f)
    {
        AudioSource audioSource = null;

        if (type == Sound.Bgm)
        {
            audioSource = _bgmAudio;
            LoadAudioClip(key, (audioClip) =>
            {
                if (audioSource.isPlaying)
                    audioSource.Stop();

                audioSource.clip = audioClip;
                audioSource.volume = volume;
                audioSource.pitch = pitch;
                audioSource.time = time;
                audioSource.Play();
            });
        }
        else if (type == Sound.Effect)
        {
            for (int i = 0; i < MAX_EFFECT_COUNT; i++)
            {
                if (!_effectAudio[i].isPlaying)
                {
                    audioSource = _effectAudio[i];
                    break;
                }
            }

            if (audioSource == null)
            {
                Debug.Log("모든 효과음 소스가 사용중입니다.");
                return null;
            }

            LoadAudioClip(key, (audioClip) =>
            {
                audioSource.pitch = pitch;
                audioSource.volume = volume;
                audioSource.time = time;
                audioSource.loop = false;
                if (time > 0f)
                {
                    audioSource.clip = audioClip;
                    audioSource.Play();
                    return;
                }
                else
                {
                    audioSource.PlayOneShot(audioClip, volume);
                }
            });
        }
        else if (type == Sound.SubEffect)
        {
            audioSource = _subEffectAudio;
            LoadAudioClip(key, (audioClip) =>
            {
                audioSource.clip = audioClip;
                audioSource.Play();
            });
        }

        return audioSource;
    }
    public IEnumerator ChangeEffectVolume(float start, float target, float duration)
    {
        if (_effectAudio == null)
            yield break;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            for (int i = 0; i < MAX_EFFECT_COUNT; i++)
            {
                _effectAudio[i].volume = Mathf.Lerp(start, target, t);
            }
            yield return null;
        }

    }
    public IEnumerator FadeInBGM(string path, float duration)
    {
        LoadAudioClip(path, (audioClip) => {
            _bgmAudio.clip = audioClip;
            _bgmAudio.volume = 0f;
            _bgmAudio.Play();
        });

        float startVolume = 0f;
        float targetVolume = 1f;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            _bgmAudio.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        _bgmAudio.volume = targetVolume;
    }

    public IEnumerator FadeOutBGM(float duration)
    {
        if (_bgmAudio.clip == null)
            yield break;

        float startVolume = _bgmAudio.volume;
        float targetVolume = 0f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            _bgmAudio.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }
        _bgmAudio.Stop();
        _bgmAudio.volume = 1f;
    }

    public void PauseBGM()
    {
        if (_bgmAudio.clip != null)
        {
            _bgmAudio.Pause();
        }
    }

    public void UnPauseBGM()
    {
        if (_bgmAudio.clip != null)
        {
            _bgmAudio.UnPause();
        }
    }

    public void StopBGM()
    {
        _bgmAudio.Stop();
        _bgmAudio.clip = null;
    }

    //현재 pause사용은 하나의 음악만 사용하므로 문제 발생 여부가 없지만
    //추후 한번에 두개 이상이 사용될시 문제 발생 및 해결 필요
    public void UnPauseSubEffect()
    {
        if (_subEffectAudio.clip != null)
        {
            _subEffectAudio.UnPause();
        }
    }

    public void PauseSubEffect()
    {
        if (_subEffectAudio.clip != null)
        {
            _subEffectAudio.Pause();
        }
    }

    public void StopSubEffect()
    {
        _subEffectAudio.Stop();
        _subEffectAudio.clip = null;
    }

    private void LoadAudioClip(string key, Action<AudioClip> callback)
    {
        AudioClip audioClip = null;
        if (_audioClips.TryGetValue(key, out audioClip))
        {
            callback?.Invoke(audioClip);
            return;
        }

        audioClip = Resources.Load<AudioClip>($"Sounds/{key}");

        if (audioClip == null)
        {
            Debug.LogError($"AudioClip not found: {key}");
        }

        if (!_audioClips.ContainsKey(key))
            _audioClips.Add(key, audioClip);

        callback?.Invoke(audioClip);
    }
}