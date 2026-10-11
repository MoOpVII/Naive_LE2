# NaiveLE2 Inventory System

NaiveLE2 is a .NET 10 electronics inventory application. The WinForms client uses the ASP.NET Core REST API for data access; the API uses SQL Server through the shared Dapper data library.

## Configure the API for local development

The API requires a JWT signing key with at least 32 bytes. The key is deliberately not stored in `appsettings.json`. Generate a new random key and store it with .NET user-secrets from PowerShell:

```powershell
$bytes = New-Object byte[] 32
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes)
$rng.Dispose()
$jwtKey = [Convert]::ToBase64String($bytes)
dotnet user-secrets set "Jwt:Key" $jwtKey --project .\InventoryAPI\InventoryAPI.csproj
```

The API loads user-secrets automatically in the Development environment. The previous signing key was in tracked configuration; rotate it and do not reuse it. Tokens signed with the previous key will no longer be valid.

Set the SQL Server connection string in user-secrets if it differs from the LocalDB default in `InventoryAPI/appsettings.json`:

```powershell
dotnet user-secrets set "ConnectionStrings:SqlDb" "<your SQL Server connection string>" --project .\InventoryAPI\InventoryAPI.csproj
```

The default connection string targets `(localdb)\MSSQLLocalDB` and the `InventoryDB` database. Deploy the `InventoryDB` SQL project before starting the API.

Start the API using the HTTPS launch profile:

```powershell
dotnet run --project .\InventoryAPI\InventoryAPI.csproj --launch-profile https
```

The WinForms client defaults to `https://localhost:7060/api`. Override it with `INVENTORY_API_BASE_URL` when the API runs at a different address.

## Deploy and use the database

Deploy `InventoryDB/InventoryDB.sqlproj` to the SQL Server instance selected by `ConnectionStrings:SqlDb`. In Visual Studio, right-click the `InventoryDB` project, choose **Publish**, select the target server and `InventoryDB` database, review the deployment script, and publish. The deployment creates the item, supplier, and inventory movement schema and stored procedures. Existing products deploy with zero stock and a zero reorder level; use a receipt or a positive adjustment to enter opening stock so the movement is recorded.

## Run the WinForms client

Start the API with the HTTPS profile, then run `InventoryWinForms/InventoryWinForms.csproj` from Visual Studio or:

```powershell
dotnet run --project .\InventoryWinForms\InventoryWinForms.csproj
```

Register an account and sign in. Add suppliers and products, then use **Stock operations** to receive, issue, or adjust stock. The dashboard shows active products, units, low-stock count, cost valuation, and retail valuation. **Movement history & reports** filters the immutable stock log and exports the current result set to CSV. Item and supplier deletion is implemented as retirement; products with stock on hand cannot be retired.

## Run tests

Run API/controller and request validation tests with:

```powershell
dotnet test .\InventoryTests\InventoryTests.csproj
```

The SQL integration tests are skipped unless `INVENTORY_TEST_CONNECTION_STRING` points to a disposable SQL Server database where the `InventoryDB` project has been deployed. Set that variable only for a test database, then rerun `dotnet test`; the tests create unique temporary records, verify failed issues do not change stock or append movements, verify receipt weighted-average cost and movement logging, and clean up their records.

## Deployment configuration

Provide secrets through a secret manager or environment variables. ASP.NET Core environment variable names use double underscores for configuration nesting:

- `Jwt__Key`: a newly generated random signing key of at least 32 bytes
- `ConnectionStrings__SqlDb`: the SQL Server connection string

Keep signing keys and production connection strings out of source control. The API validates the JWT key at startup and will not start if it is missing or too short.
