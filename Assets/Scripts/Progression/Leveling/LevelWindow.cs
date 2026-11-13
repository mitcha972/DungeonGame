/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelWindow : MonoBehaviour {

    private Text levelText;
    private Image experienceBarImage;
    private LevelSystem levelSystem;
    private LevelSystemAnimated levelSystemAnimated;

    private void Awake() {
        levelText = transform.Find("levelText").GetComponent<Text>();
        experienceBarImage = transform.Find("experienceBar").Find("bar").GetComponent<Image>();
        
        BindExperienceButton("experience5Btn", 5);
        BindExperienceButton("experience50Btn", 50);
        BindExperienceButton("experience500Btn", 500);
    }

    private void BindExperienceButton(string childName, int amount)
    {
        var button = UIPointerEvents.GetOrAdd(transform.Find(childName));
        if (button != null) button.LeftClicked = () => levelSystem?.AddExperience(amount);
    }

    private void SetExperienceBarSize(float experienceNormalized) {
        experienceBarImage.fillAmount = experienceNormalized;
    }

    private void SetLevelNumber(int levelNumber) {
        levelText.text = "LEVEL\n" + (levelNumber + 1);
    }

    public void SetLevelSystem(LevelSystem levelSystem) {
        this.levelSystem = levelSystem;
    }

    public void SetLevelSystemAnimated(LevelSystemAnimated levelSystemAnimated) {
        // Set the LevelSystemAnimated object
        Unsubscribe();
        this.levelSystemAnimated = levelSystemAnimated;
        if (levelSystemAnimated == null) return;

        // Update the starting values
        SetLevelNumber(levelSystemAnimated.GetLevelNumber());
        SetExperienceBarSize(levelSystemAnimated.GetExperienceNormalized());

        // Surbscribe to the changed events
        levelSystemAnimated.OnExperienceChanged += LevelSystemAnimated_OnExperienceChanged;
        levelSystemAnimated.OnLevelChanged += LevelSystemAnimated_OnLevelChanged;
    }

    private void OnDestroy() => Unsubscribe();

    private void Unsubscribe()
    {
        if (levelSystemAnimated == null) return;
        levelSystemAnimated.OnExperienceChanged -= LevelSystemAnimated_OnExperienceChanged;
        levelSystemAnimated.OnLevelChanged -= LevelSystemAnimated_OnLevelChanged;
    }

    private void LevelSystemAnimated_OnLevelChanged(object sender, System.EventArgs e) {
        // Level changed, update text
        SetLevelNumber(levelSystemAnimated.GetLevelNumber());
    }

    private void LevelSystemAnimated_OnExperienceChanged(object sender, System.EventArgs e) {
        // Experience changed, update bar size
        SetExperienceBarSize(levelSystemAnimated.GetExperienceNormalized());
    }
}
