param ([string]$source, [string]$dest)

Copy-Item -Path $source -Destination $dest -Container -Recurse -Force
#[Console]::Beep(600, 800)