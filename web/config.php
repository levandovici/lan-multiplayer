<?php
// Load environment variables
$autoloadPath = __DIR__ . '/vendor/autoload.php';
$dotenvLoaded = false;

if (file_exists($autoloadPath)) {
    require_once $autoloadPath;
    if (class_exists('Dotenv\Dotenv') && file_exists(__DIR__ . '/.env')) {
        try {
            $dotenv = Dotenv\Dotenv::createImmutable(__DIR__);
            $dotenv->safeLoad();
            $dotenvLoaded = true;
        } catch (Throwable $e) {
            error_log('Dotenv load failed: ' . $e->getMessage());
        }
    }
}

// Manual .env parsing as fallback
if (!$dotenvLoaded && file_exists(__DIR__ . '/.env')) {
    $lines = file(__DIR__ . '/.env', FILE_IGNORE_NEW_LINES | FILE_SKIP_EMPTY_LINES);
    foreach ($lines as $line) {
        if (strpos(trim($line), '#') === 0) {
            continue;
        }
        list($name, $value) = explode('=', $line, 2);
        $name = trim($name);
        $value = trim($value);
        if (!array_key_exists($name, $_ENV)) {
            $_ENV[$name] = $value;
            putenv("$name=$value");
        }
    }
}

function envValue($key, $default = null) {
    if (isset($_ENV[$key]) && $_ENV[$key] !== '') {
        return $_ENV[$key];
    }

    if (isset($_SERVER[$key]) && $_SERVER[$key] !== '') {
        return $_SERVER[$key];
    }

    $value = getenv($key);
    return $value !== false && $value !== '' ? $value : $default;
}

// Database Configuration
define('DB_HOST', envValue('DB_HOST', 'localhost'));
define('DB_NAME', envValue('DB_NAME', 'lan_multiplayer'));
define('DB_USER', envValue('DB_USER', 'root'));
define('DB_PASS', envValue('DB_PASS', ''));

// Email Configuration
define('SMTP_HOST', envValue('SMTP_HOST', 'smtp.hostinger.com'));
define('SMTP_PORT', envValue('SMTP_PORT', 587));
define('SMTP_SECURE', envValue('SMTP_SECURE', 'tls'));
define('SMTP_USER', envValue('SMTP_USER', ''));
define('SMTP_PASS', envValue('SMTP_PASS', ''));
define('FROM_EMAIL', envValue('FROM_EMAIL', 'support@michitai.com'));
define('FROM_NAME', envValue('FROM_NAME', 'Lan Multiplayer'));

// Site Configuration
define('CODE_EXPIRY_MINUTES', envValue('CODE_EXPIRY_MINUTES', 15));

// Environment
define('APP_ENV', envValue('APP_ENV', 'production'));

// Error Reporting
if (APP_ENV === 'development') {
    error_reporting(E_ALL);
    ini_set('display_errors', 1);
} else {
    error_reporting(0);
    ini_set('display_errors', 0);
}

// Start Session
if (session_status() === PHP_SESSION_NONE) {
    session_start();
}

// Database Connection
function getDbConnection() {
    try {
        $dsn = "mysql:host=" . DB_HOST . ";dbname=" . DB_NAME . ";charset=utf8mb4";
        $options = [
            PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
            PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
            PDO::ATTR_EMULATE_PREPARES => false,
        ];
        return new PDO($dsn, DB_USER, DB_PASS, $options);
    } catch (PDOException $e) {
        die("Database connection failed: " . $e->getMessage());
    }
}
?>
