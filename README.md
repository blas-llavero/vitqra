# VITQRA — Open Mixed Reality Research Assistant

**Science without barriers.**

VITQRA is an open-source research assistant project for mixed reality, intended to support scientific discovery, accessible collaboration and responsible global health research. It targets Meta Quest 3 using Unity and OpenXR. Version 0.3.2 is a source-only desktop and spatial prototype, not an installable Quest application.

## Installation location

- Planned APK downloads: https://github.com/blas-llavero/vitqra/releases (not published yet).
- Suggested Windows source location: `C:\Projectes\vitqra\`.
- Android application ID (planned): `com.blasllavero.vitqra`.

## Author and mission

Conceived and programmed by **Blas Llavero**. More information: https://blasllavero.com.

VITQRA is open source: everyone may use, study, modify and redistribute its own code under Apache-2.0, including for charitable purposes. The project's mission is to reduce barriers to science, health knowledge, technology access and international research collaboration. No cure, medical outcome or scientific discovery is guaranteed.

## Download and installation

No APK or public release is available at this milestone. Clone this repository or download its source ZIP. To run the preview, follow Build from source at the end of this README. Do not install an APK from an unverified source.

## How to use

1. Open the generated preview scene and press Play in Unity.
2. Review automatic research and translation settings (enabled by default).
3. Select a language or use the device language; English is the fallback.
4. Select an AI provider. No external requests are implemented or made in this milestone. A configured provider, validated credentials and a budget gate are required in a future integration.
5. Select a research topic or type a search query. The preview deliberately returns no results because a live source adapter is not implemented.
6. Save or retrieve a sample note. Use only non-sensitive notes: local preview storage is not encrypted.
7. Stop Play mode when finished. Save settings explicitly; sample notes persist locally.

## Features and status

| Feature | Status |
| --- | --- |
| Settings persistence, default automatic research and translation | Source implemented; Unity execution not verified |
| Ten-language core preview UI | Source implemented; translations need review |
| Non-sensitive note storage | Source implemented; Unity execution not verified |
| Topic selection and editable search | Preview only; no live search |
| AI providers and spending limits | Configuration only; no credentials or network calls |
| World-space panels and controller movement | Source implemented; physical testing pending |
| Quest passthrough and XR rig | Template setup documented; not verified |
| Procedural avatar selection, localized tips and review confirmation | Source implemented; Unity/headset tests pending |
| OK gesture with XR Hands | Optional source adapter; package/feature setup and device testing pending |
| Conversation history and text composer | Source implemented; keyboard and Quest testing pending |
| Natural speech, dictation and browser | Planned |
| Research and hackathon discovery, data import and research pipeline | Planned |
| Patient community and remote avatar presence | Planned; security and clinical governance gates apply |
| About and Help videos, branding and store screenshots | Pending author review; no media distributed |

## Supported languages

English (en), Spanish (es), Catalan (ca), Simplified Chinese (zh), Hindi (hi), French (fr), German (de), Portuguese (pt), Arabic (ar), Japanese (ja). Core strings are included. Arabic shaping, right-to-left layout and international font coverage require testing. Full research translation is not implemented yet.

## AI and costs

The initial paid budget is zero. OpenAI API, Anthropic and other commercial APIs may incur charges independently of consumer subscriptions. Prefer a local provider such as Ollama for no external API charges, and cache outputs. Do not embed credentials in Unity source, settings files or APKs. Codex integration is planned through supported interfaces only. Provider selection in the preview is not authentication and never starts network activity.

## Privacy, scientific integrity and safety

The initial preview stores a plain-text sample note and non-secret preferences in Unity PlayerPrefs. Do not enter medical records, patient details, secrets or confidential research. Future medical-data functionality must be separately secured, access-controlled, consent-based and reviewed for applicable data protection and clinical requirements. AI-generated hypotheses are not validated treatments. Patient matching must not infer medication causality or facilitate self-experimentation.

See [architecture](docs/ARCHITECTURE.md), [roadmap](docs/ROADMAP.md) and [security](SECURITY.md).

## Contributing and license

Own source code is licensed under Apache-2.0 (see LICENSE). Unity and Meta SDKs are separately licensed third-party dependencies and are not redistributed in this package. Contributions must not include patient records, secrets, or unapproved media. See THIRD_PARTY_NOTICES.md.

## Mixed reality prototype (v0.3.2)

Four main independently movable world-space panels are provided as a Unity source overlay. The initial layout leaves the center open: Research Challenges and My Notes on the left, Smart Advice and AI Settings on the right. The avatar panel is hidden until Avatar settings is selected, temporarily replacing AI Settings until closed. A larger, independently movable Conversation panel replaces the temporary subtitle window and temporarily occupies the Smart Advice slot. It displays session history and allows text entry through the supported platform keyboard (or desktop editor fallback). Minimize restores Smart Advice; AI Settings remains visible. Voice and AI provider adapters are not connected. Panels face the user at placement; A/X restores the default layout relative to the current view. Sample actions and controller-based repositioning remain available. Use the Mixed Reality template and follow [Quest setup and physical test checklist](docs/QUEST_SETUP.md). The desktop preview remains available. Spatial labels are localized in ten languages; glyphs, RTL and layout are not verified. See [assistant setup](docs/ASSISTANT_SETUP.md) for the procedural avatar, tip review and optional OK gesture. No speech engine or APK is provided.

## Build from source

### 1. Install dependencies

- Install Unity Hub and a supported Unity 6 LTS editor with Android Build Support, Android SDK & NDK Tools and OpenJDK.
- For later Quest builds, install Meta Quest Developer Hub and enable developer mode on your own headset.

### 2. Create the initial project

Create a new Unity 6 Universal 3D project called `VITQRA` in Unity Hub. Copy `UnityStarter/Assets/VITQRA` into its `Assets` directory. Wait for Unity to import and compile scripts. This distribution is a source overlay, not a complete generated Unity project.

### 3. Run the desktop preview

In Unity, select **VITQRA > Create Desktop Preview Scene**. Save the scene when prompted, then press Play. The current immediate-mode UI is a desktop prototype; it is not a world-space XR interface.

### 4. Prepare the future Quest target

Switch the build profile to Android. Install XR Plug-in Management, OpenXR, Unity OpenXR: Meta and the XR interaction packages using versions compatible with the installed editor. Enable OpenXR and the Meta Quest feature group for Android. Set the application identifier to `com.blasllavero.vitqra`, select ARM64 and IL2CPP and resolve project validation issues. Follow Meta's official instructions for Android and headset requirements. A tested XR rig, world-space interface and passthrough implementation must be added before generating a usable Quest build.

### 5. Verification

Unity and the headset have not been available to test this source package. Do not treat a successful import or desktop preview as Quest compatibility. Record editor version, package versions, build logs and physical-device results for subsequent milestones.
