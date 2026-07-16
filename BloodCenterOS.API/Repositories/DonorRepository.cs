using BloodCenterOS.API.Data;
using BloodCenterOS.Core.Models;
using Dapper;

namespace BloodCenterOS.API.Repositories;

public class DonorRepository : IDonorRepository
{
    private readonly IDbConnectionFactory _db;

    public DonorRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<long> CreateAsync(Donor donor)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<long>(
            "SELECT * FROM fn_donor_create(@p_center_id, @p_donor_code, @p_first_name, @p_last_name, @p_gender, @p_dob, @p_blood_group, @p_phone, @p_email, @p_aadhaar, @p_addr1, @p_addr2, @p_city, @p_pincode, @p_occupation, @p_language, @p_created_by)",
            new
            {
                p_center_id = donor.CenterId,
                p_donor_code = donor.DonorCode,
                p_first_name = donor.FirstName,
                p_last_name = donor.LastName,
                p_gender = donor.Gender,
                p_dob = donor.DateOfBirth,
                p_blood_group = donor.BloodGroup,
                p_phone = donor.Phone,
                p_email = donor.Email,
                p_aadhaar = donor.AadhaarNumber,
                p_addr1 = donor.AddressLine1,
                p_addr2 = donor.AddressLine2,
                p_city = donor.City,
                p_pincode = donor.Pincode,
                p_occupation = donor.Occupation,
                p_language = donor.PreferredLanguage,
                p_created_by = donor.CreatedBy
            });
    }

    public async Task<Donor?> GetByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        var result = await conn.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM fn_donor_get_by_id(@p_donor_id)",
            new { p_donor_id = id });
        if (result == null) return null;
        return new Donor
        {
            DonorId = (long)result.donorid,
            CenterId = (long?)result.centerid,
            DonorCode = (string?)result.donorcode,
            FirstName = (string)result.firstname,
            LastName = (string?)result.lastname,
            Gender = (string?)result.gender,
            DateOfBirth = (DateTime?)result.dateofbirth,
            BloodGroup = (string?)result.bloodgroup,
            Phone = (string?)result.phone,
            Email = (string?)result.email,
            AadhaarNumber = (string?)result.aadhaarnumber,
            AddressLine1 = (string?)result.addressline1,
            AddressLine2 = (string?)result.addressline2,
            City = (string?)result.city,
            Pincode = (string?)result.pincode,
            Occupation = (string?)result.occupation,
            PreferredLanguage = (string?)result.preferredlanguage,
            LastDonationDate = (DateTime?)result.lastdonationdate,
            TotalDonations = (int)result.totaldonations,
            CreatedAt = (DateTime)result.createdat,
            CreatedBy = (long?)result.createdby,
            UpdatedAt = (DateTime?)result.updatedat,
            UpdatedBy = (long?)result.updatedby
        };
    }

    public async Task UpdateAsync(Donor donor)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT * FROM fn_donor_update(@p_donor_id, @p_center_id, @p_donor_code, @p_first_name, @p_last_name, @p_gender, @p_dob, @p_blood_group, @p_phone, @p_email, @p_aadhaar, @p_addr1, @p_addr2, @p_city, @p_pincode, @p_occupation, @p_language, @p_updated_by)",
            new
            {
                p_donor_id = donor.DonorId,
                p_center_id = donor.CenterId,
                p_donor_code = donor.DonorCode,
                p_first_name = donor.FirstName,
                p_last_name = donor.LastName,
                p_gender = donor.Gender,
                p_dob = donor.DateOfBirth,
                p_blood_group = donor.BloodGroup,
                p_phone = donor.Phone,
                p_email = donor.Email,
                p_aadhaar = donor.AadhaarNumber,
                p_addr1 = donor.AddressLine1,
                p_addr2 = donor.AddressLine2,
                p_city = donor.City,
                p_pincode = donor.Pincode,
                p_occupation = donor.Occupation,
                p_language = donor.PreferredLanguage,
                p_updated_by = donor.UpdatedBy
            });
    }

    public async Task<PagedResult<Donor>> SearchAsync(long? centerId, string? keyword, string? bloodGroup, string? gender, int page, int size)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_donor_search(@p_center_id, @p_keyword, @p_blood_group, @p_gender, @p_page, @p_size)",
            new
            {
                p_center_id = centerId,
                p_keyword = keyword,
                p_blood_group = bloodGroup,
                p_gender = gender,
                p_page = page,
                p_size = size
            });
        var list = new List<Donor>();
        var totalCount = 0L;
        foreach (var row in rows)
        {
            totalCount = (long)row.totalcount;
            list.Add(new Donor
            {
                DonorId = (long)row.donorid,
                CenterId = (long?)row.centerid,
                DonorCode = (string?)row.donorcode,
                FirstName = (string)row.firstname,
                LastName = (string?)row.lastname,
                Gender = (string?)row.gender,
                BloodGroup = (string?)row.bloodgroup,
                Phone = (string?)row.phone,
                City = (string?)row.city,
                LastDonationDate = (DateTime?)row.lastdonationdate,
                TotalDonations = (int)row.totaldonations
            });
        }
        return new PagedResult<Donor>
        {
            Items = list,
            TotalCount = totalCount,
            Page = page,
            PageSize = size
        };
    }

    public async Task<IEnumerable<Donation>> GetDonationHistoryByDonorAsync(long donorId)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_donor_donation_get_by_donor(@p_donor_id)",
            new { p_donor_id = donorId });
        return rows.Select(r => new Donation
        {
            DonationId = (long)r.donationid,
            CenterId = null,
            DonorId = donorId,
            DonationDate = (DateTime)r.donationdate,
            DonationType = (string?)r.donationtype,
            VolumeMl = (decimal?)r.volumeml,
            BagNumber = (string?)r.bagnumber
        });
    }

    public async Task<IEnumerable<Donor>> GetByPhoneAsync(long centerId, string phone)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(
            "SELECT * FROM fn_donor_get_by_phone(@p_center_id, @p_phone)",
            new { p_center_id = centerId, p_phone = phone });
        return rows.Select(r => new Donor
        {
            DonorId = (long)r.donorid,
            CenterId = (long?)r.centerid,
            DonorCode = (string?)r.donorcode,
            FirstName = (string)r.firstname,
            LastName = (string?)r.lastname,
            Gender = (string?)r.gender,
            DateOfBirth = (DateTime?)r.dateofbirth,
            BloodGroup = (string?)r.bloodgroup,
            Phone = (string?)r.phone,
            Email = (string?)r.email,
            AadhaarNumber = (string?)r.aadhaarnumber,
            AddressLine1 = (string?)r.addressline1,
            AddressLine2 = (string?)r.addressline2,
            City = (string?)r.city,
            Pincode = (string?)r.pincode,
            Occupation = (string?)r.occupation,
            PreferredLanguage = (string?)r.preferredlanguage,
            LastDonationDate = (DateTime?)r.lastdonationdate,
            TotalDonations = (int)r.totaldonations,
            CreatedAt = (DateTime)r.createdat,
            CreatedBy = (long?)r.createdby,
            UpdatedAt = (DateTime?)r.updatedat,
            UpdatedBy = (long?)r.updatedby
        });
    }
}
