﻿using NAPS2.EtoForms;
using NAPS2.EtoForms.Desktop;
using NAPS2.EtoForms.Mac;
using NAPS2.EtoForms.Ui;
using NAPS2.Images.Mac;
using NAPS2.ImportExport;
using NAPS2.ImportExport.Pdf;
using NAPS2.Scan;
using NAPS2.Update;
using Ninject;
using Ninject.Modules;

namespace NAPS2.Modules;

public class MacModule : NinjectModule
{
    public override void Load()
    {
        // Bind<IBatchScanPerformer>().To<BatchScanPerformer>();
        Bind<IPdfPasswordProvider>().To<StubPdfPasswordProvider>();
        Bind<ErrorOutput>().To<MessageBoxErrorOutput>();
        Bind<IOverwritePrompt>().To<EtoOverwritePrompt>();
        Bind<OperationProgress>().To<EtoOperationProgress>().InSingletonScope();
        Bind<DialogHelper>().To<EtoDialogHelper>();
        Bind<IDevicePrompt>().To<EtoDevicePrompt>();
        Bind<INotificationManager>().To<StubNotificationManager>().InSingletonScope();
        Bind<ISaveNotify>().ToMethod(ctx => ctx.Kernel.Get<INotificationManager>());
        Bind<IScannedImagePrinter>().To<StubScannedImagePrinter>();
        Bind<DesktopController>().ToSelf().InSingletonScope();
        Bind<IDesktopScanController>().To<DesktopScanController>();
        Bind<IUpdateChecker>().To<UpdateChecker>();
        Bind<IExportController>().To<ExportController>();
        Bind<IDesktopSubFormController>().To<StubDesktopSubFormController>();
        Bind<DesktopFormProvider>().ToSelf().InSingletonScope();
        Bind<ImageContext>().To<MacImageContext>();
        Bind<MacImageContext>().ToSelf();

        Bind<DesktopForm>().To<MacDesktopForm>();

        EtoPlatform.Current = new MacEtoPlatform();
        // Log.EventLogger = new WindowsEventLogger(Kernel!.Get<Naps2Config>());
    }
}

public class StubDesktopSubFormController : IDesktopSubFormController
{
    private IFormFactory _formFactory;
    private DesktopImagesController _desktopImagesController;

    public StubDesktopSubFormController(IFormFactory formFactory, DesktopImagesController desktopImagesController)
    {
        _formFactory = formFactory;
        _desktopImagesController = desktopImagesController;
    }

    public void ShowCropForm()
    {
    }

    public void ShowBrightnessContrastForm()
    {
    }

    public void ShowHueSaturationForm()
    {
    }

    public void ShowBlackWhiteForm()
    {
    }

    public void ShowSharpenForm()
    {
    }

    public void ShowRotateForm()
    {
    }

    public void ShowProfilesForm()
    {
        var form = _formFactory.Create<ProfilesForm>();
        form.ImageCallback = _desktopImagesController.ReceiveScannedImage();
        form.ShowModal();
    }

    public void ShowOcrForm()
    {
        _formFactory.Create<OcrSetupForm>().ShowModal();
    }

    public void ShowBatchScanForm()
    {
    }

    public void ShowViewerForm()
    {
    }

    public void ShowPdfSettingsForm()
    {
    }

    public void ShowImageSettingsForm()
    {
    }

    public void ShowEmailSettingsForm()
    {
    }

    public void ShowAboutForm()
    {
        _formFactory.Create<AboutForm>().ShowModal();
    }

    public void ShowSettingsForm()
    {
    }
}

public class StubScannedImagePrinter : IScannedImagePrinter
{
    public Task<bool> PromptToPrint(IList<ProcessedImage> images, IList<ProcessedImage> selectedImages)
    {
        return Task.FromResult(false);
    }
}

public class StubNotificationManager : INotificationManager
{
    public void PdfSaved(string path)
    {
    }

    public void ImagesSaved(int imageCount, string path)
    {
    }

    public void DonatePrompt()
    {
    }

    public void OperationProgress(OperationProgress opModalProgress, IOperation op)
    {
    }

    public void UpdateAvailable(IUpdateChecker updateChecker, UpdateInfo update)
    {
    }

    public void Rebuild()
    {
    }
}

public class StubPdfPasswordProvider : IPdfPasswordProvider
{
    public bool ProvidePassword(string fileName, int attemptCount, out string password)
    {
        password = null!;
        return false;
    }
}