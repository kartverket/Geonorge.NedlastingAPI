@echo off
echo "============================ GENERATE C# classes ============================"

echo "Generate classes "
"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\xsd.exe" /nologo nedlastingapiv3.xsd rest.xsd /c /n:Geonorge.NedlastingApi.V3
copy /y nedlastingapiv3_rest.cs ..\ApiModelsV3.cs
del nedlastingapiv3_rest.cs

pause