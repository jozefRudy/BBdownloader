; -- Example1.iss --
; Demonstrates copying 3 files and creating an icon.

; SEE THE DOCUMENTATION FOR DETAILS ON CREATING .ISS SCRIPT FILES!

#define AppVersion "1.0.0"
#define AppName "BBdownloader"
#define Release "BBdownloader\bin\Release"
#define Root "BBdownloader"
#define Runners "runners"

[Setup]
AppVersion={#AppVersion}
AppName={#AppName}
DefaultDirName={pf}\BBdownloader
DefaultGroupName=BBdownloader
Compression=lzma2
SolidCompression=yes
OutputDir=Installation
SourceDir=..\
OutputBaseFilename= {#AppName +" v"+ AppVersion}

[Files]
Source: "{#Release}\*.*"; DestDir: "{app}"; 
Source: "{#Runners}\*.*"; DestDir: "{app}"; 

[Icons]
Name: "{group}\BBdownloader"; Filename: "{app}\start.bat"
Name: "{group}\testBBdownloader"; Filename: "{app}\test.bat"
Name: "{group}\Uninstall"; Filename: "{uninstallexe}"


