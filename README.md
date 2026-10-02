FilTjek 1.0 – Windows
====================

GETTING STARTED
Extract the ZIP file. Double-click FilTjek.exe.
Keep FilTjek.exe.config in the same folder as the executable (required for long path support).
The application uses .NET Framework 4.8 and does not require Python or Microsoft Office to be installed.
It runs using the permissions of your normal Windows user account.
The application is not digitally signed with a publisher certificate.

USAGE
1. Enter C:\Data or a UNC path such as \\server\share\folder.
   Use the same Windows account that normally has access to the network share.
2. Select 1–10 years. The cutoff date is calculated from the time the scan starts.
3. Select a date filter. Default: BOTH the last modified date and the last access date
   must be older than the cutoff. Dates exactly on the cutoff are not included.
4. Optionally select file types, e.g. .pdf;.docx;.xlsx. Leave the field empty to include all file types.
5. Click Start Scan. You can stop the scan and save a partial report.
6. Click Save Report and select a location. A new report folder will be created.

REPORT
Filer.csv: All candidate files, including full path, file type, size in bytes,
last modified date, last access date, creation date, and the filter criteria used.
Semicolon-separated UTF-8 format suitable for Excel.

Rapport.txt: Summary, criteria, total size, and scan status.

Problemer.csv: Access errors, missing folders, and links that were skipped.

The application displays a maximum of 500 results; the exported report contains ALL results.
CSV text that could be interpreted as an Excel formula is prefixed with an apostrophe.

WHAT THE RESULTS MEAN
The report identifies files that may be unused. It does not prove that a person has not
opened or used them. Windows and NAS/server systems may omit or delay updates to the
last access timestamp. Backup software, antivirus software, and other applications may
also affect file timestamps.

The application cannot show who opened a file or reconstruct historical file access
that was not logged.

The last modified date indicates when the file itself was changed, not when its contents
were read. A recently copied file may retain an old modification date; also check the
Created date.

Dates are displayed using the local time zone of the computer running the scan.
Make sure its system clock is correct.

The application reads file metadata only, not file contents, and does not delete or move anything.

Links, junctions, and other reparse points are skipped to avoid loops and scanning outside
the selected folder. This may also exclude cloud-based files.

Review Problemer.csv before drawing conclusions. Errors may mean that entire subfolders
could not be examined. An empty report therefore does not prove that all files are in use.

File size is reported as logical bytes and does not necessarily represent the amount of
physical or reclaimable disk space.

An unavailable network server may delay stopping the scan until Windows returns a response.

Temporary reports are created in your TEMP folder and removed when the application closes normally.

VERIFICATION
Tested using local test files for:
all 1–10 year cutoff values, all three date filters, subfolders, file type filtering,
Danish filenames, unchanged last-access timestamps, missing folders, scan cancellation,
and full CSV export with more than 500 displayed results.

The user interface has been launched and visually verified.

An actual network share/NAS has not been tested in this environment.

SOURCES ON FILE TIMESTAMPS
https://learn.microsoft.com/en-us/windows/win32/sysinfo/file-times
https://learn.microsoft.com/en-us/dotnet/api/system.io.file.getlastaccesstime

SOURCE CODE
FilTjek.cs is included.
Build.ps1 builds the application using the C# compiler included with Windows.

FilTjek 1.0 – Windows
====================

START
Pak ZIP-filen ud. Dobbeltklik på FilTjek.exe.
Behold FilTjek.exe.config ved siden af exe-filen (understøttelse af lange stier).
Programmet bruger .NET Framework 4.8 og kræver ingen installation af Python
eller Office. Det kører med din almindelige Windows-brugers rettigheder.
Programmet er ikke digitalt signeret med et udgivercertifikat.

BRUG
1. Indtast C:\Data eller en UNC-sti som \\server\deling\mappe.
   Brug samme Windows-login, som normalt har adgang til netværksdelingen.
2. Vælg 1–10 år. Grænsen beregnes fra tidspunktet, scanningen starter.
3. Vælg datofilter. Standard: BÅDE sidst ændret og registreret adgang
   skal være ældre end grænsen. Dato præcis på grænsen medtages ikke.
4. Vælg eventuelt filtyper, fx .pdf;.docx;.xlsx. Tomt felt = alle typer.
5. Klik Start scanning. Du kan stoppe og gemme en delvis rapport.
6. Klik Gem rapport og vælg en placering. Der oprettes en ny rapportmappe.

RAPPORT
Filer.csv: alle kandidater, fuld sti, filtype, bytes, ændring, adgang,
oprettelse og anvendte filteroplysninger. Semikolonsepareret UTF-8 til Excel.
Rapport.txt: opsummering, kriterier, størrelse og scanningens status.
Problemer.csv: adgangsfejl, manglende mapper og links, der blev sprunget over.
Visningen i programmet viser højst 500 fund; eksporten indeholder ALLE fund.
CSV-tekst, der kan fortolkes som en Excel-formel, får en indledende apostrof.

HVAD RESULTATET BETYDER
Rapporten identificerer kandidater til ubrugte filer. Den dokumenterer ikke,
at et menneske ikke har åbnet eller brugt dem. Windows og NAS/servere kan
undlade eller forsinke opdatering af sidste adgang. Backup, antivirus og
andre programmer kan også påvirke tidsstemplerne. Programmet kan ikke vise,
hvem der åbnede filen, eller genskabe historisk filadgang, der ikke blev logget.
Sidst ændret siger noget om filens ændring, ikke om læsning af indholdet.
En nyligt kopieret fil kan have gammel ændringsdato: se også Oprettet.
Datoer vises i scannercomputerens lokale tidszone. Kontrollér dens ur.

Programmet læser filmetadata, ikke filindhold, og sletter/flytter intet.
Links/junctions og andre reparse points springes over for at undgå løkker
og scanning uden for den valgte mappe. Dette kan også udelade cloudfiler.
Se Problemer.csv før konklusioner. Fejl kan betyde, at hele undermapper
ikke kunne undersøges. En tom rapport beviser derfor ikke, at alt er i brug.
Størrelse er logiske bytes, ikke nødvendigvis fysisk eller frigørelig plads.
En utilgængelig netværksserver kan forsinke stop, indtil Windows svarer.
Midlertidige rapporter oprettes i din TEMP-mappe og fjernes ved normal lukning.

VERIFIKATION
Testet med lokale testfiler: grænser for alle 1–10 år, alle tre datofiltre,
undermapper, filtypefilter, danske filnavne, uændrede adgangstidsstempler,
manglende mappe, afbrydelse og fuld CSV-eksport ud over 500 viste fund.
Brugerfladen er startet og visuelt kontrolleret.
En rigtig netværksdeling/NAS er ikke testet i dette miljø.

KILDER OM FILTIDER
https://learn.microsoft.com/en-us/windows/win32/sysinfo/file-times
https://learn.microsoft.com/en-us/dotnet/api/system.io.file.getlastaccesstime

KILDEKODE
FilTjek.cs medfølger. Build.ps1 bygger programmet med Windows' C#-compiler.
