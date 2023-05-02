# Blocky-Adventure
This project is created for the job application process. Game was made using Unity & C#.

***Features:***
- 1st player movement
- random voxel world generation using perlin texture
- 3 blocks - stone, grass, snow
- Destruction and placement of blocks

***In editor:***
- Use _Player_ prefab to set speed, jump height, etc. In _Main Camera_ you can set mouse sensitivity. 
- Use _World_ gameobject to set chunk Length, Width & Depth. (Number of voxels.)
- Set how many chunks to display around the current chunk on each side => (1 + value*2)^2 => if value is 2 number of chunks is 25.
- ***Seed*** - if value is 0 seed is random. Perlin noise generation is based on seed.

***In game:***
- use **WASD**, **arrows** or **gamepad** to move.
- use **mouse** or **gamepad right stick** to look around.
- jump using **spacebar** or **South Button** on _gamepad_.
- **Shift** and **Left Shoulder** (_gamepad_) is mapped for sprint.
