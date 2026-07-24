<#
.SYNOPSIS
    Creates a RESTworld enum file in the Enums/ folder of the specified .Common project.

.DESCRIPTION
    Generates a C# enum in <CommonFolder>/Enums/<EnumName>.cs.
    The folder is created if it does not exist.
    Enum member names must be PascalCase and English — enforce this before passing them in.

.PARAMETER EnumName
    PascalCase English name of the enum, e.g. "PostState" or "OrderStatus".

.PARAMETER CommonFolder
    Absolute path to the folder that contains the .Common.csproj file,
    e.g. "C:\Repos\MyApp\MyApp.Common".

.PARAMETER Members
    Array of strings, each being one enum member declaration.
    Each element may be just a name ("Draft") or a name with an explicit value ("Draft = 0").
    Optionally include a leading [Display] or [Description] attribute line in the same element.

    Examples:
        @("Draft", "Published", "Archived")
        @("[Display(Name = `"In Progress`")]`n    InProgress", "Done")
        @("None = 0", "Active = 1", "Deleted = 2")

    Pass an empty array @() to create an empty enum body.

.PARAMETER UnderlyingType
    Optional underlying integer type for the enum, e.g. "byte" or "int".
    Defaults to the C# default (int) when omitted.

.PARAMETER IsFlags
    When specified, adds a [Flags] attribute to the enum.

.EXAMPLE
    .\New-Enum.ps1 -EnumName "PostState" -CommonFolder "C:\Repos\Blog\Blog.Common" `
        -Members @("Draft", "Published", "Archived")

.EXAMPLE
    .\New-Enum.ps1 -EnumName "Permission" -CommonFolder "C:\Repos\Blog\Blog.Common" `
        -Members @("None = 0", "Read = 1", "Write = 2", "Delete = 4") -IsFlags
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $EnumName,

    [Parameter(Mandatory)]
    [string] $CommonFolder,

    [Parameter(Mandatory = $false)]
    [string[]] $Members = @(),

    [Parameter(Mandatory = $false)]
    [string] $UnderlyingType = '',

    [Parameter(Mandatory = $false)]
    [switch] $IsFlags
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── Derive namespace from the .csproj file name ──────────────────────────────
$csprojFile = Get-ChildItem -Path $CommonFolder -Filter '*.csproj' -File | Select-Object -First 1
if (-not $csprojFile) {
    throw "No .csproj file found in '$CommonFolder'."
}
$namespace = [System.IO.Path]::GetFileNameWithoutExtension($csprojFile.Name)

# ── Output path ───────────────────────────────────────────────────────────────
$enumsFolder = Join-Path $CommonFolder 'Enums'
if (-not (Test-Path $enumsFolder)) {
    New-Item -ItemType Directory -Path $enumsFolder -Force | Out-Null
}
$outputFile = Join-Path $enumsFolder "${EnumName}.cs"

# ── Build using-directives ────────────────────────────────────────────────────
$needsDisplay = $Members | Where-Object { $_ -match '\[Display' }
$usings = [System.Collections.Generic.List[string]]::new()
if ($needsDisplay) { $usings.Add("using System.ComponentModel.DataAnnotations;") }
$usings.Sort()
$usingBlock = if ($usings.Count -gt 0) { ($usings -join "`n") + "`n`n" } else { '' }

# ── Build member block ────────────────────────────────────────────────────────
$memberBlock = ''
if ($Members.Count -gt 0) {
    $indented = $Members | ForEach-Object {
        # Indent each line of a member declaration by 4 spaces
        ($_ -split "`n" | ForEach-Object { "    $_" }) -join "`n"
    }
    $memberBlock = "`n" + ($indented -join ",`n`n") + "`n"
}

# ── Build enum declaration header ─────────────────────────────────────────────
$flagsLine   = if ($IsFlags)            { "[Flags]`n" } else { '' }
$typeClause  = if ($UnderlyingType)     { " : $UnderlyingType" } else { '' }

# ── Render file ───────────────────────────────────────────────────────────────
$content = @"
${usingBlock}namespace $namespace.Enums;

${flagsLine}public enum ${EnumName}${typeClause}
{$memberBlock}
"@

Set-Content -Path $outputFile -Value $content -Encoding UTF8
Write-Host "Created: $outputFile"
