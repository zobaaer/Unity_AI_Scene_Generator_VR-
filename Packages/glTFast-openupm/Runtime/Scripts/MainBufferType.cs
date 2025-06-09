// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace GLTFast
{
    [Flags]
    enum MainBufferType
    {
        None = 0x0,
        Position = 0x1,
        Normal = 0x2,
        Tangent = 0x4,

        PosNorm = 0x3,
        PosNormTan = 0x7,
    }
}
