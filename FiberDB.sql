--
-- Databas f�r fiberkartor. Skapad f�r MS SQLServer 2005 eller senare.
-- Author: Henrik �stman
-- Created: 2011-08-11
--
USE [FiberDB]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--- User --------------------------------------------
IF NOT EXISTS
( SELECT [name] FROM sys.tables WHERE [name] = 'User')
BEGIN
	CREATE TABLE [dbo].[User](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[Name] [nvarchar](255) NOT NULL,
		[Username] [nvarchar](50) NOT NULL,
		[Password] [nvarchar](100) NULL,
		[Description] [nvarchar](4000) NULL,
		[IsDeleted] [bit] NOT NULL DEFAULT 0,
		[IsAdmin] [bit] NOT NULL DEFAULT 0,
		[Created] [datetime] NOT NULL DEFAULT(getdate()),
		[LastLoggedOn] [datetime] NULL,
		[LastActivity] [datetime] NULL,
	 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
END
IF NOT EXISTS (SELECT * FROM sysindexes
  WHERE id=object_id('User') and name='IX_User_Username')
BEGIN  
	CREATE UNIQUE NONCLUSTERED INDEX [IX_User_Username] ON [dbo].[User] 
	(
		[Username] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
END
--- End User -----------------------------------------

--- Municipality -----------------------------------------
IF NOT EXISTS
( SELECT [name] FROM sys.tables WHERE [name] = 'Municipality')
BEGIN
	CREATE TABLE [dbo].[Municipality](
		[Code] [varchar](10) NOT NULL,
		[Name] [nvarchar](50) NOT NULL,
		[Referencesystem] [varchar](50) NULL,
		[CenterLatitude] [varchar](50) NULL,
		[CenterLongitude] [varchar](50) NULL,
	 CONSTRAINT [PK_Municipality] PRIMARY KEY CLUSTERED 
	(
		[Code] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
END

PRINT 'Inserting values into [Municipality]'
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0114', N'Upplands V�sby', N'SWEREF 99 18 00', N'59.51961', N'17.928339999999935')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0115', N'Vallentuna', N'SWEREF 99 18 00', N'59.53569999999999', N'18.078017000000045')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0117', N'�ster�ker', N'SWEREF 99 18 00', N'59.47811499999999', N'18.766665999999987')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0120', N'V�rmd�', N'SWEREF 99 18 00', N'59.314449', N'18.39852810000002')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0123', N'J�rf�lla', N'SWEREF 99 18 00', N'59.410065', N'17.83680400000003')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0125', N'Eker�', NULL, N'59.28951550000001', N'17.81005289999996')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0126', N'Huddinge', N'SWEREF 99 18 00', N'59.23633', N'17.982156099999997')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0127', N'Botkyrka', N'SWEREF 99 18 00', N'59.2459411', N'17.840858300000036')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0128', N'Salem', N'SWEREF 99 18 00', N'59.2', N'17.766666699999973')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0136', N'Haninge', N'SWEREF 99 18 00', N'59.1827236', N'18.151090599999975')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0138', N'Tyres�', N'SWEREF 99 18 00', N'59.2425954', N'18.28339189999997')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0139', N'Upplands-Bro', N'SWEREF 99 18 00', N'59.5094727', N'17.614411700000005')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0140', N'Nykvarn', N'SWEREF 99 18 00', N'59.178177', N'17.427816000000007')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0160', N'T�by', N'SWEREF 99 18 00', N'59.4419', N'18.070329899999933')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0162', N'Danderyd', N'SWEREF 99 18 00', N'59.413708', N'18.045147000000043')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0163', N'Sollentuna', N'SWEREF 99 18 00', N'59.43911', N'17.941479999999956')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0180', N'Stockholm', N'SWEREF 99 18 00', N'59.32893000000001', N'18.064910000000054')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0181', N'S�dert�lje', N'SWEREF 99 18 00', N'59.19536300000001', N'17.625688999999966')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0182', N'Nacka', N'SWEREF 99 18 00', N'59.3106799', N'18.163524800000005')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0183', N'Sundbyberg', N'SWEREF 99 18 00', N'59.3670471', N'17.966309300000034')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0184', N'Solna', N'SWEREF 99 18 00', N'59.36887909999999', N'18.008433400000058')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0186', N'Liding�', N'SWEREF 99 18 00', N'59.36295999999999', N'18.146799999999985')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0187', N'Vaxholm', N'SWEREF 99 18 00', N'59.40329679999999', N'18.326380400000062')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0188', N'Norrt�lje', NULL, N'59.7595841', N'18.701358400000004')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0191', N'Sigtuna', N'SWEREF 99 18 00', N'59.6191463', N'17.7234191')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0192', N'Nyn�shamn', N'SWEREF 99 18 00', N'58.90292600000001', N'17.946528899999976')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0305', N'H�bo', N'SWEREF 99 18 00', N'59.62707139999999', N'17.452298100000007')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0319', N'�lvkarleby', N'SWEREF 99 16 30', N'60.56860669999999', N'17.448912899999982')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0330', N'Knivsta', N'SWEREF 99 18 00', N'59.7261195', N'17.79203640000003')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0331', N'Heby', N'SWEREF 99 16 30', N'59.9396372', N'16.858758800000032')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0360', N'Tierp', NULL, N'60.3458956', N'17.516906000000063')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0380', N'Uppsala', N'SWEREF 99 18 00', N'59.85856380000001', N'17.638926699999956')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0381', N'Enk�ping', N'SWEREF 99 16 30', N'59.63569090000001', N'17.077822800000035')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0382', N'�sthammar', N'SWEREF 99 18 00', N'60.25971149999999', N'18.366614700000014')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0428', N'Ving�ker', N'SWEREF 99 16 30', N'59.04647799999999', N'15.876965000000041')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0461', N'Gnesta', N'SWEREF 99 16 30', N'59.04834830000001', N'17.307227600000033')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0480', N'Nyk�ping', N'SWEREF 99 16 30', N'58.75284389999999', N'17.009159299999965')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0481', N'Oxel�sund', N'SWEREF 99 16 30', N'58.6701741', N'17.10373329999993')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0482', N'Flen', N'SWEREF 99 16 30', N'59.0579376', N'16.587912200000005')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0483', N'Katrineholm', N'SWEREF 99 16 30', N'58.99555109999999', N'16.2054756')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0484', N'Eskilstuna', N'SWEREF 99 16 30', N'59.37124859999999', N'16.509804499999973')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0486', N'Str�ngn�s', N'SWEREF 99 16 30', N'59.37745229999999', N'17.032119299999977')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0488', N'Trosa', N'SWEREF 99 18 00', N'58.8985136', N'17.551352700000052')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0509', N'�desh�g', N'SWEREF 99 15 00', N'58.22912059999999', N'14.652996099999996')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0512', N'Ydre', N'SWEREF 99 15 00', N'57.8595195', N'15.297345800000016')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0513', N'Kinda', N'SWEREF 99 15 00', N'57.9975708', N'15.68522340000004')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0560', N'Boxholm', N'SWEREF 99 15 00', N'58.19668949999999', N'15.04748810000001')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0561', N'�tvidaberg', NULL, N'58.20218980000001', N'15.997269800000026')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0562', N'Finsp�ng', N'SWEREF 99 16 30', N'58.7074884', N'15.773595')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0563', N'Valdemarsvik', N'SWEREF 99 16 30', N'58.20245139999999', N'16.601362600000016')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0580', N'Link�ping', N'SWEREF 99 15 00', N'58.41080700000001', N'15.621372699999938')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0581', N'Norrk�ping', N'SWEREF 99 16 30', N'58.587745', N'16.192420999999968')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0582', N'S�derk�ping', N'SWEREF 99 16 30', N'58.4759013', N'16.323430700000017')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0583', N'Motala', N'SWEREF 99 15 00', N'58.5380335', N'15.04709360000004')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0584', N'Vadstena', N'SWEREF 99 15 00', N'58.44760219999999', N'14.890233500000022')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0586', N'Mj�lby', N'SWEREF 99 15 00', N'58.3226908', N'15.133534800000007')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0604', N'Aneby', N'SWEREF 99 15 00', N'57.83852100000001', N'14.817802099999994')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0617', N'Gnosj�', N'SWEREF 99 13 30', N'57.35803979999999', N'13.73718980000001')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0642', N'Mullsj�', NULL, N'57.9165989', N'13.877317400000038')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0643', N'Habo', N'SWEREF 99 13 30', N'57.90930939999999', N'14.074366499999996')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0662', N'Gislaved', N'SWEREF 99 13 30', N'57.2985', N'13.543260000000032')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0665', N'Vaggeryd', N'SWEREF 99 13 30', N'57.4989621', N'14.148629700000015')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0680', N'J�nk�ping', NULL, N'57.78261370000001', N'14.161787600000025')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0682', N'N�ssj�', N'SWEREF 99 15 00', N'57.65303549999999', N'14.696724700000004')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0683', N'V�rnamo', N'SWEREF 99 13 30', N'57.1831605', N'14.047821399999975')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0684', N'S�vsj�', N'SWEREF 99 15 00', N'57.39899579999999', N'14.665813599999979')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0685', N'Vetlanda', N'SWEREF 99 15 00', N'57.42746', N'15.085329999999999')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0686', N'Eksj�', N'SWEREF 99 15 00', N'57.6651652', N'14.973221400000057')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0687', N'Tran�s', N'SWEREF 99 15 00', N'58.035518', N'14.97569599999997')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0760', N'Uppvidinge', N'SWEREF 99 15 00', N'57.16061', N'15.421352999999954')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0761', N'Lessebo', N'SWEREF 99 15 00', N'56.75126419999999', N'15.270000799999934')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0763', N'Tingsryd', N'SWEREF 99 15 00', N'56.5247452', N'14.978534200000013')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0764', N'Alvesta', N'SWEREF 99 15 00', N'56.89921039999999', N'14.556000600000061')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0765', N'�lmhult', N'SWEREF 99 13 30', N'56.5524461', N'14.137404699999934')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0767', N'Markaryd', N'SWEREF 99 13 30', N'56.4617744', N'13.59627439999997')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0780', N'V�xj�', N'SWEREF 99 15 00', N'56.8790044', N'14.805852200000004')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0781', N'Ljungby', N'SWEREF 99 13 30', N'56.8338774', N'13.941041700000028')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0821', N'H�gsby', N'SWEREF 99 16 30', N'57.17260400000001', N'16.02061900000001')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0834', N'Tors�s', NULL, N'56.4126384', N'15.998274600000059')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0840', N'M�rbyl�nga', N'SWEREF 99 16 30', N'56.509745', N'16.44603200000006')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0860', N'Hultsfred', N'SWEREF 99 16 30', N'57.49484', N'15.841651299999967')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0861', N'M�nster�s', NULL, N'57.0415797', N'16.44312000000002')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0862', N'Emmaboda', N'SWEREF 99 15 00', N'56.63068550000001', N'15.540005999999948')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0880', N'Kalmar', N'SWEREF 99 16 30', N'56.6634447', N'16.35677899999996')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0881', N'Nybro', N'SWEREF 99 16 30', N'56.7437998', N'15.908680600000025')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0882', N'Oskarshamn', N'SWEREF 99 16 30', N'57.26569929999999', N'16.447398399999997')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0883', N'V�stervik', N'SWEREF 99 16 30', N'57.75771559999999', N'16.63697590000004')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0884', N'Vimmerby', N'SWEREF 99 16 30', N'57.6690339', N'15.858856999999944')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0885', N'Borgholm', N'SWEREF 99 16 30', N'56.88022969999999', N'16.65623549999998')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'0980', N'Gotland', N'SWEREF 99 18 45', N'57.46841209999999', N'18.48674470000003')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1060', N'Olofstr�m', N'SWEREF 99 15 00', N'56.277708', N'14.530937999999992')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1080', N'Karlskrona', N'SWEREF 99 15 00', N'56.161224', N'15.586900000000014')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1081', N'Ronneby', N'SWEREF 99 15 00', N'56.210434', N'15.276022900000044')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1082', N'Karlshamn', N'SWEREF 99 15 00', N'56.170303', N'14.863072999999986')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1083', N'S�lvesborg', NULL, N'56.0537433', N'14.579687799999988')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1214', N'Sval�v', N'SWEREF 99 13 30', N'55.9129296', N'13.101816800000051')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1230', N'Staffanstorp', N'SWEREF 99 13 30', N'55.641065', N'13.21222899999998')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1231', N'Burl�v', N'SWEREF 99 13 30', N'55.633333', N'13.100000000000023')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1233', N'Vellinge', N'SWEREF 99 13 30', N'55.47089279999999', N'13.019989900000041')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1256', N'�stra G�inge', N'SWEREF 99 13 30', N'56.24406699999999', N'14.235148999999979')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1257', N'�rkelljunga', N'SWEREF 99 13 30', N'56.283635', N'13.278831999999966')
GO
print 'Processed 100 total records'
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1260', N'Bjuv', N'SWEREF 99 13 30', N'56.0871019', N'12.9125047')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1261', N'K�vlinge', N'SWEREF 99 13 30', N'55.7939999', N'13.110429299999964')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1262', N'Lomma', N'SWEREF 99 13 30', N'55.6731375', N'13.06740549999995')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1263', N'Svedala', N'SWEREF 99 13 30', N'55.5089084', N'13.237109700000019')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1264', N'Skurup', N'SWEREF 99 13 30', N'55.48045', N'13.502348999999981')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1265', N'Sj�bo', N'SWEREF 99 13 30', N'55.634822', N'13.70336999999995')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1266', N'H�rby', N'SWEREF 99 13 30', N'55.851716', N'13.661925500000052')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1267', N'H��r', N'SWEREF 99 13 30', N'55.9348588', N'13.539590399999952')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1270', N'Tomelilla', N'SWEREF 99 13 30', N'55.54355', N'13.95483999999999')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1272', N'Brom�lla', N'SWEREF 99 13 30', N'56.0743618', N'14.477659000000017')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1273', N'Osby', N'SWEREF 99 13 30', N'56.3815355', N'13.992941299999984')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1275', N'Perstorp', N'SWEREF 99 13 30', N'56.137972', N'13.394986199999948')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1276', N'Klippan', N'SWEREF 99 13 30', N'56.1348998', N'13.129040900000064')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1277', N'�storp', N'SWEREF 99 13 30', N'56.134262', N'12.945908000000031')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1278', N'B�stad', N'SWEREF 99 13 30', N'58.19668949999999', N'15.04748810000001')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1280', N'Malm�', N'SWEREF 99 13 30', N'55.604981', N'13.003822000000014')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1281', N'Lund', N'SWEREF 99 13 30', N'55.7046601', N'13.191007300000024')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1282', N'Landskrona', N'SWEREF 99 13 30', N'55.8703477', N'12.830080199999998')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1283', N'Helsingborg', N'SWEREF 99 13 30', N'56.0464674', N'12.694512099999997')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1284', N'H�gan�s', N'SWEREF 99 13 30', N'56.2006388', N'12.555328799999984')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1285', N'Esl�v', N'SWEREF 99 13 30', N'55.8391198', N'13.30339140000001')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1286', N'Ystad', N'SWEREF 99 13 30', N'55.4295051', N'13.82003080000004')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1287', N'Trelleborg', N'SWEREF 99 13 30', N'55.3762427', N'13.15742309999996')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1290', N'Kristianstad', N'SWEREF 99 13 30', N'56.0293936', N'14.156677800000011')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1291', N'Simrishamn', N'SWEREF 99 13 30', N'55.5573959', N'14.348965099999987')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1292', N'�ngelholm', N'SWEREF 99 13 30', N'56.245748', N'12.863880999999992')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1293', N'H�ssleholm', N'SWEREF 99 13 30', N'56.15891449999999', N'13.76676550000002')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1315', N'Hylte', N'SWEREF 99 13 30', N'56.968476', N'13.527671000000055')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1380', N'Halmstad', N'SWEREF 99 13 30', N'56.6743748', N'12.857788400000004')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1381', N'Laholm', N'SWEREF 99 13 30', N'56.50575569999999', N'13.045604799999978')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1382', N'Falkenberg', N'SWEREF 99 12 00', N'56.90273329999999', N'12.488801299999977')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1383', N'Varberg', N'SWEREF 99 12 00', N'57.107118', N'12.252090700000053')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1384', N'Kungsbacka', N'SWEREF 99 12 00', N'57.48749189999999', N'12.076192699999979')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1401', N'H�rryda', N'SWEREF 99 12 00', N'57.6917444', N'12.29441589999999')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1402', N'Partille', N'SWEREF 99 12 00', N'57.7366497', N'12.125196800000026')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1407', N'�cker�', N'SWEREF 99 12 00', N'57.711741', N'11.648788599999989')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1415', N'Stenungsund', N'SWEREF 99 12 00', N'58.0678388', N'11.829434600000013')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1419', N'Tj�rn', N'SWEREF 99 12 00', N'57.99809', N'11.556133999999929')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1421', N'Orust', N'SWEREF 99 12 00', N'58.18026709999999', N'11.675983900000006')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1427', N'Soten�s', N'SWEREF 99 12 00', N'58.441682', N'11.35293189999993')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1430', N'Munkedal', N'SWEREF 99 12 00', N'58.47150509999999', N'11.680192300000044')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1435', N'Tanum', N'SWEREF 99 12 00', N'58.7164707', N'11.331675000000018')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1438', N'Dals-Ed', N'SWEREF 99 12 00', N'58.92542899999999', N'11.951639999999998')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1439', N'F�rgelanda', N'SWEREF 99 12 00', N'58.567559', N'11.994925999999964')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1440', N'Ale', N'SWEREF 99 12 00', N'57.84117500000001', N'12.029249100000015')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1441', N'Lerum', N'SWEREF 99 12 00', N'57.7694839', N'12.268819699999995')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1442', N'V�rg�rda', N'SWEREF 99 13 30', N'58.0323039', N'12.808976400000006')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1443', N'Bollebygd', N'SWEREF 99 13 30', N'57.66896910000001', N'12.570143400000006')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1444', N'Gr�storp', N'SWEREF 99 13 30', N'58.3340403', N'12.680217699999957')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1445', N'Essunga', N'SWEREF 99 13 30', N'58.19501349999999', N'12.778249599999981')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1446', N'Karlsborg', N'SWEREF 99 13 30', N'58.5300853', N'14.510565499999984')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1447', N'Gullsp�ng', N'SWEREF 99 13 30', N'58.985467', N'14.096623000000022')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1452', N'Tranemo', N'SWEREF 99 13 30', N'57.4855688', N'13.352409999999963')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1460', N'Bengtsfors', N'SWEREF 99 12 00', N'59.028612', N'12.226943000000006')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1461', N'Mellerud', N'SWEREF 99 12 00', N'58.7025681', N'12.451842400000032')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1462', N'Lilla Edet', N'SWEREF 99 12 00', N'58.1344545', N'12.125908799999934')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1463', N'Mark', N'SWEREF 99 12 00', N'62.6', N'17.75')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1465', N'Svenljunga', N'SWEREF 99 13 30', N'57.4955715', N'13.114622499999996')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1466', N'Herrljunga', N'SWEREF 99 13 30', N'58.0780287', N'13.01883870000006')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1470', N'Vara', N'SWEREF 99 13 30', N'58.261781', N'12.960194000000001')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1471', N'G�tene', N'SWEREF 99 13 30', N'58.528002', N'13.491428000000042')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1472', N'Tibro', N'SWEREF 99 13 30', N'58.424323', N'14.161641799999984')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1473', N'T�reboda', N'SWEREF 99 13 30', N'58.70550919999999', N'14.126146800000015')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1480', N'G�teborg', N'SWEREF 99 12 00', N'57.70887', N'11.974559999999997')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1481', N'M�lndal', N'SWEREF 99 12 00', N'57.65', N'12.016666999999984')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1482', N'Kung�lv', N'SWEREF 99 12 00', N'57.86975400000001', N'11.974031700000069')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1484', N'Lysekil', N'SWEREF 99 12 00', N'58.27557299999999', N'11.435558000000015')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1485', N'Uddevalla', N'SWEREF 99 12 00', N'58.3498003', N'11.935649000000012')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1486', N'Str�mstad', N'SWEREF 99 12 00', N'58.9383459', N'11.179187100000036')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1487', N'V�nersborg', N'SWEREF 99 12 00', N'58.3797283', N'12.32480320000002')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1488', N'Trollh�ttan', N'SWEREF 99 12 00', N'58.2861851', N'12.299504800000022')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1489', N'Alings�s', N'SWEREF 99 12 00', N'57.9300205', N'12.53621129999999')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1490', N'Bor�s', N'SWEREF 99 13 30', N'57.72103500000001', N'12.939818999999943')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1491', N'Ulricehamn', N'SWEREF 99 13 30', N'57.79242300000001', N'13.41573500000004')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1492', N'�m�l', N'SWEREF 99 12 00', N'59.05111699999999', N'12.697732299999984')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1493', N'Mariestad', N'SWEREF 99 13 30', N'58.7101119', N'13.82133269999997')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1494', N'Lidk�ping', N'SWEREF 99 13 30', N'58.5035047', N'13.157076800000027')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1495', N'Skara', N'SWEREF 99 13 30', N'58.3860128', N'13.439328199999977')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1496', N'Sk�vde', N'SWEREF 99 13 30', N'58.3902782', N'13.846120799999994')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1497', N'Hjo', N'SWEREF 99 13 30', N'58.3070702', N'14.287466300000006')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1498', N'Tidaholm', N'SWEREF 99 13 30', N'58.1817692', N'13.95947369999999')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1499', N'Falk�ping', N'SWEREF 99 13 30', N'58.17502899999999', N'13.553217000000018')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1715', N'Kil', N'SWEREF 99 13 30', N'59.50368019999999', N'13.31704769999999')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1730', N'Eda', N'SWEREF 99 12 00', N'59.83305600000001', N'12.316667000000052')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1737', N'Torsby', N'SWEREF 99 13 30', N'60.14090719999999', N'13.010213199999953')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1760', N'Storfors', N'SWEREF 99 13 30', N'59.5332079', N'14.272208500000033')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1761', N'Hammar�', N'SWEREF 99 13 30', N'59.28930599999999', N'13.517713200000003')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1762', N'Munkfors', N'SWEREF 99 13 30', N'59.83534749999999', N'13.53441470000007')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1763', N'Forshaga', N'SWEREF 99 13 30', N'59.52976049999999', N'13.486083600000029')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1764', N'Grums', N'SWEREF 99 13 30', N'59.35317870000001', N'13.111731899999995')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1765', N'�rj�ng', N'SWEREF 99 12 00', N'59.3891592', N'12.132716999999957')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1766', N'Sunne', N'SWEREF 99 13 30', N'59.83655749999999', N'13.14404639999998')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1780', N'Karlstad', N'SWEREF 99 13 30', N'59.3791363', N'13.500804099999982')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1781', N'Kristinehamn', N'SWEREF 99 13 30', N'59.3100677', N'14.108919199999946')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1782', N'Filipstad', N'SWEREF 99 13 30', N'59.7139973', N'14.169844499999954')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1783', N'Hagfors', N'SWEREF 99 13 30', N'60.03437', N'13.694508000000042')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1784', N'Arvika', N'SWEREF 99 12 00', N'59.6548534', N'12.592135999999982')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1785', N'S�ffle', N'SWEREF 99 13 30', N'59.132661', N'12.930107000000021')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1814', N'Lekeberg', NULL, N'59.166291', N'14.64461700000004')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1860', N'Lax�', NULL, N'58.98269', N'14.622889999999984')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1861', N'Hallsberg', N'SWEREF 99 15 00', N'59.0665316', N'15.102290000000039')
GO
print 'Processed 200 total records'
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1862', N'Degerfors', N'SWEREF 99 15 00', N'59.2391027', N'14.433917899999983')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1863', N'H�llefors', N'SWEREF 99 15 00', N'59.783688', N'14.52256920000002')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1864', N'Ljusnarsberg', NULL, N'59.883333', N'14.983333000000016')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1880', N'�rebro', N'SWEREF 99 15 00', N'59.2752626', N'15.213410500000009')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1881', N'Kumla', NULL, N'59.12653590000001', N'15.14010529999996')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1882', N'Askersund', NULL, N'58.88942679999999', N'14.910986699999967')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1883', N'Karlskoga', N'SWEREF 99 15 00', N'59.32863399999999', N'14.536414000000036')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1884', N'Nora', NULL, N'59.51920639999999', N'15.037866699999995')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1885', N'Lindesberg', NULL, N'59.5976983', N'15.222910800000022')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1904', N'Skinnskatteberg', N'SWEREF 99 16 30', N'59.8317789', N'15.692722900000035')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1907', N'Surahammar', N'SWEREF 99 16 30', N'59.70722619999999', N'16.22684090000007')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1960', N'Kungs�r', NULL, N'59.422397', N'16.097786000000042')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1961', N'Hallstahammar', N'SWEREF 99 16 30', N'59.6132041', N'16.229475500000035')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1962', N'Norberg', N'SWEREF 99 16 30', N'60.06504200000001', N'15.923796000000038')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1980', N'V�ster�s', N'SWEREF 99 16 30', N'59.60990049999999', N'16.544809100000066')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1981', N'Sala', NULL, N'59.9208594', N'16.606327999999962')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1982', N'Fagersta', N'SWEREF 99 16 30', N'59.989144', N'15.81664180000007')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1983', N'K�ping', N'SWEREF 99 16 30', N'59.5120962', N'15.994510200000036')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'1984', N'Arboga', N'SWEREF 99 16 30', N'59.39368829999999', N'15.838174699999968')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2021', N'Vansbro', N'SWEREF 99 13 30', N'60.5099434', N'14.225341699999944')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2023', N'Malung', N'SWEREF 99 13 30', N'60.686372', N'13.720965999999976')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2026', N'Gagnef', N'SWEREF 99 15 00', N'60.59098299999999', N'15.070049100000006')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2029', N'Leksand', N'SWEREF 99 15 00', N'60.7303082', N'14.999892199999977')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2031', N'R�ttvik', N'SWEREF 99 15 00', N'60.889025', N'15.123373000000015')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2034', N'Orsa', N'SWEREF 99 15 00', N'61.1169365', N'14.628070799999932')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2039', N'�lvdalen', N'SWEREF 99 13 30', N'61.22730600000001', N'14.041987100000028')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2061', N'Smedjebacken', N'SWEREF 99 15 00', N'60.1431933', N'15.415991500000018')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2062', N'Mora', N'SWEREF 99 15 00', N'61.004878', N'14.537003000000027')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2080', N'Falun', N'SWEREF 99 15 45', N'60.60646000000001', N'15.635499999999979')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2081', N'Borl�nge', N'SWEREF 99 15 45', N'60.484304', N'15.433968999999934')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2082', N'S�ter', N'SWEREF 99 15 45', N'60.34665', N'15.747899999999959')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2083', N'Hedemora', N'SWEREF 99 15 45', N'60.2775453', N'15.985892000000035')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2084', N'Avesta', N'SWEREF 99 16 30', N'60.14532999999999', N'16.17383989999996')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2085', N'Ludvika', N'SWEREF 99 15 00', N'60.152358', N'15.19163900000001')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2101', N'Ockelbo', N'SWEREF 99 16 30', N'60.89178399999999', N'16.72018730000002')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2104', N'Hofors', N'SWEREF 99 16 30', N'60.54595', N'16.284099999999967')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2121', N'Ovan�ker', N'SWEREF 99 16 30', N'61.57379499999999', N'15.605375999999978')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2132', N'Nordanstig', NULL, N'62.0366675', N'17.20524899999998')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2161', N'Ljusdal', N'SWEREF 99 16 30', N'61.8308392', N'16.081750100000022')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2180', N'G�vle', N'SWEREF 99 16 30', N'60.6748796', N'17.14127259999998')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2181', N'Sandviken', N'SWEREF 99 16 30', N'60.621607', N'16.775918000000047')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2182', N'S�derhamn', N'SWEREF 99 16 30', N'61.3055762', N'17.062810200000058')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2183', N'Bolln�s', N'SWEREF 99 16 30', N'61.34837950000001', N'16.394268499999953')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2184', N'Hudiksvall', N'SWEREF 99 16 30', N'61.7273909', N'17.10740099999998')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2260', N'�nge', N'SWEREF 99 15 45', N'62.5228738', N'15.658941899999945')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2262', N'Timr�', N'SWEREF 99 17 15', N'62.4854563', N'17.32486510000001')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2280', N'H�rn�sand', N'SWEREF 99 17 15', N'62.6322698', N'17.940871399999992')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2281', N'Sundsvall', N'SWEREF 99 17 15', N'62.390811', N'17.306926999999973')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2282', N'Kramfors', N'SWEREF 99 17 15', N'62.9284332', N'17.786294699999985')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2283', N'Sollefte�', N'SWEREF 99 17 15', N'63.1654065', N'17.277135000000044')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2284', N'�rnsk�ldsvik', N'SWEREF 99 18 45', N'63.29004740000001', N'18.716616599999952')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2303', N'Ragunda', NULL, N'63.1246168', N'16.36658890000001')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2305', N'Br�cke', NULL, N'62.7507414', N'15.422574800000007')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2309', N'Krokom', NULL, N'63.3262424', N'14.44865440000001')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2313', N'Str�msund', N'SWEREF 99 15 45', N'63.85366209999999', N'15.556869099999972')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2321', N'�re', N'SWEREF 99 14 15', N'63.3990428', N'13.081505900000025')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2326', N'Berg', NULL, N'58.21247', N'16.027330000000006')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2361', N'H�rjedalen', N'SWEREF 99 14 15', N'62.12071299999999', N'13.17492500000003')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2380', N'�stersund', N'SWEREF 99 14 15', N'63.1766832', N'14.636068099999989')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2401', N'Nordmaling', N'SWEREF 99 20 15', N'63.569863', N'19.50428199999999')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2403', N'Bjurholm', N'SWEREF 99 18 45', N'63.9304056', N'19.219212500000026')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2404', N'Vindeln', N'SWEREF 99 20 15', N'64.2019528', N'19.71887019999997')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2409', N'Robertsfors', N'SWEREF 99 20 15', N'64.191835', N'20.848909999999933')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2417', N'Norsj�', N'SWEREF 99 18 45', N'64.9135093', N'19.48264130000007')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2418', N'Mal�', NULL, N'65.18512', N'18.74906999999996')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2421', N'Storuman', NULL, N'65.0956204', N'17.11227710000003')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2422', N'Sorsele', NULL, N'65.5326259', N'17.539694000000054')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2425', N'Dorotea', NULL, N'64.2617939', N'16.41523380000001')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2460', N'V�nn�s', N'SWEREF 99 20 15', N'63.908007', N'19.752965000000017')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2462', N'Vilhelmina', NULL, N'64.624471', N'16.65549699999997')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2463', N'�sele', N'SWEREF 99 17 15', N'64.1602339', N'17.35361499999999')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2480', N'Ume�', N'SWEREF 99 20 15', N'63.8258471', N'20.263035400000035')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2481', N'Lycksele', N'SWEREF 99 18 45', N'64.5958098', N'18.676367000000027')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2482', N'Skellefte�', N'SWEREF 99 20 15', N'64.750244', N'20.950917000000004')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2505', N'Arvidsjaur', N'SWEREF 99 18 45', N'65.5920768', N'19.180282799999986')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2506', N'Arjeplog', N'SWEREF 99 17 15', N'66.0515051', N'17.890054299999974')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2510', N'Jokkmokk', NULL, N'66.60696089999999', N'19.822920599999975')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2513', N'�verkalix', NULL, N'66.3271757', N'22.84275230000003')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2514', N'Kalix', N'SWEREF 99 23 15', N'65.8552807', N'23.14396499999998')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2518', N'�vertorne�', N'SWEREF 99 23 15', N'66.38972480000001', N'23.649496399999975')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2521', N'Pajala', NULL, N'67.21278199999999', N'23.367391999999995')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2523', N'G�llivare', NULL, N'67.1379', N'20.659361799999942')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2560', N'�lvsbyn', N'SWEREF 99 21 45', N'65.6771363', N'20.99286589999997')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2580', N'Lule�', N'SWEREF 99 21 45', N'65.58388959999999', N'22.153173599999946')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2581', N'Pite�', N'SWEREF 99 21 45', N'65.316698', N'21.480036400000017')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2582', N'Boden', N'SWEREF 99 21 45', N'65.8251188', N'21.688702799999987')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2583', N'Haparanda', N'SWEREF 99 23 15', N'65.84178589999999', N'24.127615600000013')
INSERT [dbo].[Municipality] ([Code], [Name], [Referencesystem], [CenterLatitude], [CenterLongitude]) VALUES (N'2584', N'Kiruna', N'SWEREF 99 20 15', N'67.8557995', N'20.22528209999996')
PRINT 'Done'
--- End Municipality -----------------------------------------

