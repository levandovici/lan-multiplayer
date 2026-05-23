<?php
require_once 'config.php';

if (!isset($_SESSION['verified']) || $_SESSION['verified'] !== true) {
    header('Location: download.php');
    exit;
}

$userId = $_SESSION['user_id'];

// Track download if version is specified
if (isset($_GET['version']) && in_array($_GET['version'], ['dotnet', 'unity'])) {
    try {
        $db = getDbConnection();
        $stmt = $db->prepare("INSERT INTO downloads (user_id, version_type) VALUES (?, ?)");
        $stmt->execute([$userId, $_GET['version']]);
    } catch (Exception $e) {
        error_log("Download tracking error: " . $e->getMessage());
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
                <h1>Choose Your Version</h1>
                <p>Select the version that matches your development environment</p>
            </section>

            <div class="downloads-container">
                <h2>Download Lan Multiplayer</h2>
                
                <div class="download-buttons">
                    <div class="download-btn">
                        <a href="serve_download.php?version=dotnet" class="btn">
                            📦 Download .NET Version
                        </a>
                        <p style="margin-top: 1rem; color: #666;">
                            For .NET Framework 4.8+ projects
                        </p>
                    </div>
                    
                    <div class="download-btn">
                        <a href="serve_download.php?version=unity" class="btn">
                            🎮 Download Unity Version
                        </a>
                        <p style="margin-top: 1rem; color: #666;">
                            For Unity game engine projects
                        </p>
                    </div>
                </div>

                <div class="license-info">
                    <h3>📜 License Terms</h3>
                    <p><strong>Free for commercial use with attribution</strong> to lan.michitai.com</p>
                    <p>Simply include "Powered by Lan Multiplayer (lan.michitai.com)" in your game's credits or about screen.</p>
                    <hr style="margin: 1rem 0; border: none; border-top: 1px solid #ddd;">
                    <p><strong>No attribution required: €20 one-time payment per project</strong></p>
                    <p>One-time payment to remove the attribution requirement. Contact us at support@michitai.com for details.</p>
                </div>
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
