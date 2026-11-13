/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSystemAnimated {

    public event EventHandler OnExperienceChanged;
    public event EventHandler OnLevelChanged;

    private LevelSystem levelSystem;
    private bool isAnimating;
    private float updateTimer;
    private float updateTimerMax;

    private int level;
    private int experience;
    private LevelSystemAnimatedUpdater updater;
    private bool ownsUpdaterObject;
    private bool disposed;

    public LevelSystemAnimated(LevelSystem levelSystem) : this(levelSystem, null) { }

    public LevelSystemAnimated(LevelSystem levelSystem, MonoBehaviour owner) {
        SetLevelSystem(levelSystem);
        updateTimerMax = .010f;
        ownsUpdaterObject = owner == null;
        GameObject host = ownsUpdaterObject ? new GameObject("LevelSystemAnimatedUpdater") : owner.gameObject;
        updater = host.AddComponent<LevelSystemAnimatedUpdater>();
        updater.Initialize(this);
    }

    public void Dispose() {
        if (disposed) return;
        disposed = true;
        isAnimating = false;
        levelSystem.OnExperienceChanged -= LevelSystem_OnExperienceChanged;
        levelSystem.OnLevelChanged -= LevelSystem_OnLevelChanged;
        OnExperienceChanged = null;
        OnLevelChanged = null;
        if (updater != null) {
            updater.Detach();
            if (ownsUpdaterObject) UnityEngine.Object.Destroy(updater.gameObject);
            else UnityEngine.Object.Destroy(updater);
            updater = null;
        }
    }

    internal void UpdaterDestroyed(LevelSystemAnimatedUpdater source) {
        if (updater != source) return;
        updater = null;
        Dispose();
    }

    public void SetLevelSystem(LevelSystem levelSystem) {
        if (disposed) throw new ObjectDisposedException(nameof(LevelSystemAnimated));
        if (levelSystem == null) throw new ArgumentNullException(nameof(levelSystem));
        if (this.levelSystem != null)
        {
            this.levelSystem.OnExperienceChanged -= LevelSystem_OnExperienceChanged;
            this.levelSystem.OnLevelChanged -= LevelSystem_OnLevelChanged;
        }
        this.levelSystem = levelSystem;
        isAnimating = false;
        updateTimer = 0f;

        level = levelSystem.GetLevelNumber();
        experience = levelSystem.GetExperience();

        levelSystem.OnExperienceChanged += LevelSystem_OnExperienceChanged;
        levelSystem.OnLevelChanged += LevelSystem_OnLevelChanged;
    }

    private void LevelSystem_OnLevelChanged(object sender, System.EventArgs e) {
        isAnimating = true;
    }

    private void LevelSystem_OnExperienceChanged(object sender, System.EventArgs e) {
        isAnimating = true;
    }

    internal void Tick(float deltaTime) {
        if (disposed) return;
        if (isAnimating) {
            // Check if its time to update
            updateTimer += Mathf.Max(0f, deltaTime);
            while (isAnimating && updateTimer > updateTimerMax) {
                // Time to update
                updateTimer -= updateTimerMax;
                UpdateAddExperience();
            }
        }
    }

    private void UpdateAddExperience() {
        if (levelSystem.IsMaxLevel(level)) { isAnimating = false; return; }
        if (level < levelSystem.GetLevelNumber()) {
            // Local level under target level
            AddExperience();
        } else {
            // Local level equals the target level
            if (experience < levelSystem.GetExperience()) {
                AddExperience();
            } else {
                isAnimating = false;
            }
        }
    }

    private void AddExperience() {
        experience++;
        if (experience >= levelSystem.GetExperienceToNextLevel(level)) {
            level++;
            experience = 0;
            if (OnLevelChanged != null) OnLevelChanged(this, EventArgs.Empty);
        }
        if (OnExperienceChanged != null) OnExperienceChanged(this, EventArgs.Empty);
    }

    public int GetLevelNumber() {
        return level;
    }
    
    public float GetExperienceNormalized() {
        if (levelSystem.IsMaxLevel(level)) {
            return 1f;
        } else {
            return (float)experience / levelSystem.GetExperienceToNextLevel(level);
        }
    }

}
