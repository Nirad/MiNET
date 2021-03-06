namespace MiNET.Blocks
{
	public static class BlockFactory
	{
		public static Block GetBlockById(byte blockId)
		{
			Block block = new Block(blockId);

			if (blockId == 0) block = new BlockAir();

			if (blockId == 64) block = new BlockWoodenDoor();

			return block;
		}
	}
}