using System.Security.Cryptography;
using System.Text.Json;

using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using HarmonyBadger.ConfigModels;
using HarmonyBadger.ConfigModels.Discord;

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

    /// <summary>
    /// Gets a named <see cref="DiscordRecipient"/> from config, if one exists.
    /// </summary>
    /// <param name="name">The name of the named recipient (not case sensitive).</param>
    /// <param name="logger">A helper used for logging telemetry.</param>
    /// <returns>The named <see cref="DiscordRecipient"/> if found, otherwise null.</returns>
    Task<DiscordRecipient> GetNamedDiscordRecipientAsync(string name, ILogger logger);
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

    private List<ScheduledTask> LoadedScheduledTasks { get; set; }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<ScheduledTask>> GetScheduledTasksAsync(ILogger logger)
    {
        if (this.LoadedScheduledTasks is null)
        {
            try
            {
                loadLock.Wait();
                if (this.LoadedScheduledTasks is null)
                {
                    this.LoadedScheduledTasks = await LoadScheduledTasksFromDiskAsync(this.ConfigDirectoryPath, logger);
                }
            }
            finally
            {
                loadLock.Release();
            }
        }

        return this.LoadedScheduledTasks;
    }

    private IReadOnlyDictionary<string, DiscordRecipient> LoadedNamedRecipients { get; set; }

    /// <inheritdoc />
    public async Task<DiscordRecipient> GetNamedDiscordRecipientAsync(string name, ILogger logger)
    {
        if (this.LoadedNamedRecipients is null)
        {
            try
            {
                loadLock.Wait();
                if (this.LoadedNamedRecipients is null)
                {
                    this.LoadedNamedRecipients = await LoadDiscordRecipientsFromDisk(this.ConfigDirectoryPath, logger);
                }
            }
            finally
            {
                loadLock.Release();
            }
        }

        return this.LoadedNamedRecipients.TryGetValue(name, out DiscordRecipient recipient)
            ? recipient
            : null;
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

        var loaded = new List<ScheduledTask>();

        var loadFailures = new List<(string file, Exception e)>();

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

                loaded.Add(task);
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

        return loaded;
    }

    private static async Task<Dictionary<string, DiscordRecipient>> LoadDiscordRecipientsFromDisk(
        string configDirectoryPath,
        ILogger logger)
    {
        var configFilePaths = Directory.EnumerateFiles(
            configDirectoryPath,
            $"*{Constants.DiscordRecipientConfigFileExtension}",
            new EnumerationOptions
            {
                MatchCasing = MatchCasing.CaseInsensitive
            });

        var loaded = new Dictionary<string, DiscordRecipient>(StringComparer.OrdinalIgnoreCase);

        var loadFailures = new List<(string file, Exception e)>();

        foreach (var configFilePath in configFilePaths)
        {
            try
            {
                using var fileStream = File.OpenRead(configFilePath);
                var recipientsInFile = await JsonSerializer.DeserializeAsync<Dictionary<string, DiscordRecipient>>(
                    fileStream,
                    Constants.DefaultJsonSerializerOptions);

                var configFileName = Path.GetFileName(configFilePath);

                foreach (var recipient in recipientsInFile)
                {
                    if (loaded.TryGetValue(recipient.Key, out var previouslyLoadedRecipient))
                    {
                        logger.LogWarning(
                            $"Ignoring duplicate definition for named recipient '{recipient.Key}' found while parsing file {configFileName}. Previously found in file '{previouslyLoadedRecipient.ConfigFileName}'");
                        continue;
                    }

                    recipient.Value.ConfigFileName = configFileName;
                    loaded.Add(recipient.Key, recipient.Value);
                }
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
            logger.LogError($"Failed to load one or more DiscordRecipient config files:{Environment.NewLine}{failures}");
            logger.LogMetric(Constants.MetricNames.LoadScheduleConfigFailed, loadFailures.Count);
        }

        return loaded;
    }

    private static async Task<string> GetSha256ChecksumAsync(SHA256 hasher, Stream dataStream)
    {
        hasher.Initialize();
        var hashBytes = await hasher.ComputeHashAsync(dataStream);
        return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
    }
}
