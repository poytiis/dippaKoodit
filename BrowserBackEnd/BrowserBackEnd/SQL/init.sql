CREATE TABLE SQLStorageWithLink (
 ID int NOT NULL AUTO_INCREMENT,
 FileName varchar(50),
 FilePath varchar(50),
 PRIMARY KEY (ID)
);

CREATE TABLE SQLStorage (
 ID int NOT NULL AUTO_INCREMENT,
 FileName varchar(50),
 Filedata LONGBLOB,
 PRIMARY KEY (ID)
);