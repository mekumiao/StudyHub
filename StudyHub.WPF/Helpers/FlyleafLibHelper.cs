using FlyleafLib;

namespace StudyHub.WPF.Helpers;

public class FlyleafLibHelper {
    public static void EngineStart() {
        Engine.Start(new EngineConfig() {
#if DEBUG
            LogOutput = ":debug",
            LogLevel = LogLevel.Debug,
            FFmpegLogLevel = FFmpegLogLevel.Warning,
#endif
            PluginsPath = ":Plugins",
            FFmpegPath = ":FFmpeg",

            UIRefresh = true,
            UIRefreshInterval = 100,
            UICurTimePerSecond = false
        });
    }
}
