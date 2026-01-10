## Dodawanie migracji
```
dotnet ef migrations add InitialCreate --project .\KorID.Data\KorID.Data.csproj --startup-project .\KorID.MigrationService\KorID.MigrationService.csproj
export ConnectionStrings__koriddb="Host=localhost;Port=5432;Database=koriddb;Username=postgres;Password=postgres"
dotnet ef database update --project ./KorID.Data/KorID.Data.csproj --startup-project ./KorID.MigrationService/KorID.MigrationService.csproj
```