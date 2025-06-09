// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine.Rendering;
using System.Threading.Tasks;

namespace GLTFast
{

    abstract class MeshGeneratorBase : IDisposable
    {

        public const MeshUpdateFlags defaultMeshUpdateFlags =
            MeshUpdateFlags.DontNotifyMeshUsers
            | MeshUpdateFlags.DontRecalculateBounds
            | MeshUpdateFlags.DontResetBoneBounds
            | MeshUpdateFlags.DontValidateIndices;

        protected Task<UnityEngine.Mesh> m_CreationTask;

        public bool IsCompleted => m_CreationTask == null || m_CreationTask.IsCompleted;

        protected MorphTargetsGenerator m_MorphTargetsGenerator;

        protected string m_MeshName;

        protected MeshGeneratorBase(string meshName)
        {
            m_MeshName = meshName;
        }

        public async Task<UnityEngine.Mesh> CreateMeshResult()
        {
            while (!IsCompleted)
            {
                await Task.Yield();
            }

            return m_CreationTask?.Result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_CreationTask?.Dispose();
            }
        }
    }
}
