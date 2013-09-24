USE [FiberDB]
GO

-- Skapar administrator konto
INSERT INTO [dbo].[User](Name, Username, Description, IsAdmin) VALUES ('Administrator', 'admin', '', true)

INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('HouseYes', 'HouseYes.png', 'HouseYes.png', 'Fastighet skall ha fiber')
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('HouseMaybe', 'HouseNotDecided.png', 'HouseNotDecided.png', 'Fastighet osäker om de skall ha fiber')
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('HouseNo', 'HouseNo.png', 'HouseNo.png', 'Fastighet skall inte ha fiber')
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('HouseNotContacted', 'HouseNotContacted.png', 'HouseNotContacted.png', 'Fastighet inte kontaktad')
-- Fastigheter som inte har fiber, visas på den publika kartan. Motsvarar HouseNo och HouseNotContacted --
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('HouseNoFiber', 'HouseNoFiber.png', 'HouseNoFiber.png', 'Fastighet skall inte ha fiber')
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('FiberNode', 'FiberNode.png', 'FiberNode.png', 'Central fibernod')
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('FiberBox', 'FiberBox.png', 'FiberBox.png', 'Kopplingsskåp')
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('RoadCrossing_Existing', 'RoadCrossing_Existing.png', 'RoadCrossing_Existing.png', 'Befintlig väggenomgång')
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('RoadCrossing_ToBeMade', 'RoadCrossing_ToBeMade.png', 'RoadCrossing_ToBeMade.png', 'Väggenomgång som skall göras')
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('Fornlamning', 'Fornlamning.png', 'Fornlamning.png', 'Fornlämning')
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('Observe', 'Observe.png', 'Observe.png', 'Känslig plats, iaktag försiktighet')
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('Note', 'Note.png', 'Note.png', 'Info-ruta')
-- Övriga okända symboler, allt som inte matchar de ovanför. --
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('Unknown', 'UnknownSymbol.png', 'UnknownSymbol.png', 'Okänd markör')
GO