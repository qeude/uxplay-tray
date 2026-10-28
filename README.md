# UxPlay Tray

Récepteur AirPlay pour Windows : [UxPlay](https://github.com/FDH2/UxPlay) compilé **sans Bonjour**
(mDNS interne d'UxPlay ≥ 1.74), avec GStreamer embarqué, et une icône dans la barre des tâches.

## Construire
Pousser ce dossier sur GitHub, puis **Actions → Build Windows → Run workflow**
(ou pousser un tag `v1.0` pour créer une Release). Résultat : `UxPlayTray-Setup.exe`.

## Utiliser
- L'installeur ajoute une règle de pare-feu (réseaux **privés** uniquement) et lance l'app.
- Clic droit sur l'icône :
  - **Décalage du son** : si l'image arrive après le son, choisir « Retarder le son » (option `-vsync <ms>`).
  - **Désactiver la synchro** (`-vsync no`) : pas d'images sautées, mais le son peut se décaler.
  - **Lancer au démarrage de Windows**, **Redémarrer**, **Ouvrir le journal**, **Quitter**.
- Icône verte = UxPlay tourne, grise = arrêté (voir le journal).

Si l'iPhone ne voit pas le PC : réseau Windows en « Privé », même Wi-Fi, pas d'isolation client sur la box.

Licence : UxPlay est sous GPLv3 ; le code source est sur https://github.com/FDH2/UxPlay.
