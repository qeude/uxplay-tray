# UxPlay Tray

Récepteur AirPlay pour Windows : [UxPlay](https://github.com/FDH2/UxPlay) compilé **sans Bonjour**
(mDNS interne d'UxPlay ≥ 1.74), avec GStreamer embarqué, et une icône dans la barre des tâches.
Mode **son uniquement** : le son de l'iPad/iPhone sort sur le PC, synchronisé avec l'image de l'iPad.

Sur l'iPad : Centre de contrôle → icône AirPlay de la carte audio → choisir le PC (pas « Recopie de l'écran »).

## Construire
Pousser ce dossier sur GitHub, puis **Actions → Build Windows → Run workflow**
(ou pousser un tag `v1.0` pour créer une Release). Résultat : `UxPlayTray-Setup.exe`.

## Utiliser
- L'installeur ajoute une règle de pare-feu (réseaux **privés** uniquement) et lance l'app.
- Clic droit sur l'icône :
  - **Décalage du son** : réglage fin si le son du PC n'est pas calé sur l'image de l'iPad (option `-async <ms>`).
  - **Latence minimale** (`-async no`) : pause/changement de piste plus réactifs, mais son plus calé sur l'image de l'iPad.
  - **Lancer au démarrage de Windows**, **Redémarrer**, **Ouvrir le journal**, **Quitter**.
- Icône verte = UxPlay tourne, grise = arrêté (voir le journal).

Si l'iPhone ne voit pas le PC : réseau Windows en « Privé », même Wi-Fi, pas d'isolation client sur la box.

Licence : GPLv3 (comme UxPlay). Chaque build inclut les licences, la liste des paquets embarqués (`SOURCES.txt`) et le code source exact d'UxPlay.
