using System.ComponentModel;

using McpCounting.Resources;

using ModelContextProtocol.Server;

namespace McpCounting.Tools;

[McpServerToolType]
public class DataTool(IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("CustomClient");

    [McpServerTool, Description("根据 api 地址、token、门店信息、时间范围、时间单位获取门店的客流信息，包括来店人数、店外人数和进店率")]
    public async Task<string> GetWidget(
        [Description("api 地址")] string api,
        [Description("token")] string token,
        [Description("需要获取客流数据的门店信息")] List<Store> stores,
        [Description("客流数据的时间范围，包括开始日期和结束日期，格式为YYYY/MM/DD")] List<string> date,
        [Description("客流数据的时间单位，包括小时-hh，日-dd，周-ww，月-mm，年-yyyy")] string unit
    )
    {
        return "";
    }
}
