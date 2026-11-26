# Unity project and build preparation — v0.9.1

The development workflow uses the Unity Mixed Reality template for XR Origin, OpenXR settings, and the passthrough sample scene. The preparation tool adds the VITQRA source overlay to an existing Unity 6 project or stages a fresh project from Unity's official template archive.

## Prepare a Unity project

Use Python 3.12 or newer. Run the commands from the repository root with Unity closed.

### Use an existing Mixed Reality project

Create it in Unity Hub with Unity 6.3 LTS and the **Mixed Reality** template. Close the project and point the tool at its root directory.

```sh
python3 tools/prepare_unity_project.py "/absolute/path/VITQRA-Unity"
```

The tool installs `Assets/VITQRA` and stops if the destination or its Unity metadata already exists. The existing project's manifest, package lock, scenes and settings are preserved.

### Stage a new project from the official archive

Download the [Unity Mixed Reality 3.0.0 archive](https://download.packages.unity.com/com.unity.template.mixed-reality/-/com.unity.template.mixed-reality-3.0.0.tgz) and use a new destination under an existing directory.

```sh
python3 tools/prepare_unity_project.py "/absolute/path/VITQRA-Unity" \
  --template-archive "/absolute/path/com.unity.template.mixed-reality-3.0.0.tgz"
```

The tool verifies the archive's SHA-256 and stages its assets, metadata, sample scene, package manifest, and project settings with the VITQRA overlay. The new project targets Unity 6.3 LTS `6000.3.29f1` (`96d2fe17529c`). Its template manifest specifies OpenXR `1.17.1`, Unity OpenXR Meta `2.5.1`, XR Interaction Toolkit `3.5.1`, and AR Foundation `6.5.0`. Unity will resolve the project dependencies and write `Packages/packages-lock.json` on first import. The Unity template assets stay in the generated project under the Unity Companion License.

## Verify the first Unity import

Open the project in the chosen Unity version and let package resolution and script compilation finish. Check the Console for compiler errors. For a recorded batch check, start with a fresh log file and use the Unity executable for the chosen installation.

```sh
"/absolute/path/to/Unity" -batchmode -nographics -quit \
  -projectPath "/absolute/path/VITQRA-Unity" \
  -executeMethod Vitqra.Editor.VitqraBuildPreparation.VerifyBatch \
  -logFile "/absolute/path/vitqra-unity-import.log"
```

A successful import returns exit code `0` and writes `VITQRA_IMPORT_OK` with the Editor version and active build target. Record the commit, Unity version, target, package manifest, resolved package lock and import result. Review logs before sharing them.

## Prepare the Quest scene

Open `Assets/Scenes/SampleScene.unity` in the staged template and save an app scene copy before adding VITQRA. In that copy, select the tracked camera under XR Origin and use **VITQRA > Add Spatial Workspace to Selected XR Camera**. Check its tracking space, Android build profile, and both Java plugins using the [Quest setup guide](QUEST_SETUP.md). For a local 0.9.1 build, set the product name to `VITQRA`, the application identifier to `com.blasllavero.vitqra`, and the version to `0.9.1` in Player Settings. Keep the integration runner's empty test scene separate from the app scene.

The v0.9.2 milestone covers the 35 Unity Editor suites and fixes observed in the real project. Android ARM64/IL2CPP player build and Quest 3 acceptance are scheduled for v0.9.4. See the [integration and release gates](INTEGRATION_AND_RELEASE_GATES.md).

## Official references

- [Unity Editor 6000.3.29f1](https://unity.com/releases/editor/whats-new/6000.3.29f1)
- [Create an XR project in Unity 6.3](https://docs.unity.com/en-us/engine/6000.3/manual/xr/configuring-project-for/create-projects)
- [Unity 6.3 Editor command line arguments](https://docs.unity.com/en-us/engine/6000.3/manual/unity-editor/command-line-arguments/editor)
