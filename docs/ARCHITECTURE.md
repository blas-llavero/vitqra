# Architecture

- Unity/OpenXR client: spatial panels, assistant avatars, accessibility, local settings, gesture input. Continue the existing C# implementation; Unity and necessary XR platform components are explicitly accepted proprietary exceptions.
- Research service: source adapters, canonical URLs, deduplication, provenance, cached translations, evidence review, persistent job checkpoints and cancellation.
- AI adapter layer: reviewed open-source local inference engines and separately licensed models. No commercial inference provider, paid API, cloud fallback, or required account. Fail closed if no local model is available. Public web research adapters are separate from inference. External web content is untrusted data, not instructions. Generated code runs only in a restricted sandbox with bounded resources and no secrets.
- Patient service: separate identity and storage, consent and lawful-basis review, encryption, revocation, access controls, aggregation and clinical governance. Never expose raw reports to peers by default. No experimental treatment instructions or unapproved trials.
- Presence: consent-based real-time avatar pose and voice; optional camera/video support; volumetric capture requires hardware and bandwidth evaluation.
- Media: independent review gate before publishing any asset. Use compatible open licenses and record provenance.

The desktop preview has no network implementation and no research job runner. Its settings model defines a readiness check but does not connect to providers. Live search adapters must honor website terms, rate limits and source freshness. No invented challenges or fabricated scientific results.

## Public notice discovery

The XR discovery panel queries a fixed public government endpoint with three public search terms. It validates source links and filters user keywords locally. No patient or conversation data is sent. Notices are unverified opportunities until official eligibility and deadlines are checked. See [SCIENTIFIC_DISCOVERY.md](SCIENTIFIC_DISCOVERY.md).
