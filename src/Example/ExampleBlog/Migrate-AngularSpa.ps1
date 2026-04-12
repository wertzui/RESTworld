<#
.SYNOPSIS
    Migrates an old-style ASP.NET Core Angular SPA project (using ClientApp folder and
    webpack SPA service) to the new structure using either Microsoft.AspNetCore.SpaProxy
    or Aspire orchestration with a separate .esproj client project.

.DESCRIPTION
    The script:
      1. Moves all contents of the old ClientApp folder to a new sibling folder named
         <projectname>.angular (all lowercase).
      2. Generates a <projectname>.angular.esproj file for Visual Studio JavaScript tooling.
      3. Creates aspnetcore-https.js for dev HTTPS certificate setup.
      4. Updates package.json: renames the package, adds SSL start scripts and required
         dependencies (run-script-os, jest-editor-support).
      5. Creates src/proxy.conf.js (if missing) to proxy API calls to the ASP.NET Core backend.
      6. Updates angular.json: renames the Angular project, adds proxyConfig and dev server port.
        7. Modifies the server .csproj: removes old SPA build targets/properties, adds a
            ProjectReference to the new .esproj, and either configures SpaProxy or prepares the
            project for Aspire orchestration.
        8. If using SpaProxy, updates Properties/launchSettings.json to register the SpaProxy
            hosting startup assembly. If using Aspire, updates the AppHost project and source file
            to launch the Angular client.

.PARAMETER OldProjectFolder
    The folder containing the old .csproj file and the ClientApp subfolder.

.PARAMETER EnlistInAspire
    When specified, configures the migration for Aspire orchestration instead of SpaProxy.

.PARAMETER PathToAspireApphostProjectFolder
    Required when EnlistInAspire is specified. May point to the Aspire AppHost folder or the
    AppHost .csproj file.

.EXAMPLE
    .\Migrate-AngularSpa.ps1 -OldProjectFolder "C:\Projects\MyApp\MyApp.Client"

.EXAMPLE
    .\Migrate-AngularSpa.ps1 -OldProjectFolder "C:\Projects\MyApp\MyApp.Client" -EnlistInAspire -PathToAspireApphostProjectFolder "C:\Projects\MyApp\MyApp.AppHost"
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$OldProjectFolder,

    [switch]$EnlistInAspire,

    [string]$PathToAspireApphostProjectFolder
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Helper: write a file as UTF-8 without BOM (compatible with PS 5.1 and PS 7+)
function Write-Utf8NoBom {
    param([string]$Path, [string]$Content)
    [System.IO.File]::WriteAllText($Path, $Content, [System.Text.UTF8Encoding]::new($false))
}

function Get-NormalizedProjectFolderPath {
    param([string]$Path)

    if (-not (Test-Path $Path)) {
        Write-Error "Path '$Path' does not exist."
        exit 1
    }

    $item = Get-Item $Path
    if ($item.PSIsContainer) {
        return $item.FullName
    }

    if ($item.Extension -ieq '.csproj') {
        return $item.Directory.FullName
    }

    Write-Error "Path '$Path' must be a folder or a .csproj file."
    exit 1
}

