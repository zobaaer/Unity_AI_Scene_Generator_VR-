// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using GLTFast.Schema;
using UnityEngine.Assertions;

namespace GLTFast
{
    interface IPrimitiveSet
    {
        bool HasMorphTargets { get; }
        void BuildAndDispose(out int[] indices, out MeshPrimitiveBase[] primitives, out SubMeshAssignment[] subMeshAssignments);
    }

    class PrimitiveSet : IPrimitiveSet
    {
        readonly List<int> m_Indices = new List<int>();
        readonly List<MeshPrimitiveBase> m_Primitives = new List<MeshPrimitiveBase>();
        List<SubMeshAssignment> m_SubMeshAssignments;

        public IReadOnlyList<MeshPrimitiveBase> Primitives => m_Primitives;

        public void Add(int index, MeshPrimitiveBase primitive)
        {
            if (m_Primitives.Count > 0)
            {
                for (var bufferIndex = 0; bufferIndex < m_Primitives.Count; bufferIndex++)
                {
                    var existingPrimitive = m_Primitives[bufferIndex];
                    if (PrimitiveComparer.HaveEqualVertexBuffers(existingPrimitive, primitive))
                    {
                        if (m_SubMeshAssignments == null)
                        {
                            m_SubMeshAssignments = new List<SubMeshAssignment>(m_Indices.Count + 1);
                            for (var i = 0; i < m_Indices.Count; ++i)
                            {
                                m_SubMeshAssignments.Add(new SubMeshAssignment(m_Primitives[i], i));
                            }
                        }

                        m_Indices.Add(index);
                        m_SubMeshAssignments.Add(new SubMeshAssignment(primitive, bufferIndex));
                        return;
                    }
                }
            }

            m_SubMeshAssignments?.Add(new SubMeshAssignment(primitive, m_Primitives.Count));
            m_Indices.Add(index);
            m_Primitives.Add(primitive);
        }

        public bool HasMorphTargets
        {
            get
            {
                Assert.IsTrue(m_Primitives.Count > 0);
                return m_Primitives[0].targets != null && m_Primitives[0].targets.Length > 0;
            }
        }

        public void BuildAndDispose(out int[] indices, out SubMeshAssignment[] subMeshAssignments)
        {
            indices = m_Indices.ToArray();
            subMeshAssignments = m_SubMeshAssignments?.ToArray();
            m_Indices.Clear();
            m_Primitives.Clear();
            m_SubMeshAssignments?.Clear();
            m_SubMeshAssignments = null;
        }

        public void BuildAndDispose(out int[] indices, out MeshPrimitiveBase[] primitives, out SubMeshAssignment[] subMeshAssignments)
        {
            indices = m_Indices.ToArray();
            primitives = m_Primitives.ToArray();
            subMeshAssignments = m_SubMeshAssignments?.ToArray();
            m_Indices.Clear();
            m_Primitives.Clear();
            m_SubMeshAssignments?.Clear();
            m_SubMeshAssignments = null;
        }
    }

    class PrimitiveSingle : IPrimitiveSet
    {
        readonly int m_Index;
        public MeshPrimitiveBase Primitive { get; }

        public PrimitiveSingle(int index, MeshPrimitiveBase primitive)
        {
            m_Index = index;
            Primitive = primitive;
        }

        public bool HasMorphTargets => Primitive.targets != null && Primitive.targets.Length > 0;

        public void BuildAndDispose(out int[] indices, out SubMeshAssignment[] subMeshAssignments)
        {
            indices = new[] { m_Index };
            subMeshAssignments = null;
        }

        public void BuildAndDispose(out int[] indices, out MeshPrimitiveBase[] primitives, out SubMeshAssignment[] subMeshAssignments)
        {
            indices = new[] { m_Index };
            primitives = new[] { Primitive };
            subMeshAssignments = null;
        }
    }
}
