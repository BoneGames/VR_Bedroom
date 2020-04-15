using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class TextureSaver : MonoBehaviour
{

    [Button]
    public void SaveMaterialTexture()
    {
        Texture2D tex = GetComponent<Renderer>().materials[0].mainTexture as Texture2D;
        SaveTextureAsPNG(tex, @"C:\Users\benja\OneDrive\Desktop\SpriteWall\Assets\savedTextures\savedTex");
    }


    public static void SaveTextureAsPNG(Texture2D _texture, string _fullPath)
    {
        byte[] _bytes = _texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(_fullPath, _bytes);
        Debug.Log(_bytes.Length / 1024 + " Kb was saved as: " + _fullPath);
    }

}
