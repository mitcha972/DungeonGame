using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterStats))]
public class HealthChange : MonoBehaviour 
{
    public GameObject uiPrefab;
    public GameObject textPrefab;
    //public Transform target;
    float visibleTime = 5;

    float lastMadeVisibleTime;
    Transform ui, cam;
    Image healthSlider, healthSliderBkg;

    //Damage Flsh
    public SkinnedMeshRenderer meshRenderer;
    Color originalColor;
    float Flashtime = .15f;
    
	// Use this for initialization
    void Awake () 
    {
        cam = Camera.main.transform;
    }

	void Start () 
    {
        //cam = Camera.main.transform;

        //Damage flash 
        //meshRenderer = GetComponent<SkinnedMeshRenderer>(); // Change GetComponent<SpriteRenderer>() to GetComponent<SkinnedMeshRenderer>()
        originalColor = meshRenderer.material.color;

        foreach (Canvas c in FindObjectsOfType<Canvas>())
        {
            if (c.renderMode == RenderMode.WorldSpace)
            {
                ui = Instantiate(uiPrefab, c.transform).transform;
                healthSlider = ui.GetChild(1).GetComponent<Image>(); //red
                healthSliderBkg = ui.GetChild(0).GetComponent<Image>(); //grey
                ui.gameObject.SetActive(false);
                break;
            }
        }

        //Run Health change script when a cahnge in health is detected.
        GetComponent<CharacterStats>().OnHealthChanged += OnHealthChanged;
	}

    //When Eenmy Healht is chaged (Change to take damage funtion then heal function)
    void OnHealthChanged(int maxHealth, int currentHealth) 
    {
        //Check if heal of damage
        //Flash 
        DamageFlashStart();

        if (ui != null)
        {
            ui.gameObject.SetActive(true);
            lastMadeVisibleTime = Time.time;
            float healthPercent = (float)currentHealth / maxHealth;
            healthSlider.fillAmount = healthPercent;
            
            StartCoroutine(HealthChanged());
            
            if (currentHealth <= 0)
            {
                Destroy(ui.gameObject);
            }
        }

        // Instantiate damageText on top of the player //Fix
        /*
        if (textPrefab != null)
        {
            foreach (Canvas c in FindObjectsOfType<Canvas>())
            {
                if (c.renderMode == RenderMode.WorldSpace)
                {
                     GameObject damageTextObject = Instantiate(textPrefab, c.transform);
            damageTextObject.transform.position = transform.position + Vector3.up * 3f;
            //GameObject damageTextObject = Instantiate(textPrefab, transform.position +Vector3.up * 3f, Quaternion.identity);
                    HealthTextDisplay damageText = damageTextObject.GetComponent<HealthTextDisplay>();
                    //damageText.value = ; //fix 
                    damageText.Setup(maxHealth - currentHealth);
                }
            }
        }
        */
    }

    IEnumerator HealthChanged()
 	{
        float startValue = healthSliderBkg.fillAmount;
     	float endValue = healthSlider.fillAmount;
     	float lerp = 0;

		while(lerp < 1)
		{
			lerp += Time.deltaTime / 2f;
			healthSliderBkg.fillAmount = Mathf.Lerp(startValue, endValue, lerp);
			yield return null;
		}     	
 	}

    void DamageFlashStart()
    {
        //Color lightRed = new Color(0.8f, 0.4f, 0.4f, 1);
        meshRenderer.material.color = Color.red; // Change meshRenderer.color to meshRenderer.material.color
        Invoke("DamageFlashStop", Flashtime);
    }

    void DamageFlashStop()
    {
        meshRenderer.material.color = originalColor; // Change meshRenderer.color to meshRenderer.material.color
    }

    void LateUpdate () 
    {
        if (ui != null)
        {
            ui.position =  this.transform.position + Vector3.up * 2.5f;
            ui.forward = -cam.forward;

            if (Time.time - lastMadeVisibleTime > visibleTime)
            {
                ui.gameObject.SetActive(false);
            }
        }
	}
}
