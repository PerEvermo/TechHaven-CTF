namespace SecretWebsite.Services;

public class FlagService
{
    private readonly DatabaseService _db;

    private static readonly Dictionary<string, string> ValidFlags = new()
    {
        // Easy
        ["RECON_01"]       = "Flag 1 — Reconnaissance (HTML source / browser console)",
        ["SHADOW_02"]      = "Flag 2 — Hidden Elements (DOM inspection)",
        ["HEADER_03"]      = "Flag 3 — HTTP Response Headers",
        ["WAREHOUSE_04"]   = "Flag 4 — Robots.txt / Hidden Paths",
        ["COOKIE_05"]      = "Flag 5 — Cookie Manipulation",
        ["COMMENT_08"]     = "Flag 8 — HTML Comment in Page Source",
        ["SITEMAP_09"]     = "Flag 9 — Sitemap Discovery",
        ["LOCALSTORAGE_10"]= "Flag 10 — Browser localStorage",
        ["WELLKNOWN_11"]   = "Flag 11 — Security.txt Disclosure",
        // Medium
        ["SQLINJECT_06"]   = "Flag 6 — SQL Injection Authentication Bypass",
        ["UNION_07"]       = "Flag 7 — UNION-based SQL Injection",
        ["IDOR_12"]        = "Flag 12 — Insecure Direct Object Reference (IDOR)",
        ["APIDEBUG_13"]    = "Flag 13 — Exposed Debug API Endpoint",
        ["BACKUP_14"]      = "Flag 14 — Backup File Exposed via Force Browsing",
        ["APIKEY_15"]      = "Flag 15 — API Key Authentication Bypass",
        // SQL Injection
        ["SQLCAT_16"]      = "Flag 16 — SQL Injection via Category Filter",
        ["SQLDISCOUNT_17"] = "Flag 17 — SQL Injection on Discount Code Lookup",
        ["SQLFORGOT_18"]   = "Flag 18 — SQL Injection on Forgot-Password Form",
        ["SQLUSERS_19"]    = "Flag 19 — SQL Injection on Member Search",
        // Advanced
        ["ENUMERATE_20"]   = "Flag 20 — Username Enumeration + Weak Credentials",
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
