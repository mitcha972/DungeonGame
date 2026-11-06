using UnityEngine;
using EZCameraShake;

public class ShakeDemo : MonoBehaviour 
{
    Vector3 posInf = new Vector3(3.5f, 3.5f, 3.5f);
    Vector3 rotInf = new Vector3(2, 2, 2);
    float magnitude = 3.5f, rough = 1, fadeIn = 0.1f, fadeOut = 1f;

    public CameraShaker shake;
    public GameObject Cs;
    
    public void Shake(float damageIntensity) // Get Shake based on damage amount done!
    {
        //Debug.Log("Damage Intensit - " + damageIntensity);
        float scaleFactor = Mathf.Sqrt(damageIntensity);

        Cs.GetComponent<CameraShaker>().enabled = true;
        shake.ShakeOnce(magnitude *scaleFactor, rough, fadeIn, fadeOut * scaleFactor, posInf, rotInf);
    }
}
