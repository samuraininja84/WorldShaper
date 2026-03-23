namespace WorldShaper.Editor
{
    /// <summary>
    /// Extension methods for constructing file paths to icons used in the World Shaper editor.
    /// </summary>
    public static class IconPathExtensions
    {
        /// <summary>
        /// The path to the icons used in the World Shaper editor.
        /// </summary>
        public const string IconsPath = "Assets/Plugins/Artisan/World Shaper/Editor/EditorResources/Icons/";

        /// <summary>
        /// Generates the full file path for an image by combining the specified icon name, file extension, and directory path.
        /// </summary>
        /// <remarks>
        /// This method is an extension method for the string type, allowing convenient construction of image file paths for icons. 
        /// The resulting path does not include validation for file existence.
        /// </remarks>
        /// <param name="iconName">The name of the icon file, without extension. Cannot be null or empty.</param>
        /// <param name="extension">The file extension to use for the image. Defaults to "png" if not specified.</param>
        /// <param name="path">The directory path where the image is located. Defaults to the value of IconsPath if not specified.</param>
        /// <returns>A string containing the full file path to the image, constructed from the provided icon name, extension, and path.</returns>
        public static string ToImagePath(this string iconName, string extension = "png", string path = IconsPath) => $"{path}{iconName}.{extension}";
    }
}