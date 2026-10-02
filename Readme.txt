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
