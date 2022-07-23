﻿using NAPS2.Images.Gdi;
using NAPS2.ImportExport.Pdf;
using NAPS2.Scan;
using NAPS2.Sdk.Tests.Asserts;
using NAPS2.Sdk.Tests.ImportExport;
using NAPS2.Serialization;
using Xunit;

namespace NAPS2.Sdk.Tests.Images;

public class ImageSerializerTests : ContextualTests
{
    // TODO: Add scanning context tests that verify created images have the right storage

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public void SerializesMetadata(StorageConfig config)
    {
        config.Apply(this);

        using var destContext = new ScanningContext(new GdiImageContext());

        using var sourceImage = ScanningContext.CreateProcessedImage(
            new GdiImage(SharedData.color_image), // TODO: Use an actual grayscale image
            BitDepth.Grayscale,
            true,
            -1,
            new[] { new BrightnessTransform(300) });

        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions());
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions());

        Assert.Single(destImage.TransformState.Transforms);
        Assert.Equal(300, Assert.IsType<BrightnessTransform>(destImage.TransformState.Transforms[0]).Brightness);
        Assert.True(destImage.Metadata.Lossless);
        Assert.Equal(BitDepth.Grayscale, destImage.Metadata.BitDepth);
        ImageAsserts.Similar(TransformTestsData.color_image_b_p300, ImageContext.Render(destImage));
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public void DeserializeToFileStorage(StorageConfig config)
    {
        config.Apply(this);

        using var destContext = new ScanningContext(new GdiImageContext(),
            FileStorageManager.CreateFolder(Path.Combine(FolderPath, "dest")));

        using var sourceImage = ScanningContext.CreateProcessedImage(new GdiImage(SharedData.color_image));
        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions());
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions());

        Assert.IsType<ImageFileStorage>(destImage.Storage);
        // Check that disposing the original doesn't interfere with rendering, i.e. not using the same backing file
        sourceImage.Dispose();
        ImageAsserts.Similar(TransformTestsData.color_image, ImageContext.Render(destImage));
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public void DeserializeToMemoryStorage(StorageConfig config)
    {
        config.Apply(this);

        using var destContext = new ScanningContext(new GdiImageContext());

        using var sourceImage = ScanningContext.CreateProcessedImage(new GdiImage(SharedData.color_image));
        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions());
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions());

        Assert.IsType<GdiImage>(destImage.Storage);
        // Check that disposing the original doesn't interfere with rendering, i.e. not using the same image
        sourceImage.Dispose();
        ImageAsserts.Similar(TransformTestsData.color_image, ImageContext.Render(destImage));
    }

    [Fact]
    public void ShareFileStorage()
    {
        new StorageConfig.File().Apply(this);

        using var destContext = new ScanningContext(new GdiImageContext(),
            FileStorageManager.CreateFolder(Path.Combine(FolderPath, "dest")));

        using var sourceImage = ScanningContext.CreateProcessedImage(new GdiImage(SharedData.color_image));
        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions());
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions
        {
            ShareFileStorage = true
        });

        var sourceStorage = Assert.IsType<ImageFileStorage>(sourceImage.Storage);
        var destStorage = Assert.IsType<ImageFileStorage>(destImage.Storage);
        Assert.Equal(sourceStorage.FullPath, destStorage.FullPath);
        ImageAsserts.Similar(TransformTestsData.color_image, ImageContext.Render(destImage));

        destImage.Dispose();
        Assert.True(File.Exists(sourceStorage.FullPath));
        sourceImage.Dispose();
        Assert.False(File.Exists(sourceStorage.FullPath));
    }

    [Fact]
    public void TransferOwnership()
    {
        new StorageConfig.File().Apply(this);

        using var destContext = new ScanningContext(new GdiImageContext(),
            FileStorageManager.CreateFolder(Path.Combine(FolderPath, "dest")));

        using var sourceImage = ScanningContext.CreateProcessedImage(new GdiImage(SharedData.color_image));
        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions
        {
            TransferOwnership = true
        });
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions());

        var sourceStorage = Assert.IsType<ImageFileStorage>(sourceImage.Storage);
        var destStorage = Assert.IsType<ImageFileStorage>(destImage.Storage);
        Assert.Equal(sourceStorage.FullPath, destStorage.FullPath);
        ImageAsserts.Similar(TransformTestsData.color_image, ImageContext.Render(destImage));

        sourceImage.Dispose();
        Assert.True(File.Exists(sourceStorage.FullPath));
        destImage.Dispose();
        Assert.False(File.Exists(sourceStorage.FullPath));
    }

    [Fact]
    public void CrossDevice()
    {
        new StorageConfig.File().Apply(this);

        using var destContext = new ScanningContext(new GdiImageContext(),
            FileStorageManager.CreateFolder(Path.Combine(FolderPath, "dest")));

        using var sourceImage = ScanningContext.CreateProcessedImage(new GdiImage(SharedData.color_image));
        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions
        {
            CrossDevice = true
        });
        // Disposing before deserialization should have no effect with CrossDevice set
        sourceImage.Dispose();
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions());

        ImageAsserts.Similar(TransformTestsData.color_image, ImageContext.Render(destImage));
    }

    [Fact]
    public void DisposeBeforeDeserialization()
    {
        new StorageConfig.File().Apply(this);

        using var destContext = new ScanningContext(new GdiImageContext(),
            FileStorageManager.CreateFolder(Path.Combine(FolderPath, "dest")));

        using var sourceImage = CreateScannedImage();
        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions());
        sourceImage.Dispose();

        Assert.Throws<FileNotFoundException>(() =>
            ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions()));
    }

    [Fact]
    public void ShareFileStorage_DisposeBeforeDeserialization()
    {
        new StorageConfig.File().Apply(this);

        using var destContext = new ScanningContext(new GdiImageContext(),
            FileStorageManager.CreateFolder(Path.Combine(FolderPath, "dest")));

        using var sourceImage = CreateScannedImage();
        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions());
        sourceImage.Dispose();

        Assert.Throws<FileNotFoundException>(() =>
            ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions
            {
                ShareFileStorage = true
            }));
    }

    [Fact]
    public void TransferOwnership_FailsWithClone()
    {
        new StorageConfig.File().Apply(this);

        using var sourceImage = ScanningContext.CreateProcessedImage(new GdiImage(SharedData.color_image));
        using var clone = sourceImage.Clone();
        Assert.Throws<ArgumentException>(() => ImageSerializer.Serialize(sourceImage, new SerializeImageOptions
        {
            TransferOwnership = true
        }));
    }

    [Fact]
    public void TransferOwnership_DisposesMemory()
    {
        using var sourceImage = ScanningContext.CreateProcessedImage(new GdiImage(SharedData.color_image));
        ImageSerializer.Serialize(sourceImage, new SerializeImageOptions
        {
            TransferOwnership = true
        });
        Assert.Throws<ObjectDisposedException>(() => sourceImage.Clone());
    }

    [Fact]
    public void RequireFileStorage_NoFileStorage()
    {
        using var sourceImage = CreateScannedImage();
        Assert.Throws<ArgumentException>(() => ImageSerializer.Serialize(sourceImage, new SerializeImageOptions
        {
            RequireFileStorage = true
        }));
    }

    [Fact]
    public void RequireFileStorage_AndCrossDevice()
    {
        using var sourceImage = CreateScannedImage();
        Assert.Throws<ArgumentException>(() => ImageSerializer.Serialize(sourceImage, new SerializeImageOptions
        {
            RequireFileStorage = true,
            CrossDevice = true
        }));
    }

    [Fact]
    public void CrossDevice_AndRenderedFilePath()
    {
        using var sourceImage = CreateScannedImage();
        Assert.Throws<ArgumentException>(() => ImageSerializer.Serialize(sourceImage, new SerializeImageOptions
        {
            CrossDevice = true,
            RenderedFilePath = "something.jpg"
        }));
    }

    [Fact]
    public void CrossDevice_AndTransferOwnership()
    {
        using var sourceImage = CreateScannedImage();
        Assert.Throws<ArgumentException>(() => ImageSerializer.Serialize(sourceImage, new SerializeImageOptions
        {
            CrossDevice = true,
            TransferOwnership = true
        }));
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task PdfDeserializeToFileStorage(StorageConfig config)
    {
        config.Apply(this);

        var importPath = Path.Combine(FolderPath, "import.pdf");
        File.WriteAllBytes(importPath, PdfData.word_generated_pdf);

        using var destContext = new ScanningContext(new GdiImageContext(),
            FileStorageManager.CreateFolder(Path.Combine(FolderPath, "dest")));

        using var sourceImage = (await new PdfSharpImporter(ScanningContext).Import(importPath).ToList()).First();
        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions());
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions());

        Assert.IsType<ImageFileStorage>(destImage.Storage);
        // Check that disposing the original doesn't interfere with rendering, i.e. not using the same backing file
        sourceImage.Dispose();
        ImageAsserts.Similar(PdfData.word_p1, ImageContext.Render(destImage));
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public async Task PdfDeserializeToMemoryStorage(StorageConfig config)
    {
        config.Apply(this);

        var importPath = Path.Combine(FolderPath, "import.pdf");
        File.WriteAllBytes(importPath, PdfData.word_generated_pdf);

        using var destContext = new ScanningContext(new GdiImageContext());

        using var sourceImage = (await new PdfSharpImporter(ScanningContext).Import(importPath).ToList()).First();
        var serializedImage = ImageSerializer.Serialize(sourceImage, new SerializeImageOptions());
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions());

        Assert.IsType<ImageMemoryStorage>(destImage.Storage);
        // Check that disposing the original doesn't interfere with rendering, i.e. not using the same image
        sourceImage.Dispose();
        ImageAsserts.Similar(PdfData.word_p1, ImageContext.Render(destImage));
    }

    [Theory]
    [ClassData(typeof(StorageAwareTestData))]
    public void IncludeThumbnail(StorageConfig config)
    {
        config.Apply(this);

        using var destContext = new ScanningContext(new GdiImageContext());

        using var sourceImage = ScanningContext.CreateProcessedImage(new GdiImage(SharedData.color_image));
        using var imageWithThumbnail = sourceImage.WithPostProcessingData(new PostProcessingData
        {
            Thumbnail = ImageContext.PerformTransform(ImageContext.Render(sourceImage), new ThumbnailTransform(256)),
            ThumbnailTransformState = TransformState.Empty
        }, true);

        var serializedImage = ImageSerializer.Serialize(imageWithThumbnail, new SerializeImageOptions
        {
            IncludeThumbnail = true
        });
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions());

        ImageAsserts.Similar(TransformTestsData.color_image_thumb_256, destImage.PostProcessingData.Thumbnail);
    }

    [Fact]
    public void IncludeThumbnail_InvalidTransformState()
    {
        using var destContext = new ScanningContext(new GdiImageContext());

        using var sourceImage = ScanningContext.CreateProcessedImage(new GdiImage(SharedData.color_image));
        using var imageWithThumbnail = sourceImage.WithPostProcessingData(new PostProcessingData
        {
            Thumbnail = ImageContext.PerformTransform(ImageContext.Render(sourceImage), new ThumbnailTransform(256)),
            ThumbnailTransformState = TransformState.Empty.AddOrSimplify(new BrightnessTransform(100))
        }, true);

        var serializedImage = ImageSerializer.Serialize(imageWithThumbnail, new SerializeImageOptions
        {
            IncludeThumbnail = true
        });
        using var destImage = ImageSerializer.Deserialize(destContext, serializedImage, new DeserializeImageOptions());

        Assert.Null(destImage.PostProcessingData.Thumbnail);
    }
}