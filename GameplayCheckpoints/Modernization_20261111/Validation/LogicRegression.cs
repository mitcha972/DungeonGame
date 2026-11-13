using System;
public static class LogicRegression
{
    static int checks;
    static void Check(bool condition, string label) { if (!condition) throw new Exception(label); checks++; Console.WriteLine("PASS " + label); }
    public static int Main()
    {
        var xp = new LevelSystem();
        int levels = 0, awards = 0;
        xp.OnLevelChanged += (_, __) => levels++;
        xp.OnExperienceChanged += (_, __) => awards++;
        xp.AddExperience(-1); xp.AddExperience(0);
        Check(xp.GetExperience() == 0 && awards == 0, "Ignore nonpositive XP");
        xp.AddExperience(100);
        Check(xp.GetLevelNumber() == 1 && xp.GetExperience() == 0 && levels == 1 && awards == 1, "Exact level threshold/events");
        xp.AddExperience(250);
        Check(xp.GetLevelNumber() == 3 && xp.GetExperience() == 50 && levels == 3 && awards == 2, "Multiple thresholds/remainder");
        xp.AddExperience(int.MaxValue);
        Check(xp.IsMaxLevel() && xp.GetLevelNumber() == 39 && xp.GetExperience() == 0 && xp.GetExperienceNormalized() == 1f, "Int32 maximum award plus existing XP");
        Check(levels == 39 && awards == 3, "Event counts at cap");
        xp.AddExperience(int.MaxValue);
        Check(levels == 39 && awards == 3, "Capped awards idempotent");
        Check(!CharacterCombat.CrossesDamageWindow(0f, .2f, .3f, .6f), "Before damage window");
        Check(CharacterCombat.CrossesDamageWindow(.2f, .4f, .3f, .6f), "Entering damage window");
        Check(CharacterCombat.CrossesDamageWindow(.2f, .9f, .3f, .6f), "Skipped damage window");
        Check(CharacterCombat.CrossesDamageWindow(0f, 1f, .3f, .6f), "Whole attack frame skip");
        Check(!CharacterCombat.CrossesDamageWindow(.7f, .9f, .3f, .6f), "After damage window");
        Check(CharacterCombat.CrossesDamageWindow(.2f, .8f, .5f, .5f), "Zero-width window");
        Check(CharacterCombat.CrossesDamageWindow(.9f, 1f, 1f, 1f), "Final boundary");
        Console.WriteLine("PASS " + checks + " logic checks");
        return 0;
    }
}
