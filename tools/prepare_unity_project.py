"""Stage VITQRA sources in a Unity 6 project using Python 3.12 or newer."""

import argparse
import hashlib
import json
from pathlib import Path, PurePosixPath
import shutil
import tarfile
import tempfile


OVERLAY = Path(__file__).resolve().parents[1] / "UnityStarter" / "Assets" / "VITQRA"
TEMPLATE_SHA256 = "16ca46bbc07cb71b7e440fae47642b857d51b1e3a791ac429cf0afad0b39eead"
EDITOR_VERSION = "6000.3.29f1"
EDITOR_REVISION = "96d2fe17529c"


def occupied(path):
    return path.exists() or path.is_symlink()


def move_new(staged, destination):
    if occupied(destination):
        raise FileExistsError(f"Destination already exists: {destination}")
    staged.rename(destination)


def add_overlay(project):
    assets = project / "Assets"
    if not assets.is_dir() or assets.is_symlink():
        raise ValueError(f"Expected an Assets directory in {project}")
    version = (project / "ProjectSettings" / "ProjectVersion.txt").read_text(encoding="utf-8-sig")
    if not any(line.startswith("m_EditorVersion: 6000.") for line in version.splitlines()):
        raise ValueError("Expected a Unity 6 project")
    manifest = json.loads((project / "Packages" / "manifest.json").read_text(encoding="utf-8-sig"))
    if not isinstance(manifest, dict) or not isinstance(manifest.get("dependencies"), dict):
        raise ValueError("Expected a Unity package manifest with dependencies")

    destination = assets / "VITQRA"
    if occupied(destination) or occupied(assets / "VITQRA.meta"):
        raise FileExistsError(f"Existing VITQRA assets or metadata in {assets}")
    with tempfile.TemporaryDirectory(prefix=".vitqra-overlay-", dir=project) as temp:
        staged = Path(temp) / "VITQRA"
        shutil.copytree(OVERLAY, staged)
        move_new(staged, destination)


def template_member(member, destination):
    parts = PurePosixPath(member.name).parts
    if parts == ("package", "LICENSE.md"):
        return tarfile.data_filter(member.replace(name="LICENSE-Unity-Template.md"), destination)
    if len(parts) < 3 or parts[:2] != ("package", "ProjectData~"):
        return None
    relative = PurePosixPath(*parts[2:])
    if relative.parts[0] not in {"Assets", "Packages", "ProjectSettings"}:
        return None
    if relative == PurePosixPath("Packages/packages-lock.json"):
        # The source archive's lock includes the template as an embedded package.
        return None
    if ".." in relative.parts or not (member.isfile() or member.isdir()):
        raise tarfile.FilterError(f"Unexpected template entry: {member.name}")
    return tarfile.data_filter(member.replace(name=relative.as_posix()), destination)


def create_project(project, template):
    if occupied(project):
        raise FileExistsError(f"Destination already exists: {project}")
    with template.open("rb") as source:
        if hashlib.file_digest(source, "sha256").hexdigest() != TEMPLATE_SHA256:
            raise ValueError("SHA-256 mismatch for the Unity Mixed Reality 3.0.0 archive")
        source.seek(0)
        with tempfile.TemporaryDirectory(prefix=".vitqra-project-", dir=project.parent) as temp:
            staged = Path(temp) / "project"
            staged.mkdir()
            with tarfile.open(fileobj=source, mode="r:gz") as archive:
                archive.extractall(staged, filter=template_member)
            (staged / "ProjectSettings" / "ProjectVersion.txt").write_text(
                f"m_EditorVersion: {EDITOR_VERSION}\n"
                f"m_EditorVersionWithRevision: {EDITOR_VERSION} ({EDITOR_REVISION})\n",
                encoding="utf-8",
            )
            add_overlay(staged)
            move_new(staged, project)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("project", type=Path, help="Unity project directory")
    parser.add_argument("--template-archive", type=Path, help="Official Mixed Reality 3.0.0 .tgz for a new project")
    args = parser.parse_args()
    project = args.project.absolute()

    try:
        if args.template_archive is None:
            add_overlay(project)
        else:
            create_project(project, args.template_archive)
    except (OSError, ValueError, tarfile.TarError) as error:
        parser.exit(1, f"Unity project preparation failed: {error}\n")

    print(f"VITQRA sources staged in {project}")
    print("Open the project in Unity to resolve packages and compile the scripts.")


if __name__ == "__main__":
    main()
