CREATE TABLE SQLStorage ( 
  id int AUTO_INCREMENT PRIMARY KEY, 
  fileName varchar(256),
  fileInfo varchar(256),
  fileData longblob 
);

CREATE TABLE SQLStorageWithLink ( 
  id int AUTO_INCREMENT PRIMARY KEY, 
  fileName varchar(256),
  fileInfo varchar(256),
  filePath varchar(256) 
);