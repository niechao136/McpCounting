using ModelContextProtocol.Server;

using Newtonsoft.Json;

namespace McpCounting.Resources;

public class Device
{
    public string id { get; set; }
    public string name { get; set; }
    public string type { get; set; }
}

public class Sensor
{
    public string console_name { get; set; }
    public string device_id { get; set; }
    public string device_type { get; set; }
    public string sensor_id { get; set; }
    public string sensor_name { get; set; }
    public string sensor_type { get; set; }
}

public class Store
{
    public string address { get; set; }
    public string city { get; set; }
    public string country { get; set; }
    public List<Device> devices { get; set; }
    public string province { get; set; }
    public string register_key { get; set; }
    public List<Sensor> sensors { get; set; }
    public string store_id { get; set; }
    public string store_name { get; set; }
}

public class StoreList
{
    public List<Store> stores { get; set; }
}

[McpServerResourceType]
public class StoreResource
{
    public List<Store> stores { get; set; } = [];
}

[McpServerResourceType]
public class StoreResourceProvider
{
    private static StoreResource _data = new();

    public static void SetStoreData(List<Store> stores) => _data = new StoreResource { stores = stores };

    [McpServerResource(Name = "StoreList", Title = "门店（地点）列表资源", MimeType = "application/json")]
    public string GetStoreList() => JsonConvert.SerializeObject(_data.stores);
}
