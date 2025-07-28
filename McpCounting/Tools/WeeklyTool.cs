using System.ComponentModel;
using System.Net.Http.Headers;

using McpCounting.Resources;

using ModelContextProtocol.Server;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace McpCounting.Tools;

public class WeeklyTool(IHttpClientFactory httpClientFactory)
{

    private readonly HttpClient _client = httpClientFactory.CreateClient("CustomClient");

    [McpServerTool, Description("根据 app 地址、api 地址、token、门店信息、周报日期和周报语言生成周报对应的 URL")]
    public async Task<string> GetPdfUrl(
        [Description("app 地址")] string app,
        [Description("api 地址")] string api,
        [Description("token")] string token,
        [Description("周报对应门店的信息")] Store store,
        [Description("周报日期范围的周一日期，格式：YYYY-MM-DD")] string date,
        [Description("根据问题语言自动确定的周报语言，共有 zh-CN、zh-TW、en-US 这三种，非中文时统一为 en-US")] string lang
        )
    {
        var acc = $"{api.TrimEnd('/')}/api/account/info";
        var data = new JObject
        {
            new JProperty("token", JsonConvert.DeserializeObject(token))
        };
        HttpContent httpContent = new StringContent(data.ToString());
        httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        HttpResponseMessage response = await _client.PostAsync(acc, httpContent);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        JObject? res = JsonConvert.DeserializeObject<JObject>(content);
        var accId = res?["account"]?["id"]?.ToString() ?? "";
        var accName = res?["account"]?["name"]?.ToString() ?? "";
        var name = store.store_name ?? "";
        var id = store.store_id ?? "";
        var rk = store.register_key ?? "";

        return $"{app.TrimEnd('/')}/pdf?accId=${accId}&accName=${accName}&name=${name}&id=${id}&rk=${rk}&date=${date}&lang=${lang}";
    }
}
