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
                ('admin', 'Sup3rS3cr3t!', 'admin');
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

        // PATCHED version (parameterized — use this in production):
        // cmd.CommandText = "SELECT id, name, description, price, category FROM products WHERE name LIKE @search";
        // cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");

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
        cmd.CommandText = "SELECT id, name, description, price, category FROM products ORDER BY category, name";

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

        // PATCHED version (parameterized — use this in production):
        // cmd.CommandText = "SELECT username, role FROM users WHERE username = @u AND password = @p";
        // cmd.Parameters.AddWithValue("@u", username);
        // cmd.Parameters.AddWithValue("@p", password);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return (true, reader.GetString(0), reader.GetString(1));

        return (false, string.Empty, string.Empty);
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
