# Blocky-Adventure

This project is created for the job application process. Game was made using Unity & C#.

**!!IMPORTANT - note that rendering chunks in not well-optimized and is laggy. Its not recommended to put high values into *Chunk Length Width*, *Chunk Depth* & *Chunks To Render Around*. For default I would let it be as it is. For more "smoother" experience, I would suggest keeping number of chunks at lowest value possible, chunk depth around value 20-30, and playing around with chunk length & width. (Build is a bit faster then editor playtest).**

## Features:

- 1st player movement
- random voxel world generation using perlin texture
- 3 blocks - stone, grass, snow
- Destruction and placement of blocks

## Controls, settings & setup

### In editor:

- Use *Player* prefab to set speed, jump height, etc. In *Main Camera* you can set mouse sensitivity.
  - Furthermore you can set Ray Distance (hand length) - Distance where you can destroy / place blocks.
  - Attack Delay - currently (default) set to 0.8f - This value determines in what time interval the "HoldAttack" action is executed.
  - Inventory blocks - blocks player can spawn (game is in "creative" mode).
- Use *World* gameobject to set chunk Length, Width & Depth (Number of voxels).
  - Set number of *Chunk To Render Around* (**NOCH**) around the current chunk on each side  => (1 + value\*2)^2 => if value is 2 number of chunks is 25.
  - Choose different block types for underground, ground and high level blocks.
- ***Seed*** - if value is set to "*0*" seed is random. Perlin noise generation is based on seed, chunk offset and scale.
- Create new blocks using **Create menu:** *Right click in project window -> Create -> Create New Block -> New Block*<br/>In **inspector** you can now set up *block name, destroy difficulty, maximum stack (which is useless at current state) and block material*.
 
*Block's **destroy difficulty** \* **attack delay** = actual time to destroy block (Snow 1\*0.8f, Grass 2\*0.8f, Rock 4\*0.8f).*

### In game:

- use **WASD**, **arrows** or **gamepad left stick** to move.
- use **mouse** or **gamepad right stick** to look around.
- jump using **spacebar** or **South Button** on *gamepad*.
- **Shift** and **Left Trigger** (*gamepad*) is mapped for sprint.
- **Left Mouse Button** or **Right Trigger** to destroy blocks (hold).
- **Right Mouse Button** or **Right Shoulder** to place blocks (single-click).
- **Scroll wheel** to switch blocks to place

## How it works:

### Creating new blocks

- Using basic sriptable objects

### Player Movement

- Using new input system

### World

#### Chunks

- Firstly are created abstract chunks (*NOCH* around + 1) which stores information about each voxels position. Their location & height is determined by perlin noise function.
- When they are created, game renders *NOCH* around default position (if **NOCH around** = 1, real number of chunks is 9).
- If player walks away from current chunk, new chunks are generated and old ones disappear.

#### Voxels

- Blocks are rendered in each rendered chunk.
- If block doesnt have any empty space around, it won't get mesh
- Blocks with atleast one empty space around get an BoxCollider. But actual face render gets only faces that dont have any neighbour block. Using other words, what player cant see is not being rendered.

### Player actions

#### Destroy block

- If player presses Attack button, ray is casted
- If ray hits a block, its informations are gained.
- If player holds Attack Button for specified time on the SAME block, it can be called destroyed.
- Destroy function is sent to world generation script, where can be handeled actual block destruction and update of neighbour blocks (for proper rendering).

#### Place block

- If player presses SecondaryAction button, ray is casted
- If ray hits a block, its informations are gained.
- Afterwards block spawn position is calculated.
- Place function is sent to world generation script, where can be handeled actual block creation and update of neighbour blocks (for proper rendering).

#### Switch item

-Scroll up or down switches current block which player can spawn.