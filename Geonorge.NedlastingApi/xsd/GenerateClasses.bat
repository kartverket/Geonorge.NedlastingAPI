@echo off
echo "============================ GENERATE C# classes ============================"

echo "Generate classes "
"C:\Program Files (x86)\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\xsd.exe" /nologo nedlastingapiv2.xsd rest.xsd /c /n:Geonorge.NedlastingApi.Models
copy /y nedlastingapiv2_rest.cs ..\ApiModels.cs
del nedlastingapiv2_rest.cs

pause
