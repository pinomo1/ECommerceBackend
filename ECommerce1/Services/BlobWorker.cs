using Azure.Storage.Blobs;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ECommerce1.Services
{
    public class BlobWorker
    {
        public BlobServiceClient BlobServiceClient { get; set; }
        public IConfiguration Configuration { get; set; }
        public BlobWorker(BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            BlobServiceClient = blobServiceClient;
            Configuration = configuration;
        }

        public async Task<string> AddPublicationPhoto(IFormFile? file)
        {
            if(file != null)
            {
                try
                {
                    string reference = String.Empty;
                    string containerPublicationPhoto = "uploads";
                    var containerClientPhoto = BlobServiceClient.GetBlobContainerClient(containerPublicationPhoto);
                    string newName;
                        if (file == null)
                        {
                            return String.Empty;
                        }
                        newName = Guid.NewGuid().ToString() +
                                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() + Path.GetExtension(file!.FileName);
                        using (Stream stream = file!.OpenReadStream())
                        {
                            await containerClientPhoto.UploadBlobAsync(newName, stream);
                        }
                        reference = $"{Configuration["Links:Files:Pictures"]}{newName}";
                    return reference;
                }
                catch (Exception)
                {
                    return String.Empty;
                }
            }
            return String.Empty;
        }

        public async Task<IEnumerable<string>> AddPublicationPhotos(IFormFile?[] files)
        {
            if (files != null)
            {
                try
                {
                    List<string> references = new List<string>();
                    string containerPublicationPhoto = "uploads";
                    var containerClientPhoto = BlobServiceClient.GetBlobContainerClient(containerPublicationPhoto);
                    string newName;
                    for (int i = 0; i < files.Length; i++)
                    {
                        if (files[i] == null)
                        {
                            continue;
                        }
                        newName = Guid.NewGuid().ToString() +
                                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() + Path.GetExtension(files[i]!.FileName);
                        using (Stream stream = files[i]!.OpenReadStream())
                        {
                            await containerClientPhoto.UploadBlobAsync(newName, stream);
                        }
                        references.Add($"{Configuration["Links:Files:Pictures"]}{newName}");
                    }
                    if (references.Count < 1)
                    {
                        await this.RemovePublications(references.ToArray());
                        return Enumerable.Empty<string>();
                    }
                    return references;
                }
                catch (Exception)
                {
                    return Enumerable.Empty<string>();
                }
            }
            return Enumerable.Empty<string>();
        }
        public async Task RemovePublications(string[] references)
        {
            var containerClient = BlobServiceClient.GetBlobContainerClient(Configuration["ConnectionStrings:BlobStorage"]);
            foreach (var reference in references)
            {
                if (!String.IsNullOrWhiteSpace(reference))
                {
                    await RemoveFromBlobStorage(containerClient, reference);
                }
            }
        }
        public async Task RemoveFromBlobStorage(BlobContainerClient containerClient, string path)
        {
            if (!String.IsNullOrWhiteSpace(path))
            {
                var oldFileName = path.Substring(path.LastIndexOf('/') + 1);
                await containerClient.DeleteBlobIfExistsAsync(oldFileName);
            }
        }

        private async Task<IFormFile?> FormatPhoto(IFormFile file, int thumbnailWidth)
        {
            try
            {
                using (Stream fileStream = file.OpenReadStream())
                {
                    if (fileStream != null)
                    {
                        var extension = Path.GetExtension(file.FileName);
                        var encoder = GetEncoder(extension);
                        if (encoder != null)
                        {
                            var buffer = new byte[fileStream.Length];
                            await fileStream.ReadAsync(buffer, 0, Convert.ToInt32(fileStream.Length));
                            using (Image<Rgba32> image = Image.Load<Rgba32>(buffer))
                            {
                                var output = new MemoryStream();
                                decimal divisor = image.Width / (decimal)thumbnailWidth;
                                var height = Convert.ToInt32(Math.Round(image.Height / divisor));
                                image.Mutate(x => x.Resize(thumbnailWidth, height));
                                image.Save(output, encoder);
                                output.Position = 0;
                                return new FormFile(output, 0, output.Length, "\"file\"", file.FileName);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }
        private IImageEncoder? GetEncoder(string extension)
        {
            IImageEncoder? encoder = null;
            extension = extension.Replace(".", "");
            var isSupported = Regex.IsMatch(extension, "jpe?g|png|gif", RegexOptions.IgnoreCase);
            if (isSupported)
            {
                switch (extension)
                {
                    case "png":
                        encoder = new PngEncoder();
                        break;
                    case "jpg":
                    case "jpeg":
                        encoder = new JpegEncoder();
                        break;
                    case "gif":
                        encoder = new GifEncoder();
                        break;
                }
            }
            return encoder;
        }
    }
}
