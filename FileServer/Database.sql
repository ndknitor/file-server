create database FileServer;
use FileServer;
CREATE table AppFile(
    Name nvarchar(256) primary key,
    Node nvarchar(16) not null
);
Create table NodeSpace(
    Node nvarchar(16) primary key,
    AvalibleSpace bigint unsigned not null,
    TotalSpace bigint unsigned not null
);