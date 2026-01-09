# Geonorge NedlastingAPI

This project contains a reference implementation of the download API for geonorge.no. All members of Norge Digitalt can implement this API and make their data available through the download manager available at https://geonorge.no 

## Local setup
Two files should be located in root folder:
.env
application_default_credentials.json

Both files are used by docker compose. ADC is used for connecting to Google Cloud Storage.

### local DB
For initial setup of the database, use docker compose up. 
Next, in dev powershell set the env variable EF_CONNECTION_STRING to point to the database, and GOOGLE_APPLICATION_CREDENTIALS (required for app to run properly, else we can't use EF DB update):
```
$env:EF_CONNECTION_STRING='Server=localhost,1434;Database=kartverket_devnedlasting;User Id=sa;Password=<some-password>;TrustServerCertificate=True;'

$env:GOOGLE_APPLICATION_CREDENTIALS='<path-to-repo>\Geonorge.NedlastingAPI\application_default_credentials.json'
```

When the DB container is running, run the following command to create the database structure:
```
dotnet ef database update --project Geonorge.Download --startup-project Geonorge.Download
```

Restart app container after DB creation. 