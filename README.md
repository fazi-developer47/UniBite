# UniBite - University Canteen Food Ordering System 🍽️

UniBite is a full-stack ASP.NET Core MVC application with SQL Server integration.  
It provides a complete ecosystem for **customers** to order food online and for **admins** to manage the system efficiently.

---

## 🌟 Core Modules

### 👨‍💼 Admin Side
- Manage menu items (add, edit, delete)
- Monitor system metrics (Users, Orders, Earnings, Feedbacks)
- View animated charts for earnings and order trends
- Smart search across users, orders, menu items, feedbacks
- Role-based authentication (Admin only)

### 🛒 Customer Side
- Browse menu items with categories (Breakfast, Lunch, Snacks, Drinks)
- Place food orders online
- Track order status (Pending, Preparing, Ready, Delivered)
- Submit feedback and ratings

---

## 🗄️ Database Design
- **Users Table** → Customers + Admins
- **MenuItems Table** → Food categories and items
- **Orders Table** → Order details with status tracking
- **Feedback Table** → Ratings and comments
- **Earnings Table** → Revenue tracking

---

## 🚀 Tech Stack
- **Backend:** ASP.NET Core MVC, Entity Framework
- **Frontend:** Razor Views, Bootstrap, Chart.js
- **Database:** SQL Server
- **Authentication:** ASP.NET Identity (Role-based)

---

## 🗄️ Database Setup

### Restore from SQL Script (Recommended)
1. Open **SQL Server Management Studio (SSMS)**.
2. Create a new database: `UniBiteDB`.
3. Open `UniBiteDB.sql` from this repo.
4. Execute the script → Database ready.

---

## 💻 Running the Application

1. Clone the repository:
   ```bash
   git clone https://github.com/fazi-developer47/UniBite.git
   cd UniBite
