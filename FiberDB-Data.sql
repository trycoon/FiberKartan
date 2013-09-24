USE [FiberDB]
GO

-- Skapar administrator konto
INSERT INTO [dbo].[User](Name, Username, Description, IsAdmin) VALUES ('Administrator', 'admin', '', true)

INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('HouseYes', 'HouseYes.png', 'HouseYes.png', 'Fastighet skall ha fiber')
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('HouseMaybe', 'HouseNotDecided.png', 'HouseNotDecided.png', 'Fastighet os�ker om de skall ha fiber')
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('HouseNo', 'HouseNo.png', 'HouseNo.png', 'Fastighet skall inte ha fiber')
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('HouseNotContacted', 'HouseNotContacted.png', 'HouseNotContacted.png', 'Fastighet inte kontaktad')
-- Fastigheter som inte har fiber, visas p� den publika kartan. Motsvarar HouseNo och HouseNotContacted --
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('HouseNoFiber', 'HouseNoFiber.png', 'HouseNoFiber.png', 'Fastighet skall inte ha fiber')
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('FiberNode', 'FiberNode.png', 'FiberNode.png', 'Central fibernod')
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('FiberBox', 'FiberBox.png', 'FiberBox.png', 'Kopplingssk�p')
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('RoadCrossing_Existing', 'RoadCrossing_Existing.png', 'RoadCrossing_Existing.png', 'Befintlig v�ggenomg�ng')
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('RoadCrossing_ToBeMade', 'RoadCrossing_ToBeMade.png', 'RoadCrossing_ToBeMade.png', 'V�ggenomg�ng som skall g�ras')
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('Fornlamning', 'Fornlamning.png', 'Fornlamning.png', 'Fornl�mning')
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('Observe', 'Observe.png', 'Observe.png', 'K�nslig plats, iaktag f�rsiktighet')
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('Note', 'Note.png', 'Note.png', 'Info-ruta')
-- �vriga ok�nda symboler, allt som inte matchar de ovanf�r. --
INSERT INTO [dbo].[MarkerType](Name, SourceIcon, DestIcon, Description) VALUES ('Unknown', 'UnknownSymbol.png', 'UnknownSymbol.png', 'Ok�nd mark�r')
GO