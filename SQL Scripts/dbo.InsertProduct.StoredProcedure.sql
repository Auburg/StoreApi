USE [MMTShop]
GO

/****** Object:  StoredProcedure [dbo].[InsertProduct]    Script Date: 22/01/2021 14:19:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[InsertProduct]
	-- Add the parameters for the stored procedure here
	@SKU NVARCHAR(10),
    @Name NVARCHAR(50),
	@Description varchar(max) = null,
	@Price money,
	@Id int Out
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @sql as NVARCHAR(100);		
	
	DECLARE @FirstSkuCharVariable NVARCHAR(1);  
	DECLARE @ParmDefinition NVARCHAR(500);  
	
	set @FirstSkuCharVariable = LEFT(@SKU, 1);

	if ((SELECT Id  FROM Category WHERE Id LIKE @FirstSkuCharVariable + '%') is NULL)
	begin
		SELECT @Id = -1
	end 
	else
	begin		
		  INSERT INTO Product (SKU, Name, Description, Price)
		  VALUES (@SKU, @Name, @Description, @Price);
		  SELECT @Id = SCOPE_IDENTITY()
	end
END
GO

