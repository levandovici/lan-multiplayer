<?php
require_once 'config.php';

header('Content-Type: application/json');

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    echo json_encode(['success' => false, 'message' => 'Invalid request method']);
    exit;
}

$data = json_decode(file_get_contents('php://input'), true);
$email = filter_var($data['email'] ?? '', FILTER_SANITIZE_EMAIL);
$code = trim($data['code'] ?? '');

if (!filter_var($email, FILTER_VALIDATE_EMAIL)) {
    echo json_encode(['success' => false, 'message' => 'Invalid email address']);
    exit;
}

if (!preg_match('/^[0-9]{6}$/', $code)) {
    echo json_encode(['success' => false, 'message' => 'Invalid code format']);
    exit;
}

try {
    $db = getDbConnection();
    
    $stmt = $db->prepare("SELECT id, verification_code, code_expires FROM users WHERE email = ?");
    $stmt->execute([$email]);
    $user = $stmt->fetch();
    
    if (!$user) {
        echo json_encode(['success' => false, 'message' => 'Email not found']);
        exit;
    }
    
    if ($user['verification_code'] !== $code) {
        echo json_encode(['success' => false, 'message' => 'Invalid verification code']);
        exit;
    }
    
    if (strtotime($user['code_expires']) < time()) {
        echo json_encode(['success' => false, 'message' => 'Verification code has expired']);
        exit;
    }
    
    // Mark as verified
    $stmt = $db->prepare("UPDATE users SET is_verified = 1 WHERE email = ?");
    $stmt->execute([$email]);
    
    echo json_encode(['success' => true, 'message' => 'Email verified successfully']);
    
} catch (Exception $e) {
    echo json_encode(['success' => false, 'message' => 'Database error: ' . $e->getMessage()]);
}
?>
