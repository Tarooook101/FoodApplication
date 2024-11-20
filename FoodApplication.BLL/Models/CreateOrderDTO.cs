using FoodApplication.DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodApplication.BLL.Models
{
    public class CreateOrderDTO
    {
        [Required(ErrorMessage = "Order date is required")]
        public DateTime OrderDate { get; set; }

        [Required(ErrorMessage = "Order status is required")]
        public OrderStatus Status { get; set; }

        [Required(ErrorMessage = "At least one order item is required")]
        public List<CreateOrderItemDTO> OrderItems { get; set; } = new List<CreateOrderItemDTO>();
    }
}
