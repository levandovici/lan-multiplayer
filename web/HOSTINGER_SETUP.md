# Hostinger Deployment Instructions

## Fixing 500 Error

The 500 error is caused by missing `vendor` folder. On Hostinger shared hosting, you have two options:

### Option 1: Upload vendor folder (Recommended)

1. On your local machine, run:
   ```bash
   composer install
   ```

2. Upload the entire `vendor` folder to Hostinger via FTP/File Manager

3. Upload `.env` file and configure it with your Hostinger database credentials:
   ```
   DB_HOST=localhost
   DB_NAME=your_hostinger_db_name
   DB_USER=your_hostinger_db_user
   DB_PASS=your_hostinger_db_password
   SMTP_HOST=smtp.gmail.com
   SMTP_PORT=587
   SMTP_USER=your-email@gmail.com
   SMTP_PASS=your-app-password
   FROM_EMAIL=noreply@lan.michitai.com
   FROM_NAME=Lan Multiplayer
   CODE_EXPIRY_MINUTES=15
   APP_ENV=production
   ```

### Option 2: Use Hostinger SSH (Advanced)

1. Enable SSH in Hostinger hPanel
2. Connect via SSH
3. Navigate to your public_html folder
4. Run:
   ```bash
   composer install
   ```

## Database Setup on Hostinger

1. Create a new MySQL database in Hostinger hPanel
2. Import `database.sql` using phpMyAdmin
3. Update `.env` with the database credentials from hPanel

## File Permissions

Set these permissions via FTP/File Manager:
- PHP files: 644
- .env file: 600
- Folders: 755

## Troubleshooting

If you still get 500 error:
1. Check Hostinger error logs in hPanel
2. Ensure PHP version is 7.4 or higher
3. Verify .htaccess is compatible (Hostinger uses Apache)
4. Make sure all files are uploaded in the correct directory (public_html)
