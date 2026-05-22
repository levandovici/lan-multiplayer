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
    
    try {
        $db = getDbConnection();
        
        $stmt = $db->prepare("SELECT id, verification_code, code_expires FROM users WHERE email = ?");
        $stmt->execute([$email]);
        $user = $stmt->fetch();
        
        if (!$user) {
            $message = 'Email not found. Please try again.';
            $messageType = 'error';
        } elseif ($user['verification_code'] !== $code) {
            $message = 'Invalid verification code.';
            $messageType = 'error';
        } elseif (strtotime($user['code_expires']) < time()) {
            $message = 'Verification code has expired. Please request a new one.';
            $messageType = 'error';
        } else {
            // Mark as verified
            $stmt = $db->prepare("UPDATE users SET is_verified = 1 WHERE email = ?");
            $stmt->execute([$email]);
            
            $_SESSION['verified'] = true;
            $_SESSION['user_id'] = $user['id'];
            
            header('Location: downloads.php');
            exit;
        }
    } catch (Exception $e) {
        $message = 'Database error. Please try again.';
        $messageType = 'error';
    }
}
?>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Verify Email - Lan Multiplayer</title>
    <link rel="stylesheet" href="css/style.css">
</head>
<body>
    <header>
        <div class="container">
            <nav>
                <a href="index.php" class="logo">Lan Multiplayer</a>
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
