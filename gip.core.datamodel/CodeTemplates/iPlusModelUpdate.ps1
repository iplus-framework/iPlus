# Define variables for paths, connection strings, and model names
$databaseConnectionString = "data source=.\SQLEXP16;Initial Catalog=iPlusV5;Trusted_Connection=False;Encrypt=False;user id=gip;password=netspirit;"
$namespace = "gip.core.datamodel"
$outputDir = "EFModels"
$compiledOutputDir = "EFCompiledModels"
$efBaseName = "iPlusV5";
# -------- 

$contextName = $efBaseName + "Context"
$entitiesName = $efBaseName + "_Entities"
$projectPath = "..\" + $namespace + ".csproj"
$contextFilePath = "..\" + $outputDir + "\" + $contextName + ".cs"
$modelName = $contextName + "Model"

# -------- SCRIPT --------
# Scaffold db models
Write-Host "Running the scaffold command to generate the models from the database"
dotnet ef dbcontext scaffold $databaseConnectionString Microsoft.EntityFrameworkCore.SqlServer --output-dir $outputDir --use-database-names --force --project $projectPath

# Modify context class
Write-Host "Commenting the connection string in the Context class"
(Get-Content $contextFilePath) -replace ".UseModel(" + $modelName + ".Instance)", '//$&' | Set-Content $contextFilePath
(Get-Content $contextFilePath) -replace '\.ConfigureWarnings\(warnings => warnings.Ignore\(CoreEventId.ManyServiceProvidersCreatedWarning\)\);', '.ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning))' | Set-Content $contextFilePath
(Get-Content $contextFilePath) -replace '//.*UseSqlServer.*', ".UseSqlServer(ConfigurationManager.ConnectionStrings[`"$entitiesName`"].ConnectionString);" | Set-Content $contextFilePath

# Optimize models
Write-Host "Running the command to generate compiled models"
dotnet ef dbcontext optimize --output-dir $compiledOutputDir --namespace $namespace --project $projectPath --context $contextName

# Revert changes in context class
Write-Host "Command finished, reverting the comments in the Context class"
(Get-Content $contextFilePath) -replace "//.UseModel(" + $modelName + ".Instance)", ".UseModel(" + $modelName + ".Instance)" | Set-Content $contextFilePath
(Get-Content $contextFilePath) -replace '\.ConfigureWarnings\(warnings => warnings.Ignore\(CoreEventId.ManyServiceProvidersCreatedWarning\)\)', '.ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning));' | Set-Content $contextFilePath
(Get-Content $contextFilePath) -replace '^(.*)(\.UseSqlServer.*)' , "$1//.UseSqlServer(ConfigurationManager.ConnectionStrings[`"$entitiesName`"].ConnectionString);" | Set-Content $contextFilePath

Write-Host "The database update was completed successfully, closing in 5 seconds"
Start-Sleep -Seconds 5