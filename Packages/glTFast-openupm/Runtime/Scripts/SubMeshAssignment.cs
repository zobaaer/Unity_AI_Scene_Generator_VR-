// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using GLTFast.Schema;

namespace GLTFast
{
    /// <summary>
    /// Usually one glTF primitive relates to one Unity sub-mesh.
    /// Sometimes the primitives of one mesh share the same vertex buffer accessors. To avoid duplicate import of those
    /// vertex buffers this struct reassigns the vertex buffer of one primitive (at VertexBufferIndex)
    /// to another (Primitive).
    /// </summary>
    readonly struct SubMeshAssignment
    {
        public MeshPrimitiveBase Primitive { get; }
        public int VertexBufferIndex { get; }

        public SubMeshAssignment(MeshPrimitiveBase primitive, int vertexBufferIndex)
        {
            Primitive = primitive;
            VertexBufferIndex = vertexBufferIndex;
        }
    }
}
