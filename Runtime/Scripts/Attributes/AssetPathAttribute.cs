using System;

namespace WorldShaper
{
    /// <summary>
    /// Specifies a file system path and an optional asset name for a class.
    /// </summary>
    /// <remarks>
    /// This attribute is used to associate a class with a specific file path and, optionally, an asset name. 
    /// It is typically applied to classes that represent or interact with assets stored in the file system.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AssetPathAttribute : Attribute
    {
        /// <summary>
        /// Gets the file system path associated with this instance.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the name of the asset.
        /// </summary>
        public string Asset { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetPathAttribute"/> class with the specified file path and optional asset name.
        /// </summary>
        /// <param name="path">The file path associated with the asset. This value cannot be null or empty.</param>
        /// <param name="asset">The name of the asset. This value cannot be null or empty.</param>
        public AssetPathAttribute(string asset, string path = null)
        {
            Asset = asset;
            Path = path;
        }
    }
}