using OrderManagementSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OrderManagementSystem.Domain.Entities
{
    public class Order
    {
        public Guid Id { get; private set; }

        public string OrderNumber { get; private set; }

        public string CustomerName { get; private set; }

        public string Email { get; private set; }

        public OrderStatus Status { get; private set; } = OrderStatus.Pending;

        public DateTime CreatedDate { get; private set; }
        public DateTime UpdatedDate { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedDate { get; private set; }

        public decimal TotalAmount => _items.Sum(item => item.Subtotal);


        private readonly List<OrderItem> _items = new List<OrderItem>();
    
        public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

        private Order()
        {
            // Required by EF Core
        }

        public Order(string customerName, string email, IEnumerable<OrderItem> items)
        {
            if (string.IsNullOrWhiteSpace(customerName))
            {
                throw new ArgumentException(
                    "Customer name is required.");
            }

            if(items == null)
            {
                throw new ArgumentNullException(nameof(items), "Order items cannot be null.");
            }

            var orderItems = items.ToList();

            if(orderItems.Count == 0)
            {
                throw new ArgumentException(
                    "At least one order item is required.", nameof(items));
            }

            if(string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email))
            {
                throw new ArgumentException(
                    "A valid email address is required.");
            }

            Id = Guid.NewGuid();

            OrderNumber = GenerateOrderNumber();

            CustomerName = customerName;

            Email = email;

            _items.AddRange(orderItems);

            Status = OrderStatus.Pending;

            var now = DateTime.UtcNow;

            CreatedDate = now;
            UpdatedDate = now;
        }


        public void Complete()
        {
            if (Status != OrderStatus.Processing)
            {
                throw new InvalidOperationException(
                    "Only processing orders can be completed.");
            }

            Status = OrderStatus.Completed;

            UpdateTimestamp();
        }


        public void Cancel()
        {
            if (Status == OrderStatus.Completed)
            {
                throw new InvalidOperationException(
                    "Completed orders cannot be cancelled.");
            }

            if (Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException(
                    "Cancelled orders cannot be cancelled.");
            }

            Status = OrderStatus.Cancelled;
            UpdateTimestamp();
        }

        public void AddItem(OrderItem item)
        {   
            if(item == null)
            {
                throw new ArgumentNullException(nameof(item), "Order item cannot be null.");
            }

            if (Status == OrderStatus.Completed){
                throw new InvalidOperationException(
                    "Cannot add items to a completed order.");
            }

            if(Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException(
                    "Cannot add items to a cancelled order.");
            }

            if (IsDeleted)
            {
                throw new InvalidOperationException(
                    "Cannot add items to a deleted order.");
            }

            _items.Add(item);
            UpdateTimestamp();
        }

        public void RemoveItem(OrderItem item)
        {
            if(item == null)
            {
                throw new ArgumentNullException(nameof(item), "Order item cannot be null.");
            }

            if (!_items.Contains(item))
            {
                throw new InvalidOperationException(
                    "Order item does not exist in the order.");
            }

            if (_items.Count == 1)
            {
                throw new InvalidOperationException(
                    "An order must contain at least one item.");
            }

            if (Status == OrderStatus.Completed)
            {
                throw new InvalidOperationException(
                    "Cannot remove items from a completed order.");
            }

            if (Status == OrderStatus.Cancelled)
            {
                throw new InvalidOperationException(
                    "Cannot remove items from a cancelled order.");
            }

            if (IsDeleted)
            {
                throw new InvalidOperationException(
                    "Cannot remove items from a deleted order.");
            }

            _items.Remove(item);
            UpdateTimestamp();
        }

        public void StartProcessing()
        {
            if (Status != OrderStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Only pending orders can be processed.");
            }
            Status = OrderStatus.Processing;
            UpdateTimestamp();
        }

        public void SoftDelete()
        {
            if(IsDeleted) {
                throw new InvalidOperationException(
                     "Order is already deleted.");
            }

            IsDeleted = true;
            DeletedDate = DateTime.UtcNow;
            UpdateTimestamp();
        }

        private void UpdateTimestamp()
        {
            UpdatedDate = DateTime.UtcNow;
        }

        private static string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
        }
    }
}
