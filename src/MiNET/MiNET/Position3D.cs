namespace MiNET
{
	public class Position3D
	{
		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }

		public float Yaw { get; set; }
		public float Pitch { get; set; }
		public float BodyYaw { get; set; }

		public Position3D()
		{
		}

		public Position3D(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}
	}
}
