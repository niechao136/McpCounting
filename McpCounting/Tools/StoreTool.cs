using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http.Headers;

using McpCounting.Resources;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace McpCounting.Tools;


[McpServerToolType]
public class StoreTool(IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("CustomClient");

    [McpServerTool, Description("根据 api 地址、token 和用户 ID 获取门店（地点）列表信息")]
    public async Task<string> GetStore(
        [Description("api 地址")] string api,
        [Description("token")] string token,
        [Description("用户 ID")] string userId
        )
    {
        try
        {
            var url = $"{api.TrimEnd('/')}/api/store/list";
            var data = new JObject
            {
                new JProperty("token", JsonConvert.DeserializeObject(token)),
                new JProperty("user_id", userId)
            };
            HttpContent httpContent = new StringContent(data.ToString());
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await _client.PostAsync(url, httpContent);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            StoreList? res = JsonConvert.DeserializeObject<StoreList>(content);

            StoreResourceProvider.SetStoreData(res?.stores ?? []);
            return $"成功获取并保存 {res?.stores.Count ?? 0} 家门店";
        }
        catch (Exception ex)
        {
            return $"请求失败: {ex.Message}";
        }
    }
}
