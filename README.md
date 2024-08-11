# Windows Power Service

## General

The Windows Power Service is a service that enables Windows Power plans to be managed dynamically. The service can switch between two plans that were previously defined in a configuration file. The change of plans is based on the CPU utilization. 

## Installation for users

The installation for users is carried out via an installation program in MSI format. The installation program can be downloaded via [Releases](https://github.com/renegyetvai/Windows-Power-Service/releases). Once the installation program has been downloaded, it can be started by double-clicking on it. The following steps are necessary to install the Windows Power Service:

1. start the installation program
2. confirm the user account control (UAC)
3. start a command prompt (CMD) as administrator
4. start the Windows Power Service with the command `net start PowerService`

> [!NOTE]
> The Windows Power Service can also be started via the Windows services. To do this, the service `Power Service` must be started in the service administration.

> [!WARNING]
> When the Windows Power Service is run for the first time, a configuration file is created first. After the service has created the corresponding directory and the configuration file in it, it is automatically stopped. This is necessary because the energy profiles cannot be created initially and must be entered by the user. The configuration file is located in the directory `C:\ProgramData\PowerService` and is called `config.json`.

> [!NOTE]
> To determine the GUIDs of the power profiles, the command `powercfg /list` can be executed in the command prompt (CMD).

5. edit the configuration file `config.json` in the directory `C:\ProgramData\PowerService`
6. restart the Windows Power Service with the command `net start PowerService`.

# Windows Power Service - German

## Allgemein

Der Windows Power Service ist ein Service, der es ermöglicht, Windows Power Pläne dynamisch zu verwalten. Dabei kann der Service zwischen zwei Plänen wechseln, die zuvor in einer Konfigurationsdatei festgelegt wurden. Der Wechsel der Pläne erfolgt auf Basis der CPU Auslastung. 

## Installation für Anwender

Die Installation für Anwender erfolgt über ein Installationsprogramm im MSI Format. Das Installationsprogramm kann über die [Releases](https://github.com/renegyetvai/Windows-Power-Service/releases) heruntergeladen werden. Nachdem das Installationsprogramm heruntergeladen wurde, kann es durch einen Doppelklick gestartet werden. Folgende Schritte sind notwendig, um den Windows Power Service zu installieren:

1. Starten des Installationsprogramms
2. Bestätigen der Benutzerkontensteuerung (UAC)
3. Starten einer Command Prompt (CMD) als Administrator
4. Starten des Windows Power Service mit dem Befehl `net start PowerService`

> [!NOTE]
> Der Windows Power Service kann auch über die Windows Dienste gestartet werden. Dazu muss der Dienst `Power Service` in der Diensteverwaltung gestartet werden.

> [!WARNING]
> Bei der ersten Ausführung des Windows Power Service wird zunächst eine Konfigurationsdatei erstellt. Nachdem der Dienst das entsprechende Verzeichnis und die Konfigurationsdatei darin erstellt hat, wird er automatisch gestoppt. Dies ist notwendig, da die Energieprofile initial nicht erstellt werden können und vom Nutzer eingetragen werden müssen. Die Konfigurationsdatei befindet sich im Verzeichnis `C:\ProgramData\PowerService` und heißt `config.json`.

> [!NOTE]
> Um die GUIDs der Energieprofile zu ermitteln, kann der Befehl `powercfg /list` in der Command Prompt (CMD) ausgeführt werden.

5. Bearbeiten der Konfigurationsdatei `config.json` im Verzeichnis `C:\ProgramData\PowerService`
6. Erneutes starten des Windows Power Service mit dem Befehl `net start PowerService`
