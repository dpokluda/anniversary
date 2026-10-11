#Requires -Version 7.0
<#
.SYNOPSIS
    Draws the Anniversary app icon and writes it in every format the app needs.

.DESCRIPTION
    A refreshed version of the v4 tear-off calendar icon (yellow binding, red "24", blue lines, stacked pages), drawn as
    vector shapes so every size is sharp. Small sizes drop details that would turn into noise (lines, rings, back pages).

    Outputs:
      ..\Anniversary.Shared\Assets\App.ico   Windows: 16, 20, 24, 32, 40, 48, 64, 256 px (32-bit, 256 px PNG-compressed)
      ..\Anniversary.Shared\Assets\App.png   256 px, used by the About dialog
      App.icns                               macOS .app: the calendar on a white rounded square, 32-1024 px incl. @2x
      App-1024.png                           master image (Linux, store listings)

    Requires Windows (System.Drawing / GDI+). Re-run after changing the drawing code.
#>
[CmdletBinding()]
param(
    [string] $PreviewFolder
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$assets = Join-Path $PSScriptRoot '..\Anniversary.Shared\Assets'

function Rgb([int] $r, [int] $g, [int] $b, [int] $a = 255)
{
    return [System.Drawing.Color]::FromArgb($a, $r, $g, $b)
}

function New-RoundedRect([single] $x, [single] $y, [single] $w, [single] $h, [single] $r, [bool] $bottomRounded = $true)
{
    $path = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $d = 2 * $r
    $path.AddArc($x, $y, $d, $d, 180, 90)
    $path.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    if ($bottomRounded)
    {
        $path.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90)
        $path.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    }
    else
    {
        $path.AddLine($x + $w, $y + $h, $x, $y + $h)
    }
    $path.CloseFigure()
    return $path
}

function New-VerticalGradient([single] $top, [single] $bottom, $from, $to)
{
    return [System.Drawing.Drawing2D.LinearGradientBrush]::new(
        [System.Drawing.PointF]::new(0, $top - 1), [System.Drawing.PointF]::new(0, $bottom + 1), $from, $to)
}

