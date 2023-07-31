using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // �������͵���Դ
    AudioSource m_MusicAudio;// ��������
    AudioSource m_SoundAudio;// ��Ч

    // �־û���������
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
        // ��ʼ����Ч��ѭ������
        m_MusicAudio = this.gameObject.AddComponent<AudioSource>();
        m_MusicAudio.playOnAwake = false;
        m_MusicAudio.loop = true;

        m_SoundAudio = this.gameObject.AddComponent<AudioSource>();
        m_SoundAudio.loop = false;
    }

    /// <summary>
    /// ���ű�������
    /// </summary>
    /// <param name="name"></param>
    public void PlayMusic(string name)
    {
        // ����С��0.1�Ͳ����ű���������
        if (this.MusicVolume < 0.1f)
            return;
        string oldName = "";
        if (m_MusicAudio.clip != null)
            oldName = m_MusicAudio.clip.name;// ���ڲ��ŵ�������
        // ��ͬ�����ֲ��ظ�����
        if (oldName == name.Substring(0, name.LastIndexOf(".")))
        {
            m_MusicAudio.Play();
            return;
        }
            
        // ����������Դ��������ɺ�Ļص�
        Manager.Resource.LoadMusic(name, (UnityEngine.Object obj) =>
        {
            m_MusicAudio.clip = obj as AudioClip;
            m_MusicAudio.Play();
        });
    }

    /// <summary>
    /// ������Ч
    /// </summary>
    /// <param name="name"></param>
    public void PlaySound(string name)
    {
        // ����С��0.1�Ͳ����ű���������
        if (this.MusicVolume < 0.1f)
            return;

        // ������Ч��Դ��������ɺ�Ļص�
        Manager.Resource.LoadSound(name, (UnityEngine.Object obj) =>
        {
            m_SoundAudio.PlayOneShot(obj as AudioClip);
        });
    }

    /// <summary>
    /// ��ͣ��������
    /// </summary>
    public void PauseMusic()
    {
        m_MusicAudio.Pause();
    }

    /// <summary>
    /// �������ű�������
    /// </summary>
    public void OnUnPauseMusic()
    {
        m_MusicAudio.UnPause();
    }

    /// <summary>
    /// ֹͣ���ű�������
    /// </summary>
    public void StopMusic()
    {
        m_MusicAudio.Stop();
    }

    /// <summary>
    /// ������������
    /// </summary>
    /// <param name="value"></param>
    public void SetMusicVolume(float value)
    {
        this.MusicVolume = value;
    }

    /// <summary>
    /// ������Ч����
    /// </summary>
    /// <param name="value"></param>
    public void SetSoundVolume(float value)
    {
        this.SoundVolume = value;
    }


}
