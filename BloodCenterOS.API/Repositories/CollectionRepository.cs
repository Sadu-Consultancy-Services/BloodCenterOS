using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class CollectionRepository : ICollectionRepository
{
    private readonly IDbConnectionFactory _db;

    public CollectionRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<long> CreateAsync(Collection collection, long createdBy)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_collection_create(@p_center_id, @p_branch_id, @p_camp_id, @p_donor_id, @p_bag_no, @p_barcode, @p_lot_no, @p_volume, @p_collector_id, @p_location_type, @p_start, @p_end, @p_notes, @p_created_by)",
            new
            {
                p_center_id = collection.CenterId,
                p_branch_id = collection.BranchId,
                p_camp_id = collection.CampId,
                p_donor_id = collection.DonorId,
                p_bag_no = collection.BloodBagNumber,
                p_barcode = collection.BagBarcode,
                p_lot_no = collection.BagLotNumber,
                p_volume = collection.BagVolumeMl,
                p_collector_id = collection.CollectorEmployeeId,
                p_location_type = collection.CollectionLocationType,
                p_start = collection.CollectionStartTime,
                p_end = collection.CollectionEndTime,
                p_notes = collection.Notes,
                p_created_by = createdBy
            });
    }

    public async Task<IEnumerable<Collection>> GetByCenterAsync(long centerId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_collection_get_by_center(@p_center_id)",
            new { p_center_id = centerId });
        return rows.Select(r => new Collection
        {
            CollectionId = (long)r.collectionid,
            CenterId = (long?)r.centerid,
            BranchId = (long?)r.branchid,
            CampId = (long?)r.campid,
            DonorId = (long?)r.donorid,
            BloodBagNumber = (string?)r.bloodbagnumber,
            BagBarcode = (string?)r.bagbarcode,
            BagLotNumber = (string?)r.baglotnumber,
            BagVolumeMl = (decimal?)r.bagvolumeml,
            CollectorEmployeeId = (long?)r.collectoremployeeid,
            CollectionLocationType = (string?)r.collectionlocationtype,
            CollectionStartTime = (DateTime?)r.collectionstarttime,
            CollectionEndTime = (DateTime?)r.collectionendtime,
            Notes = (string?)r.notes,
            CreatedAt = (DateTime)r.createdat,
            CreatedBy = (long?)r.createdby
        });
    }

    public async Task<Collection?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        var result = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM CollectionRecord WHERE CollectionId = @id",
            new { id });
        if (result == null) return null;
        return new Collection
        {
            CollectionId = (long)result.collectionid,
            CenterId = (long?)result.centerid,
            BranchId = (long?)result.branchid,
            CampId = (long?)result.campid,
            DonorId = (long?)result.donorid,
            BloodBagNumber = (string?)result.bloodbagnumber,
            BagBarcode = (string?)result.bagbarcode,
            BagLotNumber = (string?)result.baglotnumber,
            BagVolumeMl = (decimal?)result.bagvolumeml,
            CollectorEmployeeId = (long?)result.collectoremployeeid,
            CollectionLocationType = (string?)result.collectionlocationtype,
            CollectionStartTime = (DateTime?)result.collectionstarttime,
            CollectionEndTime = (DateTime?)result.collectionendtime,
            Notes = (string?)result.notes,
            CreatedAt = (DateTime)result.createdat,
            CreatedBy = (long?)result.createdby
        };
    }
}
