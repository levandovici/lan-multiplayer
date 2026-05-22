<?php
require_once 'config.php';

// Check if user is verified
if (!isset($_SESSION['verified']) || $_SESSION['verified'] !== true) {
    header('HTTP/1.0 403 Forbidden');
    exit('Access Denied. Please verify your email first.');
}

$version = $_GET['version'] ?? '';

// Validate version
if (!in_array($version, ['dotnet', 'unity'])) {
    header('HTTP/1.0 400 Bad Request');
    exit('Invalid download version.');
}

// Map version to filename
$files = [
    'dotnet' => 'lan-dotnet.zip',
    'unity' => 'lan-unity.zip'
];

$filename = $files[$version];
$filepath = __DIR__ . '/downloads/' . $filename;

// Check if file exists
if (!file_exists($filepath)) {
    header('HTTP/1.0 404 Not Found');
    exit('Download file not found.');
}

// Track download
try {
    $db = getDbConnection();
    $stmt = $db->prepare("INSERT INTO downloads (user_id, version_type) VALUES (?, ?)");
    $stmt->execute([$_SESSION['user_id'], $version]);
} catch (Exception $e) {
    error_log("Download tracking error: " . $e->getMessage());
}

// Serve the file
header('Content-Type: application/zip');
header('Content-Disposition: attachment; filename="' . $filename . '"');
header('Content-Length: ' . filesize($filepath));
header('Cache-Control: no-cache, must-revalidate');
header('Pragma: no-cache');
header('Expires: 0');

readfile($filepath);
exit;
?>
