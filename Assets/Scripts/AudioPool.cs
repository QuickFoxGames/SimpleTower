using System.Collections.Generic;
using UnityEngine;
using MGUtilities;
public class AudioPool : Singleton_template<AudioPool>
{
    [SerializeField] private AudioSource m_sourcePrefab;
    private List<AudioSource> m_reserveSources = new();
    private List<AudioSource> m_activeSources = new();

    private void AddAudioSource()
    {
        m_reserveSources.Add(Instantiate(m_sourcePrefab, transform));
    }
    public void SpawnAudioSource(Vector3 pos, AudioClip clip, float volume)
    {
        if (m_reserveSources.Count <= 0) AddAudioSource();
        AudioSource a = m_reserveSources[^1];
        m_reserveSources.Remove(a);
        m_activeSources.Add(a);
        a.clip = clip;
        a.transform.position = pos;
        a.gameObject.SetActive(true);
        a.volume = volume;
        a.Play();
        StartCoroutine(Coroutines.DelayFunction(a.clip.length, () => ReturnAudioSource(a)));
    }
    public void ReturnAudioSource(AudioSource a)
    {
        a.Stop();
        a.gameObject.SetActive(false);
        m_activeSources.Remove(a);
        m_reserveSources.Add(a);
    }
}