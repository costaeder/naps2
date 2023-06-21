namespace NAPS2.EtoForms.Desktop;

public class DesktopImagesController
{
    private readonly UiImageList _imageList;

    public DesktopImagesController(UiImageList imageList)
    {
        _imageList = imageList;
    }

    /// <summary>
    /// Constructs a receiver for scanned images.
    /// This keeps images from the same source together, even if multiple sources are providing images at the same time.
    /// </summary>
    /// <returns></returns>
    public Action<ProcessedImage> ReceiveScannedImage()
    {
        var lockObj = new object();
        UiImage? last = null;
        return scannedImage =>
        {
            lock (lockObj)
            {
                var uiImage = new UiImage(scannedImage);
                _imageList.Mutate(new ImageListMutation.InsertAfter(uiImage, last), isPassiveInteraction: true);
                last = uiImage;
            }
        };
    }

    public Action<ProcessedImage> ReceiveScannedImageInsert()
    {
        var lockObj = new object();

        var selection = this._imageList.Selection.First();

        var index = this._imageList.Images.IndexOf(selection);
        
        
        return scannedImage =>
        {
            lock (lockObj)
            {
                var uiImage = new UiImage(scannedImage);
                _imageList.Mutate(new ImageListMutation.InsertAt(index,uiImage), isPassiveInteraction: true);
                
            }
        };
    }

    public Action<ProcessedImage> ReceiveScannedImageReplace()
    {
        var lockObj = new object();

        var selection = this._imageList.Selection.First();

        var index = this._imageList.Images.IndexOf(selection);
               
        return scannedImage =>
        {
            lock (lockObj)
            {
                var uiImage = new UiImage(scannedImage);
                this._imageList.Mutate(new ImageListMutation.InsertAt(index, uiImage), isPassiveInteraction: true);
                this._imageList.Mutate(new ImageListMutation.DeleteSelected());

            }
        };
    }
}