function New-Icon([int] $size)
{
    $bitmap = [System.Drawing.Bitmap]::new($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bitmap)
    $g.SmoothingMode = 'AntiAlias'
    $g.PixelOffsetMode = 'HighQuality'
    $g.CompositingQuality = 'HighQuality'
    $g.Clear([System.Drawing.Color]::Transparent)

    # Everything is designed on a 256 x 256 grid; small sizes drop details.
    $k = $size / 256.0
    $g.ScaleTransform($k, $k)
    $small = $size -le 24
    $medium = $size -le 40
    $line = [Math]::Max(1.0 / $k, 2.0)

    if ($small)
    {
        # A tall page, as in v4, with a shallow binding so the day number gets most of it (title bar and taskbar
        # sizes). At 16 px its outline lies on whole pixels.
        $px = 24; $py = 8; $pw = 208; $ph = 240; $radius = 30; $header = 44
    }
    else
    {
        # A tall page, as in v4; a shallow binding leaves room for the day number and the lines of names below it.
        $px = 38; $py = 18; $pw = 180; $ph = 210; $radius = 20; $header = if ($medium) { 42 } else { 40 }
    }

    # Soft shadow
    if (-not $small)
    {
        foreach ($i in 1..4)
        {
            $shadow = New-RoundedRect ($px + 2) ($py + 6 + 3 * $i) $pw ($ph + 10) ($radius + 2)
            $g.FillPath([System.Drawing.SolidBrush]::new((Rgb 20 30 50 14)), $shadow)
        }
    }

    # Stacked pages behind (the tear-off pad)
    if (-not $medium)
    {
        $back2 = New-RoundedRect ($px + 14) ($py + 18) ($pw - 28) ($ph + 4) $radius
        $g.FillPath([System.Drawing.SolidBrush]::new((Rgb 0xC9 0xD1 0xDC)), $back2)
        $g.DrawPath([System.Drawing.Pen]::new((Rgb 0xA9 0xB4 0xC3), $line), $back2)
        $back1 = New-RoundedRect ($px + 7) ($py + 9) ($pw - 14) ($ph + 2) $radius
        $g.FillPath([System.Drawing.SolidBrush]::new((Rgb 0xE4 0xE9 0xEF)), $back1)
        $g.DrawPath([System.Drawing.Pen]::new((Rgb 0xB8 0xC2 0xCF), $line), $back1)
    }

    # Front page
    $page = New-RoundedRect $px $py $pw $ph $radius
    $g.FillPath((New-VerticalGradient $py ($py + $ph) (Rgb 255 255 255) (Rgb 0xF1 0xF4 0xF8)), $page)

    # Yellow binding (header), as v4's yellow strip
    $headerPath = New-RoundedRect $px $py $pw $header $radius $false
    $g.FillPath((New-VerticalGradient $py ($py + $header) (Rgb 0xFF 0xD7 0x4A) (Rgb 0xF5 0xA6 0x23)), $headerPath)
    $g.DrawLine([System.Drawing.Pen]::new((Rgb 0xD9 0x8A 0x00), $line), $px, $py + $header, $px + $pw, $py + $header)
    $g.DrawPath([System.Drawing.Pen]::new((Rgb 0x8E 0x9A 0xAA), $line), $page)

    # Binding rings
    if (-not $small)
    {
        foreach ($cx in @(($px + $pw * 0.3), ($px + $pw * 0.7)))
        {
            $holeTop = $py + $header / 2 - 8
            $g.FillEllipse([System.Drawing.SolidBrush]::new((Rgb 0xB0 0x6E 0x00)), $cx - 8, $holeTop, 16, 16)
            $ring = New-RoundedRect ($cx - 6) ($py - 16) 12 ($holeTop - $py + 26) 6
            $ringBrush = [System.Drawing.Drawing2D.LinearGradientBrush]::new(
                [System.Drawing.PointF]::new($cx - 7, 0), [System.Drawing.PointF]::new($cx + 7, 0),
                (Rgb 0xA4 0xAD 0xB8), (Rgb 0x4E 0x57 0x63))
            $g.FillPath($ringBrush, $ring)
        }
    }

    # The red "24"
    if ($size -eq 16)
    {
        # At 16 px anti-aliased digits blur, so they're drawn as crisp pixels (a 3 x 5 pixel font, with margins).
        $g.ResetTransform()
        $g.SmoothingMode = 'None'
        $g.PixelOffsetMode = 'Half'
        $red = [System.Drawing.SolidBrush]::new((Rgb 0xE0 0x2D 0x2D))
        $glyphs = @{
            '2' = @('###', '..#', '###', '#..', '###')
            '4' = @('#.#', '#.#', '###', '..#', '..#')
        }
        $left = 4
        foreach ($digit in '2', '4')
        {
            $rows = $glyphs[$digit]
            for ($row = 0; $row -lt 5; $row++)
            {
                for ($column = 0; $column -lt 3; $column++)
                {
                    if ($rows[$row][$column] -eq '#')
                    {
                        $g.FillRectangle($red, $left + $column, 7 + $row, 1, 1)
                    }
                }
            }
            $left += 5
        }
        $g.Dispose()
        return $bitmap
    }

    $digits = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $digits.AddString('24', [System.Drawing.FontFamily]::new('Segoe UI'), [int][System.Drawing.FontStyle]::Bold, 100,
        [System.Drawing.PointF]::new(0, 0), [System.Drawing.StringFormat]::GenericTypographic)
    $bounds = $digits.GetBounds()
    if ($small)
    {
        # The number gets about 60% of the page body, centred, so there is white space all around it. The narrow page
        # lets it take more of the width, so it's as big as on a square page.
        $body = $ph - $header; $areaHeight = $body * 0.56; $areaTop = $py + $header + ($body - $areaHeight) / 2 + 4; $maxWidth = $pw * 0.715
    }
    elseif ($medium)
    {
        $body = $ph - $header; $areaHeight = $body * 0.56; $areaTop = $py + $header + ($body - $areaHeight) / 2 + 4; $maxWidth = $pw * 0.62
    }
    else
    {
        $areaTop = $py + $header + 16; $areaHeight = 68; $maxWidth = $pw - 76
    }
    $scale = [Math]::Min($areaHeight / $bounds.Height, $maxWidth / $bounds.Width)
    $matrix = [System.Drawing.Drawing2D.Matrix]::new()
    $matrix.Translate($px + $pw / 2, $areaTop + $areaHeight / 2)
    $matrix.Scale($scale, $scale)
    $matrix.Translate(-($bounds.X + $bounds.Width / 2), -($bounds.Y + $bounds.Height / 2))
    $digits.Transform($matrix)
    $g.FillPath([System.Drawing.SolidBrush]::new((Rgb 0xE0 0x2D 0x2D)), $digits)

    # Blue lines: the names written under the day
    if (-not $medium)
    {
        # At least 2 device pixels thick and snapped to the pixel grid, so the lines stay crisp at 48-64 px.
        $pen = [System.Drawing.Pen]::new((Rgb 0x23 0x6B 0xD8), [Math]::Max(7, 2 / $k))
        $pen.StartCap = 'Round'
        $pen.EndCap = 'Round'
        # Three evenly spaced lines; two (further apart) at 48-64 px so they stay distinct.
        $lineY = $areaTop + $areaHeight + 21
        $widths = if ($size -ge 128) { @(59, 48, 35) } else { @(57, 40) }
        $spacing = if ($size -ge 128) { 19 } else { 26 }
        for ($i = 0; $i -lt $widths.Count; $i++)
        {
            $y = [Math]::Round(($lineY + $i * $spacing) * $k) / $k
            $g.DrawLine($pen, $px + $pw / 2 - $widths[$i], $y, $px + $pw / 2 + $widths[$i], $y)
        }
    }

    $g.Dispose()
    return $bitmap
}

