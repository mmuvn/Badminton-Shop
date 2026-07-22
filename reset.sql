USE master;
GO

IF DB_ID('BadmintonShopDB') IS NOT NULL
BEGIN
    ALTER DATABASE BadmintonShopDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BadmintonShopDB;
END
GO

/* =========================================================
   BADMINTON SHOP DATABASE
   Target: Microsoft SQL Server
   ========================================================= */

CREATE DATABASE BadmintonShopDB;
GO
USE BadmintonShopDB;
GO

/* =========================================================
   1. IDENTITY / ROLES
   ========================================================= */

CREATE TABLE Roles (
    RoleId      INT IDENTITY(1,1) PRIMARY KEY,
    RoleName    NVARCHAR(50) NOT NULL UNIQUE   -- Customer, Staff, Admin
);

CREATE TABLE Users (
    UserId          INT IDENTITY(1,1) PRIMARY KEY,
    FullName        NVARCHAR(100) NOT NULL,
    Username        NVARCHAR(50) NOT NULL UNIQUE,   -- used to log in
    Email           NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(255) NOT NULL,         -- never store plain-text passwords
    Phone           NVARCHAR(20),
    Address         NVARCHAR(255),
    RoleId          INT NOT NULL,
    IsActive        BIT NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt       DATETIME2 NULL,
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

/* =========================================================
   2. CATALOG
   ========================================================= */

CREATE TABLE Categories (
    CategoryId      INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName    NVARCHAR(50) NOT NULL UNIQUE,  -- Racket, Shirt, Shoe, String, Grip
    Description     NVARCHAR(255)
);

CREATE TABLE Products (
    ProductId       INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId      INT NOT NULL,
    ProductName     NVARCHAR(150) NOT NULL,
    Brand           NVARCHAR(100),
    Description     NVARCHAR(MAX),
    Price           DECIMAL(12,2) NOT NULL CHECK (Price >= 0),
    StockQuantity   INT NOT NULL DEFAULT 0 CHECK (StockQuantity >= 0),
    IsActive        BIT NOT NULL DEFAULT 1,        -- soft delete / discontinue
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt       DATETIME2 NULL,
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId)
);

CREATE TABLE RacketDetails (
    ProductId           INT PRIMARY KEY,
    Color               NVARCHAR(30),
    MaxTensionLbs       DECIMAL(5,2),
    FrameWeightGrams    DECIMAL(5,2),
    CONSTRAINT FK_RacketDetails_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE
);

CREATE TABLE ShirtDetails (
    ProductId       INT PRIMARY KEY,
    Color           NVARCHAR(30),
    Size            NVARCHAR(10),
    Material        NVARCHAR(50),
    SleeveType      NVARCHAR(20),
    Gender          NVARCHAR(20),
    CONSTRAINT FK_ShirtDetails_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE
);

CREATE TABLE ShoeDetails (
    ProductId       INT PRIMARY KEY,
    Color           NVARCHAR(30),
    Size            NVARCHAR(10),
    CONSTRAINT FK_ShoeDetails_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE
);

CREATE TABLE StringDetails (
    ProductId       INT PRIMARY KEY,
    Color           NVARCHAR(30),
    Durability      TINYINT NOT NULL CHECK (Durability BETWEEN 1 AND 10),
    Repulsion       TINYINT NOT NULL CHECK (Repulsion BETWEEN 1 AND 10),
    Sound           TINYINT NOT NULL CHECK (Sound BETWEEN 1 AND 10),
    Control         TINYINT NOT NULL CHECK (Control BETWEEN 1 AND 10),
    ShockAbsorption TINYINT NOT NULL CHECK (ShockAbsorption BETWEEN 1 AND 10),
    CONSTRAINT FK_StringDetails_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE
);

CREATE TABLE GripDetails (
    ProductId       INT PRIMARY KEY,
    ThicknessMm     DECIMAL(3,2),
    Material        NVARCHAR(50),
    Color           NVARCHAR(30),
    CONSTRAINT FK_GripDetails_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE
);

CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_Products_Name ON Products(ProductName);

/* =========================================================
   3. SHOPPING CART
   ========================================================= */

CREATE TABLE Carts (
    CartId          INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId      INT NOT NULL,
    CreatedAt       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Carts_Users FOREIGN KEY (CustomerId) REFERENCES Users(UserId)
);

