using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureGenerator
{
    public static Texture2D GenerateTexture(ref float[] hm, ref float[] slope, float maxSlope, int nx, int ny, List<int> playerClicks, List<List<int>> paths)
    {
        var texture = new Texture2D(nx, ny);
        Color[] pixels = new Color[nx * ny];

        for(int j = 0; j < ny; j++)
            for (int i = 0; i < nx; i++)
            {
                int index = j * nx + i;
                if (hm[index] < -0.5f)
                    pixels[index] = Color.blue;
                else if (hm[index] < -0.45f)
                    pixels[index] = Color.yellow;
                else if (hm[index] < 0.5f)
                {
                    float slopeLocal = slope[index] / maxSlope;
                    pixels[index] = Color.Lerp(Color.green, new Color(.4f, .254f, 0.023f), slope[index] / maxSlope);
                }
                else
                {
                    if (slope[index] / maxSlope > .5f)
                        pixels[index] = Color.gray;
                    else
                        pixels[index] = Color.white;
                }
                    
            }

        // color the pixels around the clicked pixel in red
        foreach (var click in playerClicks)
        {
            pixels[click] = Color.red;
        }

        foreach (var path in paths)
            foreach (var pixel in path)
                pixels[pixel] = Color.magenta;

        texture.SetPixels(pixels, 0);
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        return texture;
    }
}
