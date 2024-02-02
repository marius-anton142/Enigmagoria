using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NewBehaviourScript : MonoBehaviour
{
    public class Triangulation
    {
        public static List<Triangle> Triangulate(List<Point> points)
        {
            List<Triangle> triangles = new List<Triangle>();
            Triangle superTriangle = GetSuperTriangle(points);
            triangles.Add(superTriangle);
            foreach (Point p in points)
            {
                List<Triangle> badTriangles = new List<Triangle>();
                foreach (Triangle t in triangles)
                {
                    if (t.IsPointInsideCircumcircle(p))
                    {
                        badTriangles.Add(t);
                    }
                }
                List<Edge> polygon = new List<Edge>();
                for (int i = 0; i < badTriangles.Count; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Edge e = badTriangles[i].GetEdge(j);
                        bool isShared = false;
                        for (int k = 0; k < badTriangles.Count; k++)
                        {
                            if (k != i && badTriangles[k].HasEdge(e))
                            {
                                isShared = true;
                                break;
                            }
                        }
                        if (!isShared)
                        {
                            polygon.Add(e);
                        }
                    }
                }
                foreach (Triangle t in badTriangles)
                {
                    triangles.Remove(t);
                }
                foreach (Edge e in polygon)
                {
                    triangles.Add(new Triangle(p, e.P1, e.P2));
                }
            }
            triangles.RemoveAll(t => t.HasVertex(superTriangle.P1) || t.HasVertex(superTriangle.P2) || t.HasVertex(superTriangle.P3));
            return triangles;
        }

        private static Triangle GetSuperTriangle(List<Point> points)
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;
            foreach (Point p in points)
            {
                if (p.X < minX) minX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.X > maxX) maxX = p.X;
                if (p.Y > maxY) maxY = p.Y;
            }
            double dx = maxX - minX;
            double dy = maxY - minY;
            double dmax = (dx > dy) ? dx : dy;
            double xmid = minX + dx * 0.5;
            double ymid = minY + dy * 0.5;
            return new Triangle(new Point(xmid - 20 * dmax, ymid - dmax), new Point(xmid, ymid + 20 * dmax), new Point(xmid + 20 * dmax, ymid - dmax));
        }
    }

    public class Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public class Edge
    {
        public Point P1 { get; set; }
        public Point P2 { get; set; }

        public Edge(Point p1, Point p2)
        {
            P1 = p1;
            P2 = p2;
        }

        public bool Equals(Edge other)
        {
            return (P1.Equals(other.P1) && P2.Equals(other.P2)) || (P1.Equals(other.P2) && P2.Equals(other.P1));
        }
    }

    public class Triangle
    {
        public Point P1 { get; set; }
        public Point P2 { get; set; }
        public Point P3 { get; set; }

        public Triangle(Point p1, Point p2, Point p3)
        {
            P1 = p1;
            P2 = p2;
            P3 = p3;
        }

        public bool HasVertex(Point v)
        {
            return P1.Equals(v) || P2.Equals(v) || P3.Equals(v);
        }

        public Edge GetEdge(int index)
        {
            switch (index)
            {
                case 0:
                    return new Edge(P1, P2);
                case 1:
                    return new Edge(P2, P3);
                case 2:
                    return new Edge(P3, P1);
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public bool HasEdge(Edge e)
        {
            return e.Equals(GetEdge(0)) || e.Equals(GetEdge(1)) || e.Equals(GetEdge(2));
        }

        public bool IsPointInsideCircumcircle(Point p)
        {
            double ab = (P1.X * P1.X) + (P1.Y * P1.Y);
            double cd = (P2.X * P2.X) + (P2.Y * P2.Y);
            double ef = (P3.X * P3.X) + (P3.Y * P3.Y);

            double ax = p.X - P1.X;
            double ay = p.Y - P1.Y;
            double bx = p.X - P2.X;
            double by = p.Y - P2.Y;
            double cx = p.X - P3.X;
            double cy = p.Y - P3.Y;

            double circum_x = (ax * (cd + ef - ab) + bx * (ef + ab - cd) + cx * (ab + cd - ef)) / (2 * (ax * (cy - by) - ay * (cx - bx)));
            double circum_y = (ay * (cd + ef - ab) + by * (ef + ab - cd) + cy * (ab + cd - ef)) / (2 * (ax * (cy - by) - ay * (cx - bx)));
            Point circumcenter = new Point(circum_x, circum_y);

            double radius = Math.Sqrt(((circumcenter.X - P1.X) * (circumcenter.X - P1.X)) + ((circumcenter.Y - P1.Y) * (circumcenter.Y - P1.Y)));
            double distance = Math.Sqrt(((p.X - circumcenter.X) * (p.X - circumcenter.X)) + ((p.Y - circumcenter.Y) * (p.Y - circumcenter.Y)));

            return distance < radius;
        }
    }
}
