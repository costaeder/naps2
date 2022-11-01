﻿using Autofac;
using NAPS2.EtoForms;
using NAPS2.EtoForms.Ui;
using NAPS2.Modules;
using UnhandledExceptionEventArgs = Eto.UnhandledExceptionEventArgs;

namespace NAPS2.EntryPoints;

/// <summary>
/// The entry point logic for NAPS2.exe, the NAPS2 GUI.
/// </summary>
public static class GtkEntryPoint
{
    public static int Run(string[] args)
    {
        if (args.Length > 0 && args[0] is "cli" or "console")
        {
            return ConsoleEntryPoint.Run(args.Skip(1).ToArray(), new GtkModule(), false);
        }

        // Initialize Autofac (the DI framework)
        var container = AutoFacHelper.FromModules(
            new CommonModule(), new GtkModule(), new RecoveryModule(), new ContextModule());

        Paths.ClearTemp();

        // Set up basic application configuration
        container.Resolve<CultureHelper>().SetCulturesFromConfig();
        TaskScheduler.UnobservedTaskException += UnhandledTaskException;
        GLib.ExceptionManager.UnhandledException += UnhandledGtkException;
        Trace.Listeners.Add(new ConsoleTraceListener());

        // Show the main form
        var application = EtoPlatform.Current.CreateApplication();
        application.UnhandledException += UnhandledException;
        var formFactory = container.Resolve<IFormFactory>();
        var desktop = formFactory.Create<DesktopForm>();
        // TODO: Clean up invoker setting
        // Invoker.Current = new WinFormsInvoker(desktop.ToNative());
        application.Run(desktop);
        return 0;
    }

    private static void UnhandledGtkException(GLib.UnhandledExceptionArgs e)
    {
        if (e.IsTerminating)
        {
            Log.FatalException("An error occurred that caused the task to terminate.", e.ExceptionObject as Exception);
        }
        else
        {
            Log.ErrorException("An unhandled error occurred.", e.ExceptionObject as Exception);
        }
    }

    private static void UnhandledTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.FatalException("An error occurred that caused the task to terminate.", e.Exception);
        e.SetObserved();
    }

    private static void UnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        Log.FatalException("An error occurred that caused the application to close.", e.ExceptionObject as Exception);
    }
}