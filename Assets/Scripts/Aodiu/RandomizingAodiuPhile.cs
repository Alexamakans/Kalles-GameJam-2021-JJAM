using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new aodiuFile", menuName = "New Audiofile")]
public class RandomizingAodiuPhile : ScriptableObject
{
    [SerializeField] AudioClip[] _audioClips = new AudioClip[1];

    private int _lastIndex = -1;

    public AudioClip GetClip()
    {
        var clipIndex = Random.Range(0, _audioClips.Length);

        if (_lastIndex == clipIndex)
        {
            clipIndex = (clipIndex + 1) % _audioClips.Length;
        }

        _lastIndex = clipIndex;

        return _audioClips[clipIndex];
    }

}

