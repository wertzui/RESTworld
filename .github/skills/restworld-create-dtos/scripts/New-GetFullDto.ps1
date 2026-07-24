<#
.SYNOPSIS
    Creates a RESTworld TGetFullDto file for the given entity.

.DESCRIPTION
    Generates a C# TGetFullDto class (extends ChangeTrackingDtoBase) in the Dtos/<EntityName>/ subfolder
    of the specified .Common project folder.
    This DTO is returned by the Get (single-record) endpoint and contains ALL entity properties.
    Navigation properties must carry [JsonIgnore].

.PARAMETER EntityName
    PascalCase English name of the entity, e.g. "Post" or "BlogEntry".

.PARAMETER CommonFolder
    Path to the folder that contains the .Common.csproj file,
    e.g. "C:\Repos\MyApp\MyApp.Common".

.PARAMETER Properties
    Array of strings, each representing one fully-annotated C# property declaration
    (including any leading attribute lines).
    Include all scalar properties, FK ID properties, and navigation properties.
    Navigation properties must include [JsonIgnore].

    Example elements:
        "[Required]`n    public string Headline { get; set; } = default!;"
        "[Display(Name = `"Author`")]`n    public long AuthorId { get; set; }"
        "[JsonIgnore]`n    public virtual AuthorGetListDto? Author { get; set; }"

    Pass an empty array @() to create a class with no extra properties.

.EXAMPLE
    $props = @(
        "[Required]`n    public string Headline { get; set; } = default!;",
        "[Display(Name = `"Author`")]`n    public long AuthorId { get; set; }",
        "[JsonIgnore]`n    public virtual AuthorGetListDto? Author { get; set; }"
    )
    .\New-GetFullDto.ps1 -EntityName "Post" -CommonFolder "C:\Repos\Blog\Blog.Common" -Properties $props
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $EntityName,

    [Parameter(Mandatory)]
    [string] $CommonFolder,

    [Parameter(Mandatory = $false)]
    [string[]] $Properties = @()
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
$dtosFolder = Join-Path $CommonFolder 'Dtos' $EntityName
if (-not (Test-Path $dtosFolder)) {
    New-Item -ItemType Directory -Path $dtosFolder -Force | Out-Null
}
$outputFile = Join-Path $dtosFolder "${EntityName}GetFullDto.cs"

# ── Build using-directives ────────────────────────────────────────────────────
$needsEnums      = $Properties | Where-Object { $_ -match '\bEnums\b' }
$needsJsonIgnore = $Properties | Where-Object { $_ -match '\[JsonIgnore\]' }

$usings = [System.Collections.Generic.List[string]]::new()
if ($needsEnums)        { $usings.Add("using $namespace.Enums;") }
$usings.Add("using RESTworld.Common.Dtos;")
$usings.Add("using System.ComponentModel.DataAnnotations;")
if ($needsJsonIgnore)   { $usings.Add("using System.Text.Json.Serialization;") }
$usings.Sort()

# ── Build property block ──────────────────────────────────────────────────────
$propertyBlock = ''
if ($Properties.Count -gt 0) {
    $indented = $Properties | ForEach-Object {
        ($_ -split "`n" | ForEach-Object { "    $_" }) -join "`n"
    }
    $propertyBlock = "`n" + ($indented -join "`n`n") + "`n"
}

# ── Render file ───────────────────────────────────────────────────────────────
$usingBlock = ($usings | Select-Object -Unique) -join "`n"

$content = @"
$usingBlock

namespace $namespace.Dtos;

public class ${EntityName}GetFullDto : ChangeTrackingDtoBase
{$propertyBlock}
"@

Set-Content -Path $outputFile -Value $content -Encoding UTF8
Write-Host "Created: $outputFile"
