# Absynthium_Countryflags

Plugin CounterStrikeSharp pour CS2 qui affiche le drapeau du pays d'un joueur dans le slot badge/pin du scoreboard.

Le pays est resolu via MaxMind GeoLite2 Country, puis converti en ID d'icone grace a la config `CountryFlags`. Les icones doivent etre disponibles cote client via un addon Workshop.

<img width="771" height="29" alt="730_80" src="https://github.com/user-attachments/assets/68352609-2d8e-4edb-ac69-b0d2e87757f4" />


## Dependances

Serveur:

- CounterStrikeSharp, API 80 minimum: https://github.com/roflmuffin/CounterStrikeSharp
- .NET 8 runtime, comme requis par CounterStrikeSharp.
- MaxMind GeoLite2 Country: fichier `GeoLite2-Country.mmdb`.
- MultiAddonManager: https://github.com/Source2ZE/MultiAddonManager

Build local:

- .NET 8 SDK.

## Build

```powershell
dotnet build -c Release
```

Package complet:

```powershell
.\compile.ps1
```

Sorties utiles:

```text
bin/Release/net8.0/
compiled/addons/counterstrikesharp/
compiled/csgo_addons/addons_absynthium/
compiled/Absynthium_Countryflags.zip
```

## Installation

Copier le plugin dans:

```text
game/csgo/addons/counterstrikesharp/plugins/Absynthium_Countryflags/
```

Fichiers attendus dans ce dossier:

```text
Absynthium_Countryflags.dll
Absynthium_Countryflags.deps.json
MaxMind.Db.dll
GeoLite2-Country.mmdb
```

Copier la config dans:

```text
game/csgo/addons/counterstrikesharp/configs/plugins/Absynthium_Countryflags/Absynthium_Countryflags.json
```

Si vous utilisez `compile.ps1`, le ZIP contient deja l'arborescence `addons/counterstrikesharp/` et `csgo_addons/addons_absynthium/`.

## Addon Workshop

Le plugin applique uniquement des IDs de badge. Il ne force pas le telechargement des images.

Publiez ou mettez a jour l'addon Workshop genere depuis:

```text
compiled/csgo_addons/addons_absynthium/
```

Puis ajoutez l'ID Workshop dans MultiAddonManager:

```text
game/csgo/cfg/multiaddonmanager/multiaddonmanager.cfg
```

Exemple:

```cfg
mm_extra_addons "1234567890"
mm_client_extra_addons "1234567890"
```

Remplacez `1234567890` par l'ID Workshop de votre addon. Si vous avez deja d'autres addons, separez les IDs par des virgules.

## Configuration du plugin

Fichier:

```text
game/csgo/addons/counterstrikesharp/configs/plugins/Absynthium_Countryflags/Absynthium_Countryflags.json
```

Exemple minimal:

```json
{
  "ConfigVersion": 1,
  "GeoLiteCountryDatabasePath": "GeoLite2-Country.mmdb",
  "CountryFlags": {
    "UNKNOWN": 1004,
    "FR": 1018,
    "BE": 911,
    "CH": 894,
    "CA": 4689
  }
}
```

Champs:

- `ConfigVersion`: version de la config CounterStrikeSharp.
- `GeoLiteCountryDatabasePath`: chemin vers `GeoLite2-Country.mmdb`. Un chemin relatif part du dossier du plugin.
- `CountryFlags`: table `code pays ISO 3166-1 alpha-2 -> ID badge/pin CS2`.
- `UNKNOWN`: ID utilise si l'IP ne peut pas etre resolue ou si le pays n'est pas mappe.

Les codes pays sont insensibles a la casse. Les IDs doivent correspondre aux icones presentes dans l'addon Workshop charge par MultiAddonManager.

## Notes

- Les bots sont ignores.
- Le badge est reapplique regulierement pour eviter qu'il soit ecrase par le jeu.
- Sans `GeoLite2-Country.mmdb`, tous les joueurs utilisent `UNKNOWN`.
- Sans addon Workshop charge cote client, le plugin peut appliquer l'ID mais l'icone ne s'affichera pas correctement.
# Absynthium_Countryflags

