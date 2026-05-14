# Absynthium_Countryflags

<img width="771" height="29" alt="730_80" src="https://github.com/user-attachments/assets/c45ee56b-e834-41dd-9925-94ed5bb09ac9" />

Portage CounterStrikeSharp du plugin SourceMod Franug Country Flag Icons.

Le plugin remplace le badge/pin affiche dans le scoreboard CS2 par un index d'icone de drapeau pays. Les assets doivent etre fournis cote client via votre addon Workshop, puis charges sur le serveur par votre plugin/workflow Workshop habituel.

## Build

```powershell
dotnet build -c Release
```

Ou pour generer directement un package serveur propre :

```powershell
.\compile.ps1
```

Sortie principale :

```text
bin/Release/net8.0/
```

Sortie package :

```text
compiled/addons/counterstrikesharp/
compiled/csgo_addons/addons_absynthium/
compiled/Absynthium_Countryflags.zip
```

## Installation

Copier le contenu de `bin/Release/net8.0` dans :

```text
game/csgo/addons/counterstrikesharp/plugins/Absynthium_Countryflags/
```

Les JSON de config du plugin sont dans :

```text
game/csgo/addons/counterstrikesharp/configs/plugins/Absynthium_Countryflags/
```

Ajouter la base MaxMind GeoLite2 Country dans le dossier du plugin, pas dans `configs` :

```text
game/csgo/addons/counterstrikesharp/plugins/Absynthium_Countryflags/GeoLite2-Country.mmdb
```

La config principale propre est `Absynthium_Countryflags.json`. Les points importants :

- `CountryFlags`: correspondances pays -> ID badge/pin directement dans la config principale.
- `GeoLiteCountryDatabasePath`: chemin de la base GeoIP MaxMind, relatif au dossier du plugin.

## Workshop

Le plugin ne force pas le telechargement des fichiers d'icons et ne les ajoute pas au manifest CounterStrikeSharp. Il applique les IDs de `CountryFlags` dans le slot badge/pin du scoreboard, comme le plugin CS2FaceitLevels. Les images flags sont fournies dans l'addon Workshop avec les noms internes CS2 correspondant aux IDs utilises.

Le package contient un seul addon Workshop :

- `addons_absynthium`

Les assets classiques sont dans le meme addon :

```text
content/csgo_addons/addons_absynthium/panorama/images/econ/status_icons/classic_flags/
```

Le dossier `classic_flags/` sert de source. `compile.ps1` copie automatiquement ces fichiers vers la racine active :

```text
content/csgo_addons/addons_absynthium/panorama/images/econ/status_icons/
```

Les `.vtex_c` sont generes par le Resource Compiler dans `game/`.
