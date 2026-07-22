USE BadmintonShopDB;
GO

ALTER TABLE ServiceRequests ADD CartId INT NULL;
ALTER TABLE ServiceRequests ADD CONSTRAINT FK_ServiceRequests_Cart FOREIGN KEY (CartId) REFERENCES Carts(CartId);

ALTER TABLE ServiceRequests ADD IsPaid BIT NOT NULL DEFAULT 0;

ALTER TABLE ServiceRequests ADD OrderId INT NULL;
ALTER TABLE ServiceRequests ADD CONSTRAINT FK_ServiceRequests_Order FOREIGN KEY (OrderId) REFERENCES Orders(OrderId);

-- You requested altering NewStringDetails, but that table doesn't exist.
-- Assuming you meant the RequestedTension column in ServiceRequests:
ALTER TABLE ServiceRequests ALTER COLUMN RequestedTension DECIMAL(4,1);
GO
