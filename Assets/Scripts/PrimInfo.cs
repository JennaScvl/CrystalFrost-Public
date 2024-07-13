using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibreMetaverse;
using OpenMetaverse;

public class PrimInfo : MonoBehaviour
{
	public uint localID = 0;
	public int face = -1;
	public UUID textureID;
	public Primitive.TextureEntryFace textureEntryFace;
	public bool glow = false;
	public UUID uuid;
	public bool clickable = false;
	public Color color;
	public bool isTextured = false;
	public Primitive prim;
	//public bool isHUD;
}
