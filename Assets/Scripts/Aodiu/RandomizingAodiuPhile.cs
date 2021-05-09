using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new aodiuFile", menuName = "New Audiofile")]
public class RandomizingAodiuPhile : ScriptableObject
{
    [SerializeField] AudioClip[] _audioClips = new AudioClip[1];

    AudioClip GetClip()
    {
        var clipIndex = Random.Range(0, _audioClips.Length);
        return _audioClips[clipIndex];
    }

}

