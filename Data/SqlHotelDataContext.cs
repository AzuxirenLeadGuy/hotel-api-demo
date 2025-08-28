using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Data;
/// <summary>DataContext using MSSQL Server + Npgsql adapater for a PostgreSQL database</summary>
public class SqlHotelDataContext : AbstractHotelDataContext
{
    /// <inheritdoc/>
    public override DbSet<HotelDetail> DbHotelDetails { get; set; } = default!;
    /// <inheritdoc/>
    public override DbSet<HotelRoom> DbHotelRooms { get; set; } = default!;
    /// <inheritdoc/>
    public override DbSet<RoomBooking> DbRoomBookings { get; set; } = default!;
    /// <summary>Connection string for the PostgreSQL database</summary>
    public readonly string ConnectionString;
    /// <summary>If true, use MSSQL database. Otherwise use PostgreSQL database</summary>
    public readonly bool UseMSSQL;
    /// <summary>Constructor for the PostgreSQL Datacontext</summary>
    /// <param name="conn_string">The connection string</param>
    /// <param name="use_mssql">If true, use MSSQL database. Otherwise use PostgreSQL database</param>
    public SqlHotelDataContext(string conn_string, bool use_mssql)
    {
        ConnectionString = conn_string;
        UseMSSQL = use_mssql;
        Database.EnsureCreated();
    }
    /// <inheritdoc/>
    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        if (UseMSSQL)
            optionsBuilder.UseSqlServer(ConnectionString);
        else
            optionsBuilder.UseNpgsql(ConnectionString);
    }
}
