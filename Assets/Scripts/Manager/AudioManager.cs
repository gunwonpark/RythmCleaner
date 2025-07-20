using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public enum Sfx
    {
        Back_Button , Game_Clear, Game_Over, RoundNumber, Splash, Water_Crashed
    }
    
    [Header("----- BGM -----")]
    public  List<AudioClip>     bgmClip;    // 0번 메인메뉴(ThemeSong ), 1번 인게임
    public  float               bgmVolume;
    private AudioSource         bgmPlayer;
    
    [Header("----- SFX ------")]
    public  AudioClip[]   sfxClips;           
    public  float         sfxVolume;          
    public  int           channelNumber;        // 최대 효과음이 몇개까지 동시에 나게 할 것인지
    private AudioSource[] sfxPlayers;           // 채널 수 만큼, 만들어짐
    private int           currentChannelNumber; // 현재, 사용한 채널값
    
    void Awake()
    {
        instance = this;
        Init();
    }

    void Init()
    {
        // 배경음 플레이어 초기화
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        // PlayBgm(0,true);

        // 효과음 플레이어 초기화
        GameObject sfxObject       = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers                 = new AudioSource[channelNumber]; // 채널 수 만큼, 오디오 소스 배열 만들기
        
        for (int index = 0; index < sfxPlayers.Length; index++) // 오디오 소스 초기화
        {
            sfxPlayers[index] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[index].playOnAwake           = false;
            sfxPlayers[index].bypassListenerEffects = true;      // 해당 상태가 true이면, AudioHighPassFilter의 영향을 받지 않음.
            sfxPlayers[index].volume                = sfxVolume;
        }
    }
    
    public void PlayBgm(int clipNum, bool isPlay)
    {
        bgmPlayer.clip = bgmClip[clipNum];
        if (isPlay)
            bgmPlayer.Play();
        else
            bgmPlayer.Stop();
    }

    public void PlaySfx(Sfx sfx)
    {
        // sfxPlayers.Length는 channels 숫자와 같음
        for (int index = 0; index < sfxPlayers.Length; index++) 
        {
            // currentChannelNumber부터, 이어서 체크
            int loopIndex = (index + currentChannelNumber) % sfxPlayers.Length; // % sfxPlayers.Length의 나머지를 하는 이유는 
                                                                                // loopIndex값이 ChannelNumber를 넘지 않도록 하기 위함.
           // 재생하고 있으면, 넘어가기
            if (sfxPlayers[loopIndex].isPlaying)
                continue;

            currentChannelNumber = loopIndex;                // currentChannelNumber 갱신
            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx]; // 클립 변경
            sfxPlayers[loopIndex].Play();                    // 재생
            break;
        }
    }
}
