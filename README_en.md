# CyreneCutScene

CyreneCutScene is a tool for extracting cutscenes from "Honkai: Star Rail".

[ 切换至中文 ](README.md)

## Features

**Video Extraction**: Decrypt and extract videos

**Format Conversion**: FFmpeg-based copy mode conversion

| Stream Type | Container | Audio Support | Subtitle Support |
| --- | --- | --- | --- |
| `mpeg1` | `mp4` | 1 | 0 |
| `vp9` | `mkv` | MAX | MAX |
| `h264` | `mkv` | MAX | MAX |
| `hevc` | `mkv` | MAX | MAX |

**Graphical User Interface**: Built with WinUI3, featuring a custom Cyrene theme

- **B ♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪**
- **H ♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪**

## Usage

### Command Line Interface

**Basic Syntax**

```bash
CyreneCutScene [options]
```

**Parameter List**

| Alias | Full Name | Description | Default |
| --- | --- | --- | --- |
| `-i` | `--input` | Input file or directory path (multiple paths separated by `&`) | (Config.Extract.Input) |
| `-o` | `--output` | Output directory path | (Config.Extract.Output) |
| `-c` | `--convert` | Whether to convert format | (Config.Extract.Convert) |
| `-ma` | `--merge-audio` | Whether to merge audio | (Config.Extract.MergeAudio) |
| `-mc` | `--merge-caption` | Whether to merge subtitles | (Config.Extract.MergeCaption) |
| `-k` | `--keyfile` | Path to `VideoKeys.json` | (Config.Path.KeyPath) |
| `-f` | `--ffmpeg` | FFmpeg directory | (Config.Path.FFmpegPath) |
| `-r` | `--respath` | Resource directory path | (Config.Path.ResPath) |

### Graphical User Interface

**How to Use**

```bash
CyreneCutScene.exe
```

**Demo Video**

[Demo](https://github.com/user-attachments/assets/11c8cf97-6d4a-4c41-b1d1-29c28b5a3026)

## License

This project uses a **module-based licensing** model. Different directories are governed by different license terms.

### Core & CLI

All source code in the `CyreneCore` and `CyreneCLI` directories is licensed under the **GNU Affero General Public License v3.0 (AGPL-3.0)**.

### GUI

**All content under the `CyreneGUI` directory** (including but not limited to `.cs` source code, `.xaml` layout files, `Assets` resources, project configs, etc.) **is NOT open source. All rights reserved by the author (Cyrene2007).**

- Copying, extracting, modifying, or reusing any code from the `CyreneGUI` project is strictly prohibited.
- Copying, imitating, or reusing the UI layout, interaction logic, or visual style of this software is strictly prohibited.
- Extracting or using any images, icons, or audio resources from the `Assets` directory is strictly prohibited.
- Repackaging, redistributing, or selling the `CyreneGUI` project is strictly prohibited.
- The source code under `CyreneGUI` is for **reading and reference only** to demonstrate how to call the Core library, and must not be used in production or derivative projects.

---

中文版请参见 [README.md](README.md)
