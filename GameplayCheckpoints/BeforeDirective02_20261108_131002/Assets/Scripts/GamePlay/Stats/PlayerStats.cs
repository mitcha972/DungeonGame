using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/* Handles the player's stats and adds/removes modifiers when equipping items. */
public class PlayerStats : CharacterStats 
{
    // Move to Player controller
    public string name;
    public Class playerClass;
    public ShakeDemo SD;
    public Slider healthSlider, healthSlider2; // background
    [SerializeField] private TMP_Text nameText, healthText, damageText, armorText;

    // Player
    [SerializeField] private ExperienceBar experienceBar;
    [SerializeField] private TMPro.TextMeshProUGUI levelText;
    [SerializeField] private PlayerDamageFlash playerDamageFlash;
    private PlayerSword playerSword;
    private LevelSystem levelSystem;
    private LevelSystemAnimated levelSystemAnimated;
    private PlayerSkills playerSkills;

    //private EquipmentManager equipmentManager;
    public Inventory inventory;	// Reference to our inventory

    void Awake() 
    {
        equipmentManager = GetComponent<EquipmentManager>();
        equipmentManager.onEquipmentChanged += OnEquipmentChanged;
        inventory = GetComponent<Inventory>();

        playerSword = GetComponent<PlayerSword>();//Fix

        levelSystem = new LevelSystem();
        levelSystemAnimated = new LevelSystemAnimated(levelSystem);

        playerSkills = new PlayerSkills();
        
        //playerSkills.OnSkillUnlocked += PlayerSkills_OnSkillUnlocked;
    }   

    void Start()
    {
        StartUP();
        SetUpEquipment(); 

        levelSystemAnimated.OnExperienceChanged += LevelSystemAnimated_OnExperienceChanged;
        levelSystemAnimated.OnLevelChanged += LevelSystemAnimated_OnLevelChanged;
        levelText.SetText((levelSystemAnimated.GetLevelNumber() + 1).ToString());
        nameText.text = name;               
    }

    void Update()
{
    if (Input.GetKeyDown(KeyCode.Y))
    {
        TakeDamage(50);
    }
}

    public void SetUpEquipment()
    {
        if(playerClass ==null )
            return;
        //Sets up the players equptment.
        for (int i = 0; i < playerClass.equipment.Length; i++)
        {
            playerClass.equipment[i].Equip(this.gameObject, 0);
        }
    }

    public override void OnEquipmentChanged (Equipment newItem, Equipment oldItem)
	{
		base.OnEquipmentChanged(newItem, oldItem);
	
        UpdateStatsText();
    }

    public override void TakeDamage (int damageTaken, Transform attackOwner, float attackDirection)
    {
        base.TakeDamage(damageTaken, attackOwner, attackDirection);

        float damageIntensity = (float)damageTaken / maxHealth;

        SD.Shake(damageIntensity);

        StartCoroutine(HealthChanged());

        playerDamageFlash.DamageFlash(damageIntensity);
    }

    public override void TakeDamage (int damageTaken)
    {
        base.TakeDamage(damageTaken);

        float damageIntensity = (float)damageTaken / maxHealth;

        SD.Shake(damageIntensity);

        StartCoroutine(HealthChanged());

        playerDamageFlash.DamageFlash(damageIntensity);
    }

    public void Heal(int healamount)
    {
        currentHealth += healamount;
        StartCoroutine(HealthChanged());
    }

    private IEnumerator HealthChanged()
    {
        float startValue = healthSlider2.value;
        float cH = currentHealth;
        float mH = maxHealth;
        float endValue = cH / mH;
        float lerp = 0;
        healthSlider.value = endValue;

        healthText.text = currentHealth + " / " + maxHealth;

        while (lerp < 1)
        {
            lerp += Time.deltaTime / 2f;
            healthSlider2.value = Mathf.Lerp(startValue, endValue, lerp);
            yield return null;
        }     
    }

    public PlayerSkills GetPlayerSkills() 
    {
        return playerSkills;
    }

    void LevelSystemAnimated_OnLevelChanged(object sender, System.EventArgs e) 
    {
        levelText.SetText((levelSystemAnimated.GetLevelNumber() + 1).ToString());
        playerSkills.AddSkillPoint();
    }

    void LevelSystemAnimated_OnExperienceChanged(object sender, System.EventArgs e) 
    {
        experienceBar.SetSize(levelSystemAnimated.GetExperienceNormalized());
    }

    void UpdateStatsText()
    {
		// Update stat text on screen
        damageText.text = damage.ToString();
        armorText.text = armor.ToString();
	}

	public void AddXP(int xp)
	{
		levelSystem.AddExperience(xp);
	}

	public override void Die(Transform attackOwner)
	{
		base.Die(attackOwner);
		PlayerManager.instance.KillPlayer();
	}
}