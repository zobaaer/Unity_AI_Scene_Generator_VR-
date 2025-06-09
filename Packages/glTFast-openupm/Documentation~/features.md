# Features

## Workflows

|          | Runtime | Editor (design-time)
|----------| ------ | ------
| | |
| **GameObject**
| Import   | ✅️ | ✅
| Export   | ¹☑️ | ¹ ☑️
| | |
| **Entities (see [DOTS](#data-oriented-technology-stack))**
| Import   | [☑️](#data-oriented-technology-stack) | `n/a`
| Export   |  | `n/a`

¹: Experimental. Core features missing

## Core glTF&trade; features

The glTF 2.0 specification is fully supported, with only a few minor remarks.

| | Import | Export
|------------| ------ | ------
| **Format**
|glTF (.gltf) | ✅ | ✅
|glTF-Binary (.glb) | ✅ | ✅
| | |
| **Buffer**
| External URIs | ✅ | ✅
| GLB main buffer | ✅ | ✅
| Embed buffers or textures (base-64 encoded within JSON) | ✅ |
| [meshoptimizer compression][MeshOpt] (via [package][MeshOptPkg])| ✅ |
| | |
| **Basics**
| Scenes | ✅ | ✅
| Node hierarchies | ✅ | ✅
| Cameras | ✅ | ✅
| | |
| **Images**
| PNG | ✅ | ✅
| Jpeg | ✅ | ✅
| KTX&trade; with Basis Universal compression (via [KtxUnity]) | ✅ |
| | |
| **Texture sampler**
| Filtering  | ✅ with [limitations](./KnownIssues.md) | ✅ with [limitations](./KnownIssues.md) |
| Wrap modes | ✅ | ✅ |
| | |
| **Materials Overview** (see [details](#materials-details))
| [Universal Render Pipeline (URP)][URP] | ✅ | ☑️ |
| [High Definition Render Pipeline (HDRP)][HDRP] | ✅ | ☑️ |
| Built-in Render Pipeline | ✅ | ☑️ |
| | |
| **Topologies / Primitive Types**
| TRIANGLES | ✅ | ✅
| POINTS | ✅ | ✅
| LINES | ✅ | ✅
| LINE_STRIP | ✅ | ✅
| LINE_LOOP | ✅ | ✅
| TRIANGLE_STRIP | ✅ | `n/a`
| TRIANGLE_FAN | ✅ | `n/a`
| Quads | `n/a` | ✅ via triangulation
| | |
| **Meshes**
| Positions | ✅ | ✅
| Normals | ✅ | ✅
| Tangents | ✅ | ✅
| Texture coordinates / UV sets | ✅ | ✅
| Three or more texture coordinates / UV sets | ¹☑️ | ✅
| Vertex colors | ✅ | ✅
| Draco&trade; mesh compression (via [DracoForUnity]) | ✅ | ✅
| Implicit (no) indices | ✅ |
| Per primitive material | ✅ | ✅
| Joints (up to 4 per vertex) | ✅ | ✅
| Weights (up to 4 per vertex) | ✅ | ✅
| [Skins][Skins] | ✅ | ✅
| Morph Targets (Blend Shapes) | ✅ |
| Sparse accessors | ² ✅ |
| | |
| **Animation**
| via legacy Animation System | ✅ |
| via Playable API ([issue][AnimationPlayables]) |  |
| via Mecanim ([issue][AnimationMecanim]) | ³☑️ |

¹: Up to eight UV sets can imported, but *Unity glTFast* shaders only support two (see [issue][UVsets]).

²: Not on all accessor types; morph targets and vertex positions only

³: Animation clips can be imported Mecanim compatible, but they won't be assigned and cannot be played back without further work.

## Extensions

glTF defines an [extension mechanism][gltf-ext] that allows the base format to be extended with new capabilities. Here's a list of extensions and their current state of support.

### Official Khronos&reg; extensions

| | Import | Export
|------------| ------ | ------
| | |
| **Khronos**
| KHR_animation_pointer | | |
| KHR_draco_mesh_compression | ✅ | ✅
| KHR_lights_punctual | ✅ | ✅
| KHR_materials_anisotropy | | |
| KHR_materials_clearcoat | ✅ | ✅
| KHR_materials_dispersion | | |
| KHR_materials_emissive_strength | | |
| KHR_materials_ior | [ℹ️][IOR] |
| KHR_materials_iridescence | | |
| KHR_materials_sheen | [ℹ️][Sheen] |
| KHR_materials_specular | [ℹ️][Specular] |
| KHR_materials_transmission | [ℹ️][Transmission] |
| KHR_materials_unlit | ✅ | ✅
| KHR_materials_variants | ✅ |
| KHR_materials_volume | [ℹ️][Volume] |
| KHR_mesh_quantization | ✅ |
| KHR_texture_basisu | ✅ |
| KHR_texture_transform | ✅ | ✅
| KHR_xmp_json_ld |️ |
| ²KHR_materials_pbrSpecularGlossiness | ☑️ |
| | |
| **Vendor**
| ¹EXT_mesh_gpu_instancing | ✅ |
| EXT_meshopt_compression | ✅ |
| EXT_lights_image_based | [ℹ️][IBL] |
| EXT_texture_webp | | |

¹: Without support for custom vertex attributes (e.g. `_ID`)

²: Archived/obsolete; Superseded by KHR_materials_specular

Not investigated yet:

- ADOBE_materials_clearcoat_tint
- AGI_articulations
- AGI_stk_metadata
- CESIUM_primitive_outline
- EXT_lights_ies
- EXT_mesh_manifold
- GRIFFEL_bim_data
- MPEG_accessor_timed
- MPEG_animation_timing
- MPEG_audio_spatial
- MPEG_buffer_circular
- MPEG_media
- MPEG_mesh_linking
- MPEG_scene_dynamic
- MPEG_texture_video
- MPEG_viewport_recommended
- MSFT_lod
- MSFT_packing_normalRoughnessMetallic
- MSFT_packing_occlusionRoughnessMetallic
- NV_materials_mdl

 Will not become supported (reason in brackets):

- KHR_xmp (archived; prefer KHR_xmp_json_ld)
- KHR_techniques_webgl (archived)
- ADOBE_materials_clearcoat_specular (prefer KHR_materials_clearcoat)
- ADOBE_materials_thin_transparency (prefer KHR_materials_transmission)
- FB_geometry_metadata (prefer KTX_xmp)
- MSFT_texture_dds (prefer KTX/basisu)

### Custom extras and extensions

Optional `extras` and `extensions` object properties are supported. glTFast uses Newtonsoft JSON parser to access these additional properties.

See [glTFast Add-on API](UseCaseCustomExtras.md) for an example to import the `extras` property in a gltf asset.

## Materials Details

### Material Import

| Material Feature              | URP | HDRP | Built-In |
|-------------------------------|-----|------|----------|
| PBR¹ Metallic-Roughness        | ✅  | ✅   | ✅       |
| PBR¹ Specular-Glossiness       | ✅  | ✅   | ✅       |
| Unlit                         | ✅  | ✅   | ✅       |
| Normal texture                | ✅  | ✅   | ✅       |
| Occlusion texture             | ✅  | ✅   | ✅       |
| Emission texture              | ✅  | ✅   | ✅       |
| Alpha modes OPAQUE/MASK/BLEND | ✅  | ✅   | ✅       |
| Double sided / Two sided      | ✅  | ✅   | ✅       |
| Vertex colors                 | ✅  | ✅   | ✅       |
| Multiple UV sets              | ✅²  | ✅²   | ✅²       |
| Texture Transform             | ✅  | ✅   | ✅       |
| Clear coat                    | ☑️³  | ✅  | [⛔️][ClearCoat] |
| Sheen                         | [ℹ️][Sheen] | [ℹ️][Sheen] | [⛔️][Sheen] |
| Transmission                  | [☑️][Transmission]⁴ | [☑️][Transmission]⁵ | [☑️][Transmission]⁵ |
| Variants                      | ✅ | ✅ | ✅ |
| IOR                           | [ℹ️][IOR]      | [ℹ️][IOR]      | [⛔️][IOR]      |
| Specular                      | [ℹ️][Specular] | [ℹ️][Specular] | [⛔️][Specular] |
| Volume                        | [ℹ️][Volume]   | [ℹ️][Volume]   | [⛔️][Volume]   |
| Point clouds                  |      |     | Unlit only |

¹: Physically-Based Rendering (PBR) material model

²: Two sets of texture coordinates (as required by the glTF 2.0 specification) are supported, but not three or more ([issue][UVSets])

³: Only supports Universal Render Pipeline versions >= 12.0; Only coat mask and smoothness are supported, other coat related properties, such as coat normal, are not supported

⁴: There are two approximation implementations for transmission in Universal render pipeline. If the Opaque Texture is enabled (in the Universal RP Asset settings), it is sampled to provide proper transmissive filtering. The downside of this approach is transparent objects are not rendered on top of each other. If the opaque texture is not available, the common approximation (see ⁴ below) is used.

⁵: Transmission in Built-In and HD render pipeline does not support transmission textures and is only 100% correct in certain cases like clear glass (100% transmission, white base color). Otherwise it's an approximation.

### Material Export

Material export support depends largely on the shaders used. We differentiate between Unity shaders (default shaders of Unity render pipelines) and glTFast's own shaders.

#### Unity Shaders

Unity shaders are typically used on pre-existing assets.

Supported Unity shaders:

- Universal and High Definition render pipeline
  - `Lit`
  - `Unlit`
- Built-In render pipeline
  - `Standard`
  - `Unlit`

Other shaders might (partially) work if they have similar properties (with identical names).

| Material Feature              | URP¹ | HDRP² | Built-In³
|-------------------------------|-----|------|----------
| PBR Metallic-Roughness        | ✅ | ✅ | ✅
| PBR Specular-Glossiness       |  |  |&nbsp;
| Unlit                         | ✅ | ✅ | ✅
| Normal texture                | ✅ | ✅ | ✅
| Occlusion texture             | ✅ | ✅ | ✅
| Emission texture              | ✅ | ✅ | ✅
| Alpha modes OPAQUE/MASK/BLEND | ✅ | ✅ | ✅
| Double sided / Two sided      | ✅ | ✅ | ✅
| Vertex colors                 | ✅⁴ | ✅⁴ | ✅⁴
| Texture Transform             | ✅ | ✅ | ✅
| Clear coat                    | `n/a` | ✅ | `n/a`
| Sheen                         | `?` | `?` | `n/a`
| Transmission                  |  |  | `n/a`
| IOR                           |  |  | `n/a`
| Specular                      |  |  |&nbsp;
| Volume                        |  |  | `n/a`

¹: Universal Render Pipeline Lit Shader

²: High Definition Render Pipeline Lit Shader

³: Built-In Render Pipeline Standard and Unlit Shader

⁴: The vertex color attribute is only exported if the material/shader makes use of it (see [Vertex Attribute Discarding](ExportRuntime.md#vertex-attribute-discarding)).

#### glTFast Shaders

glTFast's own shaders are typically used when the assets were imported with glTFast. This enables round-trip import-export workflows. They are also the preferred way to author assets for glTF export specifically.

Supported glTFast shaders/shader graphs:

- Shader Graphs
  - [x] `Shader Graphs/glTF-pbrMetallicRoughness`
  - [x] `Shader Graphs/glTF-unlit`
  - [ ] `Shader Graphs/glTF-pbrSpecularGlossiness`
  - [ ] Legacy shader graphs (in folder `Runtime/Shader/Legacy`; used for Universal Render Pipeline 10.x and older)
- Shaders
  - [x] `glTF/PbrMetallicRoughness`
  - [x] `glTF/Unlit`
  - [ ] `glTF/PbrSpecularGlossiness`

| Material Feature              | URP | HDRP | Built-In
|-------------------------------|-----|------|----------
| Normal texture                | ✅ | ✅ | ✅
| Occlusion texture             | ✅ | ✅ | ✅
| Emission texture              | ✅ | ✅ | ✅
| Alpha modes OPAQUE/MASK/BLEND | ✅ | ✅ | ✅
| Double sided / Two sided      | ✅ | ✅ | ✅
| Vertex colors                 | ✅ | ✅ | ✅
| Multiple UV sets              | ✅¹ | ✅¹ | ✅¹
| Texture Transform             | ✅ | ✅ | ✅
| Clear coat                    | `n/a` |  | `n/a`
| Sheen                         | `?` | `?` | `n/a`
| Transmission                  |  |  | `n/a`
| IOR                           |  |  | `n/a`
| Specular                      |  |  |&nbsp;
| Volume                        |  |  | `n/a`

¹: Only two UV sets are supported by the shaders.

## Data-Oriented Technology Stack

> ⚠️ Note: DOTS is highly experimental and many features don't work yet. Do not use it for production ready projects!

Unity's [Data-Oriented Technology Stack (DOTS)][DOTS] allows users to create high performance gameplay. *Unity glTFast* has experimental import support for it.

Instead of traditional GameObjects, *Unity glTFast* will instantiate [Entities][Entities] and render them via [Entities Graphics][EntitiesGraphics].

Possibly incomplete list of things that are known to not work with Entities yet:

- Animation
- Skinning
- Morph targets
- Cameras
- Lights

### DOTS Setup

- Install the [Entities Graphics][EntitiesGraphics] package
- Use `GltfEntityAsset` instead of `GltfAsset`
- For customized behavior, use the `EntityInstantiator` instead of the `GameObjectInstantiator`

## Unity Version Support

*Unity glTFast* requires Unity 2020.3.48f1 or newer.

## Legend

- ✅ Fully supported
- ☑️ Partially supported
- ℹ️ Planned (click for issue)
- ⛔️ No plan to support (click for issue)
- `?`: Unknown / Untested
- `n/a`: Not available

## Trademarks

*Unity&reg;* is a registered trademark of [Unity Technologies][Unity].

*Khronos&reg;* is a registered trademark and *glTF&trade;* is a trademark of [The Khronos Group Inc][Khronos].

*KTX&trade;* and the KTX logo are trademarks of the [The Khronos Group Inc][Khronos].

*Draco&trade;* is a trademark of [*Google LLC*][GoogleLLC].

[AnimationMecanim]: https://github.com/atteneder/glTFast/issues/167
[AnimationPlayables]: https://github.com/atteneder/glTFast/issues/166
[ClearCoat]: https://github.com/atteneder/glTFast/issues/68
[DracoForUnity]: https://docs.unity3d.com/Packages/com.unity.cloud.draco@latest
[DOTS]: https://unity.com/dots
[Entities]: https://docs.unity3d.com/Packages/com.unity.entities@latest
[EntitiesGraphics]: https://docs.unity3d.com/Packages/com.unity.entities.graphics@latest
[GoogleLLC]: https://about.google/
[gltf-ext]: https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html#specifying-extensions
[HDRP]: https://unity.com/srp/High-Definition-Render-Pipeline
[IBL]: https://github.com/atteneder/glTFast/issues/108
[IOR]: https://github.com/atteneder/glTFast/issues/207
[Khronos]: https://www.khronos.org
[KtxUnity]: https://docs.unity3d.com/Packages/com.unity.cloud.ktx@latest
[MeshOpt]: https://github.com/KhronosGroup/glTF/tree/main/extensions/2.0/Vendor/EXT_meshopt_compression
[MeshOptPkg]: https://docs.unity3d.com/Packages/com.unity.meshopt.decompress@0.1/manual/index.html
[Sheen]: https://github.com/atteneder/glTFast/issues/110
[Skins]: https://www.khronos.org/registry/glTF/specs/2.0/glTF-2.0.html#skins
[Specular]: https://github.com/atteneder/glTFast/issues/208
[Transmission]: https://github.com/atteneder/glTFast/issues/111
[Unity]: https://unity.com
[URP]: https://unity.com/srp/universal-render-pipeline
[UVsets]: https://github.com/atteneder/glTFast/issues/206
[Volume]: https://github.com/atteneder/glTFast/issues/209
