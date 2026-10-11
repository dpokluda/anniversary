#Requires -Version 7.0
<#
.SYNOPSIS
    Builds the Anniversary calendar files (Anniversary.Calendar.*.json): name days from pinned, openly licensed sources,
    and holidays from the law and long tradition.

.DESCRIPTION
    Downloads each name-day source at a pinned version, converts it to the Anniversary calendar format (spec 5.1),
    validates it, and writes it next to this script. The Czech calendar is not generated; it comes from
    Anniversary v4 and gets its metadata and the two Czech public holidays that move with Easter added here.
    The US holiday and International Days calendars are written from the tables below, which give the source of every
    entry. Re-run the script to reproduce the files exactly.

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
    if ($locale)
    {
        $lines.Add("  `"locale`": $(ConvertTo-JsonString $locale),")
    }
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
    Write-Host ("{0,-13} {1,3} entries  {2}" -f $fileName, $keys.Count, $name)
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

# International Days: well-known days from the United Nations list of international days
# (https://www.un.org/en/observances/list-days-weeks, as of 10 October 2026; each comment gives the resolution or "UN
# list" when the list gives none), plus five fun days kept by tradition (spec 5.7). No locale, so the OS region never
# picks it.
$international = [ordered]@{
    '01-24'     = 'International Day of Education'                                       # A/RES/73/25
    '01-27'     = 'International Holocaust Remembrance Day'                              # A/RES/60/7
    '02-11'     = 'International Day of Women and Girls in Science'                      # A/RES/70/212
    '02-13'     = 'World Radio Day'                                                      # A/RES/67/124
    '02-21'     = 'International Mother Language Day'                                    # A/RES/56/262
    '03-03'     = 'World Wildlife Day'                                                   # A/RES/68/205
    '03-08'     = 'International Women''s Day'                                           # UN list
    '03-14'     = 'Pi Day'                                                               # tradition
    '03-20'     = 'International Day of Happiness'                                       # A/RES/66/281
    '03-21'     = 'World Poetry Day, International Day of Forests'                       # UNESCO: 30 C/Resolution 29; FAO: A/RES/67/200
    '03-22'     = 'World Water Day'                                                      # A/RES/47/193
    '04-02'     = 'World Autism Awareness Day'                                           # A/RES/62/139
    '04-07'     = 'World Health Day'                                                     # WHO: WHA/A.2/Res.35
    '04-12'     = 'International Day of Human Space Flight'                              # A/RES/65/271
    '04-22'     = 'International Mother Earth Day'                                       # A/RES/63/278
    '04-23'     = 'World Book and Copyright Day'                                         # UNESCO: 28 C/Resolution 3.18
    '04-30'     = 'International Jazz Day'                                               # UNESCO 36 C/Resolution 39
    '05-03'     = 'World Press Freedom Day'                                              # UNESCO 26 C/Resolution 4.3
    '05-04'     = 'Star Wars Day'                                                        # tradition
    '05-15'     = 'International Day of Families'                                        # A/RES/47/237
    '05-20'     = 'World Bee Day'                                                        # FAO: A/RES/72/211
    '05-22'     = 'International Day for Biological Diversity'                           # A/RES/55/201
    '05-31'     = 'World No Tobacco Day'                                                 # WHO: Resolution 42.19
    '05-Sat-2'  = 'World Migratory Bird Day'                                             # UNEP: UN list
    '06-03'     = 'World Bicycle Day'                                                    # A/RES/72/272
    '06-05'     = 'World Environment Day'                                                # A/RES/2994 (XXVII)
    '06-08'     = 'World Oceans Day'                                                     # A/RES/63/111
    '06-20'     = 'World Refugee Day'                                                    # A/RES/55/76
    '06-21'     = 'International Day of Yoga'                                            # A/RES/69/131
    '07-11'     = 'World Population Day'                                                 # A/RES/45/216
    '07-17'     = 'World Emoji Day'                                                      # tradition
    '07-18'     = 'Nelson Mandela International Day'                                     # A/RES/64/13
    '07-20'     = 'World Chess Day'                                                      # A/RES/74/22
    '07-30'     = 'International Day of Friendship'                                      # A/RES/65/275
    '08-09'     = 'International Day of the World''s Indigenous Peoples'                 # A/RES/49/214
    '08-12'     = 'International Youth Day'                                              # A/RES/54/120
    '08-19'     = 'World Humanitarian Day'                                               # A/RES/63/139
    '08-Fri-1'  = 'International Beer Day'                                               # tradition: the first Friday of August
    '09-08'     = 'International Literacy Day'                                           # UNESCO: 14 C/Resolution 1.441
    '09-15'     = 'International Day of Democracy'                                       # A/RES/62/7
    '09-19'     = 'Talk Like a Pirate Day'                                               # tradition
    '09-21'     = 'International Day of Peace'                                           # A/RES/36/67
    '09-27'     = 'World Tourism Day'                                                    # UN Tourism: UN list
    '10-01'     = 'International Coffee Day'                                             # A/RES/80/248
    '10-02'     = 'International Day of Non-Violence'                                    # A/RES/61/271
    '10-05'     = 'World Teachers'' Day'                                                 # UNESCO: 27 C/INF.7
    '10-10'     = 'World Mental Health Day'                                              # WHO: UN list
    '10-11'     = 'International Day of the Girl Child'                                  # A/RES/66/170
    '10-16'     = 'World Food Day'                                                       # FAO: A/RES/35/70
    '10-17'     = 'International Day for the Eradication of Poverty'                     # A/RES/47/196
    '10-24'     = 'United Nations Day'                                                   # A/RES/168 (II)
    '10-31'     = 'World Cities Day'                                                     # A/RES/68/239
    '10-Sat-2'  = 'World Migratory Bird Day'                                             # UNEP: UN list
    '11-14'     = 'World Diabetes Day'                                                   # A/RES/61/225
    '11-19'     = 'World Toilet Day'                                                     # A/RES/67/291
    '11-20'     = 'World Children''s Day'                                                # A/RES/836(IX)
    '11-25'     = 'International Day for the Elimination of Violence against Women'      # A/RES/54/134
    '12-01'     = 'World AIDS Day'                                                       # UN list
    '12-03'     = 'International Day of Persons with Disabilities'                       # A/RES/47/3
    '12-05'     = 'International Volunteer Day'                                          # A/RES/40/212
    '12-10'     = 'Human Rights Day'                                                     # A/RES/423 (V)
}
Write-Calendar 'International' 'International Days' '' `
    'United Nations list of international days (https://www.un.org/en/observances/list-days-weeks, as of 10 October 2026), plus fun days kept by tradition' `
    'MIT, Copyright (c) David Pokluda (part of Anniversary; see LICENSE)' $international -Holidays

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
