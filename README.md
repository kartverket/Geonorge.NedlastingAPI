# Geonorge NedlastingAPI

This project contains a reference implementation of the download API for geonorge.no. All members of Norge Digitalt can implement this API and make their data available through the download manager available at https://geonorge.no 

## Local setup
Two files should be located in root folder:
.env
application_default_credentials.json

Both files are used by docker compose. ADC is used for connecting to Google Cloud Storage.

### Running and debugging
Run the stack either from Visual Studio (set `docker-compose` as startup project and press F5) or with `docker compose up`, not both.
Visual Studio uses its own compose project name, so running both gives two separate stacks with their own SQL Server containers and volumes, competing for the same ports.

After changing `.env`, the app container must be recreated, a restart does not pick up the new values:
```
docker compose up -d --force-recreate --no-deps geonorge.download
```
When running from Visual Studio, add `-p <project name>` (see `docker compose ls`).

### Connection string
The SQL Server address depends on where the connection is made from:

| From | Server |
|---|---|
| The app container (`ConnectionStrings__DefaultConnection` in `.env`) | `geonorge.download.db,1433` |
| The host machine (SSMS, `dotnet ef`, app run outside Docker) | `localhost,1434` |

The database name is `kartverket_nedlasting`. Example for `.env`:
```
ConnectionStrings__DefaultConnection=Server=geonorge.download.db,1433;Database=kartverket_nedlasting;User Id=sa;Password=<some-password>;TrustServerCertificate=True;
```

### local DB
For initial setup of the database, use docker compose up. 
Next, in dev powershell set the env variable EF_CONNECTION_STRING to point to the database, and GOOGLE_APPLICATION_CREDENTIALS (required for app to run properly, else we can't use EF DB update):
```
$env:EF_CONNECTION_STRING='Server=localhost,1434;Database=kartverket_nedlasting;User Id=sa;Password=<some-password>;TrustServerCertificate=True;'

$env:GOOGLE_APPLICATION_CREDENTIALS='<path-to-repo>\Geonorge.NedlastingAPI\application_default_credentials.json'
```

When the DB container is running, run the following command to create the database structure:
```
dotnet ef database update --project Geonorge.Download --startup-project Geonorge.Download
```

Skip this step if the database was restored from a backup (backups in `volumes/backup` are mounted at `/backup` in the DB container). The tables already exist there, and `database update` fails with "table already exists".

Restart app container after DB creation. 