function New-MacIcon([int] $size)
{
    # macOS 26 and later put an icon that isn't a rounded square on a grey tile, so the macOS icon brings its own white
    # one, on Apple's grid: 824 px with 185 px corners, 100 px in from each edge of 1024.
    $bitmap = [System.Drawing.Bitmap]::new($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bitmap)
    $g.SmoothingMode = 'AntiAlias'
    $g.PixelOffsetMode = 'HighQuality'
    $g.CompositingQuality = 'HighQuality'
    $g.Clear([System.Drawing.Color]::Transparent)

    $k = $size / 1024.0
    $g.ScaleTransform($k, $k)
    foreach ($i in 1..4)
    {
        $g.FillPath([System.Drawing.SolidBrush]::new((Rgb 0 0 0 12)), (New-RoundedRect 100 (100 + 4 * $i) 824 824 185))
    }
    $tile = New-RoundedRect 100 100 824 824 185
    $g.FillPath((New-VerticalGradient 100 924 (Rgb 255 255 255) (Rgb 0xEE 0xF0 0xF4)), $tile)
    $g.ResetTransform()

    # The calendar is drawn at its own size, so it keeps that size's details, at 74% of the icon: about 36 px (at 1024)
    # from the square's edges to the rings above and the shadow below. An even size keeps it on whole pixels. Row 128.5
    # of its 256 grid goes to the middle: a little below the middle of its outline, as the thin rings weigh little.
    $inner = 2 * [Math]::Round($size * 0.743 / 2)
    if ($inner -le 24)
    {
        # The small layout's page (240 high) fills more of its square than the larger one's (210), so it's drawn smaller.
        $inner = 2 * [Math]::Round($inner * 210 / 240 / 2)
    }
    $x = ($size - $inner) / 2
    $y = [Math]::Round($size / 2 - $inner * 128.5 / 256)
    $calendar = New-Icon $inner
    $g.InterpolationMode = 'NearestNeighbor'
    $g.DrawImage($calendar, [System.Drawing.Rectangle]::new($x, $y, $inner, $inner))
    $calendar.Dispose()
    $g.Dispose()
    return $bitmap
}

function Get-PngBytes([System.Drawing.Bitmap] $bitmap)
{
    $stream = [System.IO.MemoryStream]::new()
    $bitmap.Save($stream, [System.Drawing.Imaging.ImageFormat]::Png)
    return , $stream.ToArray()
}

function Get-DibBytes([System.Drawing.Bitmap] $bitmap)
{
    # 32-bit BMP entry for small icon sizes: BITMAPINFOHEADER, bottom-up BGRA pixels, empty AND mask.
    $size = $bitmap.Width
    $stream = [System.IO.MemoryStream]::new()
    $writer = [System.IO.BinaryWriter]::new($stream)
    $writer.Write([int]40); $writer.Write([int]$size); $writer.Write([int]($size * 2))
    $writer.Write([int16]1); $writer.Write([int16]32)
    foreach ($i in 1..6) { $writer.Write([int]0) }
    for ($y = $size - 1; $y -ge 0; $y--)
    {
        for ($x = 0; $x -lt $size; $x++)
        {
            $c = $bitmap.GetPixel($x, $y)
            $writer.Write([byte]$c.B); $writer.Write([byte]$c.G); $writer.Write([byte]$c.R); $writer.Write([byte]$c.A)
        }
    }
    $maskRow = [int]([Math]::Ceiling($size / 32.0) * 4)
    $writer.Write([byte[]]::new($maskRow * $size))
    $writer.Flush()
    return , $stream.ToArray()
}

