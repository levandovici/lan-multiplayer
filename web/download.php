<?php
require_once 'config.php';
require_once 'seo.php';

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
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=5.0, viewport-fit=cover">
    <?php seoHead([
        'title' => 'Download Lan Multiplayer - Free .NET & Unity LAN Library',
        'description' => 'Download the free MIT-0 Lan Multiplayer library for .NET Framework 4.8 or Unity. Verify your email and get the package — free for commercial use, no attribution required.',
        'path' => '/download.php',
        'keywords' => 'download LAN multiplayer library, Unity LAN library download, .NET multiplayer library free, Michitai Lan download',
    ]); ?>
    <link rel="stylesheet" href="css/style.css?v=<?php echo CSS_VERSION; ?>">
</head>
<body>
    <header>
        <div class="container">
            <nav>
                <a href="index.php" class="logo"><img src="logo.png" alt="Lan Multiplayer"><span>Lan Multiplayer</span></a>
                <ul class="nav-links">
                    <li><a href="index.php">Home</a></li>
                    <li><a href="about.php">About</a></li>
                    <li><a href="docs.php">Docs</a></li>
                    <li><a href="faq.php">FAQ</a></li>
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

            <section class="what-is">
                <div class="section-header">
                    <h2>What you get</h2>
                    <p>The download is a single package with the complete Lan Multiplayer source code — no installer, no account wall beyond a one-time email verification, and no telemetry. Drop the sources into your project and you own the code.</p>
                </div>
                <div class="features-grid">
                    <div class="feature-card">
                        <h3>.NET library</h3>
                        <p>The full C# implementation for .NET Framework 4.8 and modern .NET, including the UDP broadcast discovery client, the reliable length-prefixed TCP channel, the unreliable UDP state channel, GZip compression and the terminal command system.</p>
                    </div>
                    <div class="feature-card">
                        <h3>Unity package</h3>
                        <p>The same engine adapted for Unity projects, so Windows, Linux, macOS, Android and iOS builds share one networking codebase. Copy the scripts into your Assets folder and the API is identical to the .NET version.</p>
                    </div>
                    <div class="feature-card">
                        <h3>C++ sources</h3>
                        <p>Native C++ sources for engines and platforms without a .NET runtime, plus documentation and copy-paste examples for the common patterns: discovery, starting a server, connecting a client and syncing per-frame state at 30-60 Hz.</p>
                    </div>
                </div>
            </section>

            <section class="license-info">
                <h3>How verification works</h3>
                <p>Enter your email above and we send you a short verification code. The code proves the address is real and lets us notify you about breaking changes and new releases — nothing else. After verification the download starts immediately and the library is yours under the MIT-0 license: free for personal, educational and commercial use, with no attribution required. If the message does not arrive within a minute, check the spam folder or request a new code. Once you have the package, the <a href="docs.php">documentation</a> walks through discovery, server setup, client connection and per-frame state sync, and the <a href="faq.php">FAQ</a> answers the common integration questions.</p>
            </section>
        </div>
    </main>

    <footer>
        <div class="container">
            <p>&copy; 2026 Nichita Levandovici. Released under MIT-0.</p>
            <p>
                <a href="privacy.php">Privacy Policy</a> | 
                <a href="terms.php">Terms and Conditions</a> | 
                <a href="about.php">About Us</a>
            </p>
        </div>
    </footer>
</body>
</html>
