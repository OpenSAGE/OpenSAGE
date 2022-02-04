using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using OpenSage.Content.Loaders;
using OpenSage.FileFormats.W3d;
using OpenSage.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal abstract class ShaderMaterialShaderResources : ShaderSet
    {
        private readonly Dictionary<string, Material> _materialCache = new();

        public readonly Dictionary<string, ResourceBinding> MaterialResourceBindings;
        
        public readonly Pipeline Pipeline;

        protected ShaderMaterialShaderResources(
            ShaderSetStore store,
            string shaderName,
            Func<IEnumerable<ResourceBinding>> createMaterialResourceBindings)
            : base(store, shaderName, MeshShaderResources.MeshVertex.VertexDescriptors)
        {
            var materialResourceBindings = createMaterialResourceBindings().ToArray();

            MaterialResourceBindings = materialResourceBindings.ToDictionary(x => x.Description.Name);

            Pipeline = AddDisposable(
                GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                    new GraphicsPipelineDescription(
                        BlendStateDescription.SingleDisabled,
                        DepthStencilStateDescription.DepthOnlyLessEqual,
                        RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise,
                        PrimitiveTopology.TriangleList,
                        Description,
                        ResourceLayouts,
                        store.OutputDescription)));
        }

        public Material GetCachedMaterial(W3dShaderMaterial w3dShaderMaterial, AssetLoadContext context)
        {
            var key = GetCacheKey(w3dShaderMaterial);

            if (!_materialCache.TryGetValue(key, out var result))
            {
                var materialResourceSet = CreateMaterialResourceSet(w3dShaderMaterial, context);

                result = AddDisposable(
                    new Material(
                        this,
                        Pipeline,
                        materialResourceSet));

                _materialCache.Add(key, result);
            }

            return result;
        }

        private static string GetCacheKey(W3dShaderMaterial w3dShaderMaterial)
        {
            // TODO: Optimise this.

            using var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

            sha256.AppendData(BitConverter.GetBytes(w3dShaderMaterial.Properties.Count));

            foreach (var property in w3dShaderMaterial.Properties)
            {
                sha256.AppendData(BitConverter.GetBytes((int)property.PropertyType));
                sha256.AppendData(Encoding.UTF8.GetBytes(property.PropertyName));
                sha256.AppendData(Encoding.UTF8.GetBytes(property.StringValue));
                sha256.AppendData(BitConverter.GetBytes(property.Value.Vector4.X));
                sha256.AppendData(BitConverter.GetBytes(property.Value.Vector4.Y));
                sha256.AppendData(BitConverter.GetBytes(property.Value.Vector4.Z));
                sha256.AppendData(BitConverter.GetBytes(property.Value.Vector4.W));
            }

            var hash = sha256.GetCurrentHash();

            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }

        private ResourceSet CreateMaterialResourceSet(W3dShaderMaterial w3dShaderMaterial, AssetLoadContext context)
        {
            var materialResourceSetBuilder = AddDisposable(new ShaderMaterialResourceSetBuilder(
                GraphicsDevice,
                this));

            foreach (var w3dShaderProperty in w3dShaderMaterial.Properties)
            {
                switch (w3dShaderProperty.PropertyType)
                {
                    case W3dShaderMaterialPropertyType.Texture:
                        var texture = context.AssetStore.Textures.GetByName(w3dShaderProperty.StringValue) ?? context.StandardGraphicsResources.PlaceholderTexture;
                        materialResourceSetBuilder.SetTexture(w3dShaderProperty.PropertyName, texture);
                        break;

                    case W3dShaderMaterialPropertyType.Bool:
                        materialResourceSetBuilder.SetConstant(w3dShaderProperty.PropertyName, w3dShaderProperty.Value.Bool);
                        break;

                    case W3dShaderMaterialPropertyType.Float:
                        materialResourceSetBuilder.SetConstant(w3dShaderProperty.PropertyName, w3dShaderProperty.Value.Float);
                        break;

                    case W3dShaderMaterialPropertyType.Vector2:
                        materialResourceSetBuilder.SetConstant(w3dShaderProperty.PropertyName, w3dShaderProperty.Value.Vector2);
                        break;

                    case W3dShaderMaterialPropertyType.Vector3:
                        materialResourceSetBuilder.SetConstant(w3dShaderProperty.PropertyName, w3dShaderProperty.Value.Vector3);
                        break;

                    case W3dShaderMaterialPropertyType.Vector4:
                        materialResourceSetBuilder.SetConstant(w3dShaderProperty.PropertyName, w3dShaderProperty.Value.Vector4);
                        break;

                    case W3dShaderMaterialPropertyType.Int:
                        materialResourceSetBuilder.SetConstant(w3dShaderProperty.PropertyName, w3dShaderProperty.Value.Int);
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            return materialResourceSetBuilder.CreateResourceSet();
        }

        public ResourceSet CreateMaterialResourceSet(BindableResource[] resources)
        {
            return GraphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    MaterialResourceLayout,
                    resources));
        }
    }
}
