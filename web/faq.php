<?php
define('LAN_PUBLIC_PAGE', true);
require_once 'config.php';
require_once 'seo.php';

$faqs = [
    [
        'q' => 'What is Lan Multiplayer?',
        'a' => 'Lan Multiplayer (Michitai.Lan) is a free, open-source networking library that adds local-area-network multiplayer to games. It provides automatic server discovery over UDP broadcast, a reliable TCP channel for commands and login, and an unreliable UDP channel for 30-60 FPS state synchronization — with no external servers required.',
    ],
    [
        'q' => 'Is Lan Multiplayer really free?',
        'a' => 'Yes. The library is released under the MIT No Attribution (MIT-0) license. You can use it in personal, educational and commercial projects without paying anything and without giving credit.',
    ],
    [
        'q' => 'Which engines and platforms does it support?',
        'a' => 'There are three wire-compatible builds: a .NET Framework 4.8 class library, a drop-in Unity source package, and a standalone C++20 CMake static library. Supported platforms include Windows, Linux, macOS, Android and iOS, selected via EPlatform flags.',
    ],
    [
        'q' => 'How does LAN server discovery work?',
        'a' => 'Servers run a BroadcastServer that answers UDP broadcast probes on ports 60000-60128. Clients use BroadcastClient to scan the local network; each discovered server replies with its name and TCP port, so players see a live server list without typing IP addresses.',
    ],
    [
        'q' => 'When should I use the TCP channel versus the UDP channel?',
        'a' => 'Use the reliable TCP channel (Client.Request / Server.Response, ports 50000-50128) for anything that must arrive — login, commands, world data. Use the unreliable UDP channel (Client.SendState / Server.BroadcastState) for transient per-frame values like positions and inputs, where a dropped packet is immediately replaced by the next one.',
    ],
    [
        'q' => 'Does it work over the internet?',
        'a' => 'No — Lan Multiplayer is designed for local networks (same Wi-Fi or wired LAN). Discovery uses UDP broadcast, which does not cross routers or the internet. For internet multiplayer you need a hosted backend such as api.michitai.com.',
    ],
    [
        'q' => 'How do I download the library?',
        'a' => 'Go to the download page, enter your email address and verify the 6-digit code we send you. You can then download the .NET Framework package, the Unity package, or both — free.',
    ],
    [
        'q' => 'Can AI coding assistants use this library?',
        'a' => 'Yes. The llms-full.txt file on this site contains the complete API reference in a single text file sized for a model\'s context window. Paste its URL into any AI assistant and it can generate integration code for your engine.',
    ],
];

$faqSchema = [
    '@type' => 'FAQPage',
    'mainEntity' => array_map(fn($f) => [
        '@type' => 'Question',
        'name' => $f['q'],
        'acceptedAnswer' => ['@type' => 'Answer', 'text' => $f['a']],
    ], $faqs),
];
?>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=5.0, viewport-fit=cover">
    <?php seoHead([
        'title' => 'FAQ - Lan Multiplayer',
        'description' => 'Frequently asked questions about Lan Multiplayer: licensing, supported engines, UDP server discovery, TCP vs UDP channels, downloads and AI-assistant integration.',
        'path' => '/faq.php',
        'keywords' => 'LAN multiplayer FAQ, Michitai Lan questions, Unity LAN library FAQ, MIT-0 networking library, LAN server discovery',
        'jsonld' => [$faqSchema],
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
            <div class="content-page">
                <section class="doc-section">
                    <h1>Frequently Asked Questions</h1>
                    <p>Everything you need to know about the Lan Multiplayer library. For implementation details see the <a href="docs.php">documentation</a>.</p>
                </section>

                <?php foreach ($faqs as $faq): ?>
                <section class="doc-section">
                    <h3><?php echo htmlspecialchars($faq['q']); ?></h3>
                    <p><?php echo htmlspecialchars($faq['a']); ?></p>
                </section>
                <?php endforeach; ?>
            </div>
        </div>
    </main>

    <footer>
        <div class="container">
            <p>&copy; 2026 Nichita Levandovici. Released under MIT-0.</p>
            <p>
                <a href="about.php">About</a> |
                <a href="docs.php">Documentation</a> |
                <a href="download.php">Download</a> |
                <a href="privacy.php">Privacy Policy</a> |
                <a href="terms.php">Terms and Conditions</a>
            </p>
        </div>
    </footer>
</body>
</html>
