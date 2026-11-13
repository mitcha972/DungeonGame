using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    SkinnedMeshRenderer meshRenderer;
    Color originalColor;
    float Flashtime = .15f;

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<SkinnedMeshRenderer>(); // Change GetComponent<SpriteRenderer>() to GetComponent<SkinnedMeshRenderer>()
        originalColor = meshRenderer.material.color;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            DamageFlashStart();
        }
    }

    void DamageFlashStart()
    {
        meshRenderer.material.color = Color.red; // Change meshRenderer.color to meshRenderer.material.color
        Invoke("DamageFlashStop", Flashtime);
    }

    void DamageFlashStop()
    {
        meshRenderer.material.color = originalColor; // Change meshRenderer.color to meshRenderer.material.color
    }
}
