using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Helper.Lighting
{
    //http://www.riemers.net/eng/Tutorials/XNA/Csharp/Series3/Vertex_format.php
    public struct LightingVertexFormat
    {
        private Vector3 position;
        private Color color;

        public LightingVertexFormat(Vector3 pos, Color c)
        {
            position= pos;
            color = c;
        }

        public LightingVertexFormat(VertexPositionColor vpc)
        {
            position = vpc.Position;
            color = vpc.Color;
        }

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );
    }
}
