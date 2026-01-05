namespace PharmacyProject.Models
{
    public class CartItem
    {
        public int MedicineId { get; set; }

        public string Name { get; set; } = "";

        public double Price { get; set; }

        public int Quantity { get; set; }

        public double LineTotal
        {
            get
            {
                return Price * Quantity;
            }
        }
    }
}
