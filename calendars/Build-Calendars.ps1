#Requires -Version 7.0
<#
.SYNOPSIS
    Builds the Anniversary calendar files (Anniversary.Calendar.*.json): name days from pinned, openly licensed sources,
    and holidays from the law and long tradition.

.DESCRIPTION
    Downloads each name-day source at a pinned version, converts it to the Anniversary calendar format (spec 5.1),
    validates it, and writes it next to this script. The Czech calendar is not generated; it comes from
    Anniversary v4 and gets its metadata and the two Czech public holidays that move with Easter added here.
    The US holiday calendar is written from the table below, which gives the source of every entry.
    Re-run the script to reproduce the files exactly.

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

# Rule keys for dates that move from year to year (spec 5.1), e.g. 11-Thu-4, 05-Mon-last, easter+1.
function Test-RuleKey([string] $key)
{
    return $key -match '^(?:(0[1-9]|1[0-2])-(mon|tue|wed|thu|fri|sat|sun)-([1-4]|last)|easter)([+-][1-9][0-9]?)?$'
}

function Write-Calendar([string] $fileName, [string] $name, [string] $locale, [string] $source, [string] $license,
                        [System.Collections.IDictionary] $days, [switch] $Holidays)
{
    # Ordinal order, so the files are the same whichever OS runs the script.
    [string[]] $keys = @($days.Keys)
    [Array]::Sort($keys, [StringComparer]::Ordinal)
    foreach ($key in $keys)
    {
        if (-not (Test-DayKey $key) -and -not (Test-RuleKey $key))
        {
            throw "$fileName : invalid day key '$key'"
        }
        if ([string]::IsNullOrWhiteSpace($days[$key]))
        {
            throw "$fileName : empty value for '$key'"
        }
    }
    $dayCount = @($keys | Where-Object { Test-DayKey $_ }).Count
    if (-not $Holidays -and $dayCount -lt 360)
    {
        throw "$fileName : only $dayCount days; expected a full year"
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
    Write-Host ("{0,-12} {1,3} entries  {2}" -f $fileName, $keys.Count, $name)
}

# Czech: from Anniversary v4, metadata added, plus the two public holidays that move with Easter
# (zákon č. 245/2000 Sb., § 2). The file is its own source, so adding them again changes nothing.
$czech = Get-Content $CzechSource -Raw -Encoding utf8 | ConvertFrom-Json -AsHashtable
$czech.calendar['easter-2'] = 'Velký pátek'
$czech.calendar['easter+1'] = 'Velikonoční pondělí'
Write-Calendar 'Czech' 'Czechia (Česko)' 'cs-CZ' `
    'Anniversary v4 (Anniversary.Calendar.Czech.json), plus Velký pátek and Velikonoční pondělí (zákon č. 245/2000 Sb., § 2)' `
    'MIT, Copyright (c) David Pokluda (part of Anniversary; see LICENSE)' $czech.calendar

# United States: the federal holidays and well-known observances (spec 5.7). Each comment gives the source;
# "tradition" means a date fixed by long custom (Easter dates follow the Gregorian computus).
$us = [ordered]@{
    '01-01'       = "New Year's Day"                 # 5 U.S.C. 6103(a)
    '01-Mon-3'    = 'Martin Luther King Jr. Day'     # 5 U.S.C. 6103(a): Birthday of Martin Luther King, Jr.
    '02-02'       = 'Groundhog Day'                  # tradition
    '02-14'       = "Valentine's Day"                # tradition
    '02-Mon-3'    = "Presidents' Day"                # 5 U.S.C. 6103(a): Washington's Birthday
    'easter-47'   = 'Mardi Gras'                     # tradition: the Tuesday before Ash Wednesday
    '03-17'       = "St. Patrick's Day"              # tradition
    '03-Sun-2'    = 'Daylight Saving Time begins'    # 15 U.S.C. 260a(a)
    '04-01'       = "April Fools' Day"               # tradition
    'easter-2'    = 'Good Friday'                    # tradition
    'easter'      = 'Easter'                         # tradition
    '04-22'       = 'Earth Day'                      # tradition (since 1970)
    '05-05'       = 'Cinco de Mayo'                  # tradition
    '05-Sun-2'    = "Mother's Day"                   # 36 U.S.C. 117
    '05-Mon-last' = 'Memorial Day'                   # 5 U.S.C. 6103(a)
    '06-14'       = 'Flag Day'                       # 36 U.S.C. 110
    '06-Sun-3'    = "Father's Day"                   # 36 U.S.C. 109
    '06-19'       = 'Juneteenth'                     # 5 U.S.C. 6103(a): Juneteenth National Independence Day
    '07-04'       = 'Independence Day'               # 5 U.S.C. 6103(a)
    '09-Mon-1'    = 'Labor Day'                      # 5 U.S.C. 6103(a)
    '09-11'       = 'Patriot Day'                    # 36 U.S.C. 144
    '10-Mon-2'    = 'Columbus Day'                   # 5 U.S.C. 6103(a)
    '10-31'       = 'Halloween'                      # tradition
    '11-Sun-1'    = 'Daylight Saving Time ends'      # 15 U.S.C. 260a(a)
    '11-11'       = 'Veterans Day'                   # 5 U.S.C. 6103(a)
    '11-Thu-4'    = 'Thanksgiving'                   # 5 U.S.C. 6103(a): Thanksgiving Day
    '11-Thu-4+1'  = 'Black Friday'                   # tradition: the day after Thanksgiving
    '12-24'       = 'Christmas Eve'                  # tradition
    '12-25'       = 'Christmas Day'                  # 5 U.S.C. 6103(a)
    '12-31'       = "New Year's Eve"                 # tradition
}
Write-Calendar 'US' 'United States' 'en-US' `
    '5 U.S.C. 6103(a) (federal holidays); 36 U.S.C. 109, 110, 117, 144 (observances); 15 U.S.C. 260a (daylight saving time); traditional dates' `
    'MIT, Copyright (c) David Pokluda (part of Anniversary; see LICENSE)' $us -Holidays

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
