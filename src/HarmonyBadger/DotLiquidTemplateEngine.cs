using DotLiquid;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using HarmonyBadger.ConfigModels;

namespace HarmonyBadger;

public interface ITemplateEngine
{
    /// <summary>
    /// Renders a the given string template using the variables provided.
    /// </summary>
    /// <param name="template">The templated string to render.</param>
    /// <param name="variables">The variables to use when rendering.</param>
    /// <returns>The rendered template.</returns>
    Task<string> RenderFromTemplateStringAsync(string template, Dictionary<string, object> variables);

    /// <summary>
    /// Loads a template file from disk and renders it using the variables provided.
    /// </summary>
    /// <param name="filePath">
    /// The path to the template.
    /// This method will search for this path starting from the directory defined in <see cref="Constants.TemplateFilesDirectoryName"/>.
    /// </param>
    /// <param name="variables">The variables to use when rendering.</param>
    /// <returns>The rendered template.</returns>
    Task<string> RenderFromTemplateFileAsync(string filePath, Dictionary<string, object> variables);

    /// <summary>
    /// Renders the message from the given <see cref="ITemplatedMessage"/>.
    /// </summary>
    /// <param name="templatedMessage">The message to render.</param>
    /// <returns>The rendered message.</returns>
    Task<string> RenderTemplatedMessageAsync(ITemplatedMessage templatedMessage);
}

/// <summary>
/// A helper to populate Liquid templates using the DotLiquid library.
/// 
/// For DotLiquid library documentation, see:
/// https://github.com/dotliquid/dotliquid
/// 
/// For Liquid template syntax, see:
/// https://shopify.github.io/liquid/
/// </summary>
public class DotLiquidTemplateEngine : ITemplateEngine
{
    /// <summary>
    /// Template variable automatically supplied by Harmony Badger that represents the current time (in the configured local time).
    /// </summary>
    public const string NowLocalVariableName = "HB_NowLocal";

    public DotLiquidTemplateEngine(IMemoryCache cache, IOptions<ExecutionContextOptions> azureFunctionContext, IClock clock)
    {
        this.Cache = cache;
        this.Clock = clock;
        this.TemplateFileDirectoryPath = Path.Combine(
            azureFunctionContext.Value.AppDirectory,
            Constants.TemplateFilesDirectoryName);
    }

    private IMemoryCache Cache { get; }

    private IClock Clock { get; }

    private string TemplateFileDirectoryPath { get; }

    /// <inheritdoc />
    public Task<string> RenderFromTemplateStringAsync(string template, Dictionary<string, object> variables)
    {
        var parsedTemplate = Template.Parse(template);
        var renderedTemplate = this.Render(parsedTemplate, variables);
        return Task.FromResult(renderedTemplate);
    }

    /// <inheritdoc />
    public async Task<string> RenderFromTemplateFileAsync(string filePath, Dictionary<string, object> variables)
    {
        const string TemplateCacheKeyPrefix = $"TMPL$";

        var template = await this.Cache.GetOrCreateAsync(
            $"{TemplateCacheKeyPrefix}{filePath}",
            async cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

                var fullTemplatePath = Path.Combine(
                    this.TemplateFileDirectoryPath,
                    (cacheEntry.Key as string)[TemplateCacheKeyPrefix.Length..]);

                var templateFileContents = await File.ReadAllTextAsync(fullTemplatePath);
                return Template.Parse(templateFileContents);
            });
        var renderedTemplate = this.Render(template, variables);
        return renderedTemplate;
    }

    /// <inheritdoc />
    public async Task<string> RenderTemplatedMessageAsync(ITemplatedMessage templatedMessage)
    {
        var engineParameters = new Dictionary<string, object>();

        if (templatedMessage.TemplateParameters is not null)
        {
            foreach (var kvp in templatedMessage.TemplateParameters)
            {
                engineParameters.Add(kvp.Key, kvp.Value);
            }
        }

        if (templatedMessage.TemplateFilePath is not null)
        {
            return await this.RenderFromTemplateFileAsync(templatedMessage.TemplateFilePath, engineParameters);
        }
        else
        {
            return await this.RenderFromTemplateStringAsync(templatedMessage.Message, engineParameters);
        }
    }

    private string Render(Template template, Dictionary<string, object> variables)
    {
        variables.Add(NowLocalVariableName, this.Clock.LocalNow);

        var hashedVariables = Hash.FromDictionary(variables);
        return template.Render(hashedVariables);
    }
}
