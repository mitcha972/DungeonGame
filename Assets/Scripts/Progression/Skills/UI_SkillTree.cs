using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillTree : MonoBehaviour 
{

    [SerializeField] private Material skillLockedMaterial;
    [SerializeField] private Material skillUnlockableMaterial;
    [SerializeField] private SkillUnlockPath[] skillUnlockPathArray;
    [SerializeField] private Sprite lineSprite;
    [SerializeField] private Sprite lineGlowSprite;

    private PlayerSkills playerSkills;
    private List<SkillButton> skillButtonList;
    private TMPro.TextMeshProUGUI skillPointsText;

    //Selected Skill
    //public PlayerSkills.SkillType selectedSkill;
    public static PlayerSkills.SkillType selectedSkill;

    //selected skill slot
    public Image selectedSlot;

    private void Awake() 
    {
        var label = transform.Find("skillPointsText");
        skillPointsText = label != null ? label.GetComponent<TMPro.TextMeshProUGUI>() : null;
    }

    public void SetPlayerSkills(PlayerSkills playerSkills) 
    {
        if (playerSkills == null) return;
        Unsubscribe();
        this.playerSkills = playerSkills;

        //Add Skills To Tree
        skillButtonList = new List<SkillButton>();
        skillButtonList.Add(new SkillButton(transform.Find("Skill1Btn"), playerSkills, PlayerSkills.SkillType.MoveSpeed_1, skillLockedMaterial, skillUnlockableMaterial, this));
        skillButtonList.Add(new SkillButton(transform.Find("Skill2Btn"), playerSkills, PlayerSkills.SkillType.HealthMax_1, skillLockedMaterial, skillUnlockableMaterial, this));
        skillButtonList.Add(new SkillButton(transform.Find("Skill3Btn"), playerSkills, PlayerSkills.SkillType.MoveSpeed_2, skillLockedMaterial, skillUnlockableMaterial, this));
        skillButtonList.Add(new SkillButton(transform.Find("Skill4Btn"), playerSkills, PlayerSkills.SkillType.HealthMax_2, skillLockedMaterial, skillUnlockableMaterial, this));
        skillButtonList.Add(new SkillButton(transform.Find("Skill5Btn"), playerSkills, PlayerSkills.SkillType.Whirlwind, skillLockedMaterial, skillUnlockableMaterial, this));
        skillButtonList.Add(new SkillButton(transform.Find("Skill6Btn"), playerSkills, PlayerSkills.SkillType.Earthshatter, skillLockedMaterial, skillUnlockableMaterial, this));
        
        playerSkills.OnSkillUnlocked += PlayerSkills_OnSkillUnlocked;
        playerSkills.OnSkillPointsChanged += PlayerSkills_OnSkillPointsChanged;

        UpdateVisuals();
        UpdateSkillPoints();
    }

    private void OnDestroy() => Unsubscribe();

    private void Unsubscribe()
    {
        if (playerSkills == null) return;
        playerSkills.OnSkillUnlocked -= PlayerSkills_OnSkillUnlocked;
        playerSkills.OnSkillPointsChanged -= PlayerSkills_OnSkillPointsChanged;
    }

    private void PlayerSkills_OnSkillPointsChanged(object sender, System.EventArgs e) {
        UpdateSkillPoints();
    }

    private void PlayerSkills_OnSkillUnlocked(object sender, PlayerSkills.OnSkillUnlockedEventArgs e) {
        UpdateVisuals();
    }

    private void UpdateSkillPoints() {
        if (skillPointsText != null) skillPointsText.SetText(playerSkills.GetSkillPoints().ToString());
    }

    private void UpdateVisuals() {
        foreach (SkillButton skillButton in skillButtonList) {
            skillButton.UpdateVisual();
        }

        // Darken all links
        foreach (SkillUnlockPath skillUnlockPath in skillUnlockPathArray ?? System.Array.Empty<SkillUnlockPath>()) {
            if (skillUnlockPath == null || skillUnlockPath.linkImageArray == null) continue;
            foreach (Image linkImage in skillUnlockPath.linkImageArray) {
                if (linkImage == null) continue;
                linkImage.color = new Color(.5f, .5f, .5f);
                linkImage.sprite = lineSprite;
            }
        }
        
        foreach (SkillUnlockPath skillUnlockPath in skillUnlockPathArray ?? System.Array.Empty<SkillUnlockPath>()) {
            if (skillUnlockPath == null || skillUnlockPath.linkImageArray == null) continue;
            if (playerSkills.IsSkillUnlocked(skillUnlockPath.skillType) || playerSkills.CanUnlock(skillUnlockPath.skillType)) {
                // Skill unlocked or can be unlocked
                foreach (Image linkImage in skillUnlockPath.linkImageArray) {
                if (linkImage == null) continue;
                    linkImage.color = Color.white;
                    linkImage.sprite = lineGlowSprite;
                }
            }
        }
    }

    private class SkillButton 
    {

        private UIPointerEvents pointerEvents;
        private Image image;
        private Image backgroundImage;
        private PlayerSkills playerSkills;
        private PlayerSkills.SkillType skillType;
        private Material skillLockedMaterial;
        private Material skillUnlockableMaterial;

        public SkillButton(Transform transform, PlayerSkills playerSkills, PlayerSkills.SkillType skillType, Material skillLockedMaterial, Material skillUnlockableMaterial, UI_SkillTree owner) {
            if (transform == null) return;
            pointerEvents = UIPointerEvents.GetOrAdd(transform);
            this.playerSkills = playerSkills;
            this.skillType = skillType;
            this.skillLockedMaterial = skillLockedMaterial;
            this.skillUnlockableMaterial = skillUnlockableMaterial;

            var icon = transform.Find("image");
            var background = transform.Find("background");
            image = icon != null ? icon.GetComponent<Image>() : null;
            backgroundImage = background != null ? background.GetComponent<Image>() : null;

            pointerEvents.LeftClicked = () => {
                if (!playerSkills.IsSkillUnlocked(skillType)) {
                    // Skill not yet unlocked
                    if (!playerSkills.TryUnlockSkill(skillType)) {
                        Tooltip_Warning.ShowTooltip_Static("Cannot unlock " + skillType + "!");
                    }
                }
            };

            //If Item button is clicked
            pointerEvents.RightClicked = () => 
            {
                //Check if Skill is Unlocked
                if (!playerSkills.IsSkillUnlocked(skillType)) {
                    // Skill not yet unlocked
                        Tooltip_Warning.ShowTooltip_Static("Skill Not Unlcoked Yet " + skillType + "!");
                }
                else
                {
                    Debug.Log("Skill Selected TEST" + skillType);
                    selectedSkill = skillType;
                    if (owner != null && owner.selectedSlot != null && image != null) owner.selectedSlot.sprite = image.sprite;

                    Debug.Log("player Selected TEST" + selectedSkill);
                }
            };
        }

        public void UpdateVisual() 
        {
            if (pointerEvents == null || image == null || backgroundImage == null) return;
            if (playerSkills.IsSkillUnlocked(skillType)) 
            {
                pointerEvents.SetInteractionEnabled(true);
                image.material = null;
                backgroundImage.material = null;
            } else {
                if (playerSkills.CanUnlock(skillType)) {
                    image.material = skillUnlockableMaterial;
                    backgroundImage.color = new Color32(0x4B, 0x67, 0x7D, 0xFF);
                    pointerEvents.SetInteractionEnabled(true);
                } else {
                    image.material = skillLockedMaterial;
                    backgroundImage.color = new Color(.3f, .3f, .3f);
                    pointerEvents.SetInteractionEnabled(false);
                }
            }
        }

    }

    [System.Serializable]
    public class SkillUnlockPath 
    {
        public PlayerSkills.SkillType skillType;
        public Image[] linkImageArray;
    }

}
