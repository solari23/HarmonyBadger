using System.Security.Cryptography;
using System.Text.Json;

using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using HarmonyBadger.ConfigModels;

namespace HarmonyBadger;

/// <summary>
/// Interface for a utility that loads configurations related to scheduled tasks.
/// </summary>
public interface IConfigProvider
{
    /// <summary>
    /// Gets all <see cref="ScheduledTask"/> configurations.
    /// </summary>
    /// <param name="logger">A helper used for logging telemetry.</param>
    /// <returns>The loaded <see cref="ScheduledTask"/> configurations.</returns>
    Task<IReadOnlyCollection<ScheduledTask>> GetScheduledTasksAsync(ILogger logger);
}

/// <summary>
/// Loads configurations related to scheduled tasks from the directory specified
/// by <see cref="Constants.TaskConfigsDirectoryName"/>.
/// </summary>
public class ConfigProvider : IConfigProvider
{
    /// <summary>
    /// Creates a new instance of the <see cref="ConfigProvider"/> class.
    /// </summary>
    /// <param name="azureFunctionContext">DI-injected information about the Azure Function context.</param>
    public ConfigProvider(IOptions<ExecutionContextOptions> azureFunctionContext)
    {
        this.ConfigDirectoryPath = Path.Combine(
            azureFunctionContext.Value.AppDirectory,
            Constants.TaskConfigsDirectoryName);
    }

    private string ConfigDirectoryPath { get; }

    private readonly SemaphoreSlim loadLock = new SemaphoreSlim(1);

    private List<ScheduledTask> ScheduledTasks { get; set; }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<ScheduledTask>> GetScheduledTasksAsync(ILogger logger)
    {
        if (this.ScheduledTasks is null)
        {
            try
            {
                loadLock.Wait();
                if (this.ScheduledTasks is null)
                {
                    this.ScheduledTasks = await LoadScheduledTasksFromDiskAsync(this.ConfigDirectoryPath, logger);
                }
            }
            finally
            {
                loadLock.Release();
            }
        }

        return this.ScheduledTasks;
    }

    private static async Task<List<ScheduledTask>> LoadScheduledTasksFromDiskAsync(string configDirectoryPath, ILogger logger)
    {
        using var hasher = SHA256.Create();

        var configFilePaths = Directory.EnumerateFiles(
            configDirectoryPath,
            $"*{Constants.ScheduledTaskConfigFileExtension}",
            new EnumerationOptions
            {
                MatchCasing = MatchCasing.CaseInsensitive
            });

        var loadFailures = new List<(string file, Exception e)>();
        var loadedTasks = new List<ScheduledTask>();

        foreach (var configFilePath in configFilePaths)
        {
            try
            {
                using var fileStream = File.OpenRead(configFilePath);
                var task = await JsonSerializer.DeserializeAsync<ScheduledTask>(
                    fileStream,
                    Constants.DefaultJsonSerializerOptions);

                // Reset the stream and use it again to calculate the file's checksum.
                fileStream.Seek(0, SeekOrigin.Begin);
                task.Checksum = await GetSha256ChecksumAsync(hasher, fileStream);

                task.ConfigFileName = Path.GetFileName(configFilePath);

                loadedTasks.Add(task);
            }
            catch (Exception e)
            {
                loadFailures.Add((Path.GetFileName(configFilePath), e));
            }
        }

        if (loadFailures.Any())
        {
            var failures = string.Join(
                Environment.NewLine,
                loadFailures.Select(f => $"{f.file}: {f.e}"));
            logger.LogError($"Failed to load one or more ScheduledTask config files:{Environment.NewLine}{failures}");
            logger.LogMetric(Constants.MetricNames.LoadScheduleConfigFailed, loadFailures.Count);
        }

        return loadedTasks;
    }

    private static async Task<string> GetSha256ChecksumAsync(SHA256 hasher, Stream dataStream)
    {
        hasher.Initialize();
        var hashBytes = await hasher.ComputeHashAsync(dataStream);
        return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
    }
}
