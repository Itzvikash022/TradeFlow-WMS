
# 🚀 TradeFlow - Wholesale Management System (WMS)

TradeFlow is a comprehensive web-based Wholesale Management System developed as part of a final year software engineering project and internship at Visitorz.io. The platform is designed to streamline, digitize, and enhance the efficiency of wholesale operations through centralized management of inventory, employees, orders, payments, and more.

---

## ⚙️ Tech Stack

- **Frontend**: HTML, CSS, JavaScript, Bootstrap
- **Backend**: ASP.NET Core MVC (C#), Entity Framework Core
- **Database**: Microsoft SQL Server
- **Authentication**: Email/password, Google OAuth
- **Libraries/Tools**:
  - MailKit (Email handling)
  - Razorpay API (Online payments)
  - PDFWriter (Document generation)
  - ClosedXML (Excel handling)
  - Bcrypt (Password hashing)
  - Geonames API (Location handling)

---

## 🔑 Key Features

### 🧑‍💼 User Management
- Google OAuth + Email/Password-based registration and login
- Role-based access control (Super Admin, Company Admin, Shop Owner, Employee)
- Session management and auto-timeout

### 👨‍🔧 Employee Management
- Add, update, delete employee records
- Bulk import via Excel with image support
- Dynamic permission assignment per employee

### 🛍 Product Management
- Add/edit/delete products with category and price mapping
- Mass product import with images
- Shop-specific pricing with profit margin handling

### 📦 Order & Stock Management
- Multi-type order support: C2S, S2C, S2SB/S2SS
- Real-time stock adjustments
- Manual stock corrections

### 💳 Payment System
- Razorpay integration for online payments
- Manual entry support for offline (cash) payments
- Transaction history logging

### 📊 Reporting & Analytics
- Reports: Orders, Transactions, Products, Employee, Activity Log
- Export to Excel support
- Profit/Loss breakdown per order/shop

### 🔔 Notification System
- Email notifications for key actions
- Toast alerts in-app for real-time updates

### 🛠 Admin Tools
- Master data control: Categories, Roles, Access Rights
- System logs, dashboard stats, company/shop management

---

## 📁 Project Structure

```
TradeFlow/
├── wwwroot/
├── controllers/
├── models/
├── dto/
├── repositories/
├── services/
├── views/
└── utils/
├── config/
├── appsettings.json
```

---

## 🚧 Planned Enhancements

- 🔔 Real-time notifications (SignalR/WebSocket)
- 🛍 Wishlist & delayed cart features
- 📬 Suggestion email module
- 💬 In-app chat system
- 📲 Facebook OAuth and social sharing

---

## 📚 References

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [MailKit](https://github.com/jstedfast/MailKit)
- [PDFWriter](https://github.com/augustodias/PdfWriter)
- [ClosedXML](https://github.com/ClosedXML/ClosedXML)
- [Razorpay](https://razorpay.com/docs/)
- [Bcrypt](https://github.com/kelektiv/node.bcrypt.js)

---

## 👨‍🎓 About the Developer

I'm **Vikash Maurya**, a BSc IT student and intern passionate about creating systems that actually solve real-world problems. Always learning, occasionally overthinking, and forever up for a challenge.

---
  