Plugin CounterStrikeSharp pour CS2 qui affiche le drapeau du pays d'un joueur dans le slot badge/pin du scoreboard.

Le pays est resolu via MaxMind GeoLite2 Country, puis converti en ID d'icone grace a la config `CountryFlags`. Les icones doivent etre disponibles cote client via un addon Workshop.

## Dependances

Serveur:

- CounterStrikeSharp, API 80 minimum: https://github.com/roflmuffin/CounterStrikeSharp
- .NET 8 runtime, comme requis par CounterStrikeSharp.
- MaxMind GeoLite2 Country: fichier `GeoLite2-Country.mmdb`.
- MultiAddonManager: https://github.com/Source2ZE/MultiAddonManager

Build local:

- .NET 8 SDK.

## Build

```powershell
dotnet build -c Release
```

Package complet:

```powershell
.\compile.ps1
```

Sorties utiles:

```text
bin/Release/net8.0/
compiled/addons/counterstrikesharp/
compiled/csgo_addons/addons_absynthium/
compiled/Absynthium_Countryflags.zip
```

## Installation

Copier le plugin dans:

```text
game/csgo/addons/counterstrikesharp/plugins/Absynthium_Countryflags/
```

Fichiers attendus dans ce dossier:

```text
Absynthium_Countryflags.dll
Absynthium_Countryflags.deps.json
MaxMind.Db.dll
GeoLite2-Country.mmdb
```

Copier la config dans:

```text
game/csgo/addons/counterstrikesharp/configs/plugins/Absynthium_Countryflags/Absynthium_Countryflags.json
```

Si vous utilisez `compile.ps1`, le ZIP contient deja l'arborescence `addons/counterstrikesharp/` et `csgo_addons/addons_absynthium/`.

## Addon Workshop

Le plugin applique uniquement des IDs de badge. Il ne force pas le telechargement des images.

Publiez ou mettez a jour l'addon Workshop genere depuis:

```text
compiled/csgo_addons/addons_absynthium/
```

Puis ajoutez l'ID Workshop dans MultiAddonManager:

```text
game/csgo/cfg/multiaddonmanager/multiaddonmanager.cfg
```

Exemple:

```cfg
mm_extra_addons "1234567890"
mm_client_extra_addons "1234567890"
```

Remplacez `1234567890` par l'ID Workshop de votre addon. Si vous avez deja d'autres addons, separez les IDs par des virgules.

## Configuration du plugin

Fichier:

```text
game/csgo/addons/counterstrikesharp/configs/plugins/Absynthium_Countryflags/Absynthium_Countryflags.json
```

Exemple minimal:

```json
{
  "ConfigVersion": 1,
  "GeoLiteCountryDatabasePath": "GeoLite2-Country.mmdb",
  "CountryFlags": {
    "UNKNOWN": 1004,
    "FR": 1018,
    "BE": 911,
    "CH": 894,
    "CA": 4689
  }
}
```

Champs:

- `ConfigVersion`: version de la config CounterStrikeSharp.
- `GeoLiteCountryDatabasePath`: chemin vers `GeoLite2-Country.mmdb`. Un chemin relatif part du dossier du plugin.
- `CountryFlags`: table `code pays ISO 3166-1 alpha-2 -> ID badge/pin CS2`.
- `UNKNOWN`: ID utilise si l'IP ne peut pas etre resolue ou si le pays n'est pas mappe.

Les codes pays sont insensibles a la casse. Les IDs doivent correspondre aux icones presentes dans l'addon Workshop charge par MultiAddonManager.

## Notes

- Les bots sont ignores.
- Le badge est reapplique regulierement pour eviter qu'il soit ecrase par le jeu.
- Sans `GeoLite2-Country.mmdb`, tous les joueurs utilisent `UNKNOWN`.
- Sans addon Workshop charge cote client, le plugin peut appliquer l'ID mais l'icone ne s'affichera pas correctement.
