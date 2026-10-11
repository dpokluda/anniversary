#Requires -Version 7.4
<#
.SYNOPSIS
    Builds the package for one platform from its published folder (spec 10): a macOS .app in a .dmg, a Windows
    installer (Inno Setup) or a Linux AppImage.

.DESCRIPTION
    Run it on the platform's own OS after publishing to publish/<rid>, as CI does (see README.md in this folder).
    The package is written to the output folder; its path is printed and, in GitHub Actions, set as the step output
    "path".
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('win-x64', 'win-arm64', 'osx-arm64', 'osx-x64', 'linux-x64', 'linux-arm64')]
    [string] $Rid,

    # The published app: the executable, README.md and THIRD-PARTY-NOTICES.md. Default: publish/<rid>.
    [string] $PublishFolder,

    # Where the package goes. Default: packages.
    [string] $OutputFolder
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

$repo = Split-Path $PSScriptRoot -Parent
$PublishFolder = [System.IO.Path]::GetFullPath($PublishFolder ? $PublishFolder : (Join-Path $repo 'publish' $Rid))
$OutputFolder = [System.IO.Path]::GetFullPath($OutputFolder ? $OutputFolder : (Join-Path $repo 'packages'))

# The AppImage tools, downloaded at pinned versions and checked against these SHA-256 hashes.
$appImageToolVersion = '1.9.1'
$appImageToolHashes = @{
    x86_64  = 'ed4ce84f0d9caff66f50bcca6ff6f35aae54ce8135408b3fa33abfc3cb384eb0'
    aarch64 = 'f0837e7448a0c1e4e650a93bb3e85802546e60654ef287576f46c71c126a9158'
}
$appImageRuntimeVersion = '20251108'
$appImageRuntimeHashes = @{
    x86_64  = '2fca8b443c92510f1483a883f60061ad09b46b978b2631c807cd873a47ec260d'
    aarch64 = '00cbdfcf917cc6c0ff6d3347d59e0ca1f7f45a6df1a428a0d6d8a78664d87444'
}

function Get-PinnedFile([string] $url, [string] $sha256)
{
    $path = Join-Path $work (Split-Path $url -Leaf)
    Invoke-WebRequest $url -OutFile $path
    if ((Get-FileHash $path -Algorithm SHA256).Hash -ne $sha256)
    {
        throw "The SHA-256 of $url doesn't match the pinned one."
    }
    return $path
}

function New-MacPackage
{
    $dmgFolder = Join-Path $work 'dmg'
    $app = Join-Path $dmgFolder 'Anniversary.app'
    $contents = Join-Path $app 'Contents'
    $resources = Join-Path $contents 'Resources'
    New-Item -ItemType Directory (Join-Path $contents 'MacOS'), $resources | Out-Null

    Copy-Item (Join-Path $PublishFolder 'Anniversary') (Join-Path $contents 'MacOS')
    Copy-Item (Join-Path $repo 'icon' 'App.icns') $resources
    Copy-Item $documents $resources
    $plist = (Get-Content (Join-Path $PSScriptRoot 'macos' 'Info.plist') -Raw).
        Replace('{Version}', $version).
        Replace('{Copyright}', [System.Security.SecurityElement]::Escape($copyright))
    Set-Content (Join-Path $contents 'Info.plist') $plist -NoNewline
    plutil -lint (Join-Path $contents 'Info.plist') | Out-Host

    # Apple Silicon only runs signed code, and a downloaded bundle without a valid signature is reported as damaged
    # instead of offering Open Anyway, so the whole bundle gets an ad-hoc signature.
    xattr -cr $app | Out-Host
    codesign --force --sign - $app | Out-Host
    codesign --verify --strict $app | Out-Host

    Copy-Item $documents $dmgFolder
    New-Item -ItemType SymbolicLink -Path (Join-Path $dmgFolder 'Applications') -Target '/Applications' | Out-Null

    # hdiutil sometimes fails with "Resource busy" on build machines; trying again usually works.
    $dmg = Join-Path $OutputFolder "Anniversary-$version-$Rid.dmg"
    for ($attempt = 1; ; $attempt++)
    {
        try
        {
            hdiutil create -volname 'Anniversary' -srcfolder $dmgFolder -ov -format UDZO $dmg | Out-Host
            break
        }
        catch
        {
            if ($attempt -ge 3)
            {
                throw
            }
            Start-Sleep -Seconds 5
        }
    }
    return $dmg
}

