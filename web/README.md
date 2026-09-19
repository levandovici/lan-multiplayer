# Lan Multiplayer Website

PHP/MySQL website that gates library downloads behind email verification.

## Requirements

- PHP 7.4 or higher
- MySQL 5.7+ / MariaDB 10.2+
- Composer (`vlucas/phpdotenv`, `phpmailer/phpmailer`)

## Setup

1. Import the schema:

   ```bash
   mysql -u user -p < database.sql
   ```

   Creates `users` (email, verification code, expiry, verified flag),
   `downloads` (download tracking), and `failed_attempts` (brute-force log).

2. Copy `.env.example` to `.env` and fill in DB credentials, SMTP settings
   (`SMTP_HOST`, `SMTP_PORT`, `SMTP_USER`, `SMTP_PASS`, `FROM_EMAIL`), and
   `APP_ENV`. **Never commit `.env`** — it's gitignored.

3. `composer install` and upload the `vendor/` folder.

4. Upload the downloadable packages to `downloads/` (see below).

## How the download flow works

```
download.php          verify.php            downloads.php        serve_download.php
 email form    --->    code entry    --->    version picker  --->   file stream
```

1. **`download.php`** — user submits an email. Rate limits: max 3 code
   requests/hour per IP *and* per email (counted from `users.created_at`).
   A random 6-digit code + expiry (`CODE_EXPIRY_MINUTES`) is stored in
   `users` and emailed via PHPMailer/SMTP. Redirects to `verify.php`.

2. **`verify.php`** — user enters the code. Brute-force limits: max 5
   failed attempts/15 min per IP *and* per email (`failed_attempts` table),
   plus a progressive `sleep()` delay on wrong codes. On success:
   `is_verified=1`, failed attempts cleared, `$_SESSION['verified']` and
   `$_SESSION['user_id']` set. Redirects to `downloads.php`.

3. **`downloads.php`** — requires `$_SESSION['verified']`. Two buttons:
   `serve_download.php?version=dotnet` and `?version=unity`.

4. **`serve_download.php`** — re-checks the session, maps the version to a
   file, logs a row in `downloads` (`user_id`, `version_type`), then
   streams the zip via `readfile()` with `Content-Disposition: attachment`.

### Which file is downloaded

| `version` param | File served | Contents |
|---|---|---|
| `dotnet` | `downloads/lan-dotnet.zip` | Full C# source of the `dotnet/` library (.NET Framework 4.8+) |
| `unity` | `downloads/lan-unity.zip` | Drop-in C# source of the `unity/` library (copy into `Assets/`, incl. mobile broadcast helpers) |

The zips are **not in this repository** — `downloads/` only contains
`.htaccess`/`index.php` blocking direct access. Build them from the repo
roots (`dotnet/`, `unity/`) and upload to the server. **Rebuild them after
every library change** — the current release uses a new binary frame
protocol that cannot interoperate with older builds.

### JSON API variants

`send_code.php` and `verify_code.php` accept JSON POSTs (email, code) and
return JSON. Note: `verify_code.php` sets `is_verified` in the DB but does
**not** create the verified session, so it cannot unlock `downloads.php` —
the HTML flow (`download.php` → `verify.php`) is the working path.

## Deployment notes

- `APP_ENV=development` echoes the verification code into the session /
  JSON `dev_code` field and into `error_log` — set `APP_ENV=production`
  before going live.
- `.htaccess` hardens the site; `downloads/.htaccess` blocks direct file
  access so zips are only reachable through `serve_download.php`.
- Use HTTPS in production.
- Recommended permissions: 644 for PHP files, 600 for `.env`.
- See `HOSTINGER_SETUP.md` for Hostinger-specific steps.

## Licensing shown on the website

- **License**: MIT No Attribution (MIT-0)
- **Free**: commercial and non-commercial use with no attribution required
- **No payment required**

Contact: support@michitai.com
