# Synthetic object recognition — v0.8.0 initial

A closed local geometry recognition exercise. No camera, image files, passthrough capture, device sensors, real object data, medical equipment identification, learned AI, external model, downloads, network, recording, persistence or clinical release. No new dependencies or published media. Existing workspace and avatar visibility unchanged. The generated raster is computed from code, not an image file. The editor does not change runtime XR startup.

The exercise is opt-in, disabled by default. Turning off or changing inputs clears the previous result and preview. Only seven fixed synthetic shapes can be selected: circle, square, triangle, blank, line, ring and pair. The 24 by 24 binary grid is generated in memory. The first three classes have 196 templates each (radii 5–8, integer translations -3 through 3 on each axis). Nearest-template classification computes intersection over union (IoU), floored to thousandths, taking the maximum for each class. Accept only if the best score is at least 750/1000 and the gap to the second class at least 80/1000; otherwise Unknown, including ties. These thresholds are illustrative, not calibrated probability or real-world confidence. Seeded pixel flip attempts may repeat pixels. Invalid parameters clear prior output; the preview receives a defensive copy. Unknown is an explicit abstention, not proof that a real item is safe or absent.

## Evaluation

JavaScript reference reproduction ran 432 known-shape fixtures: 36 per class per flip-attempt level (0, 6, 18, 45). Four additional fixed unknown shapes were all rejected. Exact geometry and translations overlap the template bank; this is a synthetic sanity and corruption stress test, NOT independent external validation. The corrupted samples are deterministic and share the template generator. No noise model represents a real camera. Report correct, incorrect and abstained against all tested fixtures, not accuracy only among accepted items.

- 0 flip attempts: 108/108 correct, 0 abstained, 0 incorrect.
- 6 flip attempts: 108/108 correct, 0 abstained, 0 incorrect.
- 18 flip attempts: 108/108 correct, 0 abstained, 0 incorrect.
- 45 flip attempts: 44/108 correct, 64 abstained, 0 incorrect.

C# checks include the same reference confusion matrices, unknowns, default-off behavior, invalid inputs and clearing. They are supplied but not compiled or run in Unity. The independently executed reference is a behavioral reproduction, not an audit of C# execution or hardware. Core labels translated in ten device languages; technical menu names English.

## Editor usage and next steps

VITQRA > Synthetic Object Recognition Preview / Run Synthetic Object Recognition Checks. Enable the test, choose a generated shape and its parameters, then recognize. The displayed score is template similarity, not probability; margin is percentage points. The displayed grid is not camera imagery.

Future: review supported Quest camera/passthrough APIs, permissions and platform terms before adding hardware access; never assume passthrough access grants raw camera access. Choose reviewed open-source inference implementation and separately licensed model and training data; do not publish weights, images or 3D assets without author approval. Explicit opt-in, visible capture indication, local processing, no automatic capture/upload/history, minimal field of view, and privacy review for bystanders, room contents and documents. Contextual panels should show provenance, uncertainty and verified educational information, preserve the clear central view, and never identify drugs, medical devices or clinical findings for decision-making without appropriate validation. Real-object benchmarking with representative data, unknowns, occlusions, lighting, domain shifts, accessibility and physical headset testing remains pending. No real-world identification or clinical claim.
