<?php
require_once 'config.php';

header('Content-Type: application/json');

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    echo json_encode(['success' => false, 'message' => 'Invalid request method']);
    exit;
}

$data = json_decode(file_get_contents('php://input'), true);
$email = filter_var($data['email'] ?? '', FILTER_SANITIZE_EMAIL);

if (!filter_var($email, FILTER_VALIDATE_EMAIL)) {
    echo json_encode(['success' => false, 'message' => 'Invalid email address']);
    exit;
}

try {
    $db = getDbConnection();
    
    // Generate 6-digit verification code
    $code = str_pad(random_int(0, 999999), 6, '0', STR_PAD_LEFT);
    $expires = date('Y-m-d H:i:s', strtotime('+' . CODE_EXPIRY_MINUTES . ' minutes'));
    $ip = $_SERVER['REMOTE_ADDR'] ?? '';
    
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
    
    $response = [
        'success' => true, 
        'message' => 'Verification code sent to your email'
    ];

    if (APP_ENV === 'development') {
        $response['dev_code'] = $code;
    }

    echo json_encode($response);
    
} catch (Exception $e) {
    error_log('Verification code request failed: ' . $e->getMessage());
    echo json_encode(['success' => false, 'message' => $e->getMessage()]);
}
?>
