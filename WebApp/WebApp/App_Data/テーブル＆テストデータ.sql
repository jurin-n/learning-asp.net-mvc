/*
Windows認証でSQL Serverにログインして行う作業
*/
--SQL Server DeveloperのログインモードをSQL Server認証に変更
https://docs.microsoft.com/ja-jp/sql/database-engine/configure-windows/change-server-authentication-mode?view=sql-server-ver15

--ユーザ作成
USE master
CREATE LOGIN user01 WITH PASSWORD = 'user01password';
CREATE USER user01 FOR LOGIN user01;  
GO 

--ロール付与
EXEC master..sp_addsrvrolemember @loginame = N'user01', @rolename = N'sysadmin'
GO


/*
user01ユーザでSQL Server認証して行う作業
*/
--テーブル作成
CREATE TABLE [dbo].[Users](
    [UserID] [nvarchar](32) NOT NULL,  
    [Name] [nvarchar](50) NULL,  
    [Password] [nvarchar](256) NULL,  
    [CreatedOn] [datetime] NOT NULL
CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED   
(  
[UserID] ASC  
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY]  
GO

--データ登録
INSERT INTO Users (UserId,Name,Password,CreatedOn) Values('test-user','テストユーザ','test-password','2020-07-24');


/*

*/
CREATE TABLE [dbo].[Items](
    [ItemID] [nvarchar](32) NOT NULL,  
    [Name] [nvarchar](50) NULL,  
    [Description] [nvarchar](512) NULL,
    [Type] [nvarchar](16) NULL,
    [CreatedOn] [datetime] NOT NULL
CONSTRAINT [PK_Item] PRIMARY KEY CLUSTERED   
(  
[ItemID] ASC  
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY]  
  
GO

CREATE TABLE [dbo].[Orders](
    [OrderID] [nvarchar](32) NOT NULL,
    [CreatedOn] [datetime] NOT NULL
CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED   
(  
[OrderID] ASC  
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY]  

GO

CREATE TABLE [dbo].[Orders_Items](
    [OrderID] [nvarchar](32) NOT NULL,
    [ItemID] [nvarchar](32) NOT NULL,
    [No] [int] NOT NULL
CONSTRAINT [PK_Orders_Items] PRIMARY KEY CLUSTERED   
(  
[OrderID] ASC,
[ItemID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY]  

GO
/*
ALTER TABLE [Orders_Items]
   ADD CONSTRAINT FK_Orders_Items_01 FOREIGN KEY (OrderID)
      REFERENCES [Orders] (OrderID)
GO
ALTER TABLE [Orders_Items]
   ADD CONSTRAINT FK_Orders_Items_02 FOREIGN KEY (ItemID)
      REFERENCES [Items] (ItemID)
GO
*/


--Items
INSERT INTO [Items](ItemID,Name,Description,CreatedOn,Type) VALUES('ID001','アイテムあああ(更新)','aaa<!--CRLF-->更新<!--CRLF-->更新2','2020-07-25 00:00:00.000','NUMBER');
INSERT INTO [Items](ItemID,Name,Description,CreatedOn,Type) VALUES('ID002','アイテム００２','説明０１<!--CRLF-->説明０２<!--CRLF-->説明０３<!--CRLF-->説明０４<!--CRLF-->説明０５','2020-07-24 00:00:00.000','NUMBER');
INSERT INTO [Items](ItemID,Name,Description,CreatedOn,Type) VALUES('ID003','アイテム００３','あ<!--CRLF-->い<!--CRLF-->う','2020-07-24 00:00:00.000','NUMBER');
INSERT INTO [Items](ItemID,Name,Description,CreatedOn,Type) VALUES('ID004','アイテム４４４','あああ','2020-07-24 00:00:00.000','NVARCHAR');
INSERT INTO [Items](ItemID,Name,Description,CreatedOn,Type) VALUES('ID005','アイテム５５５','あああ','2020-07-24 00:00:00.000','NVARCHAR');

UPDATE [Items]
SET Description = REPLACE(Description,'<!--CRLF-->',CHAR(13)+CHAR(10))
WHERE Description LIKE '%<!--CRLF-->%'
;

--Orders
INSERT INTO [Orders](OrderID,CreatedOn) VALUES('ORDER001','2020-07-24 00:00:00.000');


--Orders_Items
INSERT INTO [Orders_Items](OrderID,ItemID,No) VALUES('ORDER001','ID001',1);
INSERT INTO [Orders_Items](OrderID,ItemID,No) VALUES('ORDER001','ID002',2);
INSERT INTO [Orders_Items](OrderID,ItemID,No) VALUES('ORDER001','ID003',3);
INSERT INTO [Orders_Items](OrderID,ItemID,No) VALUES('ORDER001','ID004',4);
INSERT INTO [Orders_Items](OrderID,ItemID,No) VALUES('ORDER001','ID005',5);


