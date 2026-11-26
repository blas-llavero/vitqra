# Localization and repository language policy

## Canonical language

English is required for source identifiers, code comments, diagnostics, filenames, documentation, README, commit messages, release notes, issues, pull requests and build instructions. Localized interface resources are the intentional exception. User-authored notes, original scientific sources and conversation history retain their original language; never overwrite research evidence with a translation.

## Runtime language

English is the default and fallback. On each application launch, the device locale is detected automatically. Supported base language codes: en, es, ca, zh, hi, fr, de, pt, ar and ja. Regional variants such as ca-ES, en-GB, es-ES and pt-BR use their base language. Chinese currently uses Simplified Chinese for all zh variants. Unsupported or unavailable locale codes fall back to English.

On Android, Java Locale.getDefault().toLanguageTag() is preferred, with Unity system language as fallback when detection is unavailable. Desktop uses the current UI culture, then Unity system language when invariant. The locale is cached per application session. Changing the system language requires restarting VITQRA. Legacy manual language settings are reset to device mode; no manual selector is exposed.

UI labels, buttons, messages, advice and speech language selection must share the same resolved language. Future AI-generated responses, translated challenge text and synthesized speech should use that locale, subject to available audited models. Do not silently use another language's voice or paid translation API. Preserve source text and provenance. The prototype does not yet implement live translation or conversational AI.

## Translation maintenance

Use English strings as the source of truth. Every supported locale must include the same keys in the same order. Store localized values in the translation catalogs, not hard-coded in screen logic. Validate missing keys and row lengths before committing. New interface strings require ten language values. Preserve technical identifiers and canonical URLs.

## Verification checklist

- English, Catalan and Spanish device languages produce matching labels, advice, buttons and speech locale.
- Verify all ten locale codes, regional variants and unsupported languages (for example, Italian and Korean).
- Test a legacy saved language override; it must not override the device language.
- Test after device language change and application restart.
- Verify glyph coverage, text wrapping, Hindi shaping and Arabic right-to-left rendering on the headset.
- Verify localized model availability independently; unavailable voice stays text-only.
- Check repository prose and diagnostics in English; localization catalogs and user-provided content are exceptions.

Source-level checks do not establish rendering quality or actual Quest language availability. The headset may not expose every supported system locale. No new fonts or media are distributed here.
