using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureGenerator
{
    public static Texture2D GenerateTexture(float[] hm, int nx, int ny, List<int> playerClicks)
    {
        var texture = new Texture2D(nx, ny);
        Color[] pixels = new Color[nx * ny];

        for(int j = 0; j < ny; j++)
            for (int i = 0; i < nx; i++)
            {
                pixels[j * nx + i] = Color.white;
            }

        // color the pixels around the clicked pixel in red
        foreach (var click in playerClicks)
        {
            pixels[click] = Color.red;
        }

        texture.SetPixels(pixels, 0);
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        return texture;
    }
}
