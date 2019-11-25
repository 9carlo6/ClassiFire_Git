# ClassiFire
ClassiFire: un Sistema per la rilevazione di fuoco e fumo da frame video

Per far funzionare nella maniera corretta la soluzione deve essere affiancata dal Database adatto.
Di seguito viene mostrata la procedura per la creazione del database.

In Visual Studio andare su Visualizza -> Esplora Server -> Connetti a database:
-	Selezionare origine dati: ‘Microsoft SQL Server (SqlClient)’
-	Nome server: ‘(localdb)\mssqllocaldb’
-	Immettere nome di database: ‘WebApp_FireSmoke’

Creare una tabella all’interno del database ‘ClassifiedImages’ e riempirla nel seguente modo:

CREATE TABLE [dbo].[ClassifiedImages] (
    [Id]                       INT           IDENTITY (1, 1) NOT NULL,
    [Date]                     DATETIME      NULL,
    [Latitude]                 FLOAT (53)    NULL,
    [Longitude]                FLOAT (53)    NULL,
    [GeoPolygon]               VARCHAR (MAX) NULL,
    [Photo]                    VARCHAR (MAX) NULL,
    [PhotoName]                VARCHAR (MAX) NULL,
    [FileType]                 VARCHAR (MAX) NULL,
    [Geolocalization]          VARCHAR (MAX) NULL,
    [GeoJSON]                  VARCHAR (MAX) NULL,
    [FireTypeClassification]   VARCHAR (MAX) NULL,
    [SmokeTypeClassification]  VARCHAR (MAX) NULL,
    [FireScoreClassification]  FLOAT (53)    NULL,
    [SmokeScoreClassification] FLOAT (53)    NULL,
    CONSTRAINT [PK_ClassifiedImages] PRIMARY KEY CLUSTERED ([Id] ASC)
);

Successivamente Cliccare su aggiorna per apportare le modifiche.


Per usufruire del servizio Flask scritto in Python occore l'ambiente di sviluppo adatto (ad esempio: PyCharm)
Sull'ambiente di sviluppo aprire la cartella 
