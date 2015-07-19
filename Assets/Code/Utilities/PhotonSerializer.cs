using UnityEngine;
using System.Collections;
using Settworks.Hexagons;
using ExitGames.Client.Photon;

public static class PhotonSerializer
{
	public static void Register()
	{
		PhotonPeer.RegisterType(typeof(HexCoord), (byte)'C', SerializeHexCoord, DeserializeHexCoord);
	}

	public static byte[] SerializeHexCoord(object obj)
	{
		HexCoord coord = (HexCoord)obj;
		byte[] bytes = new byte[2 * 4];
		int index = 0;
		Protocol.Serialize(coord.q, bytes, ref index);
		Protocol.Serialize(coord.r, bytes, ref index);
		return bytes;
	}

	private static object DeserializeHexCoord(byte[] bytes)
	{
		int index = 0;
		int q;
		int r;
		Protocol.Deserialize(out q, bytes, ref index);
		Protocol.Deserialize(out r, bytes, ref index);
		return new HexCoord(q, r);
	}
}
