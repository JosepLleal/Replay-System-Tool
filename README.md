# ACE Replay System 

ACE Replay is a simple state-based replay system. It was developed to quickly add replays to your game without the need to program anything. It includes two example scenes to show its utilities.

ACE Replay system only supports the record of transforms,  animations, audios, and particles, as well as the handling of instantiated and deleted objects. Keep in mind that this tool was done by a single person and was my final bachelor’s degree project, so I was time-limited.

The user can customize the replay duration, to use or not the interpolation optimization, and the recording intervals.

At the moment of the release, the replay is only suited for 3D games.

![alt text](https://raw.githubusercontent.com/JosepLleal/Replay-System-Tool/main/FinalImage.png)

## Video demonstration
ACE Replay video: (https://www.youtube.com/watch?v=le7Zx4tCHVQ&t=1s)

## Features:
- Scripts and prefabs to integrate the system quickly and easily. 
- Specify easily what should be recorded by dragging and dropping the script to the desired object, without the need to program.
- Example scenes to see how the system works and its utilities.
- Recording of transforms, animations, particles, and audios.
- Supports instantiation and deletion of recorded objects.
- Optimization to record at low frame rates without losing smoothness of replay, thanks to interpolation.
- Simple UI to control instant replay.
- Replay can be seen from different camera angles using the fly-around replay camera. The replay can also be seen from any of the existing scene cameras, including the gameplay camera.
- Supports playback at different speeds: x0.25, x0.5, x1, x2, x4.
- Supports frame-by-frame playback and reverse playback.
- Travel back in time mechanic.
- Fully C# commented code that can be extended upon need.



