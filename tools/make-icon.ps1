param(
    [string]$OutPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'app.ico'),
    [string]$PreviewPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'icon-preview.png')
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$sizes = @(16, 24, 32, 48, 64, 128, 256)

function New-RoundedPath([float]$x, [float]$y, [float]$w, [float]$h, [float]$r) {
    $p = New-Object System.Drawing.Drawing2D.GraphicsPath
    $d = $r * 2
    $p.AddArc($x, $y, $d, $d, 180, 90)
    $p.AddArc($x + $w - $d, $y, $d, $d, 270, 90)
    $p.AddArc($x + $w - $d, $y + $h - $d, $d, $d, 0, 90)
    $p.AddArc($x, $y + $h - $d, $d, $d, 90, 90)
    $p.CloseFigure()
    return $p
}

# ---------- master artwork (256 x 256 design space) ----------
$master = New-Object System.Drawing.Bitmap 256, 256, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($master)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$g.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
$g.Clear([System.Drawing.Color]::Transparent)

# navy rounded tile
$bgPath = New-RoundedPath 6 6 244 244 46
$bgBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    (New-Object System.Drawing.PointF 0, 0),
    (New-Object System.Drawing.PointF 256, 256),
    [System.Drawing.Color]::FromArgb(255, 34, 66, 104),
    [System.Drawing.Color]::FromArgb(255, 12, 30, 52))
$g.FillPath($bgBrush, $bgPath)
$bgPen = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(255, 58, 96, 140)), 2.5
$g.DrawPath($bgPen, $bgPath)

$goldLight = [System.Drawing.Color]::FromArgb(255, 255, 231, 163)
$goldMid = [System.Drawing.Color]::FromArgb(255, 246, 196, 69)
$goldDark = [System.Drawing.Color]::FromArgb(255, 201, 138, 18)
$goldLine = [System.Drawing.Color]::FromArgb(255, 110, 74, 6)

# handle (knob + neck)
$handleBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    (New-Object System.Drawing.PointF 116, 58),
    (New-Object System.Drawing.PointF 140, 90),
    $goldLight, $goldDark)
$handlePen = New-Object System.Drawing.Pen $goldLine, 3
$handlePen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
$g.FillRectangle($handleBrush, 123, 68, 10, 20)
$g.DrawRectangle($handlePen, 123, 68, 10, 20)
$g.FillEllipse($handleBrush, 117, 54, 22, 22)
$g.DrawEllipse($handlePen, 117, 54, 22, 22)

# bell body
$bellPath = New-Object System.Drawing.Drawing2D.GraphicsPath
$bellPath.AddBezier(84, 172, 84, 150, 92, 136, 94, 118)
$bellPath.AddBezier(94, 118, 96, 92, 112, 80, 128, 80)
$bellPath.AddBezier(128, 80, 144, 80, 160, 92, 162, 118)
$bellPath.AddBezier(162, 118, 164, 136, 172, 150, 172, 172)
$bellPath.CloseFigure()

$bellBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    (New-Object System.Drawing.PointF 88, 84),
    (New-Object System.Drawing.PointF 176, 188),
    $goldLight, $goldDark)
$blend = New-Object System.Drawing.Drawing2D.ColorBlend 3
$blend.Colors = [System.Drawing.Color[]]@($goldLight, $goldMid, $goldDark)
$blend.Positions = [single[]]@(0.0, 0.52, 1.0)
$bellBrush.InterpolationColors = $blend
$bellPen = New-Object System.Drawing.Pen $goldLine, 3
$bellPen.LineJoin = [System.Drawing.Drawing2D.LineJoin]::Round
$g.FillPath($bellBrush, $bellPath)
$g.DrawPath($bellPen, $bellPath)

# left highlight
$state = $g.Save()
$g.TranslateTransform(106, 124)
$g.RotateTransform(-18)
$hi = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(105, 255, 255, 255))
$g.FillEllipse($hi, -10, -32, 20, 58)
$g.Restore($state)

# rim
$rimPath = New-RoundedPath 67 164 122 20 9
$rimBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    (New-Object System.Drawing.PointF 67, 164),
    (New-Object System.Drawing.PointF 67, 184),
    [System.Drawing.Color]::FromArgb(255, 255, 226, 146),
    [System.Drawing.Color]::FromArgb(255, 185, 120, 10))
