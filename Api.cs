using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Data;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;


namespace HotelManagement;

/// <summary>The API for the project</summary>
/// <param name="source">The data context being used for the project</param>
public class Api(AbstractHotelDataContext source) : IEqualityComparer<HotelRoom>
{

    /// <summary>Searches all hotel for matching keyword within its names or IDs</summary>
    /// <param name="keyword">The string to search within name or ID of hotel</param>
    /// <returns>A collection of `HotelDetail`</returns>
    public IEnumerable<HotelDetail> FindHotel(string keyword) =>
        int.TryParse(keyword, out _) ?
        Source.DbHotelDetails.Where(x => x.HotelId.ToString().Contains(keyword)) :
        Source.DbHotelDetails.Where(x => x.HotelName.Contains(keyword));

    /// <summary>Finds all available rooms for the given dates and room types</summary>
    /// <param name="data">The paramters for search input within the request body</param>
    /// <returns>RoomSearch object containing list of matching rooms</returns>

    public RoomSearchResult SearchRoom([FromBody] SearchRoomInput data)
    {
        var (ErrMessage, start, end) = ValidateDate(data.StartDate, data.EndDate);
        if (ErrMessage != string.Empty)
        {
            return new(false, ErrMessage, default, default, default);
        }
        var booked_rooms = AlreadyBookedRooms(start, end).ToHashSet(this);
        var filtered_results = Source.DbHotelRooms
            .Where(x => x.RoomTypeInt == data.RoomType)
            .ToHashSet(this)
            .Except(booked_rooms, this)
            .OrderBy(room => room.Cost);
        return new(
            true,
            "Success",
            start,
            end,
            [.. filtered_results]
        );
    }

    /// <summary>Books a room, if valid for booking</summary>
    /// <param name="data">The parameters for room booking</param>
    /// <returns>BookingResult instance that contains the details of the booking process</returns>
    public BookingResult BookRoom([FromBody] BookRoomInput data)
    {
        var (ErrMessage, start, end) = ValidateDate(data.StartDate, data.EndDate);
        if (ErrMessage != string.Empty) return new(false, ErrMessage, default);
        var hotelId = data.HotelId;
        var roomNumber = data.RoomNumber;
        var room = Source.DbHotelRooms.FirstOrDefault(
            x => x.Room_HotelId == hotelId && x.Room_RoomNumber == roomNumber);
        if (room == null)
        {
            return new(
                false,
                "Given room does not exist",
                default
            );
        }
        if (AlreadyBookedRooms(start, end).Any(x => x.Room_HotelId == hotelId && x.Room_RoomNumber == roomNumber))
        {
            return new(
                false,
                "Given room is already booked, cannot proceed for a new booking",
                default
            );
        }
        var entry = Source.DbRoomBookings.Add(new(default, hotelId, roomNumber, start, end));
        if (Source.SaveChanges() > 0)
            return new(true, "Booking succesful", entry.Entity);

        return new(false, "Unknown Error", default);
    }

    /// <summary>Fetches the details of booking</summary>
    /// <param name="data">The booking ID to fetch details for</param>
    /// <returns>BookingResult instance that contains the details of the search</returns>
    public BookingResult CheckBooking([FromBody] CheckBookingInput data)
    {
        var result = Source.DbRoomBookings.FirstOrDefault(x => x.BookingId == data.BookingId);
        if (result == null) return new(false, "Given Booking ID does not exist!", null);
        return new(true, "Search successful", result);
    }

    /// <summary>Returns rooms that are already booked for the given period</summary>
    /// <param name="start">The date of checking in</param>
    /// <param name="end">The date of checking out</param>
    /// <returns>Returns a sequence of rooms that will be unavailable for the given dates of booking</returns>
    protected IEnumerable<HotelRoom> AlreadyBookedRooms(DateTime start, DateTime end) =>
        Source.DbRoomBookings.Where(
            booking => booking.Start < end && start < booking.End
        ).Select(
            booking => new HotelRoom(
                booking.Booking_HotelId,
                booking.Booking_RoomNumber,
                default,
                default
            )
        );

    /// <summary> Seeds the dataset with random data</summary>
    /// <returns>SeedResult instance reporting the number of added hotels</returns>
    public SeedResult SeedDatabase()
    {
        const int hotel_count = 50;
        int[][] room_distribution = [
            [3, 2, 1],
            [2, 2, 2],
            [0, 3, 3],
            [3, 3, 0],
            [6, 0, 0],
        ];
        RoomType[] room_types = [RoomType.Single, RoomType.Double, RoomType.Deluxe];
        Random random = new();

        static string GenerateName(Random r, int len)
        {
            string[] consonants = [
                "b", "c", "d", "f", "g", "h", "j", "k", "l", "m",
            "ll", "n", "p", "q", "r", "s", "sh", "zh", "sk", "sw",
            "t", "v", "w", "x", "th", "wr", "cr", "tr",
            "vr", "br", "gr", "gh", "kr", "dh", "dr", "ny", "fr"];
            string[] vowels = ["a", "e", "i", "o", "u", "ae", "y", "ei", "ou", "oo", "oh", "or", "ar", "eve"];
            string Name = "";
            if ((r.Next() & 1) == 0) Name += vowels[r.Next(vowels.Length)];
            Name += consonants[r.Next(consonants.Length)];
            Name += vowels[r.Next(vowels.Length)];
            char first = Name[0];
            Name = char.ToUpper(first) + Name[1..];
            while (true)
            {
                string part = consonants[r.Next(consonants.Length)];
                part += vowels[r.Next(vowels.Length)];
                if (part.Length + Name.Length >= len) return Name;
                else Name += part;
            }
        }

        for (int idx = 0; idx < hotel_count; idx++)
        {
            int dist_id = random.Next() % room_distribution.GetLength(0);
            var random_str = GenerateName(random, random.Next(5, 16));
            var entry = Source.DbHotelDetails.Add(
                new(default, random_str, "", "", "", "")
            );
            if (Source.SaveChanges() <= 0)
                throw new Exception("Adding hotel in database failed!");

            int base_cost = random.Next(1, 15) * 5, addition = random.Next(10, 30) * 5;

            for (int r_idx = 0; r_idx < 3; r_idx++)
            {
                int type_int = (int)room_types[r_idx];
                int suffix = type_int * 10;
                for (int i = 1; i <= room_distribution[dist_id][r_idx]; i++)
                {
                    Source.DbHotelRooms.Add(new(
                        entry.Entity.HotelId,
                        suffix + i, type_int,
                        base_cost + (addition * type_int)));
                }
            }
            if (Source.SaveChanges() <= 0)
                throw new Exception("Adding rooms in database failed!");
        }
        return new(hotel_count, hotel_count * 6);
    }

