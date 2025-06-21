using MongoDB.Driver;
using Zentry.Modules.Attendance.Domain.Entities;

namespace Zentry.Modules.Attendance.Infrastructure.Persistence.Configurations;

public class BluetoothScanConfiguration
{
    public static void Configure(IMongoDatabase database)
    {
        var indexKeys =
            Builders<BluetoothScan>.IndexKeys.Ascending(n => n.Timestamp);
        var indexOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(30) };
        var indexModel =
            new CreateIndexModel<BluetoothScan>(indexKeys, indexOptions);

        var collection = database.GetCollection<BluetoothScan>("BluetoothScan");
        collection.Indexes.CreateOne(indexModel);
    }
}