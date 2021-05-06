-- -------------------------------------------------------
-- SQL Script to create the Eratos Backend Server Database
-- This script was run on Azure SQL Server
-- @author Aditya Vikram Khandelwal
-- -------------------------------------------------------

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';

-- -----------------------------------------------------
-- Schema mydb
-- -----------------------------------------------------

-- -----------------------------------------------------
-- Schema mydb
-- -----------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `mydb` DEFAULT CHARACTER SET utf8 ;
USE `mydb` ;

-- -----------------------------------------------------
-- Table `mydb`.`User`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `mydb`.`User` ;

CREATE TABLE IF NOT EXISTS `mydb`.`User` (
  `UserID` INT NOT NULL AUTO_INCREMENT,
  `UserInfo1` INT NULL,
  `UserInfo2` VARCHAR(45) NULL,
  `UserInfo3` TINYINT NULL,
  PRIMARY KEY (`UserID`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `mydb`.`Task`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `mydb`.`Task` ;

CREATE TABLE IF NOT EXISTS `mydb`.`Task` (
  `TaskID` INT NOT NULL AUTO_INCREMENT,
  `UserID` INT NOT NULL,
  `Priority` INT NULL,
  `State` VARCHAR(45) NULL,
  `Type` VARCHAR(45) NULL,
  `OtherInfo` VARCHAR(45) NULL,
  `Taskcol` VARCHAR(45) NULL,
  PRIMARY KEY (`TaskID`),
  INDEX `fk_Task_User_idx` (`UserID` ASC) VISIBLE,
  CONSTRAINT `fk_Task_User`
    FOREIGN KEY (`UserID`)
    REFERENCES `mydb`.`User` (`UserID`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `mydb`.`Resource`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `mydb`.`Resource` ;

CREATE TABLE IF NOT EXISTS `mydb`.`Resource` (
  `ResourceID` INT NOT NULL AUTO_INCREMENT,
  `ResourceURI` VARCHAR(100) NULL,
  `OtherInfo` VARCHAR(45) NULL,
  PRIMARY KEY (`ResourceID`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `mydb`.`ResourceTask`
-- -----------------------------------------------------
DROP TABLE IF EXISTS `mydb`.`ResourceTask` ;

CREATE TABLE IF NOT EXISTS `mydb`.`ResourceTask` (
  `TaskID` INT NOT NULL,
  `ResourceID` INT NOT NULL,
  PRIMARY KEY (`TaskID`, `ResourceID`),
  INDEX `fk_ResourceTask_Task1_idx` (`TaskID` ASC) VISIBLE,
  CONSTRAINT `fk_ResourceTask_Resource1`
    FOREIGN KEY (`ResourceID`)
    REFERENCES `mydb`.`Resource` (`ResourceID`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_ResourceTask_Task1`
    FOREIGN KEY (`TaskID`)
    REFERENCES `mydb`.`Task` (`TaskID`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