$g.FillPath($rimBrush, $rimPath)
$rimPen = New-Object System.Drawing.Pen $goldLine, 3
$g.DrawPath($rimPen, $rimPath)

# clapper
$clapBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    (New-Object System.Drawing.PointF 118, 184),
    (New-Object System.Drawing.PointF 138, 204),
    $goldMid, $goldDark)
$g.FillEllipse($clapBrush, 117, 183, 22, 22)
$g.DrawEllipse($rimPen, 117, 183, 22, 22)
$clapHi = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(120, 255, 255, 255))
$g.FillEllipse($clapHi, 122, 187, 7, 6)

$g.Dispose()

# ---------- frames ----------
$frames = @()
foreach ($s in $sizes) {
    $bmp = New-Object System.Drawing.Bitmap $s, $s, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $bg = [System.Drawing.Graphics]::FromImage($bmp)
    $bg.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $bg.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $bg.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
    $bg.DrawImage($master, 0, 0, $s, $s)
    $bg.Dispose()
    $ms = New-Object System.IO.MemoryStream
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    $frames += , @($s, $ms.ToArray())
    $bmp.Dispose()
    $ms.Dispose()
}

# ---------- write ICO ----------
$fs = [System.IO.File]::Create($OutPath)
$bw = New-Object System.IO.BinaryWriter $fs
$bw.Write([UInt16]0)
$bw.Write([UInt16]1)
$bw.Write([UInt16]$frames.Count)
$offset = 6 + 16 * $frames.Count
foreach ($f in $frames) {
    $s = [int]$f[0]
    $data = [byte[]]$f[1]
    $dim = if ($s -ge 256) { 0 } else { $s }
    $bw.Write([byte]$dim)
    $bw.Write([byte]$dim)
    $bw.Write([byte]0)
    $bw.Write([byte]0)
    $bw.Write([UInt16]1)
    $bw.Write([UInt16]32)
    $bw.Write([UInt32]$data.Length)
    $bw.Write([UInt32]$offset)
    $offset += $data.Length
}
foreach ($f in $frames) { $bw.Write([byte[]]$f[1]) }
$bw.Flush()
$bw.Close()
Write-Host ("Icon written: {0} ({1} bytes, {2} frames)" -f $OutPath, (Get-Item $OutPath).Length, $frames.Count)

# ---------- preview ----------
$preview = New-Object System.Drawing.Bitmap 820, 460
$pg = [System.Drawing.Graphics]::FromImage($preview)
$pg.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$pg.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::ClearTypeGridFit
$pg.Clear([System.Drawing.Color]::FromArgb(245, 248, 252))
$labelFont = New-Object System.Drawing.Font 'Segoe UI', 9
$titleFont = New-Object System.Drawing.Font 'Segoe UI', 11, ([System.Drawing.FontStyle]::Bold)
$labelBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(60, 76, 94))
$whiteBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(236, 242, 249))
$pg.DrawString('SchoolBells icon - light background', $titleFont, $labelBrush, 24, 18)

$x = 24
foreach ($s in @(256, 128, 64)) {
    $img = [System.Drawing.Image]::FromStream((New-Object System.IO.MemoryStream(, [byte[]]($frames | Where-Object { $_[0] -eq $s } | Select-Object -First 1)[1])))
    $pg.DrawImage($img, $x, 60, $s, $s)
    $pg.DrawString(('{0}px' -f $s), $labelFont, $labelBrush, $x, (60 + $s + 6))
    $x += $s + 26
    $img.Dispose()
}

$strip = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(18, 40, 66))
$pg.FillRectangle($strip, 0, 390, 820, 70)
$pg.DrawString('On dark taskbar:', $labelFont, $whiteBrush, 24, 402)
$x = 180
foreach ($s in @(48, 32, 24, 16)) {
    $img = [System.Drawing.Image]::FromStream((New-Object System.IO.MemoryStream(, [byte[]]($frames | Where-Object { $_[0] -eq $s } | Select-Object -First 1)[1])))
    $pg.DrawImage($img, $x, (426 - [int]($s / 2)), $s, $s)
    $pg.DrawString(('{0}' -f $s), $labelFont, $whiteBrush, ($x + $s + 4), 422)
    $x += $s + 42
    $img.Dispose()
}
$pg.Dispose()
$preview.Save($PreviewPath, [System.Drawing.Imaging.ImageFormat]::Png)
$preview.Dispose()
$master.Dispose()
Write-Host ("Preview written: {0}" -f $PreviewPath)
