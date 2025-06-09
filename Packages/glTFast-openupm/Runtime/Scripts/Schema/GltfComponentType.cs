// SPDX-FileCopyrightText: 2024 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;

namespace GLTFast.Schema
{
    /// <summary>
    /// The datatype of an accessor's components
    /// </summary>
    /// <seealso href="https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#accessor-data-types"/>
    public enum GltfComponentType
    {
        /// <summary>
        /// Signed byte (8-bit integer)
        /// </summary>
        Byte = 5120,
        /// <summary>
        /// Unsigned byte (8-bit integer)
        /// </summary>
        UnsignedByte = 5121,
        /// <summary>
        /// Signed short (16-bit integer)
        /// </summary>
        Short = 5122,
        /// <summary>
        /// Unsigned short (16-bit integer)
        /// </summary>
        UnsignedShort = 5123,
        /// <summary>
        /// Unsigned int (32-bit integer)
        /// </summary>
        UnsignedInt = 5125,
        /// <summary>
        /// 32-bit floating point number
        /// </summary>
        Float = 5126
    }
}
