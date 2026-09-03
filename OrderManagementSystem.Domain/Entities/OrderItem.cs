namespace OrderManagementSystem.Domain.Entities
{
    public class OrderItem
    {
        public Guid Id { get; private set; }
        
        public string ProductId { get; private set; }

        public string ProductName { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal Subtotal => Quantity * UnitPrice;

        private OrderItem()
        {
            // Required by EF Core
        }

        public OrderItem(string productId, string productName, int quantity, decimal unitPrice)
        {
            if (string.IsNullOrWhiteSpace(productId))
            {
                throw new ArgumentException(
                    "Product ID must be a non-empty value.", 
                    nameof(productId));
            }

            if(string.IsNullOrWhiteSpace(productName))
            {
                throw new ArgumentException(
                    "Product name must be a non-empty value.",
                    nameof(productName));
            }

            if (quantity <= 0)
            {
                throw new ArgumentException(
                    "Quantity must be a positive value.",
                    nameof(quantity));
            }

            if (unitPrice <= 0)
            {
                throw new ArgumentException(
                    "Unit price must be a positive value.",
                    nameof(unitPrice));
            }

            Id = Guid.NewGuid();

            ProductId = productId;

            ProductName = productName;

            Quantity = quantity;
            
            UnitPrice = unitPrice;
        }
    }
}
