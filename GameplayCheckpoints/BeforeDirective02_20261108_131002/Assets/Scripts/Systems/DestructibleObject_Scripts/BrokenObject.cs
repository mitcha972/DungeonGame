using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenObject : MonoBehaviour
{
    private Renderer[] parts;
    
    void Awake()
    {
        //Set Part array
        parts = new Renderer[transform.childCount];

        //add child to part list 
        for(int i = 0; i < parts.Length ; i++)
        {
            parts[i] = transform.GetChild(i).GetComponent<MeshRenderer>();
        }

        StartCoroutine("RemoveParts");
    }

    private IEnumerator RemoveParts()
    {
        yield return new WaitForSeconds(2f);

        float aAlpha = 1f;  

        while(aAlpha > 0)
        {   
            //fade parts
            aAlpha -= 0.4f * Time.deltaTime;
            
            for(int i = 0; i < parts.Length ; i++)
            {
                Color color = parts[i].material.color;
                color.a = aAlpha;
                parts[i].material.color = color;
            }    

            if (aAlpha <= 0)
                Destroy(this.gameObject);

            yield return null;
        }

    }

}
