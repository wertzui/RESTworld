<#
.SYNOPSIS
    Creates a RESTworld TCreateDto file for the given entity.

.DESCRIPTION
    Generates a C# TCreateDto class in the Dtos/<EntityName>/ subfolder of the specified .Common project folder.
    The class has no base class (the record has no identity yet).
    Only the properties supplied via -Properties are emitted; infrastructure properties
    (Id, Timestamp, CreatedAt, CreatedBy, LastChangedAt, LastChangedBy) are never included.

.PARAMETER EntityName
    PascalCase English name of the entity, e.g. "Post" or "BlogEntry".

.PARAMETER CommonFolder
    Path to the folder that contains the .Common.csproj file,
    e.g. "C:\Repos\MyApp\MyApp.Common".

.PARAMETER Properties
    Array of strings, each representing one fully-annotated C# property declaration
    (including any leading attribute lines).

    Example element:
        @'
        [Display(Name = "Author")]
        public long AuthorId { get; set; }

        [JsonIgnore]
        public virtual AuthorGetListDto? Author { get; set; }
        '@

    Pass an empty array @() to create a class with no extra properties.

.EXAMPLE
    $props = @(
        "[Required]`n    public string Headline { get; set; } = default!;",
        "[Display(Name = `"Author`")]`n    public long AuthorId { get; set; }",
        "[JsonIgnore]`n    public virtual AuthorGetListDto? Author { get; set; }"
    )
    .\New-CreateDto.ps1 -EntityName "Post" -CommonFolder "C:\Repos\Blog\Blog.Common" -Properties $props
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
$outputFile = Join-Path $dtosFolder "${EntityName}CreateDto.cs"

# ── Build using-directives ────────────────────────────────────────────────────
# Always include DataAnnotations and JsonSerializer; add Enums only if needed.
$needsEnums   = $Properties | Where-Object { $_ -match '\bEnums\b' }
$needsJsonIgnore = $Properties | Where-Object { $_ -match '\[JsonIgnore\]' }

$usings = [System.Collections.Generic.List[string]]::new()
if ($needsEnums)        { $usings.Add("using $namespace.Enums;") }
$usings.Add("using System.ComponentModel.DataAnnotations;")
if ($needsJsonIgnore)   { $usings.Add("using System.Text.Json.Serialization;") }
$usings.Sort()

# ── Build property block ──────────────────────────────────────────────────────
$propertyBlock = ''
if ($Properties.Count -gt 0) {
    $indented = $Properties | ForEach-Object {
        # Indent each line of a property declaration by 4 spaces
        ($_ -split "`n" | ForEach-Object { "    $_" }) -join "`n"
    }
    $propertyBlock = "`n" + ($indented -join "`n`n") + "`n"
}

# ── Render file ───────────────────────────────────────────────────────────────
$usingBlock = ($usings | Select-Object -Unique) -join "`n"

$content = @"
$usingBlock

namespace $namespace.Dtos;

public class ${EntityName}CreateDto
{$propertyBlock}
"@

Set-Content -Path $outputFile -Value $content -Encoding UTF8
Write-Host "Created: $outputFile"
