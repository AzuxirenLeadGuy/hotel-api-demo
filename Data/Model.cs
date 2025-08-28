using System;
namespace HotelManagement.Data;

/// <summary>The type of room</summary>
public enum RoomType
{
    /// <summary>Single rooms</summary>
    Single,
    /// <summary>Double rooms</summary>
    Double,
    /// <summary>Deluxe rooms</summary>
    Deluxe
}

/// <summary>The details of a hotel</summary>
/// <param name="HotelId">The automatically generated Hotel ID</param>
/// <param name="HotelName">The name of the hotel</param>
/// <param name="Address">The address of the hotel</param>
/// <param name="PostCode">The postcode of the hotel</param>
/// <param name="Phone">The contact phone number</param>
/// <param name="Email">The contact email address</param>
public record HotelDetail(
    int HotelId,
    string HotelName,
    string Address,
    string PostCode,
    string Phone,
    string Email
)
{
    /// <summary>The SQL fixed character size for hotel name</summary>
    public const byte HotelNameLength = 20;
    /// <summary>The SQL fixed character size for hotel address</summary>
    public const byte AddressLength = 30;
    /// <summary>The SQL fixed character size for post code</summary>
    public const byte PostCodeLength = 10;
    /// <summary>The SQL fixed character size for phone number</summary>
    public const byte PhoneLength = 15;
    /// <summary>The SQL fixed character size for email address</summary>
    public const byte EmailLength = 20;
}

/// <summary>Details of a hotel room</summary>
/// <param name="Room_HotelId">The ID of the hotel this rooms belongs at</param>
/// <param name="Room_RoomNumber">The room number</param>
/// <param name="RoomTypeInt">The type of room</param>
/// <param name="Cost">The cost of the room</param>
public sealed record HotelRoom(
    int Room_HotelId,
    int Room_RoomNumber,
    int RoomTypeInt,
    decimal Cost
)
{
    /// <summary>Enum conversion of room type</summary>
    public RoomType GetRoomType => (RoomType)RoomTypeInt;
}

/// <summary>The record of a room booking</summary>
/// <param name="BookingId">The booking ID</param>
/// <param name="Booking_HotelId">The ID of the hotel the room is booked at</param>
/// <param name="Booking_RoomNumber">The room number of the booked room</param>
/// <param name="Start">The check in date</param>
/// <param name="End">The check out date</param>
public record RoomBooking(
    int BookingId,
    int Booking_HotelId,
    int Booking_RoomNumber,
    DateTime Start,
    DateTime End
);

