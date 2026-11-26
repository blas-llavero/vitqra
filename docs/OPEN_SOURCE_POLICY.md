# VITQRA — Mandatory open-source and zero-paid-API policy

## Non-negotiable requirements

- Original code remains Apache-2.0.
- Required functional libraries, independently selectable tools and self-hosted services must be open source with verified compatible licenses. The author explicitly accepts Unity and necessary XR platform packages, development tooling and runtime components as proprietary exceptions; no additional proprietary functional service is authorized. Review pinned versions and transitive dependencies; record SPDX identifiers, source URLs, hashes and notices before distribution.
- AI inference, translation, speech recognition, speech synthesis and object recognition must run locally on the headset or on user-controlled hardware using reviewed open-source engines and separately reviewed models. A local service interface is permitted; a paid remote API is not.
- Required model weights, voice models and datasets must have documented provenance and applicable use, modification and redistribution rights. Do not describe open weights as necessarily open-source AI. Reject unknown, noncommercial-only or incompatible licensing for required distributed components.
- No paid API, subscription requirement, mandatory ChatGPT/Claude account, proprietary inference SDK, metered commercial endpoint, or automatic cloud fallback. No requests when local inference is unavailable. The paid API budget is always zero; it is not configurable.
- Scientific discovery can access public websites and free public datasets with terms-compliant access. Public network access does not authorize sending chat, patient, audio or research data to external AI services. Keep explicit consent and access controls for separate collaboration functionality.
- No publication of any image, video, audio or external avatar asset before author approval.
- Costs of hardware, electricity, connectivity, optional self-hosting and platform publishing are not API charges and cannot be guaranteed to be zero.

## Engine decision

The author explicitly selected Unity over Godot. Retain the existing Unity C# implementation and continue with Unity 6 LTS and OpenXR, using compatible XR Hands and Meta Quest integration packages as needed. No engine migration is planned. Unity is proprietary, including its free Personal tier, and its applicable license terms and eligibility must be checked. Meta Quest firmware, operating system and vendor runtime are also proprietary. VITQRA's own code remains open source; the complete application stack cannot be described as 100% open source.

Preserve the approved UI design: central view clear; Research Challenges and My Notes left; large Conversation/Smart Advice slot right; AI Settings below; avatar visible only in its settings. All panels independently repositionable. Retain ten languages and automatic translation preference. Continue native local speech integration, opt-in conversation persistence and panel resizing within the existing Unity project. No paid inference service or mandatory commercial AI account is permitted.

## Candidate stack (not installed or integrated)

| Capability | Candidate | Review status |
| --- | --- | --- |
| XR engine | Unity 6 LTS | Proprietary, author-approved exception; license eligibility and package audit pending |
| XR standard | OpenXR | Open standard; implementation/runtime licenses vary |
| Local speech | sherpa-onnx | Apache-2.0 code; Android ARM64 and model audit pending |
| Local speech recognition | whisper.cpp | MIT code; model and performance audit pending |
| Local neural TTS | sherpa-onnx with compatible voices; Piper alternative | Voice license review per model; current Piper GPL obligations require review |
| Local language models | llama.cpp | MIT candidate; pin and verify code/model licenses independently |
| Storage | Local files or SQLite | License and encryption approach review pending |
| 3D assets | Blender | Open-source authoring tool; media approval and asset licenses required |

## Acceptance gates

1. No production release until the Unity application can be built from source with an audited dependency manifest and platform license requirements verified.
2. Local inference fails closed on missing models, non-local providers and nonzero or unknown API cost. No commercial API credentials or endpoints.
3. Verify each selected model supports its intended language and task; if a voice is unavailable, show the text without substituting a paid service.
4. Verify offline operation of voice/chat features with networking disabled after model installation. Benchmark memory, latency and thermal impact on Quest 3; offer a user-controlled local computer if the headset cannot run a model.
5. Verify headset passthrough, controller/hand input, keyboard, avatar settings and the central gap on hardware.
6. Persistent conversation history must be privacy-aware, with opt-in storage, clear deletion controls and no patient records in the prototype. Review sensitive-data security separately.
7. Media approval and license review before bundling or publishing assets.

## Reference documentation

- https://unity.com/products/unity-personal
- https://docs.unity3d.com/Manual/XR.html
- https://github.com/k2-fsa/sherpa-onnx
- https://github.com/ggml-org/whisper.cpp
- https://github.com/ggml-org/llama.cpp
- https://github.com/OHF-Voice/piper1-gpl/blob/main/docs/LICENSING.md
- https://unity.com/legal/terms-of-service

## Repository and application language

English is the canonical repository language; the app automatically follows the supported device locale with English fallback. See [LOCALIZATION.md](LOCALIZATION.md).
