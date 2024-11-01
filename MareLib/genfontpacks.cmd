@echo off
for %%f in (*.ttf) do (
    mkdir "%%~nf_output"
    msdf-atlas-gen.exe -charset charset.txt -font "%%~nf.ttf" -size 64 -pxrange 12 -type msdf -format png -pots -imageout "%%~nf_output\font.png" -json "%%~nf_output\font.json"

    powershell -Command "Compress-Archive -Path '%%~nf_output\*' -DestinationPath '%%~nf.zip'"
    rmdir /s /q "%%~nf_output"
)