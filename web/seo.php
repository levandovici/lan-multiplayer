<?php
// Shared SEO head renderer — emits title, meta tags, Open Graph, Twitter Card,
// canonical URL, icons/manifest and JSON-LD structured data.
// Usage (inside the head element):
//   seoHead(['title' => ..., 'description' => ..., 'path' => '/about.php']);
// Requires config.php (SITE_URL, SITE_NAME).

function seoUrl(string $path = '/'): string {
    return SITE_URL . '/' . ltrim($path, '/');
}

// JSON-LD @graph shared by every page: WebSite + SoftwareApplication nodes.
function seoDefaultGraph(): array {
    return [
        [
            '@type' => 'WebSite',
            'name' => SITE_NAME,
            'alternateName' => 'Michitai.Lan',
            'url' => seoUrl('/'),
            'description' => 'Free LAN multiplayer networking library for game developers — .NET, Unity and C++.',
        ],
        [
            '@type' => 'SoftwareApplication',
            'name' => SITE_NAME,
            'alternateName' => 'Michitai.Lan',
            'url' => seoUrl('/'),
            'applicationCategory' => 'DeveloperApplication',
            'operatingSystem' => 'Windows, Linux, macOS, Android, iOS',
            'description' => 'Drop-in LAN multiplayer library: UDP server discovery, reliable TCP command channel and unreliable UDP state sync for .NET, Unity and C++ games.',
            'license' => 'https://github.com/levandovici/lan-multiplayer/blob/master/LICENSE',
            'featureList' => 'Automatic LAN server discovery (UDP broadcast), reliable length-prefixed TCP channel, unreliable UDP state channel for 30-60 FPS sync, GZip compression, command/terminal pattern, chat, cross-platform .NET, Unity and C++ builds',
            'isAccessibleForFree' => true,
            'author' => [
                '@type' => 'Person',
                'name' => 'Nichita Levandovici',
            ],
            'offers' => [
                '@type' => 'Offer',
                'price' => '0',
                'priceCurrency' => 'USD',
                'priceValidUntil' => '2030-12-31',
                'availability' => 'https://schema.org/InStock',
            ],
            'downloadUrl' => seoUrl('/download.php'),
        ],
    ];
}

function seoHead(array $opts): void {
    $title       = $opts['title'] ?? SITE_NAME;
    $description = $opts['description'] ?? 'Free MIT-0 LAN multiplayer networking library for .NET, Unity and C++ games — UDP discovery, TCP commands, UDP state sync.';
    $path        = $opts['path'] ?? '/';
    $keywords    = $opts['keywords'] ?? 'LAN multiplayer, local multiplayer library, Unity LAN, .NET networking, C++ multiplayer, UDP discovery, game networking, MIT-0, Michitai';
    $robots      = $opts['robots'] ?? 'index, follow, max-image-preview:large';
    $ogType      = $opts['og_type'] ?? 'website';
    $image       = $opts['image'] ?? seoUrl('logo.png');
    $canonical   = seoUrl($path);

    // Extra JSON-LD nodes appended to the shared WebSite/SoftwareApplication graph.
    $jsonld = $opts['jsonld'] ?? null;
    if ($jsonld === null) {
        $jsonld = ['@context' => 'https://schema.org', '@graph' => seoDefaultGraph()];
    } elseif (is_array($jsonld) && isset($jsonld['@graph'])) {
        $jsonld['@context'] = 'https://schema.org';
    } elseif (is_array($jsonld)) {
        $jsonld = ['@context' => 'https://schema.org', '@graph' => array_merge(seoDefaultGraph(), $jsonld)];
    }

    $e = fn(string $s): string => htmlspecialchars($s, ENT_QUOTES, 'UTF-8');
    ?>
    <title><?= $e($title) ?></title>
    <link rel="author" href="<?= seoUrl('humans.txt') ?>">
    <meta name="robots" content="<?= $e($robots) ?>">
    <meta name="keywords" content="<?= $e($keywords) ?>">
    <meta name="author" content="Nichita Levandovici">
    <meta name="description" content="<?= $e($description) ?>">
    <link rel="canonical" href="<?= $e($canonical) ?>">

    <!-- Open Graph Meta Tags -->
    <meta property="og:title" content="<?= $e($title) ?>">
    <meta property="og:description" content="<?= $e($description) ?>">
    <meta property="og:image" content="<?= $e($image) ?>">
    <meta property="og:url" content="<?= $e($canonical) ?>">
    <meta property="og:type" content="<?= $e($ogType) ?>">
    <meta property="og:site_name" content="<?= $e(SITE_NAME) ?>">

    <!-- Twitter Card Meta Tags -->
    <meta name="twitter:card" content="summary_large_image">
    <meta name="twitter:title" content="<?= $e($title) ?>">
    <meta name="twitter:description" content="<?= $e($description) ?>">
    <meta name="twitter:image" content="<?= $e($image) ?>">
    <meta name="twitter:site" content="@michitai">

    <!-- Structured data for search engines and AI assistants -->
    <script type="application/ld+json"><?= json_encode($jsonld, JSON_UNESCAPED_SLASHES | JSON_UNESCAPED_UNICODE | JSON_PRETTY_PRINT) ?></script>

    <link rel="icon" type="image/png" href="<?= seoUrl('logo.png') ?>">
    <link rel="apple-touch-icon" href="<?= seoUrl('logo.png') ?>">
    <link rel="manifest" href="<?= seoUrl('site.webmanifest') ?>">
    <meta name="theme-color" content="#facc15">
    <?php
}
