using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip coins, sword, destroy;

    public AudioSource adirs;
    // Start is called before the first frame update
    void Start()
    {
        coins = Resources.Load<AudioClip>("Game coin");
        sword = Resources.Load<AudioClip>("Sword");
        destroy = Resources.Load<AudioClip>("Rock Crash");
        adirs = GetComponent<AudioSource>();


    }
    public void Playsound(string clip)
    {
        switch (clip)
        {
            case "coins":
                adirs.clip = coins;
                adirs.PlayOneShot(coins, 0.6f * Fx.SfxVolume);
                break;
            case "destroy":
                adirs.clip = destroy;
                adirs.PlayOneShot(destroy, 1f * Fx.SfxVolume);
                break;
            case "sword":
                adirs.clip = sword;
                adirs.PlayOneShot(sword, 1f * Fx.SfxVolume);
                break;
        }    
    }
}
