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
    <meta http-equiv="Cache-Control" content="no-store, no-cache, must-revalidate, max-age=0">
    <meta http-equiv="Pragma" content="no-cache">
    <meta http-equiv="Expires" content="Wed, 11 Jan 1984 05:00:00 GMT">
    <title>Download Lan Multiplayer</title>
    <link rel="stylesheet" href="css/style.css?v=<?php echo CSS_VERSION; ?>">
</head>
<body>
    <header>
        <div class="container">
            <nav>
                <a href="index.php" class="logo"><img src="logo.png" alt="Lan Multiplayer"></a>
                <ul class="nav-links">
                    <li><a href="index.php">Home</a></li>
                    <li><a href="about.php">About</a></li>
                    <li><a href="docs.php">Docs</a></li>
                    <li><a href="privacy.php">Privacy</a></li>
                    <li><a href="terms.php">Terms</a></li>
                </ul>
            </nav>
        </div>
    </header>

    <main>
        <div class="container">
            <section class="hero">
                <div class="badge">MIT-0 License — Free for any use</div>
                <h1>Choose Your Version</h1>
                <p>Select the version that matches your development environment</p>
            </section>

            <div class="downloads-container">
                <h2>Download Lan Multiplayer</h2>

                <div class="alert alert-warning">
                    <strong>What's new:</strong> this release ships the rewritten transport — binary length-prefixed framing, GZip compression, and an unreliable UDP channel for 30-60 FPS state sync. It is <strong>not compatible</strong> with older builds; update all clients and servers together.
                </div>

                <div class="download-buttons">
                    <div class="download-btn">
                        <a href="serve_download.php?version=dotnet" class="btn">
                            Download .NET Version
                        </a>
                        <p>
                            <code>lan-dotnet.zip</code> — full C# source for .NET Framework 4.8+ projects
                        </p>
                    </div>

                    <div class="download-btn">
                        <a href="serve_download.php?version=unity" class="btn">
                            Download Unity Version
                        </a>
                        <p>
                            <code>lan-unity.zip</code> — drop-in C# source for Unity (Assets folder), incl. mobile broadcast helpers
                        </p>
                    </div>
                </div>

                <div class="license-info">
                    <h3>Open License</h3>
                    <p><strong>Lan Multiplayer is released under the MIT No Attribution (MIT-0) license.</strong></p>
                    <p>You may use, modify, and distribute it in personal or commercial projects without payment or attribution.</p>
                    <p>See the <a href="terms.php">Terms and Conditions</a> and the repository <a href="https://github.com/levandovici/lan-multiplayer/blob/master/LICENSE">LICENSE</a> for the full text.</p>
                </div>
            </div>
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
