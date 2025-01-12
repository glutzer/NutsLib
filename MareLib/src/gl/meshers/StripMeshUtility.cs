using OpenTK.Mathematics;
using System.Collections.Generic;

namespace MareLib;

/// <summary>
/// Tessellate a strip.
/// Might not be good for constantly updating things.
/// </summary>
public class StripMeshUtility<T> where T : unmanaged
{
    private enum PointType
    {
        Point,
        Wide
    }

    private struct PointEntry
    {
        public Vector3 point;
        public Vector3 normal;
        public float width;
        public PointType pointType;
    }

    private readonly List<PointEntry> points = new();

    private readonly MeshDelegate<T> dele;

    public StripMeshUtility(MeshDelegate<T> dele)
    {
        this.dele = dele;
    }

    public MeshInfo<T> Tessellate()
    {
        MeshInfo<T> meshInfo = new(4, 6);

        int currentVertex = 0;
        PointType lastPoint = PointType.Point;

        {
            // Add first entry.
            PointEntry first = points[0];
            PointEntry second = points[1];
            Vector3 direction = (second.point - first.point).Normalized();
            Vector3 crossWidth = Vector3.Cross(first.normal, direction).Normalized() * first.width;

            if (first.pointType == PointType.Wide)
            {
                Vector3 pos1 = first.point - crossWidth;
                Vector3 pos2 = first.point + crossWidth;

                Vector2 uv1 = new(0f, 0f);
                Vector2 uv2 = new(1f, 0f);

                MeshVertexData data1 = new(pos1, uv1, first.normal);
                MeshVertexData data2 = new(pos2, uv2, first.normal);

                meshInfo.AddVertex(dele(data1));
                meshInfo.AddVertex(dele(data2));

                currentVertex += 2;

                lastPoint = PointType.Wide;
            }
            else
            {
                MeshVertexData data = new(first.point, new Vector2(0.5f, 0f), first.normal);

                meshInfo.AddVertex(dele(data));

                currentVertex++;

                lastPoint = PointType.Point;
            }
        }

        for (int i = 1; i < points.Count; i++)
        {
            PointEntry current = points[0];
            PointEntry next = points[1];
            Vector3 direction = (next.point - current.point).Normalized();
            Vector3 crossWidth = Vector3.Cross(current.normal, direction).Normalized() * current.width;

            float progress = (float)i / (points.Count - 1);

            if (current.pointType == PointType.Wide)
            {
                Vector3 pos1 = current.point - crossWidth;
                Vector3 pos2 = current.point + crossWidth;

                Vector2 uv1 = new(0f, progress);
                Vector2 uv2 = new(1f, progress);

                MeshVertexData data1 = new(pos1, uv1, current.normal);
                MeshVertexData data2 = new(pos2, uv2, current.normal);

                meshInfo.AddVertex(dele(data1));
                meshInfo.AddVertex(dele(data2));

                if (lastPoint == PointType.Wide)
                {
                    meshInfo.AddTriangle(currentVertex - 1, currentVertex + 1, currentVertex - 2);
                    meshInfo.AddTriangle(currentVertex - 2, currentVertex + 1, currentVertex);
                }
                else
                {
                    meshInfo.AddTriangle(currentVertex - 1, currentVertex + 1, currentVertex);
                }

                currentVertex += 2;
            }
            else
            {
                MeshVertexData data = new(current.point, new Vector2(0.5f, progress), current.normal);

                meshInfo.AddVertex(dele(data));

                if (lastPoint == PointType.Wide)
                {
                    meshInfo.AddTriangle(currentVertex - 1, currentVertex - 2, currentVertex);
                }
                else
                {
                    // Don't add a triangle between 2 consecutive points.
                }

                currentVertex++;
            }
        }

        return meshInfo;
    }

    public void AddWide(Vector3 point, Vector3 normal, float width)
    {
        PointEntry entry = new()
        {
            point = point,
            normal = normal,
            width = width,
            pointType = PointType.Wide
        };

        points.Add(entry);
    }

    public void AddPoint(Vector3 point, Vector3 normal)
    {
        PointEntry entry = new()
        {
            point = point,
            normal = normal,
            width = 0f,
            pointType = PointType.Point
        };

        points.Add(entry);
    }
}