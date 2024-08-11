# Windows Power Service

## Allgemein

Der Windows Power Service ist ein Service, der es ermöglicht, Windows Power Pläne dynamisch zu verwalten. Dabei kann der Service zwischen zwei Plänen wechseln, die zuvor in einer Konfigurationsdatei festgelegt wurden. Der Wechsel der Pläne erfolgt auf Basis der CPU Auslastung. 

## Installation für Anwender

Die Installation für Anwender erfolgt über ein Installationsprogramm im MSI Format. Das Installationsprogramm kann über die [Releases](https://github.com/renegyetvai/Windows-Power-Service/releases) heruntergeladen werden. Nachdem das Installationsprogramm heruntergeladen wurde, kann es durch einen Doppelklick gestartet werden. Folgende Schritte sind notwendig, um den Windows Power Service zu installieren:

1. Starten des Installationsprogramms
2. Bestätigen der Benutzerkontensteuerung (UAC)
3. Starten einer Command Prompt (CMD) als Administrator
4. Starten des Windows Power Service mit dem Befehl `net start PowerService`

> [!NOTE] Der Windows Power Service kann auch über die Windows Dienste gestartet werden. Dazu muss der Dienst `Power Service` in der Diensteverwaltung gestartet werden.

> [!WARNING] Bei der ersten Ausführung des Windows Power Service wird zunächst eine Konfigurationsdatei erstellt. Nachdem der Dienst das entsprechende Verzeichnis und die Konfigurationsdatei darin erstellt hat, wird er automatisch gestoppt. Dies ist notwendig, da die Energieprofile initial nicht erstellt werden können und vom Nutzer eingetragen werden müssen. Die Konfigurationsdatei befindet sich im Verzeichnis `C:\ProgramData\PowerService` und heißt `config.json`.

> [!NOTE] Um die GUIDs der Energieprofile zu ermitteln, kann der Befehl `powercfg /list` in der Command Prompt (CMD) ausgeführt werden.

5. Bearbeiten der Konfigurationsdatei `config.json` im Verzeichnis `C:\ProgramData\PowerService`
6. Erneutes starten des Windows Power Service mit dem Befehl `net start PowerService`
