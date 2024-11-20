using FoodApplication.BLL.Models;
using FoodApplication.DAL.Entities;
using FoodApplication.DAL.Extend;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodApplication.BLL.Services
{
    public class OrderService : IOrderService
    {
        private readonly IGenericRepository<Order> _orderRepository;
        private readonly IGenericRepository<Recipe> _recipeRepository;
        private readonly UserManager<ApplicationUser> _userManager;


        public OrderService(IGenericRepository<Order> orderRepository,
                            IGenericRepository<Recipe> recipeRepository,
                            UserManager<ApplicationUser> userManager
                            )
        {
            _orderRepository = orderRepository;
            _recipeRepository = recipeRepository;
            _userManager = userManager;
        }

        public async Task<IEnumerable<OrderDTO>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(MapOrderToDTO);
        }

        public async Task<OrderDTO> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id, query => query.Include(o => o.User));
            return order != null ? MapOrderToDTO(order) : null;
        }

        public async Task<int> CreateOrderAsync(CreateOrderDTO dto, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new KeyNotFoundException("User not found");

            var order = new Order
            {
                OrderDate = dto.OrderDate,
                Status = dto.Status,
                UserId = userId,
                User = user,
                OrderItems = dto.OrderItems.Select(item => new OrderItem
                {
                    RecipeId = item.RecipeId,
                    Quantity = item.Quantity,
                    Price = item.Price
                }).ToList()
            };

            order.TotalAmount = order.OrderItems.Sum(oi => oi.Price * oi.Quantity);

            await _orderRepository.AddAsync(order);
            return order.Id;
        }

        public async Task UpdateOrderStatusAsync(int id, OrderStatus status)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) throw new KeyNotFoundException("Order not found");

            order.Status = status;
            await _orderRepository.UpdateAsync(order);
        }

        public async Task<IEnumerable<OrderDTO>> GetOrderHistoryForUserAsync(string userId)
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Where(o => o.UserId == userId)
                         .OrderByDescending(o => o.OrderDate)
                         .Select(MapOrderToDTO);
        }
        public async Task<DetailsOrderDTO> GetOrderDetailsAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id, query => query
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Recipe)
                        .ThenInclude(r => r.Categories)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Recipe)
                        .ThenInclude(r => r.RecipeIngredients)
                            .ThenInclude(ri => ri.Ingredient));

            if (order == null) return null;

            var detailsDto = new DetailsOrderDTO
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                OrderItems = order.OrderItems.Select(oi => new DetailsOrderItemDTO
                {
                    RecipeId = oi.RecipeId,
                    RecipeName = oi.Recipe.Name,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList(),
                Categories = order.OrderItems.SelectMany(oi => oi.Recipe.Categories.Select(c => c.Name)).Distinct().ToList(),
                Ingredients = order.OrderItems.SelectMany(oi => oi.Recipe.RecipeIngredients.Select(ri => ri.Ingredient.Name)).Distinct().ToList()
            };

            return detailsDto;
        }

        private async Task<ICollection<OrderItem>> CreateOrderItems(List<OrderItemDTO> orderItemDtos)
        {
            var orderItems = new List<OrderItem>();
            foreach (var itemDto in orderItemDtos)
            {
                var recipe = await _recipeRepository.GetByIdAsync(itemDto.RecipeId);
                if (recipe == null) throw new KeyNotFoundException($"Recipe with id {itemDto.RecipeId} not found");


                var orderItem = new OrderItem
                {
                    RecipeId = itemDto.RecipeId,
                    Recipe = recipe,
                    Quantity = itemDto.Quantity,
                    Price = itemDto.Price
                };
                orderItems.Add(orderItem);
            }
            return orderItems;
        }

        private OrderDTO MapOrderToDTO(Order order)
        {
            return new OrderDTO
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                UserId = order.UserId,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDTO
                {
                    Id = oi.Id,
                    RecipeId = oi.RecipeId,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            };
        }


        public async Task<DeleteOrderDTO> GetOrderForDeleteAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id, query => query
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Recipe));

            if (order == null) return null;

            return new DeleteOrderDTO
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                UserName = order.User.UserName,
                OrderItems = order.OrderItems.Select(oi => $"{oi.Recipe.Name} (x{oi.Quantity})").ToList()
            };
        }

        public async Task DeleteOrderAsync(int id)
        {
            await _orderRepository.DeleteAsync(id);
        }

    }
    public interface IOrderService
    {
        Task<IEnumerable<OrderDTO>> GetAllOrdersAsync();
        Task<OrderDTO> GetOrderByIdAsync(int id);
        Task<int> CreateOrderAsync(CreateOrderDTO dto, string userId);
        Task UpdateOrderStatusAsync(int id, OrderStatus status);
        Task<IEnumerable<OrderDTO>> GetOrderHistoryForUserAsync(string userId);
        Task<DetailsOrderDTO> GetOrderDetailsAsync(int id);

        Task<DeleteOrderDTO> GetOrderForDeleteAsync(int id);
        Task DeleteOrderAsync(int id);
    }
}