    /// <summary>Clears all data from the database</summary>
    /// <returns>ResetResult instance reporting the number of deleted rows</returns>
    public ResetResult ResetDatabase()
    {
        try
        {
            return new(
            true,
            Source.DbRoomBookings.ExecuteDelete() +
            Source.DbHotelRooms.ExecuteDelete() +
            Source.DbHotelDetails.ExecuteDelete()
        );
        }
        catch
        {
            return new(false, 0);
        }
    }

    static (string ErrMessage, DateTime Start, DateTime End) ValidateDate(string start_date, string end_date)
    {
        if (!DateTime.TryParseExact(
            start_date,
            DateFormat,
            null,
            DateTimeStyles.AdjustToUniversal,
            out var start))
        {
            return new(
                $"Invalid start_date={start_date}! Use Format {DateFormat} for booking!",
                default, default);
        }
        if (!DateTime.TryParseExact(
            end_date,
            DateFormat,
            null,
            DateTimeStyles.AdjustToUniversal,
            out var end))
        {
            return new(
                $"Invalid start_date={end_date}! Use Format {DateFormat} for booking!",
                default, default);
        }
        if (end <= start)
        {
            return new(
                "Invalid date range for booking!",
                start,
                end);
        }
        return (string.Empty, start.ToUniversalTime(), end.ToUniversalTime());

    }

    /// <summary> Equals function for use of Except function</summary>
    /// <param name="a">The lhs HotelRoom</param>
    /// <param name="b">The rhs HotelRoom</param>
    /// <returns>Returns true if equal, otherwise false</returns>
    public bool Equals(HotelRoom? a, HotelRoom? b)
    {
        if (a == null || b == null) return false;
        return a.Room_HotelId == b.Room_HotelId && a.Room_RoomNumber == b.Room_RoomNumber;
    }

    /// <summary>GetHashCode function for HotelRoom</summary>
    /// <param name="room">The room to generate hash for</param>
    /// <returns>int value of hash</returns>
    public int GetHashCode(HotelRoom room) => room.Room_HotelId;

    /// <summary>The DbContext that passes data from the database</summary>
    protected readonly AbstractHotelDataContext Source = source;

    /// <summary>A type to encapsulate the result of database seeding</summary>
    /// <param name="AddedHotels">The number of hotels added</param>
    /// <param name="AddedRooms">The number of hotel rooms added</param>
    public record SeedResult(int AddedHotels, int AddedRooms);

    /// <summary>A type to encapsulate the result of database reset</summary>
    /// <param name="Success">Shows if the function was completed successfully</param>
    /// <param name="DeletedRows">The number of rows deleted</param>
    public record ResetResult(bool Success, int DeletedRows);

    /// <summary>A type to encapuslate the result of a booking</summary>
    /// <param name="Success">Shows if the booking was successful</param>
    /// <param name="Message">Description for the process</param>
    /// <param name="Details">Details of the successful booking</param>
    public record struct BookingResult(
        bool Success,
        string Message,
        RoomBooking? Details);

    /// <summary>A type to encapsulate the result of a Room Search</summary>
    /// <param name="Success">Denotes if the search was valid and processed correctly</param>
    /// <param name="Message">Description of the process</param>
    /// <param name="Start">The date of checking in</param>
    /// <param name="End">The date of checking out</param>
    /// <param name="Results">The list of matching results</param>
    public record RoomSearchResult(
        bool Success,
        string Message,
        DateTime Start,
        DateTime End,
        HotelRoom[]? Results);

    /// <summary>A greeting function</summary>
    /// <returns>string for greeting</returns>
    public string Greet() => "{\"Message\": \"Hello from .NET 9\"}";

    /// <summary>
    /// The Date format string
    /// </summary>
    public const string DateFormat = "yyyy-MM-dd-HH";


    /// <summary>Input type for Search Room</summary>
    /// <param name="RoomType">The type of room to search: 0=single, 1=double, 2=deluxe</param>
    /// <param name="StartDate">The date of checking in</param>
    /// <param name="EndDate">The date of checking out</param>
    public record SearchRoomInput(
        int RoomType,
        string StartDate,
        string EndDate
    );

    /// <summary>Input Type for room booking function</summary>
    /// <param name="HotelId">The Hotel to book</param>
    /// <param name="RoomNumber">The Room to book</param>
    /// <param name="StartDate">The date of checking in</param>
    /// <param name="EndDate">The date of checking out</param>
    public record BookRoomInput(
        int HotelId,
        int RoomNumber,
        string StartDate,
        string EndDate
    );

    /// <summary>The input type for checking details of booking</summary>
    /// <param name="BookingId">The booking ID to check</param>
    public record CheckBookingInput(int BookingId);

}