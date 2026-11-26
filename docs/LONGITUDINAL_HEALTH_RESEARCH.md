# Longitudinal and preventive health research — proposed architecture

Status: author-requested product direction, not an implemented medical device, clinical service or validated prediction model. This extends the existing human-centered research and future mixed-reality community. Retain Unity/OpenXR, the current ten-language UI policy, original dependency notices and optional AI providers. No patient records, real photographs, medical datasets, credentials, external media or new dependency are introduced by this design milestone.

## Purpose and boundaries

Study disease trajectories and potentially detectable preclinical changes, and ultimately support clinicians with timely, reviewed longitudinal information before scheduled consultations. Not all conditions have detectable precursors; an association is not a causal effect or diagnosis. Do not promise prevention or predict a universal timeline. The existing educational research pipeline and its closed scientific/clinical release gates stay intact. No clinical decision, automated referral, individual prediction, prescribing, screening or patient alert is enabled by this change.

## Proposed system

1. Patient-controlled mobile/tablet capture and authorized clinical import, not requiring XR hardware. Explicit capture, not continuous surveillance. Allow clinician-approved symptom questionnaires, time-stamped photos of relevant visible changes, authorized laboratory and imaging reports, wearable observations and optional genetic data only if necessary and governed. Preserve originals separately from derived features. Capture time, availability time, time zone, units, modality, device, quality, provenance, missingness and consent/purpose scope; distinguish patient-reported, device-generated and clinician-verified information. Do not infer findings from identity or appearance. No arbitrary access to patient portals, camera feeds or scraping.
2. Private clinical data vault, separate from research and community data. Role- and purpose-based authorization, least privilege, verified clinician identity, encryption in transit and at rest, key lifecycle, revocation and retention/deletion controls, immutable audit events without raw sensitive contents, access review and incident response. Pseudonymized data remain personal data when reidentification is possible. Strip unnecessary image metadata, assess image and genomic reidentification risk; no raw photos or DNA in a shared room. No medical data in Unity PlayerPrefs, public GitHub, ordinary report ZIPs or consumer AI accounts. Keep identifiers separately controlled. Patient access and correction, consent withdrawal where applicable, and lawful retention exceptions require implementation.
3. Longitudinal research environment using approved datasets only. Distinguish clinical care from secondary research: separate legal bases, purpose, data agreements, access decisions and ethics approvals. Cohorts must include suitable comparison participants and time windows before documented diagnosis. Build condition-specific research hypotheses; use defined outcomes and clinically justified prediction horizons. Research datasets cannot be silently linked to a patient's record.
4. Multimodal analysis with reviewed task-specific models, rather than using a conversational model as a diagnostic classifier. Compare standardized images only when suitable, account for acquisition variability, use temporal biomarker measurements and missing-data handling. Explore longitudinal statistical and survival/time-to-event models, with a simple baseline before more complex models. A clinician-defined prediction time must exclude future measurements, diagnosis codes, treatment signals and other leakage. Use both event time and data-availability time. No patient-specific outputs in the initial prototype.
5. Clinician previsit workspace: a timeline of authorized original observations, trend plots, provenance, missing data, patient questions and reviewed summaries. In a future validated product, condition-specific risk estimates may be presented with horizon, uncertainty, model version, limitations and review status. Clinician controls interpretation and next steps; no automated diagnosis or treatment. Establish responsible reviewer, response-time expectations and escalation pathways before sending clinical alerts. Never tell a patient to postpone usual or urgent care because an AI signal is absent. No emergency monitoring claim.
6. Private XR collaboration: separate clinician-patient consultation, institution-controlled research room and moderated community. Users can opt into avatar or telepresence participation. Real-time sessions are not recorded by default. No medical data in shared spatial anchors, presence metadata, transcripts or avatar nameplates. XR visualization is supplementary; accessible 2D alternative always available. Telepresence remains optional and unimplemented.

## Validation and release gates

Start with generated synthetic trajectories containing explicitly fictional identifiers and nonclinical numerical features. Use synthetic data to test timestamp ordering, missingness, provenance, consent scopes and future-data exclusion; not to claim clinical accuracy. Real dataset experiments require approved access and a written analysis protocol. Split by participant and where possible site and time, never by randomly splitting images of the same person. Avoid leakage, report class prevalence and selection bias, and compare to a clinically relevant baseline. Assess sensitivity, specificity, positive and negative predictive value, calibration, uncertainty, lead time and false alerts per person-time at a predefined horizon and operating point. Report subgroup results with uncertainty; avoid claiming reliability on small samples. Assess missingness, distribution shift, external validation and prospective clinical utility, including harms from overdiagnosis, false reassurance and alert fatigue. Earlier detection alone does not demonstrate improved health outcomes. Clinical expertise, independent validation, ethics oversight, privacy/security assessment, and applicable regulatory requirements precede real clinical use. Evaluate the intended purpose with qualified regulatory specialists; clinician oversight alone does not remove medical-device obligations.

