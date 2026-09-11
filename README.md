# Puss in Hell

<p align="center">
  <img src="Puss%20in%20Hell%20SEO/Capsul%20%2B%20Biblio/logo.png" alt="Puss in Hell" width="360">
</p>

Монохромний червоний 3D-горор про маленьку кицьку, що заблукала в пеклі. Порожні коридори в стилі Backrooms, покинуті вулиці, глітч-ефекти на екрані, коли поруч щось є, і ліхтарик, який ніколи не рятує до кінця.

Гра від третьої особи з окремими зонами від першої особи. Створено на **Unity 6 (6000.3.8f1)**, **URP**, **Cinemachine**, **Timeline**.

<p align="center">
  <img src="Puss%20in%20Hell%20SEO/Sreenshots/screanshot%20(5).jpg" alt="Коридор" width="49%">
  <img src="Puss%20in%20Hell%20SEO/Sreenshots/screanshot%20(1).jpg" alt="Вулиця" width="49%">
</p>

---

## Геймплей

- **Дослідження.** Гравець керує кицькою (WASD, Shift — біг, Space — стрибок, F — ліхтарик). Камера — Cinemachine від третьої особи з колізіями, у тригерних зонах перемикається на камеру від першої особи без стрибка кута огляду.
- **Страх.** Біля ворожих NPC екран поступово «ламається» глітч-ефектом (Fronkon Games Glitches → Hacked) і звужується віньєтка. Коли інтенсивність перевищує поріг, персонаж переходить в анімацію страху та вмикаються візуальні ефекти.
- **Взаємодія.** Об'єкти підсвічуються контуром при наближенні (HaloHighlighter). Клавіша **E** активує їх: запускає анімацію, показує локалізовану підказку або відмічає прогрес. Коли всі потрібні об'єкти зібрано — стартує катсцена на Timeline.
- **Катсцени.** Тригерні зони та менеджер об'єктів запускають PlayableDirector з окремою віртуальною камерою, блокують управління й меню паузи. Пропуск — Esc.
- **AFK-анімації.** Якщо гравець стоїть без діла 10 секунд, кицька грає одну з двох випадкових idle-анімацій.
- **Кроки по поверхнях.** Звук кроків залежить від шару, на якому стоїть персонаж (бетон, дерево тощо).
- **Таргани.** Автономні NPC, що блукають по поверхнях, відбиваються від стін і видають звуки шурхоту.

## Меню та налаштування

- Інтро-відео → головне меню (Sad Main Menu Pack) → екран завантаження з відео → рівень.
- Меню паузи на Esc з паузою часу, звуками й поверненням у меню.
- Глобальний менеджер налаштувань (`DontDestroyOnLoad`, PlayerPrefs): гучність Master / FX / Music через AudioMixer, рівень якості, VSync, роздільна здатність.
- **Локалізація на 10 мов**: українська, англійська, російська, японська, німецька, французька, іспанська, турецька, італійська, польська. Тексти в UI (`LocalizedTMP`), випадаючі списки та ігрові підказки оновлюються миттєво при зміні мови.

## Структура проєкту

```
Assets/
├── Scenes/            intro, menu, loading, lvl1 (у білді), lvl2, lvl3 (у розробці)
├── Scripts/
│   ├── Player/        PlayerController, FlashlightToggle
│   ├── Cameras/       FirstPersonCamera, CameraSwitchTrigger, HorizontalCamera
│   ├── Cinematics/    CutsceneTrigger, HaloHighlighterCutsceneManager, CinematicCamera
│   ├── Objects/       HaloHighlighter, InteractiveHint, HintMessageManager, ObjectInteraction
│   ├── Npc/           CockroachController, NPCglitch (GameRenderManager, NPCRenderTrigger, Vignette)
│   ├── Menu/          PauseMenuManager, MainMenuManager, GlobalSettingsManager, локалізація
│   └── Audio/         DelayedAudioPlay
├── Prefabs/           Esc (меню паузи), Labirint, Video Player
├── Settings/          URP-ассети LOW / NORMAL / BEST, Volume-профілі GAME / HACKED
└── Materials/         моделі, аудіо, відео, шрифти та сторонні паки
Puss in Hell SEO/      логотип, капсули для Steam, скріншоти, тизер
```

## Ключові системи

| Система | Скрипт | Що робить |
|---|---|---|
| Рух | `PlayerController` | Rigidbody-рух відносно камери, біг, стрибок, кроки по шарах, стан страху, AFK |
| Глітч | `GameRenderManager` + `NPCRenderTrigger` | NPC щокадру звітують інтенсивність за відстанню, менеджер бере максимум і плавно застосовує до ефекту Hacked |
| Віньєтка | `VignetteRadiusTrigger` | Керує Vignette у Global Volume за відстанню до гравця |
| Камери | `CameraSwitchTrigger` | Перемикає пріоритети віртуальних камер, синхронізує yaw/pitch FPS-камери, ховає шар Player із culling mask |
| Підсвітка | `HaloHighlighter` | Додає outline-матеріал до рендерерів у радіусі, подія `OnAnyActivated` для менеджерів |
| Підказки | `HintMessageManager` | Синглтон із локалізованими повідомленнями за ключем, fade in/out |
| Завантаження | `VideoSceneLoader` | Асинхронно вантажить сцену, активує її після закінчення відео |

## Технології

- Unity 6000.3.8f1, Universal Render Pipeline 17.3
- Cinemachine 2.10, Timeline, Input (legacy `Input.GetKey`)
- TextMeshPro, Post Processing, Visual Effect Graph
- Fronkon Games — Glitches (ефект Hacked)
- Gabriel Bissonnette — Sad Main Menu Pack
- Backrooms Like Asset, Airduct BMT, AK Studio Art, Lowpoly Street Pack, Starfield Skybox та інші паки з Asset Store

## Запуск

1. Клонувати репозиторій.
2. Відкрити папку в Unity Hub через Unity **6000.3.8f1**.
3. Дочекатися імпорту (проєкт важкий, близько 2 ГБ ассетів).
4. Відкрити `Assets/Scenes/intro.unity` і натиснути Play. Порядок сцен у білді: intro → menu → loading → lvl1.

> У репозиторії немає вихідних архівів Blender з Backrooms-паку та сирих відеозаписів для трейлера — вони перевищують ліміт GitHub у 100 МБ на файл. На роботу проєкту це не впливає.

## Керування

| Клавіша | Дія |
|---|---|
| W A S D | Рух |
| Shift | Біг |
| Space | Стрибок |
| F | Ліхтарик |
| E | Взаємодія |
| Esc | Пауза / пропуск катсцени |

## Статус

Версія **0.1.5**, у розробці. Перший рівень грабельний, другий і третій — у роботі. Є тизер і матеріали для сторінки в Steam.

---

Автор: **MORENTY** · Cult of Code
