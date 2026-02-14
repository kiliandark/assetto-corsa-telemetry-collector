# ACReader — инструкция по установке и интеграции с Smart Monitor

## Оглавление
- [1. Требования](#1-требования)
- [2. Установка зависимостей](#2-установка-зависимостей)
- [3. Сборка и запуск](#3-сборка-и-запуск)
- [4. Проверка работы](#7-проверка-работы)
- [5. Заключение](#8-заключение)

---

## 1. Требования

- **Операционная система:** Windows 10/11  
- **.NET 9.0** 
- **Assetto Corsa (2014)**
- **NuGet-пакеты:**
  - `Newtonsoft.Json`
  - `AssettoCorsaSharedMemory`


---

## 2. Установка зависимостей

Загрузите и установите `.Net 9.0` c [официального сайта](https://dotnet.microsoft.com/ru-ru/download)

Скачайте и разархивируйте архив `ACReader.zip` в удобное для вас место.

Откройте терминал (`Windows PowerShell` или любой другой) и перейдите в папку, куда был разархивирован проект c помощью команды:

```bash
cd C:\Users\<ваш путь>\ACReader
```

В каталоге проекта выполните команды для установки зависимостей:

```bash
dotnet add package Newtonsoft.Json
dotnet add package AssettoCorsaSharedMemory
```
Для проверки установленных пакетов выполните:
```bash
dotnet list package
```

## 3. Сборка и запуск
- В папке с проектом найдите и откройте`config.json` и замените значения:
```json
{
  "logstashHost": "logstashHost", 
  "logstashPort": logstashPort
}
```

Для сборки проекта выполните:
```bash
dotnet build
```
Запуск приложения возможен двумя способами:

1. Через терминал:
```bash
dotnet run
```
2. Или вручную, запустив файл `ACReader.exe` из директории `bin`.


## 4. Проверка работы

1. Запустите `ACReader` одним из способов, описанных выше.

2. Запустите `Assetto Corsa` и начните сессию.

3. В консоли `ACReader` должны появляться следующие сообщения:
```
[[CONFIG] Конфигурация загружена. Smart Monitor Data Collector host: --sm_host, Port: --sm_port
Game status changed: AC_LIVE
[SESSION] New session started: 1762363342087
```
После выхода из сессии:
```
Game status changed: AC_OFF
[SESSION] Session ended, waiting for next one...
```

## 5. Заключение

После выполнения всех шагов данные телеметрии из `Assetto Corsa` будут автоматически собираться приложением `ACReader`, передаваться в `Smart Monitor Data Collector`.






































