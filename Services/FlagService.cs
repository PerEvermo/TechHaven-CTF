namespace SecretWebsite.Services;

public class FlagService
{
    private readonly DatabaseService _db;

    private static readonly Dictionary<string, string> ValidFlags = new()
    {
        ["RECON_01"]     = "Flag 1 — Reconnaissance (HTML source / browser console)",
        ["SHADOW_02"]    = "Flag 2 — Hidden Elements (DOM inspection)",
        ["HEADER_03"]    = "Flag 3 — HTTP Response Headers",
        ["WAREHOUSE_04"] = "Flag 4 — Robots.txt / Hidden Paths",
        ["COOKIE_05"]    = "Flag 5 — Cookie Manipulation",
        ["SQLINJECT_06"] = "Flag 6 — SQL Injection Authentication Bypass",
        ["UNION_07"]     = "Flag 7 — UNION-based SQL Injection",
    };

    public FlagService(DatabaseService db) => _db = db;

    public (bool isValid, string description) Validate(string flagValue)
    {
        var normalized = flagValue.Trim().ToUpperInvariant();
        if (ValidFlags.TryGetValue(normalized, out var desc))
            return (true, desc);
        return (false, string.Empty);
    }

    public bool Submit(string teamName, string flagValue)
    {
        var (isValid, _) = Validate(flagValue);
        _db.AddSubmission(teamName, flagValue.Trim().ToUpperInvariant(), isValid);
        return isValid;
    }

    public List<SubmissionResult> GetLeaderboard() => _db.GetCorrectSubmissions();

    public static string GetFlagDescription(string flagValue) =>
        ValidFlags.TryGetValue(flagValue.ToUpperInvariant(), out var d) ? d : flagValue;

    public static int TotalFlags => ValidFlags.Count;
}
