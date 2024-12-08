# SimpleDrive Deployment Documentation

## Prerequisites
- Compatible OS (Ubuntu, Windows, Mac, etc.).
- .NET SDK 8.0 installed.
- MySQL database installed and running.
- Git installed.

---

## Deployment Steps

### 1. Set Up the Project
1. Create a directory for the project.
    ```
    mkdir SimpleDrive
    cd SimpleDrive
    ```
2. Clone the repository from GitHub:
    ```
    git clone https://github.com/hasanayoub/SimpleDriveDotNet.git
    ```
3. Navigate to the project directory:
    ```
    cd SimpleDriveDotNet
    ```
4. Ensure .NET SDK 8.0 is installed, the following example for Unix OS:
    ```
    wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    sudo dpkg -i packages-microsoft-prod.deb
    rm packages-microsoft-prod.deb
    sudo apt install dotnet-sdk-8.0
    ```
---

### 2. Configure MySQL Database
1. Install MySQL 5 or later.
2. Create a MySQL database named `simple_drive`:
    ```
    mysql -u root -p
    CREATE DATABASE simple_drive;
    ```
---

### 3. Configure Application Settings
1. Copy the development settings file and modify it for the desired environment, for example here the environment is Testing:
    ```
    cp appsettings.Development.json appsettings.Testing.json
    ```
2. Edit the copied file, for example we use vim:
    ```
    vi appsettings.Testing.json
    ```

Update the following fields:

#### General Settings
- **StorageType**: Backend storage selection (e.g., `Ftp`, `Local`, `Database`, `S3`).

#### Authentication Settings
- **UserAuth.Username**: Username for the application.
- **UserAuth.HashedPassword**: BCrypt hashed password. Use https://bcrypt.online to generate it. Cost factor is 11

#### JWT Token Settings
- **JwtToken.TokenIssuer**: Issuer of the token.
- **JwtToken.JwtSecretKey**: Secret key for signing tokens.
- **JwtToken.TokenAudience**: Audience for the token.

#### S3 Storage Settings. Note: bucket name in SimpleDrive is always "files".
- **S3.BucketUrl**: URL of the S3 server.
- **S3.AccessKey**: Access key for S3.
- **S3.SecretKey**: Secret key for S3.
- **S3.Region**: Region of the S3 bucket.

#### FTP Storage Settings
- **FTP.FtpUrl**: URL of the FTP server.
- **FTP.FtpUsername**: FTP server username.
- **FTP.FtpPassword**: FTP server password.

#### File System Storage
- **FileSystem.StoragePath**: Path for local storage.

#### Database Settings
- **Database.Server**: Database server (e.g., `localhost`).
- **Database.DatabaseName**: Name of the database (e.g., `simple_drive`).
- **Database.User**: Database username.
- **Database.Password**: Database password.
- **Database.Rdbms**: MySQL
- **Database.RdbmsVersion**: Version of the RDBMS. You can get it using command "mysql --version". For example: "8.0.40"

#### Testing Configuration
- **Testing.FilePath**: File path for upload testing.
- **Testing.ServerUrl**: Backend server URL.
- **Testing.FileHashValue**: Hash value of the file in Testing.FilePath for testing. You may get the hash value from (https://emn178.github.io/online-tools/sha256_checksum.html).
- **Testing.Username**: Testing username.
- **Testing.Password**: Testing password.

---

### 4. Build and Run the Application
1. Run the migrations to set up the database schema:
    ```
    dotnet ef database update
    ```
2. Build the project:
    ```
    dotnet build
    ```
3. Run the application:
    ```
    dotnet run
    ```
---

### Notes
- Ensure all configurations are correctly set in `appsettings.Testing.json` for successful deployment and testing.
- Verify database connectivity and credentials before running the application.
