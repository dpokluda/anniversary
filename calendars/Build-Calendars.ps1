#Requires -Version 7.0
<#
.SYNOPSIS
    Builds the Anniversary name-day calendar files (Anniversary.Calendar.*.json) from pinned, openly licensed sources.

.DESCRIPTION
    Downloads each source at a pinned version, converts it to the Anniversary calendar format (spec 5.1),
    validates it, and writes it next to this script. The Czech calendar is not generated; it comes from
    Anniversary v4 and only gets its metadata added here. Re-run the script to reproduce the files exactly.

    Sources and licenses are listed in README.md and THIRD-PARTY-NOTICES.md.
#>
[CmdletBinding()]
param(
    [string] $OutputFolder = $PSScriptRoot,
    # Anniversary v4's Czech calendar; the generated Czech file keeps its values unchanged, so it can be its own source.
    [string] $CzechSource = (Join-Path $PSScriptRoot 'Anniversary.Calendar.Czech.json')
)

$ErrorActionPreference = 'Stop'

$radooneCommit = '5d3f301f0796b2fca8048db8c092e9bd09504dfe'
$radooneLicense = 'MIT, Copyright (c) 2024 Slovak Name Days MCP Server (see THIRD-PARTY-NOTICES.md)'
$latvianUrl = 'https://data.gov.lv/dati/dataset/2a880497-a25b-4e59-990b-382ca155fb0c/resource/39d01c5b-6964-4e91-b05c-45e6fbf1b77a/download/latvieu-tradicionlo-vrdadienu-saraksts.csv'

$radooneCalendars = @(
    @{ Code = 'at'; File = 'Austrian';  Name = 'Austria (Österreich)';     Locale = 'de-AT' }
    @{ Code = 'bg'; File = 'Bulgarian'; Name = 'Bulgaria (България)';      Locale = 'bg-BG' }
    @{ Code = 'fr'; File = 'French';    Name = 'France';                   Locale = 'fr-FR' }
    @{ Code = 'hu'; File = 'Hungarian'; Name = 'Hungary (Magyarország)';   Locale = 'hu-HU' }
    @{ Code = 'it'; File = 'Italian';   Name = 'Italy (Italia)';           Locale = 'it-IT' }
    @{ Code = 'pl'; File = 'Polish';    Name = 'Poland (Polska)';          Locale = 'pl-PL' }
    @{ Code = 'ru'; File = 'Russian';   Name = 'Russia (Россия)';          Locale = 'ru-RU' }
    @{ Code = 'sk'; File = 'Slovak';    Name = 'Slovakia (Slovensko)';     Locale = 'sk-SK' }
)

function ConvertTo-JsonString([string] $value)
{
    return '"' + $value.Replace('\', '\\').Replace('"', '\"') + '"'
}

function Test-DayKey([string] $key)
{
    $date = [datetime]::MinValue
    return $key -match '^\d{2}-\d{2}$' -and
        [datetime]::TryParseExact("2000-$key", 'yyyy-MM-dd', [cultureinfo]::InvariantCulture, 'None', [ref] $date)
}

function Write-Calendar([string] $fileName, [string] $name, [string] $locale, [string] $source, [string] $license,
                        [System.Collections.IDictionary] $days)
{
    $keys = @($days.Keys | Sort-Object)
    foreach ($key in $keys)
    {
        if (-not (Test-DayKey $key))
        {
            throw "$fileName : invalid day key '$key'"
        }
        if ([string]::IsNullOrWhiteSpace($days[$key]))
        {
            throw "$fileName : empty value for '$key'"
        }
    }
    if ($keys.Count -lt 360)
    {
        throw "$fileName : only $($keys.Count) days; expected a full year"
    }

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add('{')
    $lines.Add("  `"name`": $(ConvertTo-JsonString $name),")
    $lines.Add("  `"locale`": $(ConvertTo-JsonString $locale),")
    $lines.Add("  `"source`": $(ConvertTo-JsonString $source),")
    $lines.Add("  `"license`": $(ConvertTo-JsonString $license),")
    $lines.Add('  "calendar": {')
    for ($i = 0; $i -lt $keys.Count; $i++)
    {
        $comma = if ($i -lt $keys.Count - 1) { ',' } else { '' }
        $lines.Add("    `"$($keys[$i])`": $(ConvertTo-JsonString $days[$keys[$i]])$comma")
    }
    $lines.Add('  }')
    $lines.Add('}')

    $path = Join-Path $OutputFolder "Anniversary.Calendar.$fileName.json"
    [System.IO.File]::WriteAllLines($path, $lines, [System.Text.UTF8Encoding]::new($false))

    # Round-trip check: the written file must parse and contain the same days.
    $check = Get-Content $path -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
    if ($check.calendar.Count -ne $keys.Count)
    {
        throw "$fileName : round-trip check failed"
    }
    Write-Host ("{0,-12} {1,3} days  {2}" -f $fileName, $keys.Count, $name)
}

# Czech: from Anniversary v4, values unchanged, metadata added.
$czech = Get-Content $CzechSource -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
Write-Calendar 'Czech' 'Czechia (Česko)' 'cs-CZ' 'Anniversary v4 (Anniversary.Calendar.Czech.json)' `
    'MIT, Copyright (c) David Pokluda (part of Anniversary; see LICENSE)' $czech.calendar

# radoone/nameday-mcp: { "MM-dd": [ "Name", ... ] } -> "Name, Name"
foreach ($c in $radooneCalendars)
{
    $url = "https://raw.githubusercontent.com/radoone/nameday-mcp/$radooneCommit/src/data/$($c.Code).json"
    $data = (Invoke-WebRequest $url).Content | ConvertFrom-Json -AsHashtable
    $days = [ordered]@{}
    foreach ($key in $data.Keys)
    {
        $names = @($data[$key] | ForEach-Object { $_.Trim() } | Where-Object { $_ })
        if ($names.Count -gt 0)
        {
            $days[$key] = $names -join ', '
        }
    }
    $source = "https://github.com/radoone/nameday-mcp/blob/$radooneCommit/src/data/$($c.Code).json"
    Write-Calendar $c.File $c.Name $c.Locale $source $radooneLicense $days
}

# Latvia: official State Language Centre list (CSV "dd.MM.,""Name, Name""").
$csv = (Invoke-WebRequest $latvianUrl).Content | ConvertFrom-Csv
$latvian = [ordered]@{}
foreach ($row in $csv)
{
    if ($row.datums -match '^(\d{2})\.(\d{2})\.$' -and -not [string]::IsNullOrWhiteSpace($row.vardadienas))
    {
        $latvian["$($Matches[2])-$($Matches[1])"] = $row.vardadienas.Trim()
    }
}
Write-Calendar 'Latvian' 'Latvia (Latvija)' 'lv-LV' `
    'Valsts valodas centrs, data.gov.lv dataset "Latviešu tradicionālais un paplašinātais kalendārvārdu saraksts" (traditional list)' `
    'CC0-1.0' $latvian
