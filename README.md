# RoomPcAsist
Bilgisayarın toplam WATT kullanımı ve public ip fiziksel lcd ekrana yazar. 

## Arduino Lib's:
[ArduinoAsync](https://github.com/MatheusAlvesA/ArduinoAsync)

[Json](https://arduinojson.org)

## .NET Framework Lib's:
[LibreHardwareMonitor](https://www.nuget.org/packages/LibreHardwareMonitorLib/)

[Json](learn.microsoft.com/en-us/dotnet/api/system.text.json)

# Not
Kaynak kodunda "PortName"'i seri iletişim seri bağlantı portunuza göre değiştirin.
- [Service1.cs#L69](https://github.com/Osderda/RoomPcAsist/blob/master/RoomAsistService/Service1.cs#L69)
- [Program.cs#L56](https://github.com/Osderda/RoomPcAsist/blob/master/RoomAsist/Program.cs#L56)

Windows açıldığında otomatik başlatılması için servisi kurun.