CREATE TABLE CartItems (
    CartItemId      INT IDENTITY(1,1) PRIMARY KEY,
    CartId          INT NOT NULL,
    ProductId       INT NOT NULL,
    Quantity        INT NOT NULL CHECK (Quantity > 0),
    CONSTRAINT FK_CartItems_Carts FOREIGN KEY (CartId) REFERENCES Carts(CartId) ON DELETE CASCADE,
    CONSTRAINT FK_CartItems_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT UQ_CartItems UNIQUE (CartId, ProductId)
);

/* =========================================================
   4. ORDERS / CHECKOUT
   ========================================================= */

CREATE TABLE OrderStatuses (
    OrderStatusId   INT IDENTITY(1,1) PRIMARY KEY,
    StatusName      NVARCHAR(30) NOT NULL UNIQUE
);

CREATE TABLE Orders (
    OrderId         INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId      INT NOT NULL,
    OrderDate       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    OrderStatusId   INT NOT NULL,
    ShippingAddress NVARCHAR(255),
    TotalAmount     DECIMAL(12,2) NOT NULL DEFAULT 0,
    Note            NVARCHAR(255),
    CancelReason    NVARCHAR(255) NULL,
    CONSTRAINT FK_Orders_Users FOREIGN KEY (CustomerId) REFERENCES Users(UserId),
    CONSTRAINT FK_Orders_OrderStatuses FOREIGN KEY (OrderStatusId) REFERENCES OrderStatuses(OrderStatusId)
);
GO
CREATE TRIGGER TR_Orders_RequireCancelReason
ON Orders
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN OrderStatuses s ON s.OrderStatusId = i.OrderStatusId
        WHERE s.StatusName = 'Cancelled' AND (i.CancelReason IS NULL OR LTRIM(RTRIM(i.CancelReason)) = '')
    )
    BEGIN
        RAISERROR('CancelReason is required when Order status is Cancelled.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;
GO

CREATE TABLE OrderItems (
    OrderItemId     INT IDENTITY(1,1) PRIMARY KEY,
    OrderId         INT NOT NULL,
    ProductId       INT NOT NULL,
    Quantity        INT NOT NULL CHECK (Quantity > 0),
    UnitPrice       DECIMAL(12,2) NOT NULL,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);

CREATE INDEX IX_Orders_CustomerId ON Orders(CustomerId);
CREATE INDEX IX_Orders_OrderDate ON Orders(OrderDate);

/* =========================================================
   5. REPAIR / STRINGING SERVICE
   ========================================================= */

CREATE TABLE ServiceRequestStatuses (
    ServiceStatusId INT IDENTITY(1,1) PRIMARY KEY,
    StatusName      NVARCHAR(20) NOT NULL UNIQUE
);

CREATE TABLE ServiceTypes (
    ServiceTypeId   INT IDENTITY(1,1) PRIMARY KEY,
    TypeName        NVARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE ServiceRequests (
    ServiceRequestId    INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId          INT NOT NULL,
    RacketBrand         NVARCHAR(100),
    RacketModel         NVARCHAR(100),
    ServiceTypeId       INT NOT NULL,
    StringProductId     INT NULL,
    RequestedTension    NVARCHAR(20) NULL,
    Description         NVARCHAR(500),
    ServiceStatusId     INT NOT NULL,
    AssignedStaffId     INT NULL,
    CancelReason        NVARCHAR(255) NULL,
    Price               DECIMAL(12,2) NULL,
    RequestedDate       DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    StartedDate         DATETIME2 NULL,
    CompletedDate       DATETIME2 NULL,
    CONSTRAINT FK_ServiceRequests_Customer FOREIGN KEY (CustomerId) REFERENCES Users(UserId),
    CONSTRAINT FK_ServiceRequests_Staff FOREIGN KEY (AssignedStaffId) REFERENCES Users(UserId),
    CONSTRAINT FK_ServiceRequests_Type FOREIGN KEY (ServiceTypeId) REFERENCES ServiceTypes(ServiceTypeId),
    CONSTRAINT FK_ServiceRequests_Status FOREIGN KEY (ServiceStatusId) REFERENCES ServiceRequestStatuses(ServiceStatusId),
    CONSTRAINT FK_ServiceRequests_StringProduct FOREIGN KEY (StringProductId) REFERENCES Products(ProductId)
);

CREATE INDEX IX_ServiceRequests_Status ON ServiceRequests(ServiceStatusId);
CREATE INDEX IX_ServiceRequests_Staff ON ServiceRequests(AssignedStaffId);

CREATE TABLE ServiceStatusHistory (
    HistoryId           INT IDENTITY(1,1) PRIMARY KEY,
    ServiceRequestId    INT NOT NULL,
    OldStatusId         INT NULL,
    NewStatusId         INT NOT NULL,
    ChangedBy           INT NOT NULL,
    ChangedAt           DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    Note                NVARCHAR(255),
    CONSTRAINT FK_History_ServiceRequests FOREIGN KEY (ServiceRequestId) REFERENCES ServiceRequests(ServiceRequestId) ON DELETE CASCADE,
    CONSTRAINT FK_History_OldStatus FOREIGN KEY (OldStatusId) REFERENCES ServiceRequestStatuses(ServiceStatusId),
    CONSTRAINT FK_History_NewStatus FOREIGN KEY (NewStatusId) REFERENCES ServiceRequestStatuses(ServiceStatusId),
    CONSTRAINT FK_History_ChangedBy FOREIGN KEY (ChangedBy) REFERENCES Users(UserId)
);

GO
CREATE TRIGGER TR_ServiceRequests_RequireCancelReason
ON ServiceRequests
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN ServiceRequestStatuses s ON s.ServiceStatusId = i.ServiceStatusId
        WHERE s.StatusName = 'Cancel' AND (i.CancelReason IS NULL OR LTRIM(RTRIM(i.CancelReason)) = '')
    )
    BEGIN
        RAISERROR('CancelReason is required when ServiceRequest status is Cancel.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;
GO

/* =========================================================
   6. PAYMENTS
   ========================================================= */

CREATE TABLE Payments (
    PaymentId           INT IDENTITY(1,1) PRIMARY KEY,
    OrderId             INT NULL,
    ServiceRequestId    INT NULL,
    Amount              DECIMAL(12,2) NOT NULL,
    PaymentMethod       NVARCHAR(30) NOT NULL,
    PaymentStatus       NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    PaidAt              DATETIME2 NULL,
    CONSTRAINT FK_Payments_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    CONSTRAINT FK_Payments_ServiceRequests FOREIGN KEY (ServiceRequestId) REFERENCES ServiceRequests(ServiceRequestId),
    CONSTRAINT CK_Payments_OneTarget CHECK (
        (OrderId IS NOT NULL AND ServiceRequestId IS NULL) OR
        (OrderId IS NULL AND ServiceRequestId IS NOT NULL)
    )
);

/* =========================================================
   7. SEED LOOKUP DATA
   ========================================================= */

INSERT INTO Roles (RoleName) VALUES ('Customer'), ('Staff'), ('Admin');

INSERT INTO Categories (CategoryName, Description) VALUES
('Racket', 'Badminton rackets'),
('Shirt', 'Sports shirts'),
('Shoe', 'Court shoes'),
('String', 'Racket strings'),
('Grip', 'Handle grips');

INSERT INTO OrderStatuses (StatusName) VALUES ('Pending'), ('Paid'), ('Completed'), ('Cancelled');

INSERT INTO ServiceRequestStatuses (StatusName) VALUES ('Todo'), ('Doing'), ('Done'), ('Cancel');

INSERT INTO ServiceTypes (TypeName) VALUES ('New String'), ('Weld Frame');

/* ---- Users (1 Admin, 2 Staff, 3 Customers) ---- */

INSERT INTO Users (FullName, Username, Email, PasswordHash, Phone, Address, RoleId, IsActive) VALUES
('System Admin',   'admin',    'admin@badmintonshop.com',   'Admin@123',    '0900000001', 'Hanoi, VN',        3, 1),
('Nguyen Van Staff','staff01', 'staff01@badmintonshop.com', 'Staff@123',    '0900000002', 'Hanoi, VN',        2, 1),
('Tran Thi Staff',  'staff02', 'staff02@badmintonshop.com', 'Staff@123',    '0900000003', 'Hanoi, VN',        2, 1),
('Le Van Customer', 'cust01',  'cust01@example.com',        'Customer@123', '0900000004', 'Cau Giay, Hanoi',  1, 1),
('Pham Thi Customer','cust02', 'cust02@example.com',        'Customer@123', '0900000005', 'Dong Da, Hanoi',   1, 1),
('Hoang Van Customer','cust03','cust03@example.com',        'Customer@123', '0900000006', 'Ha Dong, Hanoi',   1, 1);

/* ---- Products + category-specific details ---- */

-- Rackets (CategoryId = 1)
INSERT INTO Products (CategoryId, ProductName, Brand, Description, Price, StockQuantity, IsActive) VALUES
(1, 'Astrox 100 ZZ', 'Yonex', 'Head-heavy power racket for attacking players', 4500000, 15, 1),
(1, 'Thruster F Claw Force', 'Victor', 'Balanced racket for all-round play', 3200000, 20, 1);

INSERT INTO RacketDetails (ProductId, Color, MaxTensionLbs, FrameWeightGrams) VALUES
(1, 'White/Red', 28.00, 83.00),
(2, 'Black/Blue', 27.00, 85.00);

-- Strings (CategoryId = 4)
INSERT INTO Products (CategoryId, ProductName, Brand, Description, Price, StockQuantity, IsActive) VALUES
(4, 'BG65', 'Yonex', 'Durable all-round string', 120000, 50, 1),
(4, 'Exbolt 63', 'Yonex', 'High repulsion string for attacking players', 180000, 30, 1);

INSERT INTO StringDetails (ProductId, Color, Durability, Repulsion, Sound, Control, ShockAbsorption) VALUES
(3, 'White', 9, 5, 6, 6, 5),
(4, 'Orange', 6, 9, 8, 7, 4);

-- Shoes (CategoryId = 3)
INSERT INTO Products (CategoryId, ProductName, Brand, Description, Price, StockQuantity, IsActive) VALUES
(3, 'Power Cushion 65 Z3', 'Yonex', 'Lightweight court shoe with good cushioning', 2100000, 25, 1),
(3, 'A960', 'Victor', 'Stable court shoe for wide court coverage', 1800000, 18, 1);

INSERT INTO ShoeDetails (ProductId, Color, Size) VALUES
(5, 'White/Blue', '42'),
(6, 'Black/Yellow', '41');

-- Shirts (CategoryId = 2)
INSERT INTO Products (CategoryId, ProductName, Brand, Description, Price, StockQuantity, IsActive) VALUES
(2, 'Team Vietnam Jersey', 'Yonex', 'Breathable competition jersey', 450000, 40, 1),
(2, 'Training Tee', 'Lining', 'Casual training shirt', 280000, 35, 1);

INSERT INTO ShirtDetails (ProductId, Color, Size, Material, SleeveType, Gender) VALUES
(7, 'Red', 'L', 'Polyester', 'Short', 'Unisex'),
(8, 'Blue', 'M', 'Dri-fit', 'Short', 'Men');

-- Grips (CategoryId = 5)
INSERT INTO Products (CategoryId, ProductName, Brand, Description, Price, StockQuantity, IsActive) VALUES
(5, 'AC102EX Towel Grip', 'Yonex', 'Absorbent towel overgrip', 35000, 100, 1),
(5, 'Super Grap', 'Yonex', 'Tacky PU overgrip', 25000, 100, 1);

INSERT INTO GripDetails (ProductId, ThicknessMm, Material, Color) VALUES
(9, 0.60, 'Towel', 'White'),
(10, 0.50, 'PU', 'Black');

/* =========================================================
   8. REPORTING VIEWS
   ========================================================= */

GO
CREATE VIEW vw_WeeklyRevenue AS
SELECT
    DATEPART(YEAR, o.OrderDate)  AS OrderYear,
    DATEPART(ISO_WEEK, o.OrderDate) AS OrderWeek,
    SUM(o.TotalAmount) AS TotalRevenue,
    COUNT(DISTINCT o.OrderId) AS OrderCount
FROM Orders o
JOIN OrderStatuses os ON os.OrderStatusId = o.OrderStatusId
WHERE os.StatusName IN ('Paid', 'Completed')
GROUP BY DATEPART(YEAR, o.OrderDate), DATEPART(ISO_WEEK, o.OrderDate);
GO

CREATE VIEW vw_WeeklyTopProducts AS
SELECT
    DATEPART(YEAR, o.OrderDate)  AS OrderYear,
    DATEPART(ISO_WEEK, o.OrderDate) AS OrderWeek,
    p.ProductId,
    p.ProductName,
    SUM(oi.Quantity) AS UnitsSold,
    SUM(oi.Quantity * oi.UnitPrice) AS Revenue
FROM OrderItems oi
JOIN Orders o ON o.OrderId = oi.OrderId
JOIN Products p ON p.ProductId = oi.ProductId
GROUP BY DATEPART(YEAR, o.OrderDate), DATEPART(ISO_WEEK, o.OrderDate), p.ProductId, p.ProductName;
GO

CREATE VIEW vw_WeeklyServiceStats AS
SELECT
    DATEPART(YEAR, sr.RequestedDate)  AS RequestYear,
    DATEPART(ISO_WEEK, sr.RequestedDate) AS RequestWeek,
    s.StatusName,
    COUNT(*) AS RequestCount
FROM ServiceRequests sr
JOIN ServiceRequestStatuses s ON s.ServiceStatusId = sr.ServiceStatusId
GROUP BY DATEPART(YEAR, sr.RequestedDate), DATEPART(ISO_WEEK, sr.RequestedDate), s.StatusName;
GO
