﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WCell.Terrain.MPQ;
using WCell.Terrain.MPQ.WMOs;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.Terrain.Legacy.BSPTree
{
    public class BSPTree
    {
        internal readonly int rootId;
        internal readonly BSPNode[] nodes;

		public BSPNode Root
        {
            get { return nodes[rootId]; }
        }

		public BSPTree(IList<BSPNode> nodes)
        {
            this.nodes = nodes.ToArray();
            rootId = FindRootNode(nodes);

            if (rootId == short.MinValue)
            {
                throw new InvalidDataException("No root node found for this BSP tree.");
            }
        }

		public BSPTree(short rootId, BSPNode[] nodes)
        {
            this.rootId = rootId;
            this.nodes = nodes;
        }

		public bool FirstPointOfIntersection(ref Ray ray, ref float tMax, Vector3[] vertices, out Vector3 pointOfIntersection)
        {
            var rayCopy = ray;

            var dist = float.MaxValue;
            VisitNodes(rootId, ref ray, ref tMax, (node) =>
            {
                if (node == null) return;
                if (node.TriIndices.Length == 0) return;
                
                for (var i = 0; i < node.TriIndices.Length; i++)
                {
                    var tri = node.TriIndices[i];
                    var v0 = vertices[tri.Index0];
                    var v1 = vertices[tri.Index1];
                    var v2 = vertices[tri.Index2];

                    float newDist;
                    if (!Intersection.RayTriangleIntersect(rayCopy, v0, v1, v2, out newDist)) continue;

                    //Collision happens behind the startPos
                    if (newDist < 0.0f) continue;

                    dist = Math.Min(dist, newDist);
                }
            });

            if (dist < tMax)
            {
                pointOfIntersection = ray.Position + dist*ray.Direction;
                return true;
            }

            pointOfIntersection = Vector3.Zero;
            return false;
        }

		public bool IntersectsWith(ref Ray ray, ref float tMax, Vector3[] vertices)
        {
            var rayCopy = ray;

            var dist = float.MaxValue;

            VisitNodes(rootId, ref ray, ref tMax, (node) =>
            {
                if (node == null) return;
                if (node.TriIndices.Length == 0) return;
                foreach (var tri in node.TriIndices)
                {
                    var v1 = vertices[tri.Index0];
                    var v2 = vertices[tri.Index1];
                    var v3 = vertices[tri.Index2];

                    float newDist;
                    if (!Intersection.RayTriangleIntersect(rayCopy, v1, v2, v3, out newDist)) continue;

                    //Collision happens behind the startPos
                    if (newDist < 0.0f) continue;

                    dist = Math.Min(dist, newDist);
                }
            });

            return (dist < tMax);
        }

		public static short FindRootNode(IList<BSPNode> nodes)
        {
            for (short rootId = 0; rootId < nodes.Count; rootId++)
            {
                var node = nodes[rootId];
                if (node == null) continue;

                var found = false;
                for (var j = 0; j < nodes.Count; j++)
                {
                    // don't check a node against itself
                    if (rootId == j) continue;

                    var parent = nodes[j];
                    if (parent == null) continue;

                    // this node is the child to another node
                    if (parent.posChild != rootId && parent.negChild != rootId) continue;

                    found = true;
                    break;
                }
                if (!found) return rootId;
            }
            return short.MinValue;
        }

		public void VisitNodes(int nodeId, ref Ray ray, ref float tMax, Action<BSPNode> callback)
        {
            if (nodeId >= nodes.Length || nodeId < 0) return;

            var node = nodes[nodeId];
            if (node == null) return;

            if ((node.flags == BSPNodeFlags.Flag_Leaf) || (node.flags == BSPNodeFlags.Flag_NoChild))
            {
                // Do stuff here
                callback(node);

                // We've reached the end of this branch, continue on with the next one.
                return;
            }

            //Figure out which child to recurse into first
            float startVal;
            float dirVal;
            var planeDist = node.planeDist;
            short first;
            short last;

            switch (node.flags)
            {
                case BSPNodeFlags.Flag_XAxis:
                    planeDist = -node.planeDist;
                    startVal = ray.Position.X;
                    dirVal = ray.Direction.X;
                    if (startVal <= planeDist)
                    {
                        first = node.posChild;
                        last = node.negChild;
                    }
                    else
                    {
                        first = node.negChild;
                        last = node.posChild;
                    }
                    break;
                case BSPNodeFlags.Flag_YAxis:
                    startVal = ray.Position.Y;
                    dirVal = ray.Direction.Y;
                    if (startVal >= planeDist)
                    {
                        first = node.posChild;
                        last = node.negChild;
                    }
                    else
                    {
                        first = node.negChild;
                        last = node.posChild;
                    }
                    break;
                case BSPNodeFlags.Flag_ZAxis:
                    startVal = ray.Position.Z;
                    dirVal = ray.Direction.Z;
                    if (startVal >= planeDist)
                    {
                        first = node.posChild;
                        last = node.negChild;
                    }
                    else
                    {
                        first = node.negChild;
                        last = node.posChild;
                    }
                    break;
                default:
                    throw new Exception("This BSPNode has no divider planes. Wierd.");
            }

            if (dirVal.NearlyZero())
            {
                // Segment is parallel to the splitting plane, visit the near side only.
                VisitNodes(first, ref ray, ref tMax, callback);
                return;
            }

            // The t-value for the intersection with the boundary plane
            var tIntersection = (planeDist - startVal) / dirVal;

            VisitNodes(first, ref ray, ref tMax, callback);

            // Test if line segment straddles the boundary plane
            if (0.0f > tIntersection || tIntersection > tMax) return;

            // It does, visit the far side
            VisitNodes(last, ref ray, ref tMax, callback);
        }

        public void DumpTree(Vector3[] vectors)
        {
            var file = File.CreateText("Divider.txt");
            DumpNodes(rootId, vectors, file);
            file.Close();
        }

		public void DumpNodes(int nodeId, Vector3[] vectors, TextWriter file)
        {
            if (nodeId < 0 || nodeId > nodes.Length) return;
            var node = nodes[nodeId];
            if (node.flags == BSPNodeFlags.Flag_Leaf) return;

            file.WriteLine("Divider = {0}: {1}", node.flags, node.planeDist);
            file.Write(file.NewLine);

            DumpBranchContents(file, node, vectors);

            file.Write(file.NewLine);
            file.Write(file.NewLine);

            DumpNodes(node.posChild, vectors, file);
            DumpNodes(node.negChild, vectors, file);
        }

        private void DumpBranchContents(TextWriter file, BSPNode startAt, IList<Vector3> vectors)
        {
            // Write the vertices referenced by the first
            var posVertList = new List<Vector3>();
            GetBranchContents(startAt.posChild, vectors, posVertList);
            file.WriteLine("Positive Polys: {0}", posVertList.Count);
            file.WriteLine("_______________");
            file.Write(file.NewLine);

            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);
            foreach (var vert in posVertList)
            {
                min = Vector3.Min(min, vert);
                max = Vector3.Max(max, vert);
            }
            file.WriteLine("MinVals: {0}", min);
            file.WriteLine("MaxVals: {0}", max);
            file.WriteLine(file.NewLine);

            //Write the vertices referenced by the last
            var negVertList = new List<Vector3>();
            GetBranchContents(startAt.negChild, vectors, negVertList);
            file.WriteLine("Negative Polys: {0}", negVertList.Count);
            file.WriteLine("_______________");
            file.Write(file.NewLine);

            min = new Vector3(float.MaxValue);
            max = new Vector3(float.MinValue);
            foreach (var vert in negVertList)
            {
                min = Vector3.Min(min, vert);
                max = Vector3.Max(max, vert);
            }
            file.WriteLine("MinVals: {0}", min);
            file.WriteLine("MaxVals: {0}", max);
            file.WriteLine(file.NewLine);
        }

		public void GetBranchContents(short nodeId, IList<Vector3> vectors, ICollection<Vector3> vertices)
        {
            if (nodeId < 0 || nodeId > nodes.Length) return;
            var node = nodes[nodeId];

            if (node.flags == BSPNodeFlags.Flag_Leaf)
            {
                foreach (var index3 in node.TriIndices)
                {
                    vertices.AddUnique(vectors[index3.Index0]);
                    vertices.AddUnique(vectors[index3.Index1]);
                    vertices.AddUnique(vectors[index3.Index2]);
                }
                return;
            }

            GetBranchContents(node.posChild, vectors, vertices);
            GetBranchContents(node.negChild, vectors, vertices);
        }
    }

    public class BSPNode
    {
        internal BSPNodeFlags flags;
        internal short negChild;
        internal short posChild;
        internal float planeDist;
        internal Index3[] TriIndices;
    }
}



