# Window resizing (v0.3.5)

Every spatial panel has independent proportional size controls above its top edge: minus, current percentage (press to reset to 100%), and plus. Controls are hidden by default. Aim the active tracked controller at any of the four edges continuously for 0.5 seconds to reveal the toolbar. The activation strip is 0.025 m wide in unscaled panel coordinates. Only the nearest visible panel accumulates dwell; moving away before the threshold resets it. The toolbar stays visible while pointing at an edge or its buttons, and hides after 1.5 seconds away from both, allowing time to reach the buttons. Hidden controls cannot receive clicks. Losing or switching controller tracking, gripping to move, disabling a panel or resetting the workspace hides its toolbar immediately. Frame time is unscaled and capped at 0.1 seconds to avoid revealing controls after a single stalled frame.

Aim at a visible button and press trigger. The controls are available on Research Challenges, My Notes, Smart Advice, AI Settings, Conversation, and the avatar settings panel whenever each is visible. Grip movement is unchanged. Eye gaze and hand-only pointing are not implemented for this feature.

- Initial scale: 100%. Step: 10 percentage points. Range: 70–150%.
- Normal panels at 100%: 0.64 × 0.34 m. Conversation at 100%: 0.86 × 0.90 m. Toolbar height is an additional 0.052 m, separated by 0.008 m.
- Uniform scaling preserves layout, aspect ratio and relative text/button size. Hit testing uses inverse transforms, so button regions scale with the visible panel.
- The percentage button resets only that panel's size, retaining position. A/X resets all sizes and default positions, closes avatar settings and respects whether the conversation is minimized.
- Scale is retained when hiding and reopening a panel during the current session. Persistent position and scale between application launches are not yet implemented.
- The center stays clear in the default layout. The user may manually move or enlarge panels into the center or another panel; no automatic collision prevention is implemented. Use grip to rearrange panels or reset the layout if needed.
- No network, paid APIs or external assets are required by this feature. The Size label uses all ten language catalogs; plus/minus and numeric percentages are language-neutral.

## Verification

In Unity Editor, select VITQRA > Run Window Resize Checks to run the included source-level editor smoke checks. They cover independent scaling, min/max clamps, reset, transformed toolbar hit regions, panel boundaries, interrupted dwell, half-second reveal, grace period, all four edge regions and hidden toolbar behavior. This menu has not been executed in the development environment.

Physical acceptance (pending): test all six panel types with both supported controllers, text legibility, large and small hit regions, independent movement and scaling, avatar hiding, conversation minimization/reopening, language rendering, visibility of the central physical environment and A/X reset. Verify no accidental activation while gripping and no interference with existing controls. Adjust bounds after actual Quest testing if needed.
