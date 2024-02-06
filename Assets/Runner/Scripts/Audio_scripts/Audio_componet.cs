using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio_componet : MonoBehaviour
{
    //this individual audio source is set to the main volume


    public AudioSource audio_source;

    Audio_controls access_audio_contorl;

    // Update is called once per frame
    void FixedUpdate()
    {
        
        access_audio_contorl.Main_volume_adjustment(audio_source);
    }
}