function New-WindowsPackage
{
    $iscc = (Get-Command 'ISCC.exe' -ErrorAction SilentlyContinue)?.Source
    if (-not $iscc)
    {
        $iscc = @(
            (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6' 'ISCC.exe'),
            (Join-Path $env:ProgramFiles 'Inno Setup 6' 'ISCC.exe'),
            (Join-Path $env:LOCALAPPDATA 'Programs' 'Inno Setup 6' 'ISCC.exe')) |
            Where-Object { Test-Path $_ } | Select-Object -First 1
    }
    if (-not $iscc)
    {
        throw 'Inno Setup 6 (ISCC.exe) was not found. Install it from https://jrsoftware.org/isinfo.php.'
    }

    & $iscc "/DAppVersion=$version" "/DRid=$Rid" "/DPublishFolder=$PublishFolder" "/DOutputFolder=$OutputFolder" `
        "/DCopyright=$copyright" (Join-Path $PSScriptRoot 'windows' 'Anniversary.iss') | Out-Host
    return Join-Path $OutputFolder "Anniversary-$version-$Rid-setup.exe"
}

function New-LinuxPackage
{
    # appimagetool runs on this machine; the runtime becomes the start of the AppImage, so it's for the target.
    $hostArch = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture -eq 'Arm64' ? 'aarch64' : 'x86_64'
    $arch = $Rid -eq 'linux-arm64' ? 'aarch64' : 'x86_64'
    $appImageTool = Get-PinnedFile `
        "https://github.com/AppImage/appimagetool/releases/download/$appImageToolVersion/appimagetool-$hostArch.AppImage" `
        $appImageToolHashes[$hostArch]
    $runtime = Get-PinnedFile `
        "https://github.com/AppImage/type2-runtime/releases/download/$appImageRuntimeVersion/runtime-$arch" `
        $appImageRuntimeHashes[$arch]
    chmod +x $appImageTool

    $appDir = Join-Path $work 'Anniversary.AppDir'
    $bin = Join-Path $appDir 'usr' 'bin'
    $doc = Join-Path $appDir 'usr' 'share' 'doc' 'anniversary'
    New-Item -ItemType Directory $bin, $doc | Out-Null
    Copy-Item (Join-Path $PublishFolder 'Anniversary') $bin
    Copy-Item $documents $doc
    Copy-Item (Join-Path $PSScriptRoot 'linux' 'AppRun'), (Join-Path $PSScriptRoot 'linux' 'anniversary.desktop') $appDir
    Copy-Item (Join-Path $repo 'Anniversary.Shared' 'Assets' 'App.png') (Join-Path $appDir 'anniversary.png')
    chmod +x (Join-Path $appDir 'AppRun') (Join-Path $bin 'Anniversary')

    # Extracting itself first means appimagetool doesn't need FUSE on the build machine.
    $appImage = Join-Path $OutputFolder "Anniversary-$version-$Rid.AppImage"
    $env:APPIMAGE_EXTRACT_AND_RUN = '1'
    $env:ARCH = $arch
    & $appImageTool --no-appstream --runtime-file $runtime $appDir $appImage | Out-Host
    return $appImage
}

$platform = $Rid.Split('-')[0]
$onThatOS = @{ osx = $IsMacOS; win = $IsWindows; linux = $IsLinux }[$platform]
if (-not $onThatOS)
{
    throw "Build the $Rid package on its own OS (see README.md in this folder)."
}

$executable = Join-Path $PublishFolder ($platform -eq 'win' ? 'Anniversary.exe' : 'Anniversary')
$documents = @((Join-Path $PublishFolder 'README.md'), (Join-Path $PublishFolder 'THIRD-PARTY-NOTICES.md'))
foreach ($file in @($executable) + $documents)
{
    if (-not (Test-Path $file))
    {
        throw "$file is missing; publish $Rid to $PublishFolder first."
    }
}

# The version and copyright come from the build (Directory.Build.props).
$properties = ([xml](Get-Content (Join-Path $repo 'Directory.Build.props') -Raw)).Project.PropertyGroup |
    Where-Object { $_.Version } | Select-Object -First 1
$version = $properties.Version
$copyright = $properties.Copyright

$work = Join-Path ([System.IO.Path]::GetTempPath()) "anniversary-package-$Rid"
Remove-Item $work -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory $work, $OutputFolder -Force | Out-Null

$package = switch ($platform)
{
    'osx' { New-MacPackage }
    'win' { New-WindowsPackage }
    'linux' { New-LinuxPackage }
}
if (-not (Test-Path $package))
{
    throw "The package wasn't created: $package"
}
Remove-Item $work -Recurse -Force

Write-Host "Package: $package"
if ($env:GITHUB_OUTPUT)
{
    Add-Content $env:GITHUB_OUTPUT "path=$package"
}