## Participant access

- Patient: own observations, consent settings, access history and clinician-reviewed information; choose whether to participate in research or community.
- Verified treating clinician: minimum necessary authorized clinical record and private consultation; accountable interpretation and reviewed previsit summary.
- Researcher: approved pseudonymized/minimized research data in a controlled environment, not patient identities or private consultations by default.
- Pharmaceutical or biotechnology organization: approved research workspace, aggregate disclosure-controlled findings by default; no automatic individual-record access, direct patient recruitment, marketing, profiling or independent prescribing. Any exceptional permitted individual-level research access must be separately governed and approved.
- Community participant: voluntary pseudonymous peer support, reviewed public information, no other patient's medical record. No inferred condition, automatic enrollment or disclosure of medical status. Role badges and conflict-of-interest declarations; moderation and safeguarding required.
- AI assistant: no independent access rights, no impersonation of a clinician, no raw patient or restricted dataset transfer to ChatGPT or other consumer AI. Future regulated deployments require a separate approved data-processing architecture, not reuse of the educational AI connector.

No room membership alone grants data access. Implement explicit resource-level authorization and separate clinical, research and community permissions. Keep a separate policy for voice, video, recording, and any secondary use. No real data until those controls and governance are independently reviewed.

## Data sources and interoperability

Potential research sources, not currently downloaded: ADNI (longitudinal clinical, imaging and biomarker research under its access terms); PhysioNet (open and controlled datasets, including MIMIC; access varies and some require training and individual agreements). Check purpose suitability and current dataset-specific agreements before any access, including restrictions on transfer to third-party AI. MIMIC hospital/ICU records are not automatically suitable for preclinical screening in the general population. Do not assume public visibility means open license or permission to reidentify or redistribute. Preserve dataset versions, permissions, provenance and quality. No scraping of patient photos or social media.

Use reviewed HL7 FHIR profiles for future authorized integration (Observation, DiagnosticReport, ImagingStudy, DocumentReference, Consent, Provenance, AuditEvent as applicable), DICOM for medical imaging and standard clinical terminology and units where licensed and appropriate. No actual EHR connection is authorized. EHDS implementation is phased; it is not a current unrestricted health-data API. For Spain/EU, assess GDPR Articles 6, 9, 25, 32, 35 and research safeguards as applicable, relevant Spanish health/privacy law, MDR/IVDR and AI Act classification and obligations. Identify the controller, processors and data protection officer where applicable, document impact assessment before high-risk processing, obtain ethics and institutional approval where necessary. Verify current legal requirements before deployment.

## Proposed implementation sequence

- v0.5.6: finish existing scientific validation and reproducibility milestone.
- v0.5.7: synthetic longitudinal schema and timeline prototype; provenance and time-cutoff checks.
- v0.5.8: approved-dataset research protocol and retrospective preclinical trajectory evaluation; no clinical deployment.
- v0.5.9: experimental multimodal feature research, condition-specific evaluation and uncertainty; no personal risk predictions in production.
- v0.6.0: private collaboration and community foundations, separate roles and resource permissions.
- v0.6.1: clinician-patient previsit workflow design and synthetic data UX, with future authorized clinical import behind an independent release gate.
- v0.6.2: researcher/industry collaboration with aggregate disclosure control and reviewed patient-facing education.
- v0.7.x: avatar and optional holographic telepresence integration, accessibility and privacy checks.
- Clinical pilot and regulated release are separate gates, not guaranteed by a software version. Preserve existing challenge search, research, reports and XR layout.

## References reviewed for this design

- https://adni.loni.usc.edu/data-samples/adni-data/
- https://physionet.org/about/database/
- https://mimic.mit.edu/docs/faq/how-to-get-access.html
- https://hl7.org/fhir/observation.html
- https://hl7.org/fhir/diagnosticreport.html
- https://health.ec.europa.eu/ehealth-digital-health-and-care/european-health-data-space-regulation-ehds_en
- https://eur-lex.europa.eu/eli/reg/2016/679/oj
- https://www.aemps.gob.es/productos-sanitarios/productos-sanitarios/

No clinical data processing or code is added in this milestone. No medical claims, model testing, Unity build or headset validation have been performed.
