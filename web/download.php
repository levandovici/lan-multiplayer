<?php
require_once 'config.php';

$message = '';
$messageType = '';

if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['email'])) {
    $email = filter_var($_POST['email'], FILTER_SANITIZE_EMAIL);
    $ip = $_SERVER['REMOTE_ADDR'] ?? '';
    
    if (filter_var($email, FILTER_VALIDATE_EMAIL)) {
        try {
            $db = getDbConnection();
            
            // Rate limiting: Max 3 email requests per hour per IP
            $stmt = $db->prepare("SELECT COUNT(*) as requests FROM users WHERE ip_address = ? AND created_at > DATE_SUB(NOW(), INTERVAL 1 HOUR)");
            $stmt->execute([$ip]);
            $requests = $stmt->fetch()['requests'];
            
            if ($requests >= 3) {
                $message = 'Too many email requests. Please wait 1 hour before trying again.';
                $messageType = 'error';
            } else {
                // Rate limiting: Max 3 email requests per hour per email
                $stmt = $db->prepare("SELECT COUNT(*) as requests FROM users WHERE email = ? AND created_at > DATE_SUB(NOW(), INTERVAL 1 HOUR)");
                $stmt->execute([$email]);
                $emailRequests = $stmt->fetch()['requests'];
                
                if ($emailRequests >= 3) {
                    $message = 'Too many requests for this email. Please wait 1 hour before trying again.';
                    $messageType = 'error';
                } else {
                    // Generate 6-digit verification code
                    $code = str_pad(random_int(0, 999999), 6, '0', STR_PAD_LEFT);
                    $expires = date('Y-m-d H:i:s', strtotime('+' . CODE_EXPIRY_MINUTES . ' minutes'));
                    
                    // Check if email already exists
                    $stmt = $db->prepare("SELECT id FROM users WHERE email = ?");
                    $stmt->execute([$email]);
                    $existing = $stmt->fetch();
                    
                    if ($existing) {
                        // Update existing user
                        $stmt = $db->prepare("UPDATE users SET verification_code = ?, code_expires = ?, is_verified = 0, ip_address = ? WHERE email = ?");
                        $stmt->execute([$code, $expires, $ip, $email]);
                    } else {
                        // Insert new user
                        $stmt = $db->prepare("INSERT INTO users (email, verification_code, code_expires, ip_address) VALUES (?, ?, ?, ?)");
                        $stmt->execute([$email, $code, $expires, $ip]);
                    }
                    
                    // Send email
                    if (!class_exists('PHPMailer\PHPMailer\PHPMailer')) {
                        throw new RuntimeException('Email service is not installed. Run composer install and upload the vendor folder.');
                    }

                    $mail = new \PHPMailer\PHPMailer\PHPMailer(true);
                    $mail->isSMTP();
                    $mail->Host = SMTP_HOST;
                    $mail->SMTPAuth = true;
                    $mail->Username = SMTP_USER;
                    $mail->Password = SMTP_PASS;
                    $mail->Port = (int) SMTP_PORT;
                    $mail->SMTPSecure = SMTP_SECURE;
                    $mail->CharSet = 'UTF-8';
                    $mail->setFrom(FROM_EMAIL, FROM_NAME);
                    $mail->addAddress($email);
                    $mail->Subject = 'Your Lan Multiplayer Verification Code';
                    $mail->Body = "Your verification code is: $code\n\nThis code will expire in " . CODE_EXPIRY_MINUTES . " minutes.\n\nIf you didn't request this code, please ignore this email.";
                    $mail->send();
                    
                    // For development, log the code
                    error_log("Verification code for $email: $code");
                    
                    $_SESSION['email'] = $email;
                    
                    if (APP_ENV === 'development') {
                        $_SESSION['dev_code'] = $code;
                    }
                    
                    header('Location: verify.php');
                    exit;
                }
            }
        } catch (Exception $e) {
            error_log('Verification code request failed: ' . $e->getMessage());
            $message = 'Failed to send verification code: ' . $e->getMessage();
            $messageType = 'error';
        }
    } else {
        $message = 'Please enter a valid email address.';
        $messageType = 'error';
    }
}
?>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Download Lan Multiplayer</title>
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
                <h1>Get Lan Multiplayer</h1>
                <p>Enter your email to receive a verification code</p>
            </section>

            <div class="form-container">
                <?php if ($message): ?>
                    <div class="alert alert-<?php echo $messageType; ?>">
                        <?php echo htmlspecialchars($message); ?>
                    </div>
                <?php endif; ?>

                <form method="POST" action="">
                    <div class="form-group">
                        <label for="email">Email Address</label>
                        <input type="email" id="email" name="email" required placeholder="your@email.com">
                    </div>
                    <button type="submit" class="btn">Send Verification Code</button>
                </form>
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