--- MapType -----------------------------------------
IF NOT EXISTS
( SELECT [name] FROM sys.tables WHERE [name] = 'MapType')
BEGIN
	CREATE TABLE [dbo].[MapType](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[CreatorId] [int] NOT NULL,
		[Title] [nvarchar](255) NOT NULL,
		[MapUrl] [nvarchar](255) NOT NULL DEFAULT '',
		[WelcomeMessage] [nvarchar](max) NULL,
		[ViewSettings] [int] NOT NULL DEFAULT 7,	-- Bitmask: 0(LSB)=Public visible, 1=Show palette, 2=Show connection statistics, 3=Show total dig length, 4=Allow view aggregated maps, 5=Only show HouseYes on public map
		[MunicipalityCode] [varchar](10) NOT NULL DEFAULT '0980',
		[ServiceCompanyId] [int] NULL
	 CONSTRAINT [PK_MapType] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
	ALTER TABLE [dbo].[MapType] WITH CHECK ADD CONSTRAINT [FK_MapType_UserId] FOREIGN KEY([CreatorId])
	REFERENCES [dbo].[User] ([Id]) ON DELETE CASCADE
	ALTER TABLE [dbo].[MapType] CHECK CONSTRAINT [FK_MapType_UserId]
	ALTER TABLE [dbo].[MapType] WITH CHECK ADD CONSTRAINT [FK_MapType_Municipality] FOREIGN KEY([MunicipalityCode])
	REFERENCES [dbo].[Municipality] ([Code]) ON DELETE NO ACTION
	ALTER TABLE [dbo].[MapType] CHECK CONSTRAINT [FK_MapType_Municipality]
	ALTER TABLE [dbo].[MapType] WITH CHECK ADD CONSTRAINT [FK_MapType_ServiceCompany] FOREIGN KEY([ServiceCompanyId])
	REFERENCES [dbo].[ServiceCompany] ([Id]) ON DELETE NO ACTION
	ALTER TABLE [dbo].[MapType] CHECK CONSTRAINT [FK_MapType_ServiceCompany]
END
--- End MapType -----------------------------------------

--- MapFiles -----------------------------------------
IF NOT EXISTS
( SELECT [name] FROM sys.tables WHERE [name] = 'MapFiles')
BEGIN
	CREATE TABLE [dbo].[MapFiles](
		[Id] [uniqueidentifier] NOT NULL DEFAULT(NEWID()),
		[MapTypeId] [int] NOT NULL,
		[CreatorId] [int] NOT NULL,
		[Created] [datetime] NOT NULL DEFAULT(getdate()),
		[MapData] [varbinary](max) NOT NULL,
	CONSTRAINT [PK_MapFiles] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
	ALTER TABLE [dbo].[MapFiles]  WITH CHECK ADD  CONSTRAINT [FK_MapFiles_CreatorId] FOREIGN KEY([CreatorId])
	REFERENCES [dbo].[User] ([Id]) ON DELETE NO ACTION
	ALTER TABLE [dbo].[MapFiles] CHECK CONSTRAINT [FK_MapFiles_CreatorId]
	ALTER TABLE [dbo].[MapFiles]  WITH CHECK ADD  CONSTRAINT [FK_MapFiles_MapTypeId] FOREIGN KEY([MapTypeId])
	REFERENCES [dbo].[MapType] ([Id]) ON DELETE CASCADE
	ALTER TABLE [dbo].[MapFiles] CHECK CONSTRAINT [FK_MapFiles_MapTypeId]
END
IF NOT EXISTS (SELECT * FROM sysindexes
  WHERE id=object_id('MapFiles') and name='IX_MapFiles_MapTypeId')
BEGIN  
	CREATE NONCLUSTERED INDEX [IX_MapFiles_MapTypeId] ON [dbo].[MapFiles] 
	(
		[MapTypeId] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
END
--- End MapFiles -----------------------------------------

--- MapTypeAccessRight -----------------------------------------
IF NOT EXISTS
( SELECT [name] FROM sys.tables WHERE [name] = 'MapTypeAccessRight')
BEGIN
	CREATE TABLE [dbo].[MapTypeAccessRight](
		[UserId] [int] NOT NULL,
		[MapTypeId] [int] NOT NULL,
		[AccessRight] [int] NOT NULL,	-- Bitmask: 0(LSB)=Read, 1=Export, 2=Write, 3=Invite others
		[EmailSubscribeChanges] [bit] NOT NULL DEFAULT 0,
		 CONSTRAINT [PK_MapTypeAccessRight] PRIMARY KEY CLUSTERED 
			(
				[UserId] ASC,
				[MapTypeId] ASC
			)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
			) ON [PRIMARY]
	ALTER TABLE [dbo].[MapTypeAccessRight]  WITH CHECK ADD  CONSTRAINT [FK_MapTypeAccessRight_MapTypeId] FOREIGN KEY([MapTypeId])
	REFERENCES [dbo].[MapType] ([Id])
	ALTER TABLE [dbo].[MapTypeAccessRight] CHECK CONSTRAINT [FK_MapTypeAccessRight_MapTypeId]
	ALTER TABLE [dbo].[MapTypeAccessRight]  WITH CHECK ADD  CONSTRAINT [FK_MapTypeAccessRight_UserId] FOREIGN KEY([UserId])
	REFERENCES [dbo].[User] ([Id])
	ALTER TABLE [dbo].[MapTypeAccessRight] CHECK CONSTRAINT [FK_MapTypeAccessRight_UserId]
END
--- End MapTypeAccessRight -----------------------------------------

--- MapAccessInvitation --------------------------------------------
IF NOT EXISTS
( SELECT [name] FROM sys.tables WHERE [name] = 'MapAccessInvitation')
BEGIN
	CREATE TABLE [dbo].[MapAccessInvitation](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[MapTypeId] [int] NOT NULL,
		[AccessRight] [int] NOT NULL,	-- Bitmask: 0(LSB)=Read, 1=Export, 2=Write, 3=Invite others
		[Email] [nvarchar](100) NOT NULL,
		[InvitationCode] [nvarchar](200) NOT NULL,
		[InvitationSent] [datetime] NOT NULL DEFAULT(getdate()),
		[InvitationSentBy] [int] NOT NULL,
	 CONSTRAINT [PK_MapAccessInvitation] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
	ALTER TABLE [dbo].[MapAccessInvitation]  WITH CHECK ADD  CONSTRAINT [FK_MapAccessInvitation_InvitationSentBy] FOREIGN KEY([InvitationSentBy])
	REFERENCES [dbo].[User] ([Id])
	ALTER TABLE [dbo].[MapAccessInvitation]  WITH CHECK ADD  CONSTRAINT [FK_MapAccessInvitation_MapTypeId] FOREIGN KEY([MapTypeId])
	REFERENCES [dbo].[MapType] ([Id])
END
IF NOT EXISTS (SELECT * FROM sysindexes
  WHERE id=object_id('MapAccessInvitation') and name='IX_MapAccessInvitation_InvitationCode')
BEGIN  
	CREATE UNIQUE NONCLUSTERED INDEX [IX_MapAccessInvitation_InvitationCode] ON [dbo].[MapAccessInvitation] 
	(
		[InvitationCode] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
END
--- End MapAccessInvitation -----------------------------------------

--- Map -----------------------------------------
IF NOT EXISTS
( SELECT [name] FROM sys.tables WHERE [name] = 'Map')
BEGIN
	CREATE TABLE [dbo].[Map](
		[MapTypeId] [int] NOT NULL,
		[Ver] [int] NOT NULL DEFAULT 1,
		[PreviousVer] [int] NULL,
		[SourceKML] [nvarchar](max) NOT NULL,
		[KML_Hash] [nvarchar](60) NOT NULL,
		[Created] [datetime] NOT NULL DEFAULT(getdate()),
		[CreatorId] [int] NOT NULL,
		[Views] [int] NOT NULL DEFAULT 0,
	 CONSTRAINT [PK_Map] PRIMARY KEY CLUSTERED 
	(
		[MapTypeId] ASC,
		[Ver] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
	ALTER TABLE [dbo].[Map]  WITH CHECK ADD  CONSTRAINT [FK_Map_MapTypeId] FOREIGN KEY([MapTypeId])
	REFERENCES [dbo].[MapType] ([Id]) ON DELETE CASCADE
	ALTER TABLE [dbo].[Map] CHECK CONSTRAINT [FK_Map_MapTypeId]
	ALTER TABLE [dbo].[Map]  WITH CHECK ADD  CONSTRAINT [FK_Map_UserId] FOREIGN KEY([CreatorId])
	REFERENCES [dbo].[User] ([Id]) ON DELETE NO ACTION 
	ALTER TABLE [dbo].[Map] CHECK CONSTRAINT [FK_Map_UserId]
END
--- End Map -----------------------------------------

--- MarkerType -----------------------------------------
IF NOT EXISTS
( SELECT [name] FROM sys.tables WHERE [name] = 'MarkerType')
BEGIN
	CREATE TABLE [dbo].[MarkerType](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[Name] [nvarchar](50) NOT NULL,
		[SourceIcon] [nvarchar](255) NOT NULL,
		[DestIcon] [nvarchar](255) NOT NULL,
		[Description] [nvarchar](50) NOT NULL DEFAULT '',
	 CONSTRAINT [PK_MarkerType] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
END
--- End MarkerType -----------------------------------------

--- Marker -----------------------------------------
IF NOT EXISTS
( SELECT [name] FROM sys.tables WHERE [name] = 'Marker')
BEGIN
	CREATE TABLE [dbo].[Marker](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[Uid] [int] NOT NULL DEFAULT 0,
		[MapTypeId] [int] NOT NULL,
		[MapVer] [int] NOT NULL,
		[MarkerTypeId] [int] NOT NULL,
		[Name] [nvarchar](512) NULL,
		[Description] [nvarchar](max) NULL,
		[Latitude] [float] NOT NULL,
		[Longitude] [float] NOT NULL,
		[Settings] [int] NOT NULL DEFAULT 0,
		[OptionalInfo] [nvarchar](max) NOT NULL DEFAULT '{}',
	 CONSTRAINT [PK_Marker] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC,
		[MapTypeId] ASC,
		[MapVer] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
	ALTER TABLE [dbo].[Marker]  WITH CHECK ADD  CONSTRAINT [FK_Marker_MarkerType] FOREIGN KEY([MarkerTypeId])
	REFERENCES [dbo].[MarkerType] ([Id]) ON DELETE CASCADE
	ALTER TABLE [dbo].[Marker] CHECK CONSTRAINT [FK_Marker_MarkerType]
	ALTER TABLE [dbo].[Marker]  WITH CHECK ADD  CONSTRAINT [FK_Marker_Map] FOREIGN KEY([MapTypeId], [MapVer])
	REFERENCES [dbo].[Map] ([MapTypeId], [Ver])
	ALTER TABLE [dbo].[Marker] CHECK CONSTRAINT [FK_Marker_Map]
END
IF NOT EXISTS (SELECT * FROM sysindexes
  WHERE id=object_id('Marker') and name='IX_Marker_Uid')
BEGIN  
	CREATE NONCLUSTERED INDEX [IX_Marker_Uid] ON [dbo].[Marker] 
	(
		[Uid] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
END
--- End Marker -----------------------------------------

--- Region -----------------------------------------
IF NOT EXISTS
( SELECT [name] FROM sys.tables WHERE [name] = 'Region')
BEGIN
	CREATE TABLE [dbo].[Region](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[Uid] [int] NOT NULL DEFAULT 0,
		[MapTypeId] [int] NOT NULL,
		[MapVer] [int] NOT NULL,		
		[Name] [nvarchar](512) NULL,
		[Description] [nvarchar](max) NULL,
		[FillColor] [varchar](8) NOT NULL,
		[LineColor] [varchar](8) NOT NULL,
		[Coordinates] [varchar](Max) NOT NULL,
	 CONSTRAINT [PK_Region] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC,
		[MapTypeId] ASC,
		[MapVer] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
	ALTER TABLE [dbo].[Region]  WITH CHECK ADD  CONSTRAINT [FK_Region_Map] FOREIGN KEY([MapTypeId], [MapVer])
	REFERENCES [dbo].[Map] ([MapTypeId], [Ver]) ON DELETE CASCADE
	ALTER TABLE [dbo].[Region] CHECK CONSTRAINT [FK_Region_Map]
END
IF NOT EXISTS (SELECT * FROM sysindexes
  WHERE id=object_id('Region') and name='IX_Region_Uid')
BEGIN  
	CREATE NONCLUSTERED INDEX [IX_Region_Uid] ON [dbo].[Region] 
	(
		[Uid] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
END
--- End Region -----------------------------------------

--- Line -----------------------------------------
IF NOT EXISTS
( SELECT [name] FROM sys.tables WHERE [name] = 'Line')
BEGIN
	CREATE TABLE [dbo].[Line](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[Uid] [int] NOT NULL DEFAULT 0,
		[MapTypeId] [int] NOT NULL,
		[MapVer] [int] NOT NULL,
		[Name] [nvarchar](512) NULL,
		[Description] [nvarchar](max) NULL,
		[LineColor] [varchar](8) NOT NULL,
		[Width] [int] NOT NULL,
		[Coordinates] [varchar](Max) NOT NULL,
		[Type] [int] NOT NULL DEFAULT 0,
	 CONSTRAINT [PK_Line] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC,
		[MapTypeId] ASC,
		[MapVer] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
	ALTER TABLE [dbo].[Line]  WITH CHECK ADD  CONSTRAINT [FK_Line_Map] FOREIGN KEY([MapTypeId], [MapVer])
	REFERENCES [dbo].[Map] ([MapTypeId], [Ver]) ON DELETE CASCADE
	ALTER TABLE [dbo].[Line] CHECK CONSTRAINT [FK_Line_Map]
END	
IF NOT EXISTS (SELECT * FROM sysindexes
  WHERE id=object_id('Line') and name='IX_Line_Uid')
BEGIN  
	CREATE NONCLUSTERED INDEX [IX_Line_Uid] ON [dbo].[Line] 
	(
		[Uid] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
END
--- End Line -----------------------------------------

--- ImportErrors -----------------------------------------
IF NOT EXISTS
( SELECT [name] FROM sys.tables WHERE [name] = 'ImportErrors')
BEGIN
	CREATE TABLE [dbo].[ImportErrors](
		[UserId] [int] NOT NULL,
		[MapTypeId] [int] NOT NULL,
		[Date] [datetime] NOT NULL,
		[MergeVersion] [int] NOT NULL DEFAULT 0,
		[KML] [nvarchar](max) NOT NULL,
		[ErrorMessage] [nvarchar](max) NOT NULL,
		[StackTrace] [nvarchar](max) NULL,	
	CONSTRAINT [PK_ImportErrors] PRIMARY KEY CLUSTERED 
	(
		[UserId] ASC,
		[MapTypeId] ASC,
		[Date] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
END	
--- End ImportErrors -----------------------------------------

--- ServiceCompany -----------------------------------------
IF NOT EXISTS
( SELECT [name] FROM sys.tables WHERE [name] = 'ServiceCompany')
BEGIN
	CREATE TABLE [dbo].[ServiceCompany](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[Name] [nvarchar](50) NOT NULL,
		[Address] [nvarchar](255) NULL,
		[Description] [nvarchar](2000) NULL,
		[ServiceEmail] [nvarchar](50) NOT NULL,
		[ContactPersonName] [nvarchar](255) NOT NULL,
		[ContactPersonEmail] [nvarchar](50) NOT NULL,
		[ContactPersonPhone] [nvarchar](30) NULL		
	CONSTRAINT [PK_ServiceCompany] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]	
END
--- End ServiceCompany -----------------------------------------

GO

-- ====================================
-- Stored Procedure - DeleteMapType
-- Removes a maptype much faster and simpler than with Linq2Sql.
-- ====================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteMapType]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DeleteMapType]
GO
CREATE PROCEDURE [dbo].[DeleteMapType] 
	@mapTypeId int
AS
BEGIN
	SET NOCOUNT ON;	
	BEGIN TRANSACTION	

		DELETE FROM Marker WHERE MapTypeId = @mapTypeId
		DELETE FROM Line WHERE MapTypeId = @mapTypeId
		DELETE FROM Region WHERE MapTypeId = @mapTypeId
		DELETE FROM MapTypeAccessRight WHERE MapTypeId = @mapTypeId
		DELETE FROM MapAccessInvitation WHERE MapTypeId = @mapTypeId
		DELETE FROM Map WHERE MapTypeId = @mapTypeId
		DELETE FROM MapType WHERE Id = @mapTypeId
	
	IF @@Error <> 0
		ROLLBACK TRANSACTION
	ELSE
		COMMIT TRANSACTION
END
GO

-- ====================================
-- Stored Procedure - GetRegionMaps
-- Get the latest version of all maps that are allowed to be viewed (Public and Allow view aggregated maps are both set).
-- One can specify the @code parameter to get only maps that belongs to a specific municipality or null to get all maps.
-- ====================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetRegionMaps]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetRegionMaps]
GO
CREATE PROCEDURE [dbo].[GetRegionMaps]
	@code [varchar](10)
AS
BEGIN
	SET NOCOUNT ON;	
	SELECT [t1].[Id] as Id, [t1].[ViewSettings] as ViewSettings, [t2].[Ver] AS Ver
	FROM (
		SELECT [Id], [ViewSettings]
		FROM [FiberDB].[dbo].[MapType]
		WHERE (
			[ViewSettings] & 1 = 1 AND 
			[ViewSettings] & 16 = 16 AND
			(@code IS NULL OR [MunicipalityCode] = @code)
		)
	) AS [t1]
	CROSS APPLY
			(
				SELECT MAX([t2].[Ver]) as Ver
				FROM [dbo].[Map] AS [t2]
				WHERE [t2].[MapTypeId] = [t1].[Id]
			) AS [t2]
END
GO

-- ====================================
-- Marker_Autoassign_Uid
-- Trigger to set Uid for Markers that has none. If Marker is inserted or updated with Uid = 0, set Uid to same as Id. This to set an unique Uid for every Marker.
-- ====================================
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[Marker_Autoassign_Uid]'))
DROP TRIGGER [dbo].[Marker_Autoassign_Uid]
GO
CREATE TRIGGER Marker_Autoassign_Uid
ON [dbo].[Marker]
AFTER INSERT, UPDATE
AS
BEGIN
	UPDATE Marker SET Marker.[Uid] = Marker.[Id]
	FROM INSERTED
	WHERE inserted.[Id] = Marker.[Id] AND inserted.[Uid] = 0
END
GO

-- ====================================
-- Line_Autoassign_Uid
-- Trigger to set Uid for Lines that has none. If Line is inserted or updated with Uid = 0, set Uid to same as Id. This to set an unique Uid for every Line.
-- ====================================
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[Line_Autoassign_Uid]'))
DROP TRIGGER [dbo].[Line_Autoassign_Uid]
GO
CREATE TRIGGER Line_Autoassign_Uid
ON [dbo].[Line]
AFTER INSERT, UPDATE
AS
BEGIN
	UPDATE Line SET Line.[Uid] = Line.[Id]
	FROM INSERTED
	WHERE inserted.[Id] = Line.[Id] AND inserted.[Uid] = 0
END
GO

-- ====================================
-- Region_Autoassign_Uid
-- Trigger to set Uid for Regions that has none. If Region is inserted or updated with Uid = 0, set Uid to same as Id. This to set an unique Uid for every Region.
-- ====================================
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[Region_Autoassign_Uid]'))
DROP TRIGGER [dbo].[Region_Autoassign_Uid]
GO
CREATE TRIGGER Region_Autoassign_Uid
ON [dbo].[Region]
AFTER INSERT, UPDATE
AS
BEGIN
	UPDATE Region SET Region.[Uid] = Region.[Id]
	FROM INSERTED
	WHERE inserted.[Id] = Region.[Id] AND inserted.[Uid] = 0
END
GO