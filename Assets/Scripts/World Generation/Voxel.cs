public class Voxel {
    public ScriptableBlock block;
    public int x;
    public int y;
    public int z;

    public Voxel(ScriptableBlock blockType) {
        block = blockType;
    }

    public ScriptableBlock GetBlock() {
        return block;
    }
}