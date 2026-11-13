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
    private Coroutine _healthRoutine;
    private bool _deathRestartStarted;
    private LevelSystem levelSystem;
    private LevelSystemAnimated levelSystemAnimated;
    private PlayerSkills playerSkills;

    //private EquipmentManager equipmentManager;
    public Inventory inventory;	// Reference to our inventory

    void Awake() 
    {
        equipmentManager = GetComponent<EquipmentManager>();
        if (equipmentManager != null) equipmentManager.onEquipmentChanged += OnEquipmentChanged;
        inventory = GetComponent<Inventory>();

        levelSystem = new LevelSystem();
        levelSystemAnimated = new LevelSystemAnimated(levelSystem, this);

        playerSkills = new PlayerSkills();
        
        //playerSkills.OnSkillUnlocked += PlayerSkills_OnSkillUnlocked;
    }   

    void Start()
    {
        if (playerClass != null && playerClass.health > 0) maxHealth = playerClass.health;
        StartUP();
        SetUpEquipment();
        RefreshHealthUI();
        levelSystemAnimated.OnExperienceChanged += LevelSystemAnimated_OnExperienceChanged;
        levelSystemAnimated.OnLevelChanged += LevelSystemAnimated_OnLevelChanged;
        if (levelText != null) levelText.SetText((levelSystemAnimated.GetLevelNumber() + 1).ToString());
        if (nameText != null) nameText.text = name;
    }

    void OnDestroy()
    {
        if (equipmentManager != null) equipmentManager.onEquipmentChanged -= OnEquipmentChanged;
        if (levelSystemAnimated == null) return;
        levelSystemAnimated.OnExperienceChanged -= LevelSystemAnimated_OnExperienceChanged;
        levelSystemAnimated.OnLevelChanged -= LevelSystemAnimated_OnLevelChanged;
        levelSystemAnimated.Dispose();
    }

    public void SetUpEquipment()
    {
        if (playerClass == null || playerClass.equipment == null || equipmentManager == null)
            return;
        //Sets up the players equptment.
        for (int i = 0; i < playerClass.equipment.Length; i++)
        {
            if (playerClass.equipment[i] != null) equipmentManager.Equip(playerClass.equipment[i]);
        }
    }

    public override void OnEquipmentChanged (Equipment newItem, Equipment oldItem)
	{
		base.OnEquipmentChanged(newItem, oldItem);
	
        UpdateStatsText();
    }

    public override void TakeDamage (int damageTaken, Transform attackOwner, float attackDirection)
    {
        if (IsDead || damageTaken <= 0) return;
        int previousHealth = currentHealth;
        base.TakeDamage(damageTaken, attackOwner, attackDirection);
        if (currentHealth == previousHealth) return;
        float damageIntensity = (float)(previousHealth - currentHealth) / Mathf.Max(1, maxHealth);
        if (SD != null) SD.Shake(damageIntensity);
        RefreshHealthUI();
        if (playerDamageFlash != null) playerDamageFlash.DamageFlash(damageIntensity);
    }

    public override void TakeDamage(int damageTaken)
    {
        TakeDamage(damageTaken, null, 0f);
    }

    public void Heal(int healamount)
    {
        if (IsDead || healamount <= 0) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + healamount);
        RefreshHealthUI();
    }

    private void RefreshHealthUI()
    {
        if (healthText != null) healthText.text = currentHealth + " / " + maxHealth;
        float normalizedHealth = (float)currentHealth / Mathf.Max(1, maxHealth);
        if (healthSlider != null) healthSlider.value = normalizedHealth;
        if (_healthRoutine != null) StopCoroutine(_healthRoutine);
        _healthRoutine = null;
        if (healthSlider2 != null && isActiveAndEnabled) _healthRoutine = StartCoroutine(HealthChanged());
    }

    private void OnDisable()
    {
        if (_healthRoutine != null) StopCoroutine(_healthRoutine);
        _healthRoutine = null;
    }

    private IEnumerator HealthChanged()
    {
        float startValue = healthSlider2.value;
        float endValue = (float)currentHealth / Mathf.Max(1, maxHealth);
        float lerp = 0;

        while (lerp < 1)
        {
            lerp += Time.deltaTime / 2f;
            healthSlider2.value = Mathf.Lerp(startValue, endValue, lerp);
            yield return null;
        }
        _healthRoutine = null;
    }

    public PlayerSkills GetPlayerSkills() 
    {
        return playerSkills;
    }

    void LevelSystemAnimated_OnLevelChanged(object sender, System.EventArgs e) 
    {
        if (levelText != null) levelText.SetText((levelSystemAnimated.GetLevelNumber() + 1).ToString());
        playerSkills.AddSkillPoint();
    }

    void LevelSystemAnimated_OnExperienceChanged(object sender, System.EventArgs e) 
    {
        if (experienceBar != null) experienceBar.SetSize(levelSystemAnimated.GetExperienceNormalized());
    }

    void UpdateStatsText()
    {
		// Update stat text on screen
        if (damageText != null) damageText.text = damage.ToString();
        if (armorText != null) armorText.text = armor.ToString();
	}

	public void AddXP(int xp)
	{
		levelSystem.AddExperience(xp);
	}

	[SerializeField, Min(0f)] private float deathRestartDelay = 1f;
	public override void Die(Transform attackOwner)
	{
	    if (_deathRestartStarted) return;
        _deathRestartStarted = true;
        base.Die(attackOwner);
	    var control = GetComponent<PlayerController>();
	    if (control != null) control.DisableMovement();
	    StartCoroutine(RestartAfterDeath());
	}

	private IEnumerator RestartAfterDeath()
	{
	    yield return new WaitForSecondsRealtime(deathRestartDelay);
	    Time.timeScale = 1f;
	    UnityEngine.SceneManagement.SceneManager.LoadScene(gameObject.scene.name);
	}
}