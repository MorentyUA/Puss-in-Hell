# Puss in Hell

<p align="center">
  <img src="Puss%20in%20Hell%20SEO/Capsul%20%2B%20Biblio/logo.png" alt="Puss in Hell" width="360">
</p>

A monochrome red 3D horror game about a little kitty lost in hell. Empty Backrooms-style corridors, abandoned streets, glitch effects that creep onto the screen when something is near, and a flashlight that never quite saves you.

Third-person exploration with dedicated first-person zones. Built with **Unity 6 (6000.3.8f1)**, **URP**, **Cinemachine** and **Timeline**.

<p align="center">
  <img src="Puss%20in%20Hell%20SEO/Sreenshots/screanshot%20(5).jpg" alt="Corridor" width="49%">
  <img src="Puss%20in%20Hell%20SEO/Sreenshots/screanshot%20(1).jpg" alt="Street" width="49%">
</p>

---

## Gameplay

- **Exploration.** You control the kitty (WASD, Shift to run, Space to jump, F for the flashlight). The camera is a third-person Cinemachine rig with collision handling. Trigger zones switch to a first-person camera without any snap in the view direction.
- **Fear.** Near hostile NPCs the screen gradually "breaks" with a glitch effect (Fronkon Games Glitches → Hacked) and the vignette closes in. Once the intensity crosses a threshold the character enters a fear animation and extra visual effects kick in.
- **Interaction.** Objects get an outline when you approach. Pressing **E** activates them: plays an animation, shows a localized hint, or marks progress. When every required memory has been collected, a Timeline cutscene starts: the camera flies to the sink, the sink fills with blood, then a window shot follows and the monster lunges at the glass.
- **Cutscenes.** Trigger volumes and the collectibles manager drive a shared cutscene player: Timeline, its own virtual camera, audio, skip, player lock.
- **AFK animations.** Stand still for 10 seconds and the kitty plays one of two random idle animations.
- **Surface footsteps.** Footstep sounds depend on the layer the character stands on.
- **Cockroaches.** Autonomous NPCs that wander over surfaces, bounce off walls and scuttle audibly.

## Menu and settings

- Intro video → main menu (Sad Main Menu Pack) → loading screen with video → level.
- Pause menu on Esc with time scale pause, sounds and a return to the main menu.
- Global settings (`DontDestroyOnLoad`, PlayerPrefs): Master / FX / Music volume through an AudioMixer, quality level, VSync, screen resolution.
- **Localization in 10 languages**: English, Ukrainian, Russian, Japanese, German, French, Spanish, Turkish, Italian, Polish. UI texts, dropdowns and in-game hints update instantly when the language changes.

## Project structure

```
Assets/
├── Animations/        animator controllers and clips per character
├── Audio/             ambient, game, NPC sounds and the main mixer
├── Cinematics/        Timeline assets
├── Models/            own meshes only (FBX / DAE), one folder per model
├── Prefabs/           Player, Esc (pause menu), Video Player, Npc/
├── Scenes/            intro, menu, loading, lvl1 (in build), lvl2, lvl3 (in progress)
├── Scripts/           game code, see below
├── Settings/          URP assets LOW / NORMAL / BEST, volume profiles, physics materials
├── ThirdParty/        Asset Store packs, kept intact
├── Videos/            intro and in-game videos
└── Visual/
    ├── Images/        UI sprites and posters
    ├── Materials/     materials, one folder per model
    ├── Shaders/       BloodWaterURP, VisibleOutline, Vertex shader graph
    └── Textures/      textures, one folder per model
Puss in Hell SEO/      logo, Steam capsules, screenshots, teaser
```

## Code layout

Every script lives in a `PussInHell.*` namespace. Business logic and presentation are separate components: logic raises C# events or UnityEvents, views subscribe to them. No `Debug.Log`, no comments.

| Namespace | Logic | Presentation |
|---|---|---|
| `Core` | `PlayerControlLock`, `CutsceneState`, `PlayerLocator` | |
| `Player` | `PlayerMotor`, `Flashlight`, `PlayerFearState`, `PlayerIdleBehaviour` | `PlayerAnimationView`, `PlayerFootsteps`, `PlayerFearEffects`, `FlashlightView` |
| `Interaction` | `Interactable`, `ProximityHint`, `ObjectInteraction`, `TextTriggerDisplay` | `OutlineHighlight`, `HintSpriteView` |
| `Hints` | `HintMessageService`, `LocalizedMessage` | `HintMessageView` |
| `Cinematics` | `CutscenePlayer`, `CutsceneTrigger`, `CollectiblesCutscene`, `DelayedTeleport` | `CameraDollyShot`, `SinkBloodFill` |
| `Npc` | `GlitchIntensityHub`, `GlitchProximityTrigger`, `CockroachWander` | `VignetteProximity`, `CockroachView` |
| `Cameras` | `CameraSwitchTrigger` | `FirstPersonCamera`, `CinematicCamera`, `HorizontalCamera` |
| `Settings` | `GameSettings` | `VolumeSlidersUI`, `QualitySwitcherUI`, `ResolutionDropdownUI`, `VSyncToggle` |
| `Localization` | `LanguageProvider`, `GameLanguage` | `LocalizedTMP`, `LocalizedDropdown`, `LanguageDropdownUI` |
| `UI` | `PauseMenu`, `VideoSceneLoader` | `MainMenuManager`, `UIButtonSounds`, `FadeOutImage` |
| `Audio` | | `AudioFadeIn` |

`MainMenuManager` stays in the `GabrielBissonnette.SAD` namespace because the menu pack's custom editor depends on it.

## Tech

- Unity 6000.3.8f1, Universal Render Pipeline 17.3
- Cinemachine 2.10, Timeline, legacy Input (`Input.GetKey`)
- TextMeshPro, Post Processing, Visual Effect Graph
- Fronkon Games — Glitches (Hacked effect)
- Gabriel Bissonnette — Sad Main Menu Pack
- Backrooms Like Asset, Airduct BMT, AK Studio Art, Lowpoly Street Pack, Starfield Skybox and other Asset Store packs

## Getting started

1. Clone the repository.
2. Open the folder in Unity Hub with Unity **6000.3.8f1**.
3. Wait for the import to finish (the project is heavy, about 2 GB of assets).
4. Open `Assets/Scenes/intro.unity` and press Play. Build scene order: intro → menu → loading → lvl1.

> The repository does not include the Blender source archives from the Backrooms pack or the raw trailer footage: both exceed GitHub's 100 MB per-file limit. The project runs fine without them.

## Controls

| Key | Action |
|---|---|
| W A S D | Move |
| Shift | Run |
| Space | Jump |
| F | Flashlight |
| E | Interact |
| Esc | Pause / skip cutscene |

## Status

Version **0.1.5**, in development. The first level is playable, the second and third are in progress. A teaser and Steam page materials are ready.

---

Author: **MORENTY** · Cult of Code
