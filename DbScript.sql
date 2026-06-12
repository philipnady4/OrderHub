
CREATE TABLE Schools (
    Id     INT IDENTITY(1,1) NOT NULL,
    Name   VARCHAR(200)      NOT NULL,
    Region VARCHAR(50)       NULL,        

    CONSTRAINT PK_Schools PRIMARY KEY CLUSTERED (Id)
);


CREATE TABLE Products (
    Id       INT IDENTITY(1,1) NOT NULL,
    Sku      VARCHAR(50)       NOT NULL,
    Name     VARCHAR(200)      NOT NULL,
    Category VARCHAR(50)       NOT NULL,
    BasePrice DECIMAL(10,2)    NOT NULL,

    CONSTRAINT PK_Products PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Products_Sku UNIQUE (Sku)
);
CREATE INDEX IX_Products_Category ON Products (Category) INCLUDE (Id, Name);


CREATE TABLE Orders (
    Id        INT IDENTITY(1,1) NOT NULL,
    SchoolId  INT               NOT NULL,
    OrderDate DATE              NOT NULL,
    Season    VARCHAR(20)       NOT NULL,  -- e.g. 'Back-to-School', 'Winter'
    Total     DECIMAL(12,2)     NOT NULL,
    Status    VARCHAR(20)       NOT NULL DEFAULT 'COMPLETE',

    CONSTRAINT PK_Orders PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Orders_Schools FOREIGN KEY (SchoolId) REFERENCES Schools (Id)
);
-- Composite index supporting the date-range + school grouping
CREATE INDEX IX_Orders_SchoolId_OrderDate ON Orders (SchoolId, OrderDate)
    INCLUDE (Season, Total);


CREATE TABLE OrderLines (
    Id         INT IDENTITY(1,1) NOT NULL,
    OrderId    INT               NOT NULL,
    ProductId  INT               NOT NULL,
    Quantity   INT               NOT NULL,
    LineTotal  DECIMAL(12,2)     NOT NULL,  

    CONSTRAINT PK_OrderLines PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_OrderLines_Orders   FOREIGN KEY (OrderId)   REFERENCES Orders (Id),
    CONSTRAINT FK_OrderLines_Products FOREIGN KEY (ProductId) REFERENCES Products (Id)
);

-- Trade-off: a nonclustered COLUMNSTORE index on the analytics columns.
-- This table is write-heavy during checkout but read-heavy (full scans/aggregates)
-- on the analytics page. NCCI gives huge compression + batch-mode aggregation
-- for the reporting query without touching the OLTP clustered index used by ProcessOrder.
CREATE NONCLUSTERED COLUMNSTORE INDEX NCCI_OrderLines_Analytics
ON OrderLines (OrderId, ProductId, Quantity, LineTotal);

-- Standard rowstore index to keep OLTP joins (e.g. "lines for this order") fast
CREATE INDEX IX_OrderLines_OrderId_ProductId ON OrderLines (OrderId, ProductId);



-------------------------------------------------------------------------


--year-on-year revenue--------
WITH yearly AS (
    SELECT
        s.Id        AS SchoolId,
        s.Name      AS SchoolName,
        o.Season,
        p.Category,
        YEAR(o.OrderDate) AS OrderYear,   

        SUM(ol.LineTotal) AS Revenue
    FROM OrderLines ol
    JOIN Orders   o ON o.Id = ol.OrderId
    JOIN Schools  s ON s.Id = o.SchoolId
    JOIN Products p ON p.Id = ol.ProductId
    -- Trade-off: filter on a large date RANGE (last 2 full years),
    -- not a function over OrderDate, so IX_Orders_SchoolId_OrderDate / NCCI
    -- can actually be used for elimination instead of a full scan.
    WHERE o.OrderDate >= DATEADD(YEAR, -1, DATEFROMPARTS(YEAR(GETDATE()), 1, 1))
      AND o.Status = 'COMPLETE'
    GROUP BY s.Id, s.Name, o.Season, p.Category, YEAR(o.OrderDate)
)
SELECT
    SchoolName,
    Season,
    Category,
    OrderYear,
    Revenue,
    -- Trade-off: LAG() over a window instead of a self-join to "last year".
    -- Cheaper than a second pass over 8M rows, at the cost of needing
    -- a deterministic ORDER BY per partition (school/season/category).
    LAG(Revenue) OVER (
        PARTITION BY SchoolName, Season, Category
        ORDER BY OrderYear
    ) AS PriorYearRevenue,
    Revenue - LAG(Revenue) OVER (
        PARTITION BY SchoolName, Season, Category
        ORDER BY OrderYear
    ) AS YoYDelta
FROM yearly
ORDER BY SchoolName, Season, Category, OrderYear;



-----------------------------------------
--The biggest risk in the current setup is that OrderLines.ProductId doesn't exist — the join to Products is done via Sku (a VARCHAR). String joins across an 8M-row table are more expensive than int joins 

--Recommended:
--Add ProductId INT to OrderLines, and use it for the joins.
