using System.Text.Json;
using System.Text.Json.Serialization;
using Azure;
using Azure.Data.Tables;

public class StorageService
{
    //private readonly HttpClient httpClient;
    private readonly TableClient _indexTable;
    private readonly TableClient _dataTable;

    public StorageService( TableServiceClient tableServiceClient)
    {
        //can be use to query a specific endpoint
        //httpClient = httpClientFactory.CreateClient();
        _indexTable = tableServiceClient.GetTableClient("MonitoringIndex");
        _dataTable = tableServiceClient.GetTableClient("MonitoringTable");
        
    }
    
    public async Task<string> GetHelp(string businessId)
    {
        if (string.IsNullOrWhiteSpace(businessId))
        {
            return JsonSerializer.Serialize(new{
                ErrorMessage = "BusinessId is empty"
            });
        }
        if (businessId == "1234")
        {
            return JsonSerializer.Serialize(new
            {
                businessId = "1234",
                detail = "This is a test businessId",
            });
        }
        try
        {
            var businessModels = await this.GetStorageDataItems(businessId);
            return JsonSerializer.Serialize(businessModels, StorageContext.Default.ListMonitoringModel);
        }
        catch (Exception e)
        {
            return JsonSerializer.Serialize(new
            {
                ErrorMessage = e.Message
            });
        }
    }
    
    public async Task<List<MonitoringModel>> GetStorageDataItems(string businessId, string flowId = "")
    {
        try
        {
            //query table storage
            var indexes = new List<StorageIndexModel>();
            var tempResults = new List<MonitoringModel>();
            var indexQuery = _indexTable.QueryAsync<StorageIndexModel>(entity => entity.PartitionKey == businessId);
            await foreach (var index in indexQuery)
            {
                indexes.Add(index);
            }

            foreach (var index in indexes)
            {
                var query = _dataTable.QueryAsync<StorageDataItem>(entity => entity.PartitionKey == index.RowKey);
                await foreach (var entity in query)
                {
                    var m = ToMonitoringModel(entity);
                    tempResults.Add(m);
                }
            }
            return tempResults;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            throw;
        }
        //return await Task.FromResult<List<MonitoringModel>>([]);
    }

    private MonitoringModel ToMonitoringModel(StorageDataItem? entity)
    {
        var result = new MonitoringModel();
        if (entity != null && !string.IsNullOrWhiteSpace((entity.TopicTimestampsJson)))
        {
            result.BusinessId = entity.BusinessId;
            result.FlowId = entity.BusinessId?.Split('|').FirstOrDefault() ?? string.Empty;
            var topicTimestamps = JsonSerializer.Deserialize<Dictionary<string, DateTimeOffset?>>(entity.TopicTimestampsJson);
            var details = new List<MonitoringModelDetail>(); 
            foreach (var topicTimestamp in topicTimestamps)
            {
                details.Add(new MonitoringModelDetail()
                {
                    ViewTopic = topicTimestamp.Key,
                    ViewTime = topicTimestamp.Value
                });
            }
            result.Details = details.ToArray();
        }
        return result;
    }

    public async Task<MonitoringModel?> GetStorageDataItem(string businessId)
    {
        var entity = await _dataTable.GetEntityAsync<StorageDataItem>( businessId, businessId);

        return entity is { HasValue: true } ? ToMonitoringModel(entity.Value) : MonitoringModel.Null;
    }

    public async Task<List<MonitoringModel>> GetBusinessIdsSince(DateTimeOffset since)
    {
        try
        {
            var tempResults = new List<MonitoringModel>();
            var query = _dataTable.QueryAsync<StorageDataItem>(entity => entity.Timestamp >= since);
            int count = 0;
            await foreach (var item in query)
            {
                tempResults.Add(ToMonitoringModel(item));
                count++;
                if (count > 500) break;
            }

            return tempResults;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            throw;
        }
    }
}

internal class StorageIndexModel : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}

internal class StorageDataItem : ITableEntity
{
    public string BusinessId { get; set; }

    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string TopicTimestampsJson { get; set; }
}

public partial class MonitoringModel
{
    public string BusinessId { get; set; }
    public string FlowId { get; set; }
    public string? ErrorMessage { get; set; }

    public MonitoringModelDetail[] Details { get; set; } = [];
    
    public static readonly MonitoringModel Null = new MonitoringModel() { BusinessId = "", FlowId = "", ErrorMessage = "" };
}

public class MonitoringModelDetail
{
    public string ViewTopic { get; set; }
    public DateTimeOffset? ViewTime { get; set; }
}

[JsonSerializable(typeof(List<MonitoringModel>))]
[JsonSerializable(typeof(MonitoringModel))]
[JsonSerializable(typeof(Dictionary<string, DateTimeOffset>))]
[JsonSerializable(typeof(List<string>))]
internal sealed partial class StorageContext : JsonSerializerContext {

}