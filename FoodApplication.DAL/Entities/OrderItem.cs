using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FoodApplication.DAL.Entities
{
    [Table("OrderItems")]
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        public int RecipeId { get; set; }
        [ForeignKey("RecipeId")]

        public Recipe Recipe { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
