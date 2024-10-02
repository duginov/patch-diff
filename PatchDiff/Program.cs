using DiffToolApp;
using DocoptNet;
using Microsoft.Extensions.DependencyInjection;
using Patch;
using Microsoft.Extensions.Options;

internal class Program {
    public static void Main(string[] args)
    {
        const string usage =
            """
            A patch file diffing tool (LightKeeper backend developer code test)

            Usage:
              patch_diff (-h | --help)
              patch_diff <patch_left> <patch_right> [-e]

            Options:
              -h --help                  Show this screen.
              <patch_left>               Path to a patch file.
              <patch_right>              Path to another patch file.
              -e --editor                Opens result in Notepad editor instead of console output

            """;

        try
        {
            var arguments = new Docopt().Apply(usage, args, version: "patch_diff 0.1", exit: false);
            // Normally IOptions<T> used for injecting into collaborators
            // here I used it slightly "off the label" to consolidate inputs
            // and reuse it for container configuration and passing file name to the App
            var options = AssertValidInputs(arguments);

            var serviceProvider = CreateContainer(options);
            var app = serviceProvider.GetRequiredService<App>();
            
            app.Run(options);
        }
        catch (Exception e)
        {
            // in real production code logging should be used instead of console output.
            // Log should include stack trace in addition to the message
            Console.WriteLine(e.Message);
            Environment.Exit(-1); // signal abnormal exit - useful if a caller cares about exit code
        }

    }

    private static ServiceProvider CreateContainer(IOptions<AppOptions> options)
    {
        var services = new ServiceCollection();
        // choice of service lifetimes is really inconsequential here since all of them are stateless
        // however I picked a single instance for the App since it will unlikely change
        // and Transient for others as they may receive some state during future modifications
        services.AddSingleton<App>();
        services.AddTransient<IPatchLoader, CsvPatchLoader>();
        services.AddTransient<IDiffDetector, DiffDetector>();
        if (options.Value.UseEditor)
        {
            services.AddTransient<ISummaryPresenter, TextEditorPresenter>();
        }
        else
        {
            services.AddTransient<ISummaryPresenter, ConsoleSummaryPresenter>();
        }

        return services.BuildServiceProvider();
    }

    // checks if both path file names were passed, and they exist, returns validated full paths and output optional parameter
    private static IOptions<AppOptions> AssertValidInputs(IDictionary<string, ValueObject> arguments)
    {
        if (arguments == null)
        {
            // faulty docopt configuration?
            throw new Exception("no argument list");
        }

        var useEditor = arguments["--editor"].IsTrue;

        // Pre-flight checks
        var pathLeft = arguments["<patch_left>"].ToString();
        var pathRight = arguments["<patch_right>"].ToString();
        if (string.IsNullOrWhiteSpace(pathLeft))
        {
            // faulty docopt configuration?
            throw new Exception($"Missing required parameter: {nameof(pathLeft)}");
        }

        if (string.IsNullOrWhiteSpace(pathRight))
        {
            // faulty docopt configuration?
            throw new Exception($"Missing required parameter: {nameof(pathRight)}");
        }

        var absPathLeft = Path.GetFullPath(pathLeft);
        var absPathRight = Path.GetFullPath(pathRight);

        if (!File.Exists(absPathLeft))
        {
            throw new FileNotFoundException($"<patch_left> file not found", absPathLeft);
        }

        if (!File.Exists(absPathRight))
        {
            throw new FileNotFoundException($"<patch_right> file not found", absPathRight);
        }

        return Options.Create(new AppOptions()
            { AbsolutePathLeft = absPathLeft, AbsolutePathRight = absPathRight, UseEditor = useEditor });
    }
}

