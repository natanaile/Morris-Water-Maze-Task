# Virtual Morris Water Task

A virtual replica of the Morris water test for evaluating spatial cognition, developed in Unity.

## Overview

For evaluating the effects of a cognitive training program on spatial cognition, an assessment specific to that cognition fulfilling its natural properties should be used. While paper and pencil psychological assessments exist for evaluating spatial cognition, they do not transfer the feeling of movement, which is the main property of natural spatial tasks. Furthermore, in these assessments, participants observe the whole test's environment from above, whereas in natural spatial tasks, participants observe a portion of the environment from a first-person point of view [1].

Due to these discrepancies between natural spatial tasks and paper-based assessments, we developed this spatial assessment with more similarities to natural settings—a virtual replica of the standard Morris water test [2]. This is one of our team's previous Unity 3D designs [3]. The effectiveness of this test in assessing humans' spatial cognition has been demonstrated in [4], [5], and [6].

### Task Description

In the current replica of the test [3], participants are instructed to locate a fixed-position hidden target within a circular arena of radius 5 Virtual Units (VUs) by moving around the environment and using the distal cues present in the environment, such as different trees, that are learned and practiced during training trials.

**Target Location:**
- Fixed at the middle of the Northwest or Southwest quadrant of the arena
- Counter-balanced between participants and between baseline and post-intervention assessment sessions to minimize learning effects

**Starting Locations:**
- Alternates between South, North, East, and West across trials

**Trial Structure:**
- **Training Trials (4):** If users cannot locate the target within 45 seconds, the target becomes visible so they can learn and memorize its location with respect to distal cues
- **Test Trial (1):** Participants must locate the target within 45 seconds before the trial ends automatically. The target is not shown at the end

---

## Technical Requirements

### Unity Version
- **Recommended:** Unity 6000.2.1f1 or later
- **Original Development:** Unity 5.3.2f1

### Platform Support
- Windows (primary)
- Can be configured for other platforms

### Dependencies
The project uses the following Unity packages:
- Unity UI (`com.unity.ugui`)
- XR modules (optional, for VR headset support)

---

## Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/natanaile/Morris-Water-Maze-Task.git
   ```

2. **Open in Unity:**
   - Open Unity Hub
   - Click "Open" and navigate to the cloned folder
   - Select the project folder

3. **First Run:**
   - The project will create settings files on first run
   - Settings are stored in: `%USERPROFILE%\AppData\LocalLow\U of M Biomedical Engineering\VRN_MorrisWaterTask\`

---

## Controls

### Keyboard
| Key | Action |
|-----|--------|
| W / Up Arrow | Move Forward |
| S / Down Arrow | Move Backward |
| A / Left Arrow | Turn Left |
| D / Right Arrow | Turn Right |
| Spacebar | Confirm / Continue |
| Escape | Pause Menu |
| J | Toggle Decoupled Mode |

### Xbox Controller / Joystick
| Input | Action |
|-------|--------|
| Left Stick Up/Down | Move Forward/Backward |
| Left Stick Left/Right | Turn Left/Right |

---

## Configuration

### Settings Files

Settings are stored as XML files in the persistent data path:

#### ChairSettings.xml
Controls hardware and display settings:
- `HMDEnabled`: `true` for VR headset, `false` for desktop/laptop display
- `SerialPortID`: COM port for IMU sensor (if used)
- `QualityLevel`: `FAST` or `BEAUTIFUL`
- `VsyncEnabled`: Enable/disable vertical sync
- `EyeHeight`: Player eye height in meters

#### Controls.xml
Controls input sensitivity:
- `StickSensitivityX`: Rotation sensitivity
- `StickSensitivityY`: Forward/backward sensitivity
- `KeySensitivityFwdBwd`: Keyboard movement speed
- `KeySensitivityPivot`: Keyboard rotation speed

### Switching Between VR and Desktop Mode

**For Desktop/Laptop Mode (default):**
- `HMDEnabled` is set to `false` by default
- Game renders to the main display

**For VR Headset Mode:**
1. Edit `ChairSettings.xml`
2. Change `<HMDEnabled>false</HMDEnabled>` to `<HMDEnabled>true</HMDEnabled>`
3. Ensure VR headset is connected and configured in Unity

---

## Project Structure

```
Assets/
├── VRNWaterMaze/           # Main Morris Water Task scene and scripts
│   ├── WaterMaze.unity     # Main scene
│   ├── MorrisWaterTaskEnvironment.cs
│   ├── Oasis.cs            # Target/goal object
│   └── VRNWaterTaskSettings.cs
├── VRNController/          # Player controller and input handling
│   ├── Code/
│   │   ├── PlayerController.cs
│   │   ├── PCPlayerController.cs
│   │   ├── PCPlayerControllerVR.cs
│   │   └── Settings/       # Configuration classes
│   └── Prefabs/
├── VRNEnvironment/         # Abstract task environment framework
│   └── Code/
├── Standard Assets/        # Unity Standard Assets (utilities)
└── Editor/                 # Editor scripts
```

---

## Customization

### Arena Configuration
In the Unity Editor, select the environment object and modify:
- `Radius`: Arena size (default: 5 VUs)
- `Oasis Inset`: Target distance from edge
- `Player Start Inset`: Player starting distance from edge
- `Timeout Seconds`: Time before hint appears (training) or trial ends (test)

### Task Order
Edit `VRNWaterTaskOrder` settings to configure:
- Trial sequence
- Target positions
- Starting positions
- Trial types (visible target, invisible target, probe)

---

## Troubleshooting

### Common Issues

**"NO OASIS PREFAB SET!!!" error:**
- In Unity, select the MorrisWaterTaskEnvironment object
- Assign `Assets/VRNWaterMaze/Oasis.prefab` to the Oasis Prefab field

**Settings not loading / XML errors:**
- Delete corrupted settings files from the persistent data path
- The game will regenerate them with defaults

**Inverted joystick controls:**
- The project is configured for standard joystick orientation
- If inverted, check `Edit > Project Settings > Input Manager > Joy_Pitch`

**Serial port errors (COM4 not found):**
- This is normal if no IMU sensor is connected
- The game will fall back to keyboard/joystick input

---

## References

1. Zakzanis, K. K., Quintin, G., Graham, S. J., & Mraz, R. (2009). Age and dementia related differences in spatial navigation within an immersive virtual environment. *Medical Science Monitor*, 15(4), CR140-CR150.

2. Morris, R. G. M. (1981). Spatial localization does not require the presence of local cues. *Learning and Motivation*, 12(2), 239-260.

3. White, P. J. F. (2016). An Immersive Virtual Reality Navigational Tool for Diagnosing and Treating Neurodegeneration. M.Sc. Thesis, Dept. of Biomedical Engineering, Univ. of Manitoba, Winnipeg, Canada.

4. Hamilton, D. A., Driscoll, I., & Sutherland, R. J. (2002). Human place learning in a virtual Morris water task: some important constraints on the flexibility of place navigation. *Behavioral Brain Research*, 129(1-2), 159-170.

5. Astur, R. S., Taylor, L. B., Mamelak, A. N., Philpott, L., & Sutherland, R. J. (2002). Humans with hippocampus damage display severe spatial memory impairments in a virtual Morris water task. *Behavioural Brain Research*, 132(1), 77-84.

6. Moffat, S. D., & Resnick, S. M. (2002). Effects of age on virtual environment place navigation and allocentric cognitive mapping. *Behavioral Neuroscience*, 116(5), 851.

---

## Citation

If you use this project in your research, please cite our work:

```bibtex
@article{white2016neurocognitive,
  title={Neurocognitive Treatment for a Patient with Alzheimer's Disease Using a Virtual Reality Navigational Environment},
  author={White, P. J. and Moussavi, Z.},
  journal={Journal of Experimental Neuroscience},
  volume={10},
  pages={129},
  year={2016}
}
```

**Plain text citation:**

> P. J. White and Z. Moussavi, "Neurocognitive Treatment for a Patient with Alzheimer's Disease Using a Virtual Reality Navigational Environment," *J. Exp. Neurosci.*, vol. 10, p. 129, 2016.

---

## License

Copyright © University of Manitoba, Biomedical Engineering Department

---

## Authors

- P. J. White - *Initial development*
- Z. Moussavi - *Research supervision*
- Seyedsaber Mirmiran - *Unity 6000 upgrade & non-immersive desktop mode*

Biomedical Engineering Department, University of Manitoba, Winnipeg, Canada

---

## Changelog

### 2024 Update (Seyedsaber Mirmiran)
- Upgraded project from Unity 5.3.2f1 to Unity 6000.2.1f1
- Added non-immersive desktop/laptop mode (can now run without VR headset)
- Fixed obsolete Unity APIs for compatibility with modern Unity versions
- Added Xbox controller support with proper joystick orientation
- Implemented arena boundary constraint to keep player within the circular arena
- Improved settings file error handling and recovery
- Added comprehensive documentation

