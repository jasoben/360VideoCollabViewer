# anthro_viewer_client

Repo for port of anthro viewer to Photon PUN 2 networking system.

## Features
* Peer 2 Peer with Relay Server using PUN2
* Networked Video Controls
* Subtitles
* Networked Laser Pointer
* Voice Chat using Photon Voice 2

### Network
Uses Photon PUN2. Devices can be located anywhere, service defaults to using EasternUS for all connected devices. Devices automatically connect to room XXX. No max devices in the room, though issue will probably present themselves as the numbers pass 16 users that would need to be addressed. PUN2 is Master/Client system. The UnityEditor or standalone PC build takesover as Master when connect. Oculus Go devices act as clients.

### Video Manager
Videos can be added to manager and will dynamically populate as list for host pc. Clicking on video loads content on host and clients. Videos have attached slots for subtitle text assets. 
Videos controls: play, pause, stop, seek. Controls are networked.

### Laser Pointer
Clients touch the Oculus Go controller touchpad to turn on their laser pointer. The “sphere” visualization of the laser pointer is networked and visible to all clients. Laser pointer and target alternated four colors: cyan, green, magenta, yellow

### Photon Voice v2
Voice chat handled by Photon Voice: 
* PhotonVOiceNetwork
* Recorder
* Photon Voice View
* Speaker 

## Getting Started
1. Download or clone repo.
2. Open in Unity2019.4.30f1 (2019 LTS Version) 
3. Open scene “Launcher”.

## Getting Started - Adding PUN2 and Photon VOice
1. Sign up for Photon: https://www.photonengine.com/en-US/Photon
2. Create PUN2 project in Photon Dashboard
3. Create Photon Voice v2 project in Photon Dashboard
4. Add Pun2 App Id to Photon Server Settings "Add Id Pun". Photon Server Settings found Window>Photon Unity Networking>Highlight Server Settings  
5. Add Voice App Id to Photon Server Settings "App Id Voice" 
6. Run in editor from launcher and confirm succesful connection to Room 

## Getting started - Deploy and test
1. Switch to Android Platform.1. 
2. Select build and run to deploy to Oculus Go devices.
2. Press play in editor to run host PC.
3. As Oculus Go devices are found, they will populate UI on host.

## Notes
* Photon PUN2 connects people together in Rooms. This project defaults everyone to "Anthro Room 1"
* PUN2 connects based on server region. This is overriden to have everyone connect to US East Server (us)
* If running host in Unity Editor make sure firewall settings allow inbound/outbound comms for editor version.
