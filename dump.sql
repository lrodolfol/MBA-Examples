use investment;


CREATE TABLE Assets (
                        `Id` int unsigned NOT NULL AUTO_INCREMENT,
                        `Name` varchar(100) NOT NULL,
                        PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=211 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE AssetsPrices (
                              `Id` int unsigned NOT NULL AUTO_INCREMENT,
                              `AssetId` int unsigned NOT NULL,
                              `Price` decimal(10,0) NOT NULL,
                              `Date` date NOT NULL,
                              PRIMARY KEY (`Id`),
                              KEY `AssetsPrices_Assets_FK` (`AssetId`),
                              CONSTRAINT `AssetsPrices_Assets_FK` FOREIGN KEY (`AssetId`) REFERENCES `Assets` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE Clients (
                         `Id` int unsigned NOT NULL AUTO_INCREMENT,
                         `Name` varchar(100) NOT NULL,
                         `Createdat` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
                         PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=13643 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE Operations (
                            `Id` int unsigned NOT NULL AUTO_INCREMENT,
                            `AssetId` int unsigned NOT NULL,
                            `ClientId` int unsigned NOT NULL,
                            `Amount` int NOT NULL,
                            `DateOperation` datetime NOT NULL,
                            `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
                            `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                            `OperationType` varchar(10) COLLATE utf8mb4_general_ci NOT NULL,
                            PRIMARY KEY (`Id`),
                            KEY `Operations_Clients_FK` (`ClientId`),
                            KEY `Operations_Assets_FK` (`AssetId`),
                            CONSTRAINT `Operations_Assets_FK` FOREIGN KEY (`AssetId`) REFERENCES `Assets` (`Id`),
                            CONSTRAINT `Operations_Clients_FK` FOREIGN KEY (`ClientId`) REFERENCES `Clients` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=48095 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE Positions (
                           `Id` int unsigned NOT NULL AUTO_INCREMENT,
                           `ClientId` int unsigned NOT NULL,
                           `AssetId` int unsigned NOT NULL,
                           `Amount` int unsigned NOT NULL,
                           `Date` date NOT NULL,
                           `CreatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
                           `UpdatedAt` timestamp NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                           `UpdatedCount` int NOT NULL DEFAULT '0',
                           PRIMARY KEY (`ClientId`,`AssetId`,`Date`),
                           UNIQUE KEY `Id` (`Id`),
                           KEY `Positions_Assets_FK` (`AssetId`),
                           CONSTRAINT `Positions_Assets_FK` FOREIGN KEY (`AssetId`) REFERENCES `Assets` (`Id`) ON DELETE RESTRICT ON UPDATE RESTRICT,
                           CONSTRAINT `Positions_Clients_FK` FOREIGN KEY (`ClientId`) REFERENCES `Clients` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=38274 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;



INSERT INTO investment.Assets (NAME) VALUES
                                         ('PETR4'), ('VALE3'), ('ITUB4'), ('BBDC4'), ('ABEV3'),
                                         ('BBAS3'), ('B3SA3'), ('WEGE3'), ('EQTL3'), ('RENT3');

INSERT INTO investment.Clients (Name,Createdat) VALUES
                                                    ('Matheus Nogueira','2025-03-30 12:35:28'),
                                                    ('Daniel Batista','2025-03-30 12:35:28'),
                                                    ('Ricardo Reis','2025-03-30 12:35:28'),
                                                    ('Frederico Nogueira','2025-03-30 12:35:28'),
                                                    ('Vitor Oliveira','2025-03-30 12:35:28'),
                                                    ('Ofélia Reis','2025-03-30 12:35:28'),
                                                    ('Roberta Barros','2025-03-30 12:35:28'),
                                                    ('Clara Albuquerque','2025-03-30 12:35:28'),
                                                    ('Miguel Pereira','2025-03-30 12:35:28'),
                                                    ('Yango Carvalho','2025-03-30 12:35:28');