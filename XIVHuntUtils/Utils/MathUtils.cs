using System.Numerics;

namespace XIVHuntUtils.Utils;

public static class MathUtils {
	public static Vector2 V2(float x) => new(x, x);
	public static Vector2 V2(float x, float y) => new(x, y);
	
	public static Vector3 V3(float x) => new(x, x, x);
	public static Vector3 V3(Vector2 xy, float z) => new(xy.X, xy.Y, z);
	public static Vector3 V3(float x, float y, float z) => new(x, y, z);
	
	public static Vector4 V4(float x) => new(x, x, x, x);
	public static Vector4 V4(Vector2 xy, float z, float w) => new(xy.X, xy.Y, z, w);
	public static Vector4 V4(Vector3 xyz, float w) => new(xyz.X, xyz.Y, xyz.Z, w);
	public static Vector4 V4(float x, float y, float z, float w) => new(x, y, z, w);

	public static Vector2 Transpose(this Vector2 vec) => new(vec.Y, vec.X);
}
