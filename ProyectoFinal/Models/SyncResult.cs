namespace ProyectoFinal.Models;

public sealed record SyncResult(bool IsOnline, int PendingCount, string Message);
