using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AudioClip_SO", menuName = "Scriptable Objects/AudioClip_SO")]
public class AudioClip_SO : ScriptableObject
{
    public List<AudioClip> BGMList;
    public List<AudioClip> SoundList;
}
