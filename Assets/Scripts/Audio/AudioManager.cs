using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

public class AudioManager : MonoBehaviour
{
    [Header("Pooling")]
    [SerializeField] private int initialPoolSize = 16;
    [Header("Sounds")]
    public Sound[] allSounds;
    public static AudioManager instance;
    private readonly Dictionary<string, Sound> _soundDict = new();
    private readonly Queue<AudioSource> _sourcePool = new();
    private readonly ConcurrentQueue<Action> _mainThreadQueue = new();
    private AudioListener _listener;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var s in allSounds) if (!string.IsNullOrEmpty(s.id)) _soundDict[s.id] = s;

        for (int i = 0; i < initialPoolSize; i++) _sourcePool.Enqueue(CreateSource());
    }

    private void Start()
    {
        _listener = FindFirstObjectByType<AudioListener>();
    }

    private void Update()
    {
        while (_mainThreadQueue.TryDequeue(out var action)) action();
    }

    // Public API
    public void Play(string id, Transform target)
    {
        if (IsMainThread()) PlayInternal(id, target, loop: false);
        else _mainThreadQueue.Enqueue(() => PlayInternal(id, target, loop: false));
    }

    public LoopHandle PlayLoop(string id, Transform target)
    {
        if (!_soundDict.TryGetValue(id, out var sound)) return default;
        if (sound.clips == null || sound.clips.Length == 0) return default;

        var src = SpawnSource(sound, target, loop: true);
        return new LoopHandle { source = src };
    }

    // Internal
    private void PlayInternal(string id, Transform target, bool loop)
    {
        if (!_soundDict.TryGetValue(id, out var sound)) return;
        if (sound.clips == null || sound.clips.Length == 0) return;

        var src = SpawnSource(sound, target, loop);

        if (!loop) StartCoroutine(ReturnWhenFinished(src));
    }

    private AudioSource SpawnSource(Sound sound, Transform target, bool loop)
    {
        var src = GetSource();

        src.clip = sound.clips[UnityEngine.Random.Range(0, sound.clips.Length)];
        src.loop = loop;

        src.minDistance = sound.minDistance;
        src.maxDistance = sound.maxDistance;
        src.volume = sound.volume;
        src.spatialBlend = sound.spatialBlend;
        src.dopplerLevel = sound.dopplerLevel;
        src.rolloffMode = AudioRolloffMode.Linear;

        // Attach updater
        var attached = src.gameObject.GetComponent<AttachedAudio>();
        if (!attached) attached = src.gameObject.AddComponent<AttachedAudio>();
        attached.Init(
            target, 
            src, 
            _listener,
            sound.lpfNear,
            sound.lpfFar);

        // Speed of sound delay (one-shots only)
        if (!loop && _listener)
        {
            float dist = Vector3.Distance(_listener.transform.position, target.position);
            float delay = dist / 343f;
            src.PlayDelayed(delay);
        }
        else
        {
            src.Play();
        }

        return src;
    }

    // Loop handle

    public struct LoopHandle
    {
        internal AudioSource source;
        public bool IsValid => source != null;

        public void Stop()
        {
            if (!source) return;

            source.Stop();
            AudioManager.instance.ResetSource(source);
            AudioManager.instance.ReturnToPool(source);
            source = null;
        }
    }

    // Position + LPF
    private class AttachedAudio : MonoBehaviour
    {
        private Transform target;
        private AudioSource src;
        private AudioListener listener;
        private AudioLowPassFilter lpf;
        private float lpfNear;
        private float lpfFar;

        public void Init(
            Transform t, 
            AudioSource s, 
            AudioListener l,
            float near,
            float far)
        {
            target = t;
            src = s;
            listener = l;
            lpfNear = near;
            lpfFar = far;

            lpf = GetComponent<AudioLowPassFilter>();
            if (!lpf) lpf = gameObject.AddComponent<AudioLowPassFilter>();
        }

        private void LateUpdate()
        {
            if (!target || !src || !listener) return;

            transform.position = target.position;

            // Distance-based low-pass (air absorption)
            float dist = Vector3.Distance(listener.transform.position, target.position);
            float t = Mathf.Clamp01(dist / src.maxDistance);
            lpf.cutoffFrequency = Mathf.Lerp(lpfNear, lpfFar, t);
        }
    }

    // Pooling
    private AudioSource GetSource()
    {
        return _sourcePool.Count > 0 ? _sourcePool.Dequeue() : CreateSource();
    }

    private AudioSource CreateSource()
    {
        var go = new GameObject("AudioSource");
        go.transform.SetParent(transform);

        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;

        return src;
    }

    private System.Collections.IEnumerator ReturnWhenFinished(AudioSource src)
    {
        yield return new WaitWhile(() => src.isPlaying);

        ResetSource(src);
        _sourcePool.Enqueue(src);
    }

    private void ResetSource(AudioSource src)
    {
        src.Stop();
        src.clip = null;
        src.loop = false;
        src.spatialBlend = 0f;
        src.dopplerLevel = 0f;
    }

    internal void ReturnToPool(AudioSource src)
    {
        _sourcePool.Enqueue(src);
    }

    private static bool IsMainThread()
    {
        return System.Threading.Thread.CurrentThread.ManagedThreadId == 1;
    }
}