/*
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using WCell.Terrain;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.Addons.Terrain
{
    public class BSPTree
    {
        internal readonly short rootId;
        internal readonly BSPNode[] nodes;

        internal BSPNode Root
        {
            get { return nodes[rootId]; }
        }

        internal BSPTree(IList<BSPNode> nodes)
        {
            this.nodes = nodes.ToArray();
            rootId = FindRootNode(nodes);
            
            if (rootId == short.MinValue)
            {
                throw new InvalidDataException("No root node found for this BSP tree.");
            }
        }

        internal BSPTree(short rootId, BSPNode[] nodes)
        {
            this.rootId = rootId;
            this.nodes = nodes;
        }

        internal float? FirstPointOfIntersection(ref Ray ray, ref float tMax, Vector3[] vertices, out Vector3 intersection)
        {
            var rayCopy = ray;

            float? result = float.MaxValue;
            var intersector = Vector3.Zero;
            VisitNodes(rootId, ref ray, ref tMax, (node) =>{
                if (node == null)
                {
                    result = null;
                    return;
                }

                if (node.TriIndices.Length == 0)
                {
                    result = null;
                    return;
                }

                for (var i = 0; i < node.TriIndices.Length; i++)
                {
                    var tri = node.TriIndices[i];
                    var v0 = vertices[tri.Index0];
                    var v1 = vertices[tri.Index1];
                    var v2 = vertices[tri.Index2];

                    float u, v;
                    var newResult = CollisionUtil.RayTriangleIntersect(ref v0, ref v1, ref v2, ref rayCopy, out u, out v);

                    //Collision never happens
                    if (newResult == null) continue;
                    //Collision happens behind the startPos
                    if (newResult < 0.0f) continue;

                    result = Math.Min(result.Value, newResult.Value);
                    intersector = v0*(1 - u - v) + v1*u + v2*v;
                }
            });

            if (result < tMax)
            {
                intersection = intersector;
                return result;
            }

            intersection = Vector3.Zero;
            return null;
        }

        internal float? IntersectsWith(ref Ray ray, ref float tMax, Vector3[] vertices)
        {
            var rayCopy = ray;
            
            float? result = float.MaxValue;

            VisitNodes(rootId, ref ray, ref tMax, (node) => {
                if (node == null)
                {
                    result = null;
                    return;
                }

                if (node.TriIndices.Length == 0)
                {
                    result = null;
                    return;
                }

                for (var i = 0; i < node.TriIndices.Length; i++)
                {
                    var tri = node.TriIndices[i];
                    var v1 = vertices[tri.Index0];
                    var v2 = vertices[tri.Index1];
                    var v3 = vertices[tri.Index2];

                    float u, v;
                    var newResult = CollisionUtil.RayTriangleIntersect(ref v1, ref v2, ref v3, ref rayCopy, out u, out v);

                    //Collision never happens
                    if (newResult == null) continue;
                    //Collision happens behind the startPos
                    if (newResult < 0.0f) continue;

                    result = Math.Min(result.Value, newResult.Value);
                }
            });

            return (result < tMax) ? result : null;
        }

        private static short FindRootNode(IList<BSPNode> nodes)
        {
            for (short rootId = 0; rootId < nodes.Count; rootId++)
            {
                var node = nodes[rootId];
                if (node == null) continue;

                var found = false;
                for (var j = 0; j < nodes.Count; j++)
                {
                    // don't check a node against itself
                    if (rootId == j) continue;

                    var parent = nodes[j];
                    if (parent == null) continue;

                    // this node is the child to another node
                    if (parent.posChild != rootId && parent.negChild != rootId) continue;

                    found = true;
                    break;
                }
                if (!found) return rootId;
            }
            return short.MinValue;
        }

        private void VisitNodes(short nodeId, ref Ray ray, ref float tMax, Action<BSPNode> callback)
        {
            if (nodeId >= nodes.Length || nodeId < 0) return;

            var node = nodes[nodeId];
            if (node == null) return;
            
            if ((node.flags == BSPNodeFlags.Flag_Leaf) || (node.flags == BSPNodeFlags.Flag_NoChild))
            {
                // Do stuff here
                callback(node);

                // We've reached the end of this branch, continue on with the next one.
                return;
            }

            //Figure out which child to recurse into first
            float startVal;
            float dirVal;
            var planeDist = node.planeDist;
            short first;
            short last;

            switch (node.flags)
            {
                case BSPNodeFlags.Flag_XAxis:
                    planeDist = -node.planeDist;
                    startVal = ray.Position.X;
                    dirVal = ray.Direction.X;
                    if (startVal <= planeDist)
                    {
                        first = node.posChild;
                        last = node.negChild;
                    }
                    else
                    {
                        first = node.negChild;
                        last = node.posChild;
                    }
                    break;
                case BSPNodeFlags.Flag_YAxis:
                    startVal = ray.Position.Y;
                    dirVal = ray.Direction.Y;
                    if (startVal >= planeDist)
                    {
                        first = node.posChild;
                        last = node.negChild;
                    }
                    else
                    {
                        first = node.negChild;
                        last = node.posChild;
                    }
                    break;
                case BSPNodeFlags.Flag_ZAxis:
                    startVal = ray.Position.Z;
                    dirVal = ray.Direction.Z;
                    if (startVal >= planeDist)
                    {
                        first = node.posChild;
                        last = node.negChild;
                    }
                    else
                    {
                        first = node.negChild;
                        last = node.posChild;
                    }
                    break;
                default:
                    Debugger.Break();
                    throw new Exception("This BSPNode has no divider planes. Wierd.");
            }
            
            if (dirVal.IsWithinEpsilon(0.0f))
            {
                // Segment is parallel to the splitting plane, visit the near side only.
                VisitNodes(first, ref ray, ref tMax, callback);
                return;
            }

            // The t-value for the intersection with the boundary plane
            var tIntersection = (planeDist - startVal) / dirVal;

            VisitNodes(first, ref ray, ref tMax, callback);

            // Test if line segment straddles the boundary plane
            if (0.0f > tIntersection || tIntersection > tMax) return;

            // It does, visit the far side
            VisitNodes(last, ref ray, ref tMax, callback);
        }

        public void DumpTree(Vector3[] vectors)
        {
            var file = File.CreateText("C:\\Users\\Nate\\Desktop\\Divider.txt");
            DumpNodes(rootId, vectors, file);
            file.Close();
        }

        public void DumpNodes(short nodeId, Vector3[] vectors, TextWriter file)
        {
            if (nodeId < 0 || nodeId > nodes.Length) return;
            var node = nodes[nodeId];
            if (node.flags == BSPNodeFlags.Flag_Leaf) return;

            file.WriteLine("Divider = {0}: {1}", node.flags, node.planeDist);
            file.Write(file.NewLine);

            DumpBranchContents(file, node, vectors);
            
            file.Write(file.NewLine);
            file.Write(file.NewLine);

            DumpNodes(node.posChild, vectors, file);
            DumpNodes(node.negChild, vectors, file);
        }

        private void DumpBranchContents(TextWriter file, BSPNode startAt, Vector3[] vectors)
        {
            // Write the vertices referenced by the first
            var posVertList = new List<Vector3>();
            GetBranchContents(startAt.posChild, vectors, posVertList);
            file.WriteLine("Positive Polys: {0}", posVertList.Count);
            file.WriteLine("_______________");
            file.Write(file.NewLine);

            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);
            foreach (var vert in posVertList)
            {
                min = Vector3.Min(min, vert);
                max = Vector3.Max(max, vert);
            }
            file.WriteLine("MinVals: {0}", min);
            file.WriteLine("MaxVals: {0}", max);
            file.WriteLine(file.NewLine);

            //Write the vertices referenced by the last
            var negVertList = new List<Vector3>();
            GetBranchContents(startAt.negChild, vectors, negVertList);
            file.WriteLine("Negative Polys: {0}", negVertList.Count);
            file.WriteLine("_______________");
            file.Write(file.NewLine);

            min = new Vector3(float.MaxValue);
            max = new Vector3(float.MinValue);
            foreach (var vert in negVertList)
            {
                min = Vector3.Min(min, vert);
                max = Vector3.Max(max, vert);
            }
            file.WriteLine("MinVals: {0}", min);
            file.WriteLine("MaxVals: {0}", max);
            file.WriteLine(file.NewLine);
        }

        private void GetBranchContents(short nodeId, Vector3[] vectors, List<Vector3> vertices)
        {
            if (nodeId < 0 || nodeId > nodes.Length) return;
            var node = nodes[nodeId];

            if (node.flags == BSPNodeFlags.Flag_Leaf)
            {
                foreach (var index3 in node.TriIndices)
                {
                    vertices.AddUnique(vectors[index3.Index0]);
                    vertices.AddUnique(vectors[index3.Index1]);
                    vertices.AddUnique(vectors[index3.Index2]);
                }
                return;
            }

            GetBranchContents(node.posChild, vectors, vertices);
            GetBranchContents(node.negChild, vectors, vertices);
        }
    }

    internal class BSPNode
    {
        internal BSPNodeFlags flags;
        internal short negChild;
        internal short posChild;
        internal float planeDist;
        internal Index3[] TriIndices;
    }

    [Flags]
    public enum BSPNodeFlags : ushort
    {
        Flag_XAxis = 0x0,
        Flag_ZAxis = 0x1,
        Flag_YAxis = 0x2,
        Flag_AxisMask = Flag_XAxis | Flag_YAxis | Flag_ZAxis,
        Flag_Leaf = 0x4,
        Flag_NoChild = 0xffff,
    }

    public struct Index3
    {
        public short Index0;
        public short Index1;
        public short Index2;
    }
}
*/