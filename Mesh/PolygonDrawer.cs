using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 将多边形绘制到图片上  （用来做贴花投影等）
/// </summary>
public class PolygonDrawer
{
    
    public static Texture2D DrawPolygonTex(Vector2[] vertices, Color lineColor, int lineWidth, Color fillColor, int width = 512, int height = 512)
    {
        float worldMinX = vertices.Min(v => v.x) /*- lineWidth - lineWidth * 10*/;
        float worldMaxX = vertices.Max(v => v.x) /*+ lineWidth + lineWidth * 10*/;
        float worldMinY = vertices.Min(v => v.y) /*- lineWidth - lineWidth * 10*/;
        float worldMaxY = vertices.Max(v => v.y) /*+ lineWidth + lineWidth * 10*/;

      

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        //  清空Texture
        Color clearColor = new Color(0, 0, 0, 0);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                texture.SetPixel(x, y, clearColor);
            }
        }

        Vector2[] textureVertices = vertices.Select(v => WorldToTextureCoordinates(v, worldMinX, worldMaxX, worldMinY, worldMaxY, width, height)).ToArray();

        // 绘制多边形边
        for (int i = 0; i < textureVertices.Length; i++)
        {
            Vector2 start = textureVertices[i];
            Vector2 end = textureVertices[(i + 1) % textureVertices.Length];
            DrawLine(texture, start, end, lineColor, lineWidth);
        }

        // 填充多边形内部
        FillPolygon(texture, textureVertices, fillColor);

        // 应用Texture更改
        texture.Apply();
      

        return texture;
    }

    #region URP
    //URP
    //public static GameObject DrawForProjector(Vector2[] vertices, Color lineColor, int lineWidth, Color fillColor, int width = 512, int height = 512)
    //{
    //    float worldMinX = vertices.Min(v => v.x) - lineWidth - lineWidth * 10;
    //    float worldMaxX = vertices.Max(v => v.x) + lineWidth + lineWidth * 10;
    //    float worldMinY = vertices.Min(v => v.y) - lineWidth - lineWidth * 10;
    //    float worldMaxY = vertices.Max(v => v.y) + lineWidth + lineWidth * 10;

    //    //生成投影

    //    var projector = new GameObject("Projector");
    //    var pj = projector.AddComponent<UnityEngine.Rendering.Universal.DecalProjector>();

    //    pj.transform.position = new Vector3((worldMaxX + worldMinX) * 0.5f, 300, (worldMaxY + worldMinY) * 0.5f);
    //    pj.transform.localEulerAngles = new Vector3(90, 0, 0);
    //    pj.drawDistance = 100000;
    //    pj.transform.localScale = new Vector3((worldMaxX - worldMinX), (worldMaxY - worldMinY), 500);
    //    pj.scaleMode = UnityEngine.Rendering.Universal.DecalScaleMode.InheritFromHierarchy;
    //    var material = new Material(Shader.Find("Shader Graphs/Decal"));
    //    pj.material = material;

    //    Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
    //    texture.filterMode = FilterMode.Point;
    //    texture.wrapMode = TextureWrapMode.Clamp;

    //    //  清空Texture
    //    Color clearColor = new Color(0, 0, 0, 0);
    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int y = 0; y < height; y++)
    //        {
    //            texture.SetPixel(x, y, clearColor);
    //        }
    //    }

    //    Vector2[] textureVertices = vertices.Select(v => WorldToTextureCoordinates(v, worldMinX, worldMaxX, worldMinY, worldMaxY, width, height)).ToArray();

    //    // 绘制多边形边
    //    for (int i = 0; i < textureVertices.Length; i++)
    //    {
    //        Vector2 start = textureVertices[i];
    //        Vector2 end = textureVertices[(i + 1) % textureVertices.Length];
    //        DrawLine(texture, start, end, lineColor, lineWidth);
    //    }

    //    // 填充多边形内部
    //    FillPolygon(texture, textureVertices, fillColor);
    //    ApplyAntiAlias(texture);
    //    // 应用Texture更改
    //    texture.Apply();
    //    material.SetTexture("Base_Map", texture);
    //    return pj.gameObject;
    //}
    #endregion

    #region HDRP

    //HDRP
    //public static GameObject DrawForProjector(Vector2[] vertices, Color lineColor, int lineWidth, Color fillColor, int width = 512, int height = 512)
    //{
    //    float worldMinX = vertices.Min(v => v.x) - lineWidth - lineWidth * 10;
    //    float worldMaxX = vertices.Max(v => v.x) + lineWidth + lineWidth * 10;
    //    float worldMinY = vertices.Min(v => v.y) - lineWidth - lineWidth * 10;
    //    float worldMaxY = vertices.Max(v => v.y) + lineWidth + lineWidth * 10;

    //    //生成投影

    //    var projector = new GameObject("Projector");
    //    var pj = projector.AddComponent<UnityEngine.Rendering.HighDefinition.DecalProjector>();

    //    pj.transform.position = new Vector3((worldMaxX + worldMinX) * 0.5f, 300, (worldMaxY + worldMinY) * 0.5f);
    //    pj.transform.localEulerAngles = new Vector3(90, 0, 0);
    //    pj.drawDistance = 100000;
    //    pj.transform.localScale = new Vector3((worldMaxX - worldMinX), (worldMaxY - worldMinY), 500);
    //    pj.scaleMode = UnityEngine.Rendering.HighDefinition.DecalScaleMode.InheritFromHierarchy;
    //    var material = new Material(Shader.Find("Shader Graphs/Decal"));
    //    pj.material = material;

    //    Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
    //    texture.filterMode = FilterMode.Point;
    //    texture.wrapMode = TextureWrapMode.Clamp;

    //    //  清空Texture
    //    Color clearColor = new Color(0, 0, 0, 0);
    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int y = 0; y < height; y++)
    //        {
    //            texture.SetPixel(x, y, clearColor);
    //        }
    //    }

    //    Vector2[] textureVertices = vertices.Select(v => WorldToTextureCoordinates(v, worldMinX, worldMaxX, worldMinY, worldMaxY, width, height)).ToArray();

    //    // 绘制多边形边
    //    for (int i = 0; i < textureVertices.Length; i++)
    //    {
    //        Vector2 start = textureVertices[i];
    //        Vector2 end = textureVertices[(i + 1) % textureVertices.Length];
    //        DrawLine(texture, start, end, lineColor, lineWidth);
    //    }

    //    // 填充多边形内部
    //    FillPolygon(texture, textureVertices, fillColor);
    //    ApplyAntiAlias(texture);
    //    // 应用Texture更改
    //    texture.Apply();
    //    material.SetTexture("Base_Map", texture);
    //    return pj.gameObject;
    //}
    #endregion

    // 将世界坐标转换为Texture坐标
    public static Vector2 WorldToTextureCoordinates(Vector2 worldPosition, float worldMinX, float worldMaxX, float worldMinY, float worldMaxY, int width, int height)
    {
        float textureX = (worldPosition.x - worldMinX) / (worldMaxX - worldMinX) * width;
        float textureY = (worldPosition.y - worldMinY) / (worldMaxY - worldMinY) * height;
        return new Vector2(textureX, textureY);
    }

    // 绘制带宽度的线条
    public static void DrawLine(Texture2D texture, Vector2 start, Vector2 end, Color color, int lineWidth)
    {
        int x0 = (int)start.x;
        int y0 = (int)start.y;
        int x1 = (int)end.x;
        int y1 = (int)end.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = (x0 < x1) ? 1 : -1;
        int sy = (y0 < y1) ? 1 : -1;
        int err = ((dx > dy) ? dx : -dy) / 2;
        int e2;

        // 获取纹理的像素数组，用于批量设置像素值
        Color[] pixels = texture.GetPixels();

        while (true)
        {
            // 绘制当前点及其周围的点
            for (int i = -lineWidth; i <= lineWidth; i++)
            {
                for (int j = -lineWidth; j <= lineWidth; j++)
                {
                    int px = x0 + i;
                    int py = y0 + j;
                    if (px >= 0 && px < texture.width && py >= 0 && py < texture.height)
                    {
                        float distance = PointToLineDistance(new Vector2(px, py), start, end);
                        if (distance <= lineWidth / 2) // 只绘制在半径范围内的点
                        {
                            var colorIndex = py * texture.width + px;

                            //float alpha = 1 - (distance / (lineWidth / 2));
                            //alpha = Mathf.Clamp01(alpha);
                            //Color pixelColor = new Color(color.r, color.g, color.b, alpha);
                            ////原本像素颜色
                        
                            //var oldColor = pixels[colorIndex];
                            //if (oldColor.a < pixelColor.a)
                            //{
                            //    pixels[colorIndex] = pixelColor; // 设置像素值
                            //}

                            Color pixelColor = new Color(color.r, color.g, color.b, 1);
                            pixels[colorIndex] = pixelColor;
                        }
                    }
                }
            }

            // 检查是否到达终点
            if (x0 == x1 && y0 == y1)
            {
                break;
            }

            e2 = err;
            if (e2 > -dx)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dy)
            {
                err += dx;
                y0 += sy;
            }
        }

        // 应用绘制结果
        texture.SetPixels(pixels);
    }

    public static void ApplyAntiAlias(Texture2D texture, int blurRadius = 2)
    {
        // 获取纹理的像素数组
        Color[] pixels = texture.GetPixels();
        Color[] newPixels = new Color[pixels.Length];

        int width = texture.width;
        int height = texture.height;

        // 遍历每个像素
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 当前像素索引
                int index = y * width + x;

                // 初始化累加颜色
                Color accumulatedColor = Color.clear;
                int count = 0;

                // 遍历当前像素周围的像素
                for (int dy = -blurRadius; dy <= blurRadius; dy++)
                {
                    for (int dx = -blurRadius; dx <= blurRadius; dx++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;

                        // 检查是否在纹理范围内
                        if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                        {
                            int neighborIndex = ny * width + nx;
                            accumulatedColor += pixels[neighborIndex];
                            count++;
                        }
                    }
                }

                // 计算平均颜色
                accumulatedColor /= count;
                newPixels[index] = accumulatedColor;
            }
        }

        // 将处理后的像素数组应用到纹理
        texture.SetPixels(newPixels);
    }
    public static float PointToLineDistance(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        // 计算向量 AB 和 AP
        Vector2 AB = lineEnd - lineStart;
        Vector2 AP = point - lineStart;

        // 计算点积
        float dot = Vector2.Dot(AP, AB);

        // 计算向量 AB 的模的平方
        float lenSq = AB.sqrMagnitude;

        // 计算参数 t
        float t = dot / lenSq;

        // 确定最近点的位置
        Vector2 closestPoint;
        if (t < 0)
        {
            closestPoint = lineStart;
        }
        else if (t > 1)
        {
            closestPoint = lineEnd;
        }
        else
        {
            closestPoint = lineStart + t * AB;
        }

        // 计算点 P 到最近点的距离
        Vector2 diff = point - closestPoint;
        return diff.magnitude;
    }
    // 填充多边形内部
    public static void FillPolygon(Texture2D texture, Vector2[] vertices, Color color)
    {
        int minY = (int)vertices.Min(v => v.y);
        int maxY = (int)vertices.Max(v => v.y);

        for (int y = minY; y <= maxY; y++)
        {
            List<int> intersections = new List<int>();

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2 v1 = vertices[i];
                Vector2 v2 = vertices[(i + 1) % vertices.Length];

                if (v1.y <= y && v2.y > y || v2.y <= y && v1.y > y)
                {
                    float x = v1.x + (y - v1.y) * (v2.x - v1.x) / (v2.y - v1.y);
                    intersections.Add((int)x);
                }
            }

            intersections.Sort();

            for (int i = 0; i < intersections.Count; i += 2)
            {
                for (int x = intersections[i]; x <= intersections[i + 1]; x++)
                {
                    if (x >= 0 && x < texture.width && y >= 0 && y < texture.height)
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
            }
        }
    }
}