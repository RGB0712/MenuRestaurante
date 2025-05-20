using Amazon.DynamoDBv2.DataModel;

namespace MenuRestaurante_RaulGonzalez.Models
{
    [DynamoDBTable("MenuItems")]
    public class MenuItem
    {
        [DynamoDBHashKey]
        public string? ItemId { get; set; }

        [DynamoDBProperty]
        public string? Name { get; set; }

        [DynamoDBProperty]
        public string? Description { get; set; }

        [DynamoDBProperty]
        public string? Category { get; set; }

        [DynamoDBProperty]
        public decimal? Price { get; set; }

        [DynamoDBProperty]
        public bool IsAvailable { get; set; }

        [DynamoDBProperty]
        public string? ImageKey { get; set; }
    }
}
