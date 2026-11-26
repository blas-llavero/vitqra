# VITQRA — Open Mixed Reality Research Assistant

**Science without barriers.**

VITQRA is an open-source research assistant project for mixed reality, intended to support scientific discovery, accessible collaboration and responsible global health research. It targets Meta Quest 3 through OpenXR. Unity with OpenXR remains the chosen development platform. The VITQRA source is open source under Apache-2.0; Unity and necessary headset runtime components are explicitly accepted proprietary platform exceptions. No installable Quest application has been published.

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
3. The interface automatically uses the supported device language; otherwise it uses English. Restart the app after changing the device language.
4. The provider is restricted to local inference. No external AI requests are implemented or made. A reviewed local engine and model must be configured before research can start.
5. In XR, use Scientific challenges to refresh public notices and filter downloaded titles locally. Open the original source to verify rules and dates. The older desktop preview remains offline.
6. Save or retrieve a sample note. Use only non-sensitive notes: local preview storage is not encrypted.
7. Stop Play mode when finished. Save settings explicitly; sample notes persist locally.

## Features and status

| Feature | Status |
| --- | --- |
| Settings persistence, default automatic research and translation | Source implemented; Unity execution not verified |
| Ten-language core preview UI | Source implemented; translations need review |
| Non-sensitive note storage | Source implemented; Unity execution not verified |
| XR public-notice discovery and local keyword filtering | Source implemented; live API and device tests pending |
| Local-only AI and zero paid API policy | Preview guard implemented; no inference backend or network calls |
| Unity/OpenXR development platform | Retained by author decision; compilation and physical tests pending |
| World-space panels and controller movement | Source implemented; physical testing pending |
| Independent proportional window resizing | Source implemented; editor smoke checks and headset tests pending |
| Persistent spatial workspace | Source implemented; editor and headset tests pending |
| Quest passthrough and XR rig | Template setup documented; not verified |
| Procedural avatar selection, localized tips and review confirmation | Source implemented; Unity/headset tests pending |
| OK gesture with XR Hands | Optional source adapter; package/feature setup and device testing pending |
| Conversation history and text composer | Source implemented; keyboard and Quest testing pending |
| Optional persistent single-conversation history | Source implemented; device storage tests pending |
| Local microphone and Sherpa-ONNX STT/TTS adapters | Source implemented; native build, model provisioning and device tests pending |
| Natural speech and dictation | Requires reviewed native binaries and models; not verified |
| Browser | Planned |
| Additional biomedical/hackathon adapters, data import and research pipeline | Planned |
| Patient community and remote avatar presence | Planned; security and clinical governance gates apply |
| About and Help videos, branding and store screenshots | Pending author review; no media distributed |

## Supported languages

English (en), Spanish (es), Catalan (ca), Simplified Chinese (zh), Hindi (hi), French (fr), German (de), Portuguese (pt), Arabic (ar), Japanese (ja). English is the canonical language of code, identifiers, comments, documentation, GitHub metadata and default interface strings. Localized resource values are the deliberate exception. On launch, the app selects the device language if supported; otherwise it uses English. Regional variants share base translations (Chinese currently uses Simplified Chinese). Arabic shaping, right-to-left layout and international font coverage require testing. Full research translation and language-controlled generated AI responses are not implemented yet. See [localization policy](docs/LOCALIZATION.md).

## Open-source and zero-paid-API policy

All VITQRA-owned code and independently selectable functional dependencies must use verified open-source licenses, except for Unity and the necessary XR platform tooling and runtime components explicitly accepted by the author. Required model weights must have separately reviewed licenses, provenance and redistribution rights; availability of weights alone is not sufficient to establish open-source status. No paid API, required commercial account, cloud inference fallback or paid SDK is permitted. The paid API budget is permanently zero. The existing Unity preview now only exposes the local provider and rejects paid or non-local inference readiness. No model backend is connected yet.

See [Open-source and zero-paid-API policy](docs/OPEN_SOURCE_POLICY.md). The proprietary headset operating system, firmware and vendor runtime are unavoidable platform dependencies for Meta Quest and are outside VITQRA's licensing control. Internet access for public research sources is distinct from paid inference services.

## Privacy, scientific integrity and safety

The initial preview stores a plain-text sample note and non-secret preferences in Unity PlayerPrefs. Do not enter medical records, patient details, secrets or confidential research. Future medical-data functionality must be separately secured, access-controlled, consent-based and reviewed for applicable data protection and clinical requirements. AI-generated hypotheses are not validated treatments. Patient matching must not infer medication causality or facilitate self-experimentation.

See [architecture](docs/ARCHITECTURE.md), [roadmap](docs/ROADMAP.md) and [security](SECURITY.md).

## Contributing and license

Own source code is licensed under Apache-2.0 (see LICENSE). Unity and Meta SDKs are separately licensed third-party dependencies and are not redistributed in this package. Contributions must not include patient records, secrets, or unapproved media. See THIRD_PARTY_NOTICES.md.

## Mixed reality prototype (v0.4.0)

Four main independently movable world-space panels are provided as a Unity source overlay. The initial layout leaves the center open: Research Challenges and My Notes on the left, Smart Advice and AI Settings on the right. The avatar panel is hidden until Avatar settings is selected, temporarily replacing AI Settings until closed. A larger, independently movable Conversation panel replaces the temporary subtitle window and temporarily occupies the Smart Advice slot. It displays session history, supports optional unencrypted local history with deletion controls, and allows text entry through the supported platform keyboard (or desktop editor fallback). Minimize restores Smart Advice; AI Settings remains visible. A local microphone and Sherpa-ONNX STT/TTS adapter are provided as source; no native libraries or models are bundled and no AI provider is connected. See [voice setup](docs/VOICE_SETUP.md). Panels face the user at placement. Each window has normally hidden minus, percentage-reset and plus controls (70–150%); pointing at any edge for 0.5 seconds reveals them, and they hide after 1.5 seconds away from the edge and toolbar; A/X resets sizes and default positions relative to the current view. See [window resizing](docs/WINDOW_RESIZING.md). Positions, orientations, sizes and conversation visibility are saved locally and restored relative to the next session's initial head pose. Avatar settings remain closed at startup. See [persistent workspace](docs/SPATIAL_WORKSPACE.md). Sample actions and controller-based repositioning remain available. Use the Mixed Reality template and follow [Quest setup and physical test checklist](docs/QUEST_SETUP.md). The desktop preview remains available. Spatial labels are localized in ten languages; glyphs, RTL and layout are not verified. See [assistant setup](docs/ASSISTANT_SETUP.md) for the procedural avatar, tip review and optional OK gesture. No compiled speech engine or APK is provided.

## Build from source

The following instructions reproduce the current Unity preview. The Unity editor and applicable XR packages have separate license terms; verify free-tier eligibility and distribution requirements before building or releasing. No paid AI API is permitted.

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

See [scientific discovery setup](docs/SCIENTIFIC_DISCOVERY.md) for the bounded public-notice adapter, official portals and verification limits.
