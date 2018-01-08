d:
cd D:\Projects\JadeFlixBackend
cmd.exe /c dotnet publish -c Release -r linux-arm
"c:\Program Files\PuTTY\pscp.exe" D:\Projects\JadeFlixBackend\JadeFlix\bin\Release\netcoreapp2.0\linux-arm\publish\* pi@192.168.1.130:/home/pi/builds/jadeflix/back/
