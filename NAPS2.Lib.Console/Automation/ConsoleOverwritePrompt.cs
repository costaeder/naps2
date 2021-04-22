﻿using System.Windows.Forms;
using NAPS2.Lang.ConsoleResources;
using NAPS2.Logging;
using NAPS2.Util;

namespace NAPS2.Automation
{
    public class ConsoleOverwritePrompt : OverwritePrompt
    {
        public static bool ForceOverwrite { get; set; }

        private readonly ErrorOutput _errorOutput;

        public ConsoleOverwritePrompt(ErrorOutput errorOutput)
        {
            _errorOutput = errorOutput;
        }

        public override DialogResult ConfirmOverwrite(string path)
        {
            if (ForceOverwrite)
            {
                return DialogResult.Yes;
            }
            else
            {
                _errorOutput.DisplayError(string.Format(ConsoleResources.FileAlreadyExists, path));
                return DialogResult.No;
            }
        }
    }
}
