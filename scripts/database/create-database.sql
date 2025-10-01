CREATE DATABASE MoreSpeakers
    ON
    ( NAME = JJGNet_Data,
        FILENAME = '/var/opt/mssql/data/morespeakers.mdf',
        SIZE = 10,
        MAXSIZE = 50,
        FILEGROWTH = 5 )
    LOG ON
    ( NAME = JJGNet_Log,
        FILENAME = '/var/opt/mssql/data/morespeakers.ldf',
        SIZE = 5MB,
        MAXSIZE = 25MB,
        FILEGROWTH = 5MB ) ;
GO

--- Replace <REPLACE_ME> with a real password
USE master
CREATE Login jguadagno
    WITH Password='5cEZpbhz&p5i&DaA2*N68Nn4sJINd2-localonly'
GO

CREATE Login cwoodruff
    WITH Password='5cEZpbhz&p5i&DaA2*N68Nn4sJINd2-localonly'
GO

USE MoreSpeakers
CREATE USER jguadagno FOR LOGIN jguadagno;

ALTER ROLE db_datareader ADD MEMBER jguadagno;
ALTER ROLE db_datawriter ADD MEMBER jguadagno;
      
CREATE USER cwoodruff FOR LOGIN cwoodruff;

ALTER ROLE db_datareader ADD MEMBER cwoodruff;
ALTER ROLE db_datawriter ADD MEMBER cwoodruff;      
GO
