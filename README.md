# Blocky-Adventure
This project is created for the job application process. Game was made using Unity & C#.

***Features:***
- 1st player movement
- random voxel world generation using perlin texture
- 3 blocks - stone, grass, snow
- Destruction and placement of blocks

***In editor:***
- Use *Player* prefab to set speed, jump height, etc. In *Main Camera* you can set mouse sensitivity. 
- Use *World* gameobject to set chunk Length, Width & Depth. (Number of voxels.)
  - Set how many chunks to display around the current chunk on each side => (1 + value\*2)^2 => if value is 2 number of chunks is 25.
  - Choose different block types for underground, ground ann high level blocks.
- ***Seed*** - if value is 0 seed is random. Perlin noise generation is based on seed.
- Create new blocks using **Create menu:** *Right click in project window -> Create -> Create New Block -> New Block*<br/>In **inspector** you can now set up *block name, destroy difficulty, maximum stack and block color*.
- 

***In game:***
- use **WASD**, **arrows** or **gamepad** to move.
- use **mouse** or **gamepad right stick** to look around.
- jump using **spacebar** or **South Button** on *gamepad*.
- **Shift** and **Left Shoulder** (*gamepad*) is mapped for sprint.
