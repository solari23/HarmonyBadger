using System.Text;
using System.Text.Encodings.Web;

namespace HarmonyBadger;

public class QueryStringBuilder
{
    private readonly StringBuilder queryBuilder = new StringBuilder();

    public void AddParameter(string name, string value, bool urlEncode = true)
    {
        if (queryBuilder.Length > 0)
        {
            queryBuilder.Append('&');
        }

        queryBuilder.Append(name);
        queryBuilder.Append('=');
        queryBuilder.Append(urlEncode ? UrlEncoder.Default.Encode(value) : value);
    }

    public string Build() => queryBuilder.ToString();
}
