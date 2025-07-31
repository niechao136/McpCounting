using System.ComponentModel;
using System.Net.Http.Headers;

using McpCounting.Resources;

using ModelContextProtocol.Server;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace McpCounting.Tools;

[McpServerToolType]
public class DataTool(IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("CustomClient");

    [McpServerTool, Description("根据 api 地址、token、门店信息、时间范围、时间单位获取门店的客流信息，包括来店人数、店外人数和进店率")]
    public async Task<string> GetWidget(
        [Description("api 地址")] string api,
        [Description("token，格式为 {\"token_id\":\"xxx\",\"token\":\"yyy\"} 的 JSON 字符串")] string token,
        [Description("需要获取客流数据的门店信息")] List<Store> stores,
        [Description("客流数据的时间范围，包括开始日期和结束日期，格式为YYYY/MM/DD")] List<string> date,
        [Description("客流数据的时间单位，包括小时-hh，日-dd，周-ww，月-mm，年-yyyy")] string unit
    )
    {
        var url = $"{api.TrimEnd('/')}/api/widget/data";
        JObject[] sources = stores.Select(store => new JObject
        {
            new JProperty("target_id", store.store_id)
        }).ToArray();
        var data = new JObject
        {
            new JProperty("data_source", new JObject
            {
                new JProperty("analytic", new JArray(new JObject
                {
                    new JProperty("caption", ""),
                    new JProperty("method", "convert_rate"),
                    new JProperty("preprocess_data", new JArray("pin", "crosscnt"))
                })),
                new JProperty("data_range", "any"),
                new JProperty("data_unit", unit),
                new JProperty("date", new JArray(date[0], date[1])),
                new JProperty("date_display", "specific"),
                new JProperty("date_end", ""),
                new JProperty("folding_unit", ""),
                new JProperty("is_aggregate", false),
                new JProperty("row_type", "chart"),
                new JProperty("source", new JArray
                {
                    new JObject
                    {
                        new JProperty("caption", ""),
                        new JProperty("chart_type", new JArray("line")),
                        new JProperty("merge_type", "none"),
                        new JProperty("monitor_type", "ppc_store_entry"),
                        new JProperty("preprocess_data", new JArray("pin")),
                        new JProperty("sources", JArray.FromObject(sources)),
                    },
                    new JObject
                    {
                        new JProperty("caption", ""),
                        new JProperty("chart_type", new JArray("line")),
                        new JProperty("merge_type", "none"),
                        new JProperty("monitor_type", "ppc_store_outside"),
                        new JProperty("preprocess_data", new JArray("crosscnt")),
                        new JProperty("sources", JArray.FromObject(sources)),
                    },
                }),
                new JProperty("tags", new JArray()),
                new JProperty("time_compare", ""),
            }),
            new JProperty("module_id", 0),
            new JProperty("token", JsonConvert.DeserializeObject(token)),
        };
        HttpContent httpContent = new StringContent(data.ToString());
        httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        HttpResponseMessage response = await _client.PostAsync(url, httpContent);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        JObject? res = JsonConvert.DeserializeObject<JObject>(content);

        List<string> label = res?["label"]?.ToObject<List<string>>() ?? [];
        var pin = new Dictionary<string, List<float>>();
        var cross = new Dictionary<string, List<float>>();
        var rate = new Dictionary<string, List<float>>();
        List<JObject> pinData = res?["retrived"]?[0]?["data"]?.ToObject<List<JObject>>() ?? [];
        List<JObject> crossData = res?["retrived"]?[1]?["data"]?.ToObject<List<JObject>>() ?? [];
        List<JObject> rateData = res?["analytic"]?[0]?["data"]?.ToObject<List<JObject>>() ?? [];

        foreach (JObject item in pinData)
        {
            List<float> value = item["pin"]?["row"]?.ToObject<List<float>>() ?? [];
            var id = item["target_id"]?.ToString() ?? "";
            pin.Add(id, value);
        }
        foreach (JObject item in crossData)
        {
            List<float> value = item["crosscnt"]?["row"]?.ToObject<List<float>>() ?? [];
            var id = item["target_id"]?.ToString() ?? "";
            cross.Add(id, value);
        }
        foreach (JObject item in rateData)
        {
            List<float> value = item["pin"]?["row"]?.ToObject<List<float>>() ?? [];
            var id = item["target_id"]?.ToString() ?? "";
            rate.Add(id, value);
        }

        JObject[] storeData = stores.Select(store => new JObject
        {
            new JProperty("store_id", store.store_id),
            new JProperty("store_name", store.store_name),
            new JProperty("register_key", store.register_key),
            new JProperty("traffic", JArray.FromObject(pin[store.store_id ?? ""])),
            new JProperty("outside", JArray.FromObject(cross[store.store_id ?? ""])),
            new JProperty("turn_in_rate", JArray.FromObject(rate[store.store_id ?? ""])),
        }).ToArray();

        var result = new JObject
        {
            new JProperty("label", JArray.FromObject(label)),
            new JProperty("store_data", JArray.FromObject(storeData)),
        };

        return JsonConvert.SerializeObject(result);
    }
}
