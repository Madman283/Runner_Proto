using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Audio_controls : MonoBehaviour
{
    //the slider that is being used in the UI need to be connected 
    public GameObject volume_slider;


    //range is here for testing in the inspector
    [Range(1.0f, 100.0f)]
    public float main_volume;
    


    //every audio soruce that is added can use this method to have is volume set to the main volume
    public void Main_volume_adjustment(AudioSource aduio_source)
    {
        aduio_source.volume = (main_volume / 100);


    }
    

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (volume_slider != null)
        {
            main_volume = volume_slider.GetComponent<Slider>().value;
        }
        else
        {
            Debug.Log("there is no Silder UI select as an game object to compare volume");
        }
    }
}
