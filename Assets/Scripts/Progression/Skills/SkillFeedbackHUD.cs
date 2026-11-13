using System.Text;
using TMPro;
using UnityEngine;

public sealed class SkillFeedbackHUD : MonoBehaviour
{
    public PlayerSkillProgress progress;
    public Canvas canvas;
    public Camera worldCamera;
    public TMP_FontAsset font;
    public AudioClip levelUpSound;
    public bool showDebug;
    private TextMeshProUGUI[] pool;
    private readonly float[] started = new float[8];
    private readonly Vector2[] origins = new Vector2[8];
    private TextMeshProUGUI celebration, debug;
    private float celebrationUntil;
    private AudioSource sound;
    public int VisiblePopups { get; private set; }
    private void Awake()
    {
        if (progress == null) progress = GetComponentInParent<PlayerSkillProgress>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (canvas == null || progress == null) { enabled = false; return; }
        pool = new TextMeshProUGUI[8];
        for (int i = 0; i < pool.Length; i++) { pool[i] = MakeText("XP " + i, 25, new Vector2(520, 50)); pool[i].gameObject.SetActive(false); }
        celebration = MakeText("Skill level celebration", 32, new Vector2(650, 110));
        celebration.rectTransform.anchorMin = celebration.rectTransform.anchorMax = new Vector2(.5f, .78f);
        celebration.gameObject.SetActive(false);
        debug = MakeText("Skills debug (Inspector toggle)", 18, new Vector2(470, 360));
        debug.alignment = TextAlignmentOptions.TopLeft;
        debug.rectTransform.anchorMin = debug.rectTransform.anchorMax = new Vector2(0, 1);
        debug.rectTransform.pivot = new Vector2(0, 1); debug.rectTransform.anchoredPosition = new Vector2(20, -100);
        debug.gameObject.SetActive(false);
        sound = gameObject.AddComponent<AudioSource>(); sound.playOnAwake = false;
    }
    private TextMeshProUGUI MakeText(string label, int size, Vector2 dimensions)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(TextMeshProUGUI)); go.transform.SetParent(transform, false);
        var text = go.GetComponent<TextMeshProUGUI>(); if (font != null) text.font = font;
        text.fontSize = size; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
        text.color = new Color(1, .85f, .38f); text.outlineWidth = .2f; text.outlineColor = Color.black;
        text.rectTransform.anchorMin = text.rectTransform.anchorMax = new Vector2(.5f, .5f); text.rectTransform.sizeDelta = dimensions;
        return text;
    }
    private void OnEnable() { if (progress != null) { progress.OnXpGained += Gained; progress.OnSkillLevelUp += Leveled; } }
    private void OnDisable()
    {
        if (progress != null) { progress.OnXpGained -= Gained; progress.OnSkillLevelUp -= Leveled; }
        if (pool != null) foreach (var text in pool) text.gameObject.SetActive(false);
        if (celebration != null) celebration.gameObject.SetActive(false);
        if (debug != null) debug.gameObject.SetActive(false);
        VisiblePopups = 0;
    }
    private void Gained(RpgSkill skill, int xp, Vector3? position)
    {
        if (pool == null) return;
        int slot = 0;
        for (int i = 0; i < pool.Length; i++)
        { if (!pool[i].gameObject.activeSelf) { slot = i; break; } if (started[i] < started[slot]) slot = i; }
        Vector2 origin = new Vector2(0, -30 + slot * 27);
        Camera camera = worldCamera != null ? worldCamera : Camera.main;
        if (position.HasValue && camera != null)
        {
            Vector3 screen = camera.WorldToScreenPoint(position.Value + Vector3.up);
            if (screen.z > 0)
            {
                screen.x = Mathf.Clamp(screen.x, 180, Mathf.Max(180, Screen.width - 180));
                screen.y = Mathf.Clamp(screen.y, 80, Mathf.Max(80, Screen.height - 120));
                RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, screen, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out origin);
                origin.y += slot * 22;
            }
        }
        pool[slot].text = "+" + xp + " " + progress.SkillName(skill) + " XP";
        pool[slot].alpha = 1; pool[slot].gameObject.SetActive(true); started[slot] = Time.unscaledTime; origins[slot] = origin;
    }
    private void Leveled(RpgSkill skill, int level)
    {
        // One reusable banner; a multi-level award finishes on the highest reached level.
        celebration.text = progress.SkillName(skill) + " Level Up!\n<size=80%>Level " + level + "</size>";
        celebrationUntil = Time.unscaledTime + 3; celebration.gameObject.SetActive(true);
        if (levelUpSound != null && !sound.isPlaying) sound.PlayOneShot(levelUpSound, .55f);
    }
    private void Update()
    {
        if (pool == null) return;
        VisiblePopups = 0;
        for (int i = 0; i < pool.Length; i++)
        {
            if (!pool[i].gameObject.activeSelf) continue;
            float t = (Time.unscaledTime - started[i]) / 1.8f;
            if (t >= 1) { pool[i].gameObject.SetActive(false); continue; }
            pool[i].rectTransform.anchoredPosition = origins[i] + Vector2.up * (t * 60);
            pool[i].alpha = 1 - Mathf.SmoothStep(0, 1, t); VisiblePopups++;
        }
        float remaining = celebrationUntil - Time.unscaledTime;
        celebration.gameObject.SetActive(remaining > 0);
        celebration.alpha = Mathf.Clamp01(remaining);
        celebration.rectTransform.localScale = Vector3.one * (1 + .06f * Mathf.Sin(Mathf.Clamp01(remaining / 3) * Mathf.PI));
        debug.gameObject.SetActive(showDebug);
        if (showDebug)
        {
            var text = new StringBuilder("SKILLS — development display\n");
            for (int i = 0; i < 10; i++) { var skill = (RpgSkill)i; text.Append(progress.SkillName(skill)).Append("  Lv ").Append(progress.GetLevel(skill)).Append("  XP ").Append(progress.GetXP(skill)).Append("  Next: ").Append(progress.GetLevel(skill) == 30 ? "MAX" : progress.XPToNextLevel(skill).ToString()).Append('\n'); }
            debug.text = text.ToString();
        }
    }
}
