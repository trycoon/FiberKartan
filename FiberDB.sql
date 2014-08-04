--
-- Databas för fiberkartor. Skapad för MS SQLServer 2005 eller senare.
-- Author: Henrik Östman
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
		[LastNotificationMessage] [int] NULL,
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

--- NotificationMessage --------------------------------------------
IF NOT EXISTS
( SELECT [name] FROM sys.tables WHERE [name] = 'NotificationMessage')
BEGIN
	CREATE TABLE [dbo].[NotificationMessage](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[Title] [nvarchar](255) NOT NULL,
		[Body] [nvarchar](4000) NULL,		
		[Created] [datetime] NOT NULL DEFAULT(getdate()),
		[CreatorId] [int] NOT NULL,
	 CONSTRAINT [PK_NotificationMessage] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
	ALTER TABLE [dbo].[NotificationMessage] WITH CHECK ADD CONSTRAINT [FK_NotificationMessage_CreatorId] FOREIGN KEY([CreatorId])
	REFERENCES [dbo].[User] ([Id]) ON DELETE CASCADE
	ALTER TABLE [dbo].[NotificationMessage] CHECK CONSTRAINT [FK_NotificationMessage_CreatorId]
END

--- End NotificationMessage -----------------------------------------

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
--- End Municipality -----------------------------------------

--- MapType -----------------------------------------
IF NOT EXISTS
( SELECT [name] FROM sys.tables WHERE [name] = 'MapType')
BEGIN
	CREATE TABLE [dbo].[MapType](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[CreatorId] [int] NOT NULL,
		[Title] [nvarchar](255) NOT NULL,
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
		[Published] [datetime] NULL,
		[Layers] [nvarchar](max) NOT NULL DEFAULT '[]',
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
	ALTER TABLE [dbo].[Line]  WITH CHECK ADD CONSTRAINT [FK_Line_Map] FOREIGN KEY([MapTypeId], [MapVer])
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

--- IncidentReport -----------------------------------------
IF NOT EXISTS
( SELECT [name] FROM sys.tables WHERE [name] = 'IncidentReport')
BEGIN
	CREATE TABLE [dbo].[IncidentReport](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[MapTypeId] [int] NOT NULL,
		[MapVer] [int] NOT NULL,
		[CreatorId] [int] NOT NULL,
		[Created] [datetime] NOT NULL DEFAULT(getdate()),
		[ServiceCompanyId] [int] NOT NULL,				
		[Latitude] [float] NOT NULL,
		[Longitude] [float] NOT NULL,
		[Estate] [nvarchar](200) NULL,
		[Description] [nvarchar](2000) NULL,
		[ReportStatus] [int] NOT NULL DEFAULT 1
	CONSTRAINT [PK_IncidentReport] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
	ALTER TABLE [dbo].[IncidentReport] WITH CHECK ADD CONSTRAINT [FK_IncidentReport_Map] FOREIGN KEY([MapTypeId], [MapVer])
	REFERENCES [dbo].[Map] ([MapTypeId], [Ver]) ON DELETE CASCADE
	ALTER TABLE [dbo].[IncidentReport] CHECK CONSTRAINT [FK_IncidentReport_Map]
	ALTER TABLE [dbo].[IncidentReport] WITH CHECK ADD CONSTRAINT [FK_IncidentReport_CreatorId] FOREIGN KEY([CreatorId])
	REFERENCES [dbo].[User] ([Id])
	ALTER TABLE [dbo].[IncidentReport] CHECK CONSTRAINT [FK_IncidentReport_CreatorId]
	ALTER TABLE [dbo].[IncidentReport] WITH CHECK ADD CONSTRAINT [FK_IncidentReport_ServiceCompanyId] FOREIGN KEY([ServiceCompanyId])
	REFERENCES [dbo].[ServiceCompany] ([Id]) ON DELETE CASCADE
	ALTER TABLE [dbo].[IncidentReport] CHECK CONSTRAINT [FK_IncidentReport_ServiceCompanyId]
END
--- End IncidentReport -----------------------------------------

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
		DELETE FROM IncidentReport WHERE MapTypeId = @mapTypeId
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