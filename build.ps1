$buildDir = $env:APPVEYOR_BUILD_FOLDER
$projectDir = $buildDir + "\src";
$projectFile = $projectDir + "\PrivateGalleryCreator.csproj";

cd $projectDir

# Restore the main project
Write-Host "Restoring project" -ForegroundColor Green
& dotnet restore $projectFile --verbosity m

# Publish the project
Write-Host "Publishing project" -ForegroundColor Green
& dotnet publish $projectFile