<?php
require_once 'config.php';

if (!isset($_SESSION['email'])) {
    header('Location: download.php');
    exit;
}

$email = $_SESSION['email'];
$message = '';
$messageType = '';

if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['code'])) {
    $code = trim($_POST['code']);
    $ip = $_SERVER['REMOTE_ADDR'] ?? '';
    
    // Rate limiting: Max 5 attempts per 15 minutes per IP
    try {
        $db = getDbConnection();
        
        // Check for rate limiting
        $stmt = $db->prepare("SELECT COUNT(*) as attempts FROM failed_attempts WHERE ip_address = ? AND attempt_time > DATE_SUB(NOW(), INTERVAL 15 MINUTE)");
        $stmt->execute([$ip]);
        $attempts = $stmt->fetch()['attempts'];
        
        if ($attempts >= 5) {
            $message = 'Too many failed attempts. Please wait 15 minutes before trying again.';
            $messageType = 'error';
        } else {
            // Check for rate limiting per email
            $stmt = $db->prepare("SELECT COUNT(*) as attempts FROM failed_attempts WHERE email = ? AND attempt_time > DATE_SUB(NOW(), INTERVAL 15 MINUTE)");
            $stmt->execute([$email]);
            $emailAttempts = $stmt->fetch()['attempts'];
            
            if ($emailAttempts >= 5) {
                $message = 'Too many failed attempts for this email. Please wait 15 minutes before trying again.';
                $messageType = 'error';
            } else {
                $stmt = $db->prepare("SELECT id, verification_code, code_expires FROM users WHERE email = ?");
                $stmt->execute([$email]);
                $user = $stmt->fetch();
                
                if (!$user) {
                    $message = 'Email not found. Please try again.';
                    $messageType = 'error';
                    logFailedAttempt($db, $email, $ip);
                } elseif ($user['verification_code'] !== $code) {
                    // Add delay to slow down brute force
                    sleep(min($attempts, 2));
                    
                    $message = 'Invalid verification code.';
                    $messageType = 'error';
                    logFailedAttempt($db, $email, $ip);
                } elseif (strtotime($user['code_expires']) < time()) {
                    $message = 'Verification code has expired. Please request a new one.';
                    $messageType = 'error';
                    logFailedAttempt($db, $email, $ip);
                } else {
                    // Mark as verified and clear failed attempts
                    $stmt = $db->prepare("UPDATE users SET is_verified = 1 WHERE email = ?");
                    $stmt->execute([$email]);
                    
                    // Clear failed attempts for this email/IP on successful verification
                    $stmt = $db->prepare("DELETE FROM failed_attempts WHERE email = ? OR ip_address = ?");
                    $stmt->execute([$email, $ip]);
                    
                    $_SESSION['verified'] = true;
                    $_SESSION['user_id'] = $user['id'];
                    
                    header('Location: downloads.php');
                    exit;
                }
            }
        }
    } catch (Exception $e) {
        $message = 'Database error. Please try again.';
        $messageType = 'error';
        error_log('Verification error: ' . $e->getMessage());
    }
}

function logFailedAttempt($db, $email, $ip) {
    try {
        $stmt = $db->prepare("INSERT INTO failed_attempts (email, ip_address) VALUES (?, ?)");
        $stmt->execute([$email, $ip]);
    } catch (Exception $e) {
        error_log('Failed to log attempt: ' . $e->getMessage());
    }
}
?>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Verify Email - Lan Multiplayer</title>
    <link rel="stylesheet" href="css/style.css?v=1.1">
</head>
<body>
    <header>
        <div class="container">
            <nav>
                <a href="index.php" class="logo"><img src="logo.png" alt="Lan Multiplayer"></a>
                <ul class="nav-links">
                    <li><a href="index.php">Home</a></li>
                    <li><a href="about.php">About</a></li>
                    <li><a href="privacy.php">Privacy</a></li>
                    <li><a href="terms.php">Terms</a></li>
                </ul>
            </nav>
        </div>
    </header>

    <main>
        <div class="container">
            <section class="hero">
                <h1>Verify Your Email</h1>
                <p>Enter the 6-digit code sent to <?php echo htmlspecialchars($email); ?></p>
            </section>

            <div class="form-container">
                <?php if ($message): ?>
                    <div class="alert alert-<?php echo $messageType; ?>">
                        <?php echo htmlspecialchars($message); ?>
                    </div>
                <?php endif; ?>

                <?php if (APP_ENV === 'development' && isset($_SESSION['dev_code'])): ?>
                    <div class="alert alert-info" style="background-color: #d1ecf1; border-color: #bee5eb; color: #0c5460; padding: 1rem; margin-bottom: 1rem; border-radius: 4px;">
                        <strong>Development Mode:</strong> Your verification code is <strong><?php echo htmlspecialchars($_SESSION['dev_code']); ?></strong>
                    </div>
                <?php endif; ?>

                <form method="POST" action="">
                    <div class="form-group">
                        <label for="code">Verification Code</label>
                        <input type="text" id="code" name="code" required placeholder="123456" maxlength="6" pattern="[0-9]{6}" style="letter-spacing: 0.5em; text-align: center; font-size: 1.5rem;">
                    </div>
                    <button type="submit" class="btn">Verify Code</button>
                </form>
                
                <p style="margin-top: 1rem; text-align: center;">
                    <a href="download.php" style="color: #667eea;">Back to email entry</a>
                </p>
            </div>
        </div>
    </main>

    <footer>
        <div class="container">
            <p>&copy; 2026 Nichita Levandovici. All rights reserved.</p>
            <p>
                <a href="privacy.php">Privacy Policy</a> | 
                <a href="terms.php">Terms and Conditions</a> | 
                <a href="about.php">About Us</a>
            </p>
        </div>
    </footer>
</body>
</html>
