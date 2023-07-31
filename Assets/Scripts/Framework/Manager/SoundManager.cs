using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // 两种类型的音源
    AudioSource m_MusicAudio;// 背景音乐
    AudioSource m_SoundAudio;// 音效

    // 持久化设置音量
    private float MusicVolume
    {
        get { return PlayerPrefs.GetFloat("MusicVolume", 1.0f); }
        set
        {
            m_MusicAudio.volume = value;
            PlayerPrefs.SetFloat("MusicVolume", value);
        }
    }
    private float SoundVolume
    {
        get { return PlayerPrefs.GetFloat("SoundVolume", 1.0f); }
        set
        {
            m_SoundAudio.volume = value;
            PlayerPrefs.SetFloat("SoundVolume", value);
        }
    }

    private void Awake()
    {
        // 初始化音效和循环设置
        m_MusicAudio = this.gameObject.AddComponent<AudioSource>();
        m_MusicAudio.playOnAwake = false;
        m_MusicAudio.loop = true;

        m_SoundAudio = this.gameObject.AddComponent<AudioSource>();
        m_SoundAudio.loop = false;
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="name"></param>
    public void PlayMusic(string name)
    {
        // 音量小于0.1就不播放背景音乐了
        if (this.MusicVolume < 0.1f)
            return;
        string oldName = "";
        if (m_MusicAudio.clip != null)
            oldName = m_MusicAudio.clip.name;// 正在播放的音乐名
        // 相同的音乐不重复加载
        if (oldName == name.Substring(0, name.LastIndexOf(".")))
        {
            m_MusicAudio.Play();
            return;
        }
            
        // 加载音乐资源及加载完成后的回调
        Manager.Resource.LoadMusic(name, (UnityEngine.Object obj) =>
        {
            m_MusicAudio.clip = obj as AudioClip;
            m_MusicAudio.Play();
        });
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="name"></param>
    public void PlaySound(string name)
    {
        // 音量小于0.1就不播放背景音乐了
        if (this.MusicVolume < 0.1f)
            return;

        // 加载音效资源及加载完成后的回调
        Manager.Resource.LoadSound(name, (UnityEngine.Object obj) =>
        {
            m_SoundAudio.PlayOneShot(obj as AudioClip);
        });
    }

    /// <summary>
    /// 暂停背景音乐
    /// </summary>
    public void PauseMusic()
    {
        m_MusicAudio.Pause();
    }

    /// <summary>
    /// 继续播放背景音乐
    /// </summary>
    public void OnUnPauseMusic()
    {
        m_MusicAudio.UnPause();
    }

    /// <summary>
    /// 停止播放背景音乐
    /// </summary>
    public void StopMusic()
    {
        m_MusicAudio.Stop();
    }

    /// <summary>
    /// 设置音乐音量
    /// </summary>
    /// <param name="value"></param>
    public void SetMusicVolume(float value)
    {
        this.MusicVolume = value;
    }

    /// <summary>
    /// 设置音效音量
    /// </summary>
    /// <param name="value"></param>
    public void SetSoundVolume(float value)
    {
        this.SoundVolume = value;
    }


}
