using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioData
{
    bool playing;

    float pitch, volume, panStereo, spatialBlend, reverbZoneMix;

    //Constructor
    public AudioData(bool play, float pitch_, float vol, float stereo, float spBlend, float revZoneMix)
    {
        playing = play;
        pitch = pitch_;
        volume = vol;
        panStereo = stereo;
        spatialBlend = spBlend;
        reverbZoneMix = revZoneMix;
    }

    public bool Playing() { return playing; }

    //Getters
    public float GetPitch() { return pitch; }
    public float GetVolume() { return volume; }
    public float GetPanStereo() { return panStereo; }
    public float GetSpatialBlend() { return spatialBlend; }
    public float GetReverbZoneMix() { return reverbZoneMix; }

}
