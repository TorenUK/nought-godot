using Godot;
using System;
using System.Collections.Generic;

public partial class UserProfile : Resource
{
    [Export] public string Username { get; set; } = "";
    [Export] public string DisplayName { get; set; } = "";
    [Export] public string SobrietyStartDateString { get; set; } = "";

    public DateTime SobrietyStartDate
    {
        get => DateTime.TryParse(SobrietyStartDateString, out var date) ? date : default;
        set => SobrietyStartDateString = value.ToString("o");
    }
    [Export] public Godot.Collections.Array<string> AddictionTypes { get; set; } = new Godot.Collections.Array<string>();
    [Export] public Godot.Collections.Array<string> NeurodivergentTraits { get; set; } = new Godot.Collections.Array<string>();
    [Export] public InputEventGesture AvatarId { get; set; } = null;
    [Export] public UserActivity CurrentActivity { get; set; } = UserActivity.Idle;
    [Export] public SafeSpace UserSafeSpace { get; set; } = new();
    [Export] public Godot.Collections.Array<string> BestFriendIds { get; set; } = new Godot.Collections.Array<string>();
    [Export] public Godot.Collections.Array<JournalEntry> JournalEntries { get; set; } = new Godot.Collections.Array<JournalEntry>();
    [Export] public UserStats Stats { get; set; } = new();

    public int GetSobrietyDays()
    {
        return (DateTime.Now - SobrietyStartDate).Days;
    }

    public string GetSobrietyString()
    {
        var days = GetSobrietyDays();
        if (days < 7) return $"{days} day{(days != 1 ? "s" : "")}";
        if (days < 30) return $"{days / 7} week{(days / 7 != 1 ? "s" : "")}";
        if (days < 365) return $"{days / 30} month{(days / 30 != 1 ? "s" : "")}";
        return $"{days / 365} year{(days / 365 != 1 ? "s" : "")}";
    }
}

public enum UserActivity
{
    Idle,
    Sleeping,
    SelfCare,
    AtGym,
    Meditating,
    Reading,
    Socializing,
    Working,
    Gaming,
    Cooking,
    Gardening
}

public partial class UserStats : Resource
{
    [Export] public int TotalLoginDays { get; set; } = 0;
    [Export] public int MoodCheckIns { get; set; } = 0;
    [Export] public int SocialPosts { get; set; } = 0;
    [Export] public int SafeSpaceVisits { get; set; } = 0;
    [Export] public string LastLoginDateString { get; set; } = "";

    public DateTime LastLoginDate
    {
        get => DateTime.TryParse(LastLoginDateString, out var date) ? date : default;
        set => LastLoginDateString = value.ToString("o");
    }
}