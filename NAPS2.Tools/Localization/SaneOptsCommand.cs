using System.Text.RegularExpressions;

namespace NAPS2.Tools.Localization;

public static class SaneOptsCommand
{
    private static readonly string[] NeededStrings =
    {
        "Color",
        "Gray",
        "Lineart",
        "ADF",
        "adf",
        "ADF Front",
        "Automatic Document Feeder",
        "ADF Duplex",
        "Duplex",
        "Flatbed",
        "FB",
        "fb"
    };

    public static int Run(SaneOptsOptions opts)
    {
        var dirPath = Path.Combine(Paths.SolutionRoot, "..", "naps2-sane", "sources", "sane-backends", "po");
        var dir = new DirectoryInfo(dirPath);
        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Couldn't find SANE sources: {dir.FullName}");
        }

        var translations = NeededStrings.ToDictionary(_ => _, _ => new HashSet<string>() { _ });

        foreach (var poFile in dir.EnumerateFiles("*.po"))
        {
            var ctx = new LanguageContext(poFile.Name.Replace(".po", ""));
            ctx.Load(poFile.FullName);
            foreach (var s in NeededStrings)
            {
                if (ctx.Strings.ContainsKey(s))
                {
                    var ts = ctx.Strings[s].Translation;
                    if (!string.IsNullOrWhiteSpace(ts))
                    {
                        translations[s].Add(ts);
                    }
                }
            }
        }

        var fields = NeededStrings.Select(x =>
        {
            var variable = Regex.Replace(x, @"\s", "_");
            var values = translations[x].OrderBy(_ => _).Select(y => $"\"{y}\"").ToArray();
            return $$"""
                    public static readonly string[] {{ variable }} = {
                        {{ string.Join(",\n        ", values) }}
                    };
                """ ;
        });

        var fileContents = $$"""
            namespace NAPS2.Scan.Internal.Sane;

            /// <summary>
            /// Translations of option values used for matching against SANE, which doesn't provide non-localized
            /// option values.
            ///
            /// This isn't perfect as it doesn't fully account for historical/future translations or third-party
            /// backends, but it's the best we can do for now.
            /// </summary>
            //
            // Auto-generated by {{ typeof(SaneOptsCommand).FullName }}.
            // Run "n2 saneopts" to update.
            internal static class SaneOptionTranslations {
            {{ string.Join("\n\n", fields) }}
            }
            """ ;

        var destPath =
            Path.Combine(Paths.SolutionRoot, "NAPS2.Sdk", "Scan", "Internal", "Sane", "SaneOptionTranslations.cs");
        File.WriteAllText(destPath, fileContents);

        return 0;
    }
}