function Get-RelativePath {
    param([string]$FromPath, [string]$ToPath)

    return [System.IO.Path]::GetRelativePath($FromPath, $ToPath).Replace('/', '\')
}

function Convert-ToAspireProjectTypeName {
    param([string]$ProjectName)

    return [regex]::Replace($ProjectName.ToLowerInvariant(), '[^a-z0-9]', '_')
}

function Add-ProjectReferenceIfMissing {
    param(
        [xml]$ProjectXml,
        [string]$ReferencePath,
        [switch]$DisableReferenceOutputAssembly
    )

    $projectRefIG = $ProjectXml.Project.ItemGroup |
        Where-Object { $_.SelectSingleNode('ProjectReference') } |
        Select-Object -First 1

    if ($null -eq $projectRefIG) {
        $projectRefIG = $ProjectXml.CreateElement('ItemGroup')
        $ProjectXml.Project.AppendChild($projectRefIG) | Out-Null
    }

    if ($null -eq $projectRefIG.SelectSingleNode("ProjectReference[@Include='$ReferencePath']")) {
        $projRefElem = $ProjectXml.CreateElement('ProjectReference')
        $projRefElem.SetAttribute('Include', $ReferencePath)

        if ($DisableReferenceOutputAssembly) {
            $refOutputElem = $ProjectXml.CreateElement('ReferenceOutputAssembly')
            $refOutputElem.InnerText = 'false'
            $projRefElem.AppendChild($refOutputElem) | Out-Null
        }

        $projectRefIG.AppendChild($projRefElem) | Out-Null
        return $true
    }

    return $false
}

function Save-ProjectXml {
    param(
        [xml]$ProjectXml,
        [string]$Path
    )

    $xmlSettings = New-Object System.Xml.XmlWriterSettings
    $xmlSettings.Indent = $true
    $xmlSettings.IndentChars = '  '
    $xmlSettings.OmitXmlDeclaration = $true
    $xmlSettings.Encoding = [System.Text.UTF8Encoding]::new($false)

    $xmlWriter = [System.Xml.XmlWriter]::Create($Path, $xmlSettings)
    $ProjectXml.Save($xmlWriter)
    $xmlWriter.Close()
}

function Add-AspireJavaScriptAppRegistration {
    param(
        [string]$FilePath,
        [string]$AppName,
        [string]$ProjectTypeName
    )

    $fileText = Get-Content $FilePath -Raw
    if ($fileText -match [regex]::Escape('builder.AddJavaScriptApp("' + $AppName + '"')) {
        return 'already-exists'
    }

    $newline = if ($fileText.Contains("`r`n")) { "`r`n" } else { "`n" }
    $snippet = @(
        '// Add The Angular part of the Frontend'
        ('builder.AddJavaScriptApp("' + $AppName + '", Directory.GetParent(new ' + $ProjectTypeName + '().ProjectPath)!.FullName)')
        '    // This will run "npm start" in the directory of the Angular project, which starts the Angular development server.'
        '    // Have a look at the package.json of the Angular project to see what the "start" script does.'
        '    .WithRunScript("start")'
        '    // Add a reference to the frontend service, so that the Angular development server can call the backend API.'
        '    // This will inject an environment variable services__ExampleBlog-Client__https__0 which is then used in the proxy.conf.json of the Angular project to proxy API calls to the backend API.'
        '    .WithReference(frontendService)'
        '    // Have the Angular part of the frontend appear as child of the dotnet part of the frontend.'
        '    .WithParentRelationship(frontendService)'
        '    // This is the Port on which the Angular development server runs. It is referenced in the package.json of the Angular project.'
        '    .WithHttpsEndpoint(env: "PORT")'
        '    // The Angular development server does not have a health check endpoint, so we just use the root URL as a readiness probe.'
        '    .WithHttpHealthCheck();'
        ''
    ) -join $newline

    $buildMatch = [regex]::Match($fileText, '(?m)^.*builder\.Build\(\).*(\r?\n)?')
    if ($buildMatch.Success) {
        $fileText = $fileText.Insert($buildMatch.Index, $snippet)
        Write-Utf8NoBom -Path $FilePath -Content $fileText
        return 'inserted-before-build'
    }

    if (-not $fileText.EndsWith($newline)) {
        $fileText += $newline
    }

    $fileText += $snippet
    Write-Utf8NoBom -Path $FilePath -Content $fileText
    return 'appended-to-end'
}

# ============================
# Validation & Setup
# ============================
$OldProjectFolder = (Resolve-Path $OldProjectFolder).Path

if ($EnlistInAspire -and [string]::IsNullOrWhiteSpace($PathToAspireApphostProjectFolder)) {
    Write-Error 'PathToAspireApphostProjectFolder is required when EnlistInAspire is specified.'
    exit 1
}

$aspireAppHostFolder = $null
if ($EnlistInAspire) {
    $aspireAppHostFolder = Get-NormalizedProjectFolderPath -Path $PathToAspireApphostProjectFolder
}

Write-Host '=== Angular SPA Migration Script ===' -ForegroundColor Cyan
Write-Host "Source folder: $OldProjectFolder"
if ($EnlistInAspire) {
    Write-Host "Mode          : Aspire orchestration" -ForegroundColor Yellow
    Write-Host "AppHost folder: $aspireAppHostFolder" -ForegroundColor Yellow
}
else {
    Write-Host 'Mode          : SpaProxy' -ForegroundColor Yellow
}

$csprojFiles = @(Get-ChildItem -Path $OldProjectFolder -Filter '*.csproj' -File)
if ($csprojFiles.Count -eq 0) {
    Write-Error "No .csproj file found in '$OldProjectFolder'"; exit 1
}
if ($csprojFiles.Count -gt 1) {
    Write-Error "Multiple .csproj files found in '$OldProjectFolder'. Ensure there is only one."; exit 1
}

$csprojFile      = $csprojFiles[0]
$serverProjectName = [System.IO.Path]::GetFileNameWithoutExtension($csprojFile.Name)
$clientAppFolder   = Join-Path $OldProjectFolder 'ClientApp'

if (-not (Test-Path $clientAppFolder)) {
    Write-Error "No 'ClientApp' folder found in '$OldProjectFolder'"; exit 1
}

$parentFolder       = Split-Path $OldProjectFolder -Parent
$clientProjectName  = $serverProjectName + '.Angular'
$clientProjectFolder = Join-Path $parentFolder $clientProjectName

if (Test-Path $clientProjectFolder) {
    Write-Error "Destination folder '$clientProjectFolder' already exists. Remove it first."; exit 1
}

# Random unique Angular dev server port in the dynamic/private port range
$spaPort = Get-Random -Minimum 49152 -Maximum 65534

Write-Host "Server project : $serverProjectName"    -ForegroundColor Yellow
Write-Host "Client project : $clientProjectName"    -ForegroundColor Yellow
Write-Host "Client folder  : $clientProjectFolder"  -ForegroundColor Yellow
Write-Host "SPA dev port   : $spaPort"              -ForegroundColor Yellow

$aspireJavaScriptAppName = "$serverProjectName-Client-Angular"
$aspireProjectTypeName = Convert-ToAspireProjectTypeName -ProjectName $clientProjectName

$confirmation = Read-Host 'Proceed with migration? [y/N]'
if ($confirmation -notmatch '^[Yy]$') {
    Write-Host 'Migration cancelled.' -ForegroundColor Red
    exit 0
}

# ============================
# STEP 1 – Move ClientApp contents
# ============================
Write-Host "`nStep 1: Moving ClientApp contents to '$clientProjectFolder'..." -ForegroundColor Green

New-Item -ItemType Directory -Path $clientProjectFolder -Force | Out-Null

# robocopy /E copies all subdirs incl. empty ones, /MOVE deletes sources after copy.
# Exit codes < 8 are success; 8+ indicate errors.
$robocopyExit = (Start-Process robocopy `
    -ArgumentList "`"$clientAppFolder`" `"$clientProjectFolder`" /E /MOVE /NFL /NDL /NJH /NJS" `
    -Wait -PassThru -NoNewWindow).ExitCode
if ($robocopyExit -ge 8) {
    Write-Error "robocopy failed with exit code $robocopyExit."; exit 1
}

Remove-Item -Path $clientAppFolder -Recurse -Force -ErrorAction SilentlyContinue
Write-Host '  Done.' -ForegroundColor Gray

# ============================
# STEP 2 – Generate .esproj file
# ============================
Write-Host "`nStep 2: Generating $clientProjectName.esproj..." -ForegroundColor Green

$esprojPath    = Join-Path $clientProjectFolder "$clientProjectName.esproj"
$esprojContent = @'
<Project Sdk="Microsoft.VisualStudio.JavaScript.Sdk/1.0.4902498">
  <PropertyGroup>
    <StartupCommand>npm start</StartupCommand>
    <JavaScriptTestFramework>Jasmine</JavaScriptTestFramework>
    <!-- Allows the build (or compile) script located on package.json to run on Build -->
    <ShouldRunBuildScript>false</ShouldRunBuildScript>
    <!-- Folder where production build objects will be placed -->
    <BuildOutputFolder>$(MSBuildProjectDirectory)\dist\CLIENT_PROJECT_NAME\browser\</BuildOutputFolder>
  </PropertyGroup>
</Project>
'@
$esprojContent = $esprojContent.Replace('CLIENT_PROJECT_NAME', $clientProjectName)
Write-Utf8NoBom -Path $esprojPath -Content $esprojContent
Write-Host "  Created: $esprojPath" -ForegroundColor Gray

# ============================
# STEP 3 – Create aspnetcore-https.js
# ============================
Write-Host "`nStep 3: Creating aspnetcore-https.js..." -ForegroundColor Green

$httpsJsPath = Join-Path $clientProjectFolder 'aspnetcore-https.js'
if (-not (Test-Path $httpsJsPath)) {
    # Uses single-quoted here-string so backticks and $ are literal (JavaScript template literals)
    $httpsJsContent = @'
// This script sets up HTTPS for the application using the ASP.NET Core HTTPS certificate
const fs = require('fs');
const spawn = require('child_process').spawn;
const path = require('path');

const baseFolder =
  process.env.APPDATA !== undefined && process.env.APPDATA !== ''
    ? `${process.env.APPDATA}/ASP.NET/https`
    : `${process.env.HOME}/.aspnet/https`;

const certificateArg = process.argv.map(arg => arg.match(/--name=(?<value>.+)/i)).filter(Boolean)[0];
const certificateName = certificateArg ? certificateArg.groups.value : process.env.npm_package_name;

if (!certificateName) {
  console.error('Invalid certificate name. Run this script in the context of an npm/yarn script or pass --name=<<app>> explicitly.')
  process.exit(-1);
}

const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

if (!fs.existsSync(baseFolder)) {
    fs.mkdirSync(baseFolder, { recursive: true });
}

if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
  spawn('dotnet', [
    'dev-certs',
    'https',
    '--export-path',
    certFilePath,
    '--format',
    'Pem',
    '--no-password',
  ], { stdio: 'inherit', })
  .on('exit', (code) => process.exit(code));
}
'@
    Write-Utf8NoBom -Path $httpsJsPath -Content $httpsJsContent
    Write-Host "  Created: $httpsJsPath" -ForegroundColor Gray
}
else {
    Write-Host "  Already exists, skipping." -ForegroundColor Gray
}

# ============================
# STEP 4 – Update package.json
# ============================
Write-Host "`nStep 4: Updating package.json..." -ForegroundColor Green

$packageJsonPath = Join-Path $clientProjectFolder 'package.json'
if (Test-Path $packageJsonPath) {
    $packageJson = Get-Content $packageJsonPath -Raw | ConvertFrom-Json

    # Update package name
    $packageJson.name = $clientProjectName

    # Ensure scripts object exists
    if ($null -eq $packageJson.PSObject.Properties['scripts']) {
        $packageJson | Add-Member -NotePropertyName 'scripts' -NotePropertyValue ([PSCustomObject]@{})
    }

    # Override start with run-script-os dispatcher
    $packageJson.scripts.start = 'run-script-os'

    # Add/overwrite the proxy-aware start scripts
    $newScripts = [ordered]@{
        'prestart'      = 'node aspnetcore-https'
        'start:windows' = 'ng serve --ssl --ssl-cert "%APPDATA%\ASP.NET\https\%npm_package_name%.pem" --ssl-key "%APPDATA%\ASP.NET\https\%npm_package_name%.key" --host=127.0.0.1'
        'start:default' = 'ng serve --ssl --ssl-cert "$HOME/.aspnet/https/${npm_package_name}.pem" --ssl-key "$HOME/.aspnet/https/${npm_package_name}.key" --host=127.0.0.1'
    }
    foreach ($entry in $newScripts.GetEnumerator()) {
        $existingProp = $packageJson.scripts.PSObject.Properties[$entry.Key]
        if ($null -eq $existingProp) {
            $packageJson.scripts | Add-Member -NotePropertyName $entry.Key -NotePropertyValue $entry.Value
        }
        else {
            $packageJson.scripts.($entry.Key) = $entry.Value
        }
    }

    # Add packageManager if absent
    if ($null -eq $packageJson.PSObject.Properties['packageManager']) {
        $packageJson | Add-Member -NotePropertyName 'packageManager' -NotePropertyValue 'npm@latest'
    }

    # Ensure dependencies object exists
    if ($null -eq $packageJson.PSObject.Properties['dependencies']) {
        $packageJson | Add-Member -NotePropertyName 'dependencies' -NotePropertyValue ([PSCustomObject]@{})
    }

    # Add required runtime dependencies
    foreach ($dep in @('run-script-os', 'jest-editor-support')) {
        if ($null -eq $packageJson.dependencies.PSObject.Properties[$dep]) {
            $packageJson.dependencies | Add-Member -NotePropertyName $dep -NotePropertyValue '*'
        }
    }

    Write-Utf8NoBom -Path $packageJsonPath -Content ($packageJson | ConvertTo-Json -Depth 20)
    Write-Host "  Updated: $packageJsonPath" -ForegroundColor Gray
}
else {
    Write-Warning "package.json not found – skipping."
}

# ============================
# STEP 5 – Create src/proxy.conf.js
# ============================
Write-Host "`nStep 5: Creating src/proxy.conf.js..." -ForegroundColor Green

$proxyConfPath = Join-Path $clientProjectFolder 'src' 'proxy.conf.js'
if (-not (Test-Path $proxyConfPath)) {
    # Detect backend HTTPS port from launchSettings if available
    $backendHttpsPort = '7109'
    $launchSettingsPath = Join-Path $OldProjectFolder 'Properties' 'launchSettings.json'
    if (Test-Path $launchSettingsPath) {
        try {
            $ls = Get-Content $launchSettingsPath -Raw | ConvertFrom-Json
            foreach ($profile in ($ls.profiles.PSObject.Properties | Select-Object -ExpandProperty Value)) {
                if ($profile.PSObject.Properties['applicationUrl'] -and
                    $profile.applicationUrl -match 'https://localhost:(\d+)') {
                    $backendHttpsPort = $matches[1]
                    break
                }
            }
        }
        catch {
            Write-Warning "Could not parse launchSettings.json – using default backend port $backendHttpsPort."
        }
    }

    # Template uses JS template literals; use single-quoted here-string then Replace for port
    $proxyTemplate = @'
const { env } = require('process');

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
  env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:BACKEND_PORT';

const PROXY_CONFIG = [
  {
    context: [
      '/api',
    ],
    target,
    secure: false
  }
]

module.exports = PROXY_CONFIG;
'@
    $proxyConfContent = $proxyTemplate.Replace('BACKEND_PORT', $backendHttpsPort)

    $srcDir = Split-Path $proxyConfPath -Parent
    if (-not (Test-Path $srcDir)) {
        New-Item -ItemType Directory -Path $srcDir -Force | Out-Null
    }
    Write-Utf8NoBom -Path $proxyConfPath -Content $proxyConfContent
    Write-Host "  Created: $proxyConfPath" -ForegroundColor Gray
    Write-Host "  NOTE: Review proxy context paths in proxy.conf.js and adjust to match your API routes." -ForegroundColor Yellow
}
else {
    Write-Host "  Already exists, skipping." -ForegroundColor Gray
}

# ============================
# STEP 6 – Update angular.json
# ============================
Write-Host "`nStep 6: Updating angular.json..." -ForegroundColor Green

$angularJsonPath = Join-Path $clientProjectFolder 'angular.json'
$oldAngularProjectKey = $null

if (Test-Path $angularJsonPath) {
    $angularJson = Get-Content $angularJsonPath -Raw | ConvertFrom-Json

    if ($angularJson.PSObject.Properties['projects']) {
        $projectsObj      = $angularJson.projects
        $oldAngularProjectKey = $projectsObj.PSObject.Properties.Name | Select-Object -First 1

        if ($oldAngularProjectKey) {
            $projectConfig = $projectsObj.$oldAngularProjectKey

            # Add proxyConfig and port to the serve architect options
            if ($projectConfig.PSObject.Properties['architect'] -and
                $projectConfig.architect.PSObject.Properties['serve']) {

                $serve = $projectConfig.architect.serve

                if ($null -eq $serve.PSObject.Properties['options']) {
                    $serve | Add-Member -NotePropertyName 'options' -NotePropertyValue ([PSCustomObject]@{})
                }

                $opts = $serve.options

                if ($null -eq $opts.PSObject.Properties['proxyConfig']) {
                    $opts | Add-Member -NotePropertyName 'proxyConfig' -NotePropertyValue 'src/proxy.conf.js'
                }
                else { $opts.proxyConfig = 'src/proxy.conf.js' }

                if ($null -eq $opts.PSObject.Properties['port']) {
                    $opts | Add-Member -NotePropertyName 'port' -NotePropertyValue $spaPort
                }
                else { $opts.port = $spaPort }
            }

            # Rename project key to new client project name
            if ($oldAngularProjectKey -ne $clientProjectName) {
                $projectsObj | Add-Member -NotePropertyName $clientProjectName -NotePropertyValue $projectConfig
                $projectsObj.PSObject.Properties.Remove($oldAngularProjectKey)
            }
        }
    }

    # Save JSON, then replace remaining internal references (e.g. in buildTarget strings)
    $angularJsonText = $angularJson | ConvertTo-Json -Depth 20
    if ($oldAngularProjectKey -and $oldAngularProjectKey -ne $clientProjectName) {
        $angularJsonText = $angularJsonText -replace [regex]::Escape($oldAngularProjectKey), $clientProjectName
    }
    Write-Utf8NoBom -Path $angularJsonPath -Content $angularJsonText
    Write-Host "  Updated: $angularJsonPath (project key: '$oldAngularProjectKey' → '$clientProjectName')" -ForegroundColor Gray
}
else {
    Write-Warning "angular.json not found – skipping."
}

# ============================
# STEP 7 – Modify server .csproj
# ============================
Write-Host "`nStep 7: Modifying server .csproj..." -ForegroundColor Green

[xml]$csproj = Get-Content $csprojFile.FullName -Raw

# Determine .NET major version for SpaProxy wildcard version
$tfNode = $csproj.SelectSingleNode('/Project/PropertyGroup/TargetFramework')
$dotnetMajor = if ($tfNode -and $tfNode.InnerText -match 'net(\d+)') { $matches[1] } else { '10' }
$spaProxyVersion = "$dotnetMajor.*-*"

# --- Remove obsolete properties ---
$obsoleteProps = @('TypeScriptCompileBlocked', 'TypeScriptToolsVersion',
                   'BuildServerSideRenderer', 'SpaRoot', 'DefaultItemExcludes',
                   'SpaProxyLaunchCommand', 'SpaProxyServerUrl')
foreach ($pg in @($csproj.Project.PropertyGroup)) {
    foreach ($propName in $obsoleteProps) {
        $node = $pg.SelectSingleNode($propName)
        if ($node) { $pg.RemoveChild($node) | Out-Null }
    }
}

# --- Add SPA properties to the first PropertyGroup ---
$mainPG = $csproj.Project.PropertyGroup | Select-Object -First 1
$newProps = [ordered]@{ 'SpaRoot' = "..\$clientProjectName" }
if (-not $EnlistInAspire) {
    $newProps['SpaProxyLaunchCommand'] = 'npm start'
    $newProps['SpaProxyServerUrl'] = "https://localhost:$spaPort"
}
foreach ($entry in $newProps.GetEnumerator()) {
    $existingNode = $mainPG.SelectSingleNode($entry.Key)
    if ($existingNode) {
        $existingNode.InnerText = $entry.Value
    }
    else {
        $elem = $csproj.CreateElement($entry.Key)
        $elem.InnerText = $entry.Value
        $mainPG.AppendChild($elem) | Out-Null
    }
}

# --- Remove ItemGroups managing the old ClientApp sources ---
$itemGroupsToRemove = @()
foreach ($ig in @($csproj.Project.ItemGroup)) {
    # ItemGroup with Content/None managing $(SpaRoot)**
    $contentRemove = $ig.SelectSingleNode("Content[@Remove]")
    if ($contentRemove -and $contentRemove.GetAttribute('Remove') -match '\$\(SpaRoot\)') {
        $itemGroupsToRemove += $ig
        continue
    }
    # ItemGroup with Folder items inside ClientApp
    $folderInClientApp = $ig.SelectSingleNode("Folder[contains(@Include,'ClientApp')]")
    if ($folderInClientApp) {
        $itemGroupsToRemove += $ig
        continue
    }
}
foreach ($ig in $itemGroupsToRemove) {
    $csproj.Project.RemoveChild($ig) | Out-Null
}

# --- Remove old SPA build targets ---
$targetsToRemove = @()
foreach ($target in @($csproj.Project.Target)) {
    if ($target.Name -in @('DebugEnsureNodeEnv', 'PublishRunWebpack')) {
        $targetsToRemove += $target
    }
}
foreach ($target in $targetsToRemove) {
    $csproj.Project.RemoveChild($target) | Out-Null
}

# --- Remove obsolete SPA package references ---
$packageRefsToRemove = @()
foreach ($ig in @($csproj.Project.ItemGroup)) {
    foreach ($packageRef in @($ig.PackageReference)) {
        if ($packageRef.Include -eq 'Microsoft.AspNetCore.SpaServices.Extensions' -or
            ($EnlistInAspire -and $packageRef.Include -eq 'Microsoft.AspNetCore.SpaProxy')) {
            $packageRefsToRemove += [PSCustomObject]@{
                ItemGroup = $ig
                PackageReference = $packageRef
            }
        }
    }
}
foreach ($entry in $packageRefsToRemove) {
    $entry.ItemGroup.RemoveChild($entry.PackageReference) | Out-Null
    if ($entry.ItemGroup.ChildNodes.Count -eq 0) {
        $csproj.Project.RemoveChild($entry.ItemGroup) | Out-Null
    }
}

# --- Remove now-empty PropertyGroups ---
$emptyPGs = @()
foreach ($pg in @($csproj.Project.PropertyGroup)) {
    if ($pg.ChildNodes.Count -eq 0) { $emptyPGs += $pg }
}
foreach ($pg in $emptyPGs) {
    $csproj.Project.RemoveChild($pg) | Out-Null
}

# --- Add Microsoft.AspNetCore.SpaProxy PackageReference when applicable ---
if (-not $EnlistInAspire) {
    $packageRefIG = $csproj.Project.ItemGroup |
        Where-Object { $_.SelectSingleNode('PackageReference') } |
        Select-Object -First 1

    if ($null -eq $packageRefIG) {
        $packageRefIG = $csproj.CreateElement('ItemGroup')
        $csproj.Project.AppendChild($packageRefIG) | Out-Null
    }

    if ($null -eq $packageRefIG.SelectSingleNode("PackageReference[@Include='Microsoft.AspNetCore.SpaProxy']")) {
        $prElem = $csproj.CreateElement('PackageReference')
        $prElem.SetAttribute('Include', 'Microsoft.AspNetCore.SpaProxy')
        $versionElem = $csproj.CreateElement('Version')
        $versionElem.InnerText = $spaProxyVersion
        $prElem.AppendChild($versionElem) | Out-Null
        $packageRefIG.AppendChild($prElem) | Out-Null
    }
}

# --- Add ProjectReference to the new .esproj ---
$esprojRelPath = "..\$clientProjectName\$clientProjectName.esproj"
Add-ProjectReferenceIfMissing -ProjectXml $csproj -ReferencePath $esprojRelPath -DisableReferenceOutputAssembly | Out-Null

Save-ProjectXml -ProjectXml $csproj -Path $csprojFile.FullName

Write-Host "  Updated: $($csprojFile.FullName)" -ForegroundColor Gray

# ============================
# STEP 8 – Update orchestration settings
# ============================
if (-not $EnlistInAspire) {
    Write-Host "`nStep 8: Updating Properties/launchSettings.json..." -ForegroundColor Green

    $launchSettingsPath = Join-Path $OldProjectFolder 'Properties' 'launchSettings.json'
    if (Test-Path $launchSettingsPath) {
        $launchSettings = Get-Content $launchSettingsPath -Raw | ConvertFrom-Json

        foreach ($profileProp in $launchSettings.profiles.PSObject.Properties) {
            $profile = $profileProp.Value

            if ($null -eq $profile.PSObject.Properties['environmentVariables']) {
                $profile | Add-Member -NotePropertyName 'environmentVariables' -NotePropertyValue ([PSCustomObject]@{})
            }

            $envVars = $profile.environmentVariables
            $spaKey  = 'ASPNETCORE_HOSTINGSTARTUPASSEMBLIES'

            if ($null -eq $envVars.PSObject.Properties[$spaKey]) {
                $envVars | Add-Member -NotePropertyName $spaKey -NotePropertyValue 'Microsoft.AspNetCore.SpaProxy'
            }
            else {
                # Merge: append if not already listed
                if ($envVars.$spaKey -notmatch 'Microsoft\.AspNetCore\.SpaProxy') {
                    $envVars.$spaKey = $envVars.$spaKey + ';Microsoft.AspNetCore.SpaProxy'
                }
            }
        }

        Write-Utf8NoBom -Path $launchSettingsPath -Content ($launchSettings | ConvertTo-Json -Depth 10)
        Write-Host "  Updated: $launchSettingsPath" -ForegroundColor Gray
    }
    else {
        Write-Warning 'launchSettings.json not found – skipping.'
    }
}
else {
    Write-Host "`nStep 8: Updating Aspire AppHost..." -ForegroundColor Green

    $launchSettingsPath = Join-Path $OldProjectFolder 'Properties' 'launchSettings.json'
    if (Test-Path $launchSettingsPath) {
        $launchSettings = Get-Content $launchSettingsPath -Raw | ConvertFrom-Json

        foreach ($profileProp in $launchSettings.profiles.PSObject.Properties) {
            $profile = $profileProp.Value
            if ($null -eq $profile.PSObject.Properties['environmentVariables']) {
                continue
            }

            $envVars = $profile.environmentVariables
            $spaKey = 'ASPNETCORE_HOSTINGSTARTUPASSEMBLIES'
            $existingValue = $envVars.PSObject.Properties[$spaKey]
            if ($null -ne $existingValue) {
                $remainingValues = @($envVars.$spaKey -split ';' | Where-Object {
                    -not [string]::IsNullOrWhiteSpace($_) -and $_ -ne 'Microsoft.AspNetCore.SpaProxy'
                })

                if ($remainingValues.Count -eq 0) {
                    $envVars.PSObject.Properties.Remove($spaKey)
                }
                else {
                    $envVars.$spaKey = ($remainingValues -join ';')
                }
            }
        }

        Write-Utf8NoBom -Path $launchSettingsPath -Content ($launchSettings | ConvertTo-Json -Depth 10)
        Write-Host "  Updated: $launchSettingsPath (removed SpaProxy hosting startup assembly)" -ForegroundColor Gray
    }

    $aspireCsprojFiles = @(Get-ChildItem -Path $aspireAppHostFolder -Filter '*.csproj' -File)
    if ($aspireCsprojFiles.Count -eq 0) {
        Write-Error "No .csproj file found in '$aspireAppHostFolder'"
        exit 1
    }
    if ($aspireCsprojFiles.Count -gt 1) {
        Write-Error "Multiple .csproj files found in '$aspireAppHostFolder'. Ensure there is only one."
        exit 1
    }

    $aspireCsprojFile = $aspireCsprojFiles[0]
    [xml]$aspireCsproj = Get-Content $aspireCsprojFile.FullName -Raw
    $aspireEsprojRelPath = Get-RelativePath -FromPath $aspireCsprojFile.Directory.FullName -ToPath $esprojPath
    Add-ProjectReferenceIfMissing -ProjectXml $aspireCsproj -ReferencePath $aspireEsprojRelPath | Out-Null
    Save-ProjectXml -ProjectXml $aspireCsproj -Path $aspireCsprojFile.FullName
    Write-Host "  Updated: $($aspireCsprojFile.FullName)" -ForegroundColor Gray

    $appHostCodeFile = $null
    foreach ($candidateName in @('AppHost.cs', 'Program.cs', 'Programm.cs')) {
        $candidate = Get-ChildItem -Path $aspireAppHostFolder -Recurse -Filter $candidateName -File | Select-Object -First 1
        if ($candidate) {
            $appHostCodeFile = $candidate
            break
        }
    }

    if ($null -eq $appHostCodeFile) {
        Write-Warning "No AppHost.cs, Program.cs, or Programm.cs file found in '$aspireAppHostFolder'. Skipping AddJavaScriptApp registration."
    }
    else {
        $registrationStatus = Add-AspireJavaScriptAppRegistration -FilePath $appHostCodeFile.FullName -AppName $aspireJavaScriptAppName -ProjectTypeName $aspireProjectTypeName
        Write-Host "  Updated: $($appHostCodeFile.FullName)" -ForegroundColor Gray

        if ($registrationStatus -eq 'appended-to-end') {
            Write-Warning "Could not find a line containing 'builder.Build()' in '$($appHostCodeFile.FullName)'. Added the AddJavaScriptApp block to the end of the file instead."
        }
        elseif ($registrationStatus -eq 'already-exists') {
            Write-Host '  AddJavaScriptApp registration already exists, skipping duplicate insert.' -ForegroundColor Gray
        }
    }
}

# ============================
# Summary
# ============================
Write-Host "`n=== Migration Complete ===" -ForegroundColor Cyan
Write-Host ''
Write-Host "New client project : $clientProjectFolder" -ForegroundColor Green
Write-Host "Angular dev port   : $spaPort"             -ForegroundColor Green
Write-Host ''
Write-Host 'Next steps:' -ForegroundColor Yellow
Write-Host "  1. Review/update '$clientProjectName\src\proxy.conf.js' – add your API route contexts."
Write-Host "  2. Run 'npm install' inside '$clientProjectFolder'."
if ($EnlistInAspire) {
    Write-Host "  3. Verify the AppHost references '$clientProjectName\$clientProjectName.esproj' and starts the Angular app."
    Write-Host "  4. Verify the Angular app builds: cd '$clientProjectFolder' && npm run build."
}
else {
    Write-Host "  3. Add '$clientProjectName\$clientProjectName.esproj' to your solution file."
    Write-Host "  4. Verify the Angular app builds: cd '$clientProjectFolder' && npm run build."
}
Write-Host "  5. (Optional) Clean up legacy files: tslint.json, browserslist, e2e/ if present."
Write-Host "  6. Adjust your CI/CD pipeline."