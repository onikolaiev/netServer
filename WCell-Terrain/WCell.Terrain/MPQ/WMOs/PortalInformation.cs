﻿using WCell.Util.Graphics;

namespace WCell.Terrain.MPQ.WMOs
{
    /// <summary>
    /// MOPI Chunk. Used in WMO Root files
    /// </summary>
    public struct PortalInformation
    {
        /// <summary>
        /// Vertex Span that says what vertices from MOPV are used
        /// </summary>
        public VertexSpan Vertices;
        /// <summary>
        /// a normal vector maybe? haven't checked.
        /// </summary>
        public Plane Plane;
    }
}
