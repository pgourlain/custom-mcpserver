


using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

[McpServerToolType]
public sealed class StorageAccountTools(StorageService saService)
{
    [McpServerTool, Description("Get a list of truc that has this part of businessId.")]
    public async Task<string> GetBusinessIds([Description("The part of businessId to looking for")] string businessId)
    {
        var businessModels = await saService.GetStorageDataItems(businessId);
        return JsonSerializer.Serialize(businessModels, StorageContext.Default.ListMonitoringModel);
    }

    [McpServerTool, Description("Get a list of truc for specific flow.")]
    public async Task<string> GetBusinessIdsByFlow([Description("The part of businessId to looking for")] string businessId, 
        [Description("The flow name of businessId to looking for")] string flowId)
    {
        var businessModels = await saService.GetStorageDataItems(businessId, flowId);
        return JsonSerializer.Serialize(businessModels, StorageContext.Default.ListMonitoringModel);
    }

    // [McpServerTool, Description("Get a truc with specific id.")]
    // public async Task<string> GetBusinessId([Description("The name of the businessId to get details for")] string businessId)
    // {
    //     var businessModel = await saService.GetStorageDataItem(businessId);
    //     return JsonSerializer.Serialize(businessModel, StorageContext.Default.MonitoringModel);
    // }
    
    [McpServerTool, Description("Get a list of businessId since a datetime.")]
    public async Task<string> GetBusinessIdsSince([Description("datetime to looking for")] DateTimeOffset startTime)
    {
        var businessIds = await saService.GetBusinessIdsSince(startTime);
        return JsonSerializer.Serialize(businessIds, StorageContext.Default.ListMonitoringModel);
    }
    
}