using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Data;
/// <summary>Represents a class that can serve as a Database Context within EF Core</summary>
public abstract class AbstractHotelDataContext : DbContext
{
    /// <summary>The collection containing HotelDetail rows</summary>
    public abstract DbSet<HotelDetail> DbHotelDetails { get; set; }
    /// <summary>The collection containing HotelRooms rows</summary>
    public abstract DbSet<HotelRoom> DbHotelRooms { get; set; }
    /// <summary>The collection containing RoomBooking rows</summary>
    public abstract DbSet<RoomBooking> DbRoomBookings { get; set; }
    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HotelDetail>().HasKey(detail => detail.HotelId);
        modelBuilder.Entity<HotelDetail>(
            entity => entity.Property(
                detail => detail.HotelId
            ).UseIdentityAlwaysColumn()
        );
        modelBuilder.Entity<HotelDetail>(
            entity => entity.Property(
                detail => detail.HotelName
            )
            .HasMaxLength(HotelDetail.HotelNameLength)
            .IsRequired()
        );
        modelBuilder.Entity<HotelDetail>(
            entity => entity.Property(
                detail => detail.Address
            ).HasMaxLength(HotelDetail.AddressLength)
        );
        modelBuilder.Entity<HotelDetail>(
            entity => entity.Property(
                detail => detail.PostCode
            ).HasMaxLength(HotelDetail.PostCodeLength)
        );
        modelBuilder.Entity<HotelDetail>(
            entity => entity.Property(
                detail => detail.Phone
            ).HasMaxLength(HotelDetail.PhoneLength)
        );
        modelBuilder.Entity<HotelDetail>(
            entity => entity.Property(
                detail => detail.Email
            ).HasMaxLength(HotelDetail.EmailLength)
        );

        modelBuilder.Entity<HotelRoom>().HasKey(
            room => new { room.Room_HotelId, room.Room_RoomNumber }
        );
        modelBuilder.Entity<HotelRoom>().HasOne<HotelDetail>().WithMany().HasForeignKey(
            room => room.Room_HotelId
        ).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<HotelRoom>(ent => ent.Property(room => room.Room_HotelId).IsRequired());
        modelBuilder.Entity<HotelRoom>(ent => ent.Property(room => room.Cost).IsRequired());

        modelBuilder.Entity<RoomBooking>().HasKey(booking => booking.BookingId);
        modelBuilder.Entity<RoomBooking>().HasOne<HotelRoom>().WithMany().HasForeignKey(
            book => new { book.Booking_HotelId, book.Booking_RoomNumber, }
        ).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<RoomBooking>(ent => ent.Property(book => book.Booking_HotelId).IsRequired());
        modelBuilder.Entity<RoomBooking>(ent => ent.Property(book => book.Booking_RoomNumber).IsRequired());
        modelBuilder.Entity<RoomBooking>(ent => ent.Property(book => book.Start).IsRequired());
        modelBuilder.Entity<RoomBooking>(ent => ent.Property(book => book.End).IsRequired());
        // modelBuilder.Entity<RoomBooking>().ToTable(table => table.HasCheckConstraint("date", "Start < End"));
    }
}
