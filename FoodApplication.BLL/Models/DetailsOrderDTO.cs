using FoodApplication.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodApplication.BLL.Models
{
    public class DetailsOrderDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public List<DetailsOrderItemDTO> OrderItems { get; set; }
        public List<string> Categories { get; set; }
        public List<string> Ingredients { get; set; }
    }
}
