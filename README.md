# 🍲 Food Explorer - Your Culinary Companion

![Project Banner](/api/placeholder/1200/300)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-7.0-blue.svg)](https://docs.microsoft.com/en-us/aspnet/core/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-7.0-purple.svg)](https://docs.microsoft.com/en-us/ef/core/)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5.0-blueviolet.svg)](https://getbootstrap.com/)

A feature-rich food management system built with ASP.NET Core MVC, empowering users to explore, create, and order delicious recipes.

## ✨ Key Features

### 👨‍🍳 For Food Enthusiasts
- Browse and search recipes by category, ingredients, or dietary preferences
- Save favorite recipes and create personal collections
- Track order history and status
- Upload and share your own recipes

### 🏪 For Administrators
- Manage recipe categories and ingredients
- Monitor and process user orders
- User role management
- Analytics dashboard

## 🛠️ Tech Stack

- **Backend**: ASP.NET Core MVC, Entity Framework Core
- **Database**: SQL Server
- **Frontend**: Bootstrap 5, jQuery
- **Authentication**: ASP.NET Core Identity
- **Architecture**: Clean three-layer architecture (Web, BLL, DAL)

## 📸 Screenshots

### Home Dashboard
![Dashboard](/api/placeholder/800/400)

### Recipe Details
![Recipe View](/api/placeholder/800/400)

### Order Management
![Orders](/api/placeholder/800/400)

## 🚀 Getting Started

1. **Clone the Repository**
```bash
git clone https://github.com/YourUsername/FoodExplorer.git
cd FoodExplorer
```

2. **Configure Database**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FoodExplorerDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

3. **Setup & Run**
```bash
dotnet restore
dotnet ef database update
dotnet run
```

Visit `https://localhost:5001` to start exploring!

## 🔒 Environment Variables

Create a `.env` file with:
```
DB_CONNECTION_STRING=your_connection_string
JWT_SECRET=your_secret_key
SMTP_HOST=your_smtp_server
```

## 🤝 Contributing

1. Fork the repository
2. Create feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📜 License

Distributed under the MIT License. See `LICENSE` for more information.

## 📞 Contact & Support

- 📧 Email: (tarek.abozeid101@gmail.com)
- 🌐 Whatsapp: (+20)1002999082

---
Made with ❤️ by Tarek Abozeid
