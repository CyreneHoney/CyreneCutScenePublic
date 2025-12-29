# CyreneCutScene

CyreneCutScene 用于提取《崩坏：星穹铁道》的过场动画。

## 功能特性

**视频提取**: 解密与提取

**格式转换**: 基于FFmpeg的copy模式转换

| 流类型 | 容器格式 | 音频支持 | 字幕支持 |
| --- | --- | --- | --- |
| `mpeg1` | `mp4` | 1 | 0 |
| `vp9` | `mkv` | MAX | MAX |
| `h264` | `mkv` | MAX | MAX |
| `hevc` | `mkv` | MAX | MAX |

**图形化界面**: 基于 WinUI3，定制 Cyrene 主题风格

- **B ♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪**
- **H ♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪♪**

## 使用说明

### 命令行界面

**基本语法**

```bash
CyreneCutScene [options]
```

**参数列表**

| 参数别名 | 全称 | 描述 | 默认值 |
| --- | --- | --- | --- |
| `-i` | `--input` | 输入文件或目录路径 (多个路径用 `&` 分隔) | (Config.Extract.Input) |
| `-o` | `--output` | 输出目录路径 | (Config.Extract.Output) |
| `-c` | `--convert` | 是否转换格式 | (Config.Extract.Convert) |
| `-ma` | `--merge-audio` | 是否合并音频 | (Config.Extract.MergeAudio) |
| `-mc` | `--merge-caption` | 是否合并字幕 | (Config.Extract.MergeCaption) |
| `-k` | `--keyfile` | `VideoKeys.json` 路径 | (Config.Path.KeyPath) |
| `-f` | `--ffmpeg` | FFmpeg 目录 | (Config.Path.FFmpegPath) |
| `-r` | `--respath` | 资源目录路径 | (Config.Path.ResPath) |

### 图形化界面

**使用方法**

```bash
CyreneCutScene.exe
```

**演示视频**

[Demo](https://github.com/user-attachments/assets/11c8cf97-6d4a-4c41-b1d1-29c28b5a3026)

## 开源协议

本项目采用 **分模块许可** 模式。不同目录下的代码遵循完全不同的许可条款。

### 核心、命令行界面

`CyreneCore`和`CyreneCLI`目录中的所有源代码遵循 **GNU Affero General Public License v3.0 (AGPL-3.0)** 协议。

### 图形化界面

**`CyreneGUI` 目录下的所有内容**（包括但不限于 `.cs` 源代码、`.xaml` 布局文件、`Assets` 资源文件、项目配置等）**均不属于开源范围, 作者 (Cyrene2007) 保留所有权利**。

- 严禁复制、提取、修改或重用 `CyreneGUI` 项目中的任何代码。
- 严禁抄袭、模仿或复用本软件的 UI 布局、交互逻辑及视觉风格。
- 严禁提取或使用 `Assets` 目录下的任何图片、图标或音频资源。
- 严禁对`CyreneGUI`进行二次打包分发或商业销售。
- `CyreneGUI` 目录下的源代码仅供**阅读参考**，用于展示如何调用 Core 库，绝不允许用于生产或衍生项目。
