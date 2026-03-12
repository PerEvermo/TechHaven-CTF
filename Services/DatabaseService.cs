using Microsoft.Data.Sqlite;

namespace SecretWebsite.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration config)
    {
        var dbPath = Path.Combine(AppContext.BaseDirectory, "techhaven.db");
        _connectionString = $"Data Source={dbPath}";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS products (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                description TEXT NOT NULL,
                price REAL NOT NULL,
                category TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                username TEXT NOT NULL,
                password TEXT NOT NULL,
                role TEXT NOT NULL DEFAULT 'user'
            );

            CREATE TABLE IF NOT EXISTS flags (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                flag_number TEXT NOT NULL,
                flag_value TEXT NOT NULL,
                hint TEXT NOT NULL,
                category TEXT NOT NULL DEFAULT 'FLAG'
            );

            CREATE TABLE IF NOT EXISTS submissions (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                team_name TEXT NOT NULL,
                flag_value TEXT NOT NULL,
                is_correct INTEGER NOT NULL DEFAULT 0,
                submitted_at TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS secrets (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                secret_key TEXT NOT NULL,
                secret_value TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS promo_codes (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                code TEXT NOT NULL,
                discount_pct REAL NOT NULL,
                description TEXT NOT NULL,
                active INTEGER NOT NULL DEFAULT 1
            );

            CREATE TABLE IF NOT EXISTS newsletter_subscribers (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                email TEXT NOT NULL,
                token TEXT NOT NULL,
                active INTEGER NOT NULL DEFAULT 1
            );

            CREATE TABLE IF NOT EXISTS community_members (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                username TEXT NOT NULL,
                badge TEXT NOT NULL,
                joined_date TEXT NOT NULL
            );
        """;
        cmd.ExecuteNonQuery();

        // Seed only if tables are empty
        cmd.CommandText = "SELECT COUNT(*) FROM products";
        if ((long)cmd.ExecuteScalar()! == 0)
            SeedProducts(conn);

        cmd.CommandText = "SELECT COUNT(*) FROM users";
        if ((long)cmd.ExecuteScalar()! == 0)
            SeedUsers(conn);

        cmd.CommandText = "SELECT COUNT(*) FROM flags";
        if ((long)cmd.ExecuteScalar()! == 0)
            SeedFlags(conn);

        cmd.CommandText = "SELECT COUNT(*) FROM secrets";
        if ((long)cmd.ExecuteScalar()! == 0)
            SeedSecrets(conn);

        cmd.CommandText = "SELECT COUNT(*) FROM promo_codes";
        if ((long)cmd.ExecuteScalar()! == 0)
            SeedPromoCodes(conn);

        cmd.CommandText = "SELECT COUNT(*) FROM newsletter_subscribers";
        if ((long)cmd.ExecuteScalar()! == 0)
            SeedNewsletterSubscribers(conn);

        cmd.CommandText = "SELECT COUNT(*) FROM community_members";
        if ((long)cmd.ExecuteScalar()! == 0)
            SeedCommunityMembers(conn);

        // Additive: insert ghost user if missing (safe parameterized query)
        cmd.CommandText = "SELECT COUNT(*) FROM users WHERE username = 'ghost'";
        if ((long)cmd.ExecuteScalar()! == 0)
        {
            cmd.CommandText = "INSERT INTO users (username, password, role) VALUES ('ghost', 'ghost123', 'staff')";
            cmd.ExecuteNonQuery();
        }

        // Additive: insert hidden draft product if missing
        cmd.CommandText = "SELECT COUNT(*) FROM products WHERE category = 'DRAFT'";
        if ((long)cmd.ExecuteScalar()! == 0)
        {
            cmd.CommandText = """
                INSERT INTO products (name, description, price, category)
                VALUES (
                    'Prototype Unit X-11',
                    'FLAG12: IDOR_12 — You accessed this product by guessing its ID directly. This is an Insecure Direct Object Reference (IDOR) vulnerability. The product listing never links here, but there is no access control on the detail endpoint.',
                    0.0,
                    'DRAFT'
                )
            """;
            cmd.ExecuteNonQuery();
        }
    }

    private void SeedProducts(SqliteConnection conn)
    {
        var products = new[]
        {
            ("Laptop Pro X1", "High-performance 15\" laptop with Intel Core i9, 32GB RAM, and 1TB SSD.", 999.99, "Laptops"),
            ("Mechanical Keyboard RGB", "Tactile Cherry MX switches with full RGB backlighting and USB passthrough.", 129.99, "Peripherals"),
            ("4K Monitor 27\"", "IPS panel, 144Hz refresh rate, HDR400, with USB-C and DisplayPort inputs.", 449.99, "Monitors"),
            ("USB-C Hub 7-in-1", "HDMI, 3x USB-A, SD card, microSD, and 100W PD passthrough.", 49.99, "Accessories"),
            ("HD Webcam Pro", "1080p at 60fps, built-in noise-cancelling mic, wide-angle lens.", 79.99, "Peripherals"),
            ("Wireless Headphones", "Active noise cancellation, 40-hour battery, premium audio drivers.", 199.99, "Audio"),
            ("Precision Gaming Mouse", "16000 DPI optical sensor, 7 programmable buttons, ergonomic design.", 59.99, "Peripherals"),
            ("NVMe SSD 1TB", "PCIe 4.0, up to 7000MB/s read speed, 5-year warranty.", 89.99, "Storage"),
            ("65W GaN Charger", "Compact 3-port charger (2x USB-C, 1x USB-A), universal voltage.", 39.99, "Accessories"),
            ("Smart Desk Lamp", "Adjustable color temperature, wireless charging base, USB-A port.", 49.99, "Accessories"),
        };

        using var tx = conn.BeginTransaction();
        foreach (var (name, desc, price, cat) in products)
        {
            var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = "INSERT INTO products (name, description, price, category) VALUES (@n, @d, @p, @c)";
            cmd.Parameters.AddWithValue("@n", name);
            cmd.Parameters.AddWithValue("@d", desc);
            cmd.Parameters.AddWithValue("@p", price);
            cmd.Parameters.AddWithValue("@c", cat);
            cmd.ExecuteNonQuery();
        }
        tx.Commit();
    }

    private void SeedUsers(SqliteConnection conn)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO users (username, password, role) VALUES
                ('johndoe', 'pass123', 'user'),
                ('admin', 'Sup3rS3cr3t!', 'admin'),
                ('ghost', 'ghost123', 'staff');
        """;
        cmd.ExecuteNonQuery();
    }

    private void SeedFlags(SqliteConnection conn)
    {
        // Only FLAG 7 lives in the database — discovered via UNION injection
        var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO flags (flag_number, flag_value, hint, category) VALUES
                ('FLAG7', 'UNION_07', 'Congratulations! You extracted the flags table via UNION-based SQL injection. This data was never meant to be readable.', 'SECRET');
        """;
        cmd.ExecuteNonQuery();
    }

    private void SeedSecrets(SqliteConnection conn)
    {
        // FLAG 16: discovered by UNION-injecting the category filter (?cat= param on /products)
        var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO secrets (secret_key, secret_value) VALUES
                ('internal_build_key', 'th-build-9f3a21'),
                ('FLAG16', 'SQLCAT_16 — You UNION-injected the category filter. Try: /products?cat=Laptops'' UNION SELECT 1,secret_key,secret_value,0.0,''x'' FROM secrets --');
        """;
        cmd.ExecuteNonQuery();
    }

    private void SeedPromoCodes(SqliteConnection conn)
    {
        // FLAG 17: the inactive promo code is revealed by bypassing "AND active = 1" via SQL injection
        var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO promo_codes (code, discount_pct, description, active) VALUES
                ('SAVE10', 10.0, 'Save 10% on your order!', 1),
                ('TECH20', 20.0, 'Tech enthusiast discount — 20% off.', 1),
                ('SUMMER15', 15.0, 'Summer sale — limited time only.', 0),
                ('FLAG17-SQLDISCOUNT_17', 100.0, 'FLAG17: SQLDISCOUNT_17 — You bypassed the active=1 filter via SQL injection on the discount code lookup!', 0);
        """;
        cmd.ExecuteNonQuery();
    }

    private void SeedNewsletterSubscribers(SqliteConnection conn)
    {
        // FLAG 18: token column contains the flag, extracted via UNION injection on the forgot-password form
        var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO newsletter_subscribers (email, token, active) VALUES
                ('alice@example.com', 'reset_a1b2c3', 1),
                ('bob@example.com', 'reset_d4e5f6', 1),
                ('noreply@techhaven.no', 'SQLFORGOT_18', 0);
        """;
        cmd.ExecuteNonQuery();
    }

    private void SeedCommunityMembers(SqliteConnection conn)
    {
        // FLAG 19: one member has the flag as their badge — found by injecting the LIKE search
        var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO community_members (username, badge, joined_date) VALUES
                ('TechWizard99', 'Gold Member', '2023-03-12'),
                ('SolderQueen', 'Hardware Hero', '2022-11-05'),
                ('ByteNinja', 'Silver Member', '2024-01-20'),
                ('Erik_H', 'Founder', '2018-06-01'),
                ('__system__', 'SQLUSERS_19 — You enumerated our member database via SQL injection on the community search!', '2000-01-01');
        """;
        cmd.ExecuteNonQuery();
    }

    // ----------------------------------------------------------------
    // VULNERABLE search — intentionally uses string concatenation
    // Used on the Products page for the SQL injection CTF challenge
    // ----------------------------------------------------------------
    public List<ProductResult> SearchProductsVulnerable(string searchTerm)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();

        // VULNERABLE: direct string interpolation — susceptible to UNION injection
        // Try: ' UNION SELECT id, flag_value, hint, 0.0, flag_number FROM flags --
        cmd.CommandText = $"SELECT id, name, description, price, category FROM products WHERE name LIKE '%{searchTerm}%'";

        var results = new List<ProductResult>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            try
            {
                results.Add(new ProductResult
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    Price = reader.GetDouble(3),
                    Category = reader.GetString(4),
                });
            }
            catch { /* skip malformed rows from injection attempts */ }
        }
        return results;
    }

    // ----------------------------------------------------------------
    // VULNERABLE category filter — Flag 16: SQLCAT_16
    // Try: /products?cat=Laptops' UNION SELECT 1,secret_key,secret_value,0.0,'x' FROM secrets --
    // ----------------------------------------------------------------
    public List<ProductResult> SearchByCategoryVulnerable(string category)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();

        // VULNERABLE: category injected directly into the query
        // Debug hint visible in the Products page source: query = SELECT id, name, description, price, category FROM products WHERE category = 'cat'
        cmd.CommandText = $"SELECT id, name, description, price, category FROM products WHERE category = '{category}'";

        var results = new List<ProductResult>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            try
            {
                results.Add(new ProductResult
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    Price = reader.GetDouble(3),
                    Category = reader.GetString(4),
                });
            }
            catch { /* skip malformed rows from injection attempts */ }
        }
        return results;
    }

    public List<ProductResult> GetAllProducts()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        // Exclude DRAFT products from the public listing — discoverable only by guessing IDs (Flag 12: IDOR)
        cmd.CommandText = "SELECT id, name, description, price, category FROM products WHERE category != 'DRAFT' ORDER BY category, name";

        var results = new List<ProductResult>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new ProductResult
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                Price = reader.GetDouble(3),
                Category = reader.GetString(4),
            });
        }
        return results;
    }

    // Parameterized — no injection possible; used for the IDOR challenge (Flag 12)
    public ProductResult? GetProductById(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, name, description, price, category FROM products WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new ProductResult
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                Price = reader.GetDouble(3),
                Category = reader.GetString(4),
            };
        }
        return null;
    }

    // ----------------------------------------------------------------
    // VULNERABLE login — intentionally uses string concatenation
    // Used on the Login page for the SQL injection CTF challenge
    // ----------------------------------------------------------------
    public (bool success, string username, string role) LoginVulnerable(string username, string password)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();

        // VULNERABLE: classic auth bypass — try: admin' --   or   ' OR '1'='1' --
        cmd.CommandText = $"SELECT username, role FROM users WHERE username = '{username}' AND password = '{password}'";

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return (true, reader.GetString(0), reader.GetString(1));

        return (false, string.Empty, string.Empty);
    }

    // Safe parameterized check — used for username enumeration (Flag 20)
    public bool UserExists(string username)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM users WHERE username = @u";
        cmd.Parameters.AddWithValue("@u", username);
        return (long)cmd.ExecuteScalar()! > 0;
    }

    // ----------------------------------------------------------------
    // VULNERABLE promo code lookup — Flag 17: SQLDISCOUNT_17
    // Try: SAVE10' OR active=0 --   or   ' OR '1'='1' --
    // ----------------------------------------------------------------
    public List<PromoCodeResult> LookupPromoCodeVulnerable(string code)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();

        // VULNERABLE: code injected directly — bypassing "AND active = 1" reveals hidden codes
        cmd.CommandText = $"SELECT code, discount_pct, description FROM promo_codes WHERE code = '{code}' AND active = 1";

        var results = new List<PromoCodeResult>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            try
            {
                results.Add(new PromoCodeResult
                {
                    Code = reader.GetString(0),
                    DiscountPct = reader.GetDouble(1),
                    Description = reader.GetString(2),
                });
            }
            catch { }
        }
        return results;
    }

    // ----------------------------------------------------------------
    // VULNERABLE forgot-password lookup — Flag 18: SQLFORGOT_18
    // Try: ' UNION SELECT email, token, 1 FROM newsletter_subscribers --
    // ----------------------------------------------------------------
    public List<ForgotPasswordResult> ForgotPasswordVulnerable(string username)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();

        // VULNERABLE: username injected directly
        // Hint: the response shows whether a user was found — also exposes username enumeration
        cmd.CommandText = $"SELECT username, password FROM users WHERE username = '{username}'";

        var results = new List<ForgotPasswordResult>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            try
            {
                results.Add(new ForgotPasswordResult
                {
                    Username = reader.GetString(0),
                    ResetToken = reader.GetString(1),
                });
            }
            catch { }
        }
        return results;
    }

    // ----------------------------------------------------------------
    // VULNERABLE member search — Flag 19: SQLUSERS_19
    // Try: %' OR '1'='1   or   ' UNION SELECT id,badge,joined_date FROM community_members --
    // ----------------------------------------------------------------
    public List<CommunityMemberResult> SearchMembersVulnerable(string query)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();

        // VULNERABLE: LIKE injection — exposes all members including __system__ with the flag badge
        cmd.CommandText = $"SELECT id, username, badge FROM community_members WHERE username LIKE '%{query}%'";

        var results = new List<CommunityMemberResult>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            try
            {
                results.Add(new CommunityMemberResult
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Badge = reader.GetString(2),
                });
            }
            catch { }
        }
        return results;
    }

    public bool HasTeamSubmittedFlag(string teamName, string flagValue)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT COUNT(*) FROM submissions
            WHERE team_name = @t AND flag_value = @f AND is_correct = 1
        """;
        cmd.Parameters.AddWithValue("@t", teamName);
        cmd.Parameters.AddWithValue("@f", flagValue);
        return (long)cmd.ExecuteScalar()! > 0;
    }

    public int GetTeamCorrectCount(string teamName)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT COUNT(DISTINCT flag_value) FROM submissions
            WHERE team_name = @t AND is_correct = 1
        """;
        cmd.Parameters.AddWithValue("@t", teamName);
        return (int)(long)cmd.ExecuteScalar()!;
    }

    public void AddSubmission(string teamName, string flagValue, bool isCorrect)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO submissions (team_name, flag_value, is_correct, submitted_at)
            VALUES (@t, @f, @c, @dt)
        """;
        cmd.Parameters.AddWithValue("@t", teamName);
        cmd.Parameters.AddWithValue("@f", flagValue);
        cmd.Parameters.AddWithValue("@c", isCorrect ? 1 : 0);
        cmd.Parameters.AddWithValue("@dt", DateTime.UtcNow.ToString("o"));
        cmd.ExecuteNonQuery();
    }

    public void ClearSubmissions()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM submissions";
        cmd.ExecuteNonQuery();
    }

    public InstructorStats GetStats()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();

        cmd.CommandText = "SELECT COUNT(*) FROM submissions";
        var total = (long)cmd.ExecuteScalar()!;

        cmd.CommandText = "SELECT COUNT(*) FROM submissions WHERE is_correct = 1";
        var correct = (long)cmd.ExecuteScalar()!;

        cmd.CommandText = "SELECT COUNT(DISTINCT team_name) FROM submissions";
        var teams = (long)cmd.ExecuteScalar()!;

        cmd.CommandText = """
            SELECT flag_value, COUNT(DISTINCT team_name)
            FROM submissions
            WHERE is_correct = 1
            GROUP BY flag_value
            ORDER BY flag_value
        """;

        var perFlag = new Dictionary<string, int>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            perFlag[reader.GetString(0)] = (int)reader.GetInt64(1);

        return new InstructorStats
        {
            TotalSubmissions = (int)total,
            CorrectSubmissions = (int)correct,
            UniqueTeams = (int)teams,
            CorrectPerFlag = perFlag,
        };
    }

    public List<SubmissionResult> GetCorrectSubmissions()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT team_name, flag_value, submitted_at
            FROM submissions
            WHERE is_correct = 1
            ORDER BY submitted_at ASC
        """;

        var results = new List<SubmissionResult>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new SubmissionResult
            {
                TeamName = reader.GetString(0),
                FlagValue = reader.GetString(1),
                SubmittedAt = DateTime.Parse(reader.GetString(2)).ToLocalTime(),
            });
        }
        return results;
    }
}

public record ProductResult
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public double Price { get; init; }
    public string Category { get; init; } = "";
}

public record PromoCodeResult
{
    public string Code { get; init; } = "";
    public double DiscountPct { get; init; }
    public string Description { get; init; } = "";
}

public record ForgotPasswordResult
{
    public string Username { get; init; } = "";
    public string ResetToken { get; init; } = "";
}

public record CommunityMemberResult
{
    public int Id { get; init; }
    public string Username { get; init; } = "";
    public string Badge { get; init; } = "";
}

public record InstructorStats
{
    public int TotalSubmissions { get; init; }
    public int CorrectSubmissions { get; init; }
    public int UniqueTeams { get; init; }
    public Dictionary<string, int> CorrectPerFlag { get; init; } = new();
}

public record SubmissionResult
{
    public string TeamName { get; init; } = "";
    public string FlagValue { get; init; } = "";
    public DateTime SubmittedAt { get; init; }
}
