using Amazon.DynamoDBv2.DataModel;

namespace MenuRestaurante_RaulGonzalez.Models
{
    [DynamoDBTable("Reservations")]
    public class Reservation
    {
        [DynamoDBHashKey]
        public string? ReservationId { get; set; }

        [DynamoDBProperty]
        public string? UserId { get; set; }

        [DynamoDBProperty]
        public DateTime DateOfReservation { get; set; }

        [DynamoDBProperty]
        public int? NumberOfPeople { get; set; }

        [DynamoDBProperty]
        public string? Notes { get; set; }
    }
}
