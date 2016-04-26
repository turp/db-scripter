# dbscripter

C# console application that will script all of the stored procedures, functions, views and triggers in a database

## Usage

Navigate to the bin directory

    DbScripter
  
will display help

    DbScripter /s DATABASE_SERVER /d DATABASE /o c:\temp

Specify the server, database and output directory

    C:\_Code\dejavu.dbscripter\bin>DbScripter.exe /s .\SQLEXPRESS /d Northwind /o c:\temp\Nwd
         dbo.Alphabetical list of products
         dbo.Category Sales for 1997
         dbo.Current Product List
         dbo.Customer and Suppliers by City
         dbo.CustOrderHist
         dbo.CustOrdersDetail
         dbo.CustOrdersOrders
         dbo.Employee Sales by Country
         dbo.Invoices
         dbo.Order Details Extended
         dbo.Order Subtotals
         dbo.Orders Qry
         dbo.Product Sales for 1997
         dbo.Products Above Average Price
         dbo.Products by Category
         dbo.Quarterly Orders
         dbo.Sales by Category
         dbo.Sales by Year
         dbo.Sales Totals by Amount
         dbo.SalesByCategory
         dbo.Summary of Sales by Quarter
         dbo.Summary of Sales by Year
         dbo.Ten Most Expensive Products
   
