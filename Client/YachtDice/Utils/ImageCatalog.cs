using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YachtDice.Client.Logic.Logging;

namespace YachtDice.Utils
{
    /// <summary>
    /// A set of images saved as PNG files in a folder of the resources of the application, so they do not depend on
    /// the theme that is active. It is shared by the avatars and the dice.
    /// </summary>
    internal sealed class ImageCatalog
    {
        private const string ResourcePathFormat = "{0}/{1}.png";
        private const string PackUriFormat = "pack://application:,,,/{0}";
        private const string PackageResourcesSuffix = ".g.resources";
        private const int DecodedSize = 256;
        private const double Origin = 0;

        private readonly string _folder;
        private readonly HashSet<string> _resourcePaths;
        private readonly Dictionary<string, ImageSource> _images = new Dictionary<string, ImageSource>();

        /// <summary>
        /// Prepares the images of a folder of the resources.
        /// </summary>
        /// <param name="folder">Relative path of the folder, for example Resources/Avatars.</param>
        public ImageCatalog(string folder)
        {
            if (folder == null)
            {
                throw new ArgumentNullException(nameof(folder), LogMessages.NullFolderMessage);
            }

            _folder = folder;
            _resourcePaths = ReadResourcePaths();
        }

        /// <summary>
        /// Gets an image by its name. The image is loaded the first time and reused after that.
        /// </summary>
        /// <param name="key">Name of the image, without the extension.</param>
        /// <param name="fallbackKey">Name of the image to use when the first one does not exist.</param>
        /// <returns>The image.</returns>
        public ImageSource Get(string key, string fallbackKey)
        {
            string foundKey = _resourcePaths.Contains(GetResourcePath(key)) ? key : fallbackKey;
            ImageSource image;

            if (!_images.TryGetValue(foundKey, out image))
            {
                image = Load(GetResourcePath(foundKey));
                _images.Add(foundKey, image);
            }

            return image;
        }

        private static HashSet<string> ReadResourcePaths()
        {
            Assembly assembly = typeof(ImageCatalog).Assembly;
            string resourcesName = assembly.GetName().Name + PackageResourcesSuffix;
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using (Stream stream = assembly.GetManifestResourceStream(resourcesName))
            using (var reader = new ResourceReader(stream))
            {
                IDictionaryEnumerator entries = reader.GetEnumerator();

                while (entries.MoveNext())
                {
                    paths.Add((string)entries.Key);
                }
            }

            return paths;
        }

        private static ImageSource Load(string resourcePath)
        {
            var bitmap = new BitmapImage();

            bitmap.BeginInit();
            bitmap.UriSource = new Uri(string.Format(CultureInfo.InvariantCulture, PackUriFormat, resourcePath));
            bitmap.DecodePixelWidth = DecodedSize;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            var drawing = new DrawingGroup();

            RenderOptions.SetBitmapScalingMode(drawing, BitmapScalingMode.HighQuality);
            drawing.Children.Add(new ImageDrawing(bitmap, new Rect(Origin, Origin, DecodedSize, DecodedSize)));

            var image = new DrawingImage(drawing);

            image.Freeze();

            return image;
        }

        private string GetResourcePath(string key)
        {
            return string.Format(CultureInfo.InvariantCulture, ResourcePathFormat, _folder, key);
        }
    }
}
