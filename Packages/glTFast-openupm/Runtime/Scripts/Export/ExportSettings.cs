// SPDX-FileCopyrightText: 2023 Unity Technologies and the glTFast authors
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEngine;

namespace GLTFast.Export
{

    /// <summary>
    /// glTF format
    /// </summary>
    public enum GltfFormat
    {
        /// <summary>
        /// JSON-based glTF (.gltf file extension)
        /// </summary>
        Json,
        /// <summary>
        /// glTF-binary (.glb file extension)
        /// </summary>
        Binary
    }

    /// <summary>
    /// Destination for image files
    /// </summary>
    public enum ImageDestination
    {
        /// <summary>
        /// Automatic decision. Main buffer for glTF-binary, separate files for JSON-based glTFs.
        /// </summary>
        Automatic,
        /// <summary>
        /// Embeds images in main buffer
        /// </summary>
        MainBuffer,
        /// <summary>
        /// Saves images as separate files relative to glTF file
        /// </summary>
        SeparateFile
    }

    /// <summary>
    /// Resolutions to existing file conflicts
    /// </summary>
    public enum FileConflictResolution
    {
        /// <summary>
        /// Abort and keep existing files
        /// </summary>
        Abort,
        /// <summary>
        /// Replace existing files with newly created ones
        /// </summary>
        Overwrite
    }

    /// <summary>
    /// glTF compression method.
    /// </summary>
    [Flags]
    public enum Compression
    {
        /// <summary>No compression</summary>
        Uncompressed = 1,
        /// <summary><a href="https://meshoptimizer.org/#vertexindex-buffer-compression">Meshopt compression</a>
        /// via <see cref="Extension.MeshoptCompression"/></summary>
        MeshOpt = 1 << 1,
        /// <summary><a href="https://google.github.io/draco/">Draco 3D Data compression</a>
        /// via <see cref="Extension.DracoMeshCompression"/></summary>
        Draco = 1 << 2,
    }

    /// <summary>
    /// glTF export settings
    /// </summary>
    public class ExportSettings
    {
        /// <summary>
        /// Export to JSON-based or binary format glTF files
        /// </summary>
        public GltfFormat Format { get; set; } = GltfFormat.Json;

        /// <inheritdoc cref="Export.ImageDestination"/>
        public ImageDestination ImageDestination { get; set; } = ImageDestination.Automatic;

        /// <inheritdoc cref="Export.FileConflictResolution"/>
        public FileConflictResolution FileConflictResolution { get; set; } = FileConflictResolution.Abort;

        /// <summary>
        /// Light intensity values are multiplied by this factor.
        /// </summary>
        [field: Tooltip("Light intensity values are multiplied by this factor")]
        public float LightIntensityFactor { get; set; } = 1.0f;

        /// <summary>
        /// Component type flags to include or exclude components from export
        /// based on type.
        /// </summary>
        public ComponentType ComponentMask { get; set; } = ComponentType.All;

        /// <summary>
        /// Type of compression to apply
        /// </summary>
        public Compression Compression { get; set; } = Compression.Uncompressed;

        /// <summary>
        /// Draco compression export settings
        /// </summary>
        public DracoExportSettings DracoSettings { get; set; }

        /// <summary>
        /// If true, the export results will not differ on repeated exports. This comes at the cost of potentially
        /// longer export times on scenes with multiple, large meshes.
        /// This can be a requirement for certain asset pipelines or automated tests.
        /// </summary>
        public bool Deterministic { get; set; }

        /// <summary>
        /// Controls which vertex attributes are preserved during export, regardless whether they are used/referenced
        /// or not. By default, unused attributes are discarded.
        /// </summary>
        public VertexAttributeUsage PreservedVertexAttributes { get; set; } = VertexAttributeUsage.None;

        /// <summary>
        /// [1-100] quality for JPG images
        /// </summary>
        public int JpgQuality { get; set; } = 60;
    }
}
