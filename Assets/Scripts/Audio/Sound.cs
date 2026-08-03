using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Sound")]
public class Sound : ScriptableObject
{
    public string id;

    [Tooltip("One or more clips. One will be chosen at random.")]
    public AudioClip[] clips;

    [Header("Range")]
    public float minDistance = 1f;
    public float maxDistance = 1000f;

    [Header("Physics")]
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0f, 1f)] public float spatialBlend = 1f;
    public float dopplerLevel = 1f;

    [Header("Low Pass")]
    public float lpfNear = 22000f;
    public float lpfFar = 1000f;
}