# 🎡 Wheel Portal (ASP.NET Core MVC)

This project is an **ASP.NET Core MVC portal** with support for API endpoints, entity management, and dynamic UI rendering.  
It follows a clean **MVC architecture** with a structured folder organization.

---

## 📂 Project Structure



MVC/
└── Portal/
├── Data/                # Database context and migrations
├── Model/               # Entity models (EF Core entities)
├── ViewModel/           # View-specific models for data binding
├── Controller/          # MVC Controllers
│    ├── WheelController.cs         # Controller for wheel-related views
│
├── API/                 # API Controllers (REST endpoints)
│    ├── WheelApiController.cs     # API for wheel data (CRUD, dropdowns, etc.)
│
├── Views/               # Razor Views
├── Wheel/                     # Views related to WheelController

````

---

## 🚀 Features

- **MVC + API Hybrid**  
  Provides both traditional MVC views and JSON APIs for frontend integration.  

- **Entity Framework Core**  
  Database integration with `DbContext` inside `Data/`.  

- **Separation of Concerns**  
  - `Model` → database entities  
  - `ViewModel` → UI-bound models  
  - `Controller` → view rendering logic  
  - `API` → JSON responses for AJAX/JS frontend  

- **Wheel Management**  
  - Hierarchical wheel sections (parent → children → grandchildren)  
  - Dropdown population via API (`WheelApiController`)  
  - Ordering and dynamic updates handled in controllers  

---

## ⚙️ Setup & Run

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/WheelPortal.git
````

2. Navigate into the project:

   ```bash
   cd MVC/Portal
   ```

3. Apply migrations:

   ```bash
   dotnet ef database update
   ```

4. Run the project:

   ```bash
   dotnet run
   ```

---

## 📡 API Endpoints

### `GET /api/WheelApi/get-all`

Fetches:

* Dropdown data (`data`)
* Wheel hierarchy (`wheeldata`)

Response Example:

```json
{
  "success": true,
  "data": [
    { "pkWheelId": 1, "name": "Section 1" }
  ],
  "wheeldata": [
    { "pkWheelId": 10, "name": "Wheel A", "children": [] }
  ]
}
```

---

## 🖼️ Frontend Integration

* **AJAX call in jQuery** is used for fetching dropdown + wheel data:

  ```javascript
  $.ajax({
      url: '/api/WheelApi/get-all',
      type: 'GET',
      success: function(response) {
          if (response.success) {
              // Use response.data & response.wheeldata
          }
      }
  });
  ```

* **Bootstrap 5 loader** included with overlay for better UX.

---

## 📌 Next Steps

* Add unit tests for `WheelController` and `WheelApiController`.
* Extend `ViewModel` for more flexible UI rendering.
* Add authentication & authorization layer.

---

## 📦 Prerequisites

Before scaffolding, install the required **NuGet packages** (via Package Manager Console or .NET CLI):

```powershell
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Microsoft.EntityFrameworkCore.Tools
````

---

## ⚡ Scaffold Command

Run the following command in **Package Manager Console (PMC):**

```powershell
Scaffold-DbContext "Server=.\SqlExpress;Database=WheelDb;Trusted_Connection=True;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context WheelDbContext -Force
```

---

## 🔑 Parameters Explanation

* **Connection String** → Connects to your SQL Server (`WheelDb` database).
* **Provider** → `Microsoft.EntityFrameworkCore.SqlServer` (use the appropriate provider if not using SQL Server).
* **-OutputDir Models** → Stores the generated entity classes inside the `Models` folder.
* **-Context WheelDbContext** → Creates a `DbContext` named `WheelDbContext`.
* **-Force** → Overwrites existing models if they already exist.

---

## 🗂 Model Example

By default, the scaffold may generate navigation properties like this:

```csharp
public partial class WheelSection
{
    public int PkWheelId { get; set; }
    public int? FkParentWheelId { get; set; }
    public string Name { get; set; } = null!;
    public string Colour { get; set; } = null!;
    public int Order { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual WheelSection? FkParentWheel { get; set; }
    public virtual ICollection<WheelSection> InverseFkParentWheel { get; set; } = new List<WheelSection>();
}
```

You can improve readability by renaming the navigation property to `Children`:

```csharp
public partial class WheelSection
{
    public int PkWheelId { get; set; }
    public int? FkParentWheelId { get; set; }
    public string Name { get; set; } = null!;
    public string Colour { get; set; } = null!;
    public int Order { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual WheelSection? FkParentWheel { get; set; }
    public virtual ICollection<WheelSection> Children { get; set; } = new List<WheelSection>();
}
```

---

## 🏗 DbContext Example

Scaffolded code may look like this:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<WheelSection>(entity =>
    {
        entity.HasKey(e => e.PkWheelId);
        entity.HasIndex(e => e.FkParentWheelId, "IX_WheelSections_FkParentWheelId");

        entity.Property(e => e.Colour).HasMaxLength(255);
        entity.Property(e => e.CreatedAt).HasDefaultValue(new DateTime(2025, 8, 5, 12, 59, 50, 81, DateTimeKind.Local).AddTicks(6247));
        entity.Property(e => e.Name).HasMaxLength(255);

        entity.HasOne(d => d.FkParentWheel)
              .WithMany(p => p.InverseFkParentWheel)
              .HasForeignKey(d => d.FkParentWheelId);
    });

    OnModelCreatingPartial(modelBuilder);
}
```

You can update it for the renamed property:

```csharp
modelBuilder.Entity<WheelSection>(entity =>
{
    entity.HasKey(e => e.PkWheelId);
    entity.HasIndex(e => e.FkParentWheelId, "IX_WheelSections_FkParentWheelId");

    entity.Property(e => e.Colour).HasMaxLength(255);
    entity.Property(e => e.CreatedAt).HasDefaultValue(new DateTime(2025, 8, 5, 12, 59, 50, 81, DateTimeKind.Local).AddTicks(6247));
    entity.Property(e => e.Name).HasMaxLength(255);

    entity.HasOne(d => d.FkParentWheel)
          .WithMany(p => p.Children)
          .HasForeignKey(d => d.FkParentWheelId);
});
```

---

## 👨‍💻 Author

Developed by **Tuaha Rasool**
Full Stack Developer (ASP.NET Core + React)

```

---

👉 Do you want me to also add a **diagram (ASCII or Mermaid flowchart)** in the `README.md` showing how **MVC Controller ↔ API Controller ↔ Views ↔ Database** interact?
```
