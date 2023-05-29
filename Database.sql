create database FileServer;
use FileServer;
CREATE table AppFile(
    Name nvarchar(256) primary key,
    Node nvarchar(16) not null
);