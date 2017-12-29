CREATE TABLE Users (
  	Id INT(10) UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
  	UserName VARCHAR(15) NOT NULL UNIQUE DEFAULT 'anonymous'
)
ENGINE=MyISAM;

CREATE TABLE Levels (
 	Id INT(10) UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
 	LevelName VARCHAR(15) NOT NULL UNIQUE DEFAULT 'test'
)
ENGINE=MyISAM;

CREATE TABLE Replay (
	Id INT(10) NOT NULL AUTO_INCREMENT PRIMARY KEY,
    UserId INT(10),
  	LevelId INT(10), 
  	ReplayDate TIMESTAMP, 
   	ReplayTime FLOAT,
    ReplayData MEDIUMTEXT,
    CONSTRAINT FK_User_Id FOREIGN KEY (UserId) REFERENCES Users (Id),
    CONSTRAINT FK_Level_Id FOREIGN KEY (LevelId) REFERENCES Levels (Id)
)
ENGINE=MyISAM;