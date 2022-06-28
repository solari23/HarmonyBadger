using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

using HarmonyBadgerFunctionApp.TaskModel;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace HarmonyBadgerFunctionApp.Scheduler;

/// <summary>
/// Interface for a utility that loads <see cref="ScheduledTask"/> configurations.
/// </summary>
public interface IScheduledTaskConfigLoader
{
    /// <summary>
    /// Loads <see cref="ScheduledTask"/> configurations.
    /// </summary>
    /// <param name="logger">Helper used for logging telemetry.</param>
    /// <param name="azureFunctionContext">The azure function execution context.</param>
    /// <returns>The loaded <see cref="ScheduledTask"/> configurations.</returns>
    Task<IReadOnlyCollection<ScheduledTask>> LoadScheduledTasksAsync(
        ILogger logger,
        ExecutionContext azureFunctionContext);
}

/// <summary>
/// Loads <see cref="ScheduledTask"/> configurations from the directory
/// specified by <see cref="Constants.TaskConfigsDirectoryName"/>.
/// </summary>
public class ScheduledTaskConfigLoader : IScheduledTaskConfigLoader
{
    /// <inheritdoc />
    public async Task<IReadOnlyCollection<ScheduledTask>> LoadScheduledTasksAsync(
        ILogger logger,
        ExecutionContext azureFunctionContext)
    {
        var configDirectoryPath = GetTaskConfigDirectoryPath(azureFunctionContext);
        var configFilePaths = Directory.EnumerateFiles(
            configDirectoryPath,
            $"*{Constants.ScheduledTaskConfigFileExtension}",
            new EnumerationOptions
            {
                MatchCasing = MatchCasing.CaseInsensitive
            });

        var loadFailures = new List<(string file, Exception e)>();
        var loadedTasks = new List<ScheduledTask>();

        foreach (var configFile in configFilePaths)
        {
            try
            {
                using var fileStream = File.OpenRead(configFile);
                var task = await JsonSerializer.DeserializeAsync<ScheduledTask>(fileStream);

                // Reset the stream and use it again to calculate the file's checksum.
                fileStream.Seek(0, SeekOrigin.Begin);
                task.Checksum = await GetSha256ChecksumAsync(fileStream);

                loadedTasks.Add(task);
            }
            catch (Exception e)
            {
                loadFailures.Add((configFile, e));
            }
        }

        if (loadFailures.Any())
        {
            var failures = string.Join(
                Environment.NewLine,
                loadFailures.Select(f => $"{f.file}: {f.e}"));
            logger.LogError($"Failed to load one or more ScheduledTask config files:{Environment.NewLine}{failures}");
        }

        return loadedTasks.AsReadOnly();
    }

    private static string GetTaskConfigDirectoryPath(ExecutionContext context)
        => Path.Combine(
            context.FunctionAppDirectory,
            Constants.TaskConfigsDirectoryName);

    private static async Task<string> GetSha256ChecksumAsync(Stream dataStream)
    {
        using var hasher = SHA256.Create();
        var hashBytes = await hasher.ComputeHashAsync(dataStream);
        return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
    }
}
