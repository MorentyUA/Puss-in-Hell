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
- **Interaction.** Objects get an outline when you approach (HaloHighlighter). Pressing **E** activates them: plays an animation, shows a localized hint, or marks progress. When every required object has been collected, a Timeline cutscene starts.
- **Cutscenes.** Trigger volumes and the object manager start a PlayableDirector with its own virtual camera, lock player input and block the pause menu. Esc skips.
- **AFK animations.** Stand still for 10 seconds and the kitty plays one of two random idle animations.
- **Surface footsteps.** Footstep sounds depend on the layer the character stands on (concrete, wood, etc.).
- **Cockroaches.** Autonomous NPCs that wander over surfaces, bounce off walls and scuttle audibly.

## Menu and settings

- Intro video → main menu (Sad Main Menu Pack) → loading screen with video → level.
- Pause menu on Esc with time scale pause, sounds and a return to the main menu.
- Global settings manager (`DontDestroyOnLoad`, PlayerPrefs): Master / FX / Music volume through an AudioMixer, quality level, VSync, screen resolution.
- **Localization in 10 languages**: English, Ukrainian, Russian, Japanese, German, French, Spanish, Turkish, Italian, Polish. UI texts (`LocalizedTMP`), dropdowns and in-game hints update instantly when the language changes.

## Project structure

```
Assets/
├── Scenes/            intro, menu, loading, lvl1 (in build), lvl2, lvl3 (in progress)
├── Scripts/
│   ├── Player/        PlayerController, FlashlightToggle
│   ├── Cameras/       FirstPersonCamera, CameraSwitchTrigger, HorizontalCamera
│   ├── Cinematics/    CutsceneTrigger, HaloHighlighterCutsceneManager, CinematicCamera
│   ├── Objects/       HaloHighlighter, InteractiveHint, HintMessageManager, ObjectInteraction
│   ├── Npc/           CockroachController, NPCglitch (GameRenderManager, NPCRenderTrigger, Vignette)
│   ├── Menu/          PauseMenuManager, MainMenuManager, GlobalSettingsManager, localization
│   └── Audio/         DelayedAudioPlay
├── Prefabs/           Esc (pause menu), Labirint, Video Player
├── Settings/          URP assets LOW / NORMAL / BEST, Volume profiles GAME / HACKED
└── Materials/         models, audio, video, fonts and third-party packs
Puss in Hell SEO/      logo, Steam capsules, screenshots, teaser
```

## Core systems

| System | Script | What it does |
|---|---|---|
| Movement | `PlayerController` | Camera-relative Rigidbody movement, running, jumping, per-layer footsteps, fear state, AFK |
| Glitch | `GameRenderManager` + `NPCRenderTrigger` | Every NPC reports an intensity based on distance each frame; the manager takes the maximum and smoothly applies it to the Hacked effect |
| Vignette | `VignetteRadiusTrigger` | Drives the Vignette in the Global Volume by distance to the player |
| Cameras | `CameraSwitchTrigger` | Swaps virtual camera priorities, syncs the FPS camera yaw/pitch, hides the Player layer from the culling mask |
| Highlight | `HaloHighlighter` | Adds an outline material to renderers within a radius, fires `OnAnyActivated` for managers |
| Hints | `HintMessageManager` | Singleton with key-based localized messages and fade in/out |
| Loading | `VideoSceneLoader` | Loads the scene asynchronously and activates it once the video finishes |

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
