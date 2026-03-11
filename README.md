# TechHaven Electronics — Cybersecurity Workshop CTF

A deliberately vulnerable fake electronics webshop built with **Blazor Web App (.NET 10)** and **SQLite**, designed as a Capture The Flag (CTF) platform for cybersecurity workshops.

Students attack the site as if it were a real target. Seven flags are hidden across the application using techniques ranging from basic source inspection to SQL injection.

---

## Table of Contents

1. [Requirements](#requirements)
2. [Running the Project](#running-the-project)
3. [Hosting on a Windows Machine](#hosting-on-a-windows-machine)
4. [Project Structure](#project-structure)
5. [The CTF Challenges — Overview](#the-ctf-challenges--overview)
6. [Instructor Walkthrough (Full Solutions)](#instructor-walkthrough-full-solutions)

---

## Requirements

| Tool | Version |
|------|---------|
| .NET SDK | 10.0 or later |
| OS | Windows / Linux / macOS |
| Browser | Any modern browser with DevTools |

Install .NET 10: https://dotnet.microsoft.com/download

---

## Running the Project

### Development (your machine)

```bash
cd SecretWebsite
dotnet run
```

The app starts on:

| Protocol | URL |
|----------|-----|
| HTTP | http://localhost:5210 |
| HTTPS | https://localhost:7182 |

> The SQLite database (`techhaven.db`) is created and seeded automatically on first run.
> It lives in the build output folder (`bin/Debug/net10.0/techhaven.db`).

To reset the database (clear all submissions, re-seed), simply delete `techhaven.db` and restart.

### Using a specific profile

```bash
dotnet run --launch-profile http    # HTTP only
dotnet run --launch-profile https   # HTTP + HTTPS
```

---

## Hosting on a Windows Machine

So your students can reach the site from their own machines on the same network:

### 1. Publish a self-contained release build

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -o C:\TechHaven
```

### 2. Run it on a specific IP / port

```powershell
cd C:\TechHaven
.\SecretWebsite.exe --urls "http://0.0.0.0:5000"
```

The app is now reachable at `http://<YOUR-IP>:5000` from any machine on the network.

### 3. Find your machine's IP

```powershell
ipconfig
# Look for "IPv4 Address" under your network adapter, e.g. 192.168.1.42
```

Students then browse to `http://192.168.1.42:5000`.

### 4. Windows Firewall (if students can't connect)

```powershell
# Run as Administrator
netsh advfirewall firewall add rule name="TechHaven CTF" dir=in action=allow protocol=TCP localport=5000
```

### Optional: Run as a background service

```powershell
# Install as a Windows Service (run once, survives reboots)
sc.exe create TechHavenCTF binPath="C:\TechHaven\SecretWebsite.exe --urls http://0.0.0.0:5000" start=auto
sc.exe start TechHavenCTF

# To stop and remove:
sc.exe stop TechHavenCTF
sc.exe delete TechHavenCTF
```

---

## Project Structure

```
SecretWebsite/
├── Components/
│   ├── Pages/
│   │   ├── Home.razor          # Flag 1 — HTML comment + console.log
│   │   ├── About.razor         # Flag 3 — HTTP response header
│   │   ├── Products.razor      # Flag 2 (hidden div) + Flag 7 (UNION injection)
│   │   ├── Login.razor         # Flag 6 — SQL injection auth bypass
│   │   ├── Admin.razor         # Flag 6 — shown after bypass
│   │   ├── Vip.razor           # Flag 5 — cookie manipulation
│   │   ├── HiddenWarehouse.razor  # Flag 4 — robots.txt discovery
│   │   └── Scoreboard.razor    # Flag submission + leaderboard
│   └── Layout/
│       └── MainLayout.razor    # Top navbar, shows Admin link when logged in
├── Services/
│   ├── DatabaseService.cs      # SQLite queries (vulnerable + patched versions)
│   ├── AuthStateService.cs     # Per-circuit login state
│   └── FlagService.cs          # Flag validation and leaderboard
├── wwwroot/
│   ├── robots.txt              # Hints at hidden paths (Flag 4)
│   └── js/ctf.js               # Cookie helpers + console banner
├── Program.cs                  # HTTP header middleware (Flag 3)
└── README.md
```

---

## The CTF Challenges — Overview

> **Tell students:** "TechHaven Electronics has 7 security flags hidden across the site. Find them, note down the value, and submit on the Scoreboard page."

| # | Flag Value | Category | Difficulty |
|---|-----------|----------|-----------|
| 1 | `RECON_01` | Source / Console | ⭐ Easy |
| 2 | `SHADOW_02` | DOM Inspection | ⭐ Easy |
| 3 | `HEADER_03` | HTTP Headers | ⭐⭐ Medium |
| 4 | `WAREHOUSE_04` | Recon / robots.txt | ⭐⭐ Medium |
| 5 | `COOKIE_05` | Cookie Manipulation | ⭐⭐ Medium |
| 6 | `SQLINJECT_06` | SQL Injection (Auth Bypass) | ⭐⭐⭐ Hard |
| 7 | `UNION_07` | SQL Injection (UNION) | ⭐⭐⭐⭐ Expert |

---

## Instructor Walkthrough (Full Solutions)

---

### FLAG 1 — RECON_01
**Page:** `/` (Home)
**Category:** Source code / Browser Console
**Difficulty:** ⭐

**Method A — View Page Source:**

Right-click the page → *View Page Source* (or `Ctrl+U`).
Search for `Dev Notes`. You'll find:

```html
<!-- TechHaven Dev Notes — build 2.3.1
     RkxBRzE6IFJFQ09OXzAx -->
```

Decode the base64 string in the browser console:
```javascript
atob("RkxBRzE6IFJFQ09OXzAx")
// → "FLAG1: RECON_01"
```

**Method B — Browser Console:**

Open DevTools → Console tab. On page load the app automatically prints:

```
[TechHaven Debug] RkxBRzE6IFJFQ09OXzAx
Hint: this looks encoded. Try atob("RkxBRzE6IFJFQ09OXzAx") in the console.
```

Run `atob("RkxBRzE6IFJFQ09OXzAx")` → `"FLAG1: RECON_01"`

**Submit:** `RECON_01`

---

### FLAG 2 — SHADOW_02
**Page:** `/products` (Products)
**Category:** DOM Inspection
**Difficulty:** ⭐

Navigate to the Products page. Open DevTools → Elements tab (or right-click → Inspect).
Search for `__debug` (Ctrl+F in the Elements panel).
You'll find a hidden `<div>` invisible to the eye:

```html
<div id="__debug" style="display:none;" aria-hidden="true">
    RkxBRzI6IFNIQURPV18wMg==
</div>
```

Decode in console:
```javascript
atob("RkxBRzI6IFNIQURPV18wMg==")
// → "FLAG2: SHADOW_02"
```

**Submit:** `SHADOW_02`

---

### FLAG 3 — HEADER_03
**Page:** `/about` (About)
**Category:** HTTP Response Headers
**Difficulty:** ⭐⭐

Navigate to the About page.
Open DevTools → **Network** tab.
Reload the page (`F5`) so the initial HTTP request is captured.
Click on the `about` document request in the list.
Open the **Response Headers** section. Look for:

```
X-Debug-Token: FLAG3: HEADER_03
```

This header is injected server-side by middleware only on `/about` requests.

**Submit:** `HEADER_03`

---

### FLAG 4 — WAREHOUSE_04
**Page:** `/hidden-warehouse`
**Category:** Reconnaissance / robots.txt
**Difficulty:** ⭐⭐

Browse to `http://<host>/robots.txt`. You'll see:

```
User-agent: *
Disallow: /admin
Disallow: /hidden-warehouse
Disallow: /vip
```

`robots.txt` is intended to tell search engines what NOT to index — but it's publicly readable and effectively a map of "interesting" URLs.

Navigate to `/hidden-warehouse`. The flag is displayed on the page.

**Teaching moment:** `robots.txt` is *not* a security mechanism. It is a courtesy to crawlers, nothing more.

**Submit:** `WAREHOUSE_04`

---

### FLAG 5 — COOKIE_05
**Page:** `/vip`
**Category:** Cookie Manipulation
**Difficulty:** ⭐⭐

When you visit the Home page, the app sets a cookie:
```
customer_tier=standard
```

The VIP page (`/vip`) reads this cookie and gates access based on its value.

**Steps:**
1. Visit `/vip` — you see "VIP Members Only".
2. Open DevTools → **Application** tab → **Cookies** → select the site.
3. Find `customer_tier`, double-click the value, change it from `standard` to `vip`.
4. **Reload** the page (`F5`).
5. The flag is now displayed.

**Teaching moment:** Client-side cookies with no signature or server-side validation can be trivially forged.

**Submit:** `COOKIE_05`

---

### FLAG 6 — SQLINJECT_06
**Page:** `/login` → `/admin`
**Category:** SQL Injection — Authentication Bypass
**Difficulty:** ⭐⭐⭐

The login page passes user input directly into a SQL string:

```sql
SELECT username, role FROM users
WHERE username = '[INPUT]' AND password = '[INPUT]'
```

**Bypass payloads:**

| Username | Password | What happens |
|----------|----------|-------------|
| `admin' --` | *(anything)* | Comments out the password check. Logs in as `admin`. |
| `' OR '1'='1' --` | *(anything)* | Always-true condition. Returns first user in table. |

After successful injection, the app redirects to `/admin` where the flag is displayed in the terminal-style admin panel.

**The vulnerable code** (in `DatabaseService.cs`):
```csharp
// VULNERABLE
cmd.CommandText = $"SELECT username, role FROM users WHERE username = '{username}' AND password = '{password}'";

// PATCHED (parameterized query):
// cmd.CommandText = "SELECT username, role FROM users WHERE username = @u AND password = @p";
// cmd.Parameters.AddWithValue("@u", username);
// cmd.Parameters.AddWithValue("@p", password);
```

**Submit:** `SQLINJECT_06`

---

### FLAG 7 — UNION_07
**Page:** `/products` (Products search)
**Category:** SQL Injection — UNION-based data extraction
**Difficulty:** ⭐⭐⭐⭐

The search bar on the Products page is also vulnerable. The underlying query is:

```sql
SELECT id, name, description, price, category FROM products
WHERE name LIKE '%[INPUT]%'
```

This hint is deliberately left in the HTML source of the Products page as a developer comment — students who check the source will find the exact column structure.

**Step 1 — Figure out column count:**
Try injecting `' UNION SELECT 1,2,3,4,5 --` and see if the page returns results without errors (it should).

**Step 2 — Extract the flags table:**

Paste this into the search bar:
```
' UNION SELECT id, flag_value, hint, 0.0, flag_number FROM flags --
```

A "product" card appears with:
- **Name:** `UNION_07`
- **Description:** `Congratulations! You extracted the flags table via UNION-based SQL injection...`
- **Price:** `0.00 kr`
- **Category:** `FLAG7`

**The vulnerable code** (in `DatabaseService.cs`):
```csharp
// VULNERABLE
cmd.CommandText = $"SELECT id, name, description, price, category FROM products WHERE name LIKE '%{searchTerm}%'";

// PATCHED:
// cmd.CommandText = "SELECT id, name, description, price, category FROM products WHERE name LIKE @search";
// cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
```

**Submit:** `UNION_07`

---

## Resetting Between Sessions

To wipe all student submissions and start fresh:

```bash
# Stop the app, then:
del bin\Debug\net10.0\techhaven.db   # Windows
rm bin/Debug/net10.0/techhaven.db    # Linux/macOS
# Restart — DB is recreated and re-seeded automatically
```
