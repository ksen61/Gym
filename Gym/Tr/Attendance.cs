﻿using System;
/// <summary>
/// Класс, представляющий посещаемость клиента на занятиях.
/// Содержит информацию о клиенте, статусе посещаемости, названии занятия, времени и дате занятия.
/// </summary>
public class Attendance
{
    public string ClientName { get; set; }
    public bool? AttendanceStatus_Bit { get; set; }
    public string ClassName { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public int Client_ID { get; set; }
    public int Class_ID { get; set; }
}