function Write-Ico([string] $path, [int[]] $sizes)
{
    $images = [System.Collections.Generic.List[byte[]]]::new()
    foreach ($s in $sizes)
    {
        $bitmap = New-Icon $s
        $images.Add($(if ($s -ge 256) { Get-PngBytes $bitmap } else { Get-DibBytes $bitmap }))
        $bitmap.Dispose()
    }
    $stream = [System.IO.MemoryStream]::new()
    $writer = [System.IO.BinaryWriter]::new($stream)
    $writer.Write([int16]0); $writer.Write([int16]1); $writer.Write([int16]$sizes.Count)
    $offset = 6 + 16 * $sizes.Count
    for ($i = 0; $i -lt $sizes.Count; $i++)
    {
        $dimension = if ($sizes[$i] -ge 256) { 0 } else { $sizes[$i] }
        $writer.Write([byte]$dimension); $writer.Write([byte]$dimension); $writer.Write([byte]0); $writer.Write([byte]0)
        $writer.Write([int16]1); $writer.Write([int16]32)
        $writer.Write([int]$images[$i].Length); $writer.Write([int]$offset)
        $offset += $images[$i].Length
    }
    foreach ($image in $images)
    {
        $writer.Write($image)
    }
    $writer.Flush()
    [System.IO.File]::WriteAllBytes($path, $stream.ToArray())
}

function Write-Icns([string] $path)
{
    # Big-endian container of PNG images: 4-char type + length (including the 8-byte header) + data. ic11-ic14 are the
    # @2x sizes. There are no 1x 16 and 32 px images: macOS reads PNGs in those slots (icp4, icp5) as noise and puts
    # its own ARGB ones (ic04, ic05) on a grey tile, so it scales the larger images down instead.
    $entries = [ordered]@{ ic07 = 128; ic08 = 256; ic09 = 512; ic10 = 1024; ic11 = 32; ic12 = 64; ic13 = 256; ic14 = 512 }
    $body = [System.IO.MemoryStream]::new()
    foreach ($type in $entries.Keys)
    {
        $bitmap = New-MacIcon $entries[$type]
        $png = Get-PngBytes $bitmap
        $bitmap.Dispose()
        $typeBytes = [System.Text.Encoding]::ASCII.GetBytes($type); $body.Write($typeBytes, 0, 4)
        $length = [BitConverter]::GetBytes([int]($png.Length + 8)); [Array]::Reverse($length); $body.Write($length, 0, 4)
        $body.Write($png, 0, $png.Length)
    }
    $file = [System.IO.MemoryStream]::new()
    $magic = [System.Text.Encoding]::ASCII.GetBytes('icns'); $file.Write($magic, 0, 4)
    $total = [BitConverter]::GetBytes([int]($body.Length + 8)); [Array]::Reverse($total); $file.Write($total, 0, 4)
    $bodyBytes = $body.ToArray(); $file.Write($bodyBytes, 0, $bodyBytes.Length)
    [System.IO.File]::WriteAllBytes($path, $file.ToArray())
}

Write-Ico (Join-Path $assets 'App.ico') @(16, 20, 24, 32, 40, 48, 64, 256)
$bitmap = New-Icon 256; $bitmap.Save((Join-Path $assets 'App.png'), [System.Drawing.Imaging.ImageFormat]::Png); $bitmap.Dispose()
Write-Icns (Join-Path $PSScriptRoot 'App.icns')
$bitmap = New-Icon 1024; $bitmap.Save((Join-Path $PSScriptRoot 'App-1024.png'), [System.Drawing.Imaging.ImageFormat]::Png); $bitmap.Dispose()

if ($PreviewFolder)
{
    New-Item -ItemType Directory -Force $PreviewFolder | Out-Null
    foreach ($s in 16, 20, 24, 32, 40, 48, 64, 128, 256)
    {
        $bitmap = New-Icon $s
        $bitmap.Save((Join-Path $PreviewFolder "App-$s.png"), [System.Drawing.Imaging.ImageFormat]::Png)
        $bitmap.Dispose()
    }
    foreach ($s in 32, 64, 128, 256, 512)
    {
        $bitmap = New-MacIcon $s
        $bitmap.Save((Join-Path $PreviewFolder "App-mac-$s.png"), [System.Drawing.Imaging.ImageFormat]::Png)
        $bitmap.Dispose()
    }
}
Write-Host 'Icon written: App.ico, App.png, App.icns, App-1024.png'

