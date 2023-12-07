# InventionLaboratory
# Deployment Guide

This guide provides step-by-step instructions for deploying the system on a host for testing purposes. Follow the instructions below for cloning the repository, installation, setting up the database, and configuring the RabbitMQ connection.

# Deployment Steps:

## 1. Clone the Repository

### 1.1. Clone the repository

Clone the repository on your local device:


```bash
git clone https://github.com/KluchnikZahar1208/InventionLaboratory.git
```

### 1.2. Navigate to the project directory

Change to the cloned project directory:


```bash
cd InventionLaboratory
```

## 2. Install and Configure Services

### 2.1. FileParserService

Run the FileParserService, responsible for processing files and sending data to RabbitMQ. Ensure that all necessary dependencies are installed and follow these steps:

```bash
cd FileParser
dotnet build
dotnet run
```

The service will start, and you will see the message "FileParserService started. Press [enter] to exit."

### 2.2. DataProcessorService

Run the DataProcessorService, responsible for processing data from RabbitMQ and saving it to the SQLite database. Execute the following steps:




```bash
cd DataProcessor
dotnet build
dotnet run
```

The service will start, and you will see the message "Data Processor Service is running. Press [Enter] to exit."

## 3. Create and Configure the Database

### 3.1. SQLite Database

+ The SQLite database will be automatically created on the first run of DataProcessorService.

+ Your path to the SQLite database: DataProcessor/DataBase/InventionLaboratory.db

## 4. Configure RabbitMQ Connection

### 4.1. Configure connection parameters

Edit appsettings.json in both projects (FileParser and DataProcessor) to set the RabbitMQ connection parameters. Find the RabbitMQSettings section and provide the correct values for HostName and QueueName.

### 4.2. Restart the services

After making changes to the configuration, restart both services (FileParserService and DataProcessorService) following the instructions in step 2.

The system is now deployed on your host, and you are ready to conduct testing. Ensure that all services are running correctly, and you are obtaining the